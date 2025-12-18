# Step-by-Step Implementation Guide
## Implementing New Custom Controls in TempestDisplay

This guide provides detailed, copy-paste-ready code for implementing all 13 new custom controls.

---

## PHASE 1: Temperature Thermometers (⭐ START HERE)

### Benefits
- Save 50% horizontal space
- Better visual comparison
- More intuitive display

### Step 1.1: Add Control Declarations

**File:** `FrmMain.Designer.vb` (at end of file, around line 1920)

**Add these lines:**
```vb
Friend WithEvents ThermCurrentTemp As TempThermometerControl
Friend WithEvents ThermFeelsLike As TempThermometerControl
Friend WithEvents ThermDewpoint As TempThermometerControl
```

### Step 1.2: Update InitializeCustomControls()

**File:** `FrmMain.Partials\FrmMain.CustomControls.vb`

**Replace the TgCurrentTemp initialization section (lines 8-40) with:**
```vb
' Temperature thermometers - Current Temp
If TgCurrentTemp IsNot Nothing Then
    ' Keep existing gauge for backward compatibility
    TgCurrentTemp.Label = "Current Temperature"
    TgCurrentTemp.BackColor = _customBackColor
    TgCurrentTemp.MinF = -5
    TgCurrentTemp.MaxF = 110
Else
    ' Create new thermometer control
    Log.Write("Creating ThermCurrentTemp as TempThermometerControl")
    Try
        If TlpData IsNot Nothing Then
            ThermCurrentTemp = New Controls.TempThermometerControl With {
                .Label = "Current",
                .BackColor = _customBackColor,
                .Dock = DockStyle.Fill,
                .MinF = -5,
                .MaxF = 110,
                .ShowFreezeMarker = True,
                .ShowDualScale = True
            }

            ' Add to column 0, row 0 (single column)
            TlpData.Controls.Add(ThermCurrentTemp, 0, 0)

            Log.Write("ThermCurrentTemp created and added to TlpData[0,0]")
        End If
    Catch ex As Exception
        Log.WriteException(ex, "Error creating ThermCurrentTemp")
    End Try
End If
```

**Replace the TgFeelsLike initialization section (lines 42-73) with:**
```vb
' Temperature thermometers - Feels Like
If TgFeelsLike IsNot Nothing Then
    TgFeelsLike.Label = "Feels Like"
    TgFeelsLike.BackColor = _customBackColor
    TgFeelsLike.MinF = -10
    TgFeelsLike.MaxF = 120
Else
    Log.Write("Creating ThermFeelsLike as TempThermometerControl")
    Try
        If TlpData IsNot Nothing Then
            ThermFeelsLike = New Controls.TempThermometerControl With {
                .Label = "Feels Like",
                .BackColor = _customBackColor,
                .Dock = DockStyle.Fill,
                .MinF = -10,
                .MaxF = 120,
                .ShowFreezeMarker = True,
                .ShowDualScale = True
            }

            ' Add to column 1, row 0 (single column)
            TlpData.Controls.Add(ThermFeelsLike, 1, 0)

            Log.Write("ThermFeelsLike created and added to TlpData[1,0]")
        End If
    Catch ex As Exception
        Log.WriteException(ex, "Error creating ThermFeelsLike")
    End Try
End If
```

**Replace the TgDewpoint initialization section (lines 75-106) with:**
```vb
' Temperature thermometers - Dew Point
If TgDewpoint IsNot Nothing Then
    TgDewpoint.Label = "Dew Point"
    TgDewpoint.BackColor = _customBackColor
    TgDewpoint.MinF = -5
    TgDewpoint.MaxF = 110
Else
    Log.Write("Creating ThermDewpoint as TempThermometerControl")
    Try
        If TlpData IsNot Nothing Then
            ThermDewpoint = New Controls.TempThermometerControl With {
                .Label = "Dew Point",
                .BackColor = _customBackColor,
                .Dock = DockStyle.Fill,
                .MinF = -5,
                .MaxF = 110,
                .ShowFreezeMarker = True,
                .ShowDualScale = True
            }

            ' Add to column 2, row 0 (single column)
            TlpData.Controls.Add(ThermDewpoint, 2, 0)

            Log.Write("ThermDewpoint created and added to TlpData[2,0]")
        End If
    Catch ex As Exception
        Log.WriteException(ex, "Error creating ThermDewpoint")
    End Try
End If
```

### Step 1.3: Update Data Binding

**File:** `FrmMain.Partials\FrmMain.ObservationUI.vb`

**Find these update methods and add thermometer updates:**

**UpdateTemperature method - Add after TgCurrentTemp update:**
```vb
' Update thermometer if it exists
If ThermCurrentTemp IsNot Nothing Then
    ThermCurrentTemp.TempF = tempF
End If
```

**UpdateFeelsLike method - Add after TgFeelsLike update:**
```vb
' Update thermometer if it exists
If ThermFeelsLike IsNot Nothing Then
    ThermFeelsLike.TempF = feelsLikeTemp
End If
```

**UpdateDewpoint method - Add after TgDewpoint update:**
```vb
' Update thermometer if it exists
If ThermDewpoint IsNot Nothing Then
    ThermDewpoint.TempF = dewpointF
End If
```

### Step 1.4: Test Phase 1
1. Build solution
2. Run application
3. Verify thermometers appear in columns 0, 1, 2
4. Check temperature updates work
5. Verify old gauges still work if present

---

## PHASE 2: Barometer & Atmospheric Controls

### Step 2.1: Add Control Declarations

**File:** `FrmMain.Designer.vb`

**Add:**
```vb
Friend WithEvents BaroPressure As BarometerControl
Friend WithEvents AltAirDensity As AirDensityAltimeter
```

### Step 2.2: Add Barometer Initialization

**File:** `FrmMain.Partials\FrmMain.CustomControls.vb`

**Add at end of InitializeCustomControls(), before the Log.Write("Initialized custom controls.") line:**

```vb
' Barometer control
Log.Write("Creating BaroPressure as BarometerControl")
Try
    If TlpData IsNot Nothing Then
        BaroPressure = New Controls.BarometerControl With {
            .BackColor = _customBackColor,
            .Dock = DockStyle.Fill,
            .MinPressure = 28.5F,
            .MaxPressure = 31.0F,
            .ShowTrendText = True
        }

        ' Add to column 4-5, row 0 (span 2 columns)
        TlpData.Controls.Add(BaroPressure, 4, 0)
        TlpData.SetColumnSpan(BaroPressure, 2)

        Log.Write("BaroPressure created and added to TlpData[4,0] with ColumnSpan=2")
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error creating BaroPressure")
End Try

' Air Density Altimeter
Log.Write("Creating AltAirDensity as AirDensityAltimeter")
Try
    If TlpData IsNot Nothing Then
        AltAirDensity = New Controls.AirDensityAltimeter With {
            .BackColor = _customBackColor,
            .Dock = DockStyle.Fill
        }

        ' Add to column 6-7, row 1 (span 2 columns)
        TlpData.Controls.Add(AltAirDensity, 6, 1)
        TlpData.SetColumnSpan(AltAirDensity, 2)

        Log.Write("AltAirDensity created and added to TlpData[6,1] with ColumnSpan=2")
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error creating AltAirDensity")
End Try
```

### Step 2.3: Add Barometer Data Binding

**File:** Create new file `FrmMain.Partials\FrmMain.AtmosphericUI.vb`

```vb
Partial Public Class FrmMain

    Private Sub UpdateBarometer(pressureInHg As Single, trend As BarometerControl.PressureTrend)
        Try
            If BaroPressure IsNot Nothing Then
                BaroPressure.PressureInHg = pressureInHg
                BaroPressure.Trend = trend
            End If
        Catch ex As Exception
            Log.WriteException(ex, "UpdateBarometer")
        End Try
    End Sub

    Private Sub UpdateAirDensityAltimeter(densityKgM3 As Single, densityAltFt As Single, category As String)
        Try
            If AltAirDensity IsNot Nothing Then
                AltAirDensity.AirDensity = densityKgM3
                AltAirDensity.DensityAltitude = densityAltFt
                AltAirDensity.Category = category
            End If
        Catch ex As Exception
            Log.WriteException(ex, "UpdateAirDensityAltimeter")
        End Try
    End Sub

End Class
```

### Step 2.4: Call from Main Update Method

**File:** `FrmMain.Partials\FrmMain.ObservationUI.vb`

**In your main observation update method, add:**
```vb
' Update barometer (calculate 3-hour trend separately)
UpdateBarometer(stationPressureInHg, CalculatePressureTrend())

' Update air density altimeter
UpdateAirDensityAltimeter(airDensityKgM3, densityAltitude, airDensityCategory)
```

---

## PHASE 3: Lightning & Rain Enhancements

### Step 3.1: Add Control Declarations

**File:** `FrmMain.Designer.vb`

```vb
Friend WithEvents LightningRadar As LightningProximityRadar
Friend WithEvents RainRate As RainRateGauge
```

### Step 3.2: Initialize Controls

**File:** `FrmMain.Partials\FrmMain.CustomControls.vb`

**Add at end:**
```vb
' Lightning Proximity Radar
Log.Write("Creating LightningRadar as LightningProximityRadar")
Try
    If TlpData IsNot Nothing Then
        LightningRadar = New Controls.LightningProximityRadar With {
            .BackColor = _customBackColor,
            .Dock = DockStyle.Fill,
            .MaxDistance = 30.0F,
            .ShowRangeRings = True
        }

        ' Add to column 0-3, row 2 (span 4 columns for prominence)
        TlpData.Controls.Add(LightningRadar, 0, 2)
        TlpData.SetColumnSpan(LightningRadar, 4)

        Log.Write("LightningRadar created and added to TlpData[0,2] with ColumnSpan=4")
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error creating LightningRadar")
End Try

' Rain Rate Gauge
Log.Write("Creating RainRate as RainRateGauge")
Try
    If TlpData IsNot Nothing Then
        RainRate = New Controls.RainRateGauge With {
            .BackColor = _customBackColor,
            .Dock = DockStyle.Fill,
            .MaxRate = 2.0F,
            .ShowPeakMarker = True
        }

        ' Add to column 4-5, row 1 (span 2 columns, near precipitation data)
        TlpData.Controls.Add(RainRate, 4, 1)
        TlpData.SetColumnSpan(RainRate, 2)

        Log.Write("RainRate created and added to TlpData[4,1] with ColumnSpan=2")
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error creating RainRate")
End Try
```

### Step 3.3: Data Binding

**File:** `FrmMain.Partials\FrmMain.PrecipitationUI.vb` (create new)

```vb
Partial Public Class FrmMain

    Private Sub UpdateLightningRadar(distanceMiles As Single, strikeTime As DateTime)
        Try
            If LightningRadar IsNot Nothing Then
                LightningRadar.AddStrike(distanceMiles, strikeTime)
                LightningRadar.LastStrikeDistance = distanceMiles
                LightningRadar.LastStrikeTime = strikeTime
            End If
        Catch ex As Exception
            Log.WriteException(ex, "UpdateLightningRadar")
        End Try
    End Sub

    Private Sub UpdateRainRate(rateInchesPerHour As Single)
        Try
            If RainRate IsNot Nothing Then
                RainRate.RainRateInchesPerHour = rateInchesPerHour
            End If
        Catch ex As Exception
            Log.WriteException(ex, "UpdateRainRate")
        End Try
    End Sub

End Class
```

---

## PHASE 4: UV & Solar Meters

### Step 4.1: Add Control Declarations

**File:** `FrmMain.Designer.vb`

```vb
Friend WithEvents UvMeter As UVIndexMeter
Friend WithEvents SolarMeter As SolarEnergyMeter
```

### Step 4.2: Initialize Controls

**File:** `FrmMain.Partials\FrmMain.CustomControls.vb`

```vb
' UV Index Meter
Log.Write("Creating UvMeter as UVIndexMeter")
Try
    If TlpData IsNot Nothing Then
        UvMeter = New Controls.UVIndexMeter With {
            .BackColor = _customBackColor,
            .Dock = DockStyle.Fill,
            .MaxUVIndex = 12.0F,
            .ShowSunIcon = True,
            .ShowRecommendations = True
        }

        ' Add to column 4-5, row 2
        TlpData.Controls.Add(UvMeter, 4, 2)
        TlpData.SetColumnSpan(UvMeter, 2)

        Log.Write("UvMeter created and added to TlpData[4,2] with ColumnSpan=2")
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error creating UvMeter")
End Try

' Solar Energy Meter
Log.Write("Creating SolarMeter as SolarEnergyMeter")
Try
    If TlpData IsNot Nothing Then
        SolarMeter = New Controls.SolarEnergyMeter With {
            .BackColor = _customBackColor,
            .Dock = DockStyle.Fill,
            .MaxRadiation = 1200.0F
        }

        ' Add to column 6-7, row 2
        TlpData.Controls.Add(SolarMeter, 6, 2)
        TlpData.SetColumnSpan(SolarMeter, 2)

        Log.Write("SolarMeter created and added to TlpData[6,2] with ColumnSpan=2")
    End If
Catch ex As Exception
    Log.WriteException(ex, "Error creating SolarMeter")
End Try
```

### Step 4.3: Data Binding

**File:** `FrmMain.Partials\FrmMain.SolarUI.vb` (create new)

```vb
Partial Public Class FrmMain

    Private Sub UpdateUVMeter(uvIndex As Single)
        Try
            If UvMeter IsNot Nothing Then
                UvMeter.UVIndex = uvIndex
            End If
        Catch ex As Exception
            Log.WriteException(ex, "UpdateUVMeter")
        End Try
    End Sub

    Private Sub UpdateSolarMeter(solarRadiationWm2 As Single)
        Try
            If SolarMeter IsNot Nothing Then
                SolarMeter.SolarRadiation = solarRadiationWm2
            End If
        Catch ex As Exception
            Log.WriteException(ex, "UpdateSolarMeter")
        End Try
    End Sub

End Class
```

---

## PHASE 5: Optional System Controls

### Step 5.1: Status LED Panel

**Declaration:**
```vb
Friend WithEvents StatusLeds As StatusLEDPanel
```

**Initialization:**
```vb
StatusLeds = New Controls.StatusLEDPanel With {
    .BackColor = _customBackColor,
    .Dock = DockStyle.Fill
}
TlpData.Controls.Add(StatusLeds, 0, 3)
TlpData.SetColumnSpan(StatusLeds, 2)
```

**Usage:**
```vb
StatusLeds.SetStatus("UDP Active", StatusLEDPanel.LEDStatus.Green)
StatusLeds.SetStatus("Battery Good", StatusLEDPanel.LEDStatus.Yellow)
StatusLeds.SetStatus("Data Fresh", StatusLEDPanel.LEDStatus.Red)
```

### Step 5.2: Trend Arrows

**Declaration:**
```vb
Friend WithEvents TrendArrows As TrendArrowsControl
```

**Initialization:**
```vb
TrendArrows = New Controls.TrendArrowsControl With {
    .BackColor = _customBackColor,
    .Dock = DockStyle.Fill
}
TlpData.Controls.Add(TrendArrows, 2, 3)
TlpData.SetColumnSpan(TrendArrows, 2)
```

**Usage:**
```vb
TrendArrows.SetTrend("Temp", 1)     ' Rising
TrendArrows.SetTrend("Press", -1)   ' Falling
TrendArrows.SetTrend("Humid", 0)    ' Steady
TrendArrows.SetTrend("Wind", 2)     ' Rising rapidly
```

---

## Final Layout After All Phases

```
┌──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┐
│Therm │Therm │Therm │ FgRH │ Baro │ Baro │ PTC  │ PTC  │ R0
│Curr  │Feels │ Dew  │      │      │      │      │      │
├──────┴──────┴──────┴──────┼──────┴──────┼──────┴──────┤
│ WrWindSpeed (Compass)     │ RainRate    │ AltAirDen   │ R1
│                           │             │             │
├──────┴──────┴──────┴──────┼──────┬──────┼──────┬──────┤
│ LightningRadar            │ UvMtr│ UvMtr│SolarM│SolarM│ R2
│                           │      │      │      │      │
└───────────────────────────┴──────┴──────┴──────┴──────┘
```

---

## Testing Checklist

- [ ] Phase 1: Thermometers display and update correctly
- [ ] Phase 2: Barometer shows pressure and trends
- [ ] Phase 2: Air density altimeter displays values
- [ ] Phase 3: Lightning radar shows strikes
- [ ] Phase 3: Rain rate gauge animates
- [ ] Phase 4: UV meter shows color zones
- [ ] Phase 4: Solar meter displays radiation
- [ ] No crashes or exceptions in logs
- [ ] All old data still updates correctly
- [ ] Layout looks balanced and professional

---

## Troubleshooting

### Control Not Showing
1. Check logs for initialization errors
2. Verify TlpData is not Nothing
3. Check column/row spans don't overlap
4. Verify control is added to correct position

### Data Not Updating
1. Check that control reference is not Nothing
2. Verify update method is being called
3. Check data type conversions
4. Look for exceptions in logs

### Build Errors
1. Ensure all controls project is built first
2. Check TempestDisplay.Controls.dll is in output
3. Verify Imports statement includes TempestDisplay.Controls
4. Clean and rebuild solution

---

**This guide provides everything needed to implement all new controls safely and systematically!**
