# WeatherFlow UDP Listener Implementation

## Overview
The WeatherFlow Smart Weather Station hub broadcasts UDP messages on port 50222 over your local network. This implementation provides a robust, event-driven listener to receive real-time weather data.

## Files Created

### Core Classes
1. **WeatherFlowUdpListener.vb** - Main UDP listener class
2. **UdpEventArgs.vb** - Event argument classes for different message types
3. **FrmMain.UdpListener.EXAMPLE.vb** - Example integration code (rename to use)
4. **USAGE_EXAMPLE.txt** - Detailed usage instructions and data format reference

## Quick Start

### 1. Add to FrmMain.vb Private Fields
```vb
Private _udpListener As WeatherFlowUdpListener
```

### 2. Initialize in FrmMain_Load (after LogService.Instance.Init())
```vb
Try
    _udpListener = New WeatherFlowUdpListener()
    AddHandler _udpListener.RapidWindReceived, AddressOf OnRapidWindReceived
    AddHandler _udpListener.ObservationReceived, AddressOf OnObservationReceived
    _udpListener.StartListening()
    Log.Write("UDP listener started successfully")
Catch ex As Exception
    Log.WriteException(ex, "Error starting UDP listener")
End Try
```

### 3. Cleanup in FrmMain_FormClosing
```vb
Try
    If _udpListener IsNot Nothing Then
        _udpListener.StopListening()
        _udpListener.Dispose()
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error stopping UDP listener")
End Try
```

## Message Types Received

### Rapid Wind (Every 3 seconds)
- **Event**: `RapidWindReceived`
- **Data**: Wind speed (m/s), Wind direction (degrees)
- **Use**: Real-time wind updates

### Observation (Every minute)
- **Event**: `ObservationReceived`
- **Data**: Complete weather observation
- **Contains**: Temperature, humidity, pressure, rain, UV, illuminance, lightning, etc.

### Device Status
- **Event**: `DeviceStatusReceived`
- **Data**: Battery level, signal strength, sensor status

### Hub Status
- **Event**: `HubStatusReceived`
- **Data**: Hub uptime, firmware version, network info

## Observation Data Format (obs_st)

The observation array contains 18 values:
```
[0]  = Timestamp (Unix epoch seconds)
[1]  = Wind Lull (m/s)
[2]  = Wind Avg (m/s)
[3]  = Wind Gust (m/s)
[4]  = Wind Direction (degrees, 0-360)
[5]  = Wind Sample Interval (seconds)
[6]  = Station Pressure (MB)
[7]  = Air Temperature (°C)
[8]  = Relative Humidity (%)
[9]  = Illuminance (lux)
[10] = UV Index
[11] = Solar Radiation (W/m²)
[12] = Rain Accumulation (mm)
[13] = Precipitation Type (0=none, 1=rain, 2=hail)
[14] = Lightning Strike Avg Distance (km)
[15] = Lightning Strike Count
[16] = Battery (volts)
[17] = Report Interval (minutes)
```

## Thread Safety

⚠️ **IMPORTANT**: UDP messages arrive on a background thread!

Always use `Invoke()` when updating UI controls:
```vb
If InvokeRequired Then
    Invoke(New Action(Sub()
        ' Update controls here
        LblTemp.Text = temperature.ToString()
    End Sub))
End If
```

## Firewall Configuration

Windows Firewall may block UDP port 50222. Options:

### Option 1: Add Inbound Rule (Recommended)
1. Open Windows Firewall with Advanced Security
2. Click "Inbound Rules" → "New Rule"
3. Rule Type: Port
4. Protocol: UDP, Specific local ports: 50222
5. Action: Allow the connection
6. Profile: Private, Domain (as appropriate)
7. Name: "TempestDisplay UDP Listener"

### Option 2: Temporary Testing
Temporarily disable Windows Firewall for testing (not recommended for production)

### Option 3: Application Exception
Add TempestDisplay.exe to Windows Firewall allowed apps

## Network Requirements

- Hub and computer must be on the same local network
- UDP broadcasts work on most home networks
- VLANs or network segmentation may block UDP broadcasts
- No router configuration needed (broadcasts are local)

## Troubleshooting

### Not Receiving Messages

1. **Check Hub Status**
   - Ensure WeatherFlow hub has power and network connection
   - Check hub LED status

2. **Verify Network**
   - Ensure computer and hub are on same network/subnet
   - Check if other devices can see the hub

3. **Check Firewall**
   - Temporarily disable firewall to test
   - Check Windows Event Viewer for blocked connections

4. **Enable Debug Logging**
   ```vb
   AddHandler _udpListener.RawMessageReceived, AddressOf OnRawMessageReceived
   ```
   This logs ALL UDP messages to help diagnose issues

5. **Check Port Availability**
   - Another application might be using port 50222
   - Use `netstat -an | findstr 50222` in command prompt

### Performance Considerations

- Rapid wind updates every 3 seconds
- Full observations every minute
- Events fire on background thread
- Use `Invoke()` for UI updates to prevent cross-thread exceptions

## Example: Parsing Observation Data

```vb
Private Sub OnObservationReceived(sender As Object, e As ObservationEventArgs)
    Try
        If InvokeRequired Then
            Invoke(New Action(Sub()
                Using document = JsonDocument.Parse(e.RawJson)
                    Dim root = document.RootElement
                    Dim obsArray = root.GetProperty("obs")
                    Dim ob = obsArray(0) ' First observation array
                    
                    ' Extract values
                    Dim temperature = ob(7).GetDouble() ' °C
                    Dim humidity = ob(8).GetInt32() ' %
                    Dim pressure = ob(6).GetDouble() ' MB
                    Dim rain = ob(12).GetDouble() ' mm
                    
                    ' Update UI
                    LblTemp.Text = $"{temperature:F1}°C"
                    LblHumidity.Text = $"{humidity}%"
                    LblPressure.Text = $"{pressure:F1} MB"
                    LblRain.Text = $"{rain:F2} mm"
                End Using
            End Sub))
        End If
    Catch ex As Exception
        Log.WriteException(ex, "Error parsing observation")
    End Try
End Sub
```

## Benefits Over REST API

✅ **Real-time**: Data arrives immediately (no polling)
✅ **No API calls**: Reduces internet traffic and API quota usage
✅ **Lower latency**: Local network communication
✅ **More frequent**: Rapid wind every 3 seconds vs API's 1-minute cache
✅ **Offline capable**: Works even if internet is down

## Limitations

⚠️ UDP only works on local network
⚠️ Historical data still requires REST API
⚠️ No data when away from home (unless VPN to home network)

## Recommended Approach

Use **both** UDP and REST API:
- **UDP**: Real-time updates when on local network
- **REST API**: Fallback when away from home or for historical data
- **Detection**: Check if UDP messages received within 2 minutes, otherwise use API

## Next Steps

1. Copy `FrmMain.UdpListener.EXAMPLE.vb` to `FrmMain.Partials\FrmMain.UdpListener.vb`
2. Add initialization call to `FrmMain_Load`
3. Implement observation parsing for your UI controls
4. Test with firewall configured properly
5. Add error handling and user notifications

## References

- [WeatherFlow UDP API Documentation](https://weatherflow.github.io/Tempest/api/udp.html)
- [Message Format Examples](https://weatherflow.github.io/Tempest/api/udp.html)
