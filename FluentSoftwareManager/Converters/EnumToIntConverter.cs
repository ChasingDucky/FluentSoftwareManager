using Microsoft.UI.Xaml.Data;

namespace FluentSoftwareManager.Converters;

public class EnumToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Enum enumValue)
        {
            return (int)(object)enumValue;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return Enum.ToObject(targetType, intValue);
        }
        return Enum.GetValues(targetType).GetValue(0) ?? 0;
    }
}
