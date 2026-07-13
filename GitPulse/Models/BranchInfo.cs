namespace GitPulse.Models;

public class BranchInfo
{
    public string Name { get; init; } = "";
    public bool IsCurrent { get; init; }
    public bool IsRemote { get; init; }
    public string TipSha { get; init; } = "";
}
