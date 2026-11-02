# 用独立夹具证明源码的 LF/CRLF 不影响单文件 HTML；不修改真实原型。
$ErrorActionPreference = 'Stop'
$builder = Join-Path (Split-Path -Parent $PSScriptRoot) 'build_standalone_prototype.ps1'
$fixtureRoot = Join-Path ([IO.Path]::GetTempPath()) ('prototype-newlines-' + [Guid]::NewGuid().ToString('N'))
$source = Join-Path $fixtureRoot 'scripts/prototype'
$design = Join-Path $fixtureRoot 'design'
try {
    New-Item -ItemType Directory -Path $source, $design -Force | Out-Null
    Copy-Item -LiteralPath $builder -Destination (Join-Path $fixtureRoot 'scripts/build_standalone_prototype.ps1')
    $template = '<!doctype html><html><head><link rel="stylesheet" href="sample.css"></head><body><img src="./logo.svg"><script src="sample.js"></script></body></html>'
    foreach ($name in 'index.template.html', 'property-panel.template.html') { [IO.File]::WriteAllText((Join-Path $source $name), $template) }
    [IO.File]::WriteAllText((Join-Path $source 'sample.css'), 'body { background-image: url("./logo.svg"); }' + [char]10)
    [IO.File]::WriteAllText((Join-Path $source 'sample.js'), 'const icon = "./logo.svg";' + [char]10)
    $svg = '<svg xmlns="http://www.w3.org/2000/svg">' + [char]10 + '<circle cx="8" cy="8" r="8"/>' + [char]10 + '</svg>' + [char]10
    [IO.File]::WriteAllText((Join-Path $source 'logo.svg'), $svg)
    $fixtureBuilder = Join-Path $fixtureRoot 'scripts/build_standalone_prototype.ps1'
    & $fixtureBuilder
    $first = @{}
    foreach ($file in Get-ChildItem -LiteralPath $design -Filter '*.html') { $first[$file.Name] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
    if ($first.Count -eq 0) { throw '夹具没有生成 HTML。' }
    foreach ($file in Get-ChildItem -LiteralPath $source -File) {
        [IO.File]::WriteAllText($file.FullName, [IO.File]::ReadAllText($file.FullName).ReplaceLineEndings(([char]13).ToString() + [char]10))
    }
    & $fixtureBuilder
    & $fixtureBuilder -Check
    foreach ($file in Get-ChildItem -LiteralPath $design -Filter '*.html') {
        if ($first[$file.Name] -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash) { throw "LF 与 CRLF 生成结果不同：$($file.Name)" }
        $html = [IO.File]::ReadAllText($file.FullName)
        if ($html.Contains('src="./logo.svg"') -or -not $html.Contains('data:image/svg+xml;base64,')) { throw 'SVG 没有内嵌。' }
    }
    Write-Host 'PASS：LF/CRLF 的 CSS、JS、SVG 和 HTML 生成完全相同的单文件原型。'

    $repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    $paletteRoot = Join-Path $fixtureRoot 'palette'
    New-Item -ItemType Directory -Path (Join-Path $paletteRoot 'scripts'), (Join-Path $paletteRoot 'docs') -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts/prototype') -Destination (Join-Path $paletteRoot 'scripts/prototype') -Recurse
    Copy-Item -LiteralPath (Join-Path $repoRoot 'docs/配色与排版令牌规则.md') -Destination (Join-Path $paletteRoot 'docs')
    Copy-Item -LiteralPath (Join-Path $repoRoot 'Directory.Build.props') -Destination $paletteRoot
    $generator = Join-Path $paletteRoot 'scripts/prototype/generate-palette-tokens.cjs'
    & node $generator
    if ($LASTEXITCODE -ne 0) { throw '配色夹具生成失败。' }
    foreach ($ending in @("`n", "`r`n")) {
        foreach ($name in 'palette-tokens.css', 'palette-meta.js') {
            $file = Join-Path $paletteRoot "scripts/prototype/assets/$name"
            [IO.File]::WriteAllText($file, [IO.File]::ReadAllText($file).ReplaceLineEndings($ending))
        }
        & node $generator --check
        if ($LASTEXITCODE -ne 0) { throw '配色新鲜度检查误报了换行差异。' }
    }
    [IO.File]::AppendAllText((Join-Path $paletteRoot 'scripts/prototype/assets/palette-tokens.css'), '/* stale */')
    & node $generator --check
    if ($LASTEXITCODE -eq 0) { throw '配色新鲜度检查漏报了内容变更。' }
    $global:LASTEXITCODE = 0
    Write-Host 'PASS：配色产物接受 LF/CRLF，仍拒绝过期内容。'
} finally {
    $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
    $resolvedTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (-not $resolvedFixture.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase)) { throw '拒绝清理临时目录之外的路径。' }
    if (Test-Path -LiteralPath $resolvedFixture) { Remove-Item -LiteralPath $resolvedFixture -Recurse -Force }
}
