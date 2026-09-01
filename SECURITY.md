# Security policy

## Supported versions

Security fixes are applied to the latest version on the `main` branch.

## Deployment guidance

- Do not expose the HTTP source port or OPC UA ports to the public internet.
- Place the gateway in a segmented industrial network.
- Allow inbound OPC UA access only from approved MDC, MES, or SCADA hosts.
- Replace anonymous access and `Security=None` with certificate-based policies
  before production use.
- Protect configuration files because they describe the machine network.
- Review `Connection/RawJson` before exporting logs or diagnostics.

## Reporting a vulnerability

Open a GitHub issue without production IP addresses, credentials, serial numbers,
customer names, or complete machine responses. For sensitive reports, contact the
repository owner privately through the GitHub profile.
