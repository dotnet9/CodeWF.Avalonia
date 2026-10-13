param()
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$catalogPath = Join-Path $repoRoot 'design/catalog.json'
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding utf8 | ConvertFrom-Json
$translations = [ordered]@{}
$descriptions = [ordered]@{}
foreach ($culture in @('zh-CN','en-US')) {
    $path = Join-Path $repoRoot ('demo/CodeWF.Avalonia.Demo/I18n/CodeWF.Avalonia.Demo.' + $culture + '.json')
    $resources = (Get-Content -LiteralPath $path -Raw -Encoding utf8 | ConvertFrom-Json).Demo
    $translations[$culture] = $resources.Catalog
    $descriptions[$culture] = $resources.Api
}
$content = '"use strict";' + "`nwindow.CodeWFDemoCatalog = " + (ConvertTo-Json -InputObject $catalog -Depth 20) + ";`nwindow.CodeWFCatalogText = " + (ConvertTo-Json -InputObject $translations -Depth 10) + ";`n"
$content += "window.CodeWFCatalogDescriptions = " + (ConvertTo-Json -InputObject $descriptions -Depth 10) + ";`n"
[System.IO.File]::WriteAllText((Join-Path $repoRoot 'design/assets/catalog-data.js'), $content, [System.Text.UTF8Encoding]::new($false))
