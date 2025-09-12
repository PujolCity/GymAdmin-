using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GymAdmin.Desktop.Converters;

public sealed class BoolToStatusPillBrushConverter : IValueConverter
{
    public Brush BrushActivo { get; set; } = new SolidColorBrush(Color.FromRgb(220, 247, 225)); // verde suave
    public Brush BrushInactivo { get; set; } = new SolidColorBrush(Color.FromRgb(253, 226, 226)); // rojo suave

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? (b ? BrushActivo : BrushInactivo) : DependencyProperty.UnsetValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
