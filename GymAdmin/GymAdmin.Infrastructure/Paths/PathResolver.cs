namespace GymAdmin.Infrastructure.Paths;

public sealed class PathResolver : IPathResolver
{
    public string Resolve(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["%MyDocuments%"] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            ["%LocalAppData%"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ["%AppData%"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ["%Desktop%"] = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        var output = input;

        foreach (var kv in map)
            output = output.Replace(kv.Key, kv.Value);

        output = Environment.ExpandEnvironmentVariables(output);
        output = output.Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(output);
    }
}
