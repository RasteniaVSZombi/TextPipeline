
using System;
using System.Collections.Generic;
using System.Linq;
using WpEngine.Models;
using WpEngine.Parsing;

namespace WpEngine.Core;

public sealed class WpEngineService
{
    private readonly WpCalculator _calculator = new();
    private readonly ProgramAnalyzer _analyzer = new();

    /// <summary>
    /// Вычисляет слабейшее предусловие программы.
    /// </summary>
    public WpResult Calculate(string program, string postcondition)
    {
        return _calculator.Calculate(program, postcondition);
    }

    /// <summary>
    /// Разбирает программу и возвращает её структуру.
    /// </summary>
    public ProgramAnalysis AnalyzeProgram(string program)
    {
        Statement statement = Parser.Parse(program);
        return _analyzer.Analyze(statement);
    }

    /// <summary>
    /// Выполняет вычисление предусловия и собирает сведения о программе.
    /// </summary>
    public WpExecutionResult Execute(
        string program,
        string postcondition)
    {
        WpResult calculation =
            _calculator.Calculate(program, postcondition);

        var result = new WpExecutionResult
        {
            Calculation = calculation
        };

        if (!calculation.Success)
            return result;

        try
        {
            ProgramAnalysis analysis = AnalyzeProgram(program);

            result.StatementCount = analysis.StatementCount;
            result.BranchCount = analysis.BranchCount;
            result.Variables = analysis.Variables
                .OrderBy(variable => variable)
                .ToList();
            result.Assignments = analysis.Assignments.ToList();
            result.Conditions = analysis.Conditions.ToList();
        }
        catch (Exception ex)
        {
            calculation.Success = false;
            calculation.Error =
                $"Ошибка анализа программы: {ex.Message}";
            calculation.Precondition = string.Empty;
            calculation.HoareTriple = string.Empty;
        }

        return result;
    }

    /// <summary>
    /// Возвращает готовые примеры программ.
    /// </summary>
    public IReadOnlyList<WpExample> GetExamples()
    {
        return ExamplePrograms.All;
    }

    /// <summary>
    /// Запускает готовый пример по его индексу.
    /// Индексация начинается с нуля.
    /// </summary>
    public WpExecutionResult ExecuteExample(int index)
    {
        IReadOnlyList<WpExample> examples = ExamplePrograms.All;

        if (index < 0 || index >= examples.Count)
        {
            return new WpExecutionResult
            {
                Calculation = new WpResult
                {
                    Success = false,
                    Error = $"Пример с индексом {index} не существует."
                }
            };
        }

        WpExample example = examples[index];

        return Execute(example.Program, example.Postcondition);
    }
}
