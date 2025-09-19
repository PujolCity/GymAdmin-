using System.Globalization;
using System.Windows.Data;

namespace GymAdmin.Desktop.Converters;

public class MinLengthToBoolConverter : IValueConverter
{
    public int MinLength { get; set; } = 2;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        var min = MinLength;
        if (parameter is string p && int.TryParse(p, out var parsed)) min = parsed;
        return !string.IsNullOrWhiteSpace(s) && s.Trim().Length >= min;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}