@echo off
REM ================================================
REM TempestDisplay Firewall Configuration Script
REM ================================================
REM This script must be run as Administrator
REM
REM What it does:
REM - Creates Windows Firewall inbound rule
REM - Allows UDP port 50222 (WeatherFlow hub broadcasts)
REM - Applies to Private and Domain networks only
REM ================================================

echo.
echo ================================================
echo TempestDisplay Firewall Configuration
echo ================================================
echo.

REM Check if running as Administrator
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo.
    echo Right-click this file and select "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo This will create a firewall rule to allow
echo TempestDisplay to receive weather data from
echo your WeatherFlow hub on UDP port 50222.
echo.
echo Rule details:
echo   Name: TempestDisplay UDP Listener
echo   Direction: Inbound
echo   Protocol: UDP
echo   Port: 50222
echo   Profiles: Private, Domain
echo.
pause

echo.
echo Creating firewall rule...
echo.

netsh advfirewall firewall add rule name="TempestDisplay UDP Listener" dir=in action=allow protocol=UDP localport=50222 profile=private,domain

if %errorlevel% equ 0 (
    echo.
    echo ================================================
    echo SUCCESS! Firewall rule created successfully.
    echo ================================================
    echo.
    echo You can now start TempestDisplay and it will
    echo receive weather data from your WeatherFlow hub.
    echo.
    echo To verify the rule, open Windows Firewall and
    echo look for "TempestDisplay UDP Listener" in
    echo Inbound Rules.
    echo.
) else (
    echo.
    echo ================================================
    echo ERROR! Failed to create firewall rule.
    echo ================================================
    echo.
    echo Possible causes:
    echo   - Not running as Administrator
    echo   - Rule already exists
    echo   - Firewall service is disabled
    echo.
    echo Try running this script again as Administrator.
    echo.
)

echo Press any key to exit...
pause >nul
