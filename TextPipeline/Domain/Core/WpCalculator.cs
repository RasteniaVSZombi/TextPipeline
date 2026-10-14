
using System;
using System.Collections.Generic;
using System.Linq;
using WpEngine.Models;
using WpEngine.Parsing;

namespace WpEngine.Core;

public sealed class WpCalculator
{
    public WpResult Calculate(
        string program,
        string postcondition)
    {
        var result = new WpResult
        {
            Postcondition = postcondition
        };

        try
        {
            if (string.IsNullOrWhiteSpace(program))
            {
                throw new ArgumentException(
                    "Программа не может быть пустой.");
            }

            if (string.IsNullOrWhiteSpace(postcondition))
            {
                throw new ArgumentException(
                    "Постусловие не может быть пустым.");
            }

            // Проверяем синтаксис постусловия и программы.
            ExpressionParser.Parse(postcondition);
            Statement statement = Parser.Parse(program);

            var typeChecker = new ExpressionTypeChecker();
            typeChecker.CheckProgram(statement, postcondition);

            var steps = new List<string>();
            var definednessConditions = new List<string>();

            // Учитываем определённость постусловия.
            List<string> postConditions =
                PredicateTransformer.GetDefinednessConditions(
                    postcondition);

            definednessConditions.AddRange(postConditions);

            string effectivePostcondition = postcondition;

            if (postConditions.Count > 0)
            {
                string guard = string.Join(
                    " && ",
                    postConditions
                        .Distinct()
                        .Select(c => $"({c})"));

                effectivePostcondition =
                    $"({guard}) && ({postcondition})";

                steps.Add(
                    $"Ограничения определённости постусловия: {guard}");
            }

            // Основное вычисление wp.
            string precondition = CalculateWp(
                statement,
                effectivePostcondition,
                steps,
                definednessConditions);

            string simplifiedPrecondition =
    ExpressionSimplifier.Simplify(precondition);

            if (simplifiedPrecondition != precondition)
            {
                steps.Add($"Упрощение предусловия: {simplifiedPrecondition}");
            }

            result.Precondition = simplifiedPrecondition;
            result.Postcondition = postcondition;
            result.Steps = steps;

            // Сохраняем ограничения определённости,
            // которые были обнаружены при вычислении wp.
            result.DefinednessConditions =
                definednessConditions
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

            result.HoareTriple =
                $"{{ {precondition} }} " +
                $"{program.Trim()} " +
                $"{{ {postcondition} }}";

            result.Success = true;
            result.Error = null;
        }

        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            result.Precondition = string.Empty;
            result.Steps.Clear();
            result.DefinednessConditions.Clear();
            result.HoareTriple = string.Empty;
        }


        return result;
    }

    private string CalculateWp(
        Statement statement,
        string postcondition,
        List<string> steps,
        List<string> definednessConditions)
    {
        switch (statement)
        {
            case Assignment assignment:
                return CalculateAssignment(
                    assignment,
                    postcondition,
                    steps,
                    definednessConditions);

            case Sequence sequence:
                return CalculateSequence(
                    sequence,
                    postcondition,
                    steps,
                    definednessConditions);

            case IfStatement conditional:
                return CalculateIf(
                    conditional,
                    postcondition,
                    steps,
                    definednessConditions);

            default:
                throw new NotSupportedException(
                    $"Неподдерживаемый оператор: " +
                    $"{statement.GetType().Name}");
        }
    }

    // wp(x := E, Q) = Def(E) && Q[x := E]
    private string CalculateAssignment(
        Assignment assignment,
        string postcondition,
        List<string> steps,
        List<string> definednessConditions)
    {
        steps.Add(
            $"Присваивание: {assignment.Variable} := " +
            $"{assignment.Expression}");

        steps.Add($"Текущее постусловие: {postcondition}");

        string precondition = PredicateTransformer.Substitute(
            postcondition,
            assignment.Variable,
            assignment.Expression,
            out List<string> conditions);

        definednessConditions.AddRange(conditions);

        steps.Add(
            $"После подстановки: {precondition}");

        return precondition;
    }

    // Последовательность вычисляется с конца к началу.
    private string CalculateSequence(
        Sequence sequence,
        string postcondition,
        List<string> steps,
        List<string> definednessConditions)
    {
        string current = postcondition;

        steps.Add(
            "Начало обратного прохода по последовательности.");

        for (int i = sequence.Statements.Count - 1; i >= 0; i--)
        {
            steps.Add(
                $"Обработка оператора {i + 1} " +
                "в обратном порядке.");

            current = CalculateWp(
                sequence.Statements[i],
                current,
                steps,
                definednessConditions);

            steps.Add(
                $"Промежуточное предусловие: {current}");
        }

        return current;
    }

    // wp(if C then S1 else S2, Q) =
    // Def(C) && ((C && wp(S1, Q)) || (!C && wp(S2, Q)))
    private string CalculateIf(
        IfStatement conditional,
        string postcondition,
        List<string> steps,
        List<string> definednessConditions)
    {
        string condition = conditional.Condition;

        steps.Add($"Условие ветвления: {condition}");

        // Проверяем определённость условия if.
        List<string> conditionConditions =
            PredicateTransformer.GetDefinednessConditions(
                condition);

        definednessConditions.AddRange(conditionConditions);

        // Вычисляем wp ветки then.
        var thenSteps = new List<string>();
        var thenConditions = new List<string>();

        string thenPrecondition = CalculateWp(
            conditional.ThenBranch,
            postcondition,
            thenSteps,
            thenConditions);

        steps.Add("Вычисление ветки then:");

        steps.AddRange(
            thenSteps.Select(s => "[then] " + s));

        // Вычисляем wp ветки else.
        var elseSteps = new List<string>();
        var elseConditions = new List<string>();

        string elsePrecondition = CalculateWp(
            conditional.ElseBranch,
            postcondition,
            elseSteps,
            elseConditions);

        steps.Add("Вычисление ветки else:");

        steps.AddRange(
            elseSteps.Select(s => "[else] " + s));

        definednessConditions.AddRange(thenConditions);
        definednessConditions.AddRange(elseConditions);

        // Объединяем предусловия обеих ветвей.
        string result =
            $"(({condition}) && ({thenPrecondition})) || " +
            $"((!({condition})) && ({elsePrecondition}))";

        // Добавляем ограничения определённости условия.
        if (conditionConditions.Count > 0)
        {
            string guard = string.Join(
                " && ",
                conditionConditions
                    .Distinct()
                    .Select(c => $"({c})"));

            result = $"({guard}) && ({result})";
        }

        steps.Add($"Результат ветвления: {result}");

        return result;
    }
}
