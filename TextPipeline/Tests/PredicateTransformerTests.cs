using System;
using System.Collections.Generic;
using WpEngine.Core;

namespace Tests;

public class PredicateTransformerTests
{
    // ---------- Format ----------

    [Theory]
    [InlineData("a+b*2", "(a + (b * 2))")]
    [InlineData("-x", "-(x)")]
    [InlineData("!a", "!(a)")]
    [InlineData("abs(x)", "abs(x)")]
    [InlineData("sqrt(a + 1)", "sqrt((a + 1))")]
    [InlineData("true", "true")]
    [InlineData("2.5", "2.5")]
    [InlineData("a >= b && c", "((a >= b) && c)")]
    [InlineData("a - b - c", "((a - b) - c)")]
    [InlineData("(a)", "a")]
    public void Format_ParsedExpression_ReturnsCanonicalText(string input, string expected)
    {
        // Каноническая запись: каждая бинарная операция берётся в скобки.
        string result = PredicateTransformer.Format(ExpressionParser.Parse(input));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Format_ThenParse_GivesSameTree()
    {
        // Запись, полученная из дерева, разбирается обратно в то же самое дерево.
        Expr original = ExpressionParser.Parse("!(a > 1) || (b + 2) * c <= abs(d)");

        Expr roundTrip = ExpressionParser.Parse(PredicateTransformer.Format(original));

        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void Format_UnknownUnaryOperator_ThrowsNotSupportedException()
    {
        var expression = new UnaryExpr("~", new VariableExpr("x"));

        Assert.Throws<NotSupportedException>(() => PredicateTransformer.Format(expression));
    }

    [Fact]
    public void Format_UnknownExpressionType_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => PredicateTransformer.Format(new UnknownExpr()));
    }

    // ---------- Substitute ----------

    [Fact]
    public void Substitute_ReplacesVariableWithExpression()
    {
        string result = PredicateTransformer.Substitute(
            "x > 5", "x", "y + 1", out List<string> definedness);

        Assert.Equal("((y + 1) > 5)", result);
        Assert.Empty(definedness);
    }

    [Fact]
    public void Substitute_ReplacesAllOccurrences()
    {
        string result = PredicateTransformer.Substitute(
            "x == x", "x", "a", out _);

        Assert.Equal("(a == a)", result);
    }

    [Fact]
    public void Substitute_KeepsOtherVariables()
    {
        string result = PredicateTransformer.Substitute(
            "x > y", "x", "3", out _);

        Assert.Equal("(3 > y)", result);
    }

    [Fact]
    public void Substitute_VariableAbsentInPredicate_LeavesPredicateUnchanged()
    {
        string result = PredicateTransformer.Substitute(
            "y > 0", "x", "1", out _);

        Assert.Equal("(y > 0)", result);
    }

    [Fact]
    public void Substitute_WholePredicateIsVariable_ReturnsExpression()
    {
        string result = PredicateTransformer.Substitute(
            "p", "p", "a > b", out _);

        Assert.Equal("(a > b)", result);
    }

    [Fact]
    public void Substitute_ReplacementInsideLargerPredicate()
    {
        string result = PredicateTransformer.Substitute(
            "x > 0 && y > 0", "x", "a * 2", out _);

        Assert.Equal("(((a * 2) > 0) && (y > 0))", result);
    }

    [Fact]
    public void Substitute_UnaryMinusExpression()
    {
        string result = PredicateTransformer.Substitute(
            "x > 0", "x", "-a", out _);

        Assert.Equal("(-(a) > 0)", result);
    }

    [Fact]
    public void Substitute_NegatedPredicate()
    {
        string result = PredicateTransformer.Substitute(
            "!flag", "flag", "a == b", out _);

        Assert.Equal("!((a == b))", result);
    }

    [Fact]
    public void Substitute_Division_AddsNonZeroGuard()
    {
        string result = PredicateTransformer.Substitute(
            "x > 0", "x", "a / b", out List<string> definedness);

        Assert.Equal("(((b) != 0)) && (((a / b) > 0))", result);
        Assert.Equal(new[] { "(b) != 0" }, definedness);
    }

    [Fact]
    public void Substitute_Sqrt_AddsNonNegativeGuard()
    {
        string result = PredicateTransformer.Substitute(
            "x > 0", "x", "sqrt(a)", out List<string> definedness);

        Assert.Equal("(((a) >= 0)) && ((sqrt(a) > 0))", result);
        Assert.Equal(new[] { "(a) >= 0" }, definedness);
    }

    [Fact]
    public void Substitute_ExpressionSubstitutedTwice_GuardIsNotDuplicated()
    {
        // 1 / c попадает в предикат дважды, но ограничение определённости одно.
        string result = PredicateTransformer.Substitute(
            "x + x > 0", "x", "1 / c", out List<string> definedness);

        Assert.Equal("(((c) != 0)) && ((((1 / c) + (1 / c)) > 0))", result);
        Assert.Single(definedness);
    }

    [Theory]
    [InlineData("", "x", "1")]
    [InlineData("   ", "x", "1")]
    [InlineData(null, "x", "1")]
    public void Substitute_EmptyPredicate_ThrowsArgumentException(
        string? predicate, string variable, string expression)
    {
        Assert.Throws<ArgumentException>(() => PredicateTransformer.Substitute(
            predicate!, variable, expression, out _));
    }

    [Theory]
    [InlineData("x > 0", "", "1")]
    [InlineData("x > 0", "  ", "1")]
    [InlineData("x > 0", null, "1")]
    public void Substitute_EmptyVariable_ThrowsArgumentException(
        string predicate, string? variable, string expression)
    {
        Assert.Throws<ArgumentException>(() => PredicateTransformer.Substitute(
            predicate, variable!, expression, out _));
    }

    [Theory]
    [InlineData("x > 0", "x", "")]
    [InlineData("x > 0", "x", "  ")]
    [InlineData("x > 0", "x", null)]
    public void Substitute_EmptyExpression_ThrowsArgumentException(
        string predicate, string variable, string? expression)
    {
        Assert.Throws<ArgumentException>(() => PredicateTransformer.Substitute(
            predicate, variable, expression!, out _));
    }

    [Fact]
    public void Substitute_InvalidPredicateSyntax_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PredicateTransformer.Substitute(
            "x >", "x", "1", out _));
    }

    [Fact]
    public void Substitute_InvalidExpressionSyntax_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PredicateTransformer.Substitute(
            "x > 0", "x", "1 +", out _));
    }

    // ---------- GetDefinednessConditions ----------

    [Theory]
    [InlineData("a + b")]
    [InlineData("a * b - c")]
    [InlineData("abs(x)")]
    [InlineData("a > b && c")]
    [InlineData("5")]
    public void GetDefinednessConditions_TotalExpression_ReturnsEmptyList(string expression)
    {
        Assert.Empty(PredicateTransformer.GetDefinednessConditions(expression));
    }

    [Fact]
    public void GetDefinednessConditions_Division_RequiresNonZeroDivisor()
    {
        List<string> result = PredicateTransformer.GetDefinednessConditions("a / b");

        Assert.Equal(new[] { "(b) != 0" }, result);
    }

    [Fact]
    public void GetDefinednessConditions_Sqrt_RequiresNonNegativeArgument()
    {
        List<string> result = PredicateTransformer.GetDefinednessConditions("sqrt(x)");

        Assert.Equal(new[] { "(x) >= 0" }, result);
    }

    [Fact]
    public void GetDefinednessConditions_ComplexDivisor_IsFormattedWithParentheses()
    {
        List<string> result = PredicateTransformer.GetDefinednessConditions("a / (b + 1)");

        Assert.Equal(new[] { "((b + 1)) != 0" }, result);
    }

    [Fact]
    public void GetDefinednessConditions_SqrtAndDivision_CollectedLeftToRight()
    {
        List<string> result = PredicateTransformer.GetDefinednessConditions("sqrt(a) / b");

        Assert.Equal(new[] { "(a) >= 0", "(b) != 0" }, result);
    }

    [Fact]
    public void GetDefinednessConditions_NestedDivision_InnerConditionComesFirst()
    {
        List<string> result = PredicateTransformer.GetDefinednessConditions("1 / (a / b)");

        Assert.Equal(new[] { "(b) != 0", "((a / b)) != 0" }, result);
    }

    [Fact]
    public void GetDefinednessConditions_SameDivisorTwice_ReturnedOnce()
    {
        List<string> result = PredicateTransformer.GetDefinednessConditions("a / b + c / b");

        Assert.Equal(new[] { "(b) != 0" }, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetDefinednessConditions_EmptyExpression_ThrowsArgumentException(string? expression)
    {
        Assert.Throws<ArgumentException>(
            () => PredicateTransformer.GetDefinednessConditions(expression!));
    }

    [Fact]
    public void GetDefinednessConditions_InvalidSyntax_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(
            () => PredicateTransformer.GetDefinednessConditions("a /"));
    }

    // Выражение неизвестного типа — для проверки защитных веток.
    private sealed record UnknownExpr : Expr;
}