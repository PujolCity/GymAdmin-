namespace GymAdmin.Infrastructure.Config.FolderConfig;

public interface IAppPaths
{
    string Root { get; }
    string DataDir { get; }
    string LogsDir { get; }
    string DbFile { get; }
    string LogFilePattern { get; }
    string SecretFile { get; }
}