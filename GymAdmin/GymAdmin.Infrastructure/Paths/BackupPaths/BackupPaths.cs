using GymAdmin.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;

namespace GymAdmin.Infrastructure.Paths.BackupPaths;

public class BackupPaths : IBackupPaths
{
    public string BackupRoot { get; }

    public BackupPaths(IOptions<BackupConfig> options, IPathResolver pathResolver)
    {
        var backupConfig = options.Value;

        var directory = string.IsNullOrWhiteSpace(backupConfig.FolderPath)
            ? "%MyDocuments%/GymAdmin-Backups"
            : backupConfig.FolderPath;

        BackupRoot = pathResolver.Resolve(directory);

        Directory.CreateDirectory(BackupRoot);
    }
}
