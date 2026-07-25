param(
    [string]$PrometheusRemoteWriteUrl = "http://localhost:9090/api/v1/write",
    [switch]$NoPrometheus
)

$ErrorActionPreference = "Continue"

# ---------------------------------------------------------------------
# k6 -> Prometheus Remote Write configuration
# ---------------------------------------------------------------------
if (-not $NoPrometheus) {
    $env:K6_PROMETHEUS_RW_SERVER_URL = $PrometheusRemoteWriteUrl

    # These trend stats will create useful Prometheus time series such as:
    # k6_http_req_duration_p95
    # k6_http_req_duration_p99
    $env:K6_PROMETHEUS_RW_TREND_STATS = "p(90),p(95),p(99),max,avg,min"

    # Push metrics to Prometheus every 5 seconds.
    $env:K6_PROMETHEUS_RW_PUSH_INTERVAL = "5s"

    Write-Host "Prometheus remote-write is ENABLED." -ForegroundColor Cyan
    Write-Host "Remote-write URL: $env:K6_PROMETHEUS_RW_SERVER_URL" -ForegroundColor Cyan
}
else {
    Remove-Item Env:\K6_PROMETHEUS_RW_SERVER_URL -ErrorAction SilentlyContinue
    Remove-Item Env:\K6_PROMETHEUS_RW_TREND_STATS -ErrorAction SilentlyContinue
    Remove-Item Env:\K6_PROMETHEUS_RW_PUSH_INTERVAL -ErrorAction SilentlyContinue

    Write-Host "Prometheus remote-write is DISABLED." -ForegroundColor Yellow
}

Write-Host ""

# ---------------------------------------------------------------------
# Test list
# ---------------------------------------------------------------------
$tests = @(
    "listmaker-login-load",
    "listmaker-generated-list-load",
    "listreader-login-load",
    "listreader-relay-load"
)

$failedTests = @()
$appendFailedTests = @()

# A single run id for the whole batch.
# This lets you filter one full batch execution in Grafana.
$runId = Get-Date -Format "yyyyMMdd-HHmmss"

Write-Host "Starting all k6 load tests..."
Write-Host "Batch Run ID: $runId"
Write-Host ""

foreach ($scriptName in $tests) {
    $k6Script = ".\$scriptName.js"

    Write-Host "============================================================"
    Write-Host "Running k6 load test: $scriptName"
    Write-Host "Run ID: $runId"
    Write-Host "============================================================"
    Write-Host ""

    if (-not (Test-Path $k6Script)) {
        Write-Warning "k6 script not found: $k6Script"
        $failedTests += $scriptName
        continue
    }

    # Reset native process exit code before k6.
    $global:LASTEXITCODE = 0

    if (-not $NoPrometheus) {
        k6 run `
            -o experimental-prometheus-rw `
            -e K6_ENV=local `
            -e TEST_NAME=$scriptName `
            -e RUN_ID=$runId `
            --tag test_name=$scriptName `
            --tag run_id=$runId `
            --tag k6_env=local `
            --insecure-skip-tls-verify `
            $k6Script
    }
    else {
        k6 run `
            -e K6_ENV=local `
            -e TEST_NAME=$scriptName `
            -e RUN_ID=$runId `
            --tag test_name=$scriptName `
            --tag run_id=$runId `
            --tag k6_env=local `
            --insecure-skip-tls-verify `
            $k6Script
    }

    $k6ExitCode = $LASTEXITCODE

    Write-Host ""
    Write-Host "Appending CSV result for: $scriptName"
    Write-Host ""

    # Reset the native exit code so a previous k6 threshold failure is not
    # incorrectly interpreted as an append-script failure.
    $global:LASTEXITCODE = 0

    & (Join-Path $PSScriptRoot "append-results.ps1") -ScriptName $scriptName

    $appendSucceeded = $?
    $appendExitCode = $LASTEXITCODE

    if (-not $appendSucceeded -and $appendExitCode -eq 0) {
        $appendExitCode = 1
    }

    if ($appendExitCode -ne 0) {
        Write-Warning "Append step failed for $scriptName with exit code $appendExitCode."
        $appendFailedTests += $scriptName
    }

    if ($k6ExitCode -ne 0) {
        Write-Warning "k6 finished with non-zero exit code $k6ExitCode for $scriptName. This may mean thresholds failed."
        $failedTests += $scriptName
    }
    else {
        Write-Host "k6 test passed: $scriptName" -ForegroundColor Green
    }

    Write-Host ""
}

Write-Host "============================================================"
Write-Host "All load tests finished."
Write-Host "============================================================"
Write-Host ""

if ($failedTests.Count -gt 0) {
    Write-Host "k6 tests with threshold/runtime failures:" -ForegroundColor Yellow
    foreach ($test in $failedTests) {
        Write-Host "- $test"
    }
}
else {
    Write-Host "All k6 tests passed." -ForegroundColor Green
}

Write-Host ""

if ($appendFailedTests.Count -gt 0) {
    Write-Host "Tests with CSV append failures:" -ForegroundColor Yellow
    foreach ($test in $appendFailedTests) {
        Write-Host "- $test"
    }
}
else {
    Write-Host "All CSV append steps passed." -ForegroundColor Green
}

Write-Host ""

if (-not $NoPrometheus) {
    Write-Host "Prometheus metrics URL:" -ForegroundColor Cyan
    Write-Host "http://localhost:9090"
    Write-Host ""
    Write-Host "Grafana URL:" -ForegroundColor Cyan
    Write-Host "http://localhost:33000"
    Write-Host ""
    Write-Host "Useful Prometheus query:" -ForegroundColor Cyan
    Write-Host "{__name__=~`"k6_.*`"}"
    Write-Host ""
}

if (($failedTests.Count -gt 0) -or ($appendFailedTests.Count -gt 0)) {
    exit 1
}

exit 0
