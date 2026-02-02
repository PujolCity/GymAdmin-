using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Enums;
using System.Collections.ObjectModel;

namespace GymAdmin.Desktop.ViewModels.Configuracion;


public partial class MetodoPagoViewModel : ViewModelBase, IDisposable
{
    private readonly IGetMetodosPagoInteractor _getMetodoPagoInteractor;
    private readonly ICreateMetodoPagoInteractor _createMetodoPagoInteractor;
    private readonly IUpdateMetodoPagoInteractor _updateMetodoPagoInteractor;
    private readonly IDeleteMetodoPagoInteractor _deleteMetodoPagoInteractor;
    private readonly IMoveDownInteractor _moveDownInteractor;
    private readonly IMoveUpInteractor _moveUpInteractor;

    public MetodoPagoViewModel(IGetMetodosPagoInteractor getMetodoPagoInteractor,
        ICreateMetodoPagoInteractor createMetodoPagoInteractor,
        IUpdateMetodoPagoInteractor updateMetodoPagoInteractor,
        IDeleteMetodoPagoInteractor deleteMetodoPagoInteractor,
        IMoveDownInteractor moveDownInteractor,
        IMoveUpInteractor moveUpInteractor)
    {
        _getMetodoPagoInteractor = getMetodoPagoInteractor;
        _createMetodoPagoInteractor = createMetodoPagoInteractor;
        _updateMetodoPagoInteractor = updateMetodoPagoInteractor;
        _deleteMetodoPagoInteractor = deleteMetodoPagoInteractor;
        _moveDownInteractor = moveDownInteractor;
        _moveUpInteractor = moveUpInteractor;

        BindBusyToCommands(LoadCommand, GoFirstPageCommand, LimpiarFiltroCommand,
                           GoPrevPageCommand, GoNextPageCommand, GoLastPageCommand,
                           MoveDownCommand);

        _ = LoadAsync();
    }

    private CancellationTokenSource? _cts;

    private const string COLOR_ROJO = "DangerButtonStyle";
    private const string COLOR_PRIMARIO = "PrimaryButtonStyle";


    [ObservableProperty] private ObservableCollection<MetodoPagoDto> metodosPago = new();
    [ObservableProperty] private MetodoPagoDto? metodoPagoSeleccionado;
    [ObservableProperty] private string textoFiltro = string.Empty;

    public ObservableCollection<TipoAjusteSaldo> TiposAjusteE { get; } = new(Enum.GetValues(typeof(TipoAjusteSaldo)).Cast<TipoAjusteSaldo>());

    public ObservableCollection<string> Estados { get; } = new(new[] { "Todos", "Activo", "Inactivo" });

    [ObservableProperty] private string estadoSeleccionado = "Todos";
    partial void OnTextoFiltroChanged(string value)
      => _ = DebouncedReloadAsync();

    partial void OnEstadoSeleccionadoChanged(string value)
        => _ = DebouncedReloadAsync();

    private StatusFilter EstadoEnum => EstadoSeleccionado switch
    {
        "Activo" => StatusFilter.Activo,
        "Inactivo" => StatusFilter.Inactivo,
        _ => StatusFilter.Todos
    };

    public ObservableCollection<int> PageSizes { get; } = new(new[] { 10, 25, 50, 100 });

    [ObservableProperty] private int pageSize = 25;
    partial void OnPageSizeChanged(int value)
    {
        PageNumber = 1;
        _ = LoadAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    [NotifyCanExecuteChangedFor(nameof(GoFirstPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoPrevPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoLastPageCommand))]
    private int pageNumber = 1;

    [ObservableProperty] private int totalCount;
    partial void OnTotalCountChanged(int value)
    {
        OnPropertyChanged(nameof(DisplayFrom));
        OnPropertyChanged(nameof(DisplayTo));
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoLastPageCommand))]
    private int totalPages = 1;

    public int DisplayFrom => Math.Min(((PageNumber - 1) * PageSize) + 1, Math.Max(1, TotalCount));
    public int DisplayTo => Math.Min(PageNumber * PageSize, TotalCount);

    private bool CanSimpleAction() => !IsBusy;
    private bool CanGoFirst() => !IsBusy && PageNumber > 1;
    private bool CanGoPrev() => !IsBusy && PageNumber > 1;
    private bool CanGoNext() => !IsBusy && PageNumber < TotalPages;
    private bool CanGoLast() => !IsBusy && PageNumber < TotalPages;


    private async Task DebouncedReloadAsync(int delayMs = 300)
    {
        var cts = new CancellationTokenSource();
        _cts?.Cancel();
        _cts = cts;

        try
        {
            await Task.Delay(delayMs, cts.Token);
            if (_cts != cts) return;

            PageNumber = 1;
            await LoadAsync(cts.Token);
        }
        catch (OperationCanceledException) { /* ignore */ }
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void NuevoMetodoPago()
    {
        MetodoPagoSeleccionado = new();
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task Guardar(CancellationToken ct = default)
    {
        if (MetodoPagoSeleccionado is null)
            return;

        int? idToKeep = MetodoPagoSeleccionado.Id == 0 ? null : MetodoPagoSeleccionado.Id;

        if (MetodoPagoSeleccionado.Id == 0)
        {
            var nuevoMetodoPago = MetodoPagoSeleccionado.ToMetodoPagoCreateDTO();
            var result = await _createMetodoPagoInteractor.ExecuteAsync(nuevoMetodoPago, ct);
            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }
            idToKeep = result.Value.Id;
        }
        else
        {
            var result = await _updateMetodoPagoInteractor.ExecuteAsync(MetodoPagoSeleccionado, ct);
            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }
        }

        await ReloadKeepingSelectionAsync(idToKeep, ct);
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task Eliminar(CancellationToken ct = default)
    {
        if (MetodoPagoSeleccionado is null)
            return;

        int? idToKeep = MetodoPagoSeleccionado.Id == 0 ? null : MetodoPagoSeleccionado.Id;

        if (MetodoPagoSeleccionado.Id == 0)
        {
            ErrorMessage = "Debe seleccionar un Metodo de Pago.";
            return;
        }

        var result = await _deleteMetodoPagoInteractor.ExecuteAsync(MetodoPagoSeleccionado, ct);
        if (!result.IsSuccess)
        {
            ErrorMessage = string.Join(Environment.NewLine, result.Errors);
            return;
        }

        await ReloadKeepingSelectionAsync(preferId: null, ct);
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            ErrorMessage = null;
            IsBusy = true;

            var req = new GetMetodoPagoRequest
            {
                Texto = string.IsNullOrWhiteSpace(TextoFiltro) ? null : TextoFiltro.Trim(),
                Status = EstadoEnum,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var result = await _getMetodoPagoInteractor.ExecuteAsync(req, ct);

            MetodosPago.Clear();
            foreach (var metodo in result.Items) MetodosPago.Add(metodo);

            TotalCount = result.TotalCount;
            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            if (PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
                await LoadAsync(CancellationToken.None);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar Metodos de pago: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MoveDown(CancellationToken ct = default)
    {
        try
        {
            if (MetodoPagoSeleccionado is null || MetodoPagoSeleccionado.Id == 0)
            {
                ErrorMessage = "Debe seleccionar un Metodo de Pago.";
                return;
            }

            int? idToKeep = MetodoPagoSeleccionado.Id == 0 ? null : MetodoPagoSeleccionado.Id;

            var result = await _moveDownInteractor.ExecuteAsync(MetodoPagoSeleccionado, ct);

            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }

            await ReloadKeepingSelectionAsync(idToKeep, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar Metodos de pago: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MoveUp(CancellationToken ct = default)
    {
        try
        {
            if (MetodoPagoSeleccionado is null || MetodoPagoSeleccionado.Id == 0)
            {
                ErrorMessage = "Debe seleccionar un Metodo de Pago.";
                return;
            }

            int? idToKeep = MetodoPagoSeleccionado.Id == 0 ? null : MetodoPagoSeleccionado.Id;

            var result = await _moveUpInteractor.ExecuteAsync(MetodoPagoSeleccionado, ct);

            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }

            await ReloadKeepingSelectionAsync(idToKeep, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar Metodos de pago: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadKeepingSelectionAsync(int? preferId = null, CancellationToken ct = default)
    {
        var idToKeep = preferId ?? MetodoPagoSeleccionado?.Id;

        await LoadAsync(ct);

        if (idToKeep is null || idToKeep.Value == 0)
        {
            MetodoPagoSeleccionado = MetodosPago.FirstOrDefault();
            return;
        }

        var found = MetodosPago.FirstOrDefault(x => x.Id == idToKeep.Value);
        MetodoPagoSeleccionado = found ?? MetodosPago.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void LimpiarFiltro()
    {
        TextoFiltro = string.Empty;
        EstadoSeleccionado = "Todos";
    }

    // --------- Navegación (paginación) ----------
    [RelayCommand(CanExecute = nameof(CanGoFirst))]
    private async Task GoFirstPageAsync()
    {
        if (PageNumber > 1)
        {
            PageNumber = 1;
            await LoadAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoPrev))]
    private async Task GoPrevPageAsync()
    {
        if (PageNumber > 1)
        {
            PageNumber--;
            await LoadAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoNextPageAsync()
    {
        if (PageNumber < TotalPages)
        {
            PageNumber++;
            await LoadAsync(CancellationToken.None);
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoLast))]
    private async Task GoLastPageAsync()
    {
        if (PageNumber < TotalPages)
        {
            PageNumber = TotalPages;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private void ClearError() => ErrorMessage = null;
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
