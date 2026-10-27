#!/usr/bin/env pwsh
# AOT 裁剪告警门禁：发布日志里只要出现「源自本仓代码」的 IL 告警就红。
#
# 判定只看告警归属：ILC 的告警行以来源路径开头（本地代码是 .cs 文件路径，包内代码是 .nuget 缓存里的 dll 路径）。
# 第三方包的整包摘要（IL2104/IL3053）与单文件反射提示（IL3000/IL3001/IL3002）不属我们能改的范围，
# 按告警码白名单放行；本仓 src/、demo/、scripts/ 的代码一旦出现 IL 告警，说明裁剪/AOT 兼容性被破坏，
# 必须修掉，或加 UnconditionalSuppressMessage 并写清理由。
#
# 用法：pwsh scripts/tests/il_trim_warnings_test.ps1 -Log artifacts/aot-publish.log
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Log,
    # 允许的告警码（默认放行第三方整包摘要与单文件反射提示）。
    [string[]] $AllowCodes = @('IL2104', 'IL3053', 'IL3000', 'IL3001', 'IL3002')
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $Log)) { throw "AOT 发布日志不存在：$Log" }

$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$warningPattern = [regex]'(?<code>IL\d{4})'

$offenders = New-Object System.Collections.Generic.List[string]
$seenCodes = @{}
foreach ($line in Get-Content -LiteralPath $Log) {
    $match = $warningPattern.Match($line)
    if (-not $match.Success) { continue }
    $code = $match.Groups['code'].Value
    $seenCodes[$code] = 1 + ($seenCodes[$code] ?? 0)

    # 告警来源路径 = 「: warning 」之前的部分；只有落在本仓根下才算我们的代码。
    $source = ($line -split ': warning ', 2)[0].Trim()
    if (-not $source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) { continue }
    if ($AllowCodes -contains $code) { continue }
    $offenders.Add($line.Trim())
}

$summary = ($seenCodes.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Name)×$($_.Value)" }) -join ' '
Write-Host "IL 告警统计：$(if ($summary) { $summary } else { '无' })"

if ($offenders.Count -gt 0) {
    Write-Host '本仓代码产生了 IL 裁剪/AOT 告警（修复或显式抑制后再提交）：'
    foreach ($offender in $offenders) { Write-Host "  $offender" }
    exit 1
}

Write-Host 'PASS: AOT 发布日志中无源自本仓代码的 IL 告警。'
