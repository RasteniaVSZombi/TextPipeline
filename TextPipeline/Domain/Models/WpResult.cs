
namespace WpEngine.Models;

public sealed class WpResult
{
    public string Precondition { get; set; } = "";
    public string Postcondition { get; set; } = "";

    public List<string> Steps { get; set; } = new();
    public List<string> DefinednessConditions { get; set; } = new();

    public string HoareTriple { get; set; } = "";

    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}
