# UDP Listener Refactoring - Testing Checklist

## Pre-Migration Steps
- [ ] Backup original FrmMain.UdpListener.vb file
- [ ] Review all new files for syntax errors
- [ ] Ensure all new files are included in project
- [ ] Build solution to check for compilation errors

## Migration Steps

### Option A: Clean Switch (Recommended)
- [ ] Rename FrmMain.UdpListener.vb to FrmMain.UdpListener.OLD.vb (backup)
- [ ] Rename FrmMain.UdpListener.Refactored.vb to FrmMain.UdpListener.vb
- [ ] Build solution
- [ ] Fix any build errors
- [ ] Run application

### Option B: Side-by-Side (Conservative)
- [ ] Keep original file as-is
- [ ] Comment out call to InitializeUdpListener() in original
- [ ] Ensure FrmMain.vb calls CreateObsStGrid() from new version
- [ ] Build and test
- [ ] Once validated, proceed with Option A

## Functional Testing

### UDP Connection
- [ ] Application starts without errors
- [ ] UDP listener initializes successfully
- [ ] Port 50222 is listening
- [ ] Log shows "UDP listener started successfully on port 50222"

### Rapid Wind Updates (Every 3 seconds)
- [ ] LblWindSpd updates with current wind speed
- [ ] LblWindDir updates with direction and cardinal (e.g., "180° S")
- [ ] Log shows "[UDP] Rapid Wind" entries every 3 seconds
- [ ] Wind speed displays in MPH
- [ ] Wind direction shows both degrees and cardinal direction

### Observation Updates (Every 60 seconds)
- [ ] Full obs_st packet arrives every minute
- [ ] TsslObs_St shows raw JSON packet
- [ ] Log shows "[UDP] Observation received" entries

#### Temperature Displays
- [ ] TgCurrentTemp shows current temperature in °F
- [ ] TgFeelsLike shows calculated feels like temperature
- [ ] TgDewpoint shows calculated dew point
- [ ] FgRH shows relative humidity percentage

#### Wind Displays
- [ ] LblAvgWindSpd shows average wind speed in MPH
- [ ] LblWindGust shows gust speed in MPH
- [ ] LblWindLull shows lull speed in MPH
- [ ] LblWindDir shows direction with cardinal

#### Atmospheric Displays
- [ ] LblBaroPress shows pressure in inHg
- [ ] LblPressTrend eventually shows trend after 3 hours
- [ ] Trend label color codes correctly (Blue=Falling, Red=Rising, Green=Steady)
- [ ] Pressure readings added to history
- [ ] Pressure history saved to file

#### Light/UV Displays
- [ ] LblUV shows UV index
- [ ] LblSolRad shows solar radiation in W/m²
- [ ] LblBrightness shows illuminance in lux

#### Rain Displays
- [ ] PTC (Precipitation Towers Control) updates
- [ ] Today's rain total displayed
- [ ] Yesterday's rain total displayed (fetched on first obs)
- [ ] Month, Year, AllTime totals displayed

#### Lightning Displays
- [ ] TxtStrikeCount shows strike count from observation
- [ ] Lightning strike events update distance and time
- [ ] Last strike info persists across app restarts

#### Air Density
- [ ] LblAirDensity calculates and displays density in kg/m³
- [ ] LblAirDensityCat shows category (e.g., "Light", "Normal", "Dense")
- [ ] Calculation log shows inputs (TempF, PressureMb, Humidity)

#### Battery Status
- [ ] LblBatteryStatus shows voltage
- [ ] Background color codes correctly:
  - [ ] >= 2.455V = Green
  - [ ] >= 2.41V = Yellow
  - [ ] >= 2.375V = Orange
  - [ ] < 2.375V = Red

#### Last Update Time
- [ ] LblUpdate shows time of last observation (e.g., "2:45:30 PM")

### DataGridView Updates

#### DgvObsSt (Observation Grid)
- [ ] Grid initializes with 20 rows on startup
- [ ] Row 0: Serial Number populated
- [ ] Row 1: Type shows "obs_st"
- [ ] Row 2: Hub SN populated
- [ ] Row 3: Epoch shows timestamp + formatted date/time
- [ ] Row 4-7: Wind data (lull, avg, gust, direction with cardinal)
- [ ] Row 8-10: Atmospheric (pressure, temp, RH)
- [ ] Row 11-13: Light (illuminance, UV, solar)
- [ ] Row 14: Rain accumulated in mm
- [ ] Row 15: Precipitation type shows text ("None", "Rain", "Hail")
- [ ] Row 16: Lightning strike distance in km
- [ ] Row 17: Lightning strike count
- [ ] Row 18: Battery voltage with 3 decimals
- [ ] Row 19: Report interval in minutes
- [ ] Grid updates every minute with new data
- [ ] Selection is cleared after update
- [ ] Log shows "[Grid] DgvObsSt updated successfully"

#### DgvHubStatus (Hub Status Grid)
- [ ] Grid initializes with 10 rows on startup
- [ ] Serial number populated
- [ ] Firmware revision shown
- [ ] Uptime formatted as "Xd Xh Xm"
- [ ] RSSI shown in dBm
- [ ] All hub status fields update when packet arrives

### Event Handling

#### Rain Start Event
- [ ] When rain starts, event fires
- [ ] TsslMessages shows "Rain Start Event at [timestamp]"
- [ ] Log shows "[UDP EVENT] Rain Start Event"

#### Lightning Strike Event
- [ ] When strike occurs, event fires
- [ ] LblLightLastStrike updates with time
- [ ] TxtLightDistance updates with distance in miles
- [ ] TsslMessages shows strike info
- [ ] Strike data saves to JSON file (LastLightningStrike.json)
- [ ] Saved strike persists across app restart
- [ ] Log shows "[Lightning] Saved strike" entry

#### Hub Status Event
- [ ] Hub status packets received
- [ ] DgvHubStatus grid updates
- [ ] Log shows "[UDP] Hub status received"

### Pressure Trend Tracking

#### On Startup
- [ ] Pressure history loaded from file (if exists)
- [ ] Old readings (> 3 hours) removed
- [ ] Log shows number of loaded readings
- [ ] If enough data, trend calculated and displayed immediately

#### During Operation
- [ ] New pressure readings added every minute
- [ ] Old readings automatically removed
- [ ] History maintained at ~180 readings (3 hours at 1 min intervals)
- [ ] History saved to file periodically
- [ ] Log shows "[UDP] Pressure history" entries

#### After 3 Hours
- [ ] Pressure trend label shows trend (Falling/Rising/Steady)
- [ ] Delta pressure shown (e.g., "+2.5 mb/3hr")
- [ ] Label color codes correctly
- [ ] Log shows "[UDP] Pressure trend calculated" entries

### File Persistence

#### Pressure History
- [ ] File created at: DataDir/PressureHistory.json
- [ ] File contains array of pressure readings
- [ ] Each reading has Timestamp and PressureMb
- [ ] Only readings from last 3 hours saved
- [ ] File deleted if no valid readings

#### Lightning Strike
- [ ] File created at: DataDir/LastLightningStrike.json
- [ ] File contains single strike data (Timestamp, DistanceKm, Energy)
- [ ] File overwrites previous strike
- [ ] Data loads on startup and displays

### UDP Logging
- [ ] UdpLogService.Instance.WriteObservation() called once per obs_st
- [ ] No duplicate logging
- [ ] Daily log file rotation works
- [ ] Log files in DataDir/logs/UDP/

### Error Handling
- [ ] Invalid JSON handled gracefully
- [ ] Missing fields don't crash app
- [ ] Non-obs_st packets ignored properly
- [ ] UI thread marshaling works (no cross-thread errors)
- [ ] All exceptions logged with context
- [ ] App continues running after errors

### Performance
- [ ] No UI lag during updates
- [ ] Memory usage stable over time
- [ ] File I/O doesn't block UI
- [ ] Background tasks (pressure save, lightning save) work correctly

## Log Verification

Review log file for these patterns:

```
✓ [UDP] Observation received from 192.168.x.x
✓ [UDP] ProcessObservation called, JSON length: XXXX
✓ [Parser] (no errors)
✓ [Grid] DgvObsSt updated successfully
✓ [UI] (no errors)
✓ [UDP] Weather: XX.X°F, XX% RH, XX.XX inHg, Wind: X.X mph @ XXX°
✓ [UDP] Pressure history: XXX readings over 3 hours
✓ [UDP] Rain gauges updated - Today: X.XX, Yesterday: X.XX...
✓ [Lightning] (if strikes occur)
```

## Cleanup After Successful Testing
- [ ] Delete FrmMain.UdpListener.OLD.vb (original backup)
- [ ] Remove any temporary test code
- [ ] Update project documentation
- [ ] Commit changes to version control

## Rollback Plan (If Issues Occur)
1. Stop application
2. Rename FrmMain.UdpListener.vb to FrmMain.UdpListener.BROKEN.vb
3. Rename FrmMain.UdpListener.OLD.vb back to FrmMain.UdpListener.vb
4. Remove new files from project:
   - ObservationData.vb
   - ObservationParser.vb
   - FrmMain.ObservationUI.vb
   - FrmMain.GridUpdates.vb
5. Rebuild and run
6. Document issues for investigation

## Success Criteria
✅ All checkboxes above marked as complete
✅ No compilation errors
✅ No runtime errors in log
✅ All UDP data flowing correctly
✅ All grids updating correctly
✅ All UI controls updating correctly
✅ File persistence working
✅ Application stable for 1+ hour of operation

## Notes Section
Use this space to document any issues found during testing:

```
Issue 1:
Description: 
Steps to reproduce:
Resolution:

Issue 2:
Description:
Steps to reproduce:
Resolution:
```
