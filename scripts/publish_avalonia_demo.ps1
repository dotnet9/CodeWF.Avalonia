[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
    [string] $RuntimeIdentifier,

    [string] $Version = ""
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$projectPath = Join-Path $repositoryRoot "demo/CodeWF.Avalonia.Demo/CodeWF.Avalonia.Demo.csproj"

if ([string]::IsNullOrWhiteSpace($Version)) {
    $props = Join-Path $repositoryRoot "Directory.Build.props"
    $match = Select-String -LiteralPath $props -Pattern '<Version>([^<]+)</Version>' | Select-Object -First 1
    if ($match) { $Version = $match.Matches[0].Groups[1].Value.Trim() }
    if ([string]::IsNullOrWhiteSpace($Version)) { $Version = "0.0.0" }
}

# 与 publish.bat / release 工作流使用同一套 PublishProfile（TFM 由 Profile 声明：
# win-* 为 net10.0-windows，其余为 net10.0；不传 -f，避免全局 TFM 传播到单 TFM 引用库触发 NETSDK1005）。
# 输出目录显式用 -o 固定为 artifacts/publish/<rid>/CodeWF.Avalonia.Demo（-o 覆盖 Profile 的 PublishDir，
# 保证 CI 各处路径一致）；Profile 仍负责 TFM、SelfContained 与 AOT 设定。
# 全平台 NativeAOT：完整反射元数据保全 Prism/DryIoc，单线程 ILC 更稳。
$outputPath = Join-Path $repositoryRoot "artifacts/publish/$RuntimeIdentifier/CodeWF.Avalonia.Demo"
$outputPath = [IO.Path]::GetFullPath($outputPath)
$allowedOutputRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/publish')) + [IO.Path]::DirectorySeparatorChar
if (-not $outputPath.StartsWith($allowedOutputRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "发布输出目录超出仓库产物范围：$outputPath"
}
if (Test-Path -LiteralPath $outputPath) {
    Remove-Item -LiteralPath $outputPath -Recurse -Force
}

$ilcArgs = @(
    "-p:PublishAot=true",
    "-p:PublishTrimmed=true",
    "-p:PublishSingleFile=false",
    "-p:DebugSymbols=true",
    "-p:DebugType=portable",
    "-p:IlcGenerateCompleteTypeMetadata=true",
    "-p:IlcTrimMetadata=false",
    "-p:IlcSingleThreaded=true",
    "-p:Version=$Version"
)
# macOS 保留符号（ld_classic 不支持压缩调试段）
if ($RuntimeIdentifier.StartsWith("osx-", [StringComparison]::OrdinalIgnoreCase)) { $ilcArgs += "-p:StripSymbols=false" }

Write-Host "发布 CodeWF.Avalonia.Demo $RuntimeIdentifier (Version=$Version)"
& dotnet publish $projectPath -c Release -r $RuntimeIdentifier -m:1 @ilcArgs "/p:PublishProfile=FolderProfile__$RuntimeIdentifier" -o $outputPath
if ($LASTEXITCODE -ne 0) { throw "publish failed for $RuntimeIdentifier" }

if (-not (Test-Path -LiteralPath $outputPath -PathType Container)) {
    throw "Publish output missing: $outputPath"
}

Write-Host "已发布到 $outputPath"
