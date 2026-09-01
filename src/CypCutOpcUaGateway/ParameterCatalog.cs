namespace CypCutOpcUaGateway;

public static class ParameterCatalog
{
    private static ParameterDefinition B(string category, string name) => new(category, name, ParameterValueKind.Boolean);
    private static ParameterDefinition N(string category, string name) => new(category, name, ParameterValueKind.Number);
    private static ParameterDefinition I(string category, string name) => new(category, name, ParameterValueKind.Integer);
    private static ParameterDefinition S(string category, string name) => new(category, name, ParameterValueKind.Text);

    public static readonly IReadOnlyList<ParameterDefinition> All = new[]
    {
        S("State", "SpeedUnitStr"), N("State", "TmEstimation"),

        I("NcState", "AlarmCount"), S("NcState", "AlarmMsg"), N("NcState", "AxisX"), N("NcState", "AxisY"),
        N("NcState", "CADHomeRefX"), N("NcState", "CADHomeRefY"), B("NcState", "CanRestoreFromStop"),
        N("NcState", "CutPercent"), N("NcState", "DA1"), N("NcState", "DA2"), N("NcState", "FeedRate"),
        N("NcState", "HomeRefX"), N("NcState", "HomeRefY"), I("NcState", "InportBytes"), B("NcState", "IsJogFast"),
        N("NcState", "LaserPower"), I("NcState", "OutportBytes"), S("NcState", "RunningTimeStr"),
        I("NcState", "SwitchTableNum"), I("NcState", "SysState"), S("NcState", "TaskName"), N("NcState", "UcsOrgX"),
        N("NcState", "UcsOrgY"), N("NcState", "WorkSpeed"), N("NcState", "WorkTime"), S("NcState", "WorkTimeStr"),

        N("DeviceState", "CurrentH"), N("DeviceState", "CurrentZ"), N("DeviceState", "DiodeCurrent"),
        N("DeviceState", "FocusPos"), N("DeviceState", "GasPressure"), S("DeviceState", "GasType"),
        B("DeviceState", "IsAimingOn"), B("DeviceState", "IsEmissionOn"), B("DeviceState", "IsFollowing"),
        B("DeviceState", "IsGasOn"), B("DeviceState", "IsLaserOn"), N("DeviceState", "PwmFreq"),
        N("DeviceState", "PwmRatio"), N("DeviceState", "TargetHeight"),

        S("GlobalParams", "AccUnit"), N("GlobalParams", "AheadOpenGas"), B("GlobalParams", "AllRequireManualReset"),
        N("GlobalParams", "ArcTolerance"), B("GlobalParams", "BCS100ZControl"), N("GlobalParams", "CompPathPrec"),
        N("GlobalParams", "CoolPointDelay"), N("GlobalParams", "CornerTolerance"), N("GlobalParams", "DelayLaserOffDis"),
        N("GlobalParams", "DiodeCurrent"), B("GlobalParams", "DisableFollower"), B("GlobalParams", "EnableFollower"),
        N("GlobalParams", "FIRFreq"), B("GlobalParams", "FollowWithFOcus"), N("GlobalParams", "GasOpenDelay"),
        N("GlobalParams", "GasOpenDelay1"), N("GlobalParams", "GasPressure"), B("GlobalParams", "LeapFrog"),
        N("GlobalParams", "MaxAcc"), N("GlobalParams", "MaxFollowHeight"), N("GlobalParams", "MoveAccX"),
        N("GlobalParams", "MoveAccY"), N("GlobalParams", "MoveFIRFreq"), B("GlobalParams", "MoveOptimize"),
        N("GlobalParams", "MoveSpeedX"), N("GlobalParams", "MoveSpeedY"), N("GlobalParams", "PauseBackDistance"),
        S("GlobalParams", "PressureUnit"), B("GlobalParams", "ProtectMachine"), N("GlobalParams", "PwmFreq"),
        N("GlobalParams", "ShortMoveSize"), S("GlobalParams", "SpeedUnit"), N("GlobalParams", "SwitchGasDelay"),
        S("GlobalParams", "TimeUnit"), N("GlobalParams", "WalkSpeed"), N("GlobalParams", "ZRange")
    };
}
