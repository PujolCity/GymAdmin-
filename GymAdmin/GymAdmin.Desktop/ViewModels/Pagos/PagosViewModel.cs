using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.MetodosDePagoDto;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.Interactor.PagosInteractors;
using GymAdmin.Desktop.ViewModels.Dialogs;
using GymAdmin.Desktop.Views.Dialogs;
using GymAdmin.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace GymAdmin.Desktop.ViewModels.Pagos;

public partial class PagosViewModel : ViewModelBase, IDisposable
{
    private const string COLOR_ROJO = "DangerButtonStyle";
    private const string COLOR_PRIMARIO = "PrimaryButtonStyle";

    private readonly IGetPagosInteractor _getPagosInteractor;
    private readonly IAnularPagoInteractor _anularPagoInteractor;
    private readonly IGetMetodosPagoInteractor _getMetodosPago;
    private readonly IServiceProvider _sp;

    private CancellationTokenSource? _cts;

    public ObservableCollection<PagoDto> Pagos { get; } = new();
    public ObservableCollection<MetodoPagoDto> MetodosPago { get; } = new();
    public ObservableCollection<string> Estados { get; } = new(new[] { "Todos", "Pagado", "Anulado" });
    public ObservableCollection<int> PageSizes { get; } = new(new[] { 10, 25, 50, 100 });

    [ObservableProperty] private string textoFiltro = string.Empty;
    [ObservableProperty] private DateTime? fechaDesde = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime? fechaHasta = DateTime.Today;
    [ObservableProperty] private MetodoPagoDto? metodoSeleccionado;

    [ObservableProperty]
    private string estadoSeleccionado = "Todos";

    [ObservableProperty]
    private string sortBy = "FECHA";

    [ObservableProperty]
    private bool sortDesc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    [NotifyCanExecuteChangedFor(nameof(GoFirstPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoPrevPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoLastPageCommand))]
    private int pageNumber = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int pageSize = 25;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    [NotifyCanExecuteChangedFor(nameof(GoNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoLastPageCommand))]
    private int totalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int totalCount;

    public int DisplayFrom => Math.Min(((PageNumber - 1) * PageSize) + 1, Math.Max(1, TotalCount));
    public int DisplayTo => Math.Min(PageNumber * PageSize, TotalCount);

    [ObservableProperty] private PagoDto? pagoSeleccionado;
    [ObservableProperty] private bool isDialogOpen;
    [ObservableProperty] private object? dialogContent;

    public PagosViewModel(
        IGetPagosInteractor get,
        IAnularPagoInteractor anularPagoInteractor,
        IGetMetodosPagoInteractor getMethods,
        IServiceProvider sp)
    {
        _getPagosInteractor = get;
        _anularPagoInteractor = anularPagoInteractor;
        _getMetodosPago = getMethods;
        _sp = sp;

        // Carga inicial
        _ = LoadAsync();
        _ = LoadMethodsAsync();

        BindBusyToCommands(LoadCommand, NuevoPagoCommand, VerPagoCommand, AnularPagoCommand, GoFirstPageCommand, GoPrevPageCommand, GoNextPageCommand,
                           GoLastPageCommand);
    }

    partial void OnTextoFiltroChanged(string value)
     => _ = DebouncedReloadAsync();

    partial void OnFechaDesdeChanged(DateTime? value)
        => _ = DebouncedReloadAsync();

    partial void OnFechaHastaChanged(DateTime? value)
        => _ = DebouncedReloadAsync();

    partial void OnMetodoSeleccionadoChanged(MetodoPagoDto? value)
        => _ = DebouncedReloadAsync();

    partial void OnEstadoSeleccionadoChanged(string value)
        => _ = DebouncedReloadAsync();

    partial void OnPageSizeChanged(int value)
    {
        PageNumber = 1;
        _ = LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoFirstPage))]
    private async Task GoFirstPage() { if (PageNumber <= 1) return; PageNumber = 1; await LoadAsync(); }
    private bool CanGoFirstPage() => !IsBusy && PageNumber > 1;

    [RelayCommand(CanExecute = nameof(CanGoPrevPage))]
    private async Task GoPrevPage() { if (PageNumber <= 1) return; PageNumber--; await LoadAsync(); }
    private bool CanGoPrevPage() => !IsBusy && PageNumber > 1;

    [RelayCommand(CanExecute = nameof(CanGoNextPage))]
    private async Task GoNextPage() { if (PageNumber >= TotalPages) return; PageNumber++; await LoadAsync(); }
    private bool CanGoNextPage() => !IsBusy && PageNumber < TotalPages;

    [RelayCommand(CanExecute = nameof(CanGoLastPage))]
    private async Task GoLastPage() { if (PageNumber >= TotalPages) return; PageNumber = TotalPages; await LoadAsync(); }
    private bool CanGoLastPage() => !IsBusy && PageNumber < TotalPages;
    private async Task DebouncedReloadAsync(int delay = 300)
    {
        var cts = new CancellationTokenSource();
        _cts?.Cancel();
        _cts = cts;
        try
        {
            await Task.Delay(delay, cts.Token);
            if (_cts != cts)
                return;
            PageNumber = 1;
            await LoadAsync(cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            IsBusy = true; ErrorMessage = null;

            var estado = EstadoSeleccionado switch
            {
                "Pagado" => StatusPagosFilter.Pagado,
                "Anulado" => StatusPagosFilter.Anulado,
                _ => StatusPagosFilter.Todos
            };

            var req = new GetPagosRequest
            {
                Texto = string.IsNullOrWhiteSpace(TextoFiltro) ? null : TextoFiltro.Trim(),
                FechaDesde = FechaDesde,
                FechaHasta = FechaHasta?.AddDays(1).AddTicks(-1),
                Status = estado,
                PageNumber = PageNumber,
                PageSize = PageSize,
                SortBy = SortBy,
                SortDesc = SortDesc
            };

            var result = await _getPagosInteractor.ExecuteAsync(req, ct);

            Pagos.Clear();
            foreach (var p in result.Items) Pagos.Add(p);

            TotalCount = result.TotalCount;
            TotalPages = Math.Max(1, result.TotalPages);
        }
        finally { IsBusy = false; }
    }

    private async Task LoadMethodsAsync()
    {
        var metodos = await _getMetodosPago.ExecuteAsync(CancellationToken.None);
        MetodosPago.Clear();
        foreach (var m in metodos) MetodosPago.Add(m);
    }

    private bool CanSimpleAction() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void NuevoPago()
    {
        var vm = _sp.GetRequiredService<AddPagoViewModel>();
        vm.CloseRequested += async () =>
        {
            IsDialogOpen = false; DialogContent = null;
            await LoadAsync();
        };
        OpenDialog(new AddPagoDialog { DataContext = vm });
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void VerPago(PagoDto? pago)
    {
        var dto = pago ?? PagoSeleccionado;
        if (dto is null) return;

        var vm = _sp.GetRequiredService<VerPagoViewModel>();
        vm.Load(dto);
        vm.CloseRequested += () =>
        {
            IsDialogOpen = false;
            DialogContent = null;
        };

        var view = new VerPagoDialog { DataContext = vm };
        OpenDialog(view);
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task AnularPago(PagoDto? pago)
    {
        var target = pago ?? PagoSeleccionado;
        if (target is null) return;

        var ok = await ConfirmAsync("Anular pago",
            $"¿Querés anular el pago de {target.SocioNombre} de {target.Precio:C}?\n" +
            $"Esta acción no elimina, sólo cambia el estado a Anulado.", "Anular", "Cancelar", COLOR_ROJO, COLOR_PRIMARIO);

        if (!ok) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var result = await _anularPagoInteractor.ExecuteAsync(target, CancellationToken.None);

            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                var motivo = string.Join(Environment.NewLine, result.Errors);
                ErrorMessage = string.IsNullOrWhiteSpace(motivo)
                    ? "No se pudo anular el pago por un error desconocido."
                    : motivo;

                await ConfirmAsync("No se pudo anular", ErrorMessage, "Aceptar", string.Empty, COLOR_PRIMARIO, COLOR_ROJO, false);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void LimpiarFiltro()
    {
        TextoFiltro = string.Empty;
        EstadoSeleccionado = "Todos";
    }
    private void OpenDialog(object content) { DialogContent = content; IsDialogOpen = true; }

    private async Task<bool> ConfirmAsync(string title, string msg, string acceptText, string cancelText, string acceptColor = COLOR_ROJO, string cancelColor = COLOR_PRIMARIO, bool showCancel = true)
    {
        var vm = new ConfirmDialogViewModel
        {
            Title = title,
            Message = msg,
            AcceptText = acceptText,
            CancelText = cancelText,
            AcceptButtonStyle = (Style)Application.Current.FindResource(acceptColor),
            CancelButtonStyle = (Style)Application.Current.FindResource(cancelColor),
            ShowCancelButton = showCancel
        };

        var view = new ConfirmDialogView { DataContext = vm };
        OpenDialog(view);
        var res = await vm.Task;
        IsDialogOpen = false; DialogContent = null;
        return res;
    }

    public void Dispose() { _cts?.Cancel(); _cts?.Dispose(); }
}