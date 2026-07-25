# C4 Model — Component Diagram: ListMaker.Api

## Purpose

This diagram shows the main internal components of `ListMaker.Api`.

`ListMaker.Api` is responsible for:

1. authenticating configured users
2. issuing JWT tokens
3. protecting the generated-list endpoint
4. returning a stable seeded list of 50 entries

---

## Component Diagram
```mermaid
flowchart TB
Client["External Client<br/>ListReader.Api / Swagger / k6"]

subgraph Api["ListMaker.Api"]
AuthController["AuthController<br/>POST /api/auth/login"]
ListsController["ListsController<br/>GET /api/lists/generated"]

JwtServiceInterface["IJwtTokenService"]
JwtService["JwtTokenService<br/>Creates signed JWT tokens"]

PersonProviderInterface["IPersonListProvider"]
PersonProvider["StablePersonListProvider<br/>Returns deterministic list of 50 people"]

JwtOptions["JwtOptions<br/>Issuer, Audience, Key, Expiration"]
StaticUserOptions["StaticUserOptions<br/>Demo username/password"]
SwaggerConfig["SwaggerConfiguration<br/>Bearer security scheme"]
end

Contracts["ListMaker.Contracts<br/>LoginRequest, LoginResponse,<br/>PersonListItemDto"]

Client -->|"Login request"| AuthController
Client -->|"Bearer token request"| ListsController

AuthController -->|"Validates credentials"| StaticUserOptions
AuthController -->|"Creates token"| JwtServiceInterface
JwtServiceInterface --> JwtService
JwtService --> JwtOptions
AuthController -->|"Returns LoginResponse"| Contracts

ListsController -->|"Reads list"| PersonProviderInterface
PersonProviderInterface --> PersonProvider
PersonProvider -->|"Returns List<PersonListItemDto>"| Contracts

SwaggerConfig --> AuthController
SwaggerConfig --> ListsController

classDef controller fill:#1168bd,color:#ffffff,stroke:#0b4884
classDef service fill:#438dd5,color:#ffffff,stroke:#2e6295
classDef config fill:#85bbf0,color:#000000,stroke:#5d82a8
classDef external fill:#999999,color:#ffffff,stroke:#666666
classDef contract fill:#f5da81,color:#000000,stroke:#b99e3f

class AuthController,ListsController controller
class JwtServiceInterface,JwtService,PersonProviderInterface,PersonProvider service
class JwtOptions,StaticUserOptions,SwaggerConfig config
class Client external
class Contracts contract
```

---

## Main Components

| Component | Responsibility |
|---|---|
| `AuthController` | Accepts login request and returns JWT token |
| `ListsController` | Returns protected generated list |
| `IJwtTokenService` | Abstraction for JWT generation |
| `JwtTokenService` | Creates signed JWT tokens |
| `IPersonListProvider` | Abstraction for list generation |
| `StablePersonListProvider` | Produces stable list of 50 person records |
| `JwtOptions` | JWT configuration |
| `StaticUserOptions` | Static demo credentials |
| `SwaggerConfiguration` | Adds Swagger JWT bearer support |

---

## Main Endpoints

| Endpoint | Method | Auth Required | Description |
|---|---:|---:|---|
| `/api/auth/login` | POST | No | Authenticates configured user and returns JWT |
| `/api/lists/generated` | GET | Yes | Returns stable seeded list |

---

## List Data Contract

Each generated list entry contains:

| Field | Type | Rule |
|---|---|---|
| `name` | string | Max 50 characters |
| `family` | string | Max 50 characters |
| `age` | integer | Between 18 and 65 |
| `gender` | string | `male`, `female`, or `non-binary` |

---

## Design Notes

`ListMaker.Api` intentionally uses stable seeded data instead of truly random output.

Reason:

- repeatable tests
- predictable load testing
- easier demo verification
- deterministic API behavior

This is correct for this demo project.

