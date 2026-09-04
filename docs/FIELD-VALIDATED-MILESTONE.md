# Field-Validated Milestone — PCUI TCP/20112

## Scope of this record

This record documents field observations from a separate reference build. It is not a protocol specification, does not include a production dump, and does not claim that the open-source prototype currently implements the PCUI TCP/20112 parser.

## Evidence observed on a real machine

### 2026-09-02 — transport and frame integrity

| Signal | Observed result |
|---|---|
| Transport | TCP connection to port `20112` remained established |
| Frames | Binary frames were received continuously |
| Frame handling | Structural decoding and tag extraction succeeded |
| Integrity | CRC checks were valid; one observed run had 14,892 frames and 0 errors |
| Example state | `NcState.SysState = 1` was observed |
| Diagnostics | `Protocol20112.SourceConnected = true` |

Safe, anonymized example:

```json
{
  "NcState": { "SysState": 1 },
  "Protocol20112": {
    "SourceConnected": true,
    "TagId": 8,
    "TagValue": 1,
    "CrcValid": true,
    "FramesReceived": 14892,
    "CrcErrors": 0
  }
}
```

### 2026-09-04 — end-to-end gateway validation

A later field run validated the broader integration path around the same observed controller transport:

```text
CypCut / PCUI TCP 20112
        ↓
field reference collector / bridge
        ↓
standalone .NET gateway
        ↓
OPC UA endpoint
        ↓
UAExpert browse / read
```

Observed results:

- the controller-side TCP source was reachable and remained connected;
- the gateway exposed a live OPC UA endpoint from a standalone Windows deployment;
- UAExpert successfully browsed and read the generated address space;
- approximately 78 configured parameter nodes were exposed in the tested build;
- live diagnostic values, including the raw normalized JSON block, were visible through OPC UA;
- the gateway was also verified running as a Windows Service;
- the tested deployment supported a machine-specific OPC UA endpoint path and configurable endpoint port.

This is stronger evidence of an end-to-end industrial connectivity path, but it remains evidence from the separate field reference build. The public repository continues to distinguish that evidence from functionality implemented in the open clean-room source.

## What this does not prove

- the complete PCUI frame specification;
- the meaning of every tag or value;
- a stable mapping from every observed numeric state to idle, run, pause, alarm, cutting, or other physical machine states;
- protocol fidelity across other CypCut versions or controller generations;
- production readiness of a public TCP/20112 implementation.

## Clean-room next steps

1. Capture independent, anonymized frame samples for known physical conditions.
2. Write a new parser from those observations only.
3. Add unit tests using synthetic data and anonymized test vectors.
4. Verify each state transition against the physical controller.
5. Publish only the resulting independent source and safe test data.
