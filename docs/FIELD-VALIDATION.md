# Field validation checklist

Use this checklist on an isolated industrial network before changing the project
status from `Experimental` to `Field tested`.

## 1. Configure the real network locally

Do not commit real addresses. Edit only the deployment copy of:

- `config/gateway.json` — central Windows server IP;
- `config/machines.csv` — machine IP, source port, output port, and route.

## 2. Validate connectivity

Run `RUN-01-VALIDATE.cmd` and confirm:

- the central IP belongs to the server;
- the source TCP port is reachable;
- every enabled machine has a unique ID and OPC UA port.

## 3. Run in console mode

Run `RUN-02-START-CONSOLE.cmd`. Confirm that the log shows both:

- the expected `http://...` polling address;
- the expected `opc.tcp://...` output address.

## 4. Verify data during an actual cutting cycle

Connect UaExpert or another OPC UA client and record which nodes change while the
machine is idle, jogging, cutting, paused, and alarmed. Priority fields:

- `AxisX`, `AxisY`, `CurrentZ`, `CurrentH`;
- `TaskName`, `SysState`, `CutPercent`, `WorkSpeed`;
- `LaserPower`, `IsLaserOn`, `IsEmissionOn`;
- `GasPressure`, `GasType`, `IsGasOn`;
- `PwmFreq`, `PwmRatio`, `DiodeCurrent`;
- `AlarmCount`, `AlarmMsg`.

## 5. Prepare public evidence

Before committing any sample:

- replace machine, server, and plant names;
- replace all addresses with `192.0.2.x` examples;
- remove paths, task names, serial numbers, customer data, and tokens;
- retain only a small representative JSON fixture.

## Acceptance criteria

- source connection remains stable for at least 30 minutes;
- OPC UA subscription updates without reconnect loops;
- at least state, coordinates, task, and cutting indicators are confirmed;
- service restarts successfully with Windows;
- no production identifiers appear in the repository.
