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

    public WorkingDirectoryViewModel(GitService git)
    {
        _git = git;
    }

    public void Refresh()
    {
        Files.Clear();
        var files = _git.GetStatus();
        foreach (var file in files)
        {
            Files.Add(file);
        }
    }

    [RelayCommand]
    private void Stage(FileChange? file)
    {
        if (file is null) return;
        _git.Stage(file.FilePath);
        Refresh();
    }

    [RelayCommand]
    private void Unstage(FileChange? file)
    {
        if (file is null) return;
        _git.Unstage(file.FilePath);
        Refresh();
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
}
