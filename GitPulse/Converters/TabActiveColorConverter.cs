using System.Globalization;
using System.Windows.Data;

namespace GitPulse.Converters;

public class TabActiveColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeTab && parameter is string targetTab)
        {
            return activeTab.Equals(targetTab, StringComparison.OrdinalIgnoreCase)
                ? "#58A6FF"
                : "#61616D";
        }
        return "#61616D";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
