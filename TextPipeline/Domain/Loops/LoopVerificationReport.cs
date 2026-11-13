namespace WpEngine.Loops;

public sealed class LoopVerificationReport
{
    public string PresetName { get; init; } = "";

    // Результаты проверки конкретного исполнения.
    public IReadOnlyList<LoopCondition> Conditions { get; init; }
        = Array.Empty<LoopCondition>();

    // Результаты символического анализа.
    public IReadOnlyList<LoopCondition> SymbolicConditions { get; init; }
        = Array.Empty<LoopCondition>();

    public bool RuntimePassed =>
        Conditions.Count == 4 &&
        Conditions.All(c =>
            c.Status == LoopConditionStatus.Passed);

    public string? CalculatedWp =>
        SymbolicConditions
            .FirstOrDefault(c => c.CalculatedWp != null)
            ?.CalculatedWp;
}