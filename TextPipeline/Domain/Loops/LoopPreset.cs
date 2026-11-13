namespace WpEngine.Loops;

public sealed class LoopPreset
{
    public string Name { get; init; } = "";

    public LoopStrategy Strategy { get; init; } = null!;

    public int[] Array { get; init; } = System.Array.Empty<int>();

    public int Threshold { get; init; }

    public int ExpectedResult { get; init; }

    public LoopExecutionState CreateInitialState()
    {
        int initialIndex =
            Strategy.Mode == LoopMode.PrefixMax ? 1 : 0;

        int initialResult = Strategy.Mode switch
        {
            LoopMode.PrefixMax when Array.Length > 0 => Array[0],

            LoopMode.PrefixMax => throw new InvalidOperationException(
                "Для поиска максимума массив не может быть пустым."),

            _ => 0
        };

        return new LoopExecutionState(
            Array,
            Strategy.Mode,
            Threshold,
            initialIndex,
            initialResult);
    }
}