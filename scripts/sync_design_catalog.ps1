param([switch] $Check)
$ErrorActionPreference = 'Stop'

# Generated files are checked in; a normal dotnet build does not require PowerShell.
function Write-GeneratedFile([string] $Path, [string] $Content) {
    if ($Check) {
        if (-not (Test-Path -LiteralPath $Path) -or
            [IO.File]::ReadAllText($Path).ReplaceLineEndings("`n") -cne $Content.ReplaceLineEndings("`n")) {
            throw "Generated catalog file is stale: $Path. Run scripts/sync_design_catalog.ps1."
        }
    } else {
        [IO.File]::WriteAllText($Path, $Content, [Text.UTF8Encoding]::new($false))
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$catalogPath = Join-Path $repoRoot 'design/catalog.json'
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding utf8 | ConvertFrom-Json
$translations = [ordered]@{}
$descriptions = [ordered]@{}
$samples = [ordered]@{}
$settings = [ordered]@{}
$pageResources = [ordered]@{}
foreach ($culture in @('zh-CN','en-US')) {
    $path = Join-Path $repoRoot ('demo/CodeWF.Avalonia.Demo/I18n/CodeWF.Avalonia.Demo.' + $culture + '.json')
    $resources = (Get-Content -LiteralPath $path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable).Demo
    $translations[$culture] = $resources.Catalog
    $descriptions[$culture] = $resources.Api
    $pageResources[$culture] = [ordered]@{ Content = $resources.Content; Scene = $resources.Scene }
    $settings[$culture] = [ordered]@{
        shell = $resources.Shell
        palettes = $resources.Palette
        language = $resources.Shell.GlobalSettings.LanguageLabel
        group = $resources.Shell.GlobalSettings.AppearanceGroup
    }
    $samples[$culture] = [ordered]@{
        viewer = $resources.Content.MarkdownViewer.Samples.Markdown
        lite = $resources.Content.MarkdownLite.Samples.Markdown
        editor = $resources.Content.MarkdownEditor.Samples.Markdown
        live = $resources.Content.MarkdownLiveEditor.Samples.Markdown
        apply = $resources.Content.MarkdownViewer.View.Apply
        append = $resources.Content.MarkdownViewer.View.AppendParagraphIncremental
        reset = $resources.Content.MarkdownViewer.View.ResetSample
        themeLabel = $resources.Content.MarkdownViewer.Properties.TypographyTheme
        sizeLabel = $resources.Content.MarkdownViewer.Properties.TypographySize
        virtualizationLabel = $resources.Content.MarkdownViewer.Properties.Virtualization
        thresholdLabel = $resources.Content.MarkdownViewer.Properties.VirtualizationThreshold
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
$content += "window.CodeWFPrototypeResources = " + (ConvertTo-Json -InputObject $pageResources -Depth 20) + ";`n"
$content += "window.CodeWFMarkdownSamples = " + (ConvertTo-Json -InputObject $samples -Depth 10) + ";`n"
$content += "window.CodeWFMarkdownTypography = " + (ConvertTo-Json -InputObject $typography -Depth 10) + ";`n"
Write-GeneratedFile (Join-Path $repoRoot 'scripts/prototype/assets/catalog-data.js') $content

$items = @($catalog | ForEach-Object { $_.items })
$keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$routes = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$constants = [Collections.Generic.List[string]]::new()
$factories = [Collections.Generic.List[string]]::new()
foreach ($item in $items) {
    $name = ($item.titleKey -split '\.')[ -1 ]
    if ($name -cnotmatch '^[A-Za-z][A-Za-z0-9]*$' -or -not $keys.Add($item.titleKey) -or -not $routes.Add($item.prototypeId)) { throw "Invalid or duplicate catalog item: $($item.titleKey)" }
    if ([string]::IsNullOrWhiteSpace($item.iconKey) -or [string]::IsNullOrWhiteSpace($item.icon) -or [string]::IsNullOrWhiteSpace($item.prototypeIcon)) { throw "Missing catalog icon: $name" }
    $expression = [string] $item.page
    if ($expression -cnotmatch '^[A-Za-z][A-Za-z0-9]*(\(.*\))?$' -or $expression -match '[;{}\r\n]') { throw "Invalid page expression: $name" }
    if (-not $expression.EndsWith(')')) { $expression += '()' }
    $constants.Add('    public const string ' + $name + ' = "' + $item.titleKey + '";')
    $factories.Add('        DemoPages.' + $name + ' => new ' + $expression + ',')
}
$code = @"
// <auto-generated />
// 来源：design/catalog.json；执行 scripts/sync_design_catalog.ps1 更新。
using CodeWF.Avalonia.Demo.Views;
using CodeWF.Avalonia.Demo.Views.Pages;
using CodeWF.Avalonia.Demo.Views.Scenes;
using CodeWF.Avalonia.Demo.ViewModels.Pages;

namespace CodeWF.Avalonia.Demo.ViewModels;

public static class DemoPages
{
$($constants -join "`n")
}

public static partial class DemoCatalog
{
    private static DemoPageBase CreatePage(string key) => key switch
    {
$($factories -join "`n")
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown catalog API.")
    };
}
"@ + "`n"
Write-GeneratedFile (Join-Path $repoRoot 'demo/CodeWF.Avalonia.Demo/ViewModels/DemoCatalog.g.cs') $code
Write-Host "Catalog synchronized: $($catalog.Count) groups, $($items.Count) pages."
& (Join-Path $PSScriptRoot 'build_standalone_prototype.ps1') -Check:$Check
