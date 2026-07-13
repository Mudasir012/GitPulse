using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GitPulse.Models;

namespace GitPulse.Converters;

public class FileStatusToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Modified = new(Color.FromRgb(250, 200, 100));
    private static readonly SolidColorBrush Added = new(Color.FromRgb(166, 227, 161));
    private static readonly SolidColorBrush Deleted = new(Color.FromRgb(243, 139, 168));
    private static readonly SolidColorBrush Untracked = new(Color.FromRgb(148, 148, 176));
    private static readonly SolidColorBrush Staged = new(Color.FromRgb(137, 180, 250));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FileStatus status)
        {
            return status switch
            {
                FileStatus.Modified => Modified,
                FileStatus.Added => Added,
                FileStatus.Deleted => Deleted,
                FileStatus.Untracked => Untracked,
                FileStatus.Staged => Staged,
                _ => Untracked
            };
        }
        return Untracked;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
