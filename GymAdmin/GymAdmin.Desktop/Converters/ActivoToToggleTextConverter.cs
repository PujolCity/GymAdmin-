using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GymAdmin.Desktop.Converters;

public sealed class ActivoToToggleTextConverter : IValueConverter
{
    public string TextoSiActivo { get; set; } = "Desactivar";
    public string TextoSiInactivo { get; set; } = "Activar";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? (b ? TextoSiActivo : TextoSiInactivo) : DependencyProperty.UnsetValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
