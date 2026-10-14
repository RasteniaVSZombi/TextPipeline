
namespace WpEngine.Models;

public abstract record Statement;

public sealed record Assignment(
    string Variable,
    string Expression
) : Statement;

public sealed record Sequence(
    IReadOnlyList<Statement> Statements
) : Statement;

public sealed record IfStatement(
    string Condition,
    Statement ThenBranch,
    Statement ElseBranch
) : Statement;
