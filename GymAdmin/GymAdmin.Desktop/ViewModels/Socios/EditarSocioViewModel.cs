using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Applications.Interactor.AsistenciaInteractors;
using GymAdmin.Applications.Interactor.SociosInteractors;
using GymAdmin.Desktop.ViewModels.Asistencia;
using GymAdmin.Desktop.ViewModels.Dialogs;
using GymAdmin.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace GymAdmin.Desktop.ViewModels.Socios;

public partial class EditarSocioViewModel : ViewModelBase
{
    private const string COLOR_ROJO = "DangerButtonStyle";
    private const string COLOR_PRIMARIO = "PrimaryButtonStyle";

    private readonly IUpdateSocioInteractor _updateSocio;
    private readonly IGetAsistenciasBySocioInteractor _getAsistencias;
    private readonly IDeleteAsistenciaInteractor _deleteAsistencia;
    private readonly ICreateAsistenciaInteractor _createAsistencia;
    private readonly IGetSocioByIdInteractor _getSocioById;
    private readonly IUpdateAsistenciaInteractor _updateAsistencia;

    private CancellationTokenSource? _cts;
    private readonly IServiceProvider _sp;

    public event Action? CloseRequested;

    [ObservableProperty] private int id;
    [ObservableProperty] private string dni = "";
    [ObservableProperty] private string nombre = "";
    [ObservableProperty] private string apellido = "";
    [ObservableProperty] private string estado = "Inactivo";
    [ObservableProperty] private int creditosRestantes;
    [ObservableProperty] private DateTime? expiracionMembresiaLocal;
    [ObservableProperty] private string vigenciaTexto = "";
    [ObservableProperty] private string ultimoPagoTexto;
    [ObservableProperty] private int totalCreditosComprados;

    // --- Paginación asistencias ---
    public ObservableCollection<AsistenciaRow> Asistencias { get; } = new();
    public ObservableCollection<int> PageSizes { get; } = new(new[] { 10, 25, 50, 100 });

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
    private int pageSize = 10;

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


    [ObservableProperty] private bool isInnerDialogOpen;
    [ObservableProperty] private object? innerDialogContent;

    private void OpenInnerDialog(object content)
    {
        InnerDialogContent = content;
        IsInnerDialogOpen = true;
    }
    private void CloseInnerDialog()
    {
        IsInnerDialogOpen = false;
        InnerDialogContent = null;
    }

    public EditarSocioViewModel(
        IUpdateSocioInteractor updateSocio,
        IGetAsistenciasBySocioInteractor getAsistencias,
        IServiceProvider sp,
        ICreateAsistenciaInteractor createAsistencia,
        IGetSocioByIdInteractor getSocioById,
        IDeleteAsistenciaInteractor deleteAsistencia
,
        IUpdateAsistenciaInteractor updateAsistencia)
    {
        _getSocioById = getSocioById;
        _updateSocio = updateSocio;
        _getAsistencias = getAsistencias;
        _createAsistencia = createAsistencia;
        _deleteAsistencia = deleteAsistencia;

        _ = LoadAsistenciasAsync();
        _sp = sp;

        BindBusyToCommands(GoFirstPageCommand, GoPrevPageCommand, GoNextPageCommand,
                           GoLastPageCommand);
        _updateAsistencia = updateAsistencia;
    }

    public async Task LoadAsync(SocioDto socio, CancellationToken ct = default)
    {
        Id = socio.Id;
        Dni = socio.Dni;
        Nombre = socio.Nombre;
        Apellido = socio.Apellido;
        Estado = socio.Estado;
        CreditosRestantes = socio.CreditosRestantes;
        VigenciaTexto = socio.VigenciaTexto ?? "";
        TotalCreditosComprados = socio.TotalCreditosComprados;
        if (!socio.UltimoPagoTexto.Equals("-"))
            socio.UltimoPagoTexto = string.Concat(
                socio.UltimoPagoTexto,
                " - Plan: ",
                socio.PlanNombre
            );

        UltimoPagoTexto = socio.UltimoPagoTexto ?? "";

        PageNumber = 1;
        await LoadAsistenciasAsync(ct);
    }
    
    private async Task ReloadSocioHeaderAsync(CancellationToken ct = default)
    {
        var result = await _getSocioById.ExecuteAsync(Id, ct);
        var socio = result.Value;

        Dni = socio.Dni;
        Nombre = socio.Nombre;
        Apellido = socio.Apellido;
        Estado = socio.Estado;
        CreditosRestantes = socio.CreditosRestantes;
        VigenciaTexto = socio.VigenciaTexto ?? "";
        TotalCreditosComprados = socio.TotalCreditosComprados;
        UltimoPagoTexto = socio.UltimoPagoTexto ?? "";
    }

    private async Task RefreshSocioAndAsistenciasAsync(bool goFirstPage = false, CancellationToken ct = default)
    {
        if (goFirstPage) PageNumber = 1;

        // Secuencial para no pelear con IsBusy interno de LoadAsistenciasAsync
        await ReloadSocioHeaderAsync(ct);
        await LoadAsistenciasAsync(ct);
    }

    private async Task LoadAsistenciasAsync(CancellationToken ct = default)
    {
        try
        {
            IsBusy = true; ErrorMessage = null;

            var req = new GetAsistenciasBySocioRequest
            {
                SocioId = Id,
                PageNumber = PageNumber,
                PageSize = PageSize
                // si querés, podés agregar filtros por fecha
            };

            var resp = await _getAsistencias.ExecuteAsync(req, ct);

            Asistencias.Clear();
            foreach (var a in resp.Items)
                Asistencias.Add(AsistenciaRow.FromDto(a));

            TotalCount = resp.TotalCount;
            TotalPages = Math.Max(1, resp.TotalPages);

            if (PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
                await LoadAsistenciasAsync(CancellationToken.None);
            }
        }
        finally { IsBusy = false; }
    }

    // --- Acciones socio ---
    [RelayCommand] private void Cancelar() => CloseRequested?.Invoke();

    [RelayCommand]
    private async Task GuardarAsync()
    {
        try
        {
            IsBusy = true; ErrorMessage = null;

            var cmd = new SocioUpdateDto
            {
                Id = Id,
                Dni = Dni,
                Nombre = Nombre,
                Apellido = Apellido
            };

            var result = await _updateSocio.ExecuteAsync(cmd, CancellationToken.None);
            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }

            CloseRequested?.Invoke();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ClearError() => ErrorMessage = null;

    // --- Paginación comandos ---
    [RelayCommand(CanExecute = nameof(CanGoFirst))]
    private async Task GoFirstPage() { if (PageNumber <= 1) return; PageNumber = 1; await LoadAsistenciasAsync(); }
    private bool CanGoFirst() => !IsBusy && PageNumber > 1;

    [RelayCommand(CanExecute = nameof(CanGoPrev))]
    private async Task GoPrevPage() { if (PageNumber <= 1) return; PageNumber--; await LoadAsistenciasAsync(); }
    private bool CanGoPrev() => !IsBusy && PageNumber > 1;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoNextPage() { if (PageNumber >= TotalPages) return; PageNumber++; await LoadAsistenciasAsync(); }
    private bool CanGoNext() => !IsBusy && PageNumber < TotalPages;

    [RelayCommand(CanExecute = nameof(CanGoLast))]
    private async Task GoLastPage() { if (PageNumber >= TotalPages) return; PageNumber = TotalPages; await LoadAsistenciasAsync(); }
    private bool CanGoLast() => !IsBusy && PageNumber < TotalPages;

    partial void OnPageSizeChanged(int value)
    {
        PageNumber = 1;
        _ = LoadAsistenciasAsync();
    }

    // --- Acciones de asistencias ---
    [RelayCommand]
    private async Task NuevaAsistenciaAsync()
    {
        ErrorMessage = null;

        if (Estado != "Activo")
        {
            await ShowConfirmAsync(
                "Registrar Asistencia",
                $"No se puede registrar la asistencia de un socio inactivo.\n" +
                $"Por favor, primero activá el estado del socio \"{Apellido} {Nombre}\".",
                "Aceptar",
                string.Empty,
                COLOR_PRIMARIO,COLOR_ROJO,false);
            return;
        }
        if (CreditosRestantes <= 0)
        {
            await ShowConfirmAsync(
                "Registrar Asistencia",
                $"El socio \"{Apellido} {Nombre}\" no tiene créditos disponibles.\n" +
                $"Por favor, primero cargá créditos al socio.",
                "Aceptar",
                string.Empty,
                COLOR_PRIMARIO, COLOR_ROJO, false);
            return;
        }

        var res = await ShowAsistenciaEditorAsync(null, null, "Nueva asistencia", "Crear");
        if (res is null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            IsBusy = true;

            var dto = new CreateAsistenciaDto
            {
                IdSocio = Id,
                Fecha = res.EntradaUtc,
                Observaciones = string.IsNullOrWhiteSpace(res.Observaciones) ? null : res.Observaciones,
            };

            var result = await _createAsistencia.ExecuteAsync(dto, ct);
            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }

            // Volver a cargar socio + asistencias
            await RefreshSocioAndAsistenciasAsync(goFirstPage: true, ct);
        }
        catch (OperationCanceledException) { /* nada */ }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditarAsistenciaAsync(AsistenciaRow? row)
    {
        if (row is null) return;
        ErrorMessage = null;

        // Abrir editor precargado
        var res = await ShowAsistenciaEditorAsync(
            row.EntradaLocal,
            row.Observaciones == "—" ? "" : row.Observaciones,
            "Editar asistencia",
            "Guardar"
        );
        if (res is null) return; // cancelado

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        var bkEntrada = row.EntradaLocal;
        var bkObs = row.Observaciones;

        try
        {
            IsBusy = true;
            row.EntradaLocal = res.EntradaLocal;
            row.Observaciones = string.IsNullOrWhiteSpace(res.Observaciones) ? "—" : res.Observaciones;

            var upd = new AsistenciaDto
            {
                Id = row.Id,
                Entrada = res.EntradaUtc,      
                SocioId = Id,               
                Observaciones = res.Observaciones?.Trim() ?? ""
            };

            var result = await _updateAsistencia.ExecuteAsync(upd, ct);
            if (!result.IsSuccess)
            {
                row.EntradaLocal = bkEntrada;
                row.Observaciones = bkObs;
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }
            await LoadAsistenciasAsync(ct);
        }
        catch (OperationCanceledException) { /* nada */ }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EliminarAsistenciaAsync(AsistenciaRow? row)
    {
        ErrorMessage = null;
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        if (row is null) return;

        var ok = await ShowConfirmAsync(
            "Eliminar Asistencia",
            $"¿Estás seguro de eliminar la asistencia del {row.EntradaLocal:dd/MM/yyyy HH:mm}?\n" +
            $"Esta acción no se puede deshacer.",
            accept: "Eliminar",
            cancel: "Cancelar");
        if (!ok) return;

        try
        {
            var request = new DeleteAsistenciaDto { Id = row.Id, SocioId = Id };
            var result = await _deleteAsistencia.ExecuteAsync(request, CancellationToken.None);
            if (result.IsSuccess)
                Asistencias.Remove(row);
            else
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);

            await RefreshSocioAndAsistenciasAsync(goFirstPage: true, ct);
        }
        catch (OperationCanceledException) { /* nada */ }
        finally { IsBusy = false; }


    }

    // --- Row VM ---
    public partial class AsistenciaRow : ObservableObject
    {
        [ObservableProperty] private int id;
        [ObservableProperty] private DateTime entradaLocal;
        [ObservableProperty] private bool seUsoCredito;
        [ObservableProperty] private string observaciones = "";

        public static AsistenciaRow FromDto(AsistenciaDto dto) => new()
        {
            Id = dto.Id,
            EntradaLocal = ToLocal(dto.Entrada),
            SeUsoCredito = dto.SeUsoCredito,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? "—" : dto.Observaciones
        };

        public AsistenciaDto ToUpdateDto() => new()
        {
            Id = Id,
            Entrada = FromLocalToUtc(EntradaLocal),
            SeUsoCredito = SeUsoCredito,
            Observaciones = Observaciones == "—" ? "" : Observaciones
        };

        private static DateTime ToLocal(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Local) return dt;
            var utc = dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return utc.ToLocalTime();
        }
        private static DateTime FromLocalToUtc(DateTime local)
        {
            if (local.Kind == DateTimeKind.Utc) return local;
            return DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();
        }
    }

    private async Task<AsistenciaEditorResult?> ShowAsistenciaEditorAsync(
    DateTime? entradaLocal, string? observ, string title, string accept)
    {
        var vm = new EditAsistenciaDialogViewModel();
        vm.Initialize(entradaLocal, observ, title, accept);

        var view = new EditAsistenciaDialog { DataContext = vm };
        OpenInnerDialog(view);

        var res = await vm.Task;
        CloseInnerDialog();
        return res;
    }

    private async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Eliminar", string cancel = "Cancelar", string acceptColor = COLOR_ROJO, string cancelColor = COLOR_PRIMARIO, bool showCancel = true)
    {
        var vm = new ConfirmDialogViewModel
        {
            Title = title,
            Message = message,
            AcceptText = accept,
            CancelText = cancel,
            AcceptButtonStyle = (Style)Application.Current.FindResource(acceptColor),
            CancelButtonStyle = (Style)Application.Current.FindResource(cancelColor),
            ShowCancelButton = showCancel
        };

        var view = new ConfirmDialogView { DataContext = vm };

        // Mostrar overlay
        InnerDialogContent = view;
        IsInnerDialogOpen = true;

        // Esperar la selección del usuario
        var ok = await vm.Task;

        // Cerrar overlay
        IsInnerDialogOpen = false;
        innerDialogContent = null;

        return ok;
    }
}