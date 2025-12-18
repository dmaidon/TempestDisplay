Partial Public Class FrmMain

    ''' <summary>
    ''' Update all UI controls with observation data
    ''' </summary>
    Private Sub UpdateWeatherUI(data As ObservationData, rawJson As String)
        Try
            ' Display raw packet in status label
            If TsslObs_St IsNot Nothing Then
                TsslObs_St.Text = rawJson
            End If

            ' Update temperature gauges
            UpdateTemperatureGauges(data)

            UpdateCurTempLabel(data)
            UpdateCurWindslabel(data)
            UpdateWindRoseControl(data)

            ' Update wind displays
            UpdateWindDisplays(data)

            ' Update atmospheric readings (labels + barometer gauge)
            UpdateAtmosphericReadings(data)

            ' Update light/UV displays
            UpdateLightDisplays(data)

            ' Update lightning displays
            UpdateLightningDisplays(data)

            ' Update air density
            UpdateAirDensity(data)

            ' Update cloud base
            UpdateCloudBase(data)

            ' Track temp/humidity/wind histories for trend display
            AddTemperatureReading(data.TempF, data.TimestampDateTime)
            AddHumidityReading(data.Humidity, data.TimestampDateTime)
            AddWindReading(data.WindAvgMph, data.TimestampDateTime)

            Dim tTrend = CalculateTempTrend(data.TempF, data.TimestampDateTime)
            Dim hTrend = CalculateHumidityTrend(data.Humidity, data.TimestampDateTime)
            Dim wTrend = CalculateWindTrend(data.WindAvgMph, data.TimestampDateTime)

            ' Update combined trend control if available
            If TrendStatusCombined IsNot Nothing Then
                Dim ToDir As Integer = If(Not tTrend.HasData, 0, If(tTrend.Delta > 0, 1, If(tTrend.Delta < 0, -1, 0)))
                Dim HoDir As Integer = If(Not hTrend.HasData, 0, If(hTrend.Delta > 0, 1, If(hTrend.Delta < 0, -1, 0)))
                Dim WoDir As Integer = If(Not wTrend.HasData, 0, If(wTrend.Delta > 0, 1, If(wTrend.Delta < 0, -1, 0)))
                TrendStatusCombined.SetTrend("Temp", ToDir)
                TrendStatusCombined.SetTrend("Humid", HoDir)
                TrendStatusCombined.SetTrend("Wind", WoDir)
            ElseIf TrendArrows IsNot Nothing Then
                Dim ToDir As Integer = If(Not tTrend.HasData, 0, If(tTrend.Delta > 0, 1, If(tTrend.Delta < 0, -1, 0)))
                Dim HoDir As Integer = If(Not hTrend.HasData, 0, If(hTrend.Delta > 0, 1, If(hTrend.Delta < 0, -1, 0)))
                Dim WoDir As Integer = If(Not wTrend.HasData, 0, If(wTrend.Delta > 0, 1, If(wTrend.Delta < 0, -1, 0)))
                TrendArrows.SetTrend("Temp", ToDir)
                TrendArrows.SetTrend("Humid", HoDir)
                TrendArrows.SetTrend("Wind", WoDir)
            End If

            ' Update sunrise/sunset panel (requires coordinates in Tag as "lat,lng")
            UpdateSunriseSunsetPanel()

            ' Update battery status
            UpdateBatteryStatus(data.Battery)

            ' Update last update time
            If LblUpdate IsNot Nothing Then
                LblUpdate.Text = $"Last Update: {data.TimestampDateTime:h:mm:ss tt}"
            End If

            Log.Write($"[UDP] Weather: {data.TempF:F1}°F, {data.Humidity}% RH, {data.PressureInHg:F2} inHg, Wind: {data.WindAvgMph:F1} mph @ {data.WindDirection}°")
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error in UpdateWeatherUI")
        End Try
    End Sub

    Private Shared ReadOnly separator As Char() = {","c}

    Private Sub UpdateSunriseSunsetPanel()
        Try
            If SunriseSunset Is Nothing Then Return
            ' Expect Tag like "lat,lng" (e.g., "40.7128,-74.0060")
            Dim coords As String = TryCast(SunriseSunset.Tag, String)
            If String.IsNullOrWhiteSpace(coords) Then
                ' No coordinates provided; skip fetch
                Exit Sub
            End If
            Dim parts = coords.Split(separator, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length <> 2 Then Exit Sub
            'Dim lat As Double
            'Dim lng As Double
            If Not Double.TryParse(parts(0).Trim(), Lat) Then Exit Sub
            If Not Double.TryParse(parts(1).Trim(), Lng) Then Exit Sub

            Dim tz As String = TimeZone

            ' Fire and forget: start task and ignore the result
            Dim _task = SunriseSunset.FetchAndUpdateAsync(Lat, Lng, tz)
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating SunriseSunset panel")
        End Try
    End Sub

    Private Sub UpdateWindRoseControl(data As ObservationData)
        Try
            If WrWindSpeed IsNot Nothing Then
                WrWindSpeed.WindSpeed = CSng(data.WindAvgMph)
                WrWindSpeed.WindDirection = CSng(data.WindDirection)
                WrWindSpeed.Refresh()
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating wind rose control")
        End Try
    End Sub

    Private Sub UpdateCurTempLabel(data As ObservationData)
        If LblCurTemp IsNot Nothing Then
            LblCurTemp.Font = New Font(LblCurTemp.Font.FontFamily, 36, FontStyle.Bold)
            LblCurTemp.ForeColor = Color.Maroon
            LblCurTemp.Text = $"{data.TempF:F1} °F"
        End If
    End Sub

    Private Sub UpdateCurWindslabel(data As ObservationData)
        If LblCurWinds IsNot Nothing Then
            LblCurWinds.Font = New Font(LblCurWinds.Font.FontFamily, 20, FontStyle.Bold)
            LblCurWinds.ForeColor = Color.Green
            LblCurWinds.Text = $"{data.WindAvgMph:F1} mph {DegreesToCardinal(data.WindDirection)}"
        End If
    End Sub

    ''' <summary>
    ''' Update temperature gauge/thermometer controls
    ''' </summary>
    Private Sub UpdateTemperatureGauges(data As ObservationData)
        Try
            ' Calculate derived values
            Dim dewPoint = CalculateDewPoint(data.TempF, data.Humidity)
            Dim feelsLike = CalculateFeelsLike(data.TempF, data.Humidity, data.WindAvgMph)

            ' Update thermometers (Phase 1 - new controls)
            If ThermCurrentTemp IsNot Nothing Then
                ThermCurrentTemp.TempF = CSng(data.TempF)
                Log.Write($"[UI] Updated ThermCurrentTemp.TempF = {data.TempF:F1}°F")
            ElseIf TgCurrentTemp IsNot Nothing Then
                ' Fallback to old gauge if thermometer not available
                TgCurrentTemp.TempF = CSng(data.TempF)
                Log.Write($"[UI] Updated TgCurrentTemp.TempF = {data.TempF:F1}°F (fallback)")
            Else
                Log.Write("[UI] WARNING: Both ThermCurrentTemp and TgCurrentTemp are Nothing!")
            End If

            If ThermFeelsLike IsNot Nothing Then
                ThermFeelsLike.TempF = CSng(feelsLike)

                ' Set background color based on which feels-like metric is active
                Dim label = GetFeelsLikeLabel(data.TempF, data.WindAvgMph)
                Select Case label
                    Case "Heat Index"
                        ThermFeelsLike.BackColor = Color.MistyRose   ' light red for hot
                    Case "Wind Chill"
                        ThermFeelsLike.BackColor = Color.LightBlue   ' light blue for cold
                    Case Else
                        ThermFeelsLike.BackColor = Color.AntiqueWhite ' neutral for comfortable range
                End Select
            ElseIf TgFeelsLike IsNot Nothing Then
                ' Fallback to old gauge
                TgFeelsLike.TempF = CSng(feelsLike)
                Dim label = GetFeelsLikeLabel(data.TempF, data.WindAvgMph)
                Select Case label
                    Case "Heat Index"
                        TgFeelsLike.BackColor = Color.MistyRose
                    Case "Wind Chill"
                        TgFeelsLike.BackColor = Color.LightBlue
                    Case Else
                        TgFeelsLike.BackColor = Color.AntiqueWhite
                End Select
            End If

            If ThermDewpoint IsNot Nothing Then
                ThermDewpoint.TempF = CSng(dewPoint)
            ElseIf TgDewpoint IsNot Nothing Then
                ' Fallback to old gauge
                TgDewpoint.TempF = CSng(dewPoint)
            End If

            ' Update humidity gauge
            If FgRH IsNot Nothing Then
                FgRH.Value = CSng(data.Humidity)
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating temperature gauges")
        End Try
    End Sub

    ''' <summary>
    ''' Update wind display labels
    ''' </summary>
    Private Sub UpdateWindDisplays(data As ObservationData)
        Try
            If LblAvgWindSpd IsNot Nothing Then
                LblAvgWindSpd.Text = $"Wind Average Speed: {data.WindAvgMph:F1} mph"
            End If

            If LblWindGust IsNot Nothing Then
                LblWindGust.Text = $"Wind Gust: {data.WindGustMph:F1} mph"
            End If

            If LblWindLull IsNot Nothing Then
                LblWindLull.Text = $"Wind Lull: {data.WindLullMph:F1} mph"
            End If

            If LblWindDir IsNot Nothing Then
                LblWindDir.Text = $"Wind Direction: {data.WindDirection}°  {DegreesToCardinal(data.WindDirection)}"
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating wind displays")
        End Try
    End Sub

    ''' <summary>
    ''' Update atmospheric pressure and trend displays
    ''' </summary>
    Private Sub UpdateAtmosphericReadings(data As ObservationData)
        Try
            Dim pressureInHg As Double = TempestDisplay.Common.Weather.UnitConversions.MillibarsToInHg(data.Pressure)

            If LblBaroPress IsNot Nothing Then
                LblBaroPress.Text = $"Barometric Pressure: {pressureInHg:F2} inHg"
            End If

            If BaroPressure IsNot Nothing Then
                BaroPressure.PressureInHg = CSng(pressureInHg)
            End If

            AddPressureReading(data.Pressure, data.TimestampDateTime)
            Dim trendResult = CalculatePressureTrend(data.Pressure, data.TimestampDateTime)

            If LblPressTrend IsNot Nothing Then
                If trendResult.HasData Then
                    Dim deltaSign = If(trendResult.Delta >= 0, "+", "")
                    LblPressTrend.Text = $"Pressure Trend: {trendResult.Trend} ({deltaSign}{trendResult.Delta:F2} mb/3hr)"
                    Select Case trendResult.Trend
                        Case "Falling"
                            LblPressTrend.ForeColor = Color.Blue
                        Case "Rising"
                            LblPressTrend.ForeColor = Color.Red
                        Case "Steady"
                            LblPressTrend.ForeColor = Color.Green
                    End Select
                Else
                    LblPressTrend.Text = "Pressure Trend: (collecting data...)"
                    LblPressTrend.ForeColor = SystemColors.ControlText
                End If
            End If

            ' Hook Trend arrows (via combined control if available)
            If TrendStatusCombined IsNot Nothing Then
                Dim pressDir As Integer = 0
                If trendResult.HasData Then
                    pressDir = If(trendResult.Trend = "Rising", 1, If(trendResult.Trend = "Falling", -1, 0))
                End If
                TrendStatusCombined.SetTrend("Press", pressDir)
                TrendStatusCombined.SetTrend("Temp", 0)
                TrendStatusCombined.SetTrend("Humid", 0)
                TrendStatusCombined.SetTrend("Wind", 0)
            ElseIf TrendArrows IsNot Nothing Then
                Dim pressDir As Integer = 0
                If trendResult.HasData Then
                    pressDir = If(trendResult.Trend = "Rising", 1, If(trendResult.Trend = "Falling", -1, 0))
                End If
                TrendArrows.SetTrend("Press", pressDir)
                TrendArrows.SetTrend("Temp", 0)
                TrendArrows.SetTrend("Humid", 0)
                TrendArrows.SetTrend("Wind", 0)
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating atmospheric readings")
        End Try
    End Sub

    ''' <summary>
    ''' Update UV and solar radiation displays
    ''' </summary>
    Private Sub UpdateLightDisplays(data As ObservationData)
        Try
            ' Update combined UV+Solar control if present
            If SolarUvCombined IsNot Nothing Then
                SolarUvCombined.UvIndex = CSng(data.UvIndex)
                SolarUvCombined.SolarRadiation = CSng(data.SolarRadiation)
            End If

            If LblUV IsNot Nothing Then
                LblUV.Text = $"UV: {data.UvIndex:F1}"
            End If

            If LblSolRad IsNot Nothing Then
                LblSolRad.Text = $"Solar Radiation: {data.SolarRadiation} W/m²"
            End If

            If LblBrightness IsNot Nothing Then
                LblBrightness.Text = $"Brightness: {data.Illuminance:N0} lux"
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating light displays")
        End Try
    End Sub

    ''' <summary>
    ''' Update lightning strike count display
    ''' </summary>
    Private Sub UpdateLightningDisplays(data As ObservationData)
        Try
            If TxtStrikeCount IsNot Nothing Then
                TxtStrikeCount.Text = data.StrikeCount.ToString()
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating lightning displays")
        End Try
    End Sub

    ''' <summary>
    ''' Calculate and update air density display
    ''' </summary>
    Private Sub UpdateAirDensity(data As ObservationData)
        Try
            If LblAirDensity IsNot Nothing Then
                Log.Write($"Calculating air density with TempF: {data.TempF:F1}, PressureMb: {data.Pressure:F1}, Humidity: {data.Humidity:F1}")
                Dim airDensity = CalculateAirDensity(data.TempF, data.Pressure, data.Humidity)
                LblAirDensity.Text = $"Air Density: {airDensity:F4} kg/m³"

                If LblAirDensityCat IsNot Nothing AndAlso LblAirDensityCat.Tag IsNot Nothing Then
                    LblAirDensityCat.Text = String.Format(LblAirDensityCat.Tag.ToString, GetAirDensityCategory(airDensity))
                End If

                Log.Write($"Air density calculated: {airDensity:F4} kg/m³")

                ' Update the AirDensityAltimeter custom control
                If AltAirDensity IsNot Nothing Then
                    Dim densityAltFt As Double = TempestDisplay.Common.Weather.WeatherCalculations.CalculateDensityAltitude(data.TempF, data.Pressure, data.Humidity)
                    AltAirDensity.AirDensity = CSng(airDensity)
                    AltAirDensity.DensityAltitude = CSng(densityAltFt)
                    AltAirDensity.Category = GetAirDensityCategory(airDensity)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating air density")
        End Try
    End Sub

    ''' <summary>
    ''' Update battery status display with color coding and add to chart history
    ''' </summary>
    Private Sub UpdateBatteryStatus(battery As Double)
        Try
            If LblBatteryStatus IsNot Nothing AndAlso LblBatteryStatus.Tag IsNot Nothing Then
                LblBatteryStatus.Text = String.Format(LblBatteryStatus.Tag.ToString, battery)

                Select Case battery
                    Case >= 2.455
                        LblBatteryStatus.BackColor = Color.ForestGreen
                        LblBatteryStatus.ForeColor = Color.White
                    Case >= 2.41
                        LblBatteryStatus.BackColor = Color.GreenYellow
                        LblBatteryStatus.ForeColor = Color.Black
                    Case >= 2.375
                        LblBatteryStatus.BackColor = Color.Orange
                        LblBatteryStatus.ForeColor = Color.Black
                    Case Else
                        LblBatteryStatus.BackColor = Color.Red
                        LblBatteryStatus.ForeColor = Color.White
                End Select
            End If

            ' Add battery reading to chart history
            AddBatteryReading(battery, DateTime.Now)
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating battery status")
        End Try
    End Sub

    ''' <summary>
    ''' Calculate and update cloud base display
    ''' Cloud base is calculated as AGL (Above Ground Level) and converted to MSL (Mean Sea Level)
    ''' Station elevation loaded from settings
    ''' </summary>
    Private Sub UpdateCloudBase(data As ObservationData)
        Try
            If LblCloudBase IsNot Nothing AndAlso LblCloudBase.Tag IsNot Nothing Then
                ' Calculate cloud base AGL (Above Ground Level) in feet
                Dim cloudBaseAGL = TempestDisplay.Common.Weather.WeatherCalculations.CalculateCloudBase(data.TempF, data.Humidity)

                ' Get station elevation from settings
                Dim settings = SettingsRoutines.LoadSettings()
                Dim stationElevationFeet = settings.StationElevation

                ' Add station elevation to get MSL (Mean Sea Level)
                Dim cloudBaseMSL = cloudBaseAGL + stationElevationFeet

                ' Update label using Tag format string
                LblCloudBase.Text = String.Format(LblCloudBase.Tag.ToString, Math.Round(cloudBaseMSL, 0))

                Log.Write($"Cloud base calculated: {cloudBaseMSL:F0} ft MSL ({cloudBaseAGL:F0} ft AGL + {stationElevationFeet:F1} ft elevation)")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UI] Error updating cloud base")
        End Try
    End Sub

End Class