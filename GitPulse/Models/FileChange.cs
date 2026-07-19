using CommunityToolkit.Mvvm.ComponentModel;

namespace GitPulse.Models;

public enum FileStatus
{
    Untracked,
    Modified,
    Added,
    Deleted,
    Staged
}

public partial class FileChange : ObservableObject
{
    [ObservableProperty]
    private string _filePath = "";

    [ObservableProperty]
    private FileStatus _status;

    [ObservableProperty]
    private bool _isStaged;
}
