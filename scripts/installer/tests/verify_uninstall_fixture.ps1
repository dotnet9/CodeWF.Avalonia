#requires -Version 7.0
[CmdletBinding()]
param(
 [Parameter(Mandatory)][string]$FixtureDirectory,
 [ValidateSet('english','chinesesimplified')][string]$Language='english',
 [switch]$Silent,
 [switch]$MissingAssets
)
$ErrorActionPreference='Stop'
if(-not $IsWindows){throw '此隔离验证器仅支持 Windows / PowerShell 7。'}
if($Silent -and $MissingAssets){throw '静默与缺资源模式应分别运行。'}
$taskRoot=(Resolve-Path -LiteralPath $FixtureDirectory).Path
$taskBuild=Get-Content -LiteralPath (Join-Path $taskRoot 'build-receipt.json') -Raw -Encoding utf8 | ConvertFrom-Json
if($taskBuild.app_id -ne 'CodeWF.Uninstall.Branding.Fixture.IsolatedOnly' -or $taskBuild.compile_exit_code -ne 0){throw '专用夹具构建收据无效。'}
$taskInstaller=Join-Path $taskRoot 'CodeWF.Uninstall.Branding.Fixture.exe'
$taskSourcePath=Join-Path $taskRoot 'CodeWF.Uninstall.Branding.Fixture.iss'
foreach($taskPair in @(
 @($taskInstaller,$taskBuild.binary_sha256),
 @($taskSourcePath,$taskBuild.source_sha256),
 @((Join-Path $PSScriptRoot 'Fixture.Uninstall.iss'),$taskBuild.observer_sha256),
 @((Join-Path $PSScriptRoot 'build_uninstall_fixture.py'),$taskBuild.generator_sha256),
 @($taskBuild.include,$taskBuild.include_sha256)
)){
 if((Get-FileHash -LiteralPath $taskPair[0] -Algorithm SHA256).Hash.ToLowerInvariant() -ne $taskPair[1]){throw "夹具源或二进制已变化，请重新构建：$($taskPair[0])"}
}
$taskSource=Get-Content -LiteralPath $taskSourcePath -Raw -Encoding utf8
if($taskSource -notmatch '(?m)^AppId=CodeWF.Uninstall.Branding.Fixture.IsolatedOnly\r?$' -or
   $taskSource -notmatch '(?m)^CreateUninstallRegKey=no\r?$' -or
   $taskSource -notmatch '(?m)^PrivilegesRequired=lowest\r?$' -or
   $taskSource -notmatch '(?m)^#define CodeWFSetupUninstallFixture\r?$' -or
   $taskSource -match '(?im)^\[(Registry|Icons|Run|UninstallRun|UninstallDelete)\]') { throw 'Unsafe fixture source' }
$taskRun=Join-Path $taskRoot ('run-'+$Language+'-'+[guid]::NewGuid().ToString('N'))
$taskInstall=Join-Path $taskRun 'app'
New-Item -ItemType Directory -Path $taskRun | Out-Null
$taskStart=[Diagnostics.ProcessStartInfo]::new($taskInstaller)
$taskStart.UseShellExecute=$false
foreach($taskArgument in @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LANG=$Language","/DIR=$taskInstall","/LOG=$taskRun/install.log")){[void]$taskStart.ArgumentList.Add($taskArgument)}
$taskSetup=$null
$taskMutex=[Threading.Mutex]::new($false,'Local\CodeWF.Uninstall.Fixture.Verification')
$taskHasMutex=$false
Add-Type -AssemblyName System.Drawing
if(-not ('CodeWFUninstallFixtureDesktop' -as [type])){
 Add-Type -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.Fixture.Desktop.cs') -ReferencedAssemblies System.Drawing.Common,System.Runtime,System.Collections,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core,System.Security.Cryptography
}
$taskDesktop=[IntPtr]::Zero
$taskProcessInfo=[CodeWFUninstallFixtureDesktop+ProcessInfo]::new()
$taskHandles=[Collections.Generic.Dictionary[uint32,IntPtr]]::new()
$taskAssertions=[Collections.Generic.List[string]]::new()
function Check([bool]$Condition,[string]$Message){if(-not $Condition){throw $Message};$taskAssertions.Add($Message);Write-Output "PASS: $Message"}
function Register-Owned {
 $taskProcesses=@(Get-CimInstance Win32_Process | Select-Object ProcessId,ParentProcessId)
 $taskKnown=[Collections.Generic.HashSet[uint32]]::new()
 [void]$taskKnown.Add([uint32]$taskProcessInfo.processId)
 foreach($taskOwnedId in $taskHandles.Keys){[void]$taskKnown.Add($taskOwnedId)}
 do{$taskAdded=$false;foreach($taskItem in $taskProcesses){if($taskKnown.Contains([uint32]$taskItem.ParentProcessId) -and $taskKnown.Add([uint32]$taskItem.ProcessId)){$taskAdded=$true}}}while($taskAdded)
 foreach($taskOwnedId in $taskKnown){if(-not $taskHandles.ContainsKey($taskOwnedId)){$taskHandle=[CodeWFUninstallFixtureDesktop]::OpenProcess(0x00100001,$false,$taskOwnedId);if($taskHandle -ne [IntPtr]::Zero){$taskHandles.Add($taskOwnedId,$taskHandle)}}}
}
function Windows { @([CodeWFUninstallFixtureDesktop]::Windows($taskDesktop) | Where-Object {[CodeWFUninstallFixtureDesktop]::IsWindowVisible($_)}) }
function Click-Dialog([IntPtr]$Dialog,[string]$Pattern){
 $taskButton=@([CodeWFUninstallFixtureDesktop]::Children($Dialog) | Where-Object {[CodeWFUninstallFixtureDesktop]::Class($_) -eq 'TNewButton' -or [CodeWFUninstallFixtureDesktop]::Class($_) -eq 'Button'} | Where-Object {([CodeWFUninstallFixtureDesktop]::Text($_) -replace '&','') -match $Pattern}) | Select-Object -First 1
 if(-not $taskButton){throw "Fixture dialog button missing: $Pattern"}
 [void][CodeWFUninstallFixtureDesktop]::Send($taskButton,245)
}
try{
 if(-not $taskMutex.WaitOne(0)){throw '卸载夹具模式必须串行执行。'}
 $taskHasMutex=$true
 $taskSetup=[Diagnostics.Process]::Start($taskStart)
 if(-not $taskSetup.WaitForExit(45000)){ $taskSetup.Kill($true); throw 'Safe fixture installation timed out' }
 if($taskSetup.ExitCode -ne 0){throw 'Safe fixture installation failed'}
 $taskUninstaller=Join-Path $taskInstall 'unins000.exe'
 if(-not (Test-Path -LiteralPath $taskUninstaller -PathType Leaf)){throw 'Fixture uninstaller missing'}
 if(-not [IO.Path]::GetFullPath($taskUninstaller).StartsWith([IO.Path]::GetFullPath($taskRun)+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Uninstaller outside fixture run directory'}
 [IO.File]::WriteAllText((Join-Path $taskInstall 'unregistered-user-note.txt'),'fixture user data must survive')
 if($MissingAssets){Remove-Item -LiteralPath (Join-Path $taskInstall '.codewf-installer/frame-dark.bmp') -Force}
 $taskDesktopName='CodeWF.Uninstall.Safe.'+[guid]::NewGuid().ToString('N')
 $taskDesktop=[CodeWFUninstallFixtureDesktop]::CreateDesktop($taskDesktopName,[IntPtr]::Zero,[IntPtr]::Zero,0,0x01ff,[IntPtr]::Zero)
 if($taskDesktop -eq [IntPtr]::Zero){throw 'Cannot create isolated desktop'}
 $taskStartup=[CodeWFUninstallFixtureDesktop+StartupInfo]::new();$taskStartup.cb=[Runtime.InteropServices.Marshal]::SizeOf($taskStartup);$taskStartup.desktop=$taskDesktopName
 $taskArguments=if($Silent){'/VERYSILENT /SUPPRESSMSGBOXES /NORESTART'}else{'/NORESTART'}
 $taskCommand=[Text.StringBuilder]::new('"'+$taskUninstaller+'" '+$taskArguments+' /LOG="'+$taskRun+'/uninstall.log"')
 if(-not [CodeWFUninstallFixtureDesktop]::CreateProcess($taskUninstaller,$taskCommand,[IntPtr]::Zero,[IntPtr]::Zero,$false,0,[IntPtr]::Zero,$taskInstall,[ref]$taskStartup,[ref]$taskProcessInfo)){throw 'Cannot launch fixture uninstaller'}
 $taskHandles.Add([uint32]$taskProcessInfo.processId,$taskProcessInfo.process)
 $taskSeen=[Collections.Generic.HashSet[string]]::new()
 $taskTimer=[Diagnostics.Stopwatch]::StartNew()
 $taskLastRegister=-5
 $taskLastClosed=[DateTime]::MinValue
 while($taskTimer.Elapsed.TotalSeconds -lt 100){
  if($taskTimer.Elapsed.TotalSeconds-$taskLastRegister -gt 3){Register-Owned;$taskLastRegister=$taskTimer.Elapsed.TotalSeconds}
  # 静默卸载的短命窗口可能在枚举后立即销毁，只等待独立证据文件。
  $taskWindows=@()
  if(-not $Silent){$taskWindows=Windows}
  foreach($taskWindow in $taskWindows){
   $taskClass=[CodeWFUninstallFixtureDesktop]::Class($taskWindow)
   if($taskClass -eq 'TUninstallProgressForm'){continue}
   $taskChildren=@([CodeWFUninstallFixtureDesktop]::Children($taskWindow))
   $taskContents=($taskChildren | ForEach-Object {[CodeWFUninstallFixtureDesktop]::Text($_)}) -join ' '
   if($taskContents -match 'HOLD ([a-z-]+)'){
    $taskStage=$Matches[1]
    if($taskSeen.Add($taskStage)){
     $taskEvidence=Get-Content -LiteralPath (Join-Path $taskRun "evidence/$taskStage.json") -Raw | ConvertFrom-Json
     $taskForm=[IntPtr][long]$taskEvidence.form
     Register-Owned
     [uint32]$taskOwnerId=0;[void][CodeWFUninstallFixtureDesktop]::GetWindowThreadProcessId($taskForm,[ref]$taskOwnerId)
     Check ($taskHandles.ContainsKey($taskOwnerId)) "${taskStage}: progress window belongs to fixture process tree"
     Check ([CodeWFUninstallFixtureDesktop]::Class($taskForm) -eq 'TUninstallProgressForm') "${taskStage}: native uninstall form"
     [CodeWFUninstallFixtureDesktop]::Capture($taskForm,(Join-Path $taskRun "$taskStage.png"))
     $taskRgn=[CodeWFUninstallFixtureDesktop]::CreateRectRgn(0,0,0,0)
     try{Check ([CodeWFUninstallFixtureDesktop]::GetWindowRgn($taskForm,$taskRgn) -gt 0) "${taskStage}: shaped native window"}finally{[void][CodeWFUninstallFixtureDesktop]::DeleteObject($taskRgn)}
     if($taskStage -like 'real-progress-*'){
      $taskExpected=[Math]::Round(($taskEvidence.native_position-$taskEvidence.native_min)*100.0/($taskEvidence.native_max-$taskEvidence.native_min))
      Check ([Math]::Abs($taskEvidence.percent-$taskExpected) -le 1) "${taskStage}: percentage matches native wrapper within one rounding point"
      $taskRawExpected=[Math]::Round($taskEvidence.current*100.0/$taskEvidence.maximum)
      Check ($taskEvidence.percent -eq $taskRawExpected) "${taskStage}: percentage equals native HWND position and range"
     }
     Check ($taskEvidence.close_enabled -eq $taskEvidence.cancel_enabled) "${taskStage}: close follows native cancellation availability"
     if($taskStage -eq 'initial-dark'){Check ($taskEvidence.theme_mode -eq 0 -and $taskEvidence.dark -eq 1) 'appearance starts dark'}
     if($taskStage -eq 'initial-light'){Check ($taskEvidence.theme_mode -eq 1 -and $taskEvidence.dark -eq 0) 'appearance cycles to light'}
     if($taskStage -eq 'real-progress-light'){Check ($taskEvidence.theme_mode -eq 1 -and $taskEvidence.dark -eq 0) 'native displayed progress cycles to light'}
     if($taskStage -in @('initial-system','system-setting-change','real-progress-system')){
      Check ($taskEvidence.theme_mode -eq 2 -and $taskEvidence.dark -eq $taskEvidence.system_dark) "${taskStage}: follows read-only system appearance"
      Check ($taskEvidence.theme_hint -match 'System|跟随系统') "${taskStage}: dynamic system appearance hint"
     }
    }
    Click-Dialog $taskWindow '^(OK|确定|確定)';$taskLastClosed=[DateTime]::UtcNow
   }elseif($taskContents -match 'completely remove|及其所有组件|完全删除|彻底删除|所有组件|成功|successfully removed|删除完毕|已被删除|已经删除|移除'){
    $taskPositive='^(Yes|是|OK|确定|確定)'
    Click-Dialog $taskWindow $taskPositive;$taskLastClosed=[DateTime]::UtcNow
   }
  }
  if((Test-Path -LiteralPath (Join-Path $taskRun 'evidence/resources-released.txt')) -and $taskWindows.Count -eq 0 -and [CodeWFUninstallFixtureDesktop]::WaitForSingleObject($taskProcessInfo.process,0) -eq 0){break}
  Start-Sleep -Milliseconds 60
 }
 Check ($taskTimer.Elapsed.TotalSeconds -lt 100) 'fixture uninstall completed within bound'
 [uint32]$taskExit=0
 if(-not [CodeWFUninstallFixtureDesktop]::GetExitCodeProcess($taskProcessInfo.process,[ref]$taskExit) -or $taskExit -ne 0){throw 'Fixture uninstaller returned a failure exit code'}
 Check ((Get-Content -LiteralPath (Join-Path $taskRun 'evidence/resources-released.txt') -Raw) -eq 'PASS') 'subclass handles, bitmaps and GDI brush released'
 Check (Test-Path -LiteralPath (Join-Path $taskInstall 'unregistered-user-note.txt')) 'unregistered fixture user file retained'
 Check (-not (Test-Path -LiteralPath (Join-Path $taskInstall 'fixture-owned/fixture-0000.txt'))) 'native uninstall removed its registered fixture files'
 Check ((Get-Content -LiteralPath (Join-Path $taskRun 'evidence/signed-coordinates.txt') -Raw) -eq 'PASS') 'signed LPARAM coordinates retain negative monitor positions'
 if($Silent){Check ((Get-Content -LiteralPath (Join-Path $taskRun 'evidence/silent.txt') -Raw) -eq '0') 'silent uninstall skips branding'}
 elseif($MissingAssets){Check (Test-Path -LiteralPath (Join-Path $taskRun 'evidence/fallback.txt')) 'missing branding asset falls back to native uninstall'}
 else{
  foreach($taskRequired in @('initial-dark','initial-light','initial-system','system-setting-change','after-theme-cycles','real-progress-first','real-progress-middle','real-progress-light','real-progress-system','real-progress-complete')){Check ($taskSeen.Contains($taskRequired)) "captured $taskRequired"}
  $taskBefore=Get-Content -LiteralPath (Join-Path $taskRun 'evidence/initial-light.json') -Raw | ConvertFrom-Json
  $taskAfter=Get-Content -LiteralPath (Join-Path $taskRun 'evidence/after-theme-cycles.json') -Raw | ConvertFrom-Json
  Check ($taskAfter.gdi -le $taskBefore.gdi+3) '60 theme changes do not accumulate GDI handles'
 }
 [pscustomobject]@{language=$Language;silent=[bool]$Silent;missing_assets=[bool]$MissingAssets;assertions=$taskAssertions;run=$taskRun;include_sha256=$taskBuild.include_sha256;fixture_sha256=$taskBuild.binary_sha256;observer_sha256=$taskBuild.observer_sha256;generator_sha256=$taskBuild.generator_sha256;runner_sha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash.ToLowerInvariant();desktop_sha256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.Fixture.Desktop.cs')).Hash.ToLowerInvariant();uninstall_exit_code=$taskExit;scope='Native PBM observations from isolated dummy-file fixture; no real product uninstall verified'} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $taskRun 'receipt.json') -Encoding utf8
 Write-Output "RESULT: $taskRun"
}finally{
 if($taskProcessInfo.processId -ne 0){Register-Owned}
 foreach($taskEntry in $taskHandles.GetEnumerator()){[void][CodeWFUninstallFixtureDesktop]::TerminateProcess($taskEntry.Value,0);[void][CodeWFUninstallFixtureDesktop]::CloseHandle($taskEntry.Value)}
 if($taskProcessInfo.thread -ne [IntPtr]::Zero){[void][CodeWFUninstallFixtureDesktop]::CloseHandle($taskProcessInfo.thread)}
 if($taskDesktop -ne [IntPtr]::Zero){[void][CodeWFUninstallFixtureDesktop]::CloseDesktop($taskDesktop)}
 if($taskSetup){$taskSetup.Dispose()}
 if($taskHasMutex){$taskMutex.ReleaseMutex()}
 $taskMutex.Dispose()
}
