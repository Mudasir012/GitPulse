using System.Collections.Concurrent;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitPulse.Converters;

public class HexToBrushConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, SolidColorBrush> Cache = new();
    private static readonly SolidColorBrush Fallback = Create(Colors.Gray);

    private static SolidColorBrush Create(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrEmpty(hex))
        {
            return Cache.GetOrAdd(hex, static h =>
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(h);
                    return Create(color);
                }
                catch
                {
                    return Fallback;
                }
            });
        }
        return Fallback;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
