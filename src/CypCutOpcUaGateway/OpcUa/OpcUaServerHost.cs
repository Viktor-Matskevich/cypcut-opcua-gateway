using Opc.Ua;
using Opc.Ua.Configuration;

namespace CypCutOpcUaGateway.OpcUa;

public sealed class OpcUaServerHost : IAsyncDisposable
{
    private readonly GatewayOptions _gateway;
    private readonly MachineOptions _machine;
    private readonly MachineDataStore _store;
    private readonly ILogger _logger;
    private CypCutOpcServer? _server;

    public OpcUaServerHost(GatewayOptions gateway, MachineOptions machine, MachineDataStore store, ILogger logger)
    {
        _gateway = gateway;
        _machine = machine;
        _store = store;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        var applicationName = $"{_gateway.Name}-{_machine.Id}";
        var applicationUri = $"urn:{Utils.GetHostName()}:{applicationName}";
        var endpoint = $"opc.tcp://{_gateway.PublishedIp}:{_machine.OpcUaPort}/CypCut/{_machine.Id}";
        var pkiRoot = Path.Combine(_gateway.PkiDirectory, _machine.Id);

        var configuration = new ApplicationConfiguration
        {
            ApplicationName = applicationName,
            ApplicationUri = applicationUri,
            ProductUri = "urn:standalone:cypcut:opcua:gateway",
            ApplicationType = ApplicationType.Server,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = Path.Combine(pkiRoot, "own"),
                    SubjectName = $"CN={applicationName}, DC={Utils.GetHostName()}"
                },
                TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = Path.Combine(pkiRoot, "trusted") },
                RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = Path.Combine(pkiRoot, "rejected") },
                AutoAcceptUntrustedCertificates = true,
                AddAppCertToTrustedStore = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 2048
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000, MaxStringLength = 4 * 1024 * 1024 },
            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = new StringCollection { endpoint },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy { SecurityMode = MessageSecurityMode.None, SecurityPolicyUri = SecurityPolicies.None },
                    new ServerSecurityPolicy { SecurityMode = MessageSecurityMode.SignAndEncrypt, SecurityPolicyUri = SecurityPolicies.Basic256Sha256 }
                },
                UserTokenPolicies = new UserTokenPolicyCollection { new UserTokenPolicy(UserTokenType.Anonymous) },
                DiagnosticsEnabled = true,
                MaxSessionCount = 100,
                MaxSubscriptionCount = 1000,
                MaxMessageQueueSize = 100,
                MaxNotificationQueueSize = 100,
                MaxPublishRequestCount = 100
            },
            TraceConfiguration = new TraceConfiguration()
        };

        await configuration.Validate(ApplicationType.Server);
        var application = new ApplicationInstance
        {
            ApplicationName = applicationName,
            ApplicationType = ApplicationType.Server,
            ApplicationConfiguration = configuration
        };

        await application.CheckApplicationInstanceCertificate(false, 0);
        _server = new CypCutOpcServer(_machine, _store);
        await application.Start(_server);
        _logger.LogInformation("{Machine}: OPC UA endpoint {Endpoint}", _machine.Id, endpoint);
    }

    public ValueTask DisposeAsync()
    {
        _server?.Stop();
        _server?.Dispose();
        return ValueTask.CompletedTask;
    }
}
