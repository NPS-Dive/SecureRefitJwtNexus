# Performance and Observability

This folder contains the local performance testing and observability stack for `ApiIntegrationDemo`.

It includes:

- k6 load tests
- Prometheus metrics storage
- Grafana dashboards
- Docker Compose configuration

---

## Folder Structure
```text
perf/
├── README.md
├── docker-compose.grafana.yml
│
├── prometheus/
│   └── prometheus.yml
│
├── grafana/
│   ├── dashboards/
│   │   └── grafana-k6-dashboard.json
│   │
│   └── provisioning/
│       ├── dashboards/
│       │   └── dashboards.yml
│       └── datasources/
│           └── prometheus.yml
│
└── k6/
├── README.md
├── run-all-load-tests.ps1
├── run-listmaker-login-load.ps1
├── run-listmaker-generated-list-load.ps1
├── run-listreader-login-load.ps1
├── run-listreader-relay-load.ps1
├── append-results.ps1
├── config/
├── helpers/
├── results/
└── *.js

```
---

## Services

The observability stack contains:

| Service | Purpose |
|---|---|
| Prometheus | Stores k6 metrics |
| Grafana | Visualizes k6 metrics |

---

## Start Observability Stack

From the solution root:

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml up -d
```

---

## Access Grafana

Grafana is available at:

```text
http://127.0.0.1:33000
```

Grafana uses host port `33000` because ports around `3000` were unavailable on the Windows host.

Container mapping:

```text
127.0.0.1:33000 -> grafana:3000
```

---

## Prometheus Datasource

Grafana should connect to Prometheus by using the Docker Compose service name:

```text
http://prometheus:9090
```

This is correct because Grafana and Prometheus run in the same Docker Compose network.

Do not use this inside Grafana datasource configuration:

```text
http://localhost:9090
```

because `localhost` would refer to the Grafana container itself.

---

## Stop Observability Stack

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml down
```

---

## Stop and Remove Volumes

```powershell
cd .\perf
docker compose -f .\docker-compose.grafana.yml down -v
```

Warning: this removes the Grafana data volume.

---

## Grafana Dashboard Persistence

Grafana UI-created dashboards are stored in the Docker volume:

```text
grafana-data
```

To version dashboards in Git:

1. Open Grafana.
2. Export dashboard JSON.
3. Save the JSON file under:

```text
perf/grafana/dashboards/
```

4. Commit the file.

---

## Running Load Tests

See:

```text
perf/k6/README.md
```

Main command:

```powershell
cd .\perf\k6
.\run-all-load-tests.ps1
```

---

## Related Documentation

See also:

```text
docs/testing/test-strategy.md
docs/testing/load-tests.md
docs/testing/test-execution.md
```
