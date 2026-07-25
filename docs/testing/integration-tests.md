# Integration Tests

## Purpose

Integration tests verify API behavior through the ASP.NET Core HTTP pipeline.

They test realistic API behavior including:

- routing
- model binding
- validation
- authentication middleware
- authorization middleware
- controller behavior
- application service behavior
- API-to-API integration behavior where applicable

---

## Location

Integration test projects are located under:
```text
tests/Integration/
```

Expected projects:

```text
tests/Integration/ListMaker.Api.IntegrationTests/
tests/Integration/ListReader.Api.IntegrationTests/
```

---

## How to Run All Integration Tests

From the solution root:

```powershell
dotnet test .\tests\Integration\
```

---

## How to Run a Specific Integration Test Project

Example:

```powershell
dotnet test .\tests\Integration\ListMaker.Api.IntegrationTests\ListMaker.Api.IntegrationTests.csproj
```

Example:

```powershell
dotnet test .\tests\Integration\ListReader.Api.IntegrationTests\ListReader.Api.IntegrationTests.csproj
```

---

## How to Run All Tests in the Solution

From the solution root:

```powershell
dotnet test .\ApiIntegrationDemo.sln
```

---

## Expected Result

A successful integration test run should show output similar to:

```text
Passed!  - Failed: 0, Passed: X, Skipped: 0
```

The exact number of tests may change over time.

---

## Typical Integration Test Scenarios

### Authentication

Integration tests should verify:

1. valid login returns HTTP 200
2. valid login returns a JWT token
3. invalid login returns HTTP 401
4. missing token on protected endpoint returns HTTP 401
5. valid token on protected endpoint returns HTTP 200

---

### ListMaker API

Recommended scenarios:

| Scenario | Expected Result |
|---|---|
| Login with valid credentials | 200 OK and token |
| Login with invalid credentials | 401 Unauthorized |
| Access generated list without token | 401 Unauthorized |
| Access generated list with token | 200 OK and list data |

---

### ListReader API

Recommended scenarios:

| Scenario | Expected Result |
|---|---|
| Login with valid credentials | 200 OK and token |
| Login with invalid credentials | 401 Unauthorized |
| Access relay endpoint without token | 401 Unauthorized |
| Access relay endpoint with token | 200 OK and relayed ListMaker data |
| Downstream ListMaker unavailable | handled response according to gateway policy |

---

## Integration Test Guidelines

Integration tests should:

1. Use `WebApplicationFactory`.
2. Avoid depending on developer-specific ports.
3. Configure test-specific settings where needed.
4. Use realistic HTTP requests.
5. Verify status codes and response bodies.
6. Keep test data deterministic.
7. Avoid unnecessary sleeps or timing assumptions.

---

## Local API Ports

The actual local development ports are defined in each API project's `launchSettings.json`.

For integration tests, prefer in-memory hosting through `WebApplicationFactory` instead of fixed ports.

---

## Difference from Load Tests

Integration tests verify correctness.

Load tests verify behavior under concurrent usage.

Integration tests answer:

```text
Does it work correctly?
```

Load tests answer:

```text
How does it behave under load?
```