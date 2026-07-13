using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitPulse.Models;
using GitPulse.Services;

namespace GitPulse.ViewModels;

public partial class CommitGraphViewModel : ObservableObject
{
    private readonly GitService _git;
    private readonly CommitGraphCalculator _calculator = new();

    public ObservableCollection<CommitGraphRow> Commits { get; } = [];

    [ObservableProperty]
    private string _searchText = "";

    public CommitGraphViewModel(GitService git)
    {
        _git = git;
    }

    public void Refresh()
    {
        try
        {
            var commits = _git.GetCommits(100);
            var branches = _git.GetBranches();
            var rows = _calculator.Calculate(commits, branches);

            Commits.Clear();
            foreach (var row in rows)
            {
                Commits.Add(row);
            }
        }
        catch
        {
            Commits.Clear();
        }
    }

    [RelayCommand]
    private void Search()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Refresh();
                return;
            }

            var results = _git.SearchCommits(SearchText);
            var branches = _git.GetBranches();
            var rows = _calculator.Calculate(results, branches);

            Commits.Clear();
            foreach (var row in rows)
            {
                Commits.Add(row);
            }
        }
        catch
        {
            Commits.Clear();
        }
    }
}
