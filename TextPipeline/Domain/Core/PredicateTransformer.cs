
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WpEngine.Core;

public static class PredicateTransformer
{
    /// <summary>
    /// Подставляет выражение вместо переменной в постусловие
    /// и добавляет ограничения определённости выражения.
    /// </summary>
    public static string Substitute(
        string predicate,
        string variable,
        string expression,
        out List<string> definednessConditions)
    {
        if (string.IsNullOrWhiteSpace(variable))
        {
            throw new ArgumentException(
                "Имя переменной не задано.",
                nameof(variable));
        }

        if (string.IsNullOrWhiteSpace(predicate))
        {
            throw new ArgumentException(
                "Постусловие не задано.",
                nameof(predicate));
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException(
                "Выражение присваивания не задано.",
                nameof(expression));
        }

        Expr postcondition = ExpressionParser.Parse(predicate);
        Expr replacement = ExpressionParser.Parse(expression);

        definednessConditions =
            GetDefinednessConditionsFromExpr(replacement);

        Expr transformed = ReplaceVariable(
            postcondition,
            variable,
            replacement);

        string result = Format(transformed);

        if (definednessConditions.Count == 0)
            return result;

        string guard = string.Join(
            " && ",
            definednessConditions
                .Distinct()
                .Select(c => $"({c})"));

        return $"({guard}) && ({result})";
    }

    /// <summary>
    /// Возвращает ограничения определённости выражения.
    /// </summary>
    public static List<string> GetDefinednessConditions(
        string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException(
                "Выражение не задано.",
                nameof(expression));
        }

        Expr parsed = ExpressionParser.Parse(expression);

        return GetDefinednessConditionsFromExpr(parsed);
    }

    private static List<string> GetDefinednessConditionsFromExpr(
        Expr expression)
    {
        var conditions = new List<string>();

        CollectDefinedness(expression, conditions);

        return conditions
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Рекурсивно заменяет все вхождения переменной.
    /// </summary>
    private static Expr ReplaceVariable(
    Expr expression,
    string variable,
    Expr replacement)
    {
        switch (expression)
        {
            case VariableExpr v when v.Name == variable:
                return replacement;

            // Важно: остальные переменные сохраняем без изменений.
            case VariableExpr:
            case NumberExpr:
            case BooleanExpr:
                return expression;

            case UnaryExpr u:
                return new UnaryExpr(
                    u.Operator,
                    ReplaceVariable(u.Operand, variable, replacement));

            case BinaryExpr b:
                return new BinaryExpr(
                    ReplaceVariable(b.Left, variable, replacement),
                    b.Operator,
                    ReplaceVariable(b.Right, variable, replacement));

            case FunctionExpr f:
                return new FunctionExpr(
                    f.Name,
                    ReplaceVariable(f.Argument, variable, replacement));

            default:
                throw new NotSupportedException(
                    $"Неизвестный тип выражения: {expression.GetType().Name}");
        }
    }

    /// <summary>
    /// Собирает ограничения для деления и квадратного корня.
    /// </summary>
    private static void CollectDefinedness(
        Expr expression,
        List<string> conditions)
    {
        switch (expression)
        {
            case BinaryExpr b:
                CollectDefinedness(b.Left, conditions);
                CollectDefinedness(b.Right, conditions);

                if (b.Operator == "/")
                {
                    conditions.Add(
                        $"({Format(b.Right)}) != 0");
                }

                break;

            case UnaryExpr u:
                CollectDefinedness(
                    u.Operand,
                    conditions);
                break;

            case FunctionExpr f:
                CollectDefinedness(
                    f.Argument,
                    conditions);

                if (f.Name == "sqrt")
                {
                    conditions.Add(
                        $"({Format(f.Argument)}) >= 0");
                }

                break;

            case NumberExpr:
            case BooleanExpr:
            case VariableExpr:
                break;

            default:
                throw new NotSupportedException(
                    $"Неизвестный тип выражения: " +
                    $"{expression.GetType().Name}");
        }
    }

    /// <summary>
    /// Преобразует AST обратно в строку.
    /// </summary>
    public static string Format(Expr expression)
    {
        return expression switch
        {
            NumberExpr n =>
                n.Value.ToString(
                    "G",
                    CultureInfo.InvariantCulture),

            BooleanExpr b =>
                b.Value ? "true" : "false",

            VariableExpr v =>
                v.Name,

            UnaryExpr u when u.Operator == "-" =>
                $"-({Format(u.Operand)})",

            UnaryExpr u when u.Operator == "!" =>
                $"!({Format(u.Operand)})",

            UnaryExpr u =>
                throw new NotSupportedException(
                    $"Неизвестный унарный оператор: {u.Operator}"),

            BinaryExpr b =>
                $"({Format(b.Left)} {b.Operator} {Format(b.Right)})",

            FunctionExpr f =>
                $"{f.Name}({Format(f.Argument)})",

            _ =>
                throw new NotSupportedException(
                    $"Неизвестный тип выражения: " +
                    $"{expression.GetType().Name}")
        };
    }
}
