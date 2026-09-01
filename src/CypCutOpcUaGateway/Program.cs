using CypCutOpcUaGateway;

if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
{
    const string sample = """
    {"status":0,"data":{"SpeedUnitStr":"mm/s","TmEstimation":12.5,
      "NcState":{"AxisX":101.25,"AxisY":202.5,"WorkSpeed":3500,"TaskName":"demo.fsc","IsJogFast":false},
      "DeviceState":{"CurrentZ":3.2,"LaserPower":0,"IsLaserOn":true,"GasPressure":1.1},
      "GlobalParams":{"MaxAcc":2500,"PressureUnit":"bar","EnableFollower":true}}}
    """;
    var mapped = CypCutJsonMapper.Map(sample);
    var expected = new[] { "State.SpeedUnitStr", "State.TmEstimation", "NcState.AxisX", "NcState.AxisY", "NcState.WorkSpeed", "DeviceState.CurrentZ", "DeviceState.IsLaserOn", "GlobalParams.MaxAcc" };
    foreach (var key in expected) if (!mapped.ContainsKey(key)) throw new InvalidOperationException($"Self-test missing {key}");
    if ((double)mapped["NcState.AxisX"]! != 101.25) throw new InvalidOperationException("Self-test AxisX mismatch.");
    if ((bool)mapped["DeviceState.IsLaserOn"]! != true) throw new InvalidOperationException("Self-test IsLaserOn mismatch.");
    Console.WriteLine($"Self-test OK. Extracted {mapped.Count} sample values; catalog contains {ParameterCatalog.All.Count} parameters.");
    return;
}

if (args.Contains("--list-parameters", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine($"Known CypCut parameters: {ParameterCatalog.All.Count}");
    foreach (var group in ParameterCatalog.All.GroupBy(x => x.Category))
    {
        Console.WriteLine($"{group.Key} ({group.Count()}): {string.Join(", ", group.Select(x => x.Name))}");
    }
    return;
}

if (args.Contains("--validate-config", StringComparer.OrdinalIgnoreCase))
{
    var (gateway, machines) = ConfigurationLoader.Load(GatewayPaths.ResolveRoot());
    Console.WriteLine($"Configuration OK. Gateway={gateway.PublishedIp}; enabled={machines.Count(x => x.Enabled)}; parameters={ParameterCatalog.All.Count}");
    foreach (var machine in machines.Where(x => x.Enabled))
        Console.WriteLine($"{machine.Id}: http://{machine.CypCutIp}:{machine.CypCutPort} -> opc.tcp://{gateway.PublishedIp}:{machine.OpcUaPort}/CypCut/{machine.Id}");
    return;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "CypCut Standalone OPC UA Gateway");
builder.Services.AddHostedService<GatewayWorker>();
await builder.Build().RunAsync();
