using GymAdmin.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;

namespace GymAdmin.Infrastructure.Paths.FolderConfig;

public sealed class AppPaths : IAppPaths
{
    public string Root { get; }
    public string DataDir { get; }
    public string LogsDir { get; }
    public string DbFile { get; }
    public string LogFilePattern { get; }
    public string SecretFile { get; }

    public AppPaths(IOptions<PathsConfig> options, IPathResolver pathResolver)
    {
        var backupConfig = options.Value;

        var rootRaw = string.IsNullOrWhiteSpace(backupConfig.Root)
            ? "%MyDocuments%/GymAdmin"
            : backupConfig.Root!;

        Root = pathResolver.Resolve(rootRaw);
        SecretFile = pathResolver.Resolve(Path.Combine(Root, backupConfig.SecretFile));
        
        LogsDir = pathResolver.Resolve(Path.Combine(Root, backupConfig.LogsDir));
        LogFilePattern = pathResolver.Resolve(Path.Combine(LogsDir, backupConfig.LogFilePattern));
        
        DataDir = pathResolver.Resolve(Path.Combine(Root, backupConfig.DataDir));
        DbFile = pathResolver.Resolve(Path.Combine(DataDir, backupConfig.DbFile));

        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(LogsDir);
    }
}