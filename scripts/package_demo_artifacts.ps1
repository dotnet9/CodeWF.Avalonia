<#
.SYNOPSIS
    Zip published outputs under artifacts/publish as zip + sha256.
    Shared by publish.bat --package and the release GitHub workflow.

.EXAMPLE
    # Package all RIDs locally (version read from Directory.Build.props)
    powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File scripts/package_demo_artifacts.ps1

    # Package a single RID in CI
    pwsh scripts/package_demo_artifacts.ps1 -RuntimeIdentifier win-x64 -Version 12.1.4.1
#>
param(
    [string]$Version,
    [string]$RuntimeIdentifier,
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

if (-not $Version) {
    $buildProps = Join-Path $RepoRoot 'Directory.Build.props'
    if (-not (Test-Path -LiteralPath $buildProps -PathType Leaf)) {
        throw "Directory.Build.props not found: $buildProps"
    }

    $match = Select-String -LiteralPath $buildProps -Pattern '<Version>(.*?)</Version>' |
        Select-Object -First 1
    if (-not $match) {
        throw "Version not found in $buildProps"
    }

    $Version = $match.Matches[0].Groups[1].Value.Trim()
}

$publishRoot = Join-Path $RepoRoot 'artifacts/publish'
$outDir = Join-Path $RepoRoot 'artifacts/release'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

$rids = if ($RuntimeIdentifier) {
    @($RuntimeIdentifier)
} else {
    if (-not (Test-Path -LiteralPath $publishRoot)) {
        throw "Publish root not found: $publishRoot (run publish.bat first)"
    }

    Get-ChildItem -LiteralPath $publishRoot -Directory | Select-Object -ExpandProperty Name
}

if (-not $rids -or $rids.Count -eq 0) {
    throw "No publish outputs under $publishRoot"
}

foreach ($rid in $rids) {
    $source = Join-Path $publishRoot "$rid/CodeWF.Avalonia.Demo"
    if (-not (Test-Path -LiteralPath $source -PathType Container)) {
        throw "Publish output missing: $source"
    }

    $zipName = "CodeWF.Avalonia.Demo-v$Version-$rid.zip"
    $zipPath = Join-Path $outDir $zipName
    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    Compress-Archive -Path (Join-Path $source '*') -DestinationPath $zipPath -CompressionLevel Optimal

    $sha = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$sha  $zipName" | Set-Content -LiteralPath "$zipPath.sha256" -Encoding ASCII

    Write-Host "Packaged: $zipPath"
}
