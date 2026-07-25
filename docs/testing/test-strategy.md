# Test Strategy

## Project

`ApiIntegrationDemo`

This project demonstrates secure API-to-API communication between two ASP.NET Core APIs:

- `ListMaker.Api`
- `ListReader.Api`

The solution uses:

- JWT authentication
- Refit
- `IHttpClientFactory`
- token caching
- Swagger/OpenAPI
- unit tests
- integration tests
- k6 load tests
- Prometheus
- Grafana

---

## Test Objectives

The testing strategy verifies that:

1. Authentication works correctly for both APIs.
2. JWT-protected endpoints reject unauthorized requests.
3. Authorized users can access protected endpoints.
4. `ListReader.Api` can securely communicate with `ListMaker.Api`.
5. Refit client contracts are correctly configured.
6. Token caching avoids unnecessary repeated login calls.
7. APIs remain stable under basic load scenarios.
8. Load test metrics can be collected and visualized through Prometheus and Grafana.

---

## Test Levels

The project contains the following test levels:

| Test Level | Location | Purpose |
|---|---|---|
| Unit Tests | `tests/Unit/` | Test isolated services/classes |
| Integration Tests | `tests/Integration/` | Test API behavior through HTTP pipeline |
| Load Tests | `perf/k6/` | Test API behavior under simulated load |
| Observability | `perf/` | Collect and visualize k6 metrics |

---

## Testing Tools

| Tool | Purpose |
|---|---|
| xUnit | Unit and integration testing |
| Microsoft.AspNetCore.Mvc.Testing | ASP.NET Core integration testing |
| FluentAssertions | Readable assertions |
| Refit | Typed HTTP client integration |
| k6 | Load and performance testing |
| Prometheus | Time-series metrics storage |
| Grafana | Metrics visualization |
| Docker Compose | Local observability stack |

---

## Test Scope

### In Scope

The test suite covers:

- login endpoint behavior
- invalid credential behavior
- JWT-protected endpoint behavior
- stable list generation behavior
- `ListReader.Api` relay behavior
- Refit client registration
- ListMaker access token caching
- load behavior for login/list/relay endpoints
- k6-to-Prometheus metric publishing
- Grafana dashboard visualization

### Out of Scope

The following items are intentionally out of scope for this demo project:

- real user database
- refresh token flow
- distributed tracing
- production-grade secret management
- Kubernetes deployment
- cloud hosting
- persistent application database
- advanced chaos testing

---

## Test Data Strategy

The project uses static/demo credentials configured through application settings.

The APIs use deterministic seeded data where applicable so that tests remain predictable and repeatable.

This is suitable for a demo and portfolio-grade API integration solution.

---

## Quality Goals

The project aims to demonstrate:

1. Clean API boundaries.
2. Predictable authentication behavior.
3. Secure internal API communication.
4. Maintainable client integration using Refit.
5. Repeatable test execution.
6. Observable performance testing.
7. Clear documentation suitable for future team handover.

---

## Test Execution Environments

The main target environment is local development on Windows using PowerShell.

The expected tools are:

- .NET SDK
- Docker Desktop
- k6
- PowerShell
- browser for Swagger/Grafana

---

## Completion Criteria

Step 7.8 is considered complete when:

- unit test documentation exists
- integration test documentation exists
- load test documentation exists
- test execution documentation exists
- `perf/README.md` explains observability startup
- `perf/k6/README.md` explains k6 execution
- root `README.md` can later link to these documents
