namespace GitPulse.Models;

public class GraphEdge
{
    public int FromLane { get; init; }
    public int ToLane { get; init; }
}

public class CommitGraphRow
{
    public CommitInfo Commit { get; init; } = null!;
    public int Lane { get; init; }
    public int TotalLanes { get; init; }
    public List<GraphEdge> Edges { get; init; } = [];
    public string? BranchLabel { get; init; }
    public string BranchColor { get; init; } = "#89B4FA";
    public List<GraphSegment> Segments { get; init; } = [];

    public override string ToString()
    {
        var segText = string.Concat(Segments.Select(s => s.Text));
        return $"{segText} {Commit.ShortSha} {Commit.Message}";
    }
}
