# C4 Model — Component Diagram: ListReader.Api

## Purpose

This diagram shows the main internal components of `ListReader.Api`.

`ListReader.Api` is responsible for:

1. authenticating external users
2. protecting its relay endpoint
3. obtaining a service JWT token from `ListMaker.Api`
4. caching the ListMaker token until near expiration
5. calling `ListMaker.Api` through Refit
6. returning the ListMaker list data to the caller

---

## Component Diagram
```mermaid
flowchart TB
User["User / API Consumer<br/>Swagger / Postman / k6"]

subgraph Reader["ListReader.Api"]
AuthController["AuthController<br/>POST /api/auth/login"]
ListsController["ListsController<br/>GET /api/lists/from-list-maker"]

ReaderJwtInterface["IJwtTokenService"]
ReaderJwtService["JwtTokenService<br/>Creates ListReader JWT"]

GatewayInterface["IListMakerGateway"]
Gateway["ListMakerGateway<br/>Coordinates ListMaker calls"]

TokenCacheInterface["IListMakerAccessTokenCacheService"]
TokenCache["ListMakerAccessTokenCacheService<br/>Caches ListMaker JWT until near expiration"]

CredentialsOptions["ListMakerCredentialsOptions<br/>ListMaker service username/password"]
ReaderJwtOptions["JwtOptions"]
ReaderStaticUsers["StaticUserOptions"]
SwaggerConfig["SwaggerConfiguration"]
end

subgraph ClientLib["ListMaker.Client"]
AuthApi["IListMakerAuthApi<br/>Refit login client"]
ListsApi["IListMakerListsApi<br/>Refit list client"]
ClientOptions["ListMakerClientOptions<br/>BaseUrl=https://localhost:7001"]
DI["ServiceCollectionExtensions<br/>AddRefitClient + IHttpClientFactory"]
end

Maker["ListMaker.Api<br/>https://localhost:7001"]
Contracts["ListMaker.Contracts<br/>LoginRequest, LoginResponse,<br/>PersonListItemDto"]

User -->|"Login"| AuthController
User -->|"Bearer token<br/>request relayed list"| ListsController

AuthController --> ReaderStaticUsers
AuthController --> ReaderJwtInterface
ReaderJwtInterface --> ReaderJwtService
ReaderJwtService --> ReaderJwtOptions

ListsController --> GatewayInterface
GatewayInterface --> Gateway

Gateway --> TokenCacheInterface
TokenCacheInterface --> TokenCache
TokenCache --> CredentialsOptions
TokenCache --> AuthApi

Gateway --> ListsApi

AuthApi -->|"POST /api/auth/login"| Maker
ListsApi -->|"GET /api/lists/generated<br/>Bearer ListMaker token"| Maker

AuthApi --> Contracts
ListsApi --> Contracts
Maker --> Contracts

DI --> AuthApi
DI --> ListsApi
DI --> ClientOptions

SwaggerConfig --> AuthController
SwaggerConfig --> ListsController

classDef controller fill:#1168bd,color:#ffffff,stroke:#0b4884
classDef service fill:#438dd5,color:#ffffff,stroke:#2e6295
classDef config fill:#85bbf0,color:#000000,stroke:#5d82a8
classDef client fill:#f5da81,color:#000000,stroke:#b99e3f
classDef external fill:#999999,color:#ffffff,stroke:#666666

class AuthController,ListsController controller
class ReaderJwtInterface,ReaderJwtService,GatewayInterface,Gateway,TokenCacheInterface,TokenCache service
class CredentialsOptions,ReaderJwtOptions,ReaderStaticUsers,SwaggerConfig,ClientOptions config
class AuthApi,ListsApi,DI,Contracts client
class User,Maker external
```

---

## Main Components

| Component | Responsibility |
|---|---|
| `AuthController` | Authenticates external users and returns ListReader JWT |
| `ListsController` | Exposes protected relay endpoint |
| `IListMakerGateway` | Abstraction for ListMaker integration |
| `ListMakerGateway` | Coordinates token retrieval and list retrieval |
| `IListMakerAccessTokenCacheService` | Abstraction for service-token caching |
| `ListMakerAccessTokenCacheService` | Logs in to ListMaker and caches token |
| `IListMakerAuthApi` | Refit client for ListMaker login |
| `IListMakerListsApi` | Refit client for ListMaker list endpoint |
| `ListMakerCredentialsOptions` | Stores service credentials for ListMaker |
| `ListMakerClientOptions` | Stores ListMaker base URL |
| `SwaggerConfiguration` | Adds Swagger JWT bearer support |

---

## Main Endpoints

| Endpoint | Method | Auth Required | Description |
|---|---:|---:|---|
| `/api/auth/login` | POST | No | Authenticates user and returns ListReader JWT |
| `/api/lists/from-list-maker` | GET | Yes | Calls ListMaker and returns generated list |

---

## API-to-API Flow

```text
User
  -> ListReader.Api
-> IListMakerGateway
-> IListMakerAccessTokenCacheService
-> IListMakerAuthApi
-> ListMaker.Api login
-> IListMakerListsApi
-> ListMaker.Api generated list
```

---

## Design Notes

`ListReader.Api` does not know ListMaker implementation details.

It only knows:

1. ListMaker base URL.
2. ListMaker login contract.
3. ListMaker list contract.
4. ListMaker service credentials.

This keeps the systems properly separated.

