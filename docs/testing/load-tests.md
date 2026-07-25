# Load Tests

## Purpose

Load tests verify the behavior of the APIs under simulated concurrent usage.

The project uses k6 to test:

- login endpoint performance
- protected list endpoint performance
- `ListReader.Api` relay endpoint performance
- API-to-API communication behavior under load
- basic latency and failure-rate thresholds

---

## Location

Load testing assets are located under:
```text
perf/k6/
```

Expected files:

```text
perf/k6/listmaker-login-load.js
perf/k6/listmaker-generated-list-load.js
perf/k6/listreader-login-load.js
perf/k6/listreader-relay-load.js
```

PowerShell runners:

```text
perf/k6/run-all-load-tests.ps1
perf/k6/run-listmaker-login-load.ps1
perf/k6/run-listmaker-generated-list-load.ps1
perf/k6/run-listreader-login-load.ps1
perf/k6/run-listreader-relay-load.ps1
```

---

## Prerequisites

Before running load tests, ensure that:

1. .NET SDK is installed.
2. k6 is installed.
3. Docker Desktop is running if Prometheus/Grafana output is required.
4. `ListMaker.Api` is running.
5. `ListReader.Api` is running.
6. The correct ports are configured in the k6 environment file.

---

## Start APIs

Start both APIs from Visual Studio or through command line.

Example command line approach:

```powershell
dotnet run --project .\src\Services\ListMaker\ListMaker.Api\ListMaker.Api.csproj
```

In another terminal:

```powershell
dotnet run --project .\src\Services\ListReader\ListReader.Api\ListReader.Api.csproj
```

Use the actual project paths and ports configured in your solution.

---

## Start Observability Stack

From the solution root:

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml up -d
```

Grafana is available at:

```text
http://127.0.0.1:33000
```

Prometheus is available internally to Grafana at:

```text
http://prometheus:9090
```

Depending on Docker Compose port mapping, Prometheus may also be available from the host.

---

## Why Grafana Uses Port 33000

Grafana normally uses port `3000`.

On this Windows environment, ports around `3000` and `3001` were unavailable because of Windows port exclusions.

The reserved range included:

```text
2996–3095
```

Therefore, Grafana is mapped as:

```text
127.0.0.1:33000:3000
```

This means:

- host browser uses `http://127.0.0.1:33000`
- container still uses port `3000`

---

## Run All Load Tests

From the solution root:

```powershell
cd .\perf\k6
.\run-all-load-tests.ps1
```

---

## Run Individual Load Tests

### ListMaker Login

```powershell
cd .\perf\k6
.\run-listmaker-login-load.ps1
```

### ListMaker Generated List

```powershell
cd .\perf\k6
.\run-listmaker-generated-list-load.ps1
```

### ListReader Login

```powershell
cd .\perf\k6
.\run-listreader-login-load.ps1
```

### ListReader Relay

```powershell
cd .\perf\k6
.\run-listreader-relay-load.ps1
```

---

## Prometheus Remote Write Output

The k6 runners use:

```text
-o experimental-prometheus-rw
```

This sends k6 metrics to Prometheus through the remote-write endpoint.

The runners also standardize environment variables such as:

```text
K6_PROMETHEUS_RW_PUSH_INTERVAL=5s
```

---

## k6 Tags

The load test runners use tags to improve filtering in Grafana.

Important tags:

| Tag | Purpose |
|---|---|
| `test_name` | identifies the load test script/scenario |
| `run_id` | identifies one execution run |
| `k6_env` | identifies the target environment |

These tags make it easier to filter dashboards by test run.

---

## CSV Results

The load test workflow also appends summarized results to CSV files.

Expected location:

```text
perf/k6/results/
```

The CSV append workflow is preserved by calling:

```text
append-results.ps1
```

The runner scripts reset:

```powershell
$global:LASTEXITCODE
```

before invoking the append step to avoid false failure propagation.

---

## Grafana Dashboard Persistence

Grafana UI-created dashboards are stored in the Docker volume:

```text
grafana-data
```

This means dashboard changes made in the Grafana UI are not automatically committed to Git.

To preserve dashboard changes:

1. Open Grafana.
2. Open the dashboard.
3. Export dashboard JSON.
4. Save the JSON file under:

```text
perf/grafana/dashboards/
```

5. Commit the exported JSON file to source control.

---

## Known Performance Findings

The relay endpoint may show higher latency than direct ListMaker endpoints because it performs API-to-API communication:

```text
ListReader.Api -> ListMaker.Client -> ListMaker.Api
```

This is expected because the relay flow includes:

1. authentication against `ListReader.Api`
2. authorization inside `ListReader.Api`
3. cached or refreshed service token for `ListMaker.Api`
4. HTTP call to `ListMaker.Api`
5. response mapping and return to caller

If the relay endpoint fails a p95 latency threshold, this should be recorded as a performance finding, not necessarily as a functional failure.

---

## Load Test Success Criteria

A load test run is considered successful when:

1. the API remains available
2. HTTP failure rate remains within threshold
3. no unexpected HTTP 500 errors occur
4. response times remain within acceptable demo limits
5. k6 results are produced
6. Prometheus receives metrics
7. Grafana dashboard can visualize the run

---

## Troubleshooting

### Grafana is not reachable

Check:

```powershell
docker ps
```

Then browse to:

```text
http://127.0.0.1:33000
```

Do not use:

```text
http://127.0.0.1:3000
```

because port `3000` is reserved/unavailable on this machine.

---

### Prometheus datasource fails in Grafana

Inside Docker Compose, Grafana should use:

``text
http://prometheus:9090
```

Do not use `localhost:9090` inside Grafana container configuration because `localhost` would refer to the Grafana container itself.

---

### k6 cannot send metrics

Verify the Prometheus remote-write endpoint is enabled and reachable according to the Docker Compose configuration.

Also verify that the runner includes:

```text
-o experimental-prometheus-rw
```

---

### API requests fail

Verify that both APIs are running and that the base URLs in:

```text
perf/k6/config/environments.js
```

match the actual API ports.

