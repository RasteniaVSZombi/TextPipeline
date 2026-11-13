using WpEngine.Core;
using WpEngine.Parsing;

namespace WpEngine.Loops;

public sealed class LoopCorrectnessChecker
{
    private readonly WpCalculator _calculator = new();

    public LoopVerificationReport Verify(LoopPreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);

        // Проверка конкретного исполнения.
        var runtimeConditions =
            VerifyConcreteExecution(preset).ToList();

        // Символический анализ через WP-калькулятор.
        var symbolicConditions =
            VerifySymbolically(preset.Strategy);

        // Добавляем рассчитанное WP к условию сохранения инварианта.
        var symbolicPreservation = symbolicConditions
            .FirstOrDefault(c => c.Name.StartsWith("2."));

        if (symbolicPreservation != null &&
            runtimeConditions.Count > 1)
        {
            var runtimePreservation = runtimeConditions[1];

            runtimeConditions[1] = new LoopCondition
            {
                Name = runtimePreservation.Name,
                Formula = runtimePreservation.Formula,
                Status = runtimePreservation.Status,
                CalculatedWp = symbolicPreservation.CalculatedWp,
                Explanation =
                    runtimePreservation.Explanation +
                    " " +
                    symbolicPreservation.Explanation
            };
        }

        return new LoopVerificationReport
        {
            PresetName = preset.Name,
            Conditions = runtimeConditions,
            SymbolicConditions = symbolicConditions
        };
    }

    public IReadOnlyList<LoopCondition> VerifyConcreteExecution(
        LoopPreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);

        var state = preset.CreateInitialState();

        bool initialization = CheckInvariant(
            preset, state.J, state.Res);

        bool preservation = initialization;

        bool variantNonNegative = state.Remaining >= 0;
        bool variantDecreases = true;

        while (!state.IsCompleted)
        {
            int oldVariant = state.Remaining;

            state.ExecuteOneStep();

            int newVariant = state.Remaining;

            preservation &= CheckInvariant(
                preset, state.J, state.Res);

            variantNonNegative &= newVariant >= 0;
            variantDecreases &= newVariant < oldVariant;
        }

        bool postcondition =
            state.Res == preset.ExpectedResult;

        bool variantValid =
            variantNonNegative && variantDecreases;

        return new[]
        {
            CreateCondition(
                "1. Инициализация инварианта",
                $"Pre => Inv: {preset.Strategy.Pre}",
                initialization,
                initialization
                    ? "Начальное состояние удовлетворяет инварианту."
                    : "Начальное состояние нарушает инвариант."),

            CreateCondition(
                "2. Сохранение инварианта",
                $"Inv && ({preset.Strategy.Guard}) => Inv после тела",
                preservation,
                preservation
                    ? "Инвариант сохраняется после каждой итерации."
                    : "Инвариант нарушен во время исполнения."),

            CreateCondition(
                "3. Достижение постусловия",
                $"Inv && !({preset.Strategy.Guard}) => Post",
                postcondition,
                postcondition
                    ? $"Получен ожидаемый результат: {state.Res}."
                    : $"Ожидалось {preset.ExpectedResult}, получено {state.Res}."),

            CreateCondition(
                "4. Убывание варианта",
                $"t = {preset.Strategy.Variant}",
                variantValid,
                variantValid
                    ? "Вариант неотрицателен и строго уменьшается."
                    : "Нарушено условие убывания варианта.")
        };
    }

    public IReadOnlyList<LoopCondition> VerifySymbolically(
        LoopStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        WpEngine.Models.WpResult? wpResult = null;
        string? error = null;

        try
        {
            var parsedBody = Parser.Parse(strategy.Body);

            if (parsedBody == null)
            {
                throw new InvalidOperationException(
                    "Парсер не смог разобрать тело цикла.");
            }

            wpResult = _calculator.Calculate(
                strategy.Body,
                strategy.Invariant);

            if (!wpResult.Success)
                error = wpResult.Error;
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        bool calculationSucceeded =
            wpResult?.Success == true && error == null;

        return new[]
        {
            new LoopCondition
            {
                Name = "1. Инициализация инварианта",
                Formula =
                    $"{strategy.Pre} => {strategy.Invariant}",
                Status = LoopConditionStatus.NotProven,
                Explanation =
                    "Необходимо доказать, что предусловие обеспечивает " +
                    "выполнение инварианта перед циклом."
            },

            new LoopCondition
            {
                Name = "2. Сохранение инварианта",
                Formula =
                    $"{strategy.Invariant} && ({strategy.Guard}) => wp(Body, Inv)",
                Status = calculationSucceeded
                    ? LoopConditionStatus.NotProven
                    : LoopConditionStatus.AnalysisError,
                CalculatedWp = calculationSucceeded
                    ? wpResult!.Precondition
                    : null,
                Explanation = calculationSucceeded
                    ? "Тело цикла успешно разобрано. WP-калькулятор вычислил " +
                      "предусловие. Логическое следствие ещё требует доказательства."
                    : $"Ошибка анализа тела цикла: {error}"
            },

            new LoopCondition
            {
                Name = "3. Достижение постусловия",
                Formula =
                    $"{strategy.Invariant} && !({strategy.Guard}) => {strategy.Post}",
                Status = LoopConditionStatus.NotProven,
                Explanation =
                    "Необходимо доказать, что инвариант при выходе " +
                    "из цикла обеспечивает постусловие."
            },

            new LoopCondition
            {
                Name = "4. Убывание варианта",
                Formula =
                    $"{strategy.Variant} >= 0 && t_after < t_before",
                Status = LoopConditionStatus.NotProven,
                Explanation =
                    "Необходимо доказать неотрицательность варианта " +
                    "и его строгое уменьшение на каждой итерации."
            }
        };
    }

    private static bool CheckInvariant(
        LoopPreset preset,
        int j,
        int result)
    {
        int[] values = preset.Array;

        if (j < 0 || j > values.Length)
            return false;

        return preset.Strategy.Mode switch
        {
            LoopMode.PrefixSum =>
                result == values.Take(j).Sum(),

            LoopMode.CountGreaterThanThreshold =>
                result == values.Take(j)
                    .Count(value => value > preset.Threshold),

            LoopMode.PrefixMax =>
                j >= 1 &&
                result == values.Take(j).Max(),

            _ => false
        };
    }

    private static LoopCondition CreateCondition(
        string name,
        string formula,
        bool passed,
        string explanation)
    {
        return new LoopCondition
        {
            Name = name,
            Formula = formula,
            Status = passed
                ? LoopConditionStatus.Passed
                : LoopConditionStatus.Failed,
            Explanation = explanation
        };
    }
}