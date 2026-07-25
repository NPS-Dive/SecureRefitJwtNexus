# Sequence Diagram — ListReader Relay Flow

## Purpose

This document shows how `ListReader.Api` calls `ListMaker.Api` and returns the generated list to the external user.

This is the central API integration flow of the project.

---

## Relay Sequence
```mermaid
sequenceDiagram
autonumber

actor User as User / API Consumer
participant ReaderApi as ListReader.Api
participant Gateway as ListMakerGateway
participant TokenCache as ListMakerAccessTokenCacheService
participant AuthClient as IListMakerAuthApi
participant ListsClient as IListMakerListsApi
participant MakerApi as ListMaker.Api

User->>ReaderApi: GET /api/lists/from-list-maker<br/>Authorization: Bearer ListReaderJwt
ReaderApi->>ReaderApi: Validate ListReader JWT

alt ListReader JWT is invalid or missing
ReaderApi-->>User: 401 Unauthorized
else ListReader JWT is valid
ReaderApi->>Gateway: GetPeopleFromListMakerAsync()
Gateway->>TokenCache: GetAccessTokenAsync()

alt ListMaker token is cached and valid
TokenCache-->>Gateway: Cached ListMaker JWT
else ListMaker token missing or near expiration
TokenCache->>AuthClient: LoginAsync(service credentials)
AuthClient->>MakerApi: POST /api/auth/login
MakerApi->>MakerApi: Validate service credentials
MakerApi-->>AuthClient: 200 OK LoginResponse
AuthClient-->>TokenCache: JWT + expiration
TokenCache->>TokenCache: Cache JWT
TokenCache-->>Gateway: New ListMaker JWT
end

Gateway->>ListsClient: GetGeneratedListAsync(Bearer ListMakerJwt)
ListsClient->>MakerApi: GET /api/lists/generated
MakerApi->>MakerApi: Validate ListMaker JWT
MakerApi->>MakerApi: Read stable seeded list
MakerApi-->>ListsClient: 200 OK List<PersonListItemDto>
ListsClient-->>Gateway: List<PersonListItemDto>
Gateway-->>ReaderApi: List<PersonListItemDto>
ReaderApi-->>User: 200 OK List<PersonListItemDto>
end
```

---

## Flow Summary

The successful relay flow is:

```text
User logs in to ListReader.Api
User calls protected ListReader relay endpoint
ListReader validates user JWT
ListReader gets or refreshes ListMaker service JWT
ListReader calls ListMaker.Api through Refit
ListMaker validates service JWT
ListMaker returns generated list
ListReader returns the list to the user
```
---

## Why Token Caching Exists

Without token caching, `ListReader.Api` would call `ListMaker.Api` login endpoint before every relayed list request.

That would cause:

- unnecessary authentication traffic
- higher latency
- worse load-test results
- noisy logs/metrics

The accepted decision is:

```text
Cache the token until near expiration.
```

This is the correct design for this demo.
