namespace GymAdmin.Desktop.Services;

public interface IOverlayDialogService
{
    Task<bool> ShowConfirmAsync(
        IDialogHost host,
        string title,
        string message,
        string accept = "Aceptar",
        string cancel = "Cancelar",
        string acceptStyleKey = "DangerButtonStyle",
        string cancelStyleKey = "PrimaryButtonStyle",
        bool showCancel = true);
}
