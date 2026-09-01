namespace CypCutOpcUaGateway;

public static class GatewayPaths
{
    public static string ResolveRoot()
    {
        var configured = Environment.GetEnvironmentVariable("CYPCUT_GATEWAY_ROOT");
        if (!string.IsNullOrWhiteSpace(configured)) return Path.GetFullPath(configured);

        var baseDirectory = AppContext.BaseDirectory;
        if (Directory.Exists(Path.Combine(baseDirectory, "config"))) return baseDirectory;

        var parent = Directory.GetParent(baseDirectory)?.FullName;
        if (parent is not null && Directory.Exists(Path.Combine(parent, "config"))) return parent;
        return baseDirectory;
    }
}
