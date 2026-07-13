using System.Globalization;
using System.Windows.Data;
using GitPulse.Models;

namespace GitPulse.Converters;

public class FileStatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FileStatus status)
        {
            return status switch
            {
                FileStatus.Modified => "M",
                FileStatus.Added => "A",
                FileStatus.Deleted => "D",
                FileStatus.Untracked => "?",
                FileStatus.Staged => "S",
                _ => "?"
            };
        }
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
