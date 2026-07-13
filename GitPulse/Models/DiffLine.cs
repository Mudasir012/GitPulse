namespace GitPulse.Models;

public enum DiffLineType
{
    Context,
    Addition,
    Deletion,
    Header
}

public class DiffLine
{
    public string Text { get; init; } = "";
    public DiffLineType Type { get; init; }
    public int LineNumber { get; init; }
}
