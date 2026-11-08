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
        if ($state.quotaOnce) {
            $state.quotaOnce = $false
            $record = [System.Management.Automation.ErrorRecord]::new([Exception]::new('Quota exceeded'), 'RateLimit',
                [System.Management.Automation.ErrorCategory]::LimitsExceeded, $Uri)
            $record.ErrorDetails = [System.Management.Automation.ErrorDetails]::new(
                '{"statusCode":403,"message":"Out of call volume quota. Quota will be replenished in 00:00:01."}')
            throw $record
        }
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
    $resume = Join-Path $OutputDirectory 'accepted-first-batch.json'
    ConvertTo-Json -InputObject @($results | Select-Object -First 315) | Set-Content $resume -Encoding utf8
    $state.denied = $false
    $state.quotaOnce = $true
    $state.sleeps = 0
    & (Join-Path $scriptRoot 'migrate_nuget_versions.ps1') -Execute -OutputDirectory $OutputDirectory -ResumeResult $resume
    $resumedResults = @(Get-Content (Join-Path $OutputDirectory 'migration-result.json') -Raw | ConvertFrom-Json)
    if ($state.deleted.Count -ne 19 -or $state.sleeps -ne 1 -or $resumedResults.Count -ne 334) {
        throw 'Quota recovery must only send the nineteen remaining unlist requests.'
    }
    Write-Output 'PASS: resumes 315 accepted requests, waits for the quota reset, and sends only the remaining 19.'
} finally { $env:NUGET_API_KEY = $previousKey }
