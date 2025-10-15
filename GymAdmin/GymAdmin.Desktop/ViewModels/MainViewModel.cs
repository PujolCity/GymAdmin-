using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.Sharp;
using GymAdmin.Desktop.ViewModels.Configuracion;
using GymAdmin.Desktop.ViewModels.Membresias;
using GymAdmin.Desktop.ViewModels.Pagos;
using GymAdmin.Desktop.ViewModels.Socios;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;

    [ObservableProperty] private ViewModelBase _currentChildView;
    [ObservableProperty] private string _caption;
    [ObservableProperty] private IconChar _icon;

    public MainViewModel(IServiceProvider sp)
    {
        _sp = sp;
        ShowInicioView();
    }

    [RelayCommand]
    private void ShowInicioView()
    {
        CurrentChildView = _sp.GetRequiredService<InicioViewModel>();
        Caption = "Inicio";
        Icon = IconChar.Home;
    }

    [RelayCommand]
    private void ShowSociosView()
    {
        CurrentChildView = _sp.GetRequiredService<SociosViewModel>();
        Caption = "Socios";
        Icon = IconChar.UserCircle;
    }

    [RelayCommand]
    private void ShowPagosView()
    {
        CurrentChildView = _sp.GetRequiredService<PagosViewModel>();
        Caption = "Pagos";
        Icon = IconChar.CreditCard;
    }

    [RelayCommand]
    private void ShowConfigView()
    {
        CurrentChildView = _sp.GetRequiredService<ConfigViewModel>();
        Caption = "Configuración";
        Icon = IconChar.Cog;
    }

    [RelayCommand]
    private void ShowPlanesView()
    {
        CurrentChildView = _sp.GetRequiredService<PlanesMembresiaViewModel>();
        Caption = "Planes de Membresía";
        Icon = IconChar.ListAlt;
    }
}