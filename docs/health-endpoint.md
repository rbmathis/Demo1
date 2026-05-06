# ❤️ Health Endpoint

This document describes the lightweight health metadata endpoint for Demo1.

## Endpoint

- **Method**: `GET`
- **URL**: `/health`

## Authentication

No authentication is required. The endpoint is publicly accessible for platform health checks.

## Response Fields

| Field         | Type       | Description                                                                      |
| ------------- | ---------- | -------------------------------------------------------------------------------- |
| `version`     | `string`   | Application version read from the `VERSION` file.                                |
| `uptime`      | `string`   | Human-readable application uptime value.                                         |
| `timestamp`   | `datetime` | Current UTC timestamp when the response is generated.                            |
| `environment` | `string`   | Current hosting environment name (for example, `Development` or `Production`).   |

## Example Response

```json
{
  "version": "1.2.3",
  "uptime": "00:12:34.5678901",
  "timestamp": "2026-01-01T12:00:00Z",
  "environment": "Production"
}
```

## Monitoring Use Case

Use `GET /health` for external monitoring systems and load balancer probes to confirm the app is running and returning basic runtime metadata.
