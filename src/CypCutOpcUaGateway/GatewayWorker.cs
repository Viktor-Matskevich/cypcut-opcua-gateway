using CypCutOpcUaGateway.OpcUa;

namespace CypCutOpcUaGateway;

public sealed class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly List<OpcUaServerHost> _servers = new();
    private readonly List<CypCutCollector> _collectors = new();

    public GatewayWorker(ILogger<GatewayWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var (gateway, machines) = ConfigurationLoader.Load(GatewayPaths.ResolveRoot());
        var enabled = machines.Where(x => x.Enabled).ToArray();
        _logger.LogInformation("Starting {Gateway} with {Count} machine(s)", gateway.Name, enabled.Length);

        var collectorTasks = new List<Task>();
        foreach (var machine in enabled)
        {
            var store = new MachineDataStore();
            var server = new OpcUaServerHost(gateway, machine, store, _logger);
            await server.StartAsync();
            _servers.Add(server);

            var collector = new CypCutCollector(machine, store, gateway, _logger);
            _collectors.Add(collector);
            collectorTasks.Add(collector.RunAsync(stoppingToken));
        }

        await Task.WhenAll(collectorTasks);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var collector in _collectors) await collector.DisposeAsync();
        foreach (var server in _servers) await server.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
