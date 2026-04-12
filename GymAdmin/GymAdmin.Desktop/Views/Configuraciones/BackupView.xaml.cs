using GymAdmin.Desktop.ViewModels.Socios;
using System.Windows.Controls;
using System.Windows.Input;

namespace GymAdmin.Desktop.Views.Configuraciones;
/// <summary>
/// Lógica de interacción para BackupView.xaml
/// </summary>
public partial class BackupView : UserControl
{
    public BackupView()
    {
        InitializeComponent();
    }
    private void DialogOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == DialogOverlay && DataContext is SociosViewModel vm)
        {
            vm.IsDialogOpen = false;
        }
    }
}
