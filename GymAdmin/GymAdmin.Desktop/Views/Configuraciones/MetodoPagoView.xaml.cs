using GymAdmin.Desktop.ViewModels.Configuracion;
using System.ComponentModel;
using System.Windows.Controls;

namespace GymAdmin.Desktop.Views.Configuraciones;
/// <summary>
/// Lógica de interacción para MetodoPagoView.xaml
/// </summary>
public partial class MetodoPagoView : UserControl
{
    public MetodoPagoView()
    {
        InitializeComponent();
    }

    private async void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        // Validar que exista VM correcto
        if (DataContext is not ConfigViewModel vm)
        {
            e.Handled = true;
            return;
        }

        var column = e.Column.SortMemberPath;
        if (string.IsNullOrWhiteSpace(column))
        {
            e.Handled = true;
            return;
        }

        // Determinar nueva dirección
        var newDirection = e.Column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        e.Handled = true;
        e.Column.SortDirection = newDirection;

        // Llamar al método de ordenamiento del VM
        //await vm.ApplySortAsync(column, newDirection == ListSortDirection.Descending);

    }
}
