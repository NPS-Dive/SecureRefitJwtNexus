# C4 Model — Container Diagram

## Purpose

This diagram shows the main deployable/runtime containers in `ApiIntegrationDemo`.

The core runtime containers are:

- `ListReader.Api`
- `ListMaker.Api`
- Prometheus
- Grafana

The supporting code-level containers/libraries are:

- `ListMaker.Client`
- `ListMaker.Contracts`

---

## Container Diagram
```mermaid
flowchart LR
User["User / API Consumer<br/>Swagger, Postman, k6, browser"]

subgraph Solution["ApiIntegrationDemo Monorepo"]
Reader["ListReader.Api<br/>ASP.NET Core Controller API<br/>https://localhost:7002"]
Maker["ListMaker.Api<br/>ASP.NET Core Controller API<br/>https://localhost:7001"]

Client["ListMaker.Client<br/>Refit client library<br/>IHttpClientFactory integration"]
Contracts["ListMaker.Contracts<br/>Shared DTO contracts"]

K6["k6 Load Tests<br/>perf/k6"]
Prometheus["Prometheus<br/>k6 metrics storage"]
Grafana["Grafana<br/>Dashboards<br/>http://127.0.0.1:33000"]
end

User -->|"Login + JWT<br/>GET relayed list"| Reader

Reader -->|"Uses"| Client
Client -->|"Uses DTOs"| Contracts
Maker -->|"Uses DTOs"| Contracts

Client -->|"HTTP calls with JWT<br/>Refit + IHttpClientFactory"| Maker

K6 -->|"Load test HTTP requests"| Reader
K6 -->|"Load test HTTP requests"| Maker
K6 -->|"Remote write metrics"| Prometheus
Grafana -->|"Prometheus datasource<br/>http://prometheus:9090"| Prometheus

classDef api fill:#1168bd,color:#ffffff,stroke:#0b4884
classDef lib fill:#438dd5,color:#ffffff,stroke:#2e6295
classDef tool fill:#999999,color:#ffffff,stroke:#666666
classDef user fill:#08427b,color:#ffffff,stroke:#052e56

class User user
class Reader,Maker api
class Client,Contracts lib
class K6,Prometheus,Grafana tool
```

---

## Containers

| Container | Type | Responsibility |
|---|---|---|
| `ListReader.Api` | ASP.NET Core Web API | Main consumer-facing API; authenticates users and relays list data |
| `ListMaker.Api` | ASP.NET Core Web API | Authenticates service calls and returns generated list data |
| `ListMaker.Client` | C# class library | Refit interfaces and dependency injection for calling `ListMaker.Api` |
| `ListMaker.Contracts` | C# class library | Shared request/response DTOs |
| k6 | Load testing tool | Executes HTTP load tests |
| Prometheus | Metrics store | Receives k6 metrics through remote write |
| Grafana | Dashboard tool | Visualizes k6/Prometheus metrics |

---

## Runtime Ports

| Runtime Component | URL |
|---|---|
| `ListMaker.Api` | `https://localhost:7001` |
| `ListReader.Api` | `https://localhost:7002` |
| Grafana | `http://127.0.0.1:33000` |

---

## API Communication

The main API-to-API flow is:

```text
User
  -> ListReader.Api
-> ListMaker.Client
-> ListMaker.Api
```

`ListReader.Api` authenticates to `ListMaker.Api` using configured static service credentials.

The received ListMaker JWT is cached until near expiration.