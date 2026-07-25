# Performance and Observability Stack

This folder contains the local performance testing and observability setup for the API lab.

It includes:

- k6 load tests
- Prometheus metrics storage
- Grafana dashboards
- Docker Compose configuration
- Provisioned Grafana data source and dashboards

---

## Folder Structure
```text
perf/
  docker-compose.grafana.yml
  prometheus.yml

  grafana/
dashboards/
grafana-k6-dashboard.json
provisioning/
datasources/
prometheus.yml
dashboards/
dashboards.yml
```

  k6/
README.md
run-all-load-tests.ps1
run-listmaker-login-load.ps1
run-listmaker-generated-list-load.ps1
run-listreader-login-load.ps1
run-listreader-relay-load.ps1
append-results.ps1
*.js
results/

---

## Services

| Service | URL | Notes |
|---|---|---|
| Prometheus | http://localhost:9090 | Receives k6 remote-write metrics |
| Grafana | http://localhost:33000 | Dashboard UI |

Grafana is mapped to host port `33000` because Windows may reserve or block ports around `3000`.

---

## Start Observability Stack

From the `perf` folder:

powershell
docker compose -f .\docker-compose.grafana.yml up -d

Check containers:

powershell
docker ps

Open:

```text
Prometheus: http://localhost:9090
Grafana:    http://localhost:33000
```

Default Grafana login:

```text
Username: admin
Password: admin
```

---

## Stop Stack

```powershell
docker compose -f .\docker-compose.grafana.yml down
```

Do not use this unless you want to delete saved Grafana dashboards and data:

```powershell
docker compose -f .\docker-compose.grafana.yml down -v
```

The `-v` option removes Docker volumes, including Grafana's internal database.

---

## Prometheus Remote Write

k6 sends live metrics to Prometheus using:

```powershell
-o experimental-prometheus-rw
```

Prometheus must be started with:

```yaml
--web.enable-remote-write-receiver
```

The remote-write endpoint is:

```text
http://localhost:9090/api/v1/write
```

---

## Grafana Dashboards

Provisioned dashboards are stored here:

```text
perf/grafana/dashboards/
```

Example:

```text
perf/grafana/dashboards/grafana-k6-dashboard.json
```

Grafana loads dashboards using:

```text
perf/grafana/provisioning/dashboards/dashboards.yml
```

---

## Where Grafana UI Dashboards Are Saved

Dashboards created or edited in the Grafana UI are stored in Grafana's internal database:

```text
/var/lib/grafana/grafana.db
```

In Docker, this database is stored inside the `grafana-data` Docker volume.

These dashboards are persistent as long as the Docker volume is not deleted.

To save UI-created dashboards into source control:

1. Open the dashboard in Grafana.
2. Go to dashboard settings or share/export.
3. Export the JSON model.
4. Save it into:

```text
perf/grafana/dashboards/
```

Then commit the JSON file.

---

## Useful Prometheus Queries

Show all k6 metrics:

```promql
{__name__=~"k6_.*"}
```

Total HTTP requests:

```promql
k6_http_reqs_total
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

Filter relay test:

```promql
k6_http_req_duration_p95{test_name="listreader-relay-load"}
```

Error rate by test:

```promql
sum by (test_name) (rate(k6_http_req_failed_total[1m]))
```

---

## Current Known Performance Finding

Direct endpoints are very fast and stable.

Examples:

- ListMaker login
- ListReader login

The relay path is the current bottleneck:

```text
ListReader.Api -> ListMaker.Api
```

The relay load test is:

```text
listreader-relay-load
```

This endpoint currently fails the `p95 < 1000ms` threshold under higher load.

Observed behavior suggests most latency is spent in HTTP waiting time, possibly caused by:

- ThreadPool contention
- HttpClient connection-pool limits
- downstream service saturation
- relay/API-to-API communication overhead

---

## Next Recommended Diagnostics

Run fixed-VU tests for the relay endpoint:

```text
100 VUs
250 VUs
500 VUs
750 VUs
1000 VUs
```

Use `dotnet-counters` against `ListReader.Api` during the test:

```powershell
dotnet-counters monitor --process-id <PID> System.Runtime Microsoft.AspNetCore.Hosting System.Net.Http
```

Important counters:

- ThreadPool Queue Length
- ThreadPool Completed Work Item Count
- GC Heap Size
- Gen 0/1/2 GC Count
- HTTP current requests
- outgoing HttpClient connections
