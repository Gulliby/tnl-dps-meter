@echo off
echo ============================================
echo TNL DPS Meter - Full Rebuild and Run
echo ============================================

echo [1/6] Killing existing TNL_DPS_Meter processes...
taskkill /f /im "TNL_DPS_Meter.exe" 2>nul
echo Done.

echo [2/6] Ensuring we are in the project directory...
cd "D:\tnl-dps-meter\TNL_DPS_Meter"
if errorlevel 1 (
    echo ERROR: Cannot navigate to TNL_DPS_Meter directory
    pause
    exit /b 1
)

echo [3/6] Cleaning previous builds...
dotnet clean
if errorlevel 1 (
    echo ERROR: dotnet clean failed
    pause
    exit /b 1
)

echo [4/6] Building project...
dotnet build
if errorlevel 1 (
    echo ERROR: dotnet build failed
    pause
    exit /b 1
)

echo [5/6] Publishing project...
dotnet publish -c Release
if errorlevel 1 (
    echo ERROR: dotnet publish failed
    pause
    exit /b 1
)

echo [6/6] Starting TNL DPS Meter...
start "" "bin\Release\net7.0-windows\win-x64\publish\TNL_DPS_Meter.exe"

echo ============================================
echo TNL DPS Meter started successfully!
echo Check the taskbar for the application icon.
echo ============================================

pause
