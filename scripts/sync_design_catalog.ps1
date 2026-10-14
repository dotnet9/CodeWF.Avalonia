param()
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$catalogPath = Join-Path $repoRoot 'design/catalog.json'
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding utf8 | ConvertFrom-Json
$translations = [ordered]@{}
$descriptions = [ordered]@{}
$samples = [ordered]@{}
$settings = [ordered]@{}
foreach ($culture in @('zh-CN','en-US')) {
    $path = Join-Path $repoRoot ('demo/CodeWF.Avalonia.Demo/I18n/CodeWF.Avalonia.Demo.' + $culture + '.json')
    $resources = (Get-Content -LiteralPath $path -Raw -Encoding utf8 | ConvertFrom-Json).Demo
    $translations[$culture] = $resources.Catalog
    $descriptions[$culture] = $resources.Api
    $settings[$culture] = [ordered]@{
        shell = $resources.Shell
        palettes = $resources.Palette
        language = $resources.Text.MainViewModel.BuildGlobalProperties025
        group = $resources.Text.MainViewModel.BuildGlobalProperties027
    }
    $samples[$culture] = [ordered]@{
        viewer = $resources.Text.MarkdownViewerPageViewModel.SampleMarkdown001
        lite = $resources.Text.MarkdownLitePageaxaml.SampleMarkdown003
        editor = $resources.Text.MarkdownEditorPageaxaml.SampleMarkdown011
        live = $resources.Text.MarkdownLiveEditorPageaxaml.SampleMarkdown009
        apply = $resources.Text.MarkdownViewerPage.View002
        append = $resources.Text.MarkdownViewerPage.View003
        reset = $resources.Text.MarkdownViewerPage.View004
        themeLabel = $resources.Text.MarkdownViewerPageaxaml.BuildProperties003
        sizeLabel = $resources.Text.MarkdownViewerPageaxaml.BuildProperties005
        virtualizationLabel = $resources.Text.MarkdownViewerPageaxaml.BuildProperties007
        thresholdLabel = $resources.Text.MarkdownViewerPageaxaml.BuildProperties009
    }
}
$typography = [ordered]@{}
$sharedThemes = Join-Path $repoRoot 'src/Markdown/CodeWF.Avalonia.Markdown.Lite/Themes/Shared'
foreach ($name in @('Common', 'Basic', 'OrangeHeart', 'InkBlack', 'ColorfulPurple', 'TenderGreen', 'Verdant', 'RedScarlet', 'BlueGlow', 'TechnologyBlue', 'LanQing', 'Yamabuki', 'FrontendPeak', 'GeekBlack', 'Simple', 'RosePurple', 'CuteGreen', 'FullStackBlue', 'WeChatFormat', 'CompactTypographyResources')) {
    $path = Join-Path $sharedThemes ($name + '.axaml')
    $metrics = [ordered]@{}
    if (Test-Path -LiteralPath $path) {
        [xml] $dictionary = Get-Content -LiteralPath $path -Raw -Encoding utf8
        foreach ($node in $dictionary.DocumentElement.ChildNodes) {
            if ($node.LocalName -eq 'Double') {
                $key = $node.GetAttribute('Key', 'http://schemas.microsoft.com/winfx/2006/xaml')
                $metrics[$key] = [double]::Parse($node.InnerText, [Globalization.CultureInfo]::InvariantCulture)
            }
        }
    }
    $typography[$name] = $metrics
}
$content = '"use strict";' + "`nwindow.CodeWFDemoCatalog = " + (ConvertTo-Json -InputObject $catalog -Depth 20) + ";`nwindow.CodeWFCatalogText = " + (ConvertTo-Json -InputObject $translations -Depth 10) + ";`n"
$content += "window.CodeWFCatalogDescriptions = " + (ConvertTo-Json -InputObject $descriptions -Depth 10) + ";`n"
$content += "window.CodeWFPrototypeSettings = " + (ConvertTo-Json -InputObject $settings -Depth 10) + ";`n"
$content += "window.CodeWFMarkdownSamples = " + (ConvertTo-Json -InputObject $samples -Depth 10) + ";`n"
$content += "window.CodeWFMarkdownTypography = " + (ConvertTo-Json -InputObject $typography -Depth 10) + ";`n"
[System.IO.File]::WriteAllText((Join-Path $repoRoot 'design/assets/catalog-data.js'), $content, [System.Text.UTF8Encoding]::new($false))
