@echo off
REM Fluent Software Manager - Clean Script (Batch)

echo =====================================
echo Fluent Software Manager - Clean
echo =====================================
echo.

set PROJECT_PATH=FluentSoftwareManager

if exist "%PROJECT_PATH%\bin" (
    echo Removing: %PROJECT_PATH%\bin
    rmdir /s /q "%PROJECT_PATH%\bin" 2>nul
    echo   [OK] Removed
) else (
    echo Skipping: %PROJECT_PATH%\bin (not found^)
)

if exist "%PROJECT_PATH%\obj" (
    echo Removing: %PROJECT_PATH%\obj
    rmdir /s /q "%PROJECT_PATH%\obj" 2>nul
    echo   [OK] Removed
) else (
    echo Skipping: %PROJECT_PATH%\obj (not found^)
)

if exist "publish" (
    echo Removing: publish
    rmdir /s /q "publish" 2>nul
    echo   [OK] Removed
) else (
    echo Skipping: publish (not found^)
)

echo.
echo =====================================
echo Clean completed!
echo =====================================
pause
