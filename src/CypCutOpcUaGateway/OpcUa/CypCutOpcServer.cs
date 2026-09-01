using Opc.Ua;
using Opc.Ua.Server;

namespace CypCutOpcUaGateway.OpcUa;

public sealed class CypCutOpcServer : StandardServer
{
    private readonly MachineOptions _machine;
    private readonly MachineDataStore _store;

    public CypCutOpcServer(MachineOptions machine, MachineDataStore store)
    {
        _machine = machine;
        _store = store;
    }

    protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
    {
        return new MasterNodeManager(server, configuration, null, new CypCutNodeManager(server, configuration, _machine, _store));
    }

    protected override ServerProperties LoadServerProperties()
    {
        return new ServerProperties
        {
            ManufacturerName = "Standalone Industrial Gateway",
            ProductName = "CypCut OPC UA Gateway",
            ProductUri = "urn:standalone:cypcut:opcua:gateway",
            SoftwareVersion = "0.1.0",
            BuildNumber = "1",
            BuildDate = DateTime.UtcNow
        };
    }
}
