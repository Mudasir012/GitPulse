using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitPulse.Services;

namespace GitPulse.ViewModels;

public partial class MergeConflictViewModel : ObservableObject
{
    private readonly GitService _git;
    private readonly Action _onResolved;

    public ObservableCollection<string> ConflictFiles { get; } = [];

    [ObservableProperty]
    private string? _selectedFile;

    [ObservableProperty]
    private bool _hasConflicts;

    public MergeConflictViewModel(GitService git, Action onResolved)
    {
        _git = git;
        _onResolved = onResolved;
    }

    public void Refresh()
    {
        ConflictFiles.Clear();
        var conflicts = _git.GetConflicts();
        foreach (var file in conflicts)
        {
            if (!string.IsNullOrEmpty(file))
                ConflictFiles.Add(file);
        }
        HasConflicts = ConflictFiles.Count > 0;
    }

    [RelayCommand]
    private void AcceptMine()
    {
        if (SelectedFile is null) return;
        _git.ResolveConflict(SelectedFile, "ours");
        Refresh();
        _onResolved();
    }

    [RelayCommand]
    private void AcceptTheirs()
    {
        if (SelectedFile is null) return;
        _git.ResolveConflict(SelectedFile, "theirs");
        Refresh();
        _onResolved();
    }

    [RelayCommand]
    private void ResolveAllMine()
    {
        foreach (var file in ConflictFiles.ToList())
        {
            _git.ResolveConflict(file, "ours");
        }
        Refresh();
        _onResolved();
    }

    [RelayCommand]
    private void ResolveAllTheirs()
    {
        foreach (var file in ConflictFiles.ToList())
        {
            _git.ResolveConflict(file, "theirs");
        }
        Refresh();
        _onResolved();
    }
}
