# 🛡️ SecureRefitJwtNexus

<div align="center">

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-Modern-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-5C2D91?style=for-the-badge&logo=dotnet)
![Refit](https://img.shields.io/badge/Refit-Typed_HTTP_Client-FF6F00?style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT-Bearer_Auth-000000?style=for-the-badge&logo=jsonwebtokens)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

![xUnit](https://img.shields.io/badge/xUnit-Unit_Tests-512BD4?style=for-the-badge)
![Integration Tests](https://img.shields.io/badge/Integration_Tests-API_Verification-0A66C2?style=for-the-badge)
![k6](https://img.shields.io/badge/k6-Load_Testing-7D64FF?style=for-the-badge&logo=k6)
![Prometheus](https://img.shields.io/badge/Prometheus-Metrics-E6522C?style=for-the-badge&logo=prometheus)
![Grafana](https://img.shields.io/badge/Grafana-Dashboards-F46800?style=for-the-badge&logo=grafana)
![Docker](https://img.shields.io/badge/Docker-Observability_Stack-2496ED?style=for-the-badge&logo=docker&logoColor=white)

![Visual Studio 2026](https://img.shields.io/badge/Visual_Studio-2026-5C2D91?style=for-the-badge&logo=visual-studio)
![PowerShell](https://img.shields.io/badge/PowerShell-Automation-5391FE?style=for-the-badge&logo=powershell&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)

</div>

---

## 📌 Overview

**SecureRefitJwtNexus** is a structured .NET 10 ASP.NET Core demonstration project focused on **secure API-to-API integration**, **clean architecture**, **JWT authentication**, **Refit-based typed HTTP clients**, **automated testing**, and **observability-driven QA/QC**.

The project demonstrates how one API, `ListReader.Api`, can securely consume another independent API, `ListMaker.Api`, through HTTP using:

- JWT bearer authentication
- Refit typed clients
- `IHttpClientFactory`
- downstream access-token caching
- isolated shared contracts
- unit and integration testing
- k6 load testing
- Prometheus/Grafana observability
- Mermaid-based C4 architecture documentation

This repository is designed from a **Senior .NET Developer**, **QA/QC Lead**, and **architecture-focused engineering** perspective.

---

## 📚 Table of Contents

- [Overview](#-overview)
- [Main Goal](#-main-goal)
- [Architecture Rule](#-architecture-rule)
- [Solution Architecture](#-solution-architecture)
- [Repository Structure](#-repository-structure)
- [Services Overview](#-services-overview)
- [Authentication Model](#-authentication-model)
- [Main API Flow](#-main-api-flow)
- [Runtime Ports](#-runtime-ports)
- [How to Run](#-how-to-run)
- [Testing Strategy](#-testing-strategy)
- [Load Testing](#-load-testing)
- [Observability](#-observability)
- [Architecture Documentation](#-architecture-documentation)
- [Current Project Status](#-current-project-status)
- [QA/QC Notes](#-qaqc-notes)
- [Production Considerations](#-production-considerations)
- [Engineering Principles](#-engineering-principles)
- [Portfolio Value](#-portfolio-value)
- [License](#-license)

---

## 🎯 Main Goal

The main goal of **SecureRefitJwtNexus** is to demonstrate a clean and secure pattern for API-to-API communication in ASP.NET Core.

The project focuses on:

- Building two independent ASP.NET Core Controller APIs
- Protecting both APIs with JWT authentication
- Calling a downstream API using Refit
- Managing HTTP clients with `IHttpClientFactory`
- Caching downstream JWT tokens until near expiration
- Separating shared contracts from service implementations
- Creating a testable and maintainable monorepo structure
- Documenting architecture with C4-style diagrams
- Validating behavior with unit, integration, and load tests
- Observing load-test metrics with Prometheus and Grafana

---

## 🧱 Architecture Rule

This project follows one strict architectural rule:

> **`ListReader.Api` must never directly reference `ListMaker.Api` implementation code.**

The only allowed communication path is:
```text
[ListReader.Api]
---> [ListMaker.Client]
---> [HTTP + JWT + Refit]
---> [ListMaker.Api]
```

Shared DTOs are placed in a dedicated contracts library:

```text
[ListMaker.Contracts]
```

This prevents implementation coupling and keeps service boundaries clean.

---

## 🏗️ Solution Architecture

At a high level, the system contains:

```text
External User / API Consumer
|
| Login + Bearer Token
v
ListReader.Api
|
| Refit + IHttpClientFactory + Cached ListMaker JWT
v
ListMaker.Api
|
| Stable generated list
v
ListReader.Api
|
v
External User / API Consumer

`ListReader.Api` acts as the user-facing API.

`ListMaker.Api` acts as a downstream data provider.
```

---

## 📂 Repository Structure

The repository branding is **SecureRefitJwtNexus**.

Depending on the current local project state, the solution file may be:

```text
ApiIntegrationDemo.sln
```

or:

```text
ApiIntegrationDemo.slnx
```

Final repository layout:

```text
SecureRefitJwtNexus/
│
├── ApiIntegrationDemo.sln
│   or
├── ApiIntegrationDemo.slnx
│
├── Directory.Build.props
├── Directory.Packages.props
├── README.md
│
├── docs/
│   ├── README.md
│   │
│   ├── architecture/
│   │   ├── c4-context.md
│   │   ├── c4-container.md
│   │   ├── c4-component-listmaker.md
│   │   ├── c4-component-listreader.md
│   │   ├── sequence-authentication.md
│   │   ├── sequence-listreader-relay.md
│   │   ├── observability.md
│   │   └── diagrams.md
│   │
│   └── testing/
│       ├── test-strategy.md
│       ├── unit-tests.md
│       ├── integration-tests.md
│       ├── load-tests.md
│       └── test-execution.md
│
├── src/
│   ├── BuildingBlocks/
│   │   └── ListMaker.Contracts/
│   │
│   ├── Clients/
│   │   └── ListMaker.Client/
│   │
│   └── Services/
│       ├── ListMaker/
│       │   └── ListMaker.Api/
│       │
│       └── ListReader/
│           └── ListReader.Api/
│
├── tests/
│   ├── Unit/
│   └── Integration/
│
└── perf/
├── README.md
├── docker-compose.grafana.yml
│
├── prometheus/
│   └── prometheus.yml
│
├── grafana/
│   ├── dashboards/
│   └── provisioning/
│
└── k6/
├── README.md
├── config/
├── helpers/
├── results/
├── *.js
└── *.ps1
```

---

## ⚙️ Services Overview

## 1. `ListMaker.Api`

`ListMaker.Api` is the downstream data-provider API.

### Responsibilities

- Authenticate configured service callers
- Issue JWT tokens
- Protect its generated-list endpoint
- Return a deterministic list of 50 person records

### Base URL

```text
https://localhost:7001
```

### Main Endpoints

| Method | Endpoint | Auth Required | Description |
|---|---|---:|---|
| `POST` | `/api/auth/login` | No | Authenticates caller and returns JWT |
| `GET` | `/api/lists/generated` | Yes | Returns stable generated list |

---

## 2. `ListReader.Api`

`ListReader.Api` is the user-facing API.

### Responsibilities

- Authenticate external/demo users
- Issue its own JWT tokens
- Protect its relay endpoint
- Authenticate to `ListMaker.Api` using service credentials
- Cache the downstream `ListMaker.Api` JWT token
- Call `ListMaker.Api` using Refit
- Return relayed list data to the caller

### Base URL

```text
https://localhost:7002
```

### Main Endpoints

| Method | Endpoint | Auth Required | Description |
|---|---|---:|---|
| `POST` | `/api/auth/login` | No | Authenticates external user and returns JWT |
| `GET` | `/api/lists/from-list-maker` | Yes | Calls `ListMaker.Api` and returns generated list |

---

## 3. `ListMaker.Client`

`ListMaker.Client` is the typed client library used by `ListReader.Api`.

### Responsibilities

- Define Refit interfaces for `ListMaker.Api`
- Register Refit clients
- Integrate with `IHttpClientFactory`
- Keep downstream HTTP communication isolated

---

## 4. `ListMaker.Contracts`

`ListMaker.Contracts` contains shared transport contracts.

### Responsibilities

- Define login request/response DTOs
- Define generated-list response DTOs
- Avoid direct implementation references between APIs

---

## 🔐 Authentication Model

The solution demonstrates two independent JWT authentication layers.

---

### 1. External User Authentication

The external user logs in to `ListReader.Api`.

```text
User
  -> POST /api/auth/login
  -> ListReader.Api
  -> receives ListReader JWT
```

The user then calls protected `ListReader.Api` endpoints with:

```text
Authorization: Bearer {ListReaderJwt}
```

---

### 2. Internal Service Authentication

`ListReader.Api` authenticates to `ListMaker.Api` using configured service credentials.

```text
ListReader.Api
  -> POST /api/auth/login
  -> ListMaker.Api
  -> receives ListMaker JWT
```

Then `ListReader.Api` calls `ListMaker.Api` with:

```text
Authorization: Bearer {ListMakerJwt}
```

---

### 3. Downstream Token Caching

`ListReader.Api` caches the `ListMaker.Api` JWT token until near expiration.

This avoids unnecessary repeated login calls and improves:

- latency
- throughput
- downstream service efficiency
- load-test stability
- metric readability

---

## 🔄 Main API Flow

Successful relay flow:

```text
1. User logs in to ListReader.Api
2. User receives ListReader JWT
3. User calls GET /api/lists/from-list-maker
4. ListReader.Api validates the user JWT
5. ListReader.Api gets cached ListMaker JWT or logs in to ListMaker.Api
6. ListReader.Api calls ListMaker.Api through Refit
7. ListMaker.Api validates the ListMaker JWT
8. ListMaker.Api returns the generated list
9. ListReader.Api returns the list to the user
```

---

## 🌐 Runtime Ports

| Component | URL |
|---|---|
| `ListMaker.Api` | `https://localhost:7001` |
| `ListReader.Api` | `https://localhost:7002` |
| Grafana | `http://127.0.0.1:33000` |

Grafana uses host port `33000` to avoid Windows-reserved port conflicts around common development ports such as `3000`.

---

## 🚀 How to Run

## Prerequisites

Recommended tools:

- .NET 10 SDK
- Visual Studio 2026
- Docker Desktop
- PowerShell
- k6
- Git

---

## 1. Clone the Repository

```powershell
git clone <repository-url>
cd SecureRefitJwtNexus
```

---

## 2. Restore Dependencies

```powershell
dotnet restore
```

---

## 3. Build the Solution

If using `.sln`:

```powershell
dotnet build .\ApiIntegrationDemo.sln
```

If using `.slnx`:

```powershell
dotnet build .\ApiIntegrationDemo.slnx
```

---

## 4. Run the APIs

Run both APIs:

- `ListMaker.Api`
- `ListReader.Api`

You can run them from Visual Studio or through the .NET CLI.

Expected URLs:

```text
https://localhost:7001
https://localhost:7002
```

---

## 5. Use Swagger

For each API:

1. Open Swagger UI.
2. Call:

```text
POST /api/auth/login
```

3. Copy the returned JWT token.
4. Click **Authorize**.
5. Enter:

```text
Bearer {token}
```

6. Call the protected endpoint.

---

## 🧪 Testing Strategy

The testing documentation is located under:

```text
docs/testing/
```

Main testing documents:

| Document | Purpose |
|---|---|
| `test-strategy.md` | Overall QA/QC test strategy |
| `unit-tests.md` | Unit test scope and rules |
| `integration-tests.md` | Integration test scope and execution |
| `load-tests.md` | Load testing scenarios |
| `test-execution.md` | Step-by-step execution guide |

---

## Unit Tests

Unit tests focus on:

- service behavior
- token generation logic
- token caching behavior
- controller/service collaboration
- isolated business rules

Run tests:

```powershell
dotnet test
```

---

## Integration Tests

Integration tests focus on:

- API endpoint behavior
- authentication flow validation
- protected endpoint verification
- downstream integration behavior
- request/response contract correctness

Run all tests:

```powershell
dotnet test
```

---

## ⚡ Load Testing

Load tests are located under:

```text
perf/k6/
```

PowerShell scripts are used to standardize test execution.

Example scripts:

```text
run-listmaker-login-load.ps1
run-listmaker-generated-list-load.ps1
run-listreader-login-load.ps1
run-listreader-relay-load.ps1
run-all-load-tests.ps1
```

Load tests cover:

- `ListMaker.Api` login
- `ListMaker.Api` generated-list endpoint
- `ListReader.Api` login
- `ListReader.Api` relay endpoint

---

## Run All Load Tests

From the repository root:

```powershell
.\perf\k6\run-all-load-tests.ps1
```

The scripts support:

- consistent execution
- k6 result collection
- Prometheus metric publishing
- CSV summary result automation

---

## 📈 Observability

The local observability stack uses:

- k6
- Prometheus
- Grafana

---

## Start Prometheus and Grafana

```powershell
docker compose -f .\perf\docker-compose.grafana.yml up -d
```

Open Grafana:

```text
http://127.0.0.1:33000
```

---

## k6 Metrics Publishing

k6 metrics are published to Prometheus using:

```text
-o experimental-prometheus-rw
```

The load-test runners may configure remote-write settings such as:

```powershell
$env:K6_PROMETHEUS_RW_PUSH_INTERVAL = "5s"
```

---

## Grafana Dashboard Persistence

Dashboards created or edited in the Grafana UI should be exported as JSON and committed to:

```text
perf/grafana/dashboards/
```

This keeps dashboard configuration version-controlled.

---

## 🧭 Architecture Documentation

Architecture documentation is located under:

text
docs/architecture/

Available documents:

| Document | Description |
|---|---|
| `c4-context.md` | System Context diagram |
| `c4-container.md` | Container diagram |
| `c4-component-listmaker.md` | Component diagram for `ListMaker.Api` |
| `c4-component-listreader.md` | Component diagram for `ListReader.Api` |
| `sequence-authentication.md` | Authentication sequence diagrams |
| `sequence-listreader-relay.md` | Main relay flow sequence diagram |
| `observability.md` | Observability architecture |
| `diagrams.md` | Central diagram index |

The diagrams are written with Mermaid so they remain:

- source-control friendly
- easy to maintain
- readable in markdown
- suitable for technical documentation

---

## ✅ Current Project Status

The project is in a finalized implementation and documentation state.

| Area | Status |
|---|---:|
| Monorepo structure | ✅ Complete |
| Shared contracts | ✅ Complete |
| `ListMaker.Api` | ✅ Complete |
| `ListMaker.Client` | ✅ Complete |
| `ListReader.Api` | ✅ Complete |
| JWT authentication | ✅ Complete |
| Swagger bearer configuration | ✅ Complete |
| Downstream token caching | ✅ Complete |
| Unit test documentation | ✅ Complete |
| Integration test documentation | ✅ Complete |
| Load-test structure | ✅ Complete |
| k6 automation scripts | ✅ Complete |
| Prometheus/Grafana structure | ✅ Complete |
| Architecture documentation | ✅ Complete |
| C4 diagrams | ✅ Complete |
| Final README | ✅ Complete |

---

## 🧪 QA/QC Notes

This repository was built with a QA/QC mindset.

Key quality goals:

- deterministic data generation
- repeatable test execution
- isolated services
- testable components
- clear dependency boundaries
- documented test strategy
- documented execution flow
- performance visibility through k6, Prometheus, and Grafana

The generated list is intentionally stable instead of random to support:

- predictable test assertions
- consistent performance baselines
- reproducible demos
- reliable debugging

---

## ⚠️ Production Considerations

This is a demonstration project.

It intentionally uses:

- static configured users
- static service credentials
- local JWT configuration
- deterministic seeded data
- local developer-focused observability

For production systems, replace demo authentication and secret handling with enterprise-grade solutions such as:

- Microsoft Entra ID
- Keycloak
- Auth0
- Duende IdentityServer
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- environment-based secure configuration

Never commit real production secrets such as:

- passwords
- API keys
- JWT signing keys
- connection strings
- service credentials

---

## 🧠 Engineering Principles

This project demonstrates:

- Clean Code
- SOLID principles
- Dependency Injection
- strict service boundaries
- contract-based communication
- typed HTTP clients
- secure API-to-API communication
- token caching
- testability
- observability
- documentation-driven delivery
- QA/QC-oriented engineering discipline

---

## 💼 Portfolio Value

**SecureRefitJwtNexus** can be used as a portfolio project to demonstrate:

- senior .NET backend development
- ASP.NET Core API architecture
- JWT-secured service communication
- Refit and `IHttpClientFactory` usage
- clean monorepo organization
- practical QA/QC strategy
- load testing and observability
- architecture documentation with C4 diagrams
- technical leadership mindset

---

## 📄 License

This project is licensed under the MIT License.

---

<div align="center">

**Built with a focus on Clean Code, SOLID principles, secure integration, observability, and QA/QC excellence.**

</div>
