using Opc.Ua;
using Opc.Ua.Server;

namespace CypCutOpcUaGateway.OpcUa;

public sealed class CypCutNodeManager : CustomNodeManager2
{
    public const string NamespaceUri = "urn:standalone:cypcut:opcua:gateway";

    private readonly MachineOptions _machine;
    private readonly MachineDataStore _store;
    private readonly Dictionary<string, BaseDataVariableState> _parameterNodes = new(StringComparer.OrdinalIgnoreCase);
    private BaseDataVariableState? _connected;
    private BaseDataVariableState? _lastUpdate;
    private BaseDataVariableState? _lastError;
    private BaseDataVariableState? _rawJson;

    public CypCutNodeManager(IServerInternal server, ApplicationConfiguration configuration, MachineOptions machine, MachineDataStore store)
        : base(server, configuration, NamespaceUri)
    {
        _machine = machine;
        _store = store;
        SystemContext.NodeIdFactory = this;
        _store.Updated += OnStoreUpdated;
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        lock (Lock)
        {
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
            {
                references = new List<IReference>();
                externalReferences[ObjectIds.ObjectsFolder] = references;
            }

            var machineFolder = CreateFolder(null, $"Machine/{_machine.Id}", _machine.Name);
            machineFolder.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
            references.Add(new NodeStateReference(ReferenceTypes.Organizes, false, machineFolder.NodeId));
            AddPredefinedNode(SystemContext, machineFolder);

            var identity = CreateFolder(machineFolder, $"Machine/{_machine.Id}/Identity", "Identity");
            CreateReadOnlyVariable(identity, "Id", DataTypeIds.String, _machine.Id);
            CreateReadOnlyVariable(identity, "Name", DataTypeIds.String, _machine.Name);
            CreateReadOnlyVariable(identity, "CypCutIp", DataTypeIds.String, _machine.CypCutIp);
            CreateReadOnlyVariable(identity, "CypCutPort", DataTypeIds.Int32, _machine.CypCutPort);
            CreateReadOnlyVariable(identity, "OpcUaPort", DataTypeIds.Int32, _machine.OpcUaPort);

            var connection = CreateFolder(machineFolder, $"Machine/{_machine.Id}/Connection", "Connection");
            _connected = CreateReadOnlyVariable(connection, "Connected", DataTypeIds.Boolean, false);
            _lastUpdate = CreateReadOnlyVariable(connection, "LastUpdateUtc", DataTypeIds.DateTime, DateTime.MinValue);
            _lastError = CreateReadOnlyVariable(connection, "LastError", DataTypeIds.String, string.Empty);
            _rawJson = CreateReadOnlyVariable(connection, "RawJson", DataTypeIds.String, string.Empty);

            foreach (var categoryGroup in ParameterCatalog.All.GroupBy(x => x.Category))
            {
                var categoryFolder = CreateFolder(machineFolder, $"Machine/{_machine.Id}/{categoryGroup.Key}", categoryGroup.Key);
                foreach (var definition in categoryGroup)
                {
                    var dataType = definition.Kind switch
                    {
                        ParameterValueKind.Boolean => DataTypeIds.Boolean,
                        ParameterValueKind.Integer => DataTypeIds.Int64,
                        ParameterValueKind.Number => DataTypeIds.Double,
                        _ => DataTypeIds.String
                    };
                    object initial = definition.Kind switch
                    {
                        ParameterValueKind.Boolean => false,
                        ParameterValueKind.Integer => 0L,
                        ParameterValueKind.Number => double.NaN,
                        _ => string.Empty
                    };
                    _parameterNodes[definition.Key] = CreateReadOnlyVariable(categoryFolder, definition.Name, dataType, initial);
                }
            }

            UpdateNodes();
        }
    }

    private FolderState CreateFolder(NodeState? parent, string nodePath, string displayName)
    {
        var folder = new FolderState(parent)
        {
            SymbolicName = displayName,
            ReferenceTypeId = parent is null ? ReferenceTypes.Organizes : ReferenceTypes.HasComponent,
            TypeDefinitionId = ObjectTypeIds.FolderType,
            NodeId = new NodeId(nodePath, NamespaceIndex),
            BrowseName = new QualifiedName(displayName, NamespaceIndex),
            DisplayName = displayName,
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            EventNotifier = EventNotifiers.None
        };
        if (parent is not null)
        {
            parent.AddChild(folder);
            AddPredefinedNode(SystemContext, folder);
        }
        return folder;
    }

    private BaseDataVariableState CreateReadOnlyVariable(NodeState parent, string name, NodeId dataType, object value)
    {
        var parentId = parent.NodeId.Identifier?.ToString() ?? "Machine";
        var variable = new BaseDataVariableState(parent)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypes.HasComponent,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            NodeId = new NodeId($"{parentId}/{name}", NamespaceIndex),
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = name,
            DataType = dataType,
            ValueRank = ValueRanks.Scalar,
            AccessLevel = AccessLevels.CurrentRead,
            UserAccessLevel = AccessLevels.CurrentRead,
            Historizing = false,
            Value = value,
            StatusCode = StatusCodes.BadWaitingForInitialData,
            Timestamp = DateTime.UtcNow
        };
        parent.AddChild(variable);
        AddPredefinedNode(SystemContext, variable);
        return variable;
    }

    private void OnStoreUpdated()
    {
        lock (Lock) UpdateNodes();
    }

    private void UpdateNodes()
    {
        if (_connected is null) return;
        SetValue(_connected, _store.Connected, StatusCodes.Good);
        SetValue(_lastUpdate!, _store.LastUpdateUtc, _store.LastUpdateUtc == default ? StatusCodes.BadWaitingForInitialData : StatusCodes.Good);
        SetValue(_lastError!, _store.LastError, StatusCodes.Good);
        SetValue(_rawJson!, _store.RawJson, _store.RawJson.Length == 0 ? StatusCodes.BadWaitingForInitialData : StatusCodes.Good);

        foreach (var definition in ParameterCatalog.All)
        {
            if (!_parameterNodes.TryGetValue(definition.Key, out var node)) continue;
            if (_store.Values.TryGetValue(definition.Key, out var value) && value.Present)
                SetValue(node, value.Value, StatusCodes.Good, value.TimestampUtc);
        }
    }

    private void SetValue(BaseDataVariableState node, object? value, StatusCode status, DateTime? timestamp = null)
    {
        node.Value = value;
        node.StatusCode = status;
        node.Timestamp = timestamp ?? DateTime.UtcNow;
        node.ClearChangeMasks(SystemContext, false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _store.Updated -= OnStoreUpdated;
        base.Dispose(disposing);
    }
}
