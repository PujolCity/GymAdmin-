using Microsoft.Extensions.Configuration;

namespace GymAdmin.Infrastructure.Config.Options;

public class AppConfig
{
    internal SqliteConfig SqliteConfig { get; set; } 
    internal BackupConfig BackupConfig { get; set; } 
    internal PathsConfig PathsConfig { get; set; } 
    internal InstallerConfig InstallerConfig { get; set; } 

    internal AppConfig(IConfiguration configuration)
    {
        SqliteConfig = configuration.GetSection(nameof(SqliteConfig)).Get<SqliteConfig>()!;
        BackupConfig = configuration.GetSection(nameof(BackupConfig)).Get<BackupConfig>()!;
        PathsConfig = configuration.GetSection(nameof(PathsConfig)).Get<PathsConfig>()!;
        InstallerConfig = configuration.GetSection(nameof(InstallerConfig)).Get<InstallerConfig>()!;
    }
}
