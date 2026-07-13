namespace GitPulse.Models;

public enum FileStatus
{
    Untracked,
    Modified,
    Added,
    Deleted,
    Staged
}

public class FileChange
{
    public string FilePath { get; init; } = "";
    public FileStatus Status { get; init; }
    public bool IsStaged { get; set; }
}
