using System;
using WpEngine.Core;

namespace Tests;

public class ExpressionSimplifierTests
{
    // ---------- Свёртка констант ----------

    [Theory]
    [InlineData("2 + 3", "5")]
    [InlineData("8 / 2", "4")]
    [InlineData("1.5 + 1.5", "3")]
    [InlineData("-5", "-5")]
    [InlineData("-(2 + 3)", "-5")]
    [InlineData("abs(-3)", "3")]
    [InlineData("sqrt(16)", "4")]
    [InlineData("sqrt(4) + 1", "3")]
    public void Simplify_ArithmeticOnConstants_IsFolded(string input, string expected)
    {
        // Арифметика над числами вычисляется сразу.
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    [Theory]
    [InlineData("3 > 2", "true")]
    [InlineData("2 > 3", "false")]
    [InlineData("2 <= 2", "true")]
    [InlineData("2 == 2", "true")]
    [InlineData("2 != 2", "false")]
    [InlineData("2 + 3 > 4", "true")]
    public void Simplify_ComparisonOfConstants_BecomesBoolean(string input, string expected)
    {
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    [Theory]
    [InlineData("true && false", "false")]
    [InlineData("true == false", "false")]
    [InlineData("true != false", "true")]
    [InlineData("!true", "false")]
    [InlineData("!false", "true")]
    public void Simplify_BooleanConstants_AreFolded(string input, string expected)
    {
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    // ---------- Нейтральные и поглощающие элементы ----------

    [Theory]
    [InlineData("x + 0", "x")]
    [InlineData("0 + x", "x")]
    [InlineData("x - 0", "x")]
    [InlineData("x * 1", "x")]
    [InlineData("1 * x", "x")]
    [InlineData("x / 1", "x")]
    [InlineData("x + 0 + 0", "x")]
    [InlineData("(x + 0) * 1", "x")]
    [InlineData("y * 1 + 0", "y")]
    public void Simplify_NeutralElements_AreRemoved(string input, string expected)
    {
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    [Theory]
    [InlineData("x * 0", "0")]
    [InlineData("0 * x", "0")]
    public void Simplify_MultiplicationByZero_BecomesZero(string input, string expected)
    {
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    [Theory]
    [InlineData("true && x", "x")]
    [InlineData("x && true", "x")]
    [InlineData("false && x", "false")]
    [InlineData("x && false", "false")]
    [InlineData("true || x", "true")]
    [InlineData("x || true", "true")]
    [InlineData("false || x", "x")]
    [InlineData("x || false", "x")]
    [InlineData("(x > 1) && true", "(x > 1)")]
    public void Simplify_BooleanIdentities_AreApplied(string input, string expected)
    {
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    [Theory]
    [InlineData("!(!x)", "x")]
    [InlineData("-(-x)", "x")]
    public void Simplify_DoubleNegation_IsRemoved(string input, string expected)
    {
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    // ---------- Случаи, когда упрощать нельзя ----------

    [Fact]
    public void Simplify_DivisionByZero_IsNotFolded()
    {
        // Деление на ноль оставляем как есть, а не падаем и не вычисляем бесконечность.
        Assert.Equal("(1 / 0)", ExpressionSimplifier.Simplify("1 / 0"));
    }

    [Fact]
    public void Simplify_SqrtOfNegativeConstant_IsNotFolded()
    {
        Assert.Equal("sqrt(-4)", ExpressionSimplifier.Simplify("sqrt(-4)"));
    }

    [Theory]
    [InlineData("x", "x")]
    [InlineData("true", "true")]
    [InlineData("x + y", "(x + y)")]
    [InlineData("abs(x)", "abs(x)")]
    [InlineData("-x", "-(x)")]
    [InlineData("a + b * 2", "(a + (b * 2))")]
    [InlineData("(a + b) * 2", "((a + b) * 2)")]
    [InlineData("a - (b - c)", "(a - (b - c))")]
    public void Simplify_ExpressionWithVariables_IsOnlyFormatted(string input, string expected)
    {
        // Если упрощать нечего, результат — это каноническая запись выражения.
        Assert.Equal(expected, ExpressionSimplifier.Simplify(input));
    }

    [Fact]
    public void Simplify_PartiallyConstantExpression_FoldsOnlyConstantPart()
    {
        // 2 * 3 + x -> 6 + x
        Assert.Equal("(6 + x)", ExpressionSimplifier.Simplify("2 * 3 + x"));
    }

    [Fact]
    public void Simplify_IsIdempotent()
    {
        // Повторное упрощение уже упрощённой записи ничего не меняет.
        string once = ExpressionSimplifier.Simplify("(x + 0) * 1 + 2 * 3");
        string twice = ExpressionSimplifier.Simplify(once);

        Assert.Equal(once, twice);
    }

    // ---------- Ошибки ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1 +")]
    [InlineData("(x")]
    public void Simplify_InvalidExpression_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => ExpressionSimplifier.Simplify(input));
    }
}