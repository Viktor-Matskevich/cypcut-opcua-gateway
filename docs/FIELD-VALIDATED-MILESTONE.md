# Field-Validated Milestone — PCUI TCP/20112

## Scope of this record

This record documents a field observation. It is not a protocol specification, does not include a production dump, and does not claim that the open-source prototype implements PCUI TCP/20112.

## Evidence observed on a real machine

| Signal | Observed result |
|---|---|
| Transport | TCP connection to port `20112` remained established |
| Frames | Binary frames were received continuously |
| Frame handling | Structural decoding and tag extraction succeeded |
| Integrity | CRC checks were valid; one observed run had 14,892 frames and 0 errors |
| Example state | `NcState.SysState = 1` was observed |
| Diagnostics | `Protocol20112.SourceConnected = true` |

## Safe, anonymized example

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

## What this does not prove

- the complete frame specification;
- the meaning of every tag or value;
- a stable mapping from `SysState = 1` to a named operational state;
- production readiness of a TCP/20112 implementation.

## Clean-room next steps

1. Capture independent, anonymized frame samples for known physical conditions.
2. Write a new parser from those observations only.
3. Add unit tests using synthetic data and anonymized test vectors.
4. Verify each state transition against the physical controller.
5. Publish only the resulting independent source and safe test data.
