using System;
using WpEngine.Core;

namespace Tests;

public class ExpressionParserTests
{
    private static Expr Num(double value) => new NumberExpr(value);

    private static Expr Var(string name) => new VariableExpr(name);

    private static Expr Bin(Expr left, string op, Expr right) => new BinaryExpr(left, op, right);

    // ---------- Литералы и идентификаторы ----------

    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("42", 42.0)]
    [InlineData("007", 7.0)]
    [InlineData("3.14", 3.14)]
    [InlineData("0.5", 0.5)]
    [InlineData(".5", 0.5)]
    public void Parse_Number_ReturnsNumberExpr(string text, double expected)
    {
        // Проверяем разбор целых и дробных чисел (точка — разделитель всегда).
        Expr result = ExpressionParser.Parse(text);

        Assert.Equal(Num(expected), result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Parse_BooleanLiteral_ReturnsBooleanExpr(string text, bool expected)
    {
        // Проверяем логические константы.
        Expr result = ExpressionParser.Parse(text);

        Assert.Equal(new BooleanExpr(expected), result);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("x_1")]
    [InlineData("_tmp")]
    [InlineData("rootsCount")]
    [InlineData("truex")]
    [InlineData("absolute")]
    [InlineData("Переменная")]
    public void Parse_Identifier_ReturnsVariableExpr(string name)
    {
        // Идентификатор, который только начинается с ключевого слова, остаётся обычной переменной.
        Expr result = ExpressionParser.Parse(name);

        Assert.Equal(Var(name), result);
    }

    // ---------- Приоритеты и ассоциативность ----------

    [Fact]
    public void Parse_MultiplicationBindsTighterThanAddition()
    {
        // 1 + 2 * 3 = 1 + (2 * 3)
        Expr result = ExpressionParser.Parse("1 + 2 * 3");

        Assert.Equal(Bin(Num(1), "+", Bin(Num(2), "*", Num(3))), result);
    }

    [Fact]
    public void Parse_Subtraction_IsLeftAssociative()
    {
        // 10 - 4 - 3 = (10 - 4) - 3
        Expr result = ExpressionParser.Parse("10 - 4 - 3");

        Assert.Equal(Bin(Bin(Num(10), "-", Num(4)), "-", Num(3)), result);

    }

    [Fact]
    public void Parse_Division_IsLeftAssociative()
    {
        // 8 / 4 / 2 = (8 / 4) / 2
        Expr result = ExpressionParser.Parse("8 / 4 / 2");

        Assert.Equal(Bin(Bin(Num(8), "/", Num(4)), "/", Num(2)), result);
    }

    [Fact]
    public void Parse_Parentheses_OverridePrecedence()
    {
        // (1 + 2) * 3
        Expr result = ExpressionParser.Parse("(1 + 2) * 3");

        Assert.Equal(Bin(Bin(Num(1), "+", Num(2)), "*", Num(3)), result);
    }

    [Fact]
    public void Parse_RedundantParentheses_AreDropped()
    {
        // Скобки не создают отдельных узлов дерева.
        Expr result = ExpressionParser.Parse("((x))");

        Assert.Equal(Var("x"), result);
    }

    [Fact]
    public void Parse_ArithmeticBindsTighterThanComparison()
    {
        // a + 1 > b * 2
        Expr result = ExpressionParser.Parse("a + 1 > b * 2");

        Expr expected = Bin(
            Bin(Var("a"), "+", Num(1)),
            ">",
            Bin(Var("b"), "*", Num(2)));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_ComparisonBindsTighterThanAnd()
    {
        // a > 1 && b < 2
        Expr result = ExpressionParser.Parse("a > 1 && b < 2");

        Expr expected = Bin(
            Bin(Var("a"), ">", Num(1)),
            "&&",
            Bin(Var("b"), "<", Num(2)));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_AndBindsTighterThanOr_OrOnTheRight()
    {
        // a || b && c = a || (b && c)
        Expr result = ExpressionParser.Parse("a || b && c");

        Assert.Equal(Bin(Var("a"), "||", Bin(Var("b"), "&&", Var("c"))), result);
    }

    [Fact]
    public void Parse_AndBindsTighterThanOr_OrOnTheLeft()
    {
        // a && b || c = (a && b) || c
        Expr result = ExpressionParser.Parse("a && b || c");

        Assert.Equal(Bin(Bin(Var("a"), "&&", Var("b")), "||", Var("c")), result);
    }

    [Theory]
    [InlineData(">")]
    [InlineData("<")]
    [InlineData(">=")]
    [InlineData("<=")]
    [InlineData("==")]
    [InlineData("!=")]
    public void Parse_ComparisonOperator_IsRecognizedAsWhole(string op)
    {
        // Двухсимвольные операторы не должны распадаться на > и =, ! и =.
        Expr result = ExpressionParser.Parse($"a {op} b");

        Assert.Equal(Bin(Var("a"), op, Var("b")), result);
    }

    [Fact]
    public void Parse_OperatorsWithoutSpaces_AreRecognized()
    {
        // Пробелы вокруг операторов необязательны.
        Expr result = ExpressionParser.Parse("a>=1&&b!=2");

        Expr expected = Bin(
            Bin(Var("a"), ">=", Num(1)),
            "&&",
            Bin(Var("b"), "!=", Num(2)));

        Assert.Equal(expected, result);
    }

    // ---------- Унарные операторы и функции ----------

    [Fact]
    public void Parse_UnaryMinus_ReturnsUnaryExpr()
    {
        Expr result = ExpressionParser.Parse("-x");

        Assert.Equal(new UnaryExpr("-", Var("x")), result);
    }

    [Fact]
    public void Parse_DoubleUnaryMinus_IsNested()
    {
        Expr result = ExpressionParser.Parse("--x");

        Assert.Equal(new UnaryExpr("-", new UnaryExpr("-", Var("x"))), result);
    }

    [Fact]
    public void Parse_LogicalNot_ReturnsUnaryExpr()
    {
        Expr result = ExpressionParser.Parse("!flag");

        Assert.Equal(new UnaryExpr("!", Var("flag")), result);
    }

    [Fact]
    public void Parse_BinaryMinusFollowedByUnaryMinus()
    {
        // a - -b
        Expr result = ExpressionParser.Parse("a - -b");

        Assert.Equal(Bin(Var("a"), "-", new UnaryExpr("-", Var("b"))), result);
    }

    [Fact]
    public void Parse_UnaryMinusBindsTighterThanMultiplication()
    {
        // -b / (2 * a) = (-b) / (2 * a)
        Expr result = ExpressionParser.Parse("-b / (2 * a)");

        Expr expected = Bin(
            new UnaryExpr("-", Var("b")),
            "/",
            Bin(Num(2), "*", Var("a")));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abs")]
    [InlineData("sqrt")]
    public void Parse_Function_ReturnsFunctionExpr(string name)
    {
        Expr result = ExpressionParser.Parse($"{name}(x + 1)");

        Assert.Equal(new FunctionExpr(name, Bin(Var("x"), "+", Num(1))), result);
    }

    [Fact]
    public void Parse_NestedFunctions_AreSupported()
    {
        Expr result = ExpressionParser.Parse("abs(sqrt(4))");

        Expr expected = new FunctionExpr("abs", new FunctionExpr("sqrt", Num(4)));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_FunctionInsideExpression()
    {
        // (-b - sqrt(D)) / (2 * a)
        Expr result = ExpressionParser.Parse("(-b - sqrt(D)) / (2 * a)");

        Expr expected = Bin(
            Bin(new UnaryExpr("-", Var("b")), "-", new FunctionExpr("sqrt", Var("D"))),
            "/",
            Bin(Num(2), "*", Var("a")));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_NotOverParenthesizedComparison_CombinedWithOr()
    {
        // !(a > 1) || b == true
        Expr result = ExpressionParser.Parse("!(a > 1) || b == true");

        Expr expected = Bin(
            new UnaryExpr("!", Bin(Var("a"), ">", Num(1))),
            "||",
            Bin(Var("b"), "==", new BooleanExpr(true)));

        Assert.Equal(expected, result);
    }

    // ---------- Пробелы ----------

    [Fact]
    public void Parse_IgnoresWhitespaceAroundTokens()
    {
        Expr result = ExpressionParser.Parse("  a   +\t b  ");

        Assert.Equal(Bin(Var("a"), "+", Var("b")), result);
    }

    // ---------- Ошибки ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Parse_EmptyOrWhitespace_ThrowsFormatException(string text)
    {
        Assert.Throws<FormatException>(() => ExpressionParser.Parse(text));
    }

    [Fact]
    public void Parse_Null_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ExpressionParser.Parse(null!));
    }

    [Theory]
    [InlineData("1 +")]
    [InlineData("a ||")]
    [InlineData("(1 + 2")]
    [InlineData("1 + 2)")]
    [InlineData("()")]
    [InlineData("*")]
    [InlineData("a $ b")]
    [InlineData("a & b")]
    [InlineData("a = b")]
    [InlineData("x := 1")]
    [InlineData("abs 5")]
    [InlineData("abs(5")]
    [InlineData("sqrt(")]
    [InlineData("1 2")]
    [InlineData("a b")]
    [InlineData("1.2.3")]
    [InlineData("foo(1)")]
    public void Parse_InvalidSyntax_ThrowsFormatException(string text)
    {
        // Любая синтаксическая ошибка сообщается через FormatException.
        Assert.Throws<FormatException>(() => ExpressionParser.Parse(text));
    }

    [Fact]
    public void Parse_Error_MessageContainsPosition()
    {
        var exception = Assert.Throws<FormatException>(
            () => ExpressionParser.Parse("1 + $"));

        Assert.Contains("Позиция", exception.Message);
        Assert.Contains("$", exception.Message);
    }

    [Fact]
    public void Parse_UnbalancedParenthesis_MessageMentionsParenthesis()
    {
        var exception = Assert.Throws<FormatException>(
            () => ExpressionParser.Parse("(1 + 2"));

        Assert.Contains(")", exception.Message);
    }
}