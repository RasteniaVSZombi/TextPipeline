using System;
using WpEngine.Models;
using WpEngine.Parsing;

namespace Tests;

public class ProgramParserTests
{
    // ---------- Присваивания и последовательности ----------

    [Fact]
    public void Parse_SingleAssignment_ReturnsAssignment()
    {
        Statement result = Parser.Parse("x := 1");

        Assert.Equal(new Assignment("x", "1"), result);
    }

    [Fact]
    public void Parse_AssignmentWithExtraSpaces_TrimsNameAndExpression()
    {
        Statement result = Parser.Parse("  x   :=   a + 1  ");

        Assert.Equal(new Assignment("x", "a + 1"), result);
    }

    [Fact]
    public void Parse_MultipleAssignments_ReturnsSequenceInOrder()
    {
        Statement result = Parser.Parse("x := 1\ny := 2\nz := 3");

        var sequence = Assert.IsType<Sequence>(result);

        Assert.Equal(3, sequence.Statements.Count);
        Assert.Equal(new Assignment("x", "1"), sequence.Statements[0]);
        Assert.Equal(new Assignment("y", "2"), sequence.Statements[1]);
        Assert.Equal(new Assignment("z", "3"), sequence.Statements[2]);
    }

    [Fact]
    public void Parse_CommentsAndBlankLines_AreIgnored()
    {
        const string program = "// начало\n\n  x := 1  \n\n// середина\n y := 2\n";

        var sequence = Assert.IsType<Sequence>(Parser.Parse(program));

        Assert.Equal(2, sequence.Statements.Count);
        Assert.Equal(new Assignment("x", "1"), sequence.Statements[0]);
        Assert.Equal(new Assignment("y", "2"), sequence.Statements[1]);
    }

    [Fact]
    public void Parse_WindowsAndUnixLineEndings_GiveSameResult()
    {
        var windows = Assert.IsType<Sequence>(Parser.Parse("x := 1\r\ny := 2\r\nz := 3"));
        var unix = Assert.IsType<Sequence>(Parser.Parse("x := 1\ny := 2\nz := 3"));

        Assert.Equal(unix.Statements, windows.Statements);
    }

    [Fact]
    public void Parse_IdentifierStartingWithIf_IsAnAssignmentNotAConditional()
    {
        // Ключевое слово if распознаётся только вместе с пробелом после него.
        Statement result = Parser.Parse("iffy := 1");

        Assert.Equal(new Assignment("iffy", "1"), result);
    }

    [Fact]
    public void Parse_UnderscoreAndDigitsInVariableName_AreAllowed()
    {
        Statement result = Parser.Parse("_tmp_1 := 5");

        Assert.Equal(new Assignment("_tmp_1", "5"), result);
    }

    // ---------- Условные операторы ----------

    [Fact]
    public void Parse_IfWithSingleStatementBranches_ReturnsIfStatement()
    {
        const string program = @"
if a >= b then
    max := a
else
    max := b
end";

        Statement result = Parser.Parse(program);

        var expected = new IfStatement(
            "a >= b",
            new Assignment("max", "a"),
            new Assignment("max", "b"));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_IfWithSeveralStatementsInBranch_WrapsBranchInSequence()
    {
        const string program = @"
if a > 0 then
    x := 1
    y := 2
else
    x := 3
end";

        var conditional = Assert.IsType<IfStatement>(Parser.Parse(program));

        var thenBranch = Assert.IsType<Sequence>(conditional.ThenBranch);

        Assert.Equal(2, thenBranch.Statements.Count);
        Assert.Equal(new Assignment("x", "1"), thenBranch.Statements[0]);
        Assert.Equal(new Assignment("y", "2"), thenBranch.Statements[1]);

        // Ветка из одного оператора остаётся самим оператором.
        Assert.Equal(new Assignment("x", "3"), conditional.ElseBranch);
    }

    [Fact]
    public void Parse_NestedIf_IsParsedRecursively()
    {
        const string program = @"
if a > 0 then
    if b > 0 then
        r := 1
    else
        r := 2
    end
else
    r := 3
end";

        Statement result = Parser.Parse(program);

        var expected = new IfStatement(
            "a > 0",
            new IfStatement(
                "b > 0",
                new Assignment("r", "1"),
                new Assignment("r", "2")),
            new Assignment("r", "3"));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_StatementsAroundIf_FormSequenceWithIfInside()
    {
        const string program = @"
x := 1
if a > 0 then
    y := 1
else
    y := 2
end
z := 3";

        var sequence = Assert.IsType<Sequence>(Parser.Parse(program));

        Assert.Equal(3, sequence.Statements.Count);
        Assert.IsType<Assignment>(sequence.Statements[0]);
        Assert.IsType<IfStatement>(sequence.Statements[1]);
        Assert.IsType<Assignment>(sequence.Statements[2]);
    }

    [Fact]
    public void Parse_ConditionContainingThenLikeText_UsesLastThen()
    {
        // Условие может содержать переменную, имя которой начинается на "then".
        Statement result = Parser.Parse("if then_flag then\n x := 1\nelse\n x := 2\nend");

        var conditional = Assert.IsType<IfStatement>(result);

        Assert.Equal("then_flag", conditional.Condition);
    }

    // ---------- Ошибки ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    public void Parse_EmptyProgram_ThrowsArgumentException(string program)
    {
        Assert.Throws<ArgumentException>(() => Parser.Parse(program));
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Parser.Parse(null!));
    }

    [Fact]
    public void Parse_OnlyComments_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() => Parser.Parse("// только комментарий"));

        Assert.Contains("не содержит операторов", exception.Message);
    }

    [Theory]
    [InlineData("x = 1")]
    [InlineData("x :=")]
    [InlineData("1x := 2")]
    [InlineData("x y := 2")]
    [InlineData(":= 5")]
    [InlineData("x := 1 +")]
    [InlineData("x := (1")]
    [InlineData("if a > b then\n x := 1\nend")]
    [InlineData("if a > b then\n x := 1\nelse\n x := 2")]
    [InlineData("if a > b then\nelse\n x := 1\nend")]
    [InlineData("if a > b then\n x := 1\nelse\nend")]
    [InlineData("if a > b\n x := 1\nelse\n x := 2\nend")]
    [InlineData("if then\n x := 1\nelse\n x := 2\nend")]
    [InlineData("if  then\n x := 1\nelse\n x := 2\nend")]
    [InlineData("if a > then\n x := 1\nelse\n x := 2\nend")]
    [InlineData("end")]
    [InlineData("else")]
    [InlineData("x := 1\nend")]
    [InlineData("x := 1\nelse")]
    [InlineData("if a > b then\n x := 1\nelse\n x := 2\nend\nend")]
    public void Parse_InvalidProgram_ThrowsFormatException(string program)
    {
        Assert.Throws<FormatException>(() => Parser.Parse(program));
    }

    [Fact]
    public void Parse_MissingAssignmentOperator_ReportsLineNumber()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parser.Parse("x := 1\ny = 2"));

        Assert.Contains("Строка 2", exception.Message);
        Assert.Contains(":=", exception.Message);
    }

    [Fact]
    public void Parse_UnexpectedEnd_ReportsLineNumber()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parser.Parse("x := 1\nend"));

        Assert.Contains("Строка 2", exception.Message);
        Assert.Contains("end", exception.Message);
    }

    [Fact]
    public void Parse_MissingElse_MessageMentionsElse()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parser.Parse("if a > b then\n x := 1\nend"));

        Assert.Contains("else", exception.Message);
    }

    [Fact]
    public void Parse_MissingEnd_MessageMentionsEnd()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parser.Parse("if a > b then\n x := 1\nelse\n x := 2"));

        Assert.Contains("end", exception.Message);
    }
}