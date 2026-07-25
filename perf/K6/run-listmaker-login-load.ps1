param(
    [string]$PrometheusRemoteWriteUrl = "http://localhost:9090/api/v1/write",
    [switch]$NoPrometheus
)

$ErrorActionPreference = "Continue"

$scriptName = "listmaker-login-load"
$k6Script = ".\$scriptName.js"
$runId = Get-Date -Format "yyyyMMdd-HHmmss"

# ---------------------------------------------------------------------
# k6 -> Prometheus Remote Write configuration
# ---------------------------------------------------------------------
if (-not $NoPrometheus) {
    $env:K6_PROMETHEUS_RW_SERVER_URL = $PrometheusRemoteWriteUrl
    $env:K6_PROMETHEUS_RW_TREND_STATS = "p(90),p(95),p(99),max,avg,min"
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
Write-Host "Running k6 load test: $scriptName"
Write-Host "Run ID: $runId"
Write-Host ""

if (-not (Test-Path $k6Script)) {
    Write-Error "k6 script not found: $k6Script"
    exit 1
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

# Reset native process exit code before append-results.ps1.
$global:LASTEXITCODE = 0

& (Join-Path $PSScriptRoot "append-results.ps1") -ScriptName $scriptName

$appendSucceeded = $?
$appendExitCode = $LASTEXITCODE

if (-not $appendSucceeded -and $appendExitCode -eq 0) {
    $appendExitCode = 1
}

if ($appendExitCode -ne 0) {
    Write-Error "Append step failed for $scriptName with exit code $appendExitCode."
    exit $appendExitCode
}

if ($k6ExitCode -ne 0) {
    Write-Warning "k6 finished with non-zero exit code $k6ExitCode for $scriptName. This may mean thresholds failed."
    exit $k6ExitCode
}

Write-Host ""
Write-Host "Completed successfully: $scriptName" -ForegroundColor Green

if (-not $NoPrometheus) {
    Write-Host ""
    Write-Host "Prometheus: http://localhost:9090" -ForegroundColor Cyan
    Write-Host "Grafana:    http://localhost:33000" -ForegroundColor Cyan
}

exit 0
