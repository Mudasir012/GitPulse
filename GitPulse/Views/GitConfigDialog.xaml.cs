using System.Windows;

namespace GitPulse.Views;

public partial class GitConfigDialog : Window
{
    public string UserName => NameBox.Text.Trim();
    public string UserEmail => EmailBox.Text.Trim();

    public GitConfigDialog()
    {
        InitializeComponent();
        DarkWindowHelper.Apply(this);
        NameBox.Focus();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text))
        {
            MessageBox.Show("Please enter both a name and an email.", "Error",
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
