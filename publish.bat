@echo off
setlocal

set "ROOT=%~dp0"
set "PROJECT=%ROOT%demo\CodeWF.Avalonia.Demo\CodeWF.Avalonia.Demo.csproj"
set "PACKAGE_AFTER=false"

rem NativeAOT ILC may fail-fast on machines with >= 32 logical cores; limit to 4.
set "DOTNET_PROCESSOR_COUNT=4"
rem Disable network NuGet audit which may hang restore behind a proxy.
set "NUGET_AUDIT=false"

if not "%~1"=="" (
    if /I "%~1"=="--package" (
        set "PACKAGE_AFTER=true"
    ) else (
        echo Usage: publish.bat [--package]
        exit /b 2
    )
)

echo Publishing CodeWF.Avalonia.Demo profiles...

rem Target framework is declared by each PublishProfile (win-* uses net10.0-windows,
rem others use net10.0). Do NOT pass -f here: the global TargetFramework propagates
rem into single-TFM referenced projects and breaks their restore (NETSDK1005).
call :publish FolderProfile__win-x64 || exit /b 1
call :publish FolderProfile__win-x86 || exit /b 1
call :publish FolderProfile__linux-x64 || exit /b 1
call :publish FolderProfile__linux-arm64 || exit /b 1
call :publish FolderProfile__osx-x64 || exit /b 1
call :publish FolderProfile__osx-arm64 || exit /b 1

echo All CodeWF.Avalonia.Demo publish profiles completed.

if /I "%PACKAGE_AFTER%"=="true" (
    echo.
    echo Packaging release artifacts...
    powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\package_demo_artifacts.ps1" || exit /b 1
)

exit /b 0

:publish
echo.
echo === %~1 ===
dotnet publish "%PROJECT%" -c Release -p:PublishProfile=%~1 -p:NuGetAudit=%NUGET_AUDIT%
exit /b %ERRORLEVEL%
