# Step-by-Step Integration Guide
# How to Add UDP Listening to Your TempestDisplay App

## Overview
This guide will walk you through integrating the UDP listener into your existing TempestDisplay application to receive real-time weather data from your WeatherFlow hub.

---

## Step 1: Add the UDP Listener Files to Your Project

The UDP listener files are already in your project at:
```
C:\VB18\TempestDisplay\TempestDisplay\Modules\UdpListener\
```

### Files to Include in Your Project:
1. `WeatherFlowUdpListener.vb` - Main UDP listener class
2. `UdpEventArgs.vb` - Event argument classes

### In Visual Studio:
1. Open your TempestDisplay solution
2. In Solution Explorer, right-click on your project
3. Select "Add" → "Existing Item..."
4. Navigate to `Modules\UdpListener\`
5. Select `WeatherFlowUdpListener.vb` and `UdpEventArgs.vb`
6. Click "Add"

**OR** if files are already in the folder:
1. In Solution Explorer, click "Show All Files" button at the top
2. Find the UdpListener folder
3. Right-click the `.vb` files and select "Include In Project"

---

## Step 2: Add Private Field to FrmMain.vb

Open `FrmMain.vb` and add this private field at the top with your other private fields:

```vb
Public Class FrmMain

    Private _previousTab As TabPage
    Private _lastDataFetchTime As DateTime = DateTime.MinValue
    Private _dataFetchIntervalSeconds As Integer = 180
    Private _settingsWatcher As FileSystemWatcher
    Private _settingsReloadTimer As Timer
    Private _settingsReloadPending As Boolean = False
    Private _lastMidnightCheck As DateTime = DateTime.MinValue
    Private _nextMidnight As DateTime
    Private _udpListener As WeatherFlowUdpListener  ' ← ADD THIS LINE
```

---

## Step 3: Create a New Partial Class File for UDP Handling

Create a new file: `FrmMain.Partials\FrmMain.UdpListener.vb`

### In Visual Studio:
1. Right-click on `FrmMain.Partials` folder
2. Select "Add" → "Class..."
3. Name it: `FrmMain.UdpListener.vb`
4. Click "Add"

### Replace the entire file content with:

```vb
Imports System.Text.Json

Partial Public Class FrmMain

    Private Sub InitializeUdpListener()
        Try
            Log.Write("Initializing UDP listener for WeatherFlow hub...")
            
            _udpListener = New WeatherFlowUdpListener()

            ' Subscribe to events
            AddHandler _udpListener.RapidWindReceived, AddressOf OnRapidWindReceived
            AddHandler _udpListener.ObservationReceived, AddressOf OnObservationReceived
            AddHandler _udpListener.DeviceStatusReceived, AddressOf OnDeviceStatusReceived
            AddHandler _udpListener.HubStatusReceived, AddressOf OnHubStatusReceived

            ' Uncomment to log all raw messages (useful for debugging):
            ' AddHandler _udpListener.RawMessageReceived, AddressOf OnRawMessageReceived

            ' Start listening
            _udpListener.StartListening()
            Log.Write("UDP listener started successfully on port 50222")

        Catch ex As Exception
            Log.WriteException(ex, "Error starting UDP listener")
            ' Don't show message box here - let the app continue with REST API fallback
        End Try
    End Sub

    Private Sub CleanupUdpListener()
        Try
            If _udpListener IsNot Nothing Then
                Log.Write("Stopping UDP listener...")
                _udpListener.StopListening()
                _udpListener.Dispose()
                _udpListener = Nothing
                Log.Write("UDP listener stopped and disposed")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error stopping UDP listener")
        End Try
    End Sub

    Private Sub OnRapidWindReceived(sender As Object, e As RapidWindEventArgs)
        ' Rapid wind data arrives every 3 seconds
        ' This gives you real-time wind updates
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                    ' Update your wind display controls here
                    ' Example:
                    ' LblWindSpeed.Text = $"{e.WindSpeed:F1} m/s"
                    ' LblWindDirection.Text = $"{e.WindDirection}°"
                    
                    Log.Write($"[UDP] Rapid Wind: {e.WindSpeed:F1} m/s @ {e.WindDirection}°")
                End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing rapid wind data")
        End Try
    End Sub

    Private Sub OnObservationReceived(sender As Object, e As ObservationEventArgs)
        ' Full observation arrives every minute
        ' This contains ALL weather data
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                    Log.Write($"[UDP] Observation received from {e.RemoteEndPoint.Address}")
                    
                    ' Parse the observation JSON
                    ParseAndDisplayObservation(e.RawJson)
                End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing observation data")
        End Try
    End Sub

    Private Sub ParseAndDisplayObservation(jsonData As String)
        Try
            Using document = JsonDocument.Parse(jsonData)
                Dim root = document.RootElement
                
                ' Get the observation array
                If root.TryGetProperty("obs", Nothing) Then
                    Dim obsArray = root.GetProperty("obs")
                    If obsArray.GetArrayLength() > 0 Then
                        Dim ob = obsArray(0) ' First observation
                        
                        ' Extract values from the observation array
                        ' See USAGE_EXAMPLE.txt for complete array format
                        
                        Dim timestamp = ob(0).GetInt64()
                        Dim windLull = ob(1).GetDouble()           ' m/s
                        Dim windAvg = ob(2).GetDouble()            ' m/s
                        Dim windGust = ob(3).GetDouble()           ' m/s
                        Dim windDirection = ob(4).GetInt32()       ' degrees
                        Dim pressure = ob(6).GetDouble()           ' MB
                        Dim temperature = ob(7).GetDouble()        ' °C
                        Dim humidity = ob(8).GetInt32()            ' %
                        Dim illuminance = ob(9).GetInt32()         ' lux
                        Dim uvIndex = ob(10).GetDouble()
                        Dim solarRadiation = ob(11).GetInt32()     ' W/m²
                        Dim rainAccum = ob(12).GetDouble()         ' mm
                        Dim precipType = ob(13).GetInt32()         ' 0=none, 1=rain, 2=hail
                        Dim strikeDistance = ob(14).GetDouble()    ' km
                        Dim strikeCount = ob(15).GetInt32()
                        Dim battery = ob(16).GetDouble()           ' volts
                        
                        ' Update your UI controls here
                        ' Example:
                        ' TgCurrentTemp.Value = temperature
                        ' LblHumidity.Text = $"{humidity}%"
                        ' LblPressure.Text = $"{pressure:F1} MB"
                        ' LblRain.Text = $"{rainAccum:F2} mm"
                        ' LblWindSpeed.Text = $"{windAvg:F1} m/s"
                        ' LblWindGust.Text = $"{windGust:F1} m/s"
                        ' LblUV.Text = $"{uvIndex:F1}"
                        
                        Log.Write($"[UDP] Temp: {temperature:F1}°C, Humidity: {humidity}%, Pressure: {pressure:F1} MB")
                        
                    End If
                End If
            End Using
        Catch ex As Exception
            Log.WriteException(ex, "Error parsing observation JSON")
        End Try
    End Sub

    Private Sub OnDeviceStatusReceived(sender As Object, e As DeviceStatusEventArgs)
        Try
            Log.Write($"[UDP] Device status received from {e.RemoteEndPoint.Address}")
            ' TODO: Parse battery level, signal strength, sensor status
            ' JSON format varies by device type
        Catch ex As Exception
            Log.WriteException(ex, "Error processing device status")
        End Try
    End Sub

    Private Sub OnHubStatusReceived(sender As Object, e As HubStatusEventArgs)
        Try
            Log.Write($"[UDP] Hub status received from {e.RemoteEndPoint.Address}")
            ' TODO: Parse hub uptime, firmware version, etc.
        Catch ex As Exception
            Log.WriteException(ex, "Error processing hub status")
        End Try
    End Sub

    Private Sub OnRawMessageReceived(sender As Object, e As RawMessageEventArgs)
        ' Log all raw UDP messages - useful for debugging
        ' WARNING: This creates a lot of log entries!
        Log.Write($"[UDP RAW] {e.RemoteEndPoint.Address}: {e.JsonMessage}")
    End Sub

End Class
```

---

## Step 4: Update FrmMain_Load to Start UDP Listener

In `FrmMain.vb`, find the `FrmMain_Load` method and add the UDP listener initialization AFTER LogService is initialized:

```vb
Private Async Sub FrmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
    ' ... existing code ...
    
    LogService.Instance.Init()

    ' ... existing code ...

    ' Initialize midnight tracking
    _lastMidnightCheck = DateTime.Now
    _nextMidnight = CalculateNextMidnight()
    Log.Write($"Next midnight update scheduled for: {_nextMidnight:yyyy-MM-dd HH:mm:ss}")

    ' Initialize UDP listener for real-time weather data
    ' ADD THESE LINES:
    Try
        InitializeUdpListener()
    Catch ex As Exception
        Log.WriteException(ex, "Failed to initialize UDP listener, will use REST API fallback")
    End Try

    ' ... rest of existing code ...
End Sub
```

---

## Step 5: Update FrmMain_FormClosing to Stop UDP Listener

In `FrmMain.vb`, find the `FrmMain_OnFormClosing` method and add cleanup:

```vb
Private Sub FrmMain_OnFormClosing(sender As Object, e As CancelEventArgs) Handles Me.FormClosing
    Log.Write("FrmMain_OnFormClosing: Entered")

    ' Cancel all pending async operations
    Try
        If Not AppCancellationTokenSource.IsCancellationRequested Then
            AppCancellationTokenSource.Cancel()
            Log.Write("Application cancellation token signaled")
        End If
    Catch ex As Exception
        Log.WriteException(ex, "Error canceling application token")
    End Try

    ' ADD THESE LINES:
    Try
        CleanupUdpListener()
    Catch ex As Exception
        Log.WriteException(ex, "Error cleaning up UDP listener")
    End Try

    ' ... existing save/cleanup code ...
End Sub
```

---

## Step 6: Configure Windows Firewall

**IMPORTANT:** The UDP listener won't receive data until you configure Windows Firewall!

### Easiest Method - Run the Batch Script:
1. Navigate to `Modules\UdpListener\`
2. Right-click `Configure-Firewall.bat`
3. Select "Run as administrator"
4. Press any key when prompted
5. Done!

### Alternative - PowerShell Command:
Open PowerShell as Administrator and run:
```powershell
New-NetFirewallRule -DisplayName "TempestDisplay UDP Listener" -Direction Inbound -Protocol UDP -LocalPort 50222 -Action Allow -Profile Private,Domain
```

See `FIREWALL_QUICK_REF.md` for more options.

---

## Step 7: Build and Test

### 1. Build the Project
- Press `F6` or click "Build" → "Build Solution"
- Fix any compilation errors

### 2. Run TempestDisplay
- Press `F5` or click "Start"

### 3. Check the Log File
Look in `Logs\` folder for your log file and verify you see:
```
UDP listener started successfully on port 50222
[UDP] Rapid Wind: 3.2 m/s @ 180°
[UDP] Observation received from 192.168.1.xxx
[UDP] Temp: 22.5°C, Humidity: 65%, Pressure: 1013.2 MB
```

### 4. If Not Receiving Data
- Check firewall is configured (Step 6)
- Verify WeatherFlow hub has power and network connection
- Ensure computer and hub are on same network
- Check log for errors
- See `FIREWALL_CONFIG_GUIDE.md` for troubleshooting

---

## Step 8: Update Your UI Controls (Customize This Part!)

In the `ParseAndDisplayObservation` method in `FrmMain.UdpListener.vb`, replace the commented examples with your actual control names.

### Example: If you have these controls:
- `TgCurrentTemp` - Temperature gauge
- `LblHumidity` - Humidity label
- `LblPressure` - Pressure label
- `LblWindSpeed` - Wind speed label

### Update the method like this:
```vb
' Update your UI controls here
TgCurrentTemp.Value = temperature
LblHumidity.Text = $"{humidity}%"
LblPressure.Text = $"{pressure:F1} MB"
LblWindSpeed.Text = $"{windAvg:F1} m/s"
LblWindGust.Text = $"{windGust:F1} m/s"
LblWindDirection.Text = $"{windDirection}°"
FgRH.Value = humidity
TgDewpoint.Value = CalculateDewpoint(temperature, humidity)
```

---

## Benefits You'll Get

✅ **Real-time wind updates** every 3 seconds (vs API's 1-minute cache)
✅ **Full observations** every minute with all weather data
✅ **No API calls needed** for real-time data (saves quota)
✅ **Lower latency** - local network is faster than internet API
✅ **Offline capable** - works even if internet is down
✅ **Automatic fallback** - If UDP isn't available, REST API still works

---

## Dual Mode: UDP + REST API (Recommended)

The best approach is to use BOTH:

### Use UDP When:
- You're on your local network (home/office)
- Hub is broadcasting (every minute)
- Low latency real-time updates needed

### Use REST API When:
- Away from home (no UDP broadcasts)
- Need historical data
- UDP hasn't received data in 2+ minutes (fallback)

### Implementation Idea:
```vb
' Track last UDP message time
Private _lastUdpMessageTime As DateTime = DateTime.MinValue

Private Sub OnObservationReceived(sender As Object, e As ObservationEventArgs)
    _lastUdpMessageTime = DateTime.Now
    ' ... parse and display ...
End Sub

' In your timer or periodic check:
Private Sub CheckDataSource()
    Dim timeSinceUdp = (DateTime.Now - _lastUdpMessageTime).TotalMinutes
    
    If timeSinceUdp > 2 Then
        ' No UDP data in 2 minutes, use REST API
        Log.Write("UDP data stale, falling back to REST API")
        Await FetchTempestDataAsync() ' Your existing API call
    Else
        Log.Write("Using UDP data (real-time)")
    End If
End Sub
```

---

## Troubleshooting Checklist

### Not Compiling?
- ☐ Did you add `WeatherFlowUdpListener.vb` to project?
- ☐ Did you add `UdpEventArgs.vb` to project?
- ☐ Do you have `Imports System.Text.Json` at top of file?

### Not Receiving Data?
- ☐ Is firewall configured for UDP port 50222?
- ☐ Does log show "UDP listener started successfully"?
- ☐ Is WeatherFlow hub powered on and connected?
- ☐ Are hub and computer on same network?
- ☐ Check `netstat -an | findstr :50222` shows port listening

### Data Received But UI Not Updating?
- ☐ Did you replace the commented examples with your actual control names?
- ☐ Are you using `InvokeRequired` and `Invoke()` for thread safety?
- ☐ Check log for parsing errors

---

## Next Steps

1. ✅ Complete Steps 1-7 above
2. ✅ Test and verify UDP data is received
3. ✅ Customize Step 8 with your actual control names
4. 📈 Add dual-mode logic (UDP + REST API fallback)
5. 🎨 Add status indicator showing if using UDP or API
6. 📊 Consider showing "LIVE" indicator when UDP is active

---

## Reference Files

- `USAGE_EXAMPLE.txt` - Detailed observation array format
- `README.md` - Complete UDP listener documentation
- `FIREWALL_QUICK_REF.md` - Firewall commands reference
- `FIREWALL_CONFIG_GUIDE.md` - Detailed firewall troubleshooting

---

## Summary

You now have:
1. ✅ UDP listener classes in your project
2. ✅ Integration code in FrmMain
3. ✅ Event handlers for real-time data
4. ✅ Firewall configured
5. ✅ Real-time weather data flowing!

Your TempestDisplay app can now receive live weather data directly from your WeatherFlow hub with minimal latency! 🌤️
