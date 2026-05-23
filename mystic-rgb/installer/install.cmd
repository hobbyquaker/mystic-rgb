@echo off
setlocal

net session >nul 2>&1
if not %errorlevel%==0 (
  powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

set "TARGET=%ProgramFiles%\mystic-rgb"
if not exist "%TARGET%" mkdir "%TARGET%"

copy /Y "%~dp0mystic-rgb.exe" "%TARGET%\mystic-rgb.exe" >nul
copy /Y "%~dp0MysticLight_SDK.dll" "%TARGET%\MysticLight_SDK.dll" >nul
copy /Y "%~dp0README.md" "%TARGET%\README.md" >nul
copy /Y "%~dp0LICENSE" "%TARGET%\LICENSE" >nul

"%TARGET%\mystic-rgb.exe" --setup

echo.
echo Installation abgeschlossen: %TARGET%
echo Du kannst das Tool jetzt ueber diesen Pfad starten.
pause
