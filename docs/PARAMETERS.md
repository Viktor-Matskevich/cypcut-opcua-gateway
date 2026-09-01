# OPC UA parameter catalog

The gateway defines **78 process variables** and **9 identity/diagnostic
variables**, for a total of **87 variables per machine endpoint**.

Values are populated only when the installed CypCut version returns a matching
field. Missing fields retain the OPC UA status `BadWaitingForInitialData`.

## State — 2

`SpeedUnitStr`, `TmEstimation`

## NcState — 26

`AlarmCount`, `AlarmMsg`, `AxisX`, `AxisY`, `CADHomeRefX`, `CADHomeRefY`,
`CanRestoreFromStop`, `CutPercent`, `DA1`, `DA2`, `FeedRate`, `HomeRefX`,
`HomeRefY`, `InportBytes`, `IsJogFast`, `LaserPower`, `OutportBytes`,
`RunningTimeStr`, `SwitchTableNum`, `SysState`, `TaskName`, `UcsOrgX`, `UcsOrgY`,
`WorkSpeed`, `WorkTime`, `WorkTimeStr`

## DeviceState — 14

`CurrentH`, `CurrentZ`, `DiodeCurrent`, `FocusPos`, `GasPressure`, `GasType`,
`IsAimingOn`, `IsEmissionOn`, `IsFollowing`, `IsGasOn`, `IsLaserOn`, `PwmFreq`,
`PwmRatio`, `TargetHeight`

## GlobalParams — 36

`AccUnit`, `AheadOpenGas`, `AllRequireManualReset`, `ArcTolerance`,
`BCS100ZControl`, `CompPathPrec`, `CoolPointDelay`, `DelayLaserOffDis`,
`DiodeCurrent`, `DisableFollower`, `EnableFollower`, `FIRFreq`, `FollowWithFOcus`,
`GasOpenDelay`, `GasOpenDelay1`, `GasPressure`, `LeapFrog`, `MaxAcc`,
`MaxFollowHeight`, `MoveAccX`, `MoveAccY`, `MoveFIRFreq`, `MoveOptimize`,
`MoveSpeedX`, `MoveSpeedY`, `PauseBackDistance`, `PressureUnit`, `ProtectMachine`,
`PwmFreq`, `ShortMoveSize`, `SpeedUnit`, `SwitchGasDelay`, `TimeUnit`, `WalkSpeed`,
`ZRange`, `CornerTolerance`

## Identity and diagnostics — 9

- `Identity`: `Id`, `Name`, `CypCutIp`, `CypCutPort`, `OpcUaPort`
- `Connection`: `Connected`, `LastUpdateUtc`, `LastError`, `RawJson`

`RawJson` preserves the latest complete source response so that newly observed
fields can be added without losing the original data during field validation.
