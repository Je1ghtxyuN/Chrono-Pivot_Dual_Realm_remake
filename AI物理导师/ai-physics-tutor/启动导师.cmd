@echo off
chcp 65001 >nul
pushd "%~dp0"
if not exist ".venv\Scripts\python.exe" (
  echo 未找到导师独立环境，请查看队友部署说明.md。
  pause
  exit /b 1
)
set HF_HUB_OFFLINE=1
set TRANSFORMERS_OFFLINE=1
".venv\Scripts\python.exe" -X utf8 prototype_delivery\local_server.py --config prototype_delivery\config.json --port 18080
popd
pause
