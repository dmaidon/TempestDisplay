Partial Public Class FrmMain

Private ReadOnly _customBackColor As Color = Color.AntiqueWhite

''' <summary>
''' Layout position constants for TlpData TableLayoutPanel
''' Makes layout changes easier to maintain and understand
''' </summary>
Private Structure TlpDataLayout
    ' Temperature Thermometers (Row 0)
    Public Const ThermCurrentTempCol As Integer = 0
    Public Const ThermCurrentTempRow As Integer = 0
    Public Const ThermFeelsLikeCol As Integer = 1
    Public Const ThermFeelsLikeRow As Integer = 0
    Public Const ThermDewpointCol As Integer = 2
    Public Const ThermDewpointRow As Integer = 0

    ' Humidity Gauge (Row 0, spans 2 columns)
    Public Const HumidityGaugeCol As Integer = 1
    Public Const HumidityGaugeRow As Integer = 0
    Public Const HumidityGaugeColSpan As Integer = 2

    ' Barometer (Row 0, spans 2 columns)
    Public Const BarometerCol As Integer = 4
    Public Const BarometerRow As Integer = 0
    Public Const BarometerColSpan As Integer = 2

    ' Precipitation Towers (Row 1, spans 2 columns)
    Public Const PrecipTowersCol As Integer = 2
    Public Const PrecipTowersRow As Integer = 1
    Public Const PrecipTowersColSpan As Integer = 2

    ' Rain Rate Gauge (Row 1)
    Public Const RainRateCol As Integer = 4
    Public Const RainRateRow As Integer = 1
    Public Const RainRateColSpan As Integer = 1

    ' Wind Rose (Row 1, spans 2 columns)
    Public Const WindRoseCol As Integer = 6
    Public Const WindRoseRow As Integer = 1
    Public Const WindRoseColSpan As Integer = 2

    ' Air Density Altimeter (Row 1, spans 2 columns)
    Public Const AirDensityCol As Integer = 6
    Public Const AirDensityRow As Integer = 1
    Public Const AirDensityColSpan As Integer = 2

    ' Lightning Radar (Row 1, spans 2 columns)
    Public Const LightningRadarCol As Integer = 4
    Public Const LightningRadarRow As Integer = 2
    Public Const LightningRadarColSpan As Integer = 2

    ' Solar/UV Combined (Row 0, spans 2 columns)
    Public Const SolarUvCol As Integer = 6
    Public Const SolarUvRow As Integer = 0
    Public Const SolarUvColSpan As Integer = 2

    ' Trend/Status Combined (Row 2, spans 2 columns)
    Public Const TrendStatusCol As Integer = 0
    Public Const TrendStatusRow As Integer = 2
    Public Const TrendStatusColSpan As Integer = 2

    ' Sunrise/Sunset Panel (Row 2, spans 2 columns)
    Public Const SunriseSunsetCol As Integer = 2
    Public Const SunriseSunsetRow As Integer = 2
    Public Const SunriseSunsetColSpan As Integer = 2

    ' Cloud Base Panel (Row 2)
    Public Const CloudBaseCol As Integer = 6
    Public Const CloudBaseRow As Integer = 2

    ' Battery Voltage (Row 2)
    Public Const BatteryVoltageCol As Integer = 7
    Public Const BatteryVoltageRow As Integer = 2
End Structure

' New: hold a reference to the combined UV+Solar control so other partials can update it
Friend SolarUvCombined As Controls.SolarUvCombinedMeter

' New: hold a reference to combined Trend + Status control
Friend TrendStatusCombined As Controls.TrendStatusCombinedControl

' New: Sunrise/Sunset panel
Friend SunriseSunset As Controls.SunriseSunsetPanel

' New: Replace FanGaugeControl with HumidityComfortGauge
Friend HumidityGauge As Controls.HumidityComfortGauge

' New: Cloud base panel
Friend CloudBase As Controls.CloudBasePanel

' New: Battery voltage display
Friend BatteryVoltage As Controls.BatteryVoltageControl

''' <summary>
''' Helper method to configure thermometer controls with consistent settings
''' </summary>
Private Sub ConfigureThermometer(therm As Controls.TempThermometerControl,
                                 label As String,
                                 minF As Single,
                                 maxF As Single,
                                 showFreezeMarker As Boolean)
    If therm Is Nothing Then
        Log.Write($"WARNING: {label} thermometer is Nothing!")
        Return
    End If

    therm.Label = label
    therm.BackColor = _customBackColor
    therm.Font = New Font(If(therm.Font, SystemFonts.DefaultFont), FontStyle.Bold)
    therm.MinF = minF
    therm.MaxF = maxF
    therm.ShowFreezeMarker = showFreezeMarker
    therm.ShowDualScale = True
End Sub

    Private Sub InitializeCustomcontrols()

        Try
            ' Temperature thermometers (Phase 1 - simplified with helper method)
            If ThermCurrentTemp IsNot Nothing Then
                Log.Write("ThermCurrentTemp control exists - configuring properties")
                ConfigureThermometer(ThermCurrentTemp, "Current", -5, 110, True)
                Log.Write($"ThermCurrentTemp configured - Visible:{ThermCurrentTemp.Visible}, Parent:{If(ThermCurrentTemp.Parent IsNot Nothing, ThermCurrentTemp.Parent.Name, "None")}")
            Else
                Log.Write("ThermCurrentTemp control is Nothing - creating dynamically")
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
                        ThermCurrentTemp.Font = New Font(ThermCurrentTemp.Font, FontStyle.Bold)
                        TlpData.Controls.Add(ThermCurrentTemp, TlpDataLayout.ThermCurrentTempCol, TlpDataLayout.ThermCurrentTempRow)
                        Log.Write($"ThermCurrentTemp created and added to TlpData[{TlpDataLayout.ThermCurrentTempCol},{TlpDataLayout.ThermCurrentTempRow}]")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add ThermCurrentTemp")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating ThermCurrentTemp control dynamically")
                End Try
            End If

            If ThermFeelsLike IsNot Nothing Then
                ConfigureThermometer(ThermFeelsLike, "Feels Like", -10, 120, True)
            Else
                Log.Write("ThermFeelsLike control is Nothing - creating dynamically")
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
                        ThermFeelsLike.Font = New Font(ThermFeelsLike.Font, FontStyle.Bold)
                        TlpData.Controls.Add(ThermFeelsLike, TlpDataLayout.ThermFeelsLikeCol, TlpDataLayout.ThermFeelsLikeRow)
                        Log.Write($"ThermFeelsLike created and added to TlpData[{TlpDataLayout.ThermFeelsLikeCol},{TlpDataLayout.ThermFeelsLikeRow}]")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add ThermFeelsLike")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating ThermFeelsLike control dynamically")
                End Try
            End If

            If ThermDewpoint IsNot Nothing Then
                ConfigureThermometer(ThermDewpoint, "Dew Point", -5, 110, False)
            Else
                Log.Write("ThermDewpoint control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        ThermDewpoint = New Controls.TempThermometerControl With {
                            .Label = "Dew Point",
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .MinF = -5,
                            .MaxF = 110,
                            .ShowFreezeMarker = False,
                            .ShowDualScale = True
                        }
                        ThermDewpoint.Font = New Font(ThermDewpoint.Font, FontStyle.Bold)
                        TlpData.Controls.Add(ThermDewpoint, TlpDataLayout.ThermDewpointCol, TlpDataLayout.ThermDewpointRow)
                        Log.Write($"ThermDewpoint created and added to TlpData[{TlpDataLayout.ThermDewpointCol},{TlpDataLayout.ThermDewpointRow}]")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add ThermDewpoint")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating ThermDewpoint control dynamically")
                End Try
            End If

            ' Humidity gauge - replace FanGaugeControl with HumidityComfortGauge
            Try
                ' Remove old FanGaugeControl if present
                If FgRH IsNot Nothing AndAlso FgRH.Parent Is TlpData Then
                    TlpData.Controls.Remove(FgRH)
                    FgRH.Dispose()
                    FgRH = Nothing
                    Log.Write("Removed existing FanGaugeControl (FgRH) from layout")
                End If

                If HumidityGauge Is Nothing Then
                    If TlpData IsNot Nothing Then
                        HumidityGauge = New Controls.HumidityComfortGauge With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        ' Place humidity gauge near thermometers
                        TlpData.Controls.Add(HumidityGauge, TlpDataLayout.HumidityGaugeCol, TlpDataLayout.HumidityGaugeRow)
                        TlpData.SetColumnSpan(HumidityGauge, TlpDataLayout.HumidityGaugeColSpan)
                        Log.Write($"HumidityComfortGauge created and added to TlpData[{TlpDataLayout.HumidityGaugeCol},{TlpDataLayout.HumidityGaugeRow}] with ColumnSpan={TlpDataLayout.HumidityGaugeColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add HumidityComfortGauge")
                    End If
                Else
                    HumidityGauge.BackColor = _customBackColor
                    HumidityGauge.Dock = DockStyle.Fill
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error creating HumidityComfortGauge dynamically")
            End Try

            ' Precipitation control
            If PTC IsNot Nothing Then
                PTC.BackColor = _customBackColor
                PTC.Font = New Font("Arial", 7.75, FontStyle.Bold)
            Else
                ' Create PTC control dynamically if it doesn't exist
                Log.Write("PTC control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        PTC = New Controls.PrecipTowersControl With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        PTC.Font = New Font(PTC.Font.FontFamily, 8, FontStyle.Bold)

                        ' Add to TableLayoutPanel with layout constants
                        TlpData.Controls.Add(PTC, TlpDataLayout.PrecipTowersCol, TlpDataLayout.PrecipTowersRow)
                        TlpData.SetColumnSpan(PTC, TlpDataLayout.PrecipTowersColSpan)

                        Log.Write($"PTC control created and added to TlpData[{TlpDataLayout.PrecipTowersCol},{TlpDataLayout.PrecipTowersRow}] with ColumnSpan={TlpDataLayout.PrecipTowersColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add PTC")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating PTC control dynamically")
                End Try
            End If

            If WrWindSpeed IsNot Nothing Then
                WrWindSpeed.BackColor = _customBackColor
                WrWindSpeed.Font = New Font(WrWindSpeed.Font, FontStyle.Bold)
                WrWindSpeed.Label = "Wind"
            Else
                Log.Write("WrWindSpeed control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        WrWindSpeed = New Controls.WindRoseControl With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .Label = "Wind"
                        }
                        WrWindSpeed.Font = New Font(WrWindSpeed.Font.FontFamily, 8, FontStyle.Bold)
                        ' Add to TableLayoutPanel with layout constants
                        TlpData.Controls.Add(WrWindSpeed, TlpDataLayout.WindRoseCol, TlpDataLayout.WindRoseRow)
                        TlpData.SetColumnSpan(WrWindSpeed, TlpDataLayout.WindRoseColSpan)
                        Log.Write($"WrWindSpeed control created and added to TlpData[{TlpDataLayout.WindRoseCol},{TlpDataLayout.WindRoseRow}] with ColumnSpan={TlpDataLayout.WindRoseColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add WrWindSpeed")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating WrWindSpeed control dynamically")
                End Try
            End If

            ' Phase 2: Barometer (Atmospheric Pressure)
            If BaroPressure IsNot Nothing Then
                BaroPressure.BackColor = _customBackColor
                BaroPressure.Font = New Font(BaroPressure.Font, FontStyle.Bold)
            Else
                Log.Write("BaroPressure control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        BaroPressure = New Controls.BarometerControl With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        BaroPressure.Font = New Font(BaroPressure.Font, FontStyle.Bold)
                        TlpData.Controls.Add(BaroPressure, TlpDataLayout.BarometerCol, TlpDataLayout.BarometerRow)
                        TlpData.SetColumnSpan(BaroPressure, TlpDataLayout.BarometerColSpan)
                        Log.Write($"BaroPressure created and added to TlpData[{TlpDataLayout.BarometerCol},{TlpDataLayout.BarometerRow}] with ColumnSpan={TlpDataLayout.BarometerColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add BaroPressure")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating BarometerControl dynamically")
                End Try
            End If

            ' Phase 2: Air Density Altimeter
            If AltAirDensity IsNot Nothing Then
                AltAirDensity.BackColor = _customBackColor
                AltAirDensity.Font = New Font(AltAirDensity.Font, FontStyle.Bold)
            Else
                Log.Write("AltAirDensity control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        AltAirDensity = New Controls.AirDensityAltimeter With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        AltAirDensity.Font = New Font(AltAirDensity.Font, FontStyle.Bold)
                        TlpData.Controls.Add(AltAirDensity, TlpDataLayout.AirDensityCol, TlpDataLayout.AirDensityRow)
                        TlpData.SetColumnSpan(AltAirDensity, TlpDataLayout.AirDensityColSpan)
                        Log.Write($"AltAirDensity created and added to TlpData[{TlpDataLayout.AirDensityCol},{TlpDataLayout.AirDensityRow}] with ColumnSpan={TlpDataLayout.AirDensityColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add AltAirDensity")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating AirDensityAltimeter dynamically")
                End Try
            End If

            ' Phase 3: Rain Rate Gauge
            If RainRate IsNot Nothing Then
                RainRate.BackColor = _customBackColor
                RainRate.Font = New Font(RainRate.Font, FontStyle.Bold)
            Else
                Log.Write("RainRate control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        RainRate = New Controls.RainRateGauge With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        RainRate.Font = New Font(RainRate.Font, FontStyle.Bold)
                        TlpData.Controls.Add(RainRate, TlpDataLayout.RainRateCol, TlpDataLayout.RainRateRow)
                        TlpData.SetColumnSpan(RainRate, TlpDataLayout.RainRateColSpan)
                        Log.Write($"RainRate created and added to TlpData[{TlpDataLayout.RainRateCol},{TlpDataLayout.RainRateRow}] with ColumnSpan={TlpDataLayout.RainRateColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add RainRate")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating RainRate control dynamically")
                End Try
            End If

            ' Phase 3: Lightning Proximity Radar
            If LightningRadar IsNot Nothing Then
                LightningRadar.BackColor = _customBackColor
                LightningRadar.Font = New Font(LightningRadar.Font, FontStyle.Bold)
            Else
                Log.Write("LightningRadar control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        LightningRadar = New Controls.LightningProximityRadar With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        LightningRadar.Font = New Font(LightningRadar.Font, FontStyle.Bold)
                        TlpData.Controls.Add(LightningRadar, TlpDataLayout.LightningRadarCol, TlpDataLayout.LightningRadarRow)
                        TlpData.SetColumnSpan(LightningRadar, TlpDataLayout.LightningRadarColSpan)
                        Log.Write($"LightningRadar created and added to TlpData[{TlpDataLayout.LightningRadarCol},{TlpDataLayout.LightningRadarRow}] with ColumnSpan={TlpDataLayout.LightningRadarColSpan}")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add LightningRadar")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating LightningProximityRadar dynamically")
                End Try
            End If

            ' Phase 4: Combined UV + Solar Energy Meter
            ' Remove existing individual meters from the layout if present
            Try
                If UvMeter IsNot Nothing AndAlso UvMeter.Parent Is TlpData Then
                    TlpData.Controls.Remove(UvMeter)
                    UvMeter.Dispose()
                    UvMeter = Nothing
                    Log.Write("Removed existing UvIndexMeter from layout")
                End If
                If SolarMeter IsNot Nothing AndAlso SolarMeter.Parent Is TlpData Then
                    TlpData.Controls.Remove(SolarMeter)
                    SolarMeter.Dispose()
                    SolarMeter = Nothing
                    Log.Write("Removed existing SolarEnergyMeter from layout")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error removing existing UV/Solar controls")
            End Try

            ' Create and install combined control at the original UV position (column 2, row 2)
            Try
                If TlpData IsNot Nothing Then
                    If SolarUvCombined Is Nothing Then
                        SolarUvCombined = New Controls.SolarUvCombinedMeter With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .Font = New Font(Me.Font, FontStyle.Bold)
                        }
                        TlpData.Controls.Add(SolarUvCombined, 2, 2)
                        TlpData.SetColumnSpan(SolarUvCombined, 2)
                        Log.Write("SolarUvCombinedMeter created and added to TlpData[2,2] with ColumnSpan=2")
                    Else
                        SolarUvCombined.BackColor = _customBackColor
                        SolarUvCombined.Font = New Font(Me.Font, FontStyle.Bold)
                    End If
                Else
                    Log.Write("WARNING: TlpData is Nothing - cannot add SolarUvCombinedMeter")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error creating SolarUvCombinedMeter dynamically")
            End Try

            ' Phase 5: Combined Trend Arrows + Status LED Panel
            ' Remove existing separate controls from layout if present
            Try
                If TrendArrows IsNot Nothing AndAlso TrendArrows.Parent Is TlpData Then
                    TlpData.Controls.Remove(TrendArrows)
                    TrendArrows.Dispose()
                    TrendArrows = Nothing
                    Log.Write("Removed existing TrendArrows control from layout")
                End If
                If StatusLeds IsNot Nothing AndAlso StatusLeds.Parent Is TlpData Then
                    TlpData.Controls.Remove(StatusLeds)
                    StatusLeds.Dispose()
                    StatusLeds = Nothing
                    Log.Write("Removed existing StatusLEDPanel from layout")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error removing existing Trend/Status controls")
            End Try

            ' Create and install the combined Trend/Status control at the prior TrendArrows slot (3,0)
            Try
                If TlpData IsNot Nothing Then
                    If TrendStatusCombined Is Nothing Then
                        TrendStatusCombined = New Controls.TrendStatusCombinedControl With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .Font = New Font(Me.Font, FontStyle.Bold)
                        }
                        TlpData.Controls.Add(TrendStatusCombined, 3, 0)
                        TlpData.SetColumnSpan(TrendStatusCombined, 1)
                        Log.Write("TrendStatusCombinedControl created and added to TlpData[3,0]")
                    Else
                        TrendStatusCombined.BackColor = _customBackColor
                        TrendStatusCombined.Font = New Font(Me.Font, FontStyle.Bold)
                    End If
                Else
                    Log.Write("WARNING: TlpData is Nothing - cannot add TrendStatusCombinedControl")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error creating TrendStatusCombinedControl dynamically")
            End Try

            ' New: Sunrise/Sunset panel at column 5, row 1
            Try
                If TlpData IsNot Nothing Then
                    If SunriseSunset Is Nothing Then
                        SunriseSunset = New Controls.SunriseSunsetPanel With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .Font = New Font(Me.Font, FontStyle.Bold),
                            .Tag = "40.7128,-74.0060" ' TODO: set to station lat,lng
                        }
                        TlpData.Controls.Add(SunriseSunset, 5, 1)
                        Log.Write("SunriseSunsetPanel created and added to TlpData[5,1]")
                    Else
                        SunriseSunset.BackColor = _customBackColor
                        SunriseSunset.Font = New Font(Me.Font, FontStyle.Bold)
                        Dim tagStr As String = SunriseSunset.Tag?.ToString()
                        If String.IsNullOrWhiteSpace(tagStr) Then
                            SunriseSunset.Tag = "40.7128,-74.0060"
                        End If
                    End If
                Else
                    Log.Write("WARNING: TlpData is Nothing - cannot add SunriseSunsetPanel")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error creating SunriseSunsetPanel dynamically")
            End Try

            ' New: Cloud base panel to fill a single cell
            Try
                If CloudBase Is Nothing Then
                    CloudBase = New Controls.CloudBasePanel With {
                        .BackColor = _customBackColor,
                        .Dock = DockStyle.Fill,
                        .Margin = New Padding(0)
                    }
                    If TlpData IsNot Nothing Then
                        TlpData.Controls.Add(CloudBase, 6, 2)
                        Log.Write("CloudBasePanel created and added to TlpData[6,2]")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add CloudBasePanel")
                    End If
                Else
                    CloudBase.BackColor = _customBackColor
                    CloudBase.Dock = DockStyle.Fill
                    CloudBase.Margin = New Padding(0)
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error creating CloudBasePanel dynamically")
            End Try

            ' New: Battery voltage display at TlpData[7,2]
            Try
                If BatteryVoltage Is Nothing Then
                    BatteryVoltage = New Controls.BatteryVoltageControl With {
                        .BackColor = _customBackColor,
                        .Dock = DockStyle.Fill,
                        .Margin = New Padding(0),
                        .Voltage = 2.6F
                    }
                    If TlpData IsNot Nothing Then
                        TlpData.Controls.Add(BatteryVoltage, 7, 2)
                        Log.Write("BatteryVoltageControl created and added to TlpData[7,2]")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add BatteryVoltageControl")
                    End If
                Else
                    BatteryVoltage.BackColor = _customBackColor
                    BatteryVoltage.Dock = DockStyle.Fill
                    BatteryVoltage.Margin = New Padding(0)
                End If
            Catch ex As Exception
                Log.WriteException(ex, "Error creating BatteryVoltageControl dynamically")
            End Try

            Log.Write("Initialized custom controls.")
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize custom controls")
        End Try
    End Sub

End Class