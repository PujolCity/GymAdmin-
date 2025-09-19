using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GymAdmin.Desktop.Views.Behaviors;

public static class NumericInput
{  
    public static readonly DependencyProperty IsDecimalProperty =
        DependencyProperty.RegisterAttached("IsDecimal", typeof(bool), typeof(NumericInput),
            new PropertyMetadata(false, OnAttachChanged));

    public static readonly DependencyProperty MaxDecimalsProperty =
        DependencyProperty.RegisterAttached("MaxDecimals", typeof(int), typeof(NumericInput),
            new PropertyMetadata(2));

    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.RegisterAttached("MaxLength", typeof(int), typeof(NumericInput),
            new PropertyMetadata(int.MaxValue));

    public static bool GetIsDecimal(DependencyObject obj) => (bool)obj.GetValue(IsDecimalProperty);
    public static void SetIsDecimal(DependencyObject obj, bool value) => obj.SetValue(IsDecimalProperty, value);

    public static int GetMaxDecimals(DependencyObject obj) => (int)obj.GetValue(MaxDecimalsProperty);
    public static void SetMaxDecimals(DependencyObject obj, int value) => obj.SetValue(MaxDecimalsProperty, value);

    public static int GetMaxLength(DependencyObject obj) => (int)obj.GetValue(MaxLengthProperty);
    public static void SetMaxLength(DependencyObject obj, int value) => obj.SetValue(MaxLengthProperty, value);

    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
        {
            tb.PreviewTextInput += OnPreviewTextInput;
            tb.PreviewKeyDown += OnPreviewKeyDown;   // para bloquear espacios
            DataObject.AddPastingHandler(tb, OnPaste);
        }
        else
        {
            tb.PreviewTextInput -= OnPreviewTextInput;
            tb.PreviewKeyDown -= OnPreviewKeyDown;
            DataObject.RemovePastingHandler(tb, OnPaste);
        }
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space) e.Handled = true;
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;

        bool isDecimal = GetIsDecimal(tb);
        int maxLen = GetMaxLength(tb);
        int maxDecs = GetMaxDecimals(tb);

        string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        string incoming = e.Text;

        // Solo dígitos o separador decimal (si se permite)
        bool isSep = isDecimal && (incoming == "." || incoming == "," || incoming == sep);
        if (!incoming.All(char.IsDigit) && !isSep)
        {
            e.Handled = true; return;
        }

        // Construir texto resultante (considerando selección)
        string newText = ComposeNewText(tb, incoming);

        // Normalizar: si el usuario puso "." en cultura con ",", reemplazar
        if (isDecimal && sep != "." && newText.Contains(".")) newText = newText.Replace(".", sep);
        if (isDecimal && sep != "," && newText.Contains(",")) newText = newText.Replace(",", sep);

        // Largo máximo
        if (newText.Length > maxLen) { e.Handled = true; return; }

        // Validar formato numérico
        if (isDecimal)
        {
            // No más de un separador decimal
            if (newText.Count(c => c.ToString() == sep) > 1) { e.Handled = true; return; }

            // Decimales máximos
            var parts = newText.Split(new[] { sep }, StringSplitOptions.None);
            if (parts.Length == 2 && parts[1].Length > maxDecs) { e.Handled = true; return; }
        }
        else
        {
            // Entero: no permitir separador
            if (newText.Contains(sep) || newText.Contains(".") || newText.Contains(",")) { e.Handled = true; return; }
        }
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox tb) { e.CancelCommand(); return; }
        if (!e.DataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }

        string pasted = (string)e.DataObject.GetData(DataFormats.Text);
        bool isDecimal = GetIsDecimal(tb);
        int maxLen = GetMaxLength(tb);
        int maxDecs = GetMaxDecimals(tb);
        string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        string newText = ComposeNewText(tb, pasted);

        // Normalizar separador
        if (isDecimal && sep != "." && newText.Contains(".")) newText = newText.Replace(".", sep);
        if (isDecimal && sep != "," && newText.Contains(",")) newText = newText.Replace(",", sep);

        if (newText.Length > maxLen) { e.CancelCommand(); return; }

        // Validaciones básicas reutilizando la lógica de escritura
        if (isDecimal)
        {
            if (newText.Any(c => !char.IsDigit(c) && c.ToString() != sep))
            { e.CancelCommand(); return; }

            if (newText.Count(c => c.ToString() == sep) > 1)
            { e.CancelCommand(); return; }

            var parts = newText.Split(new[] { sep }, StringSplitOptions.None);
            if (parts.Length == 2 && parts[1].Length > maxDecs)
            { e.CancelCommand(); return; }
        }
        else
        {
            if (newText.Any(c => !char.IsDigit(c)))
            { e.CancelCommand(); return; }
        }
    }

    private static string ComposeNewText(TextBox tb, string incoming)
    {
        var text = tb.Text ?? string.Empty;
        int start = tb.SelectionStart;
        int length = tb.SelectionLength;

        var baseText = length > 0 ? text.Remove(start, length) : text;
        return baseText.Insert(start, incoming);
    }
}
