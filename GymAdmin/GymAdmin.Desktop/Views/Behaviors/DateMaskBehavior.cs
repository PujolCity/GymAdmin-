using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace GymAdmin.Desktop.Views.Behaviors;

public static class DateMaskBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DateMaskBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.RegisterAttached(
            "Format",
            typeof(string),
            typeof(DateMaskBehavior),
            new PropertyMetadata("dd/MM/yyyy"));

    public static void SetFormat(DependencyObject element, string value) =>
        element.SetValue(FormatProperty, value);
    public static string GetFormat(DependencyObject element) =>
        (string)element.GetValue(FormatProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DatePicker dp) return;

        if ((bool)e.NewValue)
        {
            dp.Loaded += OnLoaded;
            dp.Unloaded += OnUnloaded;
        }
        else
        {
            dp.Loaded -= OnLoaded;
            dp.Unloaded -= OnUnloaded;
            var tb = GetTextBox(dp);
            if (tb != null) DetachHandlers(dp, tb);
        }
    }

    private static void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker dp) return;
        var tb = GetTextBox(dp);
        if (tb == null) return;

        AttachHandlers(dp, tb);
        tb.MaxLength = 10; // dd/MM/yyyy
        InputMethod.SetIsInputMethodEnabled(tb, false);
    }

    private static void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker dp) return;
        var tb = GetTextBox(dp);
        if (tb != null) DetachHandlers(dp, tb);
    }

    private static DatePickerTextBox? GetTextBox(DatePicker dp)
    {
        if (dp.Template == null) return null;
        dp.ApplyTemplate();
        return FindChild<DatePickerTextBox>(dp);
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            var sub = FindChild<T>(child);
            if (sub != null) return sub;
        }
        return null;
    }

    private static void AttachHandlers(DatePicker dp, DatePickerTextBox tb)
    {
        DataObject.AddPastingHandler(tb, OnPaste);
        tb.PreviewTextInput += OnPreviewTextInput;
        tb.PreviewKeyDown += OnPreviewKeyDown;
        tb.TextChanged += OnTextChanged;
        tb.LostFocus += (s, _) => CommitIfValid(dp, tb);
    }

    private static void DetachHandlers(DatePicker dp, DatePickerTextBox tb)
    {
        DataObject.RemovePastingHandler(tb, OnPaste);
        tb.PreviewTextInput -= OnPreviewTextInput;
        tb.PreviewKeyDown -= OnPreviewKeyDown;
        tb.TextChanged -= OnTextChanged;
    }

    // Parcial permitido: "","1","12","12/","12/3","12/03","12/03/","12/03/2"... hasta 10 chars
    private static readonly Regex Partial = new(@"^\d{0,2}/?\d{0,2}/?\d{0,4}$");
    // dd/MM/yyyy con rangos básicos (día 01-31, mes 01-12)
    private static readonly Regex Final = new(@"^(0[1-9]|[12]\d|3[01])/(0[1-9]|1[0-2])/\d{4}$");

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not DatePickerTextBox tb) return;
        var proposed = ProposedText(tb, e.Text);
        e.Handled = !Partial.IsMatch(proposed);
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // bloqueá espacios
        if (e.Key == Key.Space) e.Handled = true;
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not DatePickerTextBox tb) return;
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }

        var paste = (string)e.SourceDataObject.GetData(DataFormats.Text);
        var proposed = ProposedText(tb, paste);
        if (!Final.IsMatch(proposed)) e.CancelCommand();
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var tb = (DatePickerTextBox)sender;
        // Inserta "/" automáticamente en 2 y 5
        if (tb.Text.Length == 2 && !tb.Text.Contains("/"))
        {
            var caret = tb.CaretIndex;
            tb.Text += "/";
            tb.CaretIndex = Math.Min(tb.Text.Length, caret + 1);
        }
        else if (tb.Text.Length == 5 && tb.Text.Count(c => c == '/') == 1)
        {
            var caret = tb.CaretIndex;
            tb.Text += "/";
            tb.CaretIndex = Math.Min(tb.Text.Length, caret + 1);
        }
    }

    private static string ProposedText(TextBox tb, string input)
    {
        var text = tb.Text ?? "";
        var start = tb.SelectionStart;
        var len = tb.SelectionLength;
        return text.Remove(start, len).Insert(start, input);
    }

    private static void CommitIfValid(DatePicker dp, DatePickerTextBox tb)
    {
        var text = tb.Text?.Trim() ?? "";
        if (!Final.IsMatch(text)) return;

        // Validación real de calendario (31/02 no pasa)
        if (DateTime.TryParseExact(text, "dd/MM/yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            dp.SelectedDate = dt; // actualiza el binding
        }
        // si falla, dejá que tu VM lo trate como inválido (SelectedDate quedará sin actualizar)
    }
}

