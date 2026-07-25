# k6 Load Testing

This folder contains k6 load tests and PowerShell automation scripts.

The tests can run in two modes:

1. Local console/CSV mode
2. Prometheus remote-write mode for Grafana dashboards

---

## Test Scripts

| Test Name | Purpose |
|---|---|
| `listmaker-login-load` | Load test ListMaker login endpoint |
| `listmaker-generated-list-load` | Load test ListMaker generated-list endpoint |
| `listreader-login-load` | Load test ListReader login endpoint |
| `listreader-relay-load` | Load test ListReader relay path to ListMaker |

---

## PowerShell Scripts

| Script | Purpose |
|---|---|
| `run-all-load-tests.ps1` | Runs all k6 load tests |
| `run-listmaker-login-load.ps1` | Runs only ListMaker login test |
| `run-listmaker-generated-list-load.ps1` | Runs only ListMaker generated-list test |
| `run-listreader-login-load.ps1` | Runs only ListReader login test |
| `run-listreader-relay-load.ps1` | Runs only ListReader relay test |
| `append-results.ps1` | Appends summarized k6 results to CSV |

---

## Prerequisites

Install k6:
```powershell
k6 version
```

Start the target APIs before running tests.

Also start the observability stack if Prometheus/Grafana metrics are required:

```powershell
cd ..\
docker compose -f .\docker-compose.grafana.yml up -d
cd .\k6
```

---

## Run All Load Tests

From this folder:

```powershell
.\run-all-load-tests.ps1
```

This runs:

```text
listmaker-login-load
listmaker-generated-list-load
listreader-login-load
listreader-relay-load
```

Each test sends metrics to Prometheus by default.

---

## Run One Test

Example:

```powershell
.\run-listreader-relay-load.ps1
```

Other examples:

```powershell
.\run-listmaker-login-load.ps1
.\run-listmaker-generated-list-load.ps1
.\run-listreader-login-load.ps1
```

---

## Run Without Prometheus

Use `-NoPrometheus` if you only want local k6 output and CSV result appending:

```powershell
.\run-all-load-tests.ps1 -NoPrometheus
```

Single test:

```powershell
.\run-listreader-relay-load.ps1 -NoPrometheus
```

---

## Prometheus Remote Write

The scripts use:

```powershell
-o experimental-prometheus-rw
```

Default remote-write URL:

```text
http://localhost:9090/api/v1/write
```

You can override it:

```powershell
.\run-all-load-tests.ps1 -PrometheusRemoteWriteUrl "http://localhost:9090/api/v1/write"
```

---

## k6 Tags

Each test sends these tags:

| Tag | Example |
|---|---|
| `test_name` | `listreader-relay-load` |
| `run_id` | `20260725-153012` |
| `k6_env` | `local` |

These tags allow filtering in Prometheus and Grafana.

Example PromQL:

```promql
k6_http_req_duration_p95{test_name="listreader-relay-load"}
```

---

## Useful PromQL Queries

Show all k6 metrics:

```promql
{__name__=~"k6_.*"}
```

Request rate by test:

```promql
sum by (test_name) (rate(k6_http_reqs_total[1m]))
```

p95 latency by test:

```promql
max by (test_name) (k6_http_req_duration_p95)
```

p99 latency by test:

```promql
max by (test_name) (k6_http_req_duration_p99)
```

Failures by test:

```promql
sum by (test_name) (rate(k6_http_req_failed_total[1m]))
```

Relay p95:

```promql
k6_http_req_duration_p95{test_name="listreader-relay-load"}
```

Relay p99:

```promql
k6_http_req_duration_p99{test_name="listreader-relay-load"}
```

---

## CSV Results

After every test run, the automation calls:

```powershell
append-results.ps1
```

This appends summarized results into CSV files under the results folder.

The append step runs even when k6 threshold checks fail.

This is intentional because failed tests still produce useful performance data.

---

## Exit Code Behavior

k6 returns a non-zero exit code if:

- thresholds fail
- runtime errors occur
- the test script fails

The run scripts preserve the k6 exit code separately from the CSV append step.

This avoids a previous k6 threshold failure being accidentally treated as an append-script failure.

---

## Current Load Profile

The current load profile uses staged virtual users.

Example range:

```text
100 VUs up to 1000 VUs
```

The purpose is to identify performance degradation under increasing concurrency.

---

## Current Known Bottleneck

The relay endpoint is the main bottleneck:

```text
ListReader.Api -> ListMaker.Api
```

Test:

```text
listreader-relay-load
```

Known issue:

```text
p95 latency exceeds 1000ms under high load
```

Most of the request duration is waiting time.

Possible causes:

- ThreadPool contention
- HttpClient connection pool pressure
- downstream ListMaker saturation
- API-to-API relay overhead
- retry/timeout behavior
- DNS or socket exhaustion under high concurrency

---

## Recommended Next Tests

Run fixed-VU relay benchmarks:

```text
100 VUs
250 VUs
500 VUs
750 VUs
1000 VUs
```

Goal:

```text
Find the degradation knee.
```

The degradation knee is the point where latency starts increasing sharply or failure rate increases.

---

## Recommended Server-Side Profiling

Use `dotnet-counters` during relay load tests.

Find the process ID:

```powershell
dotnet-counters ps
```

Monitor counters:

```powershell
dotnet-counters monitor --process-id <PID> System.Runtime Microsoft.AspNetCore.Hosting System.Net.Http
```

Focus on:

- ThreadPool Queue Length
- ThreadPool Thread Count
- CPU usage
- GC collections
- allocation rate
- active HTTP requests
- outgoing HTTP connections


---

