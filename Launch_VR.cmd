@echo off
setlocal
cd /d "%~dp0"
if not exist "Builds\Windows\ChronoPivot_ReportVR.exe" (
  echo Windows build missing. Use Unity menu: Report VR / Build Windows x64.
  pause
  exit /b 1
)
echo Connect the headset in SteamVR and set SteamVR as the OpenXR runtime first.
start "" "%~dp0Builds\Windows\ChronoPivot_ReportVR.exe" -screen-fullscreen 0
