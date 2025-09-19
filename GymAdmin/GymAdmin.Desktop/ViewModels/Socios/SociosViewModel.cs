using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Applications.Interactor.AsistenciaInteractors;
using GymAdmin.Applications.Interactor.SociosInteractors;
using GymAdmin.Desktop.ViewModels.Dialogs;
using GymAdmin.Desktop.ViewModels.Pagos;
using GymAdmin.Desktop.Views.Dialogs;
using GymAdmin.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace GymAdmin.Desktop.ViewModels.Socios;

public sealed partial class SociosViewModel : ViewModelBase, IDisposable
{
    private readonly IGetAllSociosInteractor _getAllSociosInteractor;
    private readonly IDeleteSocioInteractor _deleteSocioInteractor;
    private readonly ICreateAsistenciaInteractor _createAsistenciaInteractor;

    private CancellationTokenSource? _cts;
    private readonly IServiceProvider _sp;

    public ObservableCollection<SocioDto> Socios { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } =
        new(new[] { "Todos", "Activo", "Inactivo" });
    public ObservableCollection<int> PageSizes { get; } =
        new(new[] { 10, 25, 50, 100 });

    // --------- Props observables (locales) ----------
    [ObservableProperty]
    private string filtroBusqueda = string.Empty;

    [ObservableProperty]
    private string selectedStatusFilter = "Todos";

    [ObservableProperty]
    private string sortBy = "NombreCompleto";

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarSocioCommand))]
    [NotifyCanExecuteChangedFor(nameof(EliminarSocioCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegistrarPagoCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegistrarAsistenciaCommand))]
    private SocioDto? socioSeleccionado;
    partial void OnSocioSeleccionadoChanged(SocioDto? value)
    {

    }

    [ObservableProperty]
    private object? dialogContent;

    [ObservableProperty]
    private bool isDialogOpen;

    public SociosViewModel(IGetAllSociosInteractor getAll,
        IServiceProvider sp,
        IDeleteSocioInteractor deleteSocioInteractor,
        ICreateAsistenciaInteractor createAsistenciaInteractor)
    {
        _getAllSociosInteractor = getAll;
        _deleteSocioInteractor = deleteSocioInteractor;

        // Estado inicial
        PageSize = 25;
        PageNumber = 1;

        _ = LoadAsync();
        _sp = sp;

        BindBusyToCommands(LoadCommand, GoFirstPageCommand, GoPrevPageCommand, GoNextPageCommand,
                           GoLastPageCommand, NuevoSocioCommand, EditarSocioCommand,
                           EliminarSocioCommand, RegistrarPagoCommand, RegistrarAsistenciaCommand,
                           LimpiarFiltroCommand, ExportarCommand);
        _createAsistenciaInteractor = createAsistenciaInteractor;
    }

    partial void OnFiltroBusquedaChanged(string value) => _ = DebouncedReloadAsync();
    partial void OnSelectedStatusFilterChanged(string value) => _ = DebouncedReloadAsync();
    partial void OnSortByChanged(string value) => _ = LoadAsync();
    partial void OnSortDescChanged(bool value) => _ = LoadAsync();
    partial void OnPageSizeChanged(int value) { PageNumber = 1; _ = LoadAsync(); }

    private StatusFilter SelectedStatusEnum =>
        SelectedStatusFilter switch
        {
            "Activo" => StatusFilter.Activo,
            "Inactivo" => StatusFilter.Inactivo,
            _ => StatusFilter.Todos
        };

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        var token = ct == default ? (_cts?.Token ?? CancellationToken.None) : ct;

        try
        {
            ErrorMessage = null;
            IsBusy = true;

            var req = new GetSociosRequest
            {
                Texto = string.IsNullOrWhiteSpace(FiltroBusqueda) ? null : FiltroBusqueda.Trim(),
                Status = SelectedStatusEnum,
                PageNumber = PageNumber,
                PageSize = PageSize,
                SortBy = SortBy,
                SortDesc = SortDesc
            };

            var result = await _getAllSociosInteractor.ExecuteAsync(req, token);

            Socios.Clear();
            foreach (var s in result.Items) Socios.Add(s);

            TotalCount = result.TotalCount;
            TotalPages = Math.Max(1, result.TotalPages);

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
            ErrorMessage = $"Error al cargar socios: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    private bool CanLoad() => !IsBusy;

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

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void NuevoSocio()
    {
        var dialogVm = _sp.GetRequiredService<AddSocioViewModel>();
        dialogVm.CloseRequested += async () =>
        {
            IsDialogOpen = false;
            DialogContent = null;
            await LoadAsync();
        };

        OpenDialog(new AddSocioDialog { DataContext = dialogVm });
    }

    private void OpenDialog(object content)
    {
        DialogContent = content;
        IsDialogOpen = true;
    }

    private bool CanSimpleAction() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRowAction))]
    private async Task EditarSocio()
    {
        /* TODO */
    }


    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task EliminarSocio()
    {
        if (SocioSeleccionado is null) return;

        var confirm = await ShowConfirmAsync(
         "Eliminar Socio",
         $"¿Querés eliminar al socio \"{SocioSeleccionado.NombreCompleto}\"? \n",
         accept: "Eliminar",
         cancel: "Cancelar");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var result = await _deleteSocioInteractor.ExecuteAsync(SocioSeleccionado, CancellationToken.None);
            if (result.IsSuccess)
                await LoadAsync();
            else
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void RegistrarPago()
    {
        var vm = _sp.GetRequiredService<AddPagoViewModel>();
     
        SocioLookupDto? socio = null;
        
        if (socioSeleccionado != null)
        {
            socio = new SocioLookupDto
            {
                Dni = SocioSeleccionado?.Dni,
                Id = SocioSeleccionado?.Id ?? 0,
                NombreCompleto = SocioSeleccionado?.NombreCompleto
            };
        }

        vm.Initialize(socio);

        vm.CloseRequested += async () =>
        {
            IsDialogOpen = false; DialogContent = null;
            await LoadAsync();
        };

        OpenDialog(new AddPagoDialog { DataContext = vm });
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task RegistrarAsistencia() 
    { 
        if (SocioSeleccionado is null) return;

        if (SocioSeleccionado.Estado != "Activo")
        {
            await ShowConfirmAsync(
                "Registrar Asistencia",
                $"No se puede registrar la asistencia de un socio inactivo.\n" +
                $"Por favor, primero activá el estado del socio \"{SocioSeleccionado.NombreCompleto}\".",
                accept: "Aceptar",
                cancel: "Cancelar");
            return;
        }
        if (SocioSeleccionado.CreditosRestantes <= 0)
        {
            await ShowConfirmAsync(
                "Registrar Asistencia",
                $"El socio \"{SocioSeleccionado.NombreCompleto}\" no tiene créditos disponibles.\n" +
                $"Por favor, primero cargá créditos al socio.",
                accept: "Aceptar",
                cancel: "Cancelar");
            return;
        }

        var confirm = await ShowConfirmAsync(
        "Registrar Asistencia",
        $"¿Querés registrar una asistencia de \"{SocioSeleccionado.NombreCompleto}\"? \n",
        accept: "Registrar",
        cancel: "Cancelar");

        if (!confirm) return;
        try
        {
            IsBusy = true;
            var asisitenciaDto = new CreateAsistenciaDto
            {
                IdSocio = SocioSeleccionado.Id,
                Fecha = DateTime.Now
            };

            var result = await _createAsistenciaInteractor.ExecuteAsync(asisitenciaDto, CancellationToken.None);
            if (result.IsSuccess)
                await LoadAsync();
            else
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRowAction() => !IsBusy && SocioSeleccionado is not null;

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void LimpiarFiltro()
    {
        FiltroBusqueda = string.Empty;
        SelectedStatusFilter = "Todos";
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void Exportar() { /* TODO */ }

    public async Task ApplySortAsync(string columnName, bool desc)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return;
        SortBy = columnName;
        SortDesc = desc;
        await LoadAsync();
    }

    // Debounce
    private async Task DebouncedReloadAsync(int delayMs = 300)
    {
        _cts?.Cancel();
        var cts = new CancellationTokenSource();
        _cts = cts;

        try
        {
            await Task.Delay(delayMs, cts.Token);
            PageNumber = 1;
            await LoadAsync(cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Eliminar", string cancel = "Cancelar")
    {
        var vm = new ConfirmDialogViewModel
        {
            Title = title,
            Message = message,
            AcceptText = accept,
            CancelText = cancel,
            AcceptButtonStyle = App.Current.FindResource("DangerButtonStyle") as System.Windows.Style,
            CancelButtonStyle = App.Current.FindResource("PrimaryButtonStyle") as System.Windows.Style
        };

        var view = new ConfirmDialogView { DataContext = vm };

        // Mostrar overlay
        DialogContent = view;
        IsDialogOpen = true;

        // Esperar la selección del usuario
        var ok = await vm.Task;

        // Cerrar overlay
        IsDialogOpen = false;
        DialogContent = null;

        return ok;
    }
}