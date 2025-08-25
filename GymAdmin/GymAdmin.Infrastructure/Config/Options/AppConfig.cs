using Microsoft.Extensions.Configuration;

namespace GymAdmin.Infrastructure.Config.Options;

public class AppConfig
{
    internal SqliteConfig SqliteConfig { get; set; } 
    internal BackupConfig BackupConfig { get; set; } 

    internal AppConfig(IConfiguration configuration)
    {
        SqliteConfig = configuration.GetSection(nameof(SqliteConfig)).Get<SqliteConfig>()!;
        BackupConfig = configuration.GetSection(nameof(BackupConfig)).Get<BackupConfig>()!;
    }
}
