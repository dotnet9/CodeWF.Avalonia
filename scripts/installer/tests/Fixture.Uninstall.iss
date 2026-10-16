// 仅用于专用临时夹具：观察其原生 PBM，不模拟或执行真实产品卸载。
[Code]
var FixtureStage: Integer; FixtureGaugeProc: LongWord;
function FixtureGetCurrentProcess: LongWord;
  external 'GetCurrentProcess@kernel32.dll stdcall';
function FixtureGetGuiResources(Process, Flags: LongWord): LongWord;
  external 'GetGuiResources@user32.dll stdcall';
procedure FixtureWrite(const Stage: String; Current, Maximum: Integer);
var Data, Root: String;
begin
  Root := ExpandConstant('{app}\..\evidence\');
  ForceDirectories(Root);
  Data := '{"stage":"' + Stage + '","current":' + IntToStr(Current) + ',"maximum":' + IntToStr(Maximum) +
    ',"percent":' + IntToStr(CWUProgress) + ',"form":' + IntToStr(UninstallProgressForm.Handle) +
    ',"gauge":' + IntToStr(UninstallProgressForm.ProgressBar.Handle) +
    ',"native_min":' + IntToStr(UninstallProgressForm.ProgressBar.Min) +
    ',"native_max":' + IntToStr(UninstallProgressForm.ProgressBar.Max) +
    ',"native_position":' + IntToStr(UninstallProgressForm.ProgressBar.Position) +
    ',"gdi":' + IntToStr(FixtureGetGuiResources(FixtureGetCurrentProcess, 0)) +
    ',"status_color":' + IntToStr(UninstallProgressForm.StatusLabel.Font.Color) +
    ',"surface_color":' + IntToStr(UninstallProgressForm.InstallingPage.Color) +
    ',"cancel_enabled":' + IntToStr(Ord(UninstallProgressForm.CancelButton.Enabled)) +
    ',"close_enabled":' + IntToStr(Ord(CWUCloseButton.Enabled)) + '}';
  Data := Copy(Data, 1, Length(Data) - 1) + ',"theme_mode":' + IntToStr(CWUThemeMode) +
    ',"dark":' + IntToStr(Ord(CWUDark)) + ',"system_dark":' + IntToStr(Ord(CWUSystemDark)) +
    ',"theme_hint":"' + CWUThemeButton.Hint + '"}';
  SaveStringToFile(Root + Stage + '.json', UTF8Encode(Data), False);
end;
procedure FixtureHold(const Stage: String; Current, Maximum: Integer);
begin
  FixtureWrite(Stage, Current, Maximum);
  MsgBox('CodeWF isolated uninstall fixture HOLD ' + Stage, mbInformation, MB_OK);
end;
function FixtureObserveGauge(Window, Message, WParam, LParam: LongWord): Longint;
var Current, Maximum: Integer;
begin
  Result := CWUCallWindowProc(FixtureGaugeProc, Window, Message, WParam, LParam);
  if not CWUInitialized then exit;
  if Message <> $0402 then exit;
  Current := CWUSendMessage(Window, $0408, 0, 0);
  Maximum := CWUSendMessage(Window, $0407, 0, 0);
  if (FixtureStage = 0) and (CWUProgress > 0) then begin
    FixtureStage := 1; FixtureHold('real-progress-first', Current, Maximum);
  end;
  if (FixtureStage = 1) and (CWUProgress >= 50) then begin
    FixtureStage := 2; FixtureHold('real-progress-middle', Current, Maximum);
    // 原生窗完成 Show 后再捕获浅色及系统态，覆盖真实按钮绘制。
    CWUThemeClick(nil); FixtureHold('real-progress-light', Current, Maximum);
    CWUThemeClick(nil); FixtureHold('real-progress-system', Current, Maximum);
    CWUThemeClick(nil);
  end;
  if (FixtureStage = 2) and (CWUProgress = 100) then begin
    FixtureStage := 3; FixtureHold('real-progress-complete', Current, Maximum);
  end;
end;
<event('InitializeUninstallProgressForm')>
procedure FixtureInitializeUninstall;
var I: Integer; Root: String;
begin
  Root := ExpandConstant('{app}\..\evidence\'); ForceDirectories(Root);
  if (CWUSignedWord(0) <> 0) or (CWUSignedWord($7FFF) <> 32767) or
    (CWUSignedWord($8000) <> -32768) or (CWUSignedWord($FFFF) <> -1) then
    RaiseException('Signed LPARAM coordinate conversion failed.');
  SaveStringToFile(Root + 'signed-coordinates.txt', 'PASS', False);
  if UninstallSilent then begin
    SaveStringToFile(Root + 'silent.txt', IntToStr(Ord(CWUInitialized)), False); exit;
  end;
  if not CWUInitialized then begin
    SaveStringToFile(Root + 'fallback.txt', 'native-window', False); exit;
  end;
  CWUShowWindow(UninstallProgressForm.Handle, SW_SHOWNORMAL);
  // 预热主题后观察 GDI 稳定性，不改变卸载进度。
  CWUThemeClick(nil); CWUThemeClick(nil); CWUThemeClick(nil);
  FixtureHold('initial-dark', 0, 0);
  CWUThemeClick(nil); FixtureHold('initial-light', 0, 0);
  CWUThemeClick(nil); FixtureHold('initial-system', 0, 0);
  CWUSendMessage(UninstallProgressForm.Handle, $001A, 0, 0);
  FixtureHold('system-setting-change', 0, 0);
  CWUThemeClick(nil);
  for I := 1 to 20 do begin CWUThemeClick(nil); CWUThemeClick(nil); CWUThemeClick(nil); end;
  FixtureHold('after-theme-cycles', 0, 0);
  FixtureGaugeProc := CWUSetWindowLong(UninstallProgressForm.ProgressBar.Handle, -4, CreateCallback(@FixtureObserveGauge));
end;
<event('DeinitializeUninstall')>
procedure FixtureObserveDeinitialize;
var Root: String;
begin
  Root := ExpandConstant('{app}\..\evidence\'); ForceDirectories(Root);
  if not CWUInitialized and (CWUBackgroundBrush = 0) and (CWUFormProc = 0) and
    (CWUGaugeProc = 0) and (CWUPageProc = 0) and (CWUFrames[0] = nil) and
    (CWUFrames[1] = nil) and (CWUReactors[0] = nil) and (CWUReactors[1] = nil) and
    (CWUCores[0] = nil) and (CWUCores[1] = nil) then
    SaveStringToFile(Root + 'resources-released.txt', 'PASS', False)
  else SaveStringToFile(Root + 'resources-released.txt', 'FAIL', False);
end;
