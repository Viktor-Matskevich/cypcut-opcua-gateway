using System.Collections.Concurrent;

namespace CypCutOpcUaGateway;

public sealed record GatewayOptions(
    string Name,
    string PublishedIp,
    string ConfigDirectory,
    string PkiDirectory,
    int RequestTimeoutMs);

public sealed record MachineOptions(
    bool Enabled,
    string Id,
    string Name,
    string CypCutIp,
    int CypCutPort,
    int OpcUaPort,
    string EndpointPath,
    int PollIntervalMs,
    string AppName);

public enum ParameterValueKind
{
    Boolean,
    Number,
    Integer,
    Text
}

public sealed record ParameterDefinition(string Category, string Name, ParameterValueKind Kind)
{
    public string Key => $"{Category}.{Name}";
}

public sealed record ParameterValue(object? Value, DateTime TimestampUtc, bool Present);

public sealed class MachineDataStore
{
    private readonly ConcurrentDictionary<string, ParameterValue> _values = new(StringComparer.OrdinalIgnoreCase);

    public event Action? Updated;

    public bool Connected { get; private set; }
    public DateTime LastUpdateUtc { get; private set; }
    public string LastError { get; private set; } = string.Empty;
    public string RawJson { get; private set; } = string.Empty;

    public IReadOnlyDictionary<string, ParameterValue> Values => _values;

    public void Apply(IReadOnlyDictionary<string, object?> values, string rawJson)
    {
        var now = DateTime.UtcNow;
        foreach (var definition in ParameterCatalog.All)
        {
            if (values.TryGetValue(definition.Key, out var value))
            {
                _values[definition.Key] = new ParameterValue(value, now, true);
            }
        }

        Connected = true;
        LastUpdateUtc = now;
        LastError = string.Empty;
        RawJson = rawJson;
        Updated?.Invoke();
    }

    public void SetError(string message)
    {
        Connected = false;
        LastError = message;
        Updated?.Invoke();
    }
}
