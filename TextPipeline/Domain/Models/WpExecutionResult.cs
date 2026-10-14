
namespace WpEngine.Models;

public sealed class WpExecutionResult
{
    public WpResult Calculation { get; set; } = new();

    public int StatementCount { get; set; }

    public int BranchCount { get; set; }

    public List<string> Variables { get; set; } = new();

    public List<string> Assignments { get; set; } = new();

    public List<string> Conditions { get; set; } = new();
}
