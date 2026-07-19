using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitPulse.Services;

namespace GitPulse.ViewModels;

public partial class CommitBoxViewModel : ObservableObject
{
    private readonly GitService _git;

    [ObservableProperty]
    private string _commitMessage = "";

    [ObservableProperty]
    private bool _canCommit;

    public CommitBoxViewModel(GitService git)
    {
        _git = git;
    }

    partial void OnCommitMessageChanged(string value)
    {
        CanCommit = !string.IsNullOrWhiteSpace(value) && _git.IsRepoOpen;
    }

    [RelayCommand]
    private void Commit()
    {
        if (string.IsNullOrWhiteSpace(CommitMessage)) return;
        if (!_git.HasUserConfig())
        {
            var dialog = new GitPulse.Views.GitConfigDialog
            {
                Owner = Application.Current.MainWindow
            };
            if (dialog.ShowDialog() != true) return;
            _git.SetLocalUserConfig(dialog.UserName, dialog.UserEmail);
        }
        var (success, error) = _git.Commit(CommitMessage);
        if (success)
        {
            CommitMessage = "";
            MessageBox.Show("Commit successful!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show($"Commit failed.\n{error}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Clear()
    {
        CommitMessage = "";
    }
}
