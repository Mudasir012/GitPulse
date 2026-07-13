using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitPulse.Models;
using GitPulse.Services;
using GitPulse.Views;

namespace GitPulse.ViewModels;

public partial class BranchesViewModel : ObservableObject
{
    private readonly GitService _git;

    public ObservableCollection<BranchInfo> Branches { get; } = [];

    [ObservableProperty]
    private BranchInfo? _selectedBranch;

    [ObservableProperty]
    private string _currentBranch = "";

    public BranchesViewModel(GitService git)
    {
        _git = git;
    }

    public void Refresh()
    {
        Branches.Clear();
        var branches = _git.GetBranches();
        foreach (var branch in branches)
        {
            Branches.Add(branch);
            if (branch.IsCurrent)
                CurrentBranch = branch.Name;
        }
    }

    [RelayCommand]
    private void SwitchBranch()
    {
        if (SelectedBranch is null) return;
        if (SelectedBranch.IsCurrent) return;
        try
        {
            _git.SwitchBranch(SelectedBranch.Name);
            Refresh();
            CurrentBranch = SelectedBranch.Name;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not switch branch: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CreateBranch()
    {
        var dialog = new InputDialog("Create Branch", "Branch name:");
        dialog.Owner = Application.Current.MainWindow;
        if (dialog.ShowDialog() != true) return;
        var name = dialog.Value;
        if (string.IsNullOrWhiteSpace(name)) return;
        try
        {
            _git.CreateBranch(name);
            Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not create branch: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteBranch()
    {
        if (SelectedBranch is null || SelectedBranch.IsCurrent) return;
        var result = MessageBox.Show($"Delete branch '{SelectedBranch.Name}'?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            _git.DeleteBranch(SelectedBranch.Name);
            Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not delete branch: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void MergeBranch()
    {
        if (SelectedBranch is null || SelectedBranch.IsCurrent) return;
        try
        {
            var status = _git.MergeBranch(SelectedBranch.Name);
            MessageBox.Show($"Merge result: {status}", "Merge",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not merge branch: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
