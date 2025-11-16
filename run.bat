@echo off
REM Fluent Software Manager - Run Script (Batch)
REM For users who prefer CMD over PowerShell

echo =====================================
echo Fluent Software Manager - Run
echo =====================================
echo.

REM Check for administrator privileges
net session >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Warning: This application requires administrator privileges.
    echo Please run this script as administrator.
    echo.
    pause
    exit /b 1
)

REM Set default configuration
set CONFIG=Debug
set PLATFORM=x64

REM Parse arguments
if not "%1"=="" set CONFIG=%1
if not "%2"=="" set PLATFORM=%2

echo Building project...
call build.bat %CONFIG% %PLATFORM%
if %ERRORLEVEL% neq 0 exit /b 1

echo.
echo Starting application...
echo.

dotnet run --project FluentSoftwareManager\FluentSoftwareManager.csproj -c %CONFIG% -p:Platform=%PLATFORM% --no-build

pause
