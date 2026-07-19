using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitPulse.Converters;

/// <summary>
/// Returns the accent brush when the active tab matches the parameter,
/// otherwise Transparent. Used for the tab underline indicator.
/// </summary>
public class TabIndicatorBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Active = Create(0x58, 0xA6, 0xFF);
    private static readonly SolidColorBrush Inactive = Create(0, 0, 0, 0);

    private static SolidColorBrush Create(byte r, byte g, byte b, byte a = 0xFF)
    {
        var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        brush.Freeze();
        return brush;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeTab && parameter is string targetTab)
        {
            return activeTab.Equals(targetTab, StringComparison.OrdinalIgnoreCase)
                ? Active
                : Inactive;
        }
        return Inactive;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
