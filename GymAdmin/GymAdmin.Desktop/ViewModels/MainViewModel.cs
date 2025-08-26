using FontAwesome.Sharp;
using System.Windows.Input;

namespace GymAdmin.Desktop.ViewModels;

class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentChildView;
    private string _caption;
    private IconChar _icon;

    public ViewModelBase CurrentChildView
    {
        get => _currentChildView;
        set
        {
            _currentChildView = value;
            OnPropertyChanged(nameof(CurrentChildView));
        }
    }

    public string Caption
    {
        get => _caption;
        set
        {
            _caption = value;
            OnPropertyChanged(nameof(Caption));
        }
    }

    public IconChar Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged(nameof(Icon));
        }
    }

    public ICommand ShowInicioViewCommand { get; }
    public ICommand ShowSociosViewCommand { get; }
    public ICommand ShowMembresiasViewCommand { get; }
    public ICommand ShowPagosViewCommand { get; }
    public ICommand ShowConfigViewCommand { get; }

    public MainViewModel()
    {
        ShowInicioViewCommand = new RelayCommand(ExecuteShowInicioViewCommand);
        ShowSociosViewCommand = new RelayCommand(ExecuteShowSociosViewCommand);
        ShowMembresiasViewCommand = new RelayCommand(ExecuteShowMembresiasViewCommand);
        ShowConfigViewCommand = new RelayCommand(ExecuteShowConfigViewCommand);
        ShowPagosViewCommand = new RelayCommand(ExecuteShowPagosViewCommand);

        ExecuteShowInicioViewCommand();
    }

    private void ExecuteShowPagosViewCommand()
    {
        CurrentChildView = new PagosViewModel();
        Caption = "Pagos";
        Icon = IconChar.CreditCard;
    }

    private void ExecuteShowConfigViewCommand()
    {
        CurrentChildView = new ConfigViewModel();
        Caption = "Configuración";
        Icon = IconChar.Cog;
    }

    private void ExecuteShowMembresiasViewCommand()
    {
        CurrentChildView = new MembresiasViewModel();
        Caption = "Membresías";
        Icon = IconChar.CalendarCheck;
    }

    private void ExecuteShowSociosViewCommand()
    {
        CurrentChildView = new SociosViewModel();
        Caption = "Socios";
        Icon = IconChar.UserCircle;
    }

    private void ExecuteShowInicioViewCommand()
    {
        CurrentChildView = new InicioViewModel();
        Caption = "Inicio";
        Icon = IconChar.Home;
    }
}
