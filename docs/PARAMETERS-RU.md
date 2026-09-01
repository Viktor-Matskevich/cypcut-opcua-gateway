# Параметры CypCut → OPC UA

Адаптер создаёт **78 известных технологических параметров**. Фактическое значение
будет заполнено только тогда, когда оно присутствует в ответе конкретной версии
CypCut. Отсутствующие поля сохраняют статус OPC UA `BadWaitingForInitialData`.

## State — 2

- `SpeedUnitStr` — единица скорости;
- `TmEstimation` — оценочное время выполнения.

## NcState — 26

- аварии: `AlarmCount`, `AlarmMsg`;
- координаты: `AxisX`, `AxisY`;
- опорные координаты: `CADHomeRefX`, `CADHomeRefY`, `HomeRefX`, `HomeRefY`;
- возможность восстановления: `CanRestoreFromStop`;
- выполнение: `CutPercent`, `WorkSpeed`, `FeedRate`, `WorkTime`, `WorkTimeStr`, `RunningTimeStr`;
- лазер и аналоговые значения: `LaserPower`, `DA1`, `DA2`;
- входы/выходы: `InportBytes`, `OutportBytes`;
- режимы: `IsJogFast`, `SwitchTableNum`, `SysState`;
- задание: `TaskName`;
- система координат: `UcsOrgX`, `UcsOrgY`.

## DeviceState — 14

- высота и ось Z: `CurrentH`, `CurrentZ`, `TargetHeight`;
- ток и фокус: `DiodeCurrent`, `FocusPos`;
- газ: `GasPressure`, `GasType`, `IsGasOn`;
- лазер: `IsAimingOn`, `IsEmissionOn`, `IsLaserOn`;
- слежение: `IsFollowing`;
- PWM: `PwmFreq`, `PwmRatio`.

## GlobalParams — 36

- единицы: `AccUnit`, `PressureUnit`, `SpeedUnit`, `TimeUnit`;
- газ: `AheadOpenGas`, `GasOpenDelay`, `GasOpenDelay1`, `GasPressure`, `SwitchGasDelay`;
- геометрия и точность: `ArcTolerance`, `CompPathPrec`, `CornerTolerance`, `ShortMoveSize`;
- ускорения и перемещения: `MaxAcc`, `MoveAccX`, `MoveAccY`, `MoveSpeedX`, `MoveSpeedY`, `WalkSpeed`;
- фильтры: `FIRFreq`, `MoveFIRFreq`;
- лазер: `DelayLaserOffDis`, `DiodeCurrent`, `PwmFreq`;
- слежение и высота: `BCS100ZControl`, `DisableFollower`, `EnableFollower`, `FollowWithFOcus`, `MaxFollowHeight`, `ZRange`;
- оптимизация: `CoolPointDelay`, `LeapFrog`, `MoveOptimize`, `PauseBackDistance`;
- защита и сброс: `AllRequireManualReset`, `ProtectMachine`.

## Дополнительные служебные узлы — 9

- `Identity`: `Id`, `Name`, `CypCutIp`, `CypCutPort`, `OpcUaPort`;
- `Connection`: `Connected`, `LastUpdateUtc`, `LastError`, `RawJson`.

`RawJson` сохраняет полный последний ответ CypCut. Поэтому новое поле можно будет
добавить в каталог без потери исходных данных.
