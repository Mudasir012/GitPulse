using System.Globalization;
using System.Windows.Data;

namespace GitPulse.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isStaged)
            return isStaged ? 0.5 : 1.0;
        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
