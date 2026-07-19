using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitPulse.Models;
using GitPulse.Services;

namespace GitPulse.ViewModels;

public partial class WorkingDirectoryViewModel : ObservableObject
{
    private readonly GitService _git;

    public ObservableCollection<FileChange> Files { get; } = [];

    [ObservableProperty]
    private FileChange? _selectedFile;

    [ObservableProperty]
    private bool _hasFiles;

    public WorkingDirectoryViewModel(GitService git)
    {
        _git = git;
    }

    /// <summary>
    /// Syncs the file list in place instead of clearing and rebuilding it,
    /// so the current selection (and the diff shown for it) survives refreshes
    /// triggered by staging/unstaging.
    /// </summary>
    public bool HasUnstagedFiles => Files.Any(f => !f.IsStaged);

    public void Refresh()
    {
        var fresh = _git.GetStatus();

        // Remove entries that no longer exist.
        for (var i = Files.Count - 1; i >= 0; i--)
        {
            if (!fresh.Any(f => f.FilePath == Files[i].FilePath && f.IsStaged == Files[i].IsStaged))
            {
                Files.RemoveAt(i);
            }
        }

        // Add new entries, update existing ones in place.
        foreach (var file in fresh)
        {
            var existing = Files.FirstOrDefault(
                f => f.FilePath == file.FilePath && f.IsStaged == file.IsStaged);
            if (existing is null)
            {
                Files.Add(file);
            }
            else
            {
                existing.Status = file.Status;
            }
        }

        HasFiles = Files.Count > 0;
        OnPropertyChanged(nameof(HasUnstagedFiles));
    }

    [RelayCommand]
    private void ToggleStage(FileChange? file)
    {
        if (file is null) return;
        if (file.IsStaged)
            _git.Unstage(file.FilePath);
        else
            _git.Stage(file.FilePath);
        Refresh();
    }

    [RelayCommand]
    private void StageAll()
    {
        foreach (var file in Files)
        {
            if (!file.IsStaged)
                _git.Stage(file.FilePath);
        }
        Refresh();
    }
}
