namespace GymAdmin.Infrastructure.Config.Options;

public class PathsConfig
{
    public string Root { get; set; }
    public string DataDir { get; set; }
    public string LogsDir { get; set; }
    public string DbFile { get; set; }
    public string LogFilePattern { get; set; }
    public string SecretFile { get; set; }
}