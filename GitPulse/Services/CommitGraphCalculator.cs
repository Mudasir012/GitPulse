using GitPulse.Models;

namespace GitPulse.Services;

public class CommitGraphCalculator
{
    private static readonly string[] BranchColors =
    [
        "#58A6FF", "#3FB950", "#D29922", "#F85149",
        "#BC8CFF", "#39C5CF", "#F0883E", "#F778BA"
    ];

    private const string DimLaneColor = "#3F3F4A";

    public List<CommitGraphRow> Calculate(List<CommitInfo> commits, List<BranchInfo> branches)
    {
        if (commits.Count == 0) return [];

        var rows = new List<CommitGraphRow>();
        var branchLanes = new Dictionary<string, int>();
        var nextLane = 0;

        var branchTipToName = new Dictionary<string, string>();
        foreach (var b in branches)
        {
            if (!b.IsRemote && !string.IsNullOrEmpty(b.TipSha))
                branchTipToName[b.TipSha] = b.Name;
        }

        var activeLanes = new Dictionary<int, string>(); // lane -> sha of commit above
        var parentLaneReservations = new Dictionary<string, int>(); // sha -> reserved lane

        foreach (var commit in commits)
        {
            var lane = -1;
            string? branchLabel = null;
            string branchColor = BranchColors[0];

            if (branchTipToName.TryGetValue(commit.Sha, out var branchName))
            {
                branchLabel = branchName;
                if (!branchLanes.ContainsKey(branchName))
                {
                    branchLanes[branchName] = nextLane++;
                }
                lane = branchLanes[branchName];
                var colorIndex = branchLanes[branchName] % BranchColors.Length;
                branchColor = BranchColors[colorIndex];
            }

            if (lane < 0)
            {
                if (parentLaneReservations.TryGetValue(commit.Sha, out var reservedLane))
                {
                    lane = reservedLane;
                }
                else
                {
                    lane = 0;
                }
            }

            var totalLanes = Math.Max(nextLane, activeLanes.Count > 0 ? activeLanes.Keys.Max() + 1 : 0);
            totalLanes = Math.Max(totalLanes, lane + 1);

            var prefixChars = new char[totalLanes * 2];
            var prefixColors = new string[totalLanes * 2];
            for (var i = 0; i < prefixChars.Length; i++)
            {
                prefixChars[i] = ' ';
                prefixColors[i] = DimLaneColor;
            }

            var laneColors = new Dictionary<int, string>();
            foreach (var kv in activeLanes)
            {
                laneColors[kv.Key] = BranchColors[kv.Key % BranchColors.Length];
            }
            laneColors[lane] = branchColor;

            foreach (var kv in activeLanes)
            {
                var li = kv.Key * 2;
                prefixChars[li] = ' ';
                prefixChars[li + 1] = ' ';
            }

            foreach (var edge in commit.ParentShas)
            {
                if (parentLaneReservations.TryGetValue(edge, out var parentLane))
                {
                    if (parentLane != lane)
                    {
                        var minLane = Math.Min(lane, parentLane);
                        var maxLane = Math.Max(lane, parentLane);
                        for (var li = minLane * 2; li <= maxLane * 2; li++)
                        {
                            if (prefixChars[li] == ' ')
                            {
                                prefixChars[li] = '\u2500';
                                prefixColors[li] = DimLaneColor;
                            }
                        }
                    }
                }
            }

            foreach (var kv in activeLanes)
            {
                var li = kv.Key * 2;
                prefixChars[li] = '\u2502';
                prefixColors[li] = laneColors[kv.Key];
                prefixChars[li + 1] = ' ';
            }

            if (lane * 2 < prefixChars.Length)
            {
                prefixChars[lane * 2] = '\u25CF';
                prefixColors[lane * 2] = branchColor;
                if (lane * 2 + 1 < prefixChars.Length)
                {
                    prefixChars[lane * 2 + 1] = ' ';
                }
            }

            var newActive = new Dictionary<int, string>();
            foreach (var parentSha in commit.ParentShas)
            {
                if (!parentLaneReservations.ContainsKey(parentSha))
                {
                    parentLaneReservations[parentSha] = lane;
                }
                newActive[parentLaneReservations[parentSha]] = parentSha;
            }

            foreach (var kv in activeLanes)
            {
                if (!newActive.ContainsKey(kv.Key))
                {
                    // Check if any future commit references this lane
                    if (!parentLaneReservations.ContainsValue(kv.Key))
                    {
                        // Lane is done
                    }
                }
            }

            activeLanes = newActive;

            var segments = new List<GraphSegment>();
            var currentText = "";
            var currentColor = "";
            for (var i = 0; i < prefixChars.Length; i++)
            {
                if (i == 0)
                {
                    currentText = prefixChars[i].ToString();
                    currentColor = prefixColors[i];
                    continue;
                }
                if (prefixColors[i] == currentColor)
                {
                    currentText += prefixChars[i];
                }
                else
                {
                    segments.Add(new GraphSegment
                    {
                        Text = currentText,
                        Color = currentColor,
                        IsCommitDot = currentText.Contains('\u25CF')
                    });
                    currentText = prefixChars[i].ToString();
                    currentColor = prefixColors[i];
                }
            }
            segments.Add(new GraphSegment
            {
                Text = currentText,
                Color = currentColor,
                IsCommitDot = currentText.Contains('\u25CF')
            });

            rows.Add(new CommitGraphRow
            {
                Commit = commit,
                Lane = lane,
                TotalLanes = totalLanes,
                Edges = commit.ParentShas.Select(ps => new GraphEdge
                {
                    FromLane = lane,
                    ToLane = parentLaneReservations.GetValueOrDefault(ps, lane)
                }).ToList(),
                BranchLabel = branchLabel,
                BranchColor = branchColor,
                Segments = segments
            });
        }

        return rows;
    }
}
