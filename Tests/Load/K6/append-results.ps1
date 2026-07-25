param(
    [Parameter(Mandatory = $true)]
    [string]$ScriptName
)

$ErrorActionPreference = "Stop"

$resultsDir = Join-Path $PSScriptRoot "results"
$currentFile = Join-Path $resultsDir "$ScriptName.current.csv"
$historyFile = Join-Path $resultsDir "$ScriptName.csv"

if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
}

if (-not (Test-Path $currentFile)) {
    Write-Error "Current CSV file not found: $currentFile"
    exit 1
}

$currentLines = Get-Content $currentFile

if ($currentLines.Count -lt 2) {
    Write-Error "Current CSV file does not contain header and data row: $currentFile"
    exit 1
}

$header = $currentLines[0]
$dataRow = $currentLines[1]

if (-not (Test-Path $historyFile)) {
    Set-Content -Path $historyFile -Value $header -Encoding UTF8
    Add-Content -Path $historyFile -Value $dataRow -Encoding UTF8

    Write-Host "Created history CSV:"
    Write-Host $historyFile
}
else {
    Add-Content -Path $historyFile -Value $dataRow -Encoding UTF8

    Write-Host "Appended row to history CSV:"
    Write-Host $historyFile
}

Remove-Item $currentFile -Force

Write-Host "Removed temporary file:"
Write-Host $currentFile

Write-Host ""
Write-Host "Last appended row:"
Write-Host $dataRow

# Explicitly report successful completion to the caller.
exit 0
