using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GitPulse.Models;

namespace GitPulse.Converters;

public class DiffLineTypeToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush AdditionBg = new(Color.FromArgb(40, 166, 227, 161));
    private static readonly SolidColorBrush DeletionBg = new(Color.FromArgb(40, 243, 139, 168));
    private static readonly SolidColorBrush HeaderBg = new(Color.FromArgb(30, 137, 180, 250));
    private static readonly SolidColorBrush DefaultBg = new(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DiffLineType type)
        {
            return type switch
            {
                DiffLineType.Addition => AdditionBg,
                DiffLineType.Deletion => DeletionBg,
                DiffLineType.Header => HeaderBg,
                _ => DefaultBg
            };
        }
        return DefaultBg;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
