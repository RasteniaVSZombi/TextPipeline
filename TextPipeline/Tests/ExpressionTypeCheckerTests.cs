using System;
using System.Collections.Generic;
using WpEngine.Core;
using WpEngine.Models;
using WpEngine.Parsing;

namespace Tests;

public class ExpressionTypeCheckerTests
{
    private static ExpressionTypeChecker Check(string program, string postcondition)
    {
        var checker = new ExpressionTypeChecker();
        checker.CheckProgram(Parser.Parse(program), postcondition);
        return checker;
    }

    // ---------- Корректные программы и вывод типов ----------

    [Fact]
    public void CheckProgram_NumericAssignment_InfersNumberForTargetAndOperands()
    {
        ExpressionTypeChecker checker = Check("x := a + 1", "x > 0");

        IReadOnlyDictionary<string, ExpressionType> types = checker.VariableTypes;

        Assert.Equal(ExpressionType.Number, types["x"]);
        Assert.Equal(ExpressionType.Number, types["a"]);
    }

    [Fact]
    public void CheckProgram_ComparisonAssignment_InfersBooleanForTarget()
    {
        ExpressionTypeChecker checker = Check("flag := a > b", "flag");

        IReadOnlyDictionary<string, ExpressionType> types = checker.VariableTypes;

        Assert.Equal(ExpressionType.Boolean, types["flag"]);
        Assert.Equal(ExpressionType.Number, types["a"]);
        Assert.Equal(ExpressionType.Number, types["b"]);
    }

    [Fact]
    public void CheckProgram_BooleanConstantAssignment_InfersBoolean()
    {
        ExpressionTypeChecker checker = Check("ok := true", "ok == true");

        Assert.Equal(ExpressionType.Boolean, checker.VariableTypes["ok"]);
    }

    [Fact]
    public void CheckProgram_FunctionsAndUnaryMinus_AreNumeric()
    {
        ExpressionTypeChecker checker = Check("y := sqrt(abs(-x))", "y >= 0");

        Assert.Equal(ExpressionType.Number, checker.VariableTypes["y"]);
        Assert.Equal(ExpressionType.Number, checker.VariableTypes["x"]);
    }

    [Fact]
    public void CheckProgram_NegationOfBooleanVariable_IsAllowed()
    {
        ExpressionTypeChecker checker = Check("p := !q\nq := a > 0", "p || q");

        Assert.Equal(ExpressionType.Boolean, checker.VariableTypes["p"]);
        Assert.Equal(ExpressionType.Boolean, checker.VariableTypes["q"]);
    }

    [Fact]
    public void CheckProgram_IfWithBothBranches_InfersTypesFromCondition()
    {
        const string program = @"
if a >= b then
    max := a
else
    max := b
end";

        ExpressionTypeChecker checker = Check(
            program,
            "(max >= a) && (max >= b) && ((max == a) || (max == b))");

        Assert.Equal(ExpressionType.Number, checker.VariableTypes["max"]);
        Assert.Equal(ExpressionType.Number, checker.VariableTypes["a"]);
        Assert.Equal(ExpressionType.Number, checker.VariableTypes["b"]);
    }

    [Fact]
    public void CheckProgram_EqualityUnifiesOperandTypes()
    {
        // a == b связывает типы a и b, а из постусловия a > 0 следует, что оба — числа.
        const string program = @"
if a == b then
    x := 1
else
    x := 2
end";

        ExpressionTypeChecker checker = Check(program, "a > 0");

        Assert.Equal(ExpressionType.Number, checker.VariableTypes["a"]);
        Assert.Equal(ExpressionType.Number, checker.VariableTypes["b"]);
    }

    [Fact]
    public void CheckProgram_VariablesWithUnknownType_AreNotReported()
    {
        // p и q только сравниваются между собой, поэтому их тип вывести нельзя.
        var checker = new ExpressionTypeChecker();

        checker.CheckProgram(new Assignment("x", "1"), "p == q");

        IReadOnlyDictionary<string, ExpressionType> types = checker.VariableTypes;

        Assert.Equal(ExpressionType.Number, types["x"]);
        Assert.DoesNotContain("p", types.Keys);
        Assert.DoesNotContain("q", types.Keys);
    }

    [Fact]
    public void CheckProgram_CalledTwice_ForgetsPreviousProgram()
    {
        var checker = new ExpressionTypeChecker();

        checker.CheckProgram(Parser.Parse("x := 1"), "x > 0");
        checker.CheckProgram(Parser.Parse("y := true"), "y");

        IReadOnlyDictionary<string, ExpressionType> types = checker.VariableTypes;

        Assert.DoesNotContain("x", types.Keys);
        Assert.Equal(ExpressionType.Boolean, types["y"]);
    }

    [Fact]
    public void VariableTypes_BeforeAnyCheck_IsEmpty()
    {
        var checker = new ExpressionTypeChecker();

        Assert.Empty(checker.VariableTypes);
    }

    // ---------- Ошибки типов ----------

    [Fact]
    public void CheckProgram_NumericPostcondition_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Check("x := 1", "x + 1"));

        Assert.Contains("Постусловие должно быть логическим выражением", exception.Message);
    }

    [Fact]
    public void CheckProgram_NonBooleanIfCondition_Throws()
    {
        const string program = @"
if a + 1 then
    x := 1
else
    x := 2
end";

        var exception = Assert.Throws<InvalidOperationException>(
            () => Check(program, "x > 0"));

        Assert.Contains("Условие if должно быть логическим", exception.Message);
    }

    [Fact]
    public void CheckProgram_VariableAssignedNumberThenBoolean_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Check("x := 1\nx := true", "x"));

        Assert.Contains("Несовместимые типы при присваивании переменной 'x'", exception.Message);
    }

    [Fact]
    public void CheckProgram_BranchesAssignDifferentTypes_Throws()
    {
        const string program = @"
if a > 0 then
    x := 1
else
    x := true
end";

        Assert.Throws<InvalidOperationException>(() => Check(program, "x == x"));
    }

    [Fact]
    public void CheckProgram_EqualityOfNumberAndBoolean_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Check("x := 1\nflag := true", "x == flag"));

        Assert.Contains("одинаковые типы", exception.Message);
    }

    [Theory]
    [InlineData("x := 1 && 2")]
    [InlineData("x := true || 1")]
    [InlineData("x := -true")]
    [InlineData("x := !5")]
    [InlineData("x := abs(true)")]
    [InlineData("x := sqrt(a > 1)")]
    [InlineData("x := true + 1")]
    [InlineData("x := 1 * false")]
    [InlineData("x := 1 < true")]
    [InlineData("x := true > false")]
    [InlineData("x := 1 == true")]
    [InlineData("x := (a > 1) / 2")]
    public void CheckProgram_OperatorAppliedToWrongType_Throws(string program)
    {
        Assert.Throws<InvalidOperationException>(() => Check(program, "true"));
    }

    [Fact]
    public void CheckProgram_UnknownFunctionInExpression_ThrowsFormatException()
    {
        // Парсер знает только abs и sqrt, поэтому вызов foo(1) не разбирается вовсе.
        var assignment = new Assignment("x", "foo(1)");

        Assert.Throws<FormatException>(
            () => new ExpressionTypeChecker().CheckProgram(assignment, "true"));
    }

    [Fact]
    public void CheckProgram_UnknownStatementType_ThrowsNotSupportedException()
    {
        var checker = new ExpressionTypeChecker();

        Assert.Throws<NotSupportedException>(
            () => checker.CheckProgram(new UnknownStatement(), "true"));
    }

    [Fact]
    public void CheckProgram_InvalidExpressionInAssignment_ThrowsFormatException()
    {
        var checker = new ExpressionTypeChecker();

        Assert.Throws<FormatException>(
            () => checker.CheckProgram(new Assignment("x", "1 +"), "true"));
    }

    [Fact]
    public void CheckProgram_InvalidPostcondition_ThrowsFormatException()
    {
        var checker = new ExpressionTypeChecker();

        Assert.Throws<FormatException>(
            () => checker.CheckProgram(new Assignment("x", "1"), "x >"));
    }

    // Оператор неизвестного типа — для проверки защитной ветки.
    private sealed record UnknownStatement : Statement;
}