namespace GitPulse.Models;

public class CommitInfo
{
    public string Sha { get; init; } = "";
    public string Author { get; init; } = "";
    public string Message { get; init; } = "";
    public DateTimeOffset AuthorTime { get; init; }
    public List<string> ParentShas { get; init; } = [];
    public List<string> BranchNames { get; init; } = [];

    public string ShortSha => Sha.Length >= 7 ? Sha[..7] : Sha;
    public string TimeAgo
    {
        get
        {
            var span = DateTimeOffset.Now - AuthorTime;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
            return AuthorTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
