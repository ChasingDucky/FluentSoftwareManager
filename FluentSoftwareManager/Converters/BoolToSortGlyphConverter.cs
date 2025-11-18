using Microsoft.UI.Xaml.Data;

namespace FluentSoftwareManager.Converters;

public class BoolToSortGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isAscending)
        {
            // &#xE70E; is the up arrow glyph for ascending
            // &#xE70D; is the down arrow glyph for descending
            return isAscending ? "\uE70E" : "\uE70D";
        }
        return "\uE70E";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
