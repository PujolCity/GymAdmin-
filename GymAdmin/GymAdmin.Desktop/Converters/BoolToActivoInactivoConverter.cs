using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GymAdmin.Desktop.Converters;

public sealed class BoolToActivoInactivoConverter : IValueConverter
{
    public string TextoActivo { get; set; } = "Activo";
    public string TextoInactivo { get; set; } = "Inactivo";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? (b ? TextoActivo : TextoInactivo) : DependencyProperty.UnsetValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s ? s.Equals(TextoActivo) : DependencyProperty.UnsetValue;
}