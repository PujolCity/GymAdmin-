using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GymAdmin.Desktop.Views.Behaviors;

public static class TimeMaskBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TimeMaskBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
        {
            DataObject.AddPastingHandler(tb, OnPaste);
            tb.PreviewTextInput += OnPreviewTextInput;
            tb.PreviewKeyDown += OnPreviewKeyDown;
            tb.TextChanged += OnTextChanged;
            InputMethod.SetIsInputMethodEnabled(tb, false); // evita IME
            tb.MaxLength = 5;
        }
        else
        {
            DataObject.RemovePastingHandler(tb, OnPaste);
            tb.PreviewTextInput -= OnPreviewTextInput;
            tb.PreviewKeyDown -= OnPreviewKeyDown;
            tb.TextChanged -= OnTextChanged;
        }
    }

    // Permite parciales: "","0","00","00:","00:1","00:12"
    private static readonly Regex Partial = new(@"^\d{0,2}:?\d{0,2}$");
    // Hora final válida 24h
    private static readonly Regex Final = new(@"^(?:[01]\d|2[0-3]):[0-5]\d$");

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var newText = GetProposedText(tb, e.Text);
        e.Handled = !Partial.IsMatch(newText);
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }

        var paste = (string)e.SourceDataObject.GetData(DataFormats.Text);
        var newText = GetProposedText(tb, paste);
        if (!Final.IsMatch(newText)) e.CancelCommand();
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space) e.Handled = true; // sin espacios
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var tb = (TextBox)sender;

        // Inserta ":" automáticamente después de HH si falta
        if (tb.Text.Length == 2 && !tb.Text.Contains(":"))
        {
            var caret = tb.CaretIndex;
            tb.Text += ":";
            tb.CaretIndex = Math.Min(tb.Text.Length, caret + 1);
        }
    }

    private static string GetProposedText(TextBox tb, string input)
    {
        var text = tb.Text ?? "";
        var start = tb.SelectionStart;
        var len = tb.SelectionLength;
        return text.Remove(start, len).Insert(start, input);
    }
}
