namespace WpEngine.Loops;

public sealed class LoopStrategy
{
    public LoopMode Mode { get; init; }

    public string Name { get; init; } = "";

    public string Pre { get; init; } = "";

    public string Invariant { get; init; } = "";

    public string Post { get; init; } = "";

    public string Variant { get; init; } = "";

    public string Guard { get; init; } = "";

    public string Body { get; init; } = "";

    public string PreExplanation { get; init; } = "";

    public string InvariantExplanation { get; init; } = "";

    public string PostExplanation { get; init; } = "";

    public string VariantExplanation { get; init; } = "";

    public string BodyExplanation { get; init; } = "";
}