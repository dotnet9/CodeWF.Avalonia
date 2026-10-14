param([switch] $Check)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'T4 tool restore failed.' }
    $templates = @(Get-ChildItem demo,src -Filter Language.tt -Recurse)
    foreach ($template in $templates) {
        $templateText = [IO.File]::ReadAllText($template.FullName)
        if ($templateText -notmatch '<#@ output extension="(?<extension>\.g\.cs|\.cs)"') {
            throw "Unsupported T4 output extension: $($template.FullName)"
        }
        $outputExtension = $Matches.extension
        $output = Join-Path $template.DirectoryName ('Language' + $outputExtension)
        $temporaryOutput = Join-Path ([IO.Path]::GetTempPath()) ('codewf-t4-' + [Guid]::NewGuid().ToString('N') + '.tmp')
        # 即使指定 -o，T4 输出指令仍会替换输出文件的扩展名。
        $generatedOutput = [IO.Path]::ChangeExtension($temporaryOutput, $outputExtension)
        try {
            dotnet tool run t4 -- $template.FullName -o $temporaryOutput
            if ($LASTEXITCODE -ne 0) { throw "T4 transformation failed: $($template.FullName)" }
            $generated = [IO.File]::ReadAllText($generatedOutput).ReplaceLineEndings("`n")
            if ([string]::IsNullOrWhiteSpace($generated)) { throw "T4 generated no resource keys: $($template.FullName)" }
            if ($Check) {
                if (-not (Test-Path -LiteralPath $output) -or [IO.File]::ReadAllText($output).ReplaceLineEndings("`n") -cne $generated) {
                    throw "Stale resource keys: $output. Run scripts/sync_localization_keys.ps1."
                }
            } else {
                [IO.File]::WriteAllText($output, $generated, [Text.UTF8Encoding]::new($false))
            }
        } finally {
            if (Test-Path -LiteralPath $generatedOutput) { Remove-Item -LiteralPath $generatedOutput -Force }
        }
    }
    Write-Host "T4 resource mappings verified: $($templates.Count) templates."
} finally { Pop-Location }
