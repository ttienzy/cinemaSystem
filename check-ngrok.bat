@echo off
echo Checking ngrok status...
echo.

tasklist | findstr /i "ngrok.exe" >nul
if %errorlevel% equ 0 (
    echo [OK] Ngrok is RUNNING
    echo.
    echo Opening ngrok dashboard...
    start http://localhost:4040
) else (
    echo [ERROR] Ngrok is NOT running
    echo.
    echo To start ngrok, run:
    echo   ngrok http 5003 --host-header=localhost:5003
)
echo.
pause
