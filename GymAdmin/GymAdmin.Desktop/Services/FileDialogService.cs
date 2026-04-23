using Microsoft.Win32;
using System.IO;

namespace GymAdmin.Desktop.Services;

public class FileDialogService : IFileDialogService
{
    public string? SelectZipFile(string initialDirectory)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Backups",
            Filter = "Archivos ZIP (*.zip)|*.zip",
            DefaultExt = ".zip",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        return dialog.ShowDialog() == true
            ? dialog.FileName
            : null;
    }
}
