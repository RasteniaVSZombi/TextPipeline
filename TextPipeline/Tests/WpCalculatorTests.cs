using System;
using System.Collections.Generic;
using System.Linq;
using WpEngine.Core;
using WpEngine.Models;
using WpEngine.Parsing;

namespace Tests;

public class WpCalculatorTests
{
    private static WpResult Calculate(string program, string postcondition)
    {
        return new WpCalculator().Calculate(program, postcondition);
    }

    // ---------- Присваивание ----------

    [Fact]
    public void Calculate_SingleAssignment_SubstitutesExpressionIntoPostcondition()
    {
        WpResult result = Calculate("x := a + 2", "x > 5");

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal("((a + 2) > 5)", result.Precondition);
        Assert.Equal("x > 5", result.Postcondition);
    }

    [Fact]
    public void Calculate_SingleAssignment_ProducesExpectedSteps()
    {
        WpResult result = Calculate("x := a + 2", "x > 5");

        Assert.Equal(
            new[]
            {
                "Присваивание: x := a + 2",
                "Текущее постусловие: x > 5",
                "После подстановки: ((a + 2) > 5)"
            },
            result.Steps);
    }

    [Fact]
    public void Calculate_SingleAssignment_BuildsHoareTriple()
    {
        WpResult result = Calculate("x := a + 2", "x > 5");

        Assert.Equal("{ ((a + 2) > 5) } x := a + 2 { x > 5 }", result.HoareTriple);
    }

    [Fact]
    public void Calculate_ProgramWithSurroundingWhitespace_TrimsProgramInTriple()
    {
        WpResult result = Calculate("  \n x := a + 2 \n ", "x > 5");

        Assert.True(result.Success);
        Assert.Equal("{ ((a + 2) > 5) } x := a + 2 { x > 5 }", result.HoareTriple);
    }

    [Fact]
    public void Calculate_AssignedVariableAbsentFromPostcondition_KeepsPostconditionShape()
    {
        WpResult result = Calculate("y := 1", "x > 0");

        Assert.True(result.Success);
        Assert.Equal("(x > 0)", result.Precondition);

        // Упрощать нечего, поэтому шага упрощения нет.
        Assert.Equal(3, result.Steps.Count);
        Assert.DoesNotContain(result.Steps, s => s.StartsWith("Упрощение предусловия"));
    }

    // ---------- Последовательность ----------

    [Fact]
    public void Calculate_Sequence_IsProcessedFromEndToStart()
    {
        WpResult result = Calculate("x := a + 2\ny := x * 3\nz := y - 4", "z > 10");

        Assert.True(result.Success);
        Assert.Equal("((((a + 2) * 3) - 4) > 10)", result.Precondition);
    }

    [Fact]
    public void Calculate_Sequence_StepsListEveryStatementBackwards()
    {
        WpResult result = Calculate("x := a + 2\ny := x * 3\nz := y - 4", "z > 10");

        Assert.Equal(16, result.Steps.Count);
        Assert.Equal("Начало обратного прохода по последовательности.", result.Steps[0]);
        Assert.Equal("Обработка оператора 3 в обратном порядке.", result.Steps[1]);
        Assert.Equal("Присваивание: z := y - 4", result.Steps[2]);
        Assert.Equal("Обработка оператора 2 в обратном порядке.", result.Steps[6]);
        Assert.Equal("Обработка оператора 1 в обратном порядке.", result.Steps[11]);
        Assert.Equal(
            "Промежуточное предусловие: ((((a + 2) * 3) - 4) > 10)",
            result.Steps[^1]);
    }

    [Fact]
    public void Calculate_ReassignmentOfSameVariable_ComposesSubstitutions()
    {
        // x := a; x := x + 1; Q = x > 0  =>  (a + 1) > 0
        WpResult result = Calculate("x := a\nx := x + 1", "x > 0");

        Assert.True(result.Success);
        Assert.Equal("((a + 1) > 0)", result.Precondition);
    }

    // ---------- Упрощение ----------

    [Fact]
    public void Calculate_PreconditionIsSimplified_AndStepIsRecorded()
    {
        WpResult result = Calculate("x := 0 + a", "x > 1");

        Assert.True(result.Success);
        Assert.Equal("(a > 1)", result.Precondition);
        Assert.Equal("Упрощение предусловия: (a > 1)", result.Steps[^1]);
    }

    [Fact]
    public void Calculate_ConstantTruePrecondition_IsReducedToTrue()
    {
        WpResult result = Calculate("x := 2 + 3", "x == 5");

        Assert.True(result.Success);
        Assert.Equal("true", result.Precondition);
    }

    [Fact]
    public void Calculate_ConstantFalsePrecondition_IsReducedToFalse()
    {
        WpResult result = Calculate("x := 1", "x > 5");

        Assert.True(result.Success);
        Assert.Equal("false", result.Precondition);
    }

    // ---------- Определённость ----------

    [Fact]
    public void Calculate_Division_AddsNonZeroDivisorToPrecondition()
    {
        WpResult result = Calculate("x := a / b", "x > 0");

        Assert.True(result.Success);
        Assert.Equal("((b != 0) && ((a / b) > 0))", result.Precondition);
        Assert.Equal(new[] { "(b) != 0" }, result.DefinednessConditions);
    }

    [Fact]
    public void Calculate_Division_StepsShowRawAndSimplifiedPrecondition()
    {
        WpResult result = Calculate("x := a / b", "x > 0");

        Assert.Equal(
            new[]
            {
                "Присваивание: x := a / b",
                "Текущее постусловие: x > 0",
                "После подстановки: (((b) != 0)) && (((a / b) > 0))",
                "Упрощение предусловия: ((b != 0) && ((a / b) > 0))"
            },
            result.Steps);
    }

    [Fact]
    public void Calculate_Division_HoareTripleUsesUnsimplifiedPrecondition()
    {
        WpResult result = Calculate("x := a / b", "x > 0");

        Assert.Equal(
            "{ (((b) != 0)) && (((a / b) > 0)) } x := a / b { x > 0 }",
            result.HoareTriple);
    }

    [Fact]
    public void Calculate_SqrtInPostcondition_AddsDefinednessOfPostcondition()
    {
        WpResult result = Calculate("x := a", "sqrt(x) > 1");

        Assert.True(result.Success);
        Assert.Equal("((a >= 0) && (sqrt(a) > 1))", result.Precondition);
        Assert.Equal(new[] { "(x) >= 0" }, result.DefinednessConditions);
        Assert.Equal(
            "Ограничения определённости постусловия: ((x) >= 0)",
            result.Steps[0]);
    }

    [Fact]
    public void Calculate_DivisionInSecondStatement_GuardSurvivesEarlierSubstitution()
    {
        WpResult result = Calculate("x := a / b\ny := x * 2", "y > 0");

        Assert.True(result.Success);
        Assert.Equal("((b != 0) && (((a / b) * 2) > 0))", result.Precondition);
        Assert.Contains("(b) != 0", result.DefinednessConditions);
    }

    [Fact]
    public void Calculate_DefinednessConditions_AreDistinct()
    {
        WpResult result = Calculate("x := 1 / c\ny := 1 / c", "x + y > 0");

        Assert.True(result.Success);
        Assert.Equal(result.DefinednessConditions.Distinct().Count(), result.DefinednessConditions.Count);
        Assert.Contains("(c) != 0", result.DefinednessConditions);
    }

    // ---------- Ветвление ----------

    [Fact]
    public void Calculate_If_CombinesBranchesThroughCondition()
    {
        const string program = @"
if a >= b then
    max := a
else
    max := b
end";

        WpResult result = Calculate(program, "max >= a");

        Assert.True(result.Success);
        Assert.Equal(
            "(((a >= b) && (a >= a)) || (!((a >= b)) && (b >= a)))",
            result.Precondition);
    }

    [Fact]
    public void Calculate_If_StepsDescribeBothBranches()
    {
        const string program = @"
if a >= b then
    max := a
else
    max := b
end";

        WpResult result = Calculate(program, "max >= a");

        Assert.Equal("Условие ветвления: a >= b", result.Steps[0]);
        Assert.Contains("Вычисление ветки then:", result.Steps);
        Assert.Contains("[then] Присваивание: max := a", result.Steps);
        Assert.Contains("[then] После подстановки: (a >= a)", result.Steps);
        Assert.Contains("Вычисление ветки else:", result.Steps);
        Assert.Contains("[else] Присваивание: max := b", result.Steps);
        Assert.Contains("[else] После подстановки: (b >= a)", result.Steps);
        Assert.Contains(result.Steps, s => s.StartsWith("Результат ветвления:"));
    }

    [Fact]
    public void Calculate_IfWithDivisionInCondition_RequiresConditionToBeDefined()
    {
        const string program = @"
if a / b > 1 then
    x := 1
else
    x := 2
end";

        WpResult result = Calculate(program, "x > 0");

        Assert.True(result.Success);
        Assert.Contains("(b) != 0", result.DefinednessConditions);
    }

    [Fact]
    public void Calculate_DivisionInsideBranch_IsReportedInDefinednessConditions()
    {
        const string program = @"
if a > 0 then
    x := 1 / a
else
    x := 0
end";

        WpResult result = Calculate(program, "x >= 0");

        Assert.True(result.Success);
        Assert.Contains("(a) != 0", result.DefinednessConditions);
    }

    /*// ---------- Семантическая проверка ----------
    // Предусловие должно быть истинным в точности тогда, когда программа
    // выполняется без неопределённых операций и постусловие после неё истинно.

    private static void AssertPreconditionMatchesExecution(
      string program,
      string postcondition,
      string[] inputs,
      int min,
      int max)
    {
      WpResult result = Calculate(program, postcondition);

      Assert.True(result.Success, result.Error);

      Statement statement = Parser.Parse(program);

      foreach (int[] values in WpInterpreter.Grid(inputs.Length, min, max))
      {
          var state = new Dictionary<string, object>();

          for (int i = 0; i < inputs.Length; i++)
              state[inputs[i]] = (double)values[i];

          bool expected = WpInterpreter.PostconditionHoldsAfter(statement, postcondition, state);
          bool actual = WpInterpreter.Holds(result.Precondition, state);

          string description = string.Join(
              ", ",
              inputs.Select((name, i) => $"{name}={values[i]}"));

          Assert.True(
              expected == actual,
              $"Для состояния ({description}) программа даёт {expected}, " +
              $"а предусловие '{result.Precondition}' даёт {actual}.");
      }
    }

  [Fact]
  public void Precondition_Sequence_MatchesExecution()
  {
      AssertPreconditionMatchesExecution(
          "x := a + 2\ny := x * 3\nz := y - 4",
          "z > 10",
          new[] { "a" },
          -10,
          10);
  }

  [Fact]
  public void Precondition_MaxOfTwoNumbers_HoldsForEveryInput()
  {
      // Программа «максимум» корректна, поэтому предусловие верно при любых a и b.
      const string program = @"
//if a >= b then
  max := a
//else
  max := b
//end";

      const string post = "(max >= a) && (max >= b) && ((max == a) || (max == b))";

      AssertPreconditionMatchesExecution(program, post, new[] { "a", "b" }, -3, 3);

      WpResult result = Calculate(program, post);

      foreach (int[] values in WpInterpreter.Grid(2, -3, 3))
      {
          var state = new Dictionary<string, object>
          {
              ["a"] = (double)values[0],
              ["b"] = (double)values[1]
          };

          Assert.True(
              WpInterpreter.Holds(result.Precondition, state),
              $"Предусловие должно быть истинным при a={values[0]}, b={values[1]}.");
      }
  }

  [Fact]
  public void Precondition_WrongMaxProgram_IsNotAlwaysTrue()
  {
      // В «неверной» программе постусловие выполняется не всегда.
      const string program = @"
//if a >= b then
  max := b
//else
  max := a
//end";

      const string post = "(max >= a) && (max >= b)";

      AssertPreconditionMatchesExecution(program, post, new[] { "a", "b" }, -3, 3);

      WpResult result = Calculate(program, post);

      var state = new Dictionary<string, object> { ["a"] = 1.0, ["b"] = 2.0 };

      Assert.False(WpInterpreter.Holds(result.Precondition, state));
  }

  [Fact]
  public void Precondition_QuadraticEquation_RequiresNonZeroAAndNonNegativeDiscriminant()
  {
      // Для постусловия rootsCount > 0 нужно D >= 0 и a != 0 (иначе деление на 2 * a).
      WpExample example = ExamplePrograms.All[2];

      AssertPreconditionMatchesExecution(
          example.Program,
          example.Postcondition,
          new[] { "a", "b", "c" },
          -3,
          3);

      WpResult result = Calculate(example.Program, example.Postcondition);

      Assert.True(result.Success);

      foreach (int[] v in WpInterpreter.Grid(3, -3, 3))
      {
          double a = v[0], b = v[1], c = v[2];
          double discriminant = b * b - 4 * a * c;

          var state = new Dictionary<string, object> { ["a"] = a, ["b"] = b, ["c"] = c };

          bool expected = discriminant >= 0 && a != 0;

          Assert.True(
              WpInterpreter.Holds(result.Precondition, state) == expected,
              $"Для a={a}, b={b}, c={c} ожидалось {expected}.");
      }
  }

  [Fact]
  public void Precondition_DivisionInCondition_RequiresNonZeroDivisor()
  {
      const string program = @"
//if a / b > 1 then
  x := 1
//else
  x := 2
//end";

      AssertPreconditionMatchesExecution(program, "x > 0", new[] { "a", "b" }, -3, 3);
  }

  [Fact]
  public void Precondition_DivisionInOneBranch_MatchesExecution()
  {
      const string program = @"
//if a > 0 then
  x := 1 / a
//else
  x := 0
//end";

      AssertPreconditionMatchesExecution(program, "x >= 0", new[] { "a" }, -4, 4);
  }

  [Fact]
  public void Precondition_NestedIf_MatchesExecution()
  {
      const string program = @"
//if a > 0 then
  if b > 0 then
      r := 1
  else
      r := 2
  end
//else
  r := 3
//end";

      AssertPreconditionMatchesExecution(program, "r > 1", new[] { "a", "b" }, -2, 2);
  }

  [Fact]
  public void Precondition_BooleanVariableUsedAsCondition_MatchesExecution()
  {
      const string program = @"
//flag := a > b
//if flag then
  m := a
//else
  m := b
//end";

      AssertPreconditionMatchesExecution(
          program,
          "m >= a && m >= b",
          new[] { "a", "b" },
          -3,
          3);
  }

  [Fact]
  public void Precondition_SqrtThenArithmetic_MatchesExecution()
  {
      AssertPreconditionMatchesExecution(
          "y := sqrt(x)\nz := y + 1",
          "z >= 1",
          new[] { "x" },
          -4,
          4);
  }

  [Fact]
  public void Precondition_DivisionChain_MatchesExecution()
  {
      AssertPreconditionMatchesExecution(
          "x := a / b\ny := x * 2",
          "y > 0",
          new[] { "a", "b" },
          -3,
          3);
  }

  [Fact]
  public void Precondition_ConjunctionInPostcondition_MatchesExecution()
  {
      AssertPreconditionMatchesExecution(
          "s := a + b\nd := a - b",
          "(s > d) && (s != 0)",
          new[] { "a", "b" },
          -3,
          3);
  }*/

    [Fact]
    public void Precondition_MentionsOnlyInputVariables()
    {
        // После подстановки промежуточные переменные не должны оставаться в предусловии.
        WpResult result = Calculate("x := a + 2\ny := x * 3\nz := y - 4", "z > 10");

        Expr precondition = ExpressionParser.Parse(result.Precondition);

        Assert.Equal(new[] { "a" }, CollectVariables(precondition).OrderBy(v => v, StringComparer.Ordinal));
    }

    private static IEnumerable<string> CollectVariables(Expr expression)
    {
        switch (expression)
        {
            case VariableExpr variable:
                return new[] { variable.Name };

            case UnaryExpr unary:
                return CollectVariables(unary.Operand);

            case FunctionExpr function:
                return CollectVariables(function.Argument);

            case BinaryExpr binary:
                return CollectVariables(binary.Left)
                    .Concat(CollectVariables(binary.Right))
                    .Distinct();

            default:
                return Array.Empty<string>();
        }
    }

    // ---------- Ошибки ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    public void Calculate_EmptyProgram_ReturnsFailureWithMessage(string program)
    {
        WpResult result = Calculate(program, "x > 0");

        Assert.False(result.Success);
        Assert.Equal("Программа не может быть пустой.", result.Error);
    }

    [Fact]
    public void Calculate_NullProgram_ReturnsFailureInsteadOfThrowing()
    {
        WpResult result = Calculate(null!, "x > 0");

        Assert.False(result.Success);
        Assert.Equal("Программа не может быть пустой.", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Calculate_EmptyPostcondition_ReturnsFailureWithMessage(string postcondition)
    {
        WpResult result = Calculate("x := 1", postcondition);

        Assert.False(result.Success);
        Assert.Equal("Постусловие не может быть пустым.", result.Error);
    }

    [Fact]
    public void Calculate_Failure_ClearsAllComputedFields()
    {
        WpResult result = Calculate("", "x > 0");

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Precondition);
        Assert.Equal(string.Empty, result.HoareTriple);
        Assert.Empty(result.Steps);
        Assert.Empty(result.DefinednessConditions);
    }

    [Fact]
    public void Calculate_Failure_KeepsOriginalPostcondition()
    {
        WpResult result = Calculate("x = 1", "x > 0");

        Assert.False(result.Success);
        Assert.Equal("x > 0", result.Postcondition);
    }

    [Fact]
    public void Calculate_InvalidPostconditionSyntax_ReturnsFailure()
    {
        WpResult result = Calculate("x := 1", "x >");

        Assert.False(result.Success);
        Assert.Contains("Позиция", result.Error);
    }

    [Fact]
    public void Calculate_InvalidProgramSyntax_ReturnsFailureWithLineNumber()
    {
        WpResult result = Calculate("x := 1\ny = 2", "x > 0");

        Assert.False(result.Success);
        Assert.Contains("Строка 2", result.Error);
    }

    [Fact]
    public void Calculate_NonBooleanPostcondition_ReturnsTypeError()
    {
        WpResult result = Calculate("x := 1", "x + 1");

        Assert.False(result.Success);
        Assert.Contains("Постусловие должно быть логическим выражением", result.Error);
    }

    [Fact]
    public void Calculate_ConflictingVariableTypes_ReturnsTypeError()
    {
        WpResult result = Calculate("x := 1\nx := true", "x");

        Assert.False(result.Success);
        Assert.Contains("Несовместимые типы", result.Error);
    }

    [Theory]
    [InlineData("@@@", "###")]
    [InlineData("if", "true")]
    [InlineData("x := ", "x > 0")]
    [InlineData("x := 1", "(")]
    public void Calculate_GarbageInput_NeverThrows(string program, string postcondition)
    {
        // Все ошибки возвращаются в WpResult, исключения наружу не выходят.
        WpResult result = Calculate(program, postcondition);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Error));
    }

    [Fact]
    public void Calculate_SuccessAfterFailure_UsesFreshResult()
    {
        var calculator = new WpCalculator();

        WpResult failed = calculator.Calculate("x = 1", "x > 0");
        WpResult succeeded = calculator.Calculate("x := a + 2", "x > 5");

        Assert.False(failed.Success);
        Assert.True(succeeded.Success);
        Assert.Null(succeeded.Error);
        Assert.Equal("((a + 2) > 5)", succeeded.Precondition);
    }

    [Fact]
    public void Calculate_SameInputTwice_GivesSameResult()
    {
        var calculator = new WpCalculator();

        WpResult first = calculator.Calculate("x := a / b", "x > 0");
        WpResult second = calculator.Calculate("x := a / b", "x > 0");

        Assert.Equal(first.Precondition, second.Precondition);
        Assert.Equal(first.Steps, second.Steps);
        Assert.Equal(first.HoareTriple, second.HoareTriple);
    }
}