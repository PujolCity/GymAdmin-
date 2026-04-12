namespace GymAdmin.Desktop.Services;

public interface IDialogHost
{
    object? DialogContent { get; set; }
    bool IsDialogOpen { get; set; }
}
