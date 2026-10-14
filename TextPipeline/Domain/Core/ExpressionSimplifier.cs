
using System;
using System.Globalization;

namespace WpEngine.Core;

public static class ExpressionSimplifier
{
    public static string Simplify(string expression)
    {
        Expr parsed = ExpressionParser.Parse(expression);
        Expr simplified = SimplifyExpr(parsed);
        return PredicateTransformer.Format(simplified);
    }

    private static Expr SimplifyExpr(Expr expression)
    {
        switch (expression)
        {
            case NumberExpr:
            case BooleanExpr:
            case VariableExpr:
                return expression;

            case UnaryExpr unary:
                {
                    Expr operand = SimplifyExpr(unary.Operand);

                    if (unary.Operator == "-" &&
                        operand is NumberExpr number)
                    {
                        return new NumberExpr(-number.Value);
                    }

                    if (unary.Operator == "!" &&
                        operand is BooleanExpr boolean)
                    {
                        return new BooleanExpr(!boolean.Value);
                    }

                    if (unary.Operator == "-" &&
                        operand is UnaryExpr { Operator: "-" } nested)
                    {
                        return nested.Operand;
                    }

                    if (unary.Operator == "!" &&
                        operand is UnaryExpr { Operator: "!" } negated)
                    {
                        return negated.Operand;
                    }

                    return new UnaryExpr(unary.Operator, operand);
                }

            case FunctionExpr function:
                {
                    Expr argument = SimplifyExpr(function.Argument);

                    if (argument is NumberExpr number)
                    {
                        if (function.Name == "abs")
                            return new NumberExpr(Math.Abs(number.Value));

                        if (function.Name == "sqrt" && number.Value >= 0)
                            return new NumberExpr(Math.Sqrt(number.Value));
                    }

                    return new FunctionExpr(function.Name, argument);
                }

            case BinaryExpr binary:
                return SimplifyBinary(binary);

            default:
                throw new NotSupportedException(
                    $"Неизвестное выражение: {expression.GetType().Name}");
        }
    }

    private static Expr SimplifyBinary(BinaryExpr binary)
    {
        Expr left = SimplifyExpr(binary.Left);
        Expr right = SimplifyExpr(binary.Right);
        string op = binary.Operator;

        if (left is NumberExpr a && right is NumberExpr b)
        {
            switch (op)
            {
                case "+": return new NumberExpr(a.Value + b.Value);
                case "-": return new NumberExpr(a.Value - b.Value);
                case "*": return new NumberExpr(a.Value * b.Value);

                case "/" when b.Value != 0:
                    return new NumberExpr(a.Value / b.Value);

                case ">": return new BooleanExpr(a.Value > b.Value);
                case "<": return new BooleanExpr(a.Value < b.Value);
                case ">=": return new BooleanExpr(a.Value >= b.Value);
                case "<=": return new BooleanExpr(a.Value <= b.Value);
                case "==": return new BooleanExpr(a.Value == b.Value);
                case "!=": return new BooleanExpr(a.Value != b.Value);
            }
        }

        if (left is BooleanExpr leftBool &&
            right is BooleanExpr rightBool)
        {
            switch (op)
            {
                case "&&":
                    return new BooleanExpr(leftBool.Value && rightBool.Value);
                case "||":
                    return new BooleanExpr(leftBool.Value || rightBool.Value);
                case "==":
                    return new BooleanExpr(leftBool.Value == rightBool.Value);
                case "!=":
                    return new BooleanExpr(leftBool.Value != rightBool.Value);
            }
        }

        if (op == "&&")
        {
            if (left is BooleanExpr { Value: false } ||
                right is BooleanExpr { Value: false })
                return new BooleanExpr(false);

            if (left is BooleanExpr { Value: true })
                return right;

            if (right is BooleanExpr { Value: true })
                return left;
        }

        if (op == "||")
        {
            if (left is BooleanExpr { Value: true } ||
                right is BooleanExpr { Value: true })
                return new BooleanExpr(true);

            if (left is BooleanExpr { Value: false })
                return right;

            if (right is BooleanExpr { Value: false })
                return left;
        }

        if (op == "+")
        {
            if (IsNumber(left, 0)) return right;
            if (IsNumber(right, 0)) return left;
        }

        if (op == "-")
        {
            if (IsNumber(right, 0)) return left;
        }

        if (op == "*")
        {
            if (IsNumber(left, 0) || IsNumber(right, 0))
                return new NumberExpr(0);

            if (IsNumber(left, 1)) return right;
            if (IsNumber(right, 1)) return left;
        }

        if (op == "/" && IsNumber(right, 1))
            return left;

        return new BinaryExpr(left, op, right);
    }

    private static bool IsNumber(Expr expression, double value)
    {
        return expression is NumberExpr number &&
               number.Value == value;
    }
}
