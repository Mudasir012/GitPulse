using System.Globalization;
using System.Windows.Data;

namespace GitPulse.Converters;

/// <summary>
/// Converts a string to a fixed pixel width based on its character count.
/// Used by the commit graph so every glyph column has an identical width,
/// which keeps graph lanes aligned even when a glyph (e.g. '●') falls back
/// to a different font family with different advance metrics.
/// </summary>
public class TextLengthToWidthConverter : IValueConverter
{
    private const double CharWidth = 8.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return Math.Max(CharWidth, text.Length * CharWidth);
        }
        return CharWidth;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
