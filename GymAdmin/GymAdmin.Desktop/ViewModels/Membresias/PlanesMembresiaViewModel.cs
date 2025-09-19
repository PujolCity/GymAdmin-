using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Applications.Interactor.PlanesMembresia;
using GymAdmin.Desktop.ViewModels.Dialogs;
using GymAdmin.Desktop.Views.Dialogs;
using GymAdmin.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace GymAdmin.Desktop.ViewModels.Membresias;

public partial class PlanesMembresiaViewModel : ViewModelBase, IDisposable
{
    private readonly IGetPlanesMembresiaInteractor _getPlanes;
    private readonly ICreateOrUpdatePlanInteractor _upsert;
    private readonly IDeletePlanMembresiaInteractor _deletePlan;
    private readonly IServiceProvider _sp;

    private CancellationTokenSource? _cts;

    [ObservableProperty] private object? dialogContent;
    [ObservableProperty] private bool isDialogOpen;

    public ObservableCollection<PlanMembresiaDto> Planes { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarPlanCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClonarPlanCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePlanCommand))]
    private PlanMembresiaDto? planSeleccionado;
    partial void OnPlanSeleccionadoChanged(PlanMembresiaDto? value)
    { }

    [ObservableProperty] private string filtro = string.Empty;

    partial void OnFiltroChanged(string value) => _ = DebouncedReloadAsync();

    public ObservableCollection<string> Estados { get; } = new(new[] { "Todos", "Activo", "Inactivo" });

    [ObservableProperty] private string estadoSeleccionado = "Todos";

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

    public PlanesMembresiaViewModel(
        IGetPlanesMembresiaInteractor getPlanes,
        IDeletePlanMembresiaInteractor deletePlan,
        IServiceProvider serviceProvider,
        ICreateOrUpdatePlanInteractor upsert)
    {
        _getPlanes = getPlanes;
        _upsert = upsert;
        _deletePlan = deletePlan;
        _sp = serviceProvider;

        BindBusyToCommands(LoadCommand, NuevoPlanCommand,
            EditarPlanCommand, ClonarPlanCommand,
            TogglePlanCommand, LimpiarFiltroCommand,
            GoFirstPageCommand, GoPrevPageCommand,
            GoNextPageCommand, GoLastPageCommand);

        _ = LoadAsync();
    }

    // --------- Carga / Debounce ----------
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
    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            ErrorMessage = null;
            IsBusy = true;

            var req = new GetPlanesRequest
            {
                Texto = string.IsNullOrWhiteSpace(Filtro) ? null : Filtro.Trim(),
                Status = EstadoEnum,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var result = await _getPlanes.ExecuteAsync(req, ct);

            Planes.Clear();
            foreach (var p in result.Items)
                Planes.Add(p);

            TotalCount = result.TotalCount;
            TotalPages = Math.Max(1, result.TotalPages);

            if (PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
                await LoadAsync(_cts.Token);
                return;
            }

            OnPropertyChanged(nameof(DisplayFrom));
            OnPropertyChanged(nameof(DisplayTo));
        }
        catch (OperationCanceledException) { /* navegación/tecleo rápido */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar planes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void NuevoPlan()
    {
        var vm = _sp.GetRequiredService<AddEditPlanViewModel>();
        vm.CloseRequested += async () =>
        {
            IsDialogOpen = false;
            DialogContent = null;
            await LoadAsync();
        };

        OpenDialog(new AddEditPlanDialog { DataContext = vm });
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void EditarPlan()
    {
        if (PlanSeleccionado is null) return;

        var vm = _sp.GetRequiredService<AddEditPlanViewModel>();
        vm.CargarParaEdicion(PlanSeleccionado);
        vm.CloseRequested += async () =>
        {
            IsDialogOpen = false;
            DialogContent = null;
            await LoadAsync();
        };

        OpenDialog(new AddEditPlanDialog { DataContext = vm });
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void ClonarPlan()
    {
        if (PlanSeleccionado is null) return;

        var vm = _sp.GetRequiredService<AddEditPlanViewModel>();
        vm.CargarParaEdicion(new PlanMembresiaDto
        {
            Id = 0,
            Nombre = $"{PlanSeleccionado.Nombre} (copia)",
            Descripcion = PlanSeleccionado.Descripcion,
            Creditos = PlanSeleccionado.Creditos,
            DiasValidez = PlanSeleccionado.DiasValidez,
            Precio = PlanSeleccionado.Precio,
            IsActive = PlanSeleccionado.IsActive
        });

        vm.CloseRequested += async () =>
        {
            IsDialogOpen = false;
            DialogContent = null;
            await LoadAsync();
        };

        OpenDialog(new AddEditPlanDialog { DataContext = vm });
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private async Task EliminarPlanAsync()
    {
        if (PlanSeleccionado is null) return;

        var confirm = await ShowConfirmAsync(
            "Eliminar plan",
            $"¿Querés eliminar el plan \"{PlanSeleccionado.Nombre}\"? \n",
            accept: "Eliminar",
            cancel: "Cancelar");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var result = await _deletePlan.ExecuteAsync(PlanSeleccionado, CancellationToken.None);
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
    private async Task TogglePlanAsync()
    {
        if (PlanSeleccionado is null) return;

        var previous = PlanSeleccionado.IsActive;

        try
        {
            IsBusy = true;
            var result = await _upsert.ExecuteAsync(PlanSeleccionado, CancellationToken.None);
            if (!result.IsSuccess)
            {
                PlanSeleccionado.IsActive = previous;
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
            }
            else
            {
                await LoadAsync();
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
        Filtro = string.Empty;
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

    // --------- Helpers UI ----------
    private void OpenDialog(object content)
    {
        DialogContent = content;
        IsDialogOpen = true;
    }

    // --------- Dispose ----------
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