using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.VisualBasic.FileIO;

namespace CypCutOpcUaGateway;

public static class ConfigurationLoader
{
    public static (GatewayOptions Gateway, IReadOnlyList<MachineOptions> Machines) Load(string baseDirectory)
    {
        var configDirectory = Path.Combine(baseDirectory, "config");
        var gatewayPath = Path.Combine(configDirectory, "gateway.json");
        var machinesPath = Path.Combine(configDirectory, "machines.csv");

        if (!File.Exists(gatewayPath)) throw new FileNotFoundException("Gateway configuration not found.", gatewayPath);
        if (!File.Exists(machinesPath)) throw new FileNotFoundException("Machine registry not found.", machinesPath);

        using var gatewayDocument = JsonDocument.Parse(File.ReadAllText(gatewayPath));
        var root = gatewayDocument.RootElement;
        var gateway = new GatewayOptions(
            GetString(root, "name", "CypCut-Standalone-Gateway"),
            GetString(root, "publishedIp", "127.0.0.1"),
            configDirectory,
            Path.GetFullPath(Path.Combine(baseDirectory, GetString(root, "pkiDirectory", "pki"))),
            GetInt(root, "requestTimeoutMs", 3000));

        if (!IPAddress.TryParse(gateway.PublishedIp, out _))
            throw new InvalidDataException($"Invalid publishedIp: {gateway.PublishedIp}");

        var machines = LoadMachines(machinesPath);
        ValidateMachines(machines);
        return (gateway, machines);
    }

    private static IReadOnlyList<MachineOptions> LoadMachines(string path)
    {
        using var parser = new TextFieldParser(path) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true };
        parser.SetDelimiters(",");
        var header = parser.ReadFields() ?? throw new InvalidDataException("machines.csv has no header.");
        var indexes = header.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
        string Field(string[] row, string name, string defaultValue = "") => indexes.TryGetValue(name, out var i) && i < row.Length ? row[i].Trim() : defaultValue;

        var result = new List<MachineOptions>();
        while (!parser.EndOfData)
        {
            var row = parser.ReadFields();
            if (row is null || row.All(string.IsNullOrWhiteSpace)) continue;
            result.Add(new MachineOptions(
                ParseBool(Field(row, "Enabled")),
                Field(row, "Id"),
                Field(row, "Name"),
                Field(row, "CypCutIp"),
                ParseInt(Field(row, "CypCutPort", "8080"), "CypCutPort"),
                ParseInt(Field(row, "OpcUaPort", "4880"), "OpcUaPort"),
                Field(row, "EndpointPath", "/api/monitor/cutSystemState?ip={ip}&appName={appName}"),
                ParseInt(Field(row, "PollIntervalMs", "1000"), "PollIntervalMs"),
                Field(row, "AppName", "CypCut")));
        }
        return result;
    }

    private static void ValidateMachines(IReadOnlyList<MachineOptions> machines)
    {
        var enabled = machines.Where(x => x.Enabled).ToArray();
        if (enabled.Length == 0) throw new InvalidDataException("At least one machine must have Enabled=true.");
        foreach (var machine in enabled)
        {
            if (string.IsNullOrWhiteSpace(machine.Id) || !machine.Id.All(c => char.IsLetterOrDigit(c) || c is '-' or '_'))
                throw new InvalidDataException($"Invalid machine Id: {machine.Id}");
            if (!IPAddress.TryParse(machine.CypCutIp, out _)) throw new InvalidDataException($"Invalid CypCutIp for {machine.Id}: {machine.CypCutIp}");
            ValidatePort(machine.CypCutPort, machine.Id, "CypCutPort");
            ValidatePort(machine.OpcUaPort, machine.Id, "OpcUaPort");
            if (machine.PollIntervalMs is < 100 or > 60000) throw new InvalidDataException($"PollIntervalMs for {machine.Id} must be 100..60000.");
        }

        var duplicateIds = enabled.GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicateIds is not null) throw new InvalidDataException($"Duplicate Id: {duplicateIds.Key}");
        var duplicatePorts = enabled.GroupBy(x => x.OpcUaPort).FirstOrDefault(x => x.Count() > 1);
        if (duplicatePorts is not null) throw new InvalidDataException($"Duplicate OpcUaPort: {duplicatePorts.Key}");
    }

    private static string GetString(JsonElement root, string name, string defaultValue) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? defaultValue : defaultValue;

    private static int GetInt(JsonElement root, string name, int defaultValue) =>
        root.TryGetProperty(name, out var value) && value.TryGetInt32(out var result) ? result : defaultValue;

    private static bool ParseBool(string value) => value.Equals("true", StringComparison.OrdinalIgnoreCase) || value is "1" or "yes" or "YES";
    private static int ParseInt(string value, string field) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : throw new InvalidDataException($"Invalid {field}: {value}");
    private static void ValidatePort(int value, string id, string name) { if (value is < 1 or > 65535) throw new InvalidDataException($"{name} for {id} must be 1..65535."); }
}
