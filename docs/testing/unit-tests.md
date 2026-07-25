# Unit Tests

## Purpose

Unit tests verify isolated application behavior without starting the full API host.

They are used to test classes such as:

- JWT token services
- list providers
- Refit client configuration
- token cache services
- gateway behavior where mocking is appropriate

---

## Location

Unit test projects are located under:
```text
tests/Unit/
```

Expected projects:

```text
tests/Unit/ListMaker.Api.UnitTests/
tests/Unit/ListReader.Api.UnitTests/
tests/Unit/ListMaker.Client.UnitTests/
```
---

## How to Run All Unit Tests

From the solution root:

```powershell
dotnet test .\tests\Unit\
```

Alternatively, run all tests in the solution:

```powershell
dotnet test .\ApiIntegrationDemo.sln
```

---

## How to Run a Specific Unit Test Project

Example:

```powershell
dotnet test .\tests\Unit\ListMaker.Api.UnitTests\ListMaker.Api.UnitTests.csproj
```

Example:

```powershell
dotnet test .\tests\Unit\ListReader.Api.UnitTests\ListReader.Api.UnitTests.csproj
```

Example:

```powershell
dotnet test .\tests\Unit\ListMaker.Client.UnitTests\ListMaker.Client.UnitTests.csproj
```

---

## Expected Result

A successful unit test run should show output similar to:

```text
Passed!  - Failed: 0, Passed: X, Skipped: 0
```

The exact number of tests may change as the project evolves.

---

## Unit Test Guidelines

Unit tests should:

1. Be fast.
2. Avoid real network calls.
3. Avoid Docker dependencies.
4. Avoid real API hosting unless required.
5. Use mocks/fakes where appropriate.
6. Test one behavior per test.
7. Use readable test names.

---

## Recommended Naming Convention

Use descriptive test names:

```csharp
MethodName_WhenCondition_ShouldExpectedResult()
```

Example:

```csharp
GenerateToken_WhenUserIsValid_ShouldReturnJwtToken()
```

Example:

```csharp
GetAccessTokenAsync_WhenTokenIsCached_ShouldReturnCachedToken()
```

---

## What Should Be Unit Tested

Recommended unit test targets:

| Area | Example |
|---|---|
| JWT token generation | token contains expected claims |
| seeded list provider | returns stable deterministic data |
| token cache | reuses token before expiration |
| options validation | detects invalid configuration |
| Refit registration | required clients can be resolved |
| gateway logic | handles successful and failed downstream calls |

---

## What Should Not Be Unit Tested

Do not unit test framework internals such as:

- ASP.NET Core routing
- built-in JWT middleware internals
- `HttpClientFactory` internals
- Refit implementation internals

Those behaviors are better covered indirectly through integration tests.
