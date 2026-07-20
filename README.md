# ApiIntegrationDemo

`ApiIntegrationDemo` is a .NET 10 ASP.NET Core demo solution built with a microservice-style structure.

It contains two APIs:

1. `ListMaker.Api`
2. `ListReader.Api`


## Main Goal

The solution demonstrates how one API can call another API securely using:

- ASP.NET Core controller-based APIs
- JWT authentication
- Swagger/OpenAPI
- `IHttpClientFactory`
- Refit
- cached downstream JWT tokens
- clean monorepo organization
- unit-testable structure

## Services

### ListMaker.Api

Base URL:
```text
https://localhost:7001

```

Responsibilities:

- Authenticate service callers.
- Issue JWT tokens.
- Generate and expose a stable seeded list of 50 people.

Planned endpoints:

http
POST /api/auth/login
GET  /api/lists/generated

### ListReader.Api

Base URL:

```text
https://localhost:7002

```

Responsibilities:

- Authenticate external/demo users.
- Issue JWT tokens.
- Call `ListMaker.Api` using Refit and `IHttpClientFactory`.
- Return the data received from `ListMaker.Api`.

Planned endpoints:

- http
- POST /api/auth/login
- GET  /api/lists/from-maker

## Important Architecture Rule

`ListReader.Api` must not reference `ListMaker.Api` directly.

Instead:

```text
ListReader.Api -> ListMaker.Client -> HTTP/ReFit -> ListMaker.Api

```

Shared request/response DTOs live in:

```text
ListMaker.Contracts

```

## Project Structure

```text
src/
  BuildingBlocks/
ListMaker.Contracts/
  Clients/
ListMaker.Client/
  Services/
ListMaker/
ListMaker.Api/
ListReader/
ListReader.Api/

```

## Development Notes

This demo will use static configured users.

That is acceptable for a learning/demo project.

For production systems, use a real identity provider such as:

- Microsoft Entra ID
- Keycloak
- Auth0
- OpenIddict
- Duende IdentityServer

Never store real production passwords or signing keys in source control.


---

# 17. `ListMaker.Contracts.csproj`

Path:

```text
src/BuildingBlocks/ListMaker.Contracts/ListMaker.Contracts.csproj

```
