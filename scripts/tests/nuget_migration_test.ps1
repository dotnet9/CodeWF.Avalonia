param([string]$OutputDirectory = (Join-Path ([IO.Path]::GetTempPath()) 'CodeWFNugetMigrationTest'))
$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content (Join-Path $scriptRoot 'nuget-version-migration.json') -Raw | ConvertFrom-Json
$state = @{ attempts = @{}; deleted = [Collections.Generic.List[string]]::new(); sleeps = 0 }

# 所有网络与等待均在测试作用域内替换，真实发布凭据不会用于测试。
function Invoke-WebRequest {
    param([string]$Uri, [string]$Method = 'Get', $Headers, [string]$OutFile, [switch]$PassThru, [int]$TimeoutSec)
    $id = [Uri]::UnescapeDataString(([Uri]$Uri).Segments[-2].Trim('/'))
    if ($id -notin $manifest.packageIds) { throw "Invalid package ID entered the polling queue: $id" }
    if ($Method -eq 'Delete') {
        if ($state.denied) { throw 'Fixture: API key does not grant Unlist permission.' }
        $state.deleted.Add($Uri)
        return [pscustomobject]@{ StatusCode = 200 }
    }
    if (-not $state.attempts.ContainsKey($id)) {
        $state.attempts[$id] = 1
        throw 'Fixture: indexing is pending.'
    }
    $state.attempts[$id]++
    [IO.File]::WriteAllBytes($OutFile, [byte[]]@(0x50, 0x4b, 3, 4, 0))
    return [pscustomobject]@{ StatusCode = 200 }
}
function Start-Sleep {
    param([int]$Seconds)
    $state.sleeps++
    if ($state.sleeps -gt 2) { throw 'Polling did not recover after packages became available.' }
}
$previousKey = $env:NUGET_API_KEY
try {
    $env:NUGET_API_KEY = 'offline-test-placeholder'
    & (Join-Path $scriptRoot 'migrate_nuget_versions.ps1') -Execute -OutputDirectory $OutputDirectory
    $results = @(Get-Content (Join-Path $OutputDirectory 'migration-result.json') -Raw | ConvertFrom-Json)
    if ($state.sleeps -ne 1 -or $state.attempts.Count -ne 11 -or $results.Count -ne $manifest.packages.Count -or
        $state.deleted.Count -ne $manifest.packages.Count) { throw 'Migration fixture did not verify the exact package set.' }
    Write-Output "PASS: delayed public packages recovered; $($results.Count) exact legacy versions unlisted offline."
    $state.denied = $true
    $state.deleted.Clear()
    $rejected = $false
    try { & (Join-Path $scriptRoot 'migrate_nuget_versions.ps1') -Execute -OutputDirectory $OutputDirectory }
    catch { $rejected = $_.Exception.Message -eq 'Fixture: API key does not grant Unlist permission.' }
    $failedResults = Get-Content (Join-Path $OutputDirectory 'migration-result.json') -Raw
    if (-not $rejected -or $state.deleted.Count -ne 0 -or $failedResults.Trim() -ne '[]') {
        throw 'A denied unlist operation must fail and preserve an empty JSON audit.'
    }
    Write-Output 'PASS: missing Unlist permission fails without reporting successful removals.'
} finally { $env:NUGET_API_KEY = $previousKey }
