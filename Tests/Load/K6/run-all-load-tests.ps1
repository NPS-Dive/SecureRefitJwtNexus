$ErrorActionPreference = "Continue"

$tests = @(
    "listmaker-login-load",
    "listmaker-generated-list-load",
    "listreader-login-load",
    "listreader-relay-load"
)

$failedTests = @()
$appendFailedTests = @()

Write-Host "Starting all k6 load tests..."
Write-Host ""

foreach ($scriptName in $tests) {
    $k6Script = ".\$scriptName.js"

    Write-Host "============================================================"
    Write-Host "Running k6 load test: $scriptName"
    Write-Host "============================================================"
    Write-Host ""

    k6 run -e K6_ENV=local --insecure-skip-tls-verify $k6Script
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
        Write-Host "k6 test passed: $scriptName"
    }

    Write-Host ""
}

Write-Host "============================================================"
Write-Host "All load tests finished."
Write-Host "============================================================"
Write-Host ""

if ($failedTests.Count -gt 0) {
    Write-Host "k6 tests with threshold/runtime failures:"
    foreach ($test in $failedTests) {
        Write-Host "- $test"
    }
}
else {
    Write-Host "All k6 tests passed."
}

Write-Host ""

if ($appendFailedTests.Count -gt 0) {
    Write-Host "Tests with CSV append failures:"
    foreach ($test in $appendFailedTests) {
        Write-Host "- $test"
    }
}
else {
    Write-Host "All CSV append steps passed."
}

Write-Host ""

if (($failedTests.Count -gt 0) -or ($appendFailedTests.Count -gt 0)) {
    exit 1
}

exit 0
