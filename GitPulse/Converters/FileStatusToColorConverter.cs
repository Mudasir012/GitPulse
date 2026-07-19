using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GitPulse.Models;

namespace GitPulse.Converters;

public class FileStatusToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Modified = Create(210, 153, 34);
    private static readonly SolidColorBrush Added = Create(63, 185, 80);
    private static readonly SolidColorBrush Deleted = Create(248, 81, 73);
    private static readonly SolidColorBrush Untracked = Create(156, 156, 168);
    private static readonly SolidColorBrush Staged = Create(88, 166, 255);

    private static SolidColorBrush Create(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

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
