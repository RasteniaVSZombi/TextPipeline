
using System;
using System.Collections.Generic;
using WpEngine.Models;

namespace WpEngine.Core;

public enum ExpressionType
{
    Number,
    Boolean
}

public sealed class ExpressionTypeChecker
{
    private readonly Dictionary<string, TypeNode> _variables = new();

    public IReadOnlyDictionary<string, ExpressionType> VariableTypes
    {
        get
        {
            var result = new Dictionary<string, ExpressionType>();

            foreach (var pair in _variables)
            {
                TypeNode root = Find(pair.Value);

                if (root.Type.HasValue)
                    result[pair.Key] = root.Type.Value;
            }

            return result;
        }
    }

    public void CheckProgram(Statement statement, string postcondition)
    {
        _variables.Clear();

        CheckStatement(statement);

        Expr post = ExpressionParser.Parse(postcondition);
        TypeNode postType = Infer(post);

        Require(postType, ExpressionType.Boolean,
            "Постусловие должно быть логическим выражением.");
    }

    private void CheckStatement(Statement statement)
    {
        switch (statement)
        {
            case Assignment assignment:
                {
                    Expr expression =
                        ExpressionParser.Parse(assignment.Expression);

                    TypeNode target = GetVariable(assignment.Variable);
                    TypeNode value = Infer(expression);

                    Unify(target, value,
                        $"Несовместимые типы при присваивании переменной '{assignment.Variable}'.");

                    break;
                }

            case Sequence sequence:
                foreach (Statement child in sequence.Statements)
                    CheckStatement(child);
                break;

            case IfStatement conditional:
                {
                    Expr condition =
                        ExpressionParser.Parse(conditional.Condition);

                    Require(Infer(condition), ExpressionType.Boolean,
                        "Условие if должно быть логическим.");

                    CheckStatement(conditional.ThenBranch);
                    CheckStatement(conditional.ElseBranch);
                    break;
                }

            default:
                throw new NotSupportedException(
                    $"Неподдерживаемый оператор: {statement.GetType().Name}");
        }
    }

    private TypeNode Infer(Expr expression)
    {
        switch (expression)
        {
            case NumberExpr:
                return NewNode(ExpressionType.Number);

            case BooleanExpr:
                return NewNode(ExpressionType.Boolean);

            case VariableExpr variable:
                return GetVariable(variable.Name);

            case UnaryExpr unary:
                {
                    TypeNode operand = Infer(unary.Operand);

                    if (unary.Operator == "-")
                    {
                        Require(operand, ExpressionType.Number,
                            "Унарный минус применим только к числу.");

                        return NewNode(ExpressionType.Number);
                    }

                    if (unary.Operator == "!")
                    {
                        Require(operand, ExpressionType.Boolean,
                            "Оператор ! применим только к логическому выражению.");

                        return NewNode(ExpressionType.Boolean);
                    }

                    throw new NotSupportedException(
                        $"Неизвестный унарный оператор: {unary.Operator}");
                }

            case BinaryExpr binary:
                return InferBinary(binary);

            case FunctionExpr function:
                {
                    if (function.Name != "abs" && function.Name != "sqrt")
                    {
                        throw new NotSupportedException(
                            $"Неизвестная функция: {function.Name}");
                    }

                    Require(Infer(function.Argument), ExpressionType.Number,
                        $"Аргумент функции {function.Name} должен быть числом.");

                    return NewNode(ExpressionType.Number);
                }

            default:
                throw new NotSupportedException(
                    $"Неизвестное выражение: {expression.GetType().Name}");
        }
    }

    private TypeNode InferBinary(BinaryExpr expression)
    {
        TypeNode left = Infer(expression.Left);
        TypeNode right = Infer(expression.Right);

        switch (expression.Operator)
        {
            case "+":
            case "-":
            case "*":
            case "/":
                Require(left, ExpressionType.Number,
                    $"Левый операнд '{expression.Operator}' должен быть числом.");

                Require(right, ExpressionType.Number,
                    $"Правый операнд '{expression.Operator}' должен быть числом.");

                return NewNode(ExpressionType.Number);

            case ">":
            case "<":
            case ">=":
            case "<=":
                Require(left, ExpressionType.Number,
                    "Сравнение требует числовые операнды.");

                Require(right, ExpressionType.Number,
                    "Сравнение требует числовые операнды.");

                return NewNode(ExpressionType.Boolean);

            case "==":
            case "!=":
                Unify(left, right,
                    "Операнды == и != должны иметь одинаковые типы.");

                return NewNode(ExpressionType.Boolean);

            case "&&":
            case "||":
                Require(left, ExpressionType.Boolean,
                    $"Оператор '{expression.Operator}' требует логические операнды.");

                Require(right, ExpressionType.Boolean,
                    $"Оператор '{expression.Operator}' требует логические операнды.");

                return NewNode(ExpressionType.Boolean);

            default:
                throw new NotSupportedException(
                    $"Неизвестный бинарный оператор: {expression.Operator}");
        }
    }

    private TypeNode GetVariable(string name)
    {
        if (!_variables.TryGetValue(name, out TypeNode? node))
        {
            node = new TypeNode();
            _variables[name] = node;
        }

        return node;
    }

    private static TypeNode NewNode(ExpressionType type)
    {
        return new TypeNode { Type = type };
    }

    private static TypeNode Find(TypeNode node)
    {
        if (!ReferenceEquals(node, node.Parent))
            node.Parent = Find(node.Parent);

        return node.Parent;
    }

    private static void Unify(
        TypeNode first,
        TypeNode second,
        string errorMessage)
    {
        TypeNode a = Find(first);
        TypeNode b = Find(second);

        if (ReferenceEquals(a, b))
            return;

        if (a.Type.HasValue && b.Type.HasValue &&
            a.Type.Value != b.Type.Value)
        {
            throw new InvalidOperationException(
                $"{errorMessage} Обнаружены типы {a.Type} и {b.Type}.");
        }

        if (a.Rank < b.Rank)
            (a, b) = (b, a);

        b.Parent = a;

        if (a.Rank == b.Rank)
            a.Rank++;

        if (!a.Type.HasValue)
            a.Type = b.Type;
    }

    private static void Require(
        TypeNode node,
        ExpressionType expected,
        string errorMessage)
    {
        Unify(node, NewNode(expected), errorMessage);
    }

    private sealed class TypeNode
    {
        public TypeNode Parent { get; set; }
        public int Rank { get; set; }
        public ExpressionType? Type { get; set; }

        public TypeNode()
        {
            Parent = this;
        }
    }
}
