$ErrorActionPreference = "Continue"

$scriptName = "listmaker-generated-list-load"
$k6Script = ".\$scriptName.js"

Write-Host "Running k6 load test: $scriptName"
Write-Host ""

k6 run -e K6_ENV=local --insecure-skip-tls-verify $k6Script
$k6ExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "Appending CSV result for: $scriptName"
Write-Host ""

.\append-results.ps1 -ScriptName $scriptName
$appendExitCode = $LASTEXITCODE

if ($appendExitCode -ne 0) {
    Write-Error "Append step failed for $scriptName with exit code $appendExitCode."
    exit $appendExitCode
}

if ($k6ExitCode -ne 0) {
    Write-Warning "k6 finished with non-zero exit code $k6ExitCode for $scriptName. This may mean thresholds failed."
    exit $k6ExitCode
}

Write-Host ""
Write-Host "Completed successfully: $scriptName"
exit 0
