using GymAdmin.Desktop.ViewModels.Dialogs;
using GymAdmin.Desktop.Views.Dialogs;
using System.Windows;

namespace GymAdmin.Desktop.Services;

public class OverlayDialogService : IOverlayDialogService
{
    public async Task<bool> ShowConfirmAsync(
         IDialogHost host,
         string title,
         string message,
         string accept = "Aceptar",
         string cancel = "Cancelar",
         string acceptStyleKey = "DangerButtonStyle",
         string cancelStyleKey = "PrimaryButtonStyle",
         bool showCancel = true)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        var vm = new ConfirmDialogViewModel
        {
            Title = title,
            Message = message,
            AcceptText = accept,
            CancelText = cancel,
            AcceptButtonStyle = (Style)Application.Current.FindResource(acceptStyleKey),
            CancelButtonStyle = (Style)Application.Current.FindResource(cancelStyleKey),
            ShowCancelButton = showCancel
        };

        var view = new ConfirmDialogView
        {
            DataContext = vm
        };

        host.DialogContent = view;
        host.IsDialogOpen = true;

        try
        {
            return await vm.Task;
        }
        finally
        {
            host.IsDialogOpen = false;
            host.DialogContent = null;
        }
    }
}
