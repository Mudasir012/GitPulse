using System.Windows;
using Microsoft.Win32;

namespace GitPulse.Views;

public partial class CloneDialog : Window
{
    public string CloneUrl => UrlBox.Text.Trim();
    public string ClonePath => PathBox.Text.Trim();

    public CloneDialog()
    {
        InitializeComponent();
        DarkWindowHelper.Apply(this);
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            PathBox.Text = dialog.FolderName;
        }
    }

    private void Clone_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UrlBox.Text) || string.IsNullOrWhiteSpace(PathBox.Text))
        {
            MessageBox.Show("Please enter a URL and path.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
