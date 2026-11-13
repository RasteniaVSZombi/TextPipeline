namespace WpEngine.Loops;

public sealed class LoopWorkflowService
{
    private readonly LoopCorrectnessChecker _checker = new();

    // Получение всех доступных пресетов.
    public IReadOnlyList<LoopPreset> GetPresets()
    {
        return LoopPresetProvider.GetAll();
    }

    // Получение пресета по названию.
    public LoopPreset GetPreset(string name)
    {
        return LoopPresetProvider.GetByName(name);
    }

    // Получение стратегии алгоритма.
    public LoopStrategy GetStrategy(string presetName)
    {
        return GetPreset(presetName).Strategy;
    }

    // Создание нового состояния выполнения.
    public LoopExecutionState CreateExecution(string presetName)
    {
        return GetPreset(presetName).CreateInitialState();
    }

    // Выполнение одной итерации.
    public bool ExecuteStep(LoopExecutionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state.ExecuteOneStep();
    }

    // Выполнение всех оставшихся итераций.
    public void ExecuteAll(LoopExecutionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.ExecuteAll();
    }

    // Получение текущего состояния выполнения.
    public (int Index, int Result, int Remaining, bool IsCompleted)
        GetExecutionProgress(LoopExecutionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return (
            state.J,
            state.Res,
            state.Remaining,
            state.IsCompleted
        );
    }

    // Проверка одного пресета.
    public LoopVerificationReport Verify(string presetName)
    {
        return _checker.Verify(GetPreset(presetName));
    }

    // Проверка переданного пресета.
    public LoopVerificationReport Verify(LoopPreset preset)
    {
        return _checker.Verify(preset);
    }

    // Проверка всех доступных пресетов.
    public IReadOnlyList<LoopVerificationReport> VerifyAll()
    {
        return GetPresets()
            .Select(preset => _checker.Verify(preset))
            .ToList();
    }
}