using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GitPulse.Models;

namespace GitPulse.Converters;

public class DiffLineTypeToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush AdditionBg = Create(28, 63, 185, 80);
    private static readonly SolidColorBrush DeletionBg = Create(28, 248, 81, 73);
    private static readonly SolidColorBrush HeaderBg = Create(22, 88, 166, 255);
    private static readonly SolidColorBrush DefaultBg = Create(0, 0, 0, 0);

    private static SolidColorBrush Create(byte a, byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        brush.Freeze();
        return brush;
    }

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
