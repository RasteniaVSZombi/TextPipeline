using System;
using System.Collections.Generic;
using System.Linq;
using WpEngine.Core;
using WpEngine.Models;

namespace Tests;

public class WpEngineServiceTests
{
    private readonly WpEngineService _service = new();

    // ---------- Calculate ----------

    [Fact]
    public void Calculate_DelegatesToCalculator()
    {
        WpResult fromService = _service.Calculate("x := a + 2", "x > 5");
        WpResult fromCalculator = new WpCalculator().Calculate("x := a + 2", "x > 5");

        Assert.True(fromService.Success);
        Assert.Equal(fromCalculator.Precondition, fromService.Precondition);
        Assert.Equal(fromCalculator.HoareTriple, fromService.HoareTriple);
        Assert.Equal(fromCalculator.Steps, fromService.Steps);
    }

    [Fact]
    public void Calculate_InvalidInput_ReturnsFailure()
    {
        WpResult result = _service.Calculate("x = 1", "x > 0");

        Assert.False(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Error));
    }

    // ---------- AnalyzeProgram ----------

    [Fact]
    public void AnalyzeProgram_ValidProgram_ReturnsStructure()
    {
        ProgramAnalysis analysis = _service.AnalyzeProgram("x := a + b");

        Assert.Equal(1, analysis.StatementCount);
        Assert.Equal(0, analysis.BranchCount);
        Assert.Equal(3, analysis.Variables.Count);
        Assert.Contains("x", analysis.Variables);
        Assert.Contains("a", analysis.Variables);
        Assert.Contains("b", analysis.Variables);
    }

    [Fact]
    public void AnalyzeProgram_InvalidSyntax_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _service.AnalyzeProgram("x = 1"));
    }

    [Fact]
    public void AnalyzeProgram_EmptyProgram_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _service.AnalyzeProgram("  "));
    }

    // ---------- Execute ----------

    [Fact]
    public void Execute_ValidProgram_FillsCalculationAndStatistics()
    {
        const string program = @"
if a >= b then
    max := a
else
    max := b
end";

        WpExecutionResult result = _service.Execute(program, "max >= a");

        Assert.True(result.Calculation.Success);
        Assert.Equal(3, result.StatementCount);
        Assert.Equal(1, result.BranchCount);
        Assert.Equal(new[] { "a", "b", "max" }, result.Variables);
        Assert.Equal(new[] { "max := a", "max := b" }, result.Assignments);
        Assert.Equal(new[] { "a >= b" }, result.Conditions);
    }

    [Fact]
    public void Execute_Variables_AreSortedAlphabetically()
    {
        WpExecutionResult result = _service.Execute("z := b + a\ny := z", "y > 0");

        Assert.True(result.Calculation.Success);
        Assert.Equal(new[] { "a", "b", "y", "z" }, result.Variables);
    }

    [Fact]
    public void Execute_InvalidProgram_ReturnsFailureWithoutStatistics()
    {
        WpExecutionResult result = _service.Execute("x = 1", "x > 0");

        Assert.False(result.Calculation.Success);
        Assert.False(string.IsNullOrEmpty(result.Calculation.Error));
        Assert.Equal(0, result.StatementCount);
        Assert.Equal(0, result.BranchCount);
        Assert.Empty(result.Variables);
        Assert.Empty(result.Assignments);
        Assert.Empty(result.Conditions);
    }

    [Fact]
    public void Execute_ResultMatchesCalculate()
    {
        WpExecutionResult executed = _service.Execute("x := a / b", "x > 0");
        WpResult calculated = _service.Calculate("x := a / b", "x > 0");

        Assert.Equal(calculated.Precondition, executed.Calculation.Precondition);
        Assert.Equal(calculated.DefinednessConditions, executed.Calculation.DefinednessConditions);
    }

    // ---------- Примеры ----------

    [Fact]
    public void GetExamples_ReturnsFourExamplesWithExpectedNames()
    {
        IReadOnlyList<WpExample> examples = _service.GetExamples();

        Assert.Equal(4, examples.Count);
        Assert.Equal("Ручной ввод", examples[0].Name);
        Assert.Equal("Максимум двух чисел", examples[1].Name);
        Assert.Equal("Квадратное уравнение", examples[2].Name);
        Assert.Equal("Последовательность присваиваний", examples[3].Name);
    }

    [Fact]
    public void GetExamples_ReturnsSharedExampleList()
    {
        Assert.Same(ExamplePrograms.All, _service.GetExamples());
    }

    [Fact]
    public void Examples_ManualInput_IsEmpty()
    {
        WpExample manual = ExamplePrograms.All[0];

        Assert.True(string.IsNullOrWhiteSpace(manual.Program));
        Assert.Equal(string.Empty, manual.Postcondition);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Examples_ReadyExamples_AreFullyFilledIn(int index)
    {
        WpExample example = ExamplePrograms.All[index];

        Assert.False(string.IsNullOrWhiteSpace(example.Name));
        Assert.False(string.IsNullOrWhiteSpace(example.Program));
        Assert.False(string.IsNullOrWhiteSpace(example.Postcondition));
        Assert.False(string.IsNullOrWhiteSpace(example.PostconditionDescription));
    }

    [Fact]
    public void Examples_Names_AreUnique()
    {
        List<string> names = ExamplePrograms.All.Select(e => e.Name).ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ExecuteExample_ReadyExample_Succeeds(int index)
    {
        WpExecutionResult result = _service.ExecuteExample(index);

        Assert.True(result.Calculation.Success, result.Calculation.Error);
        Assert.False(string.IsNullOrWhiteSpace(result.Calculation.Precondition));
        Assert.NotEmpty(result.Calculation.Steps);
        Assert.True(result.StatementCount > 0);
    }

    [Fact]
    public void ExecuteExample_ManualInput_FailsBecauseProgramIsEmpty()
    {
        WpExecutionResult result = _service.ExecuteExample(0);

        Assert.False(result.Calculation.Success);
        Assert.Equal("Программа не может быть пустой.", result.Calculation.Error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(100)]
    public void ExecuteExample_IndexOutOfRange_ReturnsFailureWithIndexInMessage(int index)
    {
        WpExecutionResult result = _service.ExecuteExample(index);

        Assert.False(result.Calculation.Success);
        Assert.Equal($"Пример с индексом {index} не существует.", result.Calculation.Error);
        Assert.Equal(0, result.StatementCount);
    }

    [Fact]
    public void ExecuteExample_QuadraticEquation_CollectsExpectedStatistics()
    {
        WpExecutionResult result = _service.ExecuteExample(2);

        Assert.True(result.Calculation.Success);

        // D, внешний if, 3 присваивания в then, внутренний if и по 3 присваивания в его ветках.
        Assert.Equal(12, result.StatementCount);
        Assert.Equal(2, result.BranchCount);
        Assert.Equal(10, result.Assignments.Count);
        Assert.Equal(new[] { "D < 0", "D == 0" }, result.Conditions);

        foreach (string name in new[] { "D", "a", "b", "c", "rootsCount", "x1", "x2" })
            Assert.Contains(name, result.Variables);

        Assert.Equal(7, result.Variables.Count);
    }

    [Fact]
    public void ExecuteExample_QuadraticEquation_ReportsDefinednessOfDivisionAndSqrt()
    {
        WpExecutionResult result = _service.ExecuteExample(2);

        List<string> conditions = result.Calculation.DefinednessConditions;

        Assert.Contains(conditions, c => c.Contains("!= 0"));
        Assert.Contains("(D) >= 0", conditions);
    }
}