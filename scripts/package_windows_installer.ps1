[CmdletBinding()]
param(
    [string] $Version = '',
    [string] $SourceDirectory = '',
    [string] $OutputDirectory = '',
    [string] $InnoSetupCompiler = ''
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml] $properties = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Directory.Build.props') -Raw
    $Version = [string] $properties.Project.PropertyGroup.Version
}
if ($Version -notmatch '^\d+(\.\d+){1,3}$') { throw "版本号无效：$Version" }
if ([string]::IsNullOrWhiteSpace($SourceDirectory)) {
    $SourceDirectory = Join-Path $repositoryRoot 'artifacts/publish/win-x64/CodeWF.Avalonia.Demo'
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot 'artifacts/release'
}
if ([string]::IsNullOrWhiteSpace($InnoSetupCompiler)) {
    $command = Get-Command ISCC.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) { $InnoSetupCompiler = $command.Source }
    elseif (${env:ProgramFiles(x86)}) { $InnoSetupCompiler = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe' }
}
if (-not $InnoSetupCompiler -or -not (Test-Path -LiteralPath $InnoSetupCompiler -PathType Leaf)) {
    throw '请安装 Inno Setup 6，或通过 -InnoSetupCompiler 指定编译器。'
}
if (-not (Test-Path -LiteralPath (Join-Path $SourceDirectory 'CodeWF.Avalonia.Demo.exe') -PathType Leaf)) {
    throw "未找到 Windows 发布程序：$SourceDirectory"
}
$sourcePath = (Resolve-Path -LiteralPath $SourceDirectory).Path
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
& $InnoSetupCompiler (Join-Path $PSScriptRoot 'CodeWF.Avalonia.Demo.iss') "/DAppVersion=$Version" "/DSourceDir=$sourcePath" "/DOutputDir=$outputPath"
if ($LASTEXITCODE -ne 0) { throw "安装包编译失败：$LASTEXITCODE" }
$name = 'CodeWF.Avalonia.Demo-v' + $Version + '-win-x64-setup.exe'
$installer = Join-Path $outputPath $name
if (-not (Test-Path -LiteralPath $installer -PathType Leaf) -or (Get-Item -LiteralPath $installer).Length -eq 0) {
    throw "未生成安装包：$installer"
}
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $name" | Set-Content -LiteralPath "$installer.sha256" -Encoding ascii
Write-Host "已生成安装包：$installer"
