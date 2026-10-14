
using System;
using System.Collections.Generic;
using WpEngine.Models;

namespace WpEngine.Core;

public sealed class ProgramAnalyzer
{
    public ProgramAnalysis Analyze(Statement statement)
    {
        var result = new ProgramAnalysis();
        AnalyzeStatement(statement, result);
        return result;
    }

    private static void AnalyzeStatement(
        Statement statement,
        ProgramAnalysis result)
    {
        switch (statement)
        {
            case Assignment assignment:
                result.StatementCount++;
                result.Assignments.Add(
                    $"{assignment.Variable} := {assignment.Expression}");

                result.Variables.Add(assignment.Variable);
                CollectVariables(assignment.Expression, result);
                break;

            case Sequence sequence:
                foreach (Statement child in sequence.Statements)
                    AnalyzeStatement(child, result);
                break;

            case IfStatement conditional:
                result.StatementCount++;
                result.BranchCount++;
                result.Conditions.Add(conditional.Condition);
                CollectVariables(conditional.Condition, result);

                AnalyzeStatement(conditional.ThenBranch, result);
                AnalyzeStatement(conditional.ElseBranch, result);
                break;

            default:
                throw new NotSupportedException(
                    $"Неподдерживаемый оператор: {statement.GetType().Name}");
        }
    }

    private static void CollectVariables(
        string expression,
        ProgramAnalysis result)
    {
        Expr parsed = ExpressionParser.Parse(expression);
        CollectVariables(parsed, result);
    }

    private static void CollectVariables(
        Expr expression,
        ProgramAnalysis result)
    {
        switch (expression)
        {
            case VariableExpr variable:
                result.Variables.Add(variable.Name);
                break;

            case UnaryExpr unary:
                CollectVariables(unary.Operand, result);
                break;

            case BinaryExpr binary:
                CollectVariables(binary.Left, result);
                CollectVariables(binary.Right, result);
                break;

            case FunctionExpr function:
                CollectVariables(function.Argument, result);
                break;

            case NumberExpr:
            case BooleanExpr:
                break;

            default:
                throw new NotSupportedException(
                    $"Неизвестное выражение: {expression.GetType().Name}");
        }
    }
}

public sealed class ProgramAnalysis
{
    public int StatementCount { get; internal set; }

    public int BranchCount { get; internal set; }

    public HashSet<string> Variables { get; } = new();

    public List<string> Assignments { get; } = new();

    public List<string> Conditions { get; } = new();
}
