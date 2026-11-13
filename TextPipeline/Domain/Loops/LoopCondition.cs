namespace WpEngine.Loops;

public enum LoopConditionStatus
{
    Passed,
    Failed,
    NotProven,
    AnalysisError
}

public sealed class LoopCondition
{
    public string Name { get; init; } = "";

    public string Formula { get; init; } = "";

    public LoopConditionStatus Status { get; init; }

    public string Explanation { get; init; } = "";

    public string? CalculatedWp { get; init; }
}