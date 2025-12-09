# UDP Listener Architecture

## Before Refactoring (Monolithic)
```
┌──────────────────────────────────────────────────────────┐
│                                                          │
│            FrmMain.UdpListener.vb (950 lines)            │
│                                                          │
│  • Event Handlers                                        │
│  • JSON Parsing                                          │
│  • Unit Conversions                                      │
│  • UI Updates                                            │
│  • Grid Updates                                          │
│  • Pressure Tracking                                     │
│  • Lightning Persistence                                 │
│  • Rain Gauge Updates                                    │
│  • Everything mixed together!                            │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

## After Refactoring (Modular)
```
┌─────────────────────────────────────────────────────────────────────┐
│                     UDP Packet Arrives                              │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│         FrmMain.UdpListener.vb (450 lines)                          │
│         Event Coordinators & Flow Control                           │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────┐     │
│  │ OnObservationReceived()                                  │     │
│  │   • Receives raw JSON from UDP                           │     │
│  │   • Logs to UdpLogService                                │     │
│  │   • Calls ProcessObservation()                           │     │
│  └────────────────────┬─────────────────────────────────────┘     │
│                       │                                             │
│  ┌────────────────────▼─────────────────────────────────────┐     │
│  │ ProcessObservation()                                     │     │
│  │   • Main coordinator method (~20 lines)                  │     │
│  │   • Delegates to specialized components                  │     │
│  └────┬─────────┬──────────────┬──────────────┬────────────┘     │
│       │         │              │              │                    │
└───────┼─────────┼──────────────┼──────────────┼────────────────────┘
        │         │              │              │
        ▼         ▼              ▼              ▼
┌───────────┐ ┌───────────┐ ┌──────────┐ ┌──────────────┐
│  Parser   │ │ Grid UI   │ │ Main UI  │ │ Rain Gauges  │
└───────────┘ └───────────┘ └──────────┘ └──────────────┘
```

## Component Details

### 1. ObservationParser (Static Utility)
```
┌─────────────────────────────────────────────────────────┐
│      ObservationParser.vb (90 lines)                    │
│      Static JSON Parsing Utility                        │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Input:  Raw JSON string                               │
│          ↓                                              │
│  Process: • Validate obs_st type                       │
│          • Parse JSON array                            │
│          • Extract all 18 fields                       │
│          ↓                                              │
│  Output: ObservationData object                        │
│         (or Nothing if parse fails)                    │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### 2. ObservationData (Model)
```
┌─────────────────────────────────────────────────────────┐
│      ObservationData.vb (100 lines)                     │
│      Data Transfer Object                               │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Raw Properties:                                        │
│    • Timestamp, WindLull, WindAvg, WindGust            │
│    • WindDirection, Pressure, Temperature              │
│    • Humidity, Illuminance, UvIndex                    │
│    • SolarRadiation, RainAccum, PrecipType             │
│    • StrikeDistance, StrikeCount, Battery              │
│                                                         │
│  Computed Properties (Unit Conversions):                │
│    • TempF (°C → °F)                                   │
│    • WindAvgMph, WindGustMph, WindLullMph (m/s → mph)  │
│    • RainInches (mm → inches)                          │
│    • PressureInHg (mb → inHg)                          │
│    • TimestampDateTime (Unix → DateTime)               │
│    • PrecipTypeText ("None", "Rain", "Hail")           │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### 3. UI Update Flow
```
┌─────────────────────────────────────────────────────────┐
│     FrmMain.ObservationUI.vb (180 lines)                │
│     Specialized UI Update Methods                       │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  UpdateWeatherUI(data, rawJson)  ← Master coordinator  │
│         │                                               │
│         ├─→ UpdateTemperatureGauges(data)              │
│         │     • TgCurrentTemp                           │
│         │     • TgFeelsLike (calculates)                │
│         │     • TgDewpoint (calculates)                 │
│         │     • FgRH (humidity)                         │
│         │                                               │
│         ├─→ UpdateWindDisplays(data)                    │
│         │     • LblAvgWindSpd                           │
│         │     • LblWindGust                             │
│         │     • LblWindLull                             │
│         │     • LblWindDir                              │
│         │                                               │
│         ├─→ UpdateAtmosphericReadings(data)             │
│         │     • LblBaroPress                            │
│         │     • AddPressureReading()                    │
│         │     • CalculatePressureTrend()                │
│         │     • LblPressTrend (with color)              │
│         │                                               │
│         ├─→ UpdateLightDisplays(data)                   │
│         │     • LblUV                                   │
│         │     • LblSolRad                               │
│         │     • LblBrightness                           │
│         │                                               │
│         ├─→ UpdateLightningDisplays(data)               │
│         │     • TxtStrikeCount                          │
│         │                                               │
│         ├─→ UpdateAirDensity(data)                      │
│         │     • CalculateAirDensity()                   │
│         │     • LblAirDensity                           │
│         │     • LblAirDensityCat                        │
│         │                                               │
│         └─→ UpdateBatteryStatus(battery)                │
│               • LblBatteryStatus                        │
│               • Color coding based on voltage           │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### 4. Grid Update Flow
```
┌─────────────────────────────────────────────────────────┐
│     FrmMain.GridUpdates.vb (220 lines)                  │
│     DataGridView Management                             │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Initialization (called once at startup):               │
│    • CreateHubStatusGrid()    → 10 rows                │
│    • CreateObsStGrid()        → 20 rows                │
│                                                         │
│  Updates (called every minute):                         │
│                                                         │
│    UpdateObsStGrid(data, root)                          │
│      ├─ Rows 0-2:  Serial, Type, Hub SN (from root)    │
│      ├─ Row 3:     Timestamp (formatted)               │
│      ├─ Rows 4-7:  Wind data (with cardinal)           │
│      ├─ Rows 8-10: Atmospheric (pressure, temp, RH)    │
│      ├─ Rows 11-13: Light (lux, UV, solar)             │
│      ├─ Rows 14-15: Rain (accumulated, type text)      │
│      ├─ Rows 16-17: Lightning (distance, count)        │
│      └─ Rows 18-19: Device (battery, interval)         │
│                                                         │
│    ParseAndDisplayHubStatus(json)                       │
│      └─ Updates DgvHubStatus with hub telemetry        │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Data Flow Diagram

```
UDP Packet (Raw JSON)
    │
    ▼
┌─────────────────────┐
│ OnObservationReceived│  (Event Handler)
└──────────┬──────────┘
           │
           ├─→ Log to UdpLogService
           │
           ▼
┌─────────────────────┐
│ ProcessObservation   │  (Coordinator - 20 lines)
└──────────┬──────────┘
           │
           ├─→ ObservationParser.ParseObsStPacket(json)
           │       │
           │       └─→ Returns ObservationData
           │
           ├─→ UpdateObsStGrid(data, root)
           │       └─→ Updates DgvObsSt (20 rows)
           │
           ├─→ UpdateWeatherUI(data, json)
           │       ├─→ Temperature gauges
           │       ├─→ Wind displays
           │       ├─→ Pressure + trend
           │       ├─→ Light displays
           │       ├─→ Lightning count
           │       ├─→ Air density
           │       └─→ Battery status
           │
           └─→ UpdateRainGaugesAsync()
                   └─→ Fetches from MeteoBridge API
```

## Benefits of Modular Design

### Separation of Concerns
```
┌──────────────┬────────────────────────────────────┐
│ Component    │ Responsibility                     │
├──────────────┼────────────────────────────────────┤
│ Parser       │ JSON → Data Model only             │
│ Model        │ Data + Unit Conversions only       │
│ UI Updates   │ Control Updates only               │
│ Grid Updates │ DataGridView Updates only          │
│ Main Listener│ Event Coordination only            │
└──────────────┴────────────────────────────────────┘
```

### Easy to Extend
```
Want to add a new sensor reading?

1. Add field to ObservationData
2. Add parsing in ObservationParser
3. Add UI update method in ObservationUI
4. Add grid row in GridUpdates
5. No changes needed to main coordinator!
```

### Easy to Debug
```
Problem with temperature display?
  → Look in FrmMain.ObservationUI.vb
  → UpdateTemperatureGauges() method

Problem with grid not updating?
  → Look in FrmMain.GridUpdates.vb
  → UpdateObsStGrid() method

Problem parsing JSON?
  → Look in ObservationParser.vb
  → ParseObsStPacket() method
```

## File Organization
```
TempestDisplay/
├── Models/
│   ├── ObservationData.vb          (100 lines - Data model)
│   └── ObservationParser.vb        (90 lines - Parser)
│
└── FrmMain.Partials/
    ├── FrmMain.UdpListener.vb      (450 lines - Main coordinator)
    ├── FrmMain.ObservationUI.vb    (180 lines - UI updates)
    └── FrmMain.GridUpdates.vb      (220 lines - Grid management)
```

Total: 1,040 lines across 5 focused files
Original: 950 lines in 1 monolithic file

**Result:** Each file < 500 lines, single responsibility, easy to maintain!
