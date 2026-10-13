<#
.SYNOPSIS
    Zip published outputs under artifacts/publish as zip + sha256.
    Shared by local packaging and the release GitHub workflow.

.EXAMPLE
    # Package all published RIDs locally (version read from Directory.Build.props)
    powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File scripts/package_demo_artifacts.ps1

    # Package a single RID in CI
    pwsh scripts/package_demo_artifacts.ps1 -RuntimeIdentifier win-x64 -Version 12.1.4.4
#>
param(
    [string]$Version,
    [string]$RuntimeIdentifier,
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$productName = 'CodeWF.Avalonia.Demo'

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
        throw "Publish root not found: $publishRoot (run the publish script first)"
    }

    Get-ChildItem -LiteralPath $publishRoot -Directory | Select-Object -ExpandProperty Name
}

if (-not $rids -or $rids.Count -eq 0) {
    throw "No publish outputs under $publishRoot"
}

foreach ($rid in $rids) {
    $source = Join-Path $publishRoot "$rid/$productName"
    if (-not (Test-Path -LiteralPath $source -PathType Container)) {
        throw "Publish output missing: $source"
    }

    $zipName = "$productName-v$Version-$rid.zip"
    $zipPath = Join-Path $outDir $zipName
    if ((Test-Path -LiteralPath $zipPath) -and -not $Force) {
        throw "Artifact already exists: $zipPath (use -Force to overwrite)."
    }
    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    # 便携包不带调试符号（裁剪后的 pdb 对最终用户无用）
    $files = @(Get-ChildItem -LiteralPath $source -Recurse -File |
        Where-Object { $_.Extension -notin '.pdb', '.dbg' })
    if ($files.Count -eq 0) {
        throw "Publish directory contains no release files: $source"
    }

    $stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("$productName-zip-" + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null
    try {
        $prefix = (Resolve-Path -LiteralPath $source).Path.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
        foreach ($file in $files) {
            $relative = $file.FullName.Substring($prefix.Length)
            $target = Join-Path $stagingRoot $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            Copy-Item -LiteralPath $file.FullName -Destination $target -Force
        }

        Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
    }
    finally {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    $sha = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$sha  $zipName" | Set-Content -LiteralPath "$zipPath.sha256" -Encoding ASCII

    Write-Host "Packaged: $zipPath"
}
