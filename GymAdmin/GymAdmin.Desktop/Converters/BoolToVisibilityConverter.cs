using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GymAdmin.Desktop.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;
    public bool UseHidden { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool b) return DependencyProperty.UnsetValue;
        if (Invert) b = !b;

        if (b) return Visibility.Visible;
        return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v
            ? (Invert ? v != Visibility.Visible : v == Visibility.Visible)
            : DependencyProperty.UnsetValue;
}
