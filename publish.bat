@echo off
setlocal

set "ROOT=%~dp0"
set "PROJECT=%ROOT%demo\CodeWF.Avalonia.Demo\CodeWF.Avalonia.Demo.csproj"
set "PACKAGE_AFTER=false"

rem 本机限制编译并发，其他平台在对应运行器执行发布脚本。
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

pwsh -NoLogo -NoProfile -File "%ROOT%scripts\publish_avalonia_demo.ps1" -RuntimeIdentifier win-x64
if errorlevel 1 exit /b %ERRORLEVEL%

if /I "%PACKAGE_AFTER%"=="true" (
    echo.
    echo Packaging release artifacts...
    pwsh -NoLogo -NoProfile -File "%ROOT%scripts\package_windows_installer.ps1" || exit /b 1
)

exit /b 0
