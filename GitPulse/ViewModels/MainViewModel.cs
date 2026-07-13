using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitPulse.Services;
using GitPulse.Views;
using Microsoft.Win32;

namespace GitPulse.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly GitService _git;

    public BranchesViewModel Branches { get; }
    public WorkingDirectoryViewModel WorkingDirectory { get; }
    public DiffViewModel Diff { get; }
    public CommitBoxViewModel CommitBox { get; }
    public CommitGraphViewModel CommitGraph { get; }
    public MergeConflictViewModel MergeConflicts { get; }

    [ObservableProperty]
    private string _repoPath = "";

    [ObservableProperty]
    private string _currentBranch = "";

    [ObservableProperty]
    private bool _isRepoOpen;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _progressText = "";

    [ObservableProperty]
    private string _activeTab = "changes";

    public MainViewModel()
    {
        _git = new GitService();
        Branches = new BranchesViewModel(_git);
        WorkingDirectory = new WorkingDirectoryViewModel(_git);
        Diff = new DiffViewModel(_git);
        CommitBox = new CommitBoxViewModel(_git);
        CommitGraph = new CommitGraphViewModel(_git);
        MergeConflicts = new MergeConflictViewModel(_git, OnConflictsResolved);

        WorkingDirectory.PropertyChanged += OnFileSelected;
        Branches.PropertyChanged += OnBranchChanged;
    }

    private void OnFileSelected(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorkingDirectoryViewModel.SelectedFile))
        {
            Diff.ShowDiff(WorkingDirectory.SelectedFile);
        }
    }

    private void OnBranchChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BranchesViewModel.CurrentBranch))
        {
            RefreshAll();
        }
    }

    private void OnConflictsResolved()
    {
        RefreshAll();
    }

    [RelayCommand]
    private void SwitchToChanges()
    {
        ActiveTab = "changes";
    }

    [RelayCommand]
    private void SwitchToHistory()
    {
        ActiveTab = "history";
        CommitGraph.Refresh();
    }

    [RelayCommand]
    private void OpenRepo()
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                _git.OpenRepo(dialog.FolderName);
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open repository: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task CloneRepo()
    {
        var dialog = new CloneDialog();
        dialog.Owner = Application.Current.MainWindow;
        if (dialog.ShowDialog() != true) return;

        var url = dialog.CloneUrl;
        var path = dialog.ClonePath;
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(path)) return;

        IsBusy = true;
        ProgressText = "Cloning repository...";
        try
        {
            await _git.CloneAsync(url, path);
            RefreshAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Clone failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            ProgressText = "";
        }
    }

    [RelayCommand]
    private async Task Pull()
    {
        if (!_git.IsRepoOpen) return;
        IsBusy = true;
        ProgressText = "Pulling...";
        try
        {
            await _git.PullAsync();
            RefreshAll();
            CheckConflicts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Pull failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            ProgressText = "";
        }
    }

    [RelayCommand]
    private async Task Push()
    {
        if (!_git.IsRepoOpen) return;
        IsBusy = true;
        ProgressText = "Pushing...";
        try
        {
            await _git.PushAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Push failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            ProgressText = "";
        }
    }

    [RelayCommand]
    private async Task Fetch()
    {
        if (!_git.IsRepoOpen) return;
        IsBusy = true;
        ProgressText = "Fetching...";
        try
        {
            await _git.FetchAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fetch failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            ProgressText = "";
        }
    }

    private void CheckConflicts()
    {
        MergeConflicts.Refresh();
        if (MergeConflicts.HasConflicts)
        {
            var dialog = new MergeConflictView
            {
                DataContext = MergeConflicts,
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
        }
    }

    private void RefreshAll()
    {
        try
        {
            var state = _git.GetState();
            RepoPath = state.RepoPath;
            CurrentBranch = state.CurrentBranch;
            IsRepoOpen = state.IsOpen;
        }
        catch { }

        try { Branches.Refresh(); } catch { }
        try { WorkingDirectory.Refresh(); } catch { }
        try { Diff.Clear(); } catch { }
        try { CommitBox.Clear(); } catch { }
        try { CommitGraph.Refresh(); } catch { }
    }
}
