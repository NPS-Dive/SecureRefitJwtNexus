# Architecture Diagrams

This document provides a single index of the main architecture diagrams for `SecureRefitJwtNexus`.

---

## 1. System Context
```mermaid
flowchart LR
User["User / API Consumer"]
System["SecureRefitJwtNexus"]
Maker["ListMaker.Api<br/>Separate API system"]

User -->|"Uses JWT-secured API"| System
System -->|"Calls using Refit + JWT"| Maker

classDef person fill:#08427b,color:#ffffff,stroke:#052e56
classDef system fill:#1168bd,color:#ffffff,stroke:#0b4884
classDef external fill:#999999,color:#ffffff,stroke:#666666

class User person
class System system
class Maker external
```

---

## 2. Container Diagram

```mermaid
flowchart LR
User["User / API Consumer"]

Reader["ListReader.Api<br/>https://localhost:7002"]
Maker["ListMaker.Api<br/>https://localhost:7001"]
Client["ListMaker.Client<br/>Refit + IHttpClientFactory"]
Contracts["ListMaker.Contracts<br/>Shared DTOs"]

K6["k6"]
Prometheus["Prometheus"]
Grafana["Grafana<br/>http://127.0.0.1:33000"]

User --> Reader
Reader --> Client
Client --> Maker
Client --> Contracts
Maker --> Contracts

K6 --> Reader
K6 --> Maker
K6 --> Prometheus
Grafana --> Prometheus

classDef api fill:#1168bd,color:#ffffff,stroke:#0b4884
classDef lib fill:#438dd5,color:#ffffff,stroke:#2e6295
classDef tool fill:#999999,color:#ffffff,stroke:#666666
classDef person fill:#08427b,color:#ffffff,stroke:#052e56

class User person
class Reader,Maker api
class Client,Contracts lib
class K6,Prometheus,Grafana tool
```

---

## 3. Main Runtime Flow

```mermaid
sequenceDiagram
autonumber

actor User
participant Reader as ListReader.Api
participant Cache as ListMakerAccessTokenCacheService
participant AuthClient as IListMakerAuthApi
participant ListsClient as IListMakerListsApi
participant Maker as ListMaker.Api

User->>Reader: GET /api/lists/from-list-maker<br/>Bearer ListReader JWT
Reader->>Reader: Validate JWT
Reader->>Cache: Get ListMaker token

alt Cached token valid
Cache-->>Reader: Cached token
else Token missing/near expiration
Cache->>AuthClient: Login to ListMaker
AuthClient->>Maker: POST /api/auth/login
Maker-->>AuthClient: JWT
AuthClient-->>Cache: JWT
Cache-->>Reader: New token
end

Reader->>ListsClient: Request generated list
ListsClient->>Maker: GET /api/lists/generated<br/>Bearer ListMaker JWT
Maker-->>ListsClient: 50 person records
ListsClient-->>Reader: 50 person records
Reader-->>User: 200 OK
```

---

## 4. Observability Flow

```mermaid
flowchart LR
K6["k6 Load Tests"]
APIs["ListMaker.Api / ListReader.Api"]
Prometheus["Prometheus"]
Grafana["Grafana"]
Csv["CSV Results"]

K6 -->|"HTTP requests"| APIs
K6 -->|"remote write"| Prometheus
K6 -->|"append summaries"| Csv
Grafana -->|"query datasource"| Prometheus
```

---

## Documentation Links

Detailed diagrams are available here:

- [C4 Context](./c4-context.md)
- [C4 Container](./c4-container.md)
- [C4 Component - ListMaker.Api](./c4-component-listmaker.md)
- [C4 Component - ListReader.Api](./c4-component-listreader.md)
- [Authentication Sequence](./sequence-authentication.md)
- [ListReader Relay Sequence](./sequence-listreader-relay.md)
- [Observability](./observability.md)
