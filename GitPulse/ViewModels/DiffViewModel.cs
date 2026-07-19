using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GitPulse.Models;
using GitPulse.Services;

namespace GitPulse.ViewModels;

public partial class DiffViewModel : ObservableObject
{
    private readonly GitService _git;

    public ObservableCollection<DiffLine> DiffLines { get; } = [];

    [ObservableProperty]
    private string _fileName = "";

    [ObservableProperty]
    private bool _hasDiff;

    public DiffViewModel(GitService git)
    {
        _git = git;
    }

    public void ShowDiff(FileChange? file)
    {
        DiffLines.Clear();
        if (file is null)
        {
            FileName = "";
            HasDiff = false;
            return;
        }
        FileName = file.FilePath;
        var raw = _git.GetDiff(file.FilePath, file.Status == FileStatus.Untracked);
        ParseDiff(raw);
        HasDiff = DiffLines.Count > 0;
    }

    /// <summary>
    /// Classifies raw unified-diff lines. Only +/- lines inside a hunk (after the
    /// first @@ header) are additions/deletions - the "--- a/x" and "+++ b/x" file
    /// headers and other preamble lines are metadata and must not be painted red/green.
    /// </summary>
    private void ParseDiff(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;
        var lines = raw.Split('\n');
        var lineNum = 0;
        var inHunk = false;
        foreach (var line in lines)
        {
            DiffLineType type;
            if (line.StartsWith("@@", StringComparison.Ordinal))
            {
                type = DiffLineType.Header;
                inHunk = true;
            }
            else if (!inHunk)
            {
                type = DiffLineType.Meta;
            }
            else
            {
                type = line switch
                {
                    ['+', ..] => DiffLineType.Addition,
                    ['-', ..] => DiffLineType.Deletion,
                    ['\\', ..] => DiffLineType.Meta,
                    _ => DiffLineType.Context
                };
            }
            DiffLines.Add(new DiffLine
            {
                Text = line.TrimEnd('\r'),
                Type = type,
                LineNumber = lineNum++
            });
        }
    }

    public void Clear()
    {
        DiffLines.Clear();
        FileName = "";
        HasDiff = false;
    }
}
