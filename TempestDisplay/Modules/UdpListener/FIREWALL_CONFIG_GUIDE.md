# Windows Firewall Configuration for UDP Port 50222
# Step-by-Step Guide for TempestDisplay

## Method 1: PowerShell (Fastest - Run as Administrator)

### Open PowerShell as Administrator
1. Press Windows key
2. Type "PowerShell"
3. Right-click "Windows PowerShell"
4. Select "Run as administrator"

### Run This Command:
```powershell
New-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" -Direction Inbound -Protocol UDP -LocalPort 50222 -Action Allow -Profile Private,Domain
```

### Verify the Rule Was Created:
```powershell
Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
```

### To Remove the Rule Later (if needed):
```powershell
Remove-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
```

---

## Method 2: Windows Firewall GUI (Step-by-Step with Screenshots Guide)

### Step 1: Open Windows Firewall with Advanced Security
**Option A - Via Control Panel:**
1. Press `Windows + R`
2. Type: `wf.msc`
3. Press Enter

**Option B - Via Settings:**
1. Press `Windows + I` (Settings)
2. Click "Privacy & Security" (Windows 11) or "Update & Security" (Windows 10)
3. Click "Windows Security"
4. Click "Firewall & network protection"
5. Click "Advanced settings" at the bottom

### Step 2: Create Inbound Rule
1. In the left pane, click **"Inbound Rules"**
2. In the right pane, click **"New Rule..."**

### Step 3: Rule Type
1. Select **"Port"**
2. Click **Next**

### Step 4: Protocol and Ports
1. Select **"UDP"**
2. Select **"Specific local ports"**
3. Enter: **50222**
4. Click **Next**

### Step 5: Action
1. Select **"Allow the connection"**
2. Click **Next**

### Step 6: Profile (When does this rule apply?)
Select the networks where TempestDisplay will run:
- ☑ **Domain** (if on corporate network)
- ☑ **Private** (recommended - home/work networks)
- ☐ **Public** (not recommended for security)

**Recommendation:** Check "Private" and "Domain" only
Click **Next**

### Step 7: Name
1. **Name:** TempestDisplay UDP Listener
2. **Description:** Allows TempestDisplay to receive weather data from WeatherFlow hub on UDP port 50222
3. Click **Finish**

### Step 8: Verify
1. In the Inbound Rules list, find "TempestDisplay UDP Listener"
2. Ensure "Enabled" column shows "Yes"
3. Double-click the rule to view/edit settings if needed

---

## Method 3: Command Prompt (Alternative to PowerShell)

### Open Command Prompt as Administrator
1. Press Windows key
2. Type "cmd"
3. Right-click "Command Prompt"
4. Select "Run as administrator"

### Run This Command:
```cmd
netsh advfirewall firewall add rule name="TempestDisplay UDP Listener" dir=in action=allow protocol=UDP localport=50222 profile=private,domain
```

### Verify the Rule:
```cmd
netsh advfirewall firewall show rule name="TempestDisplay UDP Listener"
```

### To Remove the Rule Later:
```cmd
netsh advfirewall firewall delete rule name="TempestDisplay UDP Listener"
```

---

## Verification Steps

### 1. Check if Port is Listening
After starting TempestDisplay, open Command Prompt and run:
```cmd
netstat -an | findstr :50222
```

You should see:
```
UDP    0.0.0.0:50222          *:*
```

This confirms your app is listening on port 50222.

### 2. Test with Firewall Temporarily Disabled
**For troubleshooting only:**
1. Open Windows Security
2. Go to Firewall & network protection
3. Click your active network (Private/Domain/Public)
4. Turn OFF "Windows Defender Firewall"
5. Check if TempestDisplay receives data
6. Turn firewall back ON immediately
7. If it worked with firewall off, the firewall rule needs adjustment

### 3. Check Windows Event Log
1. Press `Windows + X`
2. Select "Event Viewer"
3. Navigate to: Windows Logs → Security
4. Look for firewall blocking events
5. Filter by Event ID 5157 (blocked connection)

---

## Troubleshooting

### Rule Created But Still Not Working?

**Check 1: Is the rule enabled?**
```powershell
Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" | Select-Object DisplayName, Enabled
```

**Check 2: Enable if disabled:**
```powershell
Enable-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
```

**Check 3: Verify rule details:**
```powershell
Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" | Get-NetFirewallPortFilter
```

Should show:
- Protocol: UDP
- LocalPort: 50222

**Check 4: Check if another program is using port 50222:**
```cmd
netstat -ano | findstr :50222
```

If you see a different PID (Process ID), another program is using that port.

### Third-Party Firewall Software

If you have third-party firewall/antivirus (Norton, McAfee, Bitdefender, etc.):
1. The Windows Firewall rule may not be enough
2. You'll need to configure the third-party firewall too
3. Look for "Application Control" or "Firewall Rules" in your security software
4. Add exception for TempestDisplay.exe
5. Allow UDP port 50222 inbound

### Network Security Software

Some corporate environments have additional network security:
- **Check with IT department** if on corporate network
- VLANs may block UDP broadcasts
- Some routers filter UDP broadcasts

---

## Security Considerations

### Why Private/Domain Only?
- **Private Networks**: Home, work, trusted networks
- **Domain Networks**: Corporate/enterprise networks
- **Public Networks**: Coffee shops, airports, hotels

**Recommendation:** Only allow on Private/Domain networks to prevent security risks on public WiFi.

### Port 50222 Security
- Port 50222 is WeatherFlow's standard UDP broadcast port
- Your app only listens (receives), doesn't send responses
- UDP broadcasts only work on local network (can't be accessed from internet)
- Still, limit to Private/Domain networks as good practice

---

## Quick Reference Commands

### PowerShell (Run as Admin)
```powershell
# Create rule
New-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" -Direction Inbound -Protocol UDP -LocalPort 50222 -Action Allow -Profile Private,Domain

# Check rule exists
Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"

# Enable rule
Enable-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"

# Disable rule
Disable-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"

# Remove rule
Remove-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
```

### Command Prompt (Run as Admin)
```cmd
REM Create rule
netsh advfirewall firewall add rule name="TempestDisplay UDP Listener" dir=in action=allow protocol=UDP localport=50222 profile=private,domain

REM Show rule
netsh advfirewall firewall show rule name="TempestDisplay UDP Listener"

REM Delete rule
netsh advfirewall firewall delete rule name="TempestDisplay UDP Listener"
```

### Check if Port is Listening
```cmd
netstat -an | findstr :50222
```

---

## Alternative: Application-Based Rule

Instead of port-based rule, you can create an application rule:

### PowerShell
```powershell
New-NetFirewallRule -DisplayName "TempestDisplay App" -Direction Inbound -Program "C:\VB18\TempestDisplay\TempestDisplay\bin\Debug\TempestDisplay.exe" -Action Allow -Profile Private,Domain
```

### GUI Method
1. In Firewall Advanced Security → Inbound Rules → New Rule
2. Select **"Program"** instead of "Port"
3. Browse to your TempestDisplay.exe
4. Allow the connection
5. Select Private/Domain profiles
6. Name it "TempestDisplay App"

**Advantage:** Only TempestDisplay.exe can use the port, more secure
**Disadvantage:** Need to update rule if exe location changes

---

## Batch Script for Easy Setup

Save this as `Configure-Firewall.bat` and run as Administrator:

```batch
@echo off
echo ================================================
echo TempestDisplay Firewall Configuration
echo ================================================
echo.
echo This will create a firewall rule to allow
echo TempestDisplay to receive UDP broadcasts on port 50222
echo.
pause

echo Creating firewall rule...
netsh advfirewall firewall add rule name="TempestDisplay UDP Listener" dir=in action=allow protocol=UDP localport=50222 profile=private,domain

if %errorlevel% equ 0 (
    echo.
    echo SUCCESS! Firewall rule created successfully.
    echo.
    echo Rule Name: TempestDisplay UDP Listener
    echo Direction: Inbound
    echo Protocol: UDP
    echo Port: 50222
    echo Profiles: Private, Domain
) else (
    echo.
    echo ERROR! Failed to create firewall rule.
    echo Make sure you ran this as Administrator.
)

echo.
pause
```

---

## Summary

**Easiest Method:** PowerShell command (copy/paste, run as admin)
**Most Visible Method:** GUI (good for learning/verification)
**For IT/Deployment:** Batch script or PowerShell script

After configuration, start TempestDisplay and check the log file for:
```
UDP listener started successfully on port 50222
```

If you see this message and firewall rule is configured, you should start receiving weather data from your WeatherFlow hub!
