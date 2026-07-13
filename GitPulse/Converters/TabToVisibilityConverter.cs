using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GitPulse.Converters;

public class TabToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeTab && parameter is string targetTab)
        {
            return activeTab.Equals(targetTab, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
