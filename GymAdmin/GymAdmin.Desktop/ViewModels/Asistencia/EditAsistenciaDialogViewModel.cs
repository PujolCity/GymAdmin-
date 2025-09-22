using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GymAdmin.Desktop.ViewModels.Asistencia;

public sealed partial class EditAsistenciaDialogViewModel : ViewModelBase, IDataErrorInfo, IDisposable
{
    private readonly TaskCompletionSource<AsistenciaEditorResult?> _tcs = new();
    private CancellationTokenSource? _cts;

    public Task<AsistenciaEditorResult?> Task => _tcs.Task;

    public EditAsistenciaDialogViewModel()
    {
        Title = "Asistencia";
        AcceptText = "Guardar";

        // si tenés este helper en tu ViewModelBase, igual que en AddEditPlan:
        BindBusyToCommands(AcceptCommand);
    }

    // --- UI header
    [ObservableProperty] private string title = "Asistencia";
    [ObservableProperty] private string acceptText = "Guardar";

    // --- Campos editables
    [ObservableProperty] private DateTime? entradaDate;
    [ObservableProperty] private string entradaTimeText = ""; // "HH:mm"
    [ObservableProperty] private string observaciones = "";
    [ObservableProperty] private bool seUsoCredito; // por si lo querés tildar en el editor

    // --- Estado UI
    [ObservableProperty] private string? errorMessage;

    // flags de interacción para mostrar errores como en Planes
    [ObservableProperty] private bool hasDateInteracted;
    [ObservableProperty] private bool hasTimeInteracted;

    partial void OnEntradaDateChanged(DateTime? value) { HasDateInteracted = true; NotifyValidationChanged(); }
    partial void OnEntradaTimeTextChanged(string value) { HasTimeInteracted = true; NotifyValidationChanged(); }

    private void NotifyValidationChanged()
    {
        OnPropertyChanged(nameof(FormularioValido));
        AcceptCommand.NotifyCanExecuteChanged();
    }

    // --- Inicialización (nuevo/edición)
    public void Initialize(DateTime? entradaLocal = null, string? obs = null, string? title = null, string? accept = null, bool? usado = null)
    {
        var now = DateTime.Now;
        var baseDt = entradaLocal ?? now;

        EntradaDate = baseDt.Date;
        EntradaTimeText = baseDt.ToString("HH:mm", CultureInfo.InvariantCulture);
        Observaciones = obs ?? "";

        if (!string.IsNullOrWhiteSpace(title)) Title = title!;
        if (!string.IsNullOrWhiteSpace(accept)) AcceptText = accept!;
    }

    // --- Validaciones (IDataErrorInfo)
    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(EntradaDate) =>
            !HasDateInteracted ? string.Empty :
            (EntradaDate is null ? "Seleccione una fecha." : string.Empty),

        nameof(EntradaTimeText) =>
            !HasTimeInteracted ? string.Empty :
            (ValidarHora(EntradaTimeText)),

        _ => string.Empty
    };

    private static string ValidarHora(string? value)
    {
        var text = value ?? "";
        if (!Regex.IsMatch(text, @"^\d{2}:\d{2}$"))
            return "Hora inválida. Use HH:mm (p. ej. 09:30).";

        var parts = text.Split(':');
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hh) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mm) ||
            hh is < 0 or > 23 || mm is < 0 or > 59)
            return "Hora inválida. Use HH:mm (p. ej. 09:30).";

        return string.Empty;
    }

    public bool FormularioValido =>
        string.IsNullOrEmpty(ForceValidate(nameof(EntradaDate))) &&
        string.IsNullOrEmpty(ForceValidate(nameof(EntradaTimeText)));

    private string ForceValidate(string propertyName) => this[propertyName];

    // --- Comandos
    [RelayCommand]
    private void Cancel() => _tcs.TrySetResult(null);

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private void Accept()
    {
        // Forzar “mostrar errores” si no tocaron los campos
        if (!HasDateInteracted) { HasDateInteracted = true; OnPropertyChanged(nameof(EntradaDate)); }
        if (!HasTimeInteracted) { HasTimeInteracted = true; OnPropertyChanged(nameof(EntradaTimeText)); }
        OnPropertyChanged(nameof(FormularioValido));

        if (!FormularioValido) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var parts = EntradaTimeText!.Split(':');
            var hh = int.Parse(parts[0], CultureInfo.InvariantCulture);
            var mm = int.Parse(parts[1], CultureInfo.InvariantCulture);

            var local = EntradaDate!.Value.Date.AddHours(hh).AddMinutes(mm);
            // si tu backend persiste UTC, convertí acá:
            var utc = DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();

            var res = new AsistenciaEditorResult
            {
                EntradaLocal = local,
                EntradaUtc = utc,
                Observaciones = (Observaciones ?? "").Trim(),
                SeUsoCredito = SeUsoCredito
            };

            _tcs.TrySetResult(res);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAccept() => FormularioValido && !IsBusy;

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

public sealed class AsistenciaEditorResult
{
    public DateTime EntradaLocal { get; set; }
    public DateTime EntradaUtc { get; set; }  // útil si persistís en UTC
    public string Observaciones { get; set; } = "";
    public bool SeUsoCredito { get; set; }
}