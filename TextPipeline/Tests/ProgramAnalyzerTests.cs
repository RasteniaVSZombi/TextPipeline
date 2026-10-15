using System;
using System.Collections.Generic;
using System.Linq;
using WpEngine.Core;
using WpEngine.Models;
using WpEngine.Parsing;

namespace Tests;

public class ProgramAnalyzerTests
{
    private static ProgramAnalysis Analyze(string program)
    {
        return new ProgramAnalyzer().Analyze(Parser.Parse(program));
    }

    private static List<string> SortedVariables(ProgramAnalysis analysis)
    {
        return analysis.Variables.OrderBy(v => v, StringComparer.Ordinal).ToList();
    }

    [Fact]
    public void Analyze_SingleAssignment_CountsOneStatement()
    {
        ProgramAnalysis analysis = Analyze("x := a + b");

        Assert.Equal(1, analysis.StatementCount);
        Assert.Equal(0, analysis.BranchCount);
        Assert.Equal(new[] { "x := a + b" }, analysis.Assignments);
        Assert.Empty(analysis.Conditions);
    }

    [Fact]
    public void Analyze_Assignment_CollectsTargetAndOperandVariables()
    {
        ProgramAnalysis analysis = Analyze("x := a + b");

        Assert.Equal(new[] { "a", "b", "x" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_Sequence_CountsEveryStatementInOrder()
    {
        ProgramAnalysis analysis = Analyze("x := a + 2\ny := x * 3\nz := y - 4");

        Assert.Equal(3, analysis.StatementCount);
        Assert.Equal(0, analysis.BranchCount);
        Assert.Equal(
            new[] { "x := a + 2", "y := x * 3", "z := y - 4" },
            analysis.Assignments);
        Assert.Equal(new[] { "a", "x", "y", "z" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_VariableUsedSeveralTimes_IsReportedOnce()
    {
        ProgramAnalysis analysis = Analyze("x := a + a * a\nx := x + 1");

        Assert.Equal(new[] { "a", "x" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_ConstantsAndFunctions_ContributeOnlyVariables()
    {
        ProgramAnalysis analysis = Analyze("y := abs(x) + sqrt(4) + 1.5");

        Assert.Equal(new[] { "x", "y" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_BooleanConstants_AreNotVariables()
    {
        ProgramAnalysis analysis = Analyze("flag := true && p");

        Assert.Equal(new[] { "flag", "p" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_If_CountsConditionalAndBranchStatements()
    {
        const string program = @"
if a >= b then
    max := a
else
    max := b
end";

        ProgramAnalysis analysis = Analyze(program);

        // Один if и два присваивания.
        Assert.Equal(3, analysis.StatementCount);
        Assert.Equal(1, analysis.BranchCount);
        Assert.Equal(new[] { "a >= b" }, analysis.Conditions);
        Assert.Equal(new[] { "max := a", "max := b" }, analysis.Assignments);
        Assert.Equal(new[] { "a", "b", "max" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_ConditionVariables_AreCollected()
    {
        const string program = @"
if p && (q || r) then
    x := 1
else
    x := 2
end";

        ProgramAnalysis analysis = Analyze(program);

        Assert.Equal(new[] { "p", "q", "r", "x" }, SortedVariables(analysis));
    }

    [Fact]
    public void Analyze_NestedIf_CountsBothBranchesAndKeepsConditionOrder()
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

        ProgramAnalysis analysis = Analyze(program);

        // Два if и три присваивания.
        Assert.Equal(5, analysis.StatementCount);
        Assert.Equal(2, analysis.BranchCount);
        Assert.Equal(new[] { "a > 0", "b > 0" }, analysis.Conditions);
        Assert.Equal(new[] { "r := 1", "r := 2", "r := 3" }, analysis.Assignments);
    }

    [Fact]
    public void Analyze_AssignmentsBeforeAndAfterIf_AreAllCounted()
    {
        const string program = @"
x := 1
if a > 0 then
    y := 1
else
    y := 2
end
z := 3";

        ProgramAnalysis analysis = Analyze(program);

        Assert.Equal(5, analysis.StatementCount);
        Assert.Equal(1, analysis.BranchCount);
        Assert.Equal(
            new[] { "x := 1", "y := 1", "y := 2", "z := 3" },
            analysis.Assignments);
    }

    [Fact]
    public void Analyze_AssignmentTextIsKeptAsWritten()
    {
        // В списке присваиваний хранится исходное выражение, а не его каноническая запись.
        ProgramAnalysis analysis = Analyze("x   :=   a+1");

        Assert.Equal(new[] { "x := a+1" }, analysis.Assignments);
    }

    [Fact]
    public void Analyze_FreshAnalyzerResult_DoesNotAccumulateBetweenCalls()
    {
        var analyzer = new ProgramAnalyzer();

        ProgramAnalysis first = analyzer.Analyze(Parser.Parse("x := 1"));
        ProgramAnalysis second = analyzer.Analyze(Parser.Parse("y := 2\nz := 3"));

        Assert.Equal(1, first.StatementCount);
        Assert.Equal(2, second.StatementCount);
        Assert.DoesNotContain("x", second.Variables);
    }

    [Fact]
    public void Analyze_UnknownStatementType_ThrowsNotSupportedException()
    {
        var analyzer = new ProgramAnalyzer();

        Assert.Throws<NotSupportedException>(() => analyzer.Analyze(new UnknownStatement()));
    }

    [Fact]
    public void Analyze_UnknownExpressionInAssignment_ThrowsFormatException()
    {
        // Выражение хранится строкой и разбирается при анализе.
        var analyzer = new ProgramAnalyzer();

        Assert.Throws<FormatException>(
            () => analyzer.Analyze(new Assignment("x", "1 +")));
    }

    // Оператор неизвестного типа — для проверки защитной ветки.
    private sealed record UnknownStatement : Statement;
}