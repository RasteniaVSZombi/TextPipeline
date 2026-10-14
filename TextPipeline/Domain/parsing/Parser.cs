
using System;
using System.Collections.Generic;
using System.Linq;
using WpEngine.Core;
using WpEngine.Models;

namespace WpEngine.Parsing;

public sealed class Parser
{
    private readonly string[] _lines;
    private int _position;

    private Parser(string source)
    {
        _lines = source
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("//"))
            .ToArray();
    }

    public static Statement Parse(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Программа не может быть пустой.");

        var parser = new Parser(source);
        var statements = parser.ParseBlock();

        if (parser._position < parser._lines.Length)
        {
            throw parser.Error(
                $"Неожиданный оператор '{parser.Current}'.");
        }

        if (statements.Count == 0)
            throw new FormatException("Программа не содержит операторов.");

        return statements.Count == 1
            ? statements[0]
            : new Sequence(statements);
    }

    private string Current =>
        _position < _lines.Length
            ? _lines[_position]
            : "";

    private FormatException Error(string message) =>
        new($"Строка {_position + 1}: {message}");

    private List<Statement> ParseBlock()
    {
        var statements = new List<Statement>();

        while (_position < _lines.Length)
        {
            string line = Current;

            if (line == "else" || line == "end")
                break;

            if (line.StartsWith("if ", StringComparison.Ordinal))
            {
                statements.Add(ParseIf());
            }
            else
            {
                statements.Add(ParseAssignment(line));
                _position++;
            }
        }

        return statements;
    }

    private Statement ParseIf()
    {
        string line = Current;
        _position++;

        const string prefix = "if ";
        int thenIndex = line.LastIndexOf(
            " then", StringComparison.Ordinal);

        if (!line.StartsWith(prefix, StringComparison.Ordinal) ||
            thenIndex < prefix.Length)
        {
            throw Error(
                "Ожидается конструкция: if условие then");
        }

        string condition = line[
            prefix.Length..thenIndex].Trim();

        if (condition.Length == 0)
            throw Error("Условие if не может быть пустым.");

        // Проверяем синтаксис условия.
        ExpressionParser.Parse(condition);

        List<Statement> thenStatements = ParseBlock();

        if (_position >= _lines.Length || Current != "else")
            throw Error("После ветки then ожидается else.");

        _position++; // Пропускаем else.

        List<Statement> elseStatements = ParseBlock();

        if (_position >= _lines.Length || Current != "end")
            throw Error("После ветки else ожидается end.");

        _position++; // Пропускаем end.

        if (thenStatements.Count == 0)
            throw Error("Ветка then не содержит операторов.");

        if (elseStatements.Count == 0)
            throw Error("Ветка else не содержит операторов.");

        return new IfStatement(
            condition,
            CreateBlock(thenStatements),
            CreateBlock(elseStatements));
    }

    private Statement ParseAssignment(string line)
    {
        int index = line.IndexOf(":=", StringComparison.Ordinal);

        if (index < 0)
            throw Error(
                $"Ожидается присваивание ':='. Получено: '{line}'.");

        string variable = line[..index].Trim();
        string expression = line[(index + 2)..].Trim();

        if (!IsIdentifier(variable))
            throw Error($"Недопустимое имя переменной '{variable}'.");

        if (expression.Length == 0)
            throw Error("Выражение присваивания не может быть пустым.");

        // Проверяем выражение до запуска вычислителя.
        ExpressionParser.Parse(expression);

        return new Assignment(variable, expression);
    }

    private static Statement CreateBlock(List<Statement> statements)
    {
        return statements.Count == 1
            ? statements[0]
            : new Sequence(statements);
    }

    private static bool IsIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value) ||
            !(char.IsLetter(value[0]) || value[0] == '_'))
            return false;

        return value.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}
