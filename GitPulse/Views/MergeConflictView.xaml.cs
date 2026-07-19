using System.Windows;

namespace GitPulse.Views;

public partial class MergeConflictView : Window
{
    public MergeConflictView()
    {
        InitializeComponent();
        DarkWindowHelper.Apply(this);
    }
}
