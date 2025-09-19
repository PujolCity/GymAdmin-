using GymAdmin.Desktop.ViewModels.Pagos;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GymAdmin.Desktop.Views.Dialogs;
/// <summary>
/// Lógica de interacción para AddPagoDialog.xaml
/// </summary>
public partial class AddPagoDialog : UserControl
{
    private AddPagoViewModel? Vm => DataContext as AddPagoViewModel;

    public AddPagoDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        SocioSearchBox.PreviewKeyDown += SocioSearchBox_PreviewKeyDown;
        SocioListBox.MouseDoubleClick += SocioListBox_MouseDoubleClick;
        SocioListBox.PreviewKeyDown += SocioListBox_PreviewKeyDown;

        var precioTb = FindName("PrecioTextBox") as TextBox;
        if (precioTb != null)
        {
            precioTb.PreviewTextInput += PrecioTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(precioTb, PrecioTextBox_OnPaste);
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SociosSugeridos is INotifyCollectionChanged incc)
            incc.CollectionChanged += SociosSugeridos_CollectionChanged;

        SocioSearchBox.TextChanged += SocioSearchBox_TextChanged;
        SocioSearchBox.LostKeyboardFocus += (_, __) => SocioPopup.IsOpen = false;

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            SocioSearchBox.Focus();
            SocioSearchBox.CaretIndex = SocioSearchBox.Text?.Length ?? 0;
        }));
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SociosSugeridos is INotifyCollectionChanged incc)
            incc.CollectionChanged -= SociosSugeridos_CollectionChanged;

        SocioSearchBox.TextChanged -= SocioSearchBox_TextChanged;
    }

    private void SocioSearchBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateSocioPopup();
    private void SociosSugeridos_CollectionChanged(object? s, NotifyCollectionChangedEventArgs e) => UpdateSocioPopup();

    private void UpdateSocioPopup()
    {
        var hasText = !string.IsNullOrWhiteSpace(SocioSearchBox.Text) && SocioSearchBox.Text.Trim().Length >= 2;
        var hasResults = Vm?.SociosSugeridos?.Count > 0;
        var hasFocus = SocioSearchBox.IsKeyboardFocusWithin;

        SocioPopup.IsOpen = hasText && hasResults && hasFocus;
    }

    private void SocioSearchBox_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && SocioListBox.Items.Count > 0)
        {
            SocioListBox.Focus();
            SocioListBox.SelectedIndex = 0;
            (SocioListBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem)?.Focus();
            e.Handled = true;
        }
    }

    private void SocioListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ConfirmSocioSelection();

    private void SocioListBox_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { ConfirmSocioSelection(); e.Handled = true; }
        else if (e.Key == Key.Escape) { SocioPopup.IsOpen = false; SocioSearchBox.Focus(); e.Handled = true; }
    }

    private void ConfirmSocioSelection()
    {
        if (SocioListBox.SelectedItem != null)
        {
            SocioPopup.IsOpen = false;
            SocioSearchBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }

    // --- Numeric Precio ---
    private static readonly char[] AllowedDecimalSeparators =
        { Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), '.', ',' };

    private void PrecioTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) { e.Handled = true; return; }

        bool isDigit = e.Text.All(char.IsDigit);
        bool isSep = e.Text.Length == 1 && AllowedDecimalSeparators.Contains(e.Text[0]);
        if (!isDigit && !isSep) { e.Handled = true; return; }

        if (isSep && (tb.Text.Contains(',') || tb.Text.Contains('.')))
            e.Handled = true;
    }

    private void PrecioTextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }
        var pasted = (string)e.DataObject.GetData(DataFormats.Text);
        if (!decimal.TryParse(pasted.Replace('.', ','), out _)) e.CancelCommand();
    }
}