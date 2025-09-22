using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GymAdmin.Desktop.Views.Dialogs;
/// <summary>
/// Lógica de interacción para EditarSocioDialog.xaml
/// </summary>
public partial class EditarSocioDialog : UserControl
{
    public EditarSocioDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            // Enfocar DNI al abrir
            DniTextBox.Focus();
            DniTextBox.CaretIndex = DniTextBox.Text?.Length ?? 0;
            DniTextBox.SelectAll();
        }));
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);

        if (!e.Handled && sender is TextBox tb)
        {
            var newText = GetTextAfterInput(tb, e.Text);
            if (newText.Length > 8) e.Handled = true;
        }
    }

    private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox tb)
        {
            e.CancelCommand();
            return;
        }

        if (!e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        string pasted = (string)e.DataObject.GetData(DataFormats.Text);

        // Debe ser todo dígitos
        if (string.IsNullOrWhiteSpace(pasted) || !pasted.All(char.IsDigit))
        {
            e.CancelCommand();
            return;
        }

        // Respetar longitud máxima (8)
        var newText = GetTextAfterPaste(tb, pasted);
        if (newText.Length > 8)
        {
            e.CancelCommand();
            return;
        }
    }

    private void NombreTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;

        foreach (char c in e.Text)
        {
            if (!char.IsLetter(c) &&
                !char.IsWhiteSpace(c) &&
                c != '-' &&
                c != '\'' &&
                c != '´' &&
                c != '`' &&
                !"áéíóúÁÉÍÓÚüÜñÑ".Contains(c))
            {
                e.Handled = true;
                return;
            }
        }

        if (e.Text == " " && tb.Text.EndsWith(" "))
        {
            e.Handled = true;
        }
    }

    private static string GetTextAfterInput(TextBox tb, string incomingText)
    {
        var text = tb.Text ?? string.Empty;
        int selStart = tb.SelectionStart;
        int selLength = tb.SelectionLength;

        var before = selLength > 0 ? text.Remove(selStart, selLength) : text;
        return before.Insert(selStart, incomingText);
    }

    private static string GetTextAfterPaste(TextBox tb, string pasted)
    {
        var text = tb.Text ?? string.Empty;
        int selStart = tb.SelectionStart;
        int selLength = tb.SelectionLength;

        var withoutSelection = selLength > 0 ? text.Remove(selStart, selLength) : text;
        return withoutSelection.Insert(selStart, pasted);
    }
}
