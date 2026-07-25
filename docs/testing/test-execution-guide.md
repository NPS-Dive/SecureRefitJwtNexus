# Test Execution Guide

## Purpose

This document explains the recommended end-to-end test execution flow for `ApiIntegrationDemo`.

It covers:

1. restoring and building the solution
2. running unit tests
3. running integration tests
4. starting local APIs
5. starting observability stack
6. running k6 load tests
7. reviewing CSV/Grafana results

---

## 1. Restore Dependencies

From the solution root:
```powershell
dotnet restore .\ApiIntegrationDemo.sln
```

---

## 2. Build the Solution

```powershell
dotnet build .\ApiIntegrationDemo.sln
```

Recommended stricter build:

```powershell
dotnet build .\ApiIntegrationDemo.sln --no-restore
```

---

## 3. Run All Automated .NET Tests

```powershell
dotnet test .\ApiIntegrationDemo.sln --no-build
```

If the solution was not built first, use:

```powershell
dotnet test .\ApiIntegrationDemo.sln
```

---

## 4. Run Unit Tests Only

```powershell
dotnet test .\tests\Unit\
```

---

## 5. Run Integration Tests Only

```powershell
dotnet test .\tests\Integration\
```

---

## 6. Start APIs for Manual and Load Testing

Start `ListMaker.Api`:

```powershell
dotnet run --project .\src\Services\ListMaker\ListMaker.Api\ListMaker.Api.csproj
```

Start `ListReader.Api` in another terminal:

```powershell
dotnet run --project .\src\Services\ListReader\ListReader.Api\ListReader.Api.csproj
```

Alternatively, start both APIs from Visual Studio.

---

## 7. Validate APIs Through Swagger

Open the Swagger UI URLs configured in each API's launch profile.

Typical manual verification flow:

1. Open Swagger for `ListMaker.Api`.
2. Call login endpoint.
3. Copy returned JWT token.
4. Authorize Swagger with Bearer token.
5. Call protected list endpoint.
6. Repeat equivalent login/authorization flow for `ListReader.Api`.
7. Call `ListReader.Api` relay endpoint.

---

## 8. Start Prometheus and Grafana

From the solution root:

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml up -d
```

Open Grafana:

```text
http://127.0.0.1:33000
```

Grafana connects to Prometheus internally by using:

```text
http://prometheus:9090
```

---

## 9. Run All k6 Load Tests

From the solution root:

```powershell
cd .\perf\k6
.\run-all-load-tests.ps1
```

---

## 10. Run One k6 Test

Example:

```powershell
cd .\perf\k6
.\run-listmaker-login-load.ps1
```

Example:

```powershell
cd .\perf\k6
.\run-listreader-relay-load.ps1
```

---

## 11. Review CSV Results

CSV results are stored under:

```text
perf/k6/results/
```

These files can be used for:

- historical comparison
- test reporting
- QA/QC evidence
- performance trend tracking

---

## 12. Review Grafana Dashboard

Open:

```text
http://127.0.0.1:33000
```

Use dashboard filters such as:

- `test_name`
- `run_id`
- `k6_env`

These tags help isolate individual test executions.

---

## 13. Stop Observability Stack

From the solution root:

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml down
```

To remove volumes as well:

```powershell
docker compose -f .\docker-compose.grafana.yml down -v
```

Be careful: removing volumes deletes Grafana UI-created dashboards unless they were exported first.

---

## 14. Recommended Full Local Verification Flow

Use this full sequence before final project delivery:

```powershell
dotnet restore .\ApiIntegrationDemo.sln
dotnet build .\ApiIntegrationDemo.sln --no-restore
dotnet test .\ApiIntegrationDemo.sln --no-build
```

Then start both APIs.

Then start observability:

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml up -d
```

Then run load tests:

```powershell
cd .\k6
.\run-all-load-tests.ps1
```

Finally, check:

```text
perf/k6/results/
```

and:

```
text
http://127.0.0.1:33000
```

---

## 15. Test Execution Checklist

Before marking testing complete, verify:

- [ ] solution restores successfully
- [ ] solution builds successfully
- [ ] unit tests pass
- [ ] integration tests pass
- [ ] both APIs start successfully
- [ ] Swagger works for both APIs
- [ ] valid login returns JWT token
- [ ] unauthorized requests are rejected
- [ ] authorized protected endpoints work
- [ ] ListReader relay endpoint works
- [ ] Prometheus container starts
- [ ] Grafana container starts
- [ ] Grafana is reachable on `127.0.0.1:33000`
- [ ] Grafana datasource connects to Prometheus
- [ ] k6 tests run successfully
- [ ] CSV results are generated/appended
- [ ] Grafana receives k6 metrics
- [ ] dashboards are exported and committed if modified
