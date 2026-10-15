namespace WpEngine.Models;

public sealed record WpExample(
    string Name,
    string Program,
    string Postcondition,
    string PostconditionDescription = "");