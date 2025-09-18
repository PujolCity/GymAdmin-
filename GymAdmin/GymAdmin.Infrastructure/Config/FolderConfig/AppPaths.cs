using GymAdmin.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;

namespace GymAdmin.Infrastructure.Config.FolderConfig;

public sealed class AppPaths : IAppPaths
{
    public string Root { get; }
    public string DataDir { get; }
    public string LogsDir { get; }
    public string DbFile { get; }
    public string LogFilePattern { get; }
    public string SecretFile { get; }

    public AppPaths(IOptions<PathsConfig> opts)
    {
        var o = opts.Value;

        var rootRaw = string.IsNullOrWhiteSpace(o.Root) ? "%MyDocuments%/GymAdmin" : o.Root!;
        Root = MakeAbsolute(ExpandTokens(rootRaw));

        DataDir = MakeAbsolute(Path.Combine(Root, o.DataDir));
        LogsDir = MakeAbsolute(Path.Combine(Root, o.LogsDir));

        DbFile = MakeAbsolute(Path.Combine(DataDir, o.DbFile));
        LogFilePattern = MakeAbsolute(Path.Combine(LogsDir, o.LogFilePattern));
        SecretFile = MakeAbsolute(Path.Combine(Root, o.SecretFile));

        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(LogsDir);
    }

    private static string MakeAbsolute(string path) => Path.GetFullPath(path);

    // Soporta %MyDocuments%, %LocalAppData%, %AppData% y variables de entorno
    private static string ExpandTokens(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["%MyDocuments%"] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            ["%LocalAppData%"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ["%AppData%"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ["%Desktop%"] = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        var output = input;
        foreach (var kv in map) output = output.Replace(kv.Key, kv.Value);
        output = Environment.ExpandEnvironmentVariables(output);
        output = output.Replace('/', Path.DirectorySeparatorChar);
        return output;
    }
}