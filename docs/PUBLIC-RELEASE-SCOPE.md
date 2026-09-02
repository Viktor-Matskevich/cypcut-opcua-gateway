# Public Release Scope and Safety Boundary

## Included

- independently authored .NET source for the HTTP/JSON-to-OPC-UA prototype;
- synthetic JSON samples and documentation-only addresses;
- high-level, anonymized field observations;
- build instructions, security guidance, and clean-room validation plan.

## Excluded

- binaries, source, configuration, logs, or documentation from legacy adapters;
- integration details of proprietary monitoring platforms;
- customer-specific addresses, machine identifiers, names, credentials, and production telemetry;
- copied or derivative code from closed-source software;
- raw PCUI traffic captures unless specifically anonymized and independently approved for publication.

## Publication rule

Every new file must be reviewed for private addresses, company references, serial numbers, credentials, proprietary URLs, and non-independent code before it is committed to the public repository.
