namespace GitPulse.Models;

public class RepositoryState
{
    public bool IsOpen { get; set; }
    public string CurrentBranch { get; set; } = "";
    public string RepoPath { get; set; } = "";
}
