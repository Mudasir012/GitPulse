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
        var raw = _git.GetDiff(file.FilePath);
        ParseDiff(raw);
        HasDiff = DiffLines.Count > 0;
    }

    private void ParseDiff(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;
        var lines = raw.Split('\n');
        var lineNum = 0;
        foreach (var line in lines)
        {
            var type = line switch
            {
                ['+', ..] => DiffLineType.Addition,
                ['-', ..] => DiffLineType.Deletion,
                ['@', ..] => DiffLineType.Header,
                _ => DiffLineType.Context
            };
            DiffLines.Add(new DiffLine
            {
                Text = line,
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
