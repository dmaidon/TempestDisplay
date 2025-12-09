# Windows Firewall Configuration - Quick Reference

## ⚡ FASTEST METHOD (Copy & Paste)

### PowerShell (Run as Administrator)
```powershell
New-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" -Direction Inbound -Protocol UDP -LocalPort 50222 -Action Allow -Profile Private,Domain
```

---

## 🚀 AUTOMATED SCRIPTS

### Option 1: Run Configure-Firewall.bat
1. Right-click `Configure-Firewall.bat`
2. Select "Run as administrator"
3. Press any key to continue
4. Done!

### Option 2: Run Configure-Firewall.ps1
1. Right-click `Configure-Firewall.ps1`
2. Select "Run with PowerShell as Administrator"
3. Type "Y" and press Enter
4. Done!

---

## 🖱️ GUI METHOD (Step-by-Step)

1. Press `Windows + R`, type `wf.msc`, press Enter
2. Click "Inbound Rules" → "New Rule..."
3. Select "Port" → Next
4. Select "UDP", enter port `50222` → Next
5. Select "Allow the connection" → Next
6. Check "Private" and "Domain" → Next
7. Name: `TempestDisplay UDP Listener` → Finish

---

## ✅ VERIFY IT WORKS

### Check if rule exists:
```powershell
Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
```

### Check if port is listening:
```cmd
netstat -an | findstr :50222
```
Should show: `UDP    0.0.0.0:50222          *:*`

---

## 🗑️ REMOVE RULE (If Needed)

### PowerShell:
```powershell
Remove-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
```

### Command Prompt:
```cmd
netsh advfirewall firewall delete rule name="TempestDisplay UDP Listener"
```

---

## 🔧 TROUBLESHOOTING

### Not receiving data?

1. **Check firewall rule exists:**
   ```powershell
   Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
   ```

2. **Check if rule is enabled:**
   ```powershell
   Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" | Select-Object Enabled
   ```

3. **Enable if disabled:**
   ```powershell
   Enable-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"
   ```

4. **Test with firewall temporarily off:**
   - Windows Security → Firewall → Turn off (for Private network)
   - Test TempestDisplay
   - Turn firewall back on
   - If it worked, firewall rule needs fixing

5. **Check another app isn't using port 50222:**
   ```cmd
   netstat -ano | findstr :50222
   ```

6. **Check TempestDisplay log file for:**
   - "UDP listener started successfully on port 50222" ✅
   - "Error starting UDP listener" ❌

---

## 📋 COMMAND REFERENCE

| Task | PowerShell Command |
|------|-------------------|
| Create Rule | `New-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" -Direction Inbound -Protocol UDP -LocalPort 50222 -Action Allow -Profile Private,Domain` |
| Check Rule | `Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"` |
| Enable Rule | `Enable-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"` |
| Disable Rule | `Disable-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"` |
| Remove Rule | `Remove-NetFirewallRule -DisplayName "TempestDisplay UDP Listener"` |
| Check Port | `Get-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" \| Get-NetFirewallPortFilter` |

---

## ⚠️ IMPORTANT NOTES

- **Administrator required** for all firewall changes
- **Private/Domain only** for security (not Public networks)
- **UDP port 50222** is WeatherFlow's standard broadcast port
- **Local network only** - broadcasts don't go through internet
- **Third-party firewalls** (Norton, McAfee, etc.) need separate configuration

---

## 📞 NEED HELP?

1. Check `FIREWALL_CONFIG_GUIDE.md` for detailed troubleshooting
2. Check `README.md` for complete UDP listener documentation
3. Check TempestDisplay log file in `Logs` folder
4. Ensure WeatherFlow hub has power and network connection
5. Verify hub and computer are on same network/subnet

---

## 🎯 SUCCESS INDICATORS

After configuring firewall and starting TempestDisplay:

✅ Log shows: "UDP listener started successfully on port 50222"
✅ `netstat -an | findstr :50222` shows the port is listening
✅ Log shows: "Rapid Wind received" or "Observation received"
✅ Weather data updates in real-time

If you see all of these, your firewall is configured correctly!
