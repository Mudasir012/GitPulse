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
                ? "#89B4FA"
                : "#585B70";
        }
        return "#585B70";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
