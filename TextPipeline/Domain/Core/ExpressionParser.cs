
using System;
using System.Globalization;

namespace WpEngine.Core;

// Абстрактное синтаксическое дерево выражений.
public abstract record Expr;

public sealed record NumberExpr(double Value) : Expr;

public sealed record BooleanExpr(bool Value) : Expr;

public sealed record VariableExpr(string Name) : Expr;

public sealed record UnaryExpr(
    string Operator,
    Expr Operand) : Expr;

public sealed record BinaryExpr(
    Expr Left,
    string Operator,
    Expr Right) : Expr;

public sealed record FunctionExpr(
    string Name,
    Expr Argument) : Expr;

public sealed class ExpressionParser
{
    private readonly string _text;
    private int _position;

    private ExpressionParser(string text)
    {
        _text = text;
        _position = 0;
    }

    public static Expr Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException(
                "Выражение не может быть пустым.");
        }

        var parser = new ExpressionParser(text);
        Expr result = parser.ParseOr();

        parser.SkipWhitespace();

        if (parser._position < parser._text.Length)
        {
            throw parser.Error(
                $"Неожиданный символ '{parser._text[parser._position]}'.");
        }

        return result;
    }

    private FormatException Error(string message)
    {
        return new FormatException(
            $"{message} Позиция: {_position + 1}.");
    }

    // Логическое ИЛИ: ||
    private Expr ParseOr()
    {
        Expr left = ParseAnd();

        while (Match("||"))
        {
            Expr right = ParseAnd();
            left = new BinaryExpr(left, "||", right);
        }

        return left;
    }

    // Логическое И: &&
    private Expr ParseAnd()
    {
        Expr left = ParseComparison();

        while (Match("&&"))
        {
            Expr right = ParseComparison();
            left = new BinaryExpr(left, "&&", right);
        }

        return left;
    }

    // Сравнения: >, <, >=, <=, ==, !=
    private Expr ParseComparison()
    {
        Expr left = ParseAdditive();

        while (true)
        {
            string? op = null;

            if (Match(">="))
                op = ">=";
            else if (Match("<="))
                op = "<=";
            else if (Match("=="))
                op = "==";
            else if (Match("!="))
                op = "!=";
            else if (Match(">"))
                op = ">";
            else if (Match("<"))
                op = "<";

            if (op == null)
                break;

            Expr right = ParseAdditive();
            left = new BinaryExpr(left, op, right);
        }

        return left;
    }

    // Сложение и вычитание.
    private Expr ParseAdditive()
    {
        Expr left = ParseMultiplicative();

        while (true)
        {
            if (Match("+"))
            {
                Expr right = ParseMultiplicative();
                left = new BinaryExpr(left, "+", right);
            }
            else if (Match("-"))
            {
                Expr right = ParseMultiplicative();
                left = new BinaryExpr(left, "-", right);
            }
            else
            {
                break;
            }
        }

        return left;
    }

    // Умножение и деление.
    private Expr ParseMultiplicative()
    {
        Expr left = ParseUnary();

        while (true)
        {
            if (Match("*"))
            {
                Expr right = ParseUnary();
                left = new BinaryExpr(left, "*", right);
            }
            else if (Match("/"))
            {
                Expr right = ParseUnary();
                left = new BinaryExpr(left, "/", right);
            }
            else
            {
                break;
            }
        }

        return left;
    }

    // Унарный минус и логическое НЕ.
    private Expr ParseUnary()
    {
        if (Match("-"))
            return new UnaryExpr("-", ParseUnary());

        if (Match("!"))
            return new UnaryExpr("!", ParseUnary());

        return ParsePrimary();
    }

    // Числа, логические константы, переменные,
    // функции и выражения в скобках.
    private Expr ParsePrimary()
    {
        SkipWhitespace();

        if (_position >= _text.Length)
            throw Error("Ожидалось выражение.");

        if (Match("("))
        {
            Expr expression = ParseOr();

            if (!Match(")"))
                throw Error(
                    "Ожидалась закрывающая скобка ')'.");

            return expression;
        }

        char current = _text[_position];

        if (char.IsDigit(current) ||
            (current == '.' &&
             _position + 1 < _text.Length &&
             char.IsDigit(_text[_position + 1])))
        {
            return ParseNumber();
        }

        if (IsIdentifierStart(current))
        {
            string name = ParseIdentifier();

            if (name == "true")
                return new BooleanExpr(true);

            if (name == "false")
                return new BooleanExpr(false);

            if (name == "abs" || name == "sqrt")
            {
                if (!Match("("))
                {
                    throw Error(
                        $"После функции '{name}' ожидалась скобка '('.");
                }

                Expr argument = ParseOr();

                if (!Match(")"))
                {
                    throw Error(
                        $"Ожидалась закрывающая скобка " +
                        $"после аргумента функции '{name}'.");
                }

                return new FunctionExpr(name, argument);
            }

            return new VariableExpr(name);
        }

        throw Error(
            $"Неожиданный символ '{_text[_position]}'.");
    }

    private Expr ParseNumber()
    {
        SkipWhitespace();

        int start = _position;
        bool hasDot = false;

        while (_position < _text.Length)
        {
            char c = _text[_position];

            if (char.IsDigit(c))
            {
                _position++;
            }
            else if (c == '.' && !hasDot)
            {
                hasDot = true;
                _position++;
            }
            else
            {
                break;
            }
        }

        string numberText = _text[start.._position];

        if (!double.TryParse(
                numberText,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out double value))
        {
            throw Error(
                $"Некорректное число '{numberText}'.");
        }

        return new NumberExpr(value);
    }

    private string ParseIdentifier()
    {
        SkipWhitespace();

        int start = _position;

        while (_position < _text.Length &&
               IsIdentifierPart(_text[_position]))
        {
            _position++;
        }

        return _text[start.._position];
    }

    private static bool IsIdentifierStart(char c)
    {
        return char.IsLetter(c) || c == '_';
    }

    private static bool IsIdentifierPart(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    private bool Match(string token)
    {
        SkipWhitespace();

        if (_position + token.Length > _text.Length)
            return false;

        if (!_text.AsSpan(_position, token.Length)
                  .SequenceEqual(token.AsSpan()))
        {
            return false;
        }

        _position += token.Length;
        return true;
    }

    private void SkipWhitespace()
    {
        while (_position < _text.Length &&
               char.IsWhiteSpace(_text[_position]))
        {
            _position++;
        }
    }
}
