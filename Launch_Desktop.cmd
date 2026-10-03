@echo off
setlocal
cd /d "%~dp0"
if not exist "Builds\WindowsDesktop\ChronoPivot_ReportVR.exe" (
  echo Windows build missing. Use Unity menu: Report VR / Build Windows x64.
  pause
  exit /b 1
)
start "" "%~dp0Builds\WindowsDesktop\ChronoPivot_ReportVR.exe" -desktop -screen-fullscreen 0
