using System.Net.Http.Headers;

namespace CypCutOpcUaGateway;

public sealed class CypCutCollector : IAsyncDisposable
{
    private readonly MachineOptions _machine;
    private readonly MachineDataStore _store;
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public CypCutCollector(MachineOptions machine, MachineDataStore store, GatewayOptions gateway, ILogger logger)
    {
        _machine = machine;
        _store = store;
        _logger = logger;
        _client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(gateway.RequestTimeoutMs) };
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var endpoint = BuildEndpoint();
        _logger.LogInformation("{Machine}: polling {Endpoint}", _machine.Id, endpoint);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var response = await _client.GetAsync(endpoint, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                response.EnsureSuccessStatusCode();
                var values = CypCutJsonMapper.Map(json);
                if (values.Count == 0) throw new InvalidDataException("CypCut response contained none of the configured parameters.");
                _store.Apply(values, json);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _store.SetError(ex.Message);
                _logger.LogWarning("{Machine}: {Error}", _machine.Id, ex.Message);
            }

            try { await Task.Delay(_machine.PollIntervalMs, cancellationToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private Uri BuildEndpoint()
    {
        var path = _machine.EndpointPath
            .Replace("{ip}", Uri.EscapeDataString(_machine.CypCutIp), StringComparison.OrdinalIgnoreCase)
            .Replace("{appName}", Uri.EscapeDataString(_machine.AppName), StringComparison.OrdinalIgnoreCase);
        if (!path.StartsWith('/')) path = "/" + path;
        return new Uri($"http://{_machine.CypCutIp}:{_machine.CypCutPort}{path}");
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
