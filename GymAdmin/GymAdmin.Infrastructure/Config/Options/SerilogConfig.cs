namespace GymAdmin.Infrastructure.Config.Options;

public class SerilogConfig
{
    public string LogFilePath { get; set; }
    public int RetainedFileCountLimit { get; set; }
    public string ConsoleOutputTemplate { get; set; }
    public string FileOutputTemplate { get; set; }
    public string MinimumLevel { get; set; }
}