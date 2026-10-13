; CodeWF.Avalonia Demo Windows installer.
; Build from the repository root with Inno Setup 6 and pass /DAppVersion=x.y.z.

#ifndef AppVersion
#define AppVersion "0.0.0"
#endif

#ifndef SourceDir
#define SourceDir "..\artifacts\publish\win-x64\CodeWF.Avalonia.Demo"
#endif

#ifndef OutputDir
#define OutputDir "..\artifacts\release"
#endif

[Setup]
AppId={{7C4E9A21-6B3D-4F8E-9A57-2D1C8B6F4E30}
AppName=CodeWF.Avalonia Demo
AppVersion={#AppVersion}
AppPublisher=Dotnet9
AppPublisherURL=https://github.com/dotnet9/CodeWF.Avalonia
AppSupportURL=https://github.com/dotnet9/CodeWF.Avalonia/issues
DefaultDirName={autopf}\CodeWF.Avalonia Demo
DefaultGroupName=CodeWF.Avalonia Demo
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=CodeWF.Avalonia.Demo-v{#AppVersion}-win-x64-setup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; per-user 安装：演示应用以普通权限运行，日志写入用户临时目录。
PrivilegesRequired=lowest
ChangesAssociations=no
CloseApplications=yes
RestartApplications=yes
CloseApplicationsFilter=CodeWF.Avalonia.Demo.exe
UninstallDisplayIcon={app}\CodeWF.Avalonia.Demo.exe
WizardStyle=modern

[Languages]
Name: "chinesesimplified"; MessagesFile: "Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\CodeWF.Avalonia Demo"; Filename: "{app}\CodeWF.Avalonia.Demo.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\CodeWF.Avalonia Demo"; Filename: "{app}\CodeWF.Avalonia.Demo.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\CodeWF.Avalonia.Demo.exe"; Description: "Launch CodeWF.Avalonia Demo"; Flags: nowait postinstall skipifsilent
