@echo off
setlocal

set "ROOT=%~dp0"
set "SOLUTION=%ROOT%CodeWF.Avalonia.slnx"
set "CONFIGURATION=%~1"
if "%CONFIGURATION%"=="" set "CONFIGURATION=Release"

set "PACKAGE_DIR=%ROOT%artifacts\packages"

set "PROJECTS=%ROOT%src\MindView\CodeWF.Avalonia.MindView\CodeWF.Avalonia.MindView.csproj;%ROOT%src\Controls\CodeWF.Avalonia.Controls\CodeWF.Avalonia.Controls.csproj;%ROOT%src\DataGrid\CodeWF.Avalonia.DataGrid\CodeWF.Avalonia.DataGrid.csproj;%ROOT%src\TreeDataGrid\CodeWF.Avalonia.TreeDataGrid\CodeWF.Avalonia.TreeDataGrid.csproj;%ROOT%src\Dock\CodeWF.Avalonia.Dock\CodeWF.Avalonia.Dock.csproj;%ROOT%src\Markdown\CodeWF.Avalonia.Markdown\CodeWF.Avalonia.Markdown.csproj;%ROOT%src\Markdown\CodeWF.Avalonia.Markdown.Lite\CodeWF.Avalonia.Markdown.Lite.csproj;%ROOT%src\Log\CodeWF.Avalonia.Log\CodeWF.Avalonia.Log.csproj;%ROOT%src\Lang\CodeWF.Avalonia.Lang\CodeWF.Avalonia.Lang.csproj;%ROOT%src\Lang\CodeWF.Avalonia.Lang.Generator\CodeWF.Avalonia.Lang.Generator.csproj"

echo [CodeWF.Avalonia] Restore solution...
dotnet restore "%SOLUTION%"
if errorlevel 1 goto :failed

if not exist "%PACKAGE_DIR%" mkdir "%PACKAGE_DIR%"
del /q "%PACKAGE_DIR%\*.nupkg" 2>nul
del /q "%PACKAGE_DIR%\*.snupkg" 2>nul

echo [CodeWF.Avalonia] Build %CONFIGURATION% solution...
dotnet build "%SOLUTION%" -c "%CONFIGURATION%" --no-restore -nologo
if errorlevel 1 goto :failed

echo [CodeWF.Avalonia] Pack NuGet packages...
for %%P in ("%PROJECTS:;=" "%") do (
    dotnet pack %%P -c "%CONFIGURATION%" --no-build --no-restore -nologo -o "%PACKAGE_DIR%"
    if errorlevel 1 goto :failed
)

for /r "%PACKAGE_DIR%" %%F in (*.pdb) do del /q "%%F" 2>nul

if not exist "%PACKAGE_DIR%\*.nupkg" goto :failed

echo.
echo [CodeWF.Avalonia] Packages:
dir /b "%PACKAGE_DIR%\*.nupkg"
echo.
echo [CodeWF.Avalonia] Done. Output: %PACKAGE_DIR%
exit /b 0

:failed
echo.
echo [CodeWF.Avalonia] Pack failed.
exit /b 1
