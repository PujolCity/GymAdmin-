namespace GymAdmin.Infrastructure.Config.Options;

public class BackupConfig
{
    public string FolderPath { get; set; }
    public int RetentionDays { get; set; }
    public bool AutoBackup { get; set; }
}
