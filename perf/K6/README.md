# k6 Load Tests

This folder contains k6 load tests for `ApiIntegrationDemo`.

The tests cover:

- `ListMaker.Api` login
- `ListMaker.Api` generated list endpoint
- `ListReader.Api` login
- `ListReader.Api` relay endpoint

---

## Folder Structure
```text
perf/k6/
├── README.md
├── run-all-load-tests.ps1
├── run-listmaker-login-load.ps1
├── run-listmaker-generated-list-load.ps1
├── run-listreader-login-load.ps1
├── run-listreader-relay-load.ps1
├── append-results.ps1
│
├── config/
│   └── environments.js
│
├── helpers/
│   ├── auth.js
│   ├── checks.js
│   ├── csv-summary.js
│   └── headers.js
│
├── results/
│   └── .gitkeep
│
├── listmaker-login-load.js
├── listmaker-generated-list-load.js
├── listreader-login-load.js
└── listreader-relay-load.js
```

---

## Prerequisites

Install k6:

```powershell
k6 version
```

Ensure both APIs are running before executing load tests.

---

## API Prerequisites

Start `ListMaker.Api`:

```powershell
dotnet run --project .\src\Services\ListMaker\ListMaker.Api\ListMaker.Api.csproj
```

Start `ListReader.Api` in another terminal:

```powershell
dotnet run --project .\src\Services\ListReader\ListReader.Api\ListReader.Api.csproj
```

Run these commands from the solution root.

---

## Environment Configuration

Base URLs and environment-specific values are configured in:

```text
perf/k6/config/environments.js
```

Before running tests, verify that the configured ports match the running APIs.

---

## Run All Load Tests

From the solution root:

```powershell
cd .\perf\k6
.\run-all-load-tests.ps1
```

---

## Run Individual Tests

### ListMaker Login

```powershell
.\run-listmaker-login-load.ps1
```

### ListMaker Generated List

```powershell
.\run-listmaker-generated-list-load.ps1
```

### ListReader Login

```powershell
.\run-listreader-login-load.ps1
```

### ListReader Relay

```powershell
.\run-listreader-relay-load.ps1
```

---

## Prometheus Remote Write

The PowerShell runners send k6 metrics to Prometheus using:

```text
-o experimental-prometheus-rw
```

The runner scripts configure Prometheus remote write settings through environment variables.

Example:

```powershell
$env:K6_PROMETHEUS_RW_PUSH_INTERVAL = "5s"
```

---

## Tags

The runners use tags for better metric filtering.

Important tags:

| Tag | Description |
|---|---|
| `test_name` | Identifies the test scenario |
| `run_id` | Identifies the specific execution |
| `k6_env` | Identifies the target environment |

Example use cases:

- compare one run with another
- isolate `listreader-relay-load`
- filter local test data
- review one execution in Grafana

---

## CSV Results

The load test workflow appends summary results to CSV files under:

```text
perf/k6/results/
```

The append process is handled by:

```text
append-results.ps1
```

The runner scripts preserve compatibility with this workflow by resetting:

```powershell
$global:LASTEXITCODE
```

before invoking the CSV append step.

---

## Grafana

Start the observability stack first:

```powershell
cd ..\
docker compose -f .\docker-compose.grafana.yml up -d
```

Then open:

```text
http://127.0.0.1:33000
```

Use dashboard filters such as:

- `test_name`
- `run_id`
- `k6_env`

---

## Expected Results

A successful run should produce:

1. k6 console output
2. CSV result updates under `perf/k6/results/`
3. Prometheus metrics
4. Grafana dashboard data

---

## Known Notes

The `ListReader.Api` relay endpoint is expected to be slower than direct `ListMaker.Api` endpoints because it performs downstream communication:

```text
Client -> ListReader.Api -> ListMaker.Api -> ListReader.Api -> Client
```

Higher p95 latency for the relay endpoint should be reviewed as a performance finding.

It is not automatically a functional defect unless the error rate increases or unexpected HTTP 500 responses occur.

---

## Troubleshooting

### k6 command not found

Verify k6 installation:

```powershell
k6 version
```

---

### API connection fails

Check that both APIs are running and that `environments.js` contains the correct URLs.

---

### Grafana has no data

Check that:

1. observability stack is running
2. k6 was executed with Prometheus remote write output
3. Prometheus remote write endpoint is configured
4. Grafana datasource points to:

```text
http://prometheus:9090
```

---

### CSV file not updated

Check:

1. `append-results.ps1` exists
2. `results/` folder exists
3. PowerShell execution policy allows script execution
4. runner scripts reset `$global:LASTEXITCODE` before appending
