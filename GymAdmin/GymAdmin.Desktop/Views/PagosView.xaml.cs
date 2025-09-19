using GymAdmin.Desktop.ViewModels.Pagos;
using GymAdmin.Desktop.ViewModels.Socios;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GymAdmin.Desktop.Views;
/// <summary>
/// Lógica de interacción para PagosView.xaml
/// </summary>
public partial class PagosView : UserControl
{
    public PagosView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        if (DataContext is SociosViewModel vm)
        {
            var column = e.Column.SortMemberPath;
            if (string.IsNullOrWhiteSpace(column))
            {
                // no ordenar esta columna
                e.Handled = true;
                return;
            }

            var newDirection = e.Column.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            e.Handled = true;
            e.Column.SortDirection = newDirection;

            await vm.ApplySortAsync(column, newDirection == ListSortDirection.Descending);
        }
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is PagosViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            SearchTextBox.Focus();
            SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
            SearchTextBox.SelectAll();
        }));
    }
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is PagosViewModel vm)
            vm.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PagosViewModel.IsDialogOpen))
        {
            var viewModel = (PagosViewModel)DataContext;
            DialogOverlay.Visibility = viewModel.IsDialogOpen ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    private void DialogOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == DialogOverlay && DataContext is PagosViewModel vm)
        {
            vm.IsDialogOpen = false;
        }
    }

}
