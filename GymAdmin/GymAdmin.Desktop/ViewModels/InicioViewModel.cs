using GymAdmin.Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace GymAdmin.Desktop.ViewModels;

public class InicioViewModel : ViewModelBase
{
    private object _currentView;
    private NavigationItem _selectedMenuItem;

    public ObservableCollection<NavigationItem> MenuItems { get; set; }

    public object CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public NavigationItem SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            SetProperty(ref _selectedMenuItem, value);
            ChangeView(value?.ViewName);
        }
    }

    public RelayCommand ExitCommand { get; }

    // ... resto del código ...

    public InicioViewModel()
    {
        // Inicializar comandos
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());

        // Inicializar menú
        MenuItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem("Dashboard", "DashboardView"),
                new NavigationItem("Miembros", "MembersView"),
                new NavigationItem("Pagos", "PaymentsView"),
                new NavigationItem("Membresías", "MembershipsView"),
                new NavigationItem("Configuración", "SettingsView")
            };

        // Vista inicial
        SelectedMenuItem = MenuItems[0]; // Dashboard
    }

    private void ChangeView(string viewName)
    {
        if (string.IsNullOrEmpty(viewName)) return;

        try
        {
            // Crear instancia de la vista usando reflexión
            var viewType = Type.GetType($"GymAdmin.Views.{viewName}");
            if (viewType != null)
            {
                CurrentView = Activator.CreateInstance(viewType);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando la vista: {ex.Message}");
        }
    }
}
