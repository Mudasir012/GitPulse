using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace GitPulse.Converters;

public class BoolToStrikeThroughConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isStaged && isStaged)
            return TextDecorations.Strikethrough;
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
