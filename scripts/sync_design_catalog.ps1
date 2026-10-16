param([switch] $Check)
$ErrorActionPreference = 'Stop'

# 生成文件随源码提交，正常 dotnet build 不依赖 PowerShell。
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
    $pageResources[$culture] = [ordered]@{ Content = $resources.Content; Scene = $resources.Scene; Segment = $resources.Segment }
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

# 浏览器直接复用控件主题令牌，避免深浅模式、语义色和按钮状态维护两份色值。
function Read-PrototypeColor([hashtable] $Resources, [string] $Key) {
    $visited = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    while ($visited.Add($Key)) {
        $resource = $Resources[$Key]
        if ($null -eq $resource) { throw "原型颜色资源不存在：$Key" }
        if ($resource.LocalName -eq 'StaticResource') {
            $Key = $resource.GetAttribute('ResourceKey')
            continue
        }
        $value = $resource.GetAttribute('Color')
        if (-not $value) { $value = $resource.InnerText.Trim() }
        if ($value -match '^\{StaticResource (.+)\}$') { $Key = $Matches[1]; continue }
        if ($value -notmatch '^(#[0-9A-Fa-f]{6}|White|Black)$') { throw "原型颜色值不受支持：$Key = $value" }
        return $value.ToLowerInvariant()
    }
    throw "原型颜色资源循环引用：$Key"
}
$paletteCss = [Collections.Generic.List[string]]::new()
$paletteCss.Add('/* 生成来源：Themes/Studio/Tokens；执行 scripts/sync_design_catalog.ps1 更新。 */')
$paletteNames = [ordered]@{ DotNet = 'dotnet'; Aurora = 'aurora'; Ember = 'ember'; Default = 'default'; Aquatic = 'aqua'; Desert = 'desert'; Dusk = 'dusk'; NightSky = 'night' }
$roles = [ordered]@{
    'bg' = 'StudioBackgroundColor'; 'bg2' = 'StudioSurfaceColor'; 'bg3' = 'StudioRaisedColor'
    'tx' = 'StudioTextColor'; 'tx2' = 'StudioMutedColor'; 'tx3' = 'StudioMutedColor'
    'bd' = 'StudioBorderColor'; 'bd2' = 'StudioBorderColor'; 'line' = 'StudioBorderColor'
    'hov' = 'StudioRaisedColor'; 'sel' = 'StudioRaisedColor'; 'workbg' = 'StudioRaisedColor'
    'pri' = 'StudioAccentColor'; 'pri-h' = 'StudioAccentColor'; 'brand' = 'StudioPrimaryColor'
    'glow' = 'StudioAccentColor'; 'secondary' = 'StudioSecondaryColor'; 'text-gradient-end' = 'StudioTextGradientEndColor'
    'button-bg' = 'StudioPrimaryColor'; 'button-hover' = 'StudioPrimaryHover'; 'button-pressed' = 'StudioPrimaryPressed'; 'on-primary' = 'StudioOnPrimary'
    'ok' = 'SemiColorSuccess'; 'warn' = 'SemiColorWarning'; 'err' = 'SemiColorDanger'
    'badge-text' = 'StatusBadgeLeftTextForeground'; 'card-top' = 'StatusCardTopTextForeground'
}
foreach ($palette in $paletteNames.Keys) {
    foreach ($mode in @('Light', 'Dark')) {
        $file = if ($palette -eq 'Default') { "$mode.axaml" } else { "$palette$mode.axaml" }
        [xml] $tokens = Get-Content -LiteralPath (Join-Path $repoRoot "src/Controls/CodeWF.Avalonia.Controls/Themes/Studio/Tokens/$file") -Raw -Encoding utf8
        $resources = @{}
        foreach ($node in $tokens.DocumentElement.ChildNodes) {
            if ($node -isnot [Xml.XmlElement]) { continue }
            $key = $node.GetAttribute('Key', 'http://schemas.microsoft.com/winfx/2006/xaml')
            if ($key) {
                if ($resources.ContainsKey($key)) { throw "主题资源键重复：$file/$key" }
                $resources[$key] = $node
            }
        }
        $paletteCss.Add(':root[data-theme=' + $mode.ToLowerInvariant() + '][data-palette=' + $paletteNames[$palette] + '] {')
        foreach ($role in $roles.Keys) { $paletteCss.Add('  --' + $role + ':' + (Read-PrototypeColor $resources $roles[$role]) + ';') }
        foreach ($kind in @('Debug', 'Info', 'Warn', 'Error', 'Fatal')) {
            foreach ($entry in @(
                @(('badge-' + $kind.ToLowerInvariant()), "StatusBadge${kind}BorderBrush"),
                @(('badge-' + $kind.ToLowerInvariant() + '-bg'), "StatusBadge${kind}Background"),
                @(('card-' + $kind.ToLowerInvariant()), "StatusCard${kind}Foreground"),
                @(('card-' + $kind.ToLowerInvariant() + '-bg'), "StatusCard${kind}Background")
            )) { $paletteCss.Add('  --' + $entry[0] + ':' + (Read-PrototypeColor $resources $entry[1]) + ';') }
        }
        $end = $resources['StudioActionGradient'].SelectSingleNode('*[local-name()="GradientStop"][@Offset="1"]').GetAttribute('Color')
        $paletteCss.Add('  --action-gradient:linear-gradient(135deg,var(--button-bg),' + $end.ToLowerInvariant() + ');')
        $paletteCss.Add('  --pri-lt:var(--bg3);')
        $paletteCss.Add('}')
    }
}
Write-GeneratedFile (Join-Path $repoRoot 'scripts/prototype/assets/palette-tokens.css') (($paletteCss -join "`n") + "`n")
Write-Host "Catalog synchronized: $($catalog.Count) groups, $($items.Count) pages."
& (Join-Path $PSScriptRoot 'build_standalone_prototype.ps1') -Check:$Check
