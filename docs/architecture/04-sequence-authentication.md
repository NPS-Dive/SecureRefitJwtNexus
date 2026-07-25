# Sequence Diagram — Authentication

## Purpose

This document shows the authentication flows for both APIs.

There are two authentication flows:

1. External user logs in to `ListReader.Api`.
2. `ListReader.Api` logs in to `ListMaker.Api` using service credentials.

---

## User Login to ListReader.Api
```mermaid
sequenceDiagram
autonumber

actor User as User / API Consumer
participant ReaderSwagger as ListReader Swagger/Postman
participant ReaderAuth as ListReader.Api AuthController
participant ReaderJwt as ListReader JwtTokenService

User->>ReaderSwagger: Enter username/password
ReaderSwagger->>ReaderAuth: POST /api/auth/login
ReaderAuth->>ReaderAuth: Validate static user credentials

alt Valid credentials
ReaderAuth->>ReaderJwt: Generate JWT
ReaderJwt-->>ReaderAuth: JWT token + expiration
ReaderAuth-->>ReaderSwagger: 200 OK LoginResponse
ReaderSwagger-->>User: User copies/applies Bearer token
else Invalid credentials
ReaderAuth-->>ReaderSwagger: 401 Unauthorized
ReaderSwagger-->>User: Login failed
end
```

---

## ListReader.Api Login to ListMaker.Api

```mermaid
sequenceDiagram
autonumber

participant Reader as ListReader.Api
participant Cache as ListMakerAccessTokenCacheService
participant AuthClient as IListMakerAuthApi
participant MakerAuth as ListMaker.Api AuthController
participant MakerJwt as ListMaker JwtTokenService

Reader->>Cache: GetAccessTokenAsync()

alt Cached token is valid
Cache-->>Reader: Return cached ListMaker JWT
else Token missing or near expiration
Cache->>AuthClient: LoginAsync(LoginRequest)
AuthClient->>MakerAuth: POST /api/auth/login
MakerAuth->>MakerAuth: Validate service credentials

alt Valid service credentials
MakerAuth->>MakerJwt: Generate JWT
MakerJwt-->>MakerAuth: JWT token + expiration
MakerAuth-->>AuthClient: 200 OK LoginResponse
AuthClient-->>Cache: LoginResponse
Cache->>Cache: Store token until near expiration
Cache-->>Reader: Return new ListMaker JWT
else Invalid service credentials
MakerAuth-->>AuthClient: 401 Unauthorized
AuthClient-->>Cache: Authentication failed
Cache-->>Reader: Token retrieval failed
end
end
```

---

## Authentication Notes

Both APIs have separate JWT configuration.

This is intentional because they are treated as independent systems.

`ListReader.Api` authenticates the external user.

`ListMaker.Api` authenticates the service/client that calls it.

---

## Swagger Authentication

Both APIs configure Swagger/OpenAPI with a bearer token security definition.

Manual Swagger usage:

1. Call `/api/auth/login`.
2. Copy the returned token.
3. Click Swagger `Authorize`.
4. Enter:

```text
Bearer {token}
```

5. Call protected endpoints.
