[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("linux-x64", "linux-arm64")]
    [string] $RuntimeIdentifier,

    [Parameter(Mandatory = $true)]
    [string] $Version,

    [string] $SourceDirectory = "",
    [string] $OutputDirectory = "",
    [switch] $Force
)

$ErrorActionPreference = "Stop"
$scriptRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path

$productName = "CodeWF.Avalonia.Demo"
$displayName = "CodeWF.Avalonia Demo"
$packageId = "codewf-avalonia-demo"
$installDir = "/usr/lib/codewf-avalonia-demo"

if ([string]::IsNullOrWhiteSpace($SourceDirectory)) {
    $SourceDirectory = Join-Path $repositoryRoot "artifacts/publish/$RuntimeIdentifier/$productName"
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot "artifacts/release"
}

$sourcePath = (Resolve-Path -LiteralPath $SourceDirectory).Path
$outputPath = [System.IO.Path]::GetFullPath($OutputDirectory)
$cleanVersion = $Version.Trim().TrimStart('v', 'V').Split('+')[0]
if ($cleanVersion -notmatch '^\d+(\.\d+){1,3}$') {
    throw "Version must be numeric (for example 12.1.4.4): $Version"
}

$architecture = if ($RuntimeIdentifier -eq "linux-arm64") { "arm64" } else { "amd64" }
$packageName = "$productName-v$cleanVersion-$RuntimeIdentifier.deb"
$packagePath = Join-Path $outputPath $packageName
$stageRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("$productName-deb-" + [Guid]::NewGuid().ToString("N"))
$appPath = Join-Path $stageRoot "usr/lib/codewf-avalonia-demo"
$binPath = Join-Path $stageRoot "usr/bin"
$desktopPath = Join-Path $stageRoot "usr/share/applications"
$controlPath = Join-Path $stageRoot "DEBIAN"

if ((Test-Path -LiteralPath $packagePath) -and -not $Force) {
    throw "Artifact already exists: $packagePath (use -Force to overwrite)."
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
New-Item -ItemType Directory -Path $appPath, $binPath, $desktopPath, $controlPath -Force | Out-Null

try {
    Get-ChildItem -LiteralPath $sourcePath -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $appPath -Recurse -Force
    }

    $executable = Join-Path $appPath "$productName"
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
        throw "Published executable was not produced: $executable"
    }
    & chmod +x $executable

    @(
        "#!/bin/sh",
        "exec $installDir/$productName `"`$@`""
    ) | Set-Content -LiteralPath (Join-Path $binPath "codewf-avalonia-demo") -Encoding ascii
    & chmod +x (Join-Path $binPath "codewf-avalonia-demo")

    @(
        "[Desktop Entry]",
        "Type=Application",
        "Name=$displayName",
        "Comment=CodeWF.Avalonia control library demo application",
        "Exec=codewf-avalonia-demo",
        "Terminal=false",
        "Categories=Development;"
    ) | Set-Content -LiteralPath (Join-Path $desktopPath "codewf-avalonia-demo.desktop") -Encoding ascii

    @(
        "Package: $packageId",
        "Version: $cleanVersion",
        "Section: devel",
        "Priority: optional",
        "Architecture: $architecture",
        "Maintainer: Dotnet9 <1012434131@qq.com>",
        "Depends: libc6, libx11-6, libxrandr2, libxrender1, libxi6, libfontconfig1, libfreetype6, libglib2.0-0",
        "Description: $displayName",
        " Demo application for the CodeWF.Avalonia control library built with Avalonia."
    ) | Set-Content -LiteralPath (Join-Path $controlPath "control") -Encoding ascii

    if (Test-Path -LiteralPath $packagePath) {
        Remove-Item -LiteralPath $packagePath -Force
    }
    & dpkg-deb --build --root-owner-group $stageRoot $packagePath
    if ($LASTEXITCODE -ne 0) {
        throw "dpkg-deb failed with exit code $LASTEXITCODE."
    }

    $sha = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$sha  $packageName" | Set-Content -LiteralPath "$packagePath.sha256" -Encoding ascii
    Write-Host "Package: $packagePath"
}
finally {
    if (Test-Path -LiteralPath $stageRoot) {
        Remove-Item -LiteralPath $stageRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
