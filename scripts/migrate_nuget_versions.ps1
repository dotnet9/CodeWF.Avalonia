[CmdletBinding()]
param(
    [switch]$Execute,
    [string]$ResumeResult = '',
    [string]$OutputDirectory = (Join-Path ([IO.Path]::GetTempPath()) 'CodeWFAvaloniaVersionMigration')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content (Join-Path $PSScriptRoot 'nuget-version-migration.json') -Raw | ConvertFrom-Json
[xml]$props = Get-Content (Join-Path $root 'Directory.Build.props') -Raw
[xml]$versions = Get-Content (Join-Path $root 'build/Versions.props') -Raw
if ($manifest.replacementVersion -ne $props.Project.PropertyGroup.Version -or
    $manifest.avaloniaVersion -ne $versions.Project.PropertyGroup.AvaloniaVersion -or
    $manifest.replacementVersion -ne '12.1.5.2') { throw 'Migration version does not match the release.' }
$ids = @(Get-ChildItem (Join-Path $root 'src') -Filter '*.csproj' -Recurse |
    Where-Object { (Get-Content $_.FullName -Raw) -match '<IsPackable>true</IsPackable>' } |
    ForEach-Object BaseName | Sort-Object)
if ($ids.Count -ne 11 -or (Compare-Object $ids @($manifest.packageIds | Sort-Object))) {
    throw 'Migration package IDs must match exactly the eleven library projects.'
}
foreach ($package in $manifest.packages) {
    if ($package.id -notin $ids -or $package.version -notmatch '^12\.2\.\d+(?:\.\d+)?(?:-[0-9A-Za-z.-]+)?$') {
        throw 'Only the reviewed 12.2.xx package versions can be unlisted.'
    }
}
Write-Output "Validated migration: $($manifest.packages.Count) legacy versions across $($ids.Count) packages -> $($manifest.replacementVersion)"
if (-not $Execute) { return }
if ([string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) { throw 'A NuGet API key with Unlist permission is required.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

# 先证明新版本可公开下载，再隐藏旧系列；只使用官方源，不借用构建产物或本地缓存。
$remaining = @($ids)
$deadline = [DateTime]::UtcNow.AddMinutes(20)
while ($remaining.Count) {
    $remaining = @(foreach ($id in $remaining) {
        $download = Join-Path $OutputDirectory "$id.$($manifest.replacementVersion).nupkg"
        $available = $false
        try {
            $response = Invoke-WebRequest "https://www.nuget.org/api/v2/package/$id/$($manifest.replacementVersion)" -OutFile $download -PassThru -TimeoutSec 60
            if ($response.StatusCode -eq 200) {
                $stream = [IO.File]::OpenRead($download)
                try { $available = $stream.Length -gt 4 -and $stream.ReadByte() -eq 0x50 -and $stream.ReadByte() -eq 0x4b }
                finally { $stream.Dispose() }
            }
        } catch { Write-Host "Waiting for public package: $id" }
        if (-not $available) { $id }
    })
    if ($remaining.Count) {
        if ([DateTime]::UtcNow -ge $deadline) { throw "Public replacement packages are not ready: $($remaining -join ', ')" }
        Start-Sleep -Seconds 30
    }
}

$results = [Collections.Generic.List[object]]::new()
$completed = @{}
if ($ResumeResult) {
    foreach ($entry in @(Get-Content -LiteralPath $ResumeResult -Raw | ConvertFrom-Json)) {
        $key = "$($entry.id)/$($entry.version)"
        if ($entry.status -notin @(200, 202, 204) -or $completed.ContainsKey($key) -or
            -not @($manifest.packages | Where-Object { $_.id -ceq $entry.id -and $_.version -ceq $entry.version }).Count) {
            throw 'Resume audit contains an invalid or duplicate completed package.'
        }
        $completed[$key] = $true
        $results.Add($entry)
    }
    Write-Output "Resuming after $($completed.Count) accepted unlist requests."
}
try {
    foreach ($package in $manifest.packages) {
        if ($completed.ContainsKey("$($package.id)/$($package.version)")) { continue }
        $uri = "https://www.nuget.org/api/v2/package/$($package.id)/$($package.version)"
        while ($true) {
            try {
                $response = Invoke-WebRequest -Uri $uri -Method Delete -Headers @{ 'X-NuGet-ApiKey' = $env:NUGET_API_KEY } -TimeoutSec 60
                break
            } catch {
                if ($_.ErrorDetails.Message -notmatch 'Out of call volume quota.*replenished in (\d{2}:\d{2}:\d{2})') { throw }
                $delay = [TimeSpan]::Parse($Matches[1], [Globalization.CultureInfo]::InvariantCulture)
                if ($delay.TotalSeconds -gt 3600) { throw }
                Write-Host "NuGet quota exhausted; resume at $([DateTime]::UtcNow.Add($delay).ToString('O'))."
                Start-Sleep -Seconds ([int][Math]::Ceiling($delay.TotalSeconds) + 5)
            }
        }
        if ($response.StatusCode -notin @(200, 202, 204)) { throw "Unlist failed: $($package.id)/$($package.version)" }
        $results.Add([pscustomobject]@{ id = $package.id; version = $package.version; status = $response.StatusCode })
        Write-Output "Unlisted $($package.id) $($package.version)"
    }
}
finally {
    ConvertTo-Json -InputObject @($results.ToArray()) -Depth 4 | Set-Content (Join-Path $OutputDirectory 'migration-result.json') -Encoding utf8
}
Write-Output "Unlisting accepted: $($results.Count) versions. Explicit old references remain restorable."
