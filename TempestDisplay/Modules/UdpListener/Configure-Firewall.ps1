# ================================================
# TempestDisplay Firewall Configuration Script
# ================================================
# This script must be run as Administrator
#
# What it does:
# - Creates Windows Firewall inbound rule
# - Allows UDP port 50222 (WeatherFlow hub broadcasts)
# - Applies to Private and Domain networks only
# ================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TempestDisplay Firewall Configuration" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Right-click this file and select 'Run with PowerShell as Administrator'" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or from an elevated PowerShell window, run:" -ForegroundColor Yellow
    Write-Host "  .\Configure-Firewall.ps1" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "This will create a firewall rule to allow"
Write-Host "TempestDisplay to receive weather data from"
Write-Host "your WeatherFlow hub on UDP port 50222."
Write-Host ""
Write-Host "Rule details:" -ForegroundColor Yellow
Write-Host "  Name: TempestDisplay UDP Listener"
Write-Host "  Direction: Inbound"
Write-Host "  Protocol: UDP"
Write-Host "  Port: 50222"
Write-Host "  Profiles: Private, Domain"
Write-Host ""
$continue = Read-Host "Continue? (Y/N)"

if ($continue -ne "Y" -and $continue -ne "y") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Creating firewall rule..." -ForegroundColor Cyan
Write-Host ""

try {
    # Check if rule already exists
    $existingRule = Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" -ErrorAction SilentlyContinue
    
    if ($existingRule) {
        Write-Host "Rule already exists! Removing old rule..." -ForegroundColor Yellow
        Remove-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
    }
    
    # Create new rule
    New-NetFirewallRule `
        -DisplayName "TempestDisplay UDP Listener" `
        -Description "Allows TempestDisplay to receive weather data from WeatherFlow hub on UDP port 50222" `
        -Direction Inbound `
        -Protocol UDP `
        -LocalPort 50222 `
        -Action Allow `
        -Profile Private,Domain `
        -Enabled True | Out-Null
    
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "SUCCESS! Firewall rule created successfully." -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Rule created:" -ForegroundColor Cyan
    Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" | Format-List DisplayName, Enabled, Direction, Action
    
    Write-Host ""
    Write-Host "Port filter:" -ForegroundColor Cyan
    Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" | Get-NetFirewallPortFilter | Format-List Protocol, LocalPort
    
    Write-Host ""
    Write-Host "You can now start TempestDisplay and it will" -ForegroundColor Green
    Write-Host "receive weather data from your WeatherFlow hub." -ForegroundColor Green
    Write-Host ""
    Write-Host "To verify in GUI:" -ForegroundColor Yellow
    Write-Host "  1. Press Windows + R"
    Write-Host "  2. Type: wf.msc"
    Write-Host "  3. Click Inbound Rules"
    Write-Host "  4. Look for 'TempestDisplay UDP Listener'"
    Write-Host ""
    
} catch {
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "ERROR! Failed to create firewall rule." -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error details:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible causes:" -ForegroundColor Yellow
    Write-Host "  - Firewall service is disabled"
    Write-Host "  - Insufficient permissions"
    Write-Host "  - Third-party firewall interference"
    Write-Host ""
}

Write-Host ""
Read-Host "Press Enter to exit"
