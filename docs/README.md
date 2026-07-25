# ApiIntegrationDemo Documentation

This folder contains project documentation for `SecureRefitJwtNexus`.

`SecureRefitJwtNexus` is a demo ASP.NET Core solution that shows secure API-to-API communication between two independent APIs:

- `ListMaker.Api`
- `ListReader.Api`

The solution demonstrates:

- ASP.NET Core Controller APIs
- JWT authentication
- Swagger/OpenAPI security configuration
- Refit-based API communication
- `IHttpClientFactory`
- token caching between APIs
- unit tests
- integration tests
- k6 load tests
- Prometheus/Grafana observability
- C4 architecture documentation

---

## Documentation Structure
```text
docs/
├── README.md
│
├── architecture/
│   ├── c4-context.md
│   ├── c4-container.md
│   ├── c4-component-listmaker.md
│   ├── c4-component-listreader.md
│   ├── sequence-authentication.md
│   ├── sequence-listreader-relay.md
│   ├── observability.md
│   └── diagrams.md
│
└── testing/
├── test-strategy.md
├── unit-tests.md
├── integration-tests.md
├── load-tests.md
└── test-execution.md
```
---

## Architecture Documentation

See:

- [C4 System Context](./architecture/c4-context.md)
- [C4 Container Diagram](./architecture/c4-container.md)
- [C4 Component Diagram - ListMaker.Api](./architecture/c4-component-listmaker.md)
- [C4 Component Diagram - ListReader.Api](./architecture/c4-component-listreader.md)
- [Authentication Sequence](./architecture/sequence-authentication.md)
- [ListReader Relay Sequence](./architecture/sequence-listreader-relay.md)
- [Observability](./architecture/observability.md)
- [All Diagrams](./architecture/diagrams.md)

---

## Testing Documentation

See:

- [Test Strategy](./testing/test-strategy.md)
- [Unit Tests](./testing/unit-tests.md)
- [Integration Tests](./testing/integration-tests.md)
- [Load Tests](./testing/load-tests.md)
- [Test Execution Guide](./testing/test-execution.md)

---

## Main Runtime Ports

| Application | URL |
|---|---|
| ListMaker.Api | `https://localhost:7001` |
| ListReader.Api | `https://localhost:7002` |
| Grafana | `http://127.0.0.1:33000` |

---

## Important Design Decisions

| Decision | Selected Option |
|---|---|
| API style | Controller APIs |
| Authentication | JWT bearer tokens |
| Users | Static demo users |
| Seed data | Stable deterministic list data |
| Gender values | String values |
| API-to-API client | Refit |
| HTTP client management | `IHttpClientFactory` |
| Token handling | Cache ListMaker token until near expiration |
| HTTP resilience | Simple timeout |
| Observability | k6 + Prometheus + Grafana |

