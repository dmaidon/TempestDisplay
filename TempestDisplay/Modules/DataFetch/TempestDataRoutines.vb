' Last Edit: February 17, 2026 (Batched station UI updates to reduce cross-thread invokes)
Imports System.Globalization

Imports TempestDisplay.Common.Weather

''' <summary>
''' Tempest data routines - Now primarily supports UDP listener
''' REST API download functionality has been removed; all real-time data comes from UDP broadcasts
''' This module now handles data display and rain data fetching from MeteoBridge
''' </summary>
Friend Module TempestDataRoutines

    Private Const KmToMilesFactor As Single = 0.621371F
    Private Const RainQueryMaxConcurrency As Integer = 3
    Private Const MillimetersToInches As Single = 1.0F / 25.4F
    Private Const InchesToMillimeters As Single = 25.4F

    Private ReadOnly ParseCulture As CultureInfo = CultureInfo.InvariantCulture

    ' Thread-safe access to last Tempest data
    Private ReadOnly _lastTempestDataLock As New Object()
    Private _lastTempestData As TempestModel

    ''' <summary>
    ''' Thread-safe access to the last received Tempest data
    ''' </summary>
    Friend Property LastTempestData As TempestModel
        Get
            SyncLock _lastTempestDataLock
                Return _lastTempestData
            End SyncLock
        End Get
        Set(value As TempestModel)
            SyncLock _lastTempestDataLock
                _lastTempestData = value
            End SyncLock
        End Set
    End Property

    ' Cache synchronization
    Private ReadOnly _rainCacheLock As New Object()

    Private _rainDataCache As RainAccumData?
    Private _rainDataCacheTime As DateTime = DateTime.MinValue
    Private ReadOnly _rainCacheTtlMinutes As Integer = 15

    Private Function TryParseSingleInvariant(text As String, ByRef value As Single) As Boolean
        Return Single.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, ParseCulture, value)
    End Function

    ''' <summary>
    ''' Helper to set control text with optional UI marshaling
    ''' </summary>
    Private Sub SetControlText(control As Control, text As String, useInvoke As Boolean)
        If control Is Nothing Then Return

        If useInvoke Then
            UIService.SafeSetText(control, text)
        Else
            control.Text = text
        End If
    End Sub

    ''' <summary>
    ''' Helper to set control text with formatting and optional UI marshaling
    ''' </summary>
    Private Sub SetControlTextFormat(control As Control, format As String, args As Object(), useInvoke As Boolean)
        If control Is Nothing Then Return

        If useInvoke Then
            UIService.SafeSetTextFormat(control, format, args)
        Else
            Try
                control.Text = String.Format(format, args)
            Catch ex As FormatException
                control.Text = format
                Log.WriteException(ex, $"[SetControlTextFormat] Format error for control '{control.Name}' with format '{format}' and args [{String.Join(",", args)}]")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Helper to update a label control with optional Tag-based formatting
    ''' </summary>
    Private Sub UpdateLabelWithFormat(control As Control, value As Single, Optional defaultFormat As String = "F1", Optional useInvoke As Boolean = True)
        If control Is Nothing Then Return

        Dim formatStr = If(control.Tag, "").ToString()
        If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
            SetControlTextFormat(control, formatStr, New Object() {value}, useInvoke)
        Else
            SetControlText(control, value.ToString(defaultFormat), useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Helper to update a label control with Tag-based formatting (string overload)
    ''' </summary>
    Private Sub UpdateLabelWithFormat(control As Control, value As String, Optional useInvoke As Boolean = True)
        If control Is Nothing Then Return

        Dim formatStr = If(control.Tag, "").ToString()
        If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
            SetControlTextFormat(control, formatStr, New Object() {value}, useInvoke)
        Else
            SetControlText(control, value, useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Helper to run a control update action with optional UI marshaling
    ''' </summary>
    Private Sub UpdateControl(control As Control, action As Action, useInvoke As Boolean)
        If control Is Nothing OrElse action Is Nothing Then Return

        If useInvoke Then
            UIService.SafeInvoke(control, action)
        Else
            action()
        End If
    End Sub

    ''' <summary>
    ''' Write station data to UI controls
    ''' Called by REST API (legacy) or can be used for other purposes
    ''' Note: UDP listener updates controls directly in FrmMain.UdpListener.vb
    ''' </summary>
    Friend Async Function WriteStationDataAsync(tNfo As TempestModel) As Task
        Try
            Log.Write("[WriteStationData] Starting station data write")

            ' Guard clause - validate all preconditions up front
            If tNfo Is Nothing OrElse tNfo.obs Is Nothing OrElse tNfo.obs.Length = 0 Then
                Log.Write("[WriteStationData] No observation data available")
                Return
            End If

            Dim frm = Application.OpenForms.Cast(Of Form)().OfType(Of FrmMain)().FirstOrDefault()
            If frm Is Nothing Then
                Log.Write("[WriteStationData] FrmMain not found in OpenForms")
                Return
            End If

            Dim first = tNfo.obs(0)
            If first Is Nothing Then
                Log.Write("[WriteStationData] First observation is null")
                Return
            End If

            ' Update all UI sections with batched invoke when needed
            If frm.InvokeRequired Then
                UIService.SafeInvoke(frm, Sub() UpdateStationUiCore(frm, first))
            Else
                UpdateStationUiCore(frm, first)
            End If
            Await UpdateRainAccumulationAsync(frm, first).ConfigureAwait(False)

            Log.Write("[WriteStationData] Completed successfully")
        Catch ex As Exception
            Log.WriteException(ex, "[WriteStationData] Error writing Tempest station data to UI")
        End Try
    End Function

    ''' <summary>
    ''' Update temperature-related controls
    ''' </summary>
    Private Sub UpdateStationUiCore(frm As FrmMain, first As Ob)
        UpdateTemperatureControls(frm, first, useInvoke:=False)
        UpdateWindControls(frm, first, useInvoke:=False)
        UpdateHumidityControls(frm, first, useInvoke:=False)
        UpdateRainMinutesControls(frm, first, useInvoke:=False)
        UpdatePressureControls(frm, first, useInvoke:=False)
        UpdateSolarControls(frm, first, useInvoke:=False)
        UpdateLightningControls(frm, first, useInvoke:=False)
        UpdateTimestampControl(frm, first, useInvoke:=False)
    End Sub

    Private Sub UpdateTemperatureControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing OrElse Not first.air_temperature.HasValue Then Return

        Dim tempC As Single = first.air_temperature.Value
        Dim tempF As Single = UnitConversions.CelsiusToFahrenheit(tempC)

        UpdateControl(frm.TgCurrentTemp, Sub()
                                             frm.TgCurrentTemp.TempF = tempF
                                             frm.TgCurrentTemp.TempC = tempC
                                         End Sub, useInvoke)

        ' Feels like temperature
        If first.feels_like.HasValue AndAlso frm.TgFeelsLike IsNot Nothing Then
            Dim feelsC As Single = first.feels_like.Value
            Dim feelsF As Single = UnitConversions.CelsiusToFahrenheit(feelsC)
            UpdateControl(frm.TgFeelsLike, Sub()
                                               frm.TgFeelsLike.TempF = feelsF
                                               frm.TgFeelsLike.TempC = feelsC
                                           End Sub, useInvoke)
        End If

        ' Dew point temperature
        If first.dew_point.HasValue AndAlso frm.TgDewpoint IsNot Nothing Then
            Dim dewC As Single = first.dew_point.Value
            Dim dewF As Single = UnitConversions.CelsiusToFahrenheit(dewC)
            UpdateControl(frm.TgDewpoint, Sub()
                                              frm.TgDewpoint.TempF = dewF
                                              frm.TgDewpoint.TempC = dewC
                                          End Sub, useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Update wind-related controls
    ''' </summary>
    Private Sub UpdateWindControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing Then Return

        ' Wind direction
        If frm.LblWindDir IsNot Nothing AndAlso first.wind_direction.HasValue Then
            Dim winDir As String = first.wind_direction.Value.ToString()
            If frm.LblWindDir.Tag IsNot Nothing AndAlso TypeOf frm.LblWindDir.Tag Is String AndAlso frm.LblWindDir.Tag.ToString().Contains("{"c) Then
                Dim tagFormat As String = frm.LblWindDir.Tag.ToString()
                Dim textArgs As Object() = {winDir, UnitConversions.DegreesToCardinal(CInt(CDbl(winDir)))}
                SetControlTextFormat(frm.LblWindDir, tagFormat, textArgs, useInvoke)
            Else
                SetControlText(frm.LblWindDir, winDir, useInvoke)
            End If
        End If

        ' Wind speeds
        If first.wind_avg.HasValue Then
            UpdateLabelWithFormat(frm.LblAvgWindSpd, first.wind_avg.Value, useInvoke:=useInvoke)
        End If

        If first.wind_gust.HasValue Then
            UpdateLabelWithFormat(frm.LblWindGust, first.wind_gust.Value, useInvoke:=useInvoke)
        End If

        If first.wind_lull.HasValue Then
            UpdateLabelWithFormat(frm.LblWindLull, first.wind_lull.Value, useInvoke:=useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Update humidity control
    ''' </summary>
    Private Sub UpdateHumidityControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing OrElse Not first.relative_humidity.HasValue Then Return
        If frm.FgRH Is Nothing Then Return

        Dim rhValue As Integer = CInt(first.relative_humidity.Value)
        UpdateControl(frm.FgRH, Sub() frm.FgRH.Value = rhValue, useInvoke)
    End Sub

    ''' <summary>
    ''' Update rain minutes controls
    ''' </summary>
    Private Sub UpdateRainMinutesControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing Then Return

        If frm.TxtRainTodayMinutes IsNot Nothing Then
            SetControlText(frm.TxtRainTodayMinutes, CStr(If(first.precip_minutes_local_day, 0)), useInvoke)
        End If

        If frm.TxtRainYesterdayMinutes IsNot Nothing Then
            SetControlText(frm.TxtRainYesterdayMinutes, CStr(If(first.precip_minutes_local_yesterday_final, 0)), useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Update pressure-related controls
    ''' </summary>
    Private Sub UpdatePressureControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing Then Return

        ' Barometric pressure
        If first.barometric_pressure.HasValue Then
            If frm.LblBaroPress IsNot Nothing Then
                Dim formatStr = If(frm.LblBaroPress.Tag, "").ToString()
                If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                    SetControlTextFormat(frm.LblBaroPress, formatStr, New Object() {first.barometric_pressure.Value}, useInvoke)
                Else
                    SetControlText(frm.LblBaroPress, first.barometric_pressure.Value.ToString(), useInvoke)
                End If
            End If
        End If

        ' Pressure trend
        If Not String.IsNullOrEmpty(first.pressure_trend) Then
            UpdateLabelWithFormat(frm.LblPressTrend, first.pressure_trend, useInvoke:=useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Update solar and UV controls
    ''' </summary>
    Private Sub UpdateSolarControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing Then Return

        If first.uv.HasValue Then
            UpdateLabelWithFormat(frm.LblUV, first.uv.Value, useInvoke:=useInvoke)
        End If

        If first.solar_radiation.HasValue Then
            UpdateLabelWithFormat(frm.LblSolRad, first.solar_radiation.Value, useInvoke:=useInvoke)
        End If

        If first.brightness.HasValue Then
            UpdateLabelWithFormat(frm.LblBrightness, first.brightness.Value, useInvoke:=useInvoke)
        End If

        If first.air_density.HasValue Then
            UpdateLabelWithFormat(frm.LblAirDensity, first.air_density.Value, useInvoke:=useInvoke)
        End If
    End Sub

    ''' <summary>
    ''' Update lightning-related controls
    ''' </summary>
    Private Sub UpdateLightningControls(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing Then Return

        If frm.TxtStrikeCount IsNot Nothing Then
            SetControlText(frm.TxtStrikeCount, CStr(If(first.lightning_strike_count, 0)), useInvoke)
        End If

        If frm.TxtLightHrCount IsNot Nothing Then
            SetControlText(frm.TxtLightHrCount, CStr(If(first.lightning_strike_count_last_1hr, 0)), useInvoke)
        End If

        If frm.TxtLight3hrCount IsNot Nothing Then
            SetControlText(frm.TxtLight3hrCount, CStr(If(first.lightning_strike_count_last_3hr, 0)), useInvoke)
        End If

        If frm.TxtLightDistance IsNot Nothing Then
            Dim distKm As Integer = If(first.lightning_strike_last_distance, 0)
            Dim distMiles As Single = distKm * KmToMilesFactor
            SetControlText(frm.TxtLightDistance, distMiles.ToString("F1"), useInvoke)
        End If

        If frm.LblLightLastStrike IsNot Nothing Then
            If first.lightning_strike_last_epoch.HasValue AndAlso first.lightning_strike_last_epoch.Value > 0 Then
                Dim lastStrike As DateTime = DateTimeOffset.FromUnixTimeSeconds(first.lightning_strike_last_epoch.Value).ToLocalTime().DateTime
                Dim labelFormat As String = If(frm.LblLightLastStrike.Tag IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(frm.LblLightLastStrike.Tag.ToString()), frm.LblLightLastStrike.Tag.ToString(), "{0}")
                SetControlText(frm.LblLightLastStrike, String.Format(labelFormat, lastStrike.ToString("MMM d H:mm")), useInvoke)
            Else
                SetControlText(frm.LblLightLastStrike, "Last Strike: N/A", useInvoke)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Update timestamp control
    ''' </summary>
    Private Sub UpdateTimestampControl(frm As FrmMain, first As Ob, Optional useInvoke As Boolean = True)
        If first Is Nothing OrElse Not first.timestamp.HasValue Then Return
        If frm.LblUpdate Is Nothing Then Return

        Dim dt As DateTime = DateTimeOffset.FromUnixTimeSeconds(first.timestamp.Value).ToLocalTime().DateTime
        SetControlText(frm.LblUpdate, String.Format(CStr(frm.LblUpdate.Tag), dt.ToString("MMMM d @ h:mm:ss tt")), useInvoke)
    End Sub

    ''' <summary>
    ''' Update rain accumulation controls (async operation)
    ''' </summary>
    Private Async Function UpdateRainAccumulationAsync(frm As FrmMain, first As Ob) As Task
        If first Is Nothing Then Return

        Try
            Log.Write("[WriteStationData] About to call FetchRainDataAsync")
            Dim startTime = DateTime.UtcNow
            Dim rainData = Await FetchRainDataAsync().ConfigureAwait(False)
            Dim elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds
            Log.Write($"[WriteStationData] FetchRainDataAsync completed in {elapsed:F0}ms")

            ' Tempest API returns precipitation in millimeters, convert to inches
            Dim todayIn As Single = If(first.precip_accum_local_day.HasValue, first.precip_accum_local_day.Value * MillimetersToInches, 0.0F)
            Dim yesterdayIn As Single = If(first.precip_accum_local_yesterday_final.HasValue, first.precip_accum_local_yesterday_final.Value * MillimetersToInches, 0.0F)

            Dim precipValues As Single() = {
                todayIn,
                yesterdayIn,
                rainData.MonthAccum,
                rainData.YearAccum,
                rainData.AllTimeAccum
            }

            Log.Write($"[WriteStationData] Setting PTC values - Day: {precipValues(0):F2}, Yesterday: {precipValues(1):F2}, Month: {precipValues(2):F2}, Year: {precipValues(3):F2}, AllTime: {precipValues(4):F2}")

            If frm.PTC IsNot Nothing Then
                UIService.SafeInvoke(frm.PTC, Sub() frm.PTC.Values = precipValues)
            Else
                Log.Write("[WriteStationData] ERROR: frm.PTC is Nothing!")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[WriteStationData] Error updating rain accumulation")
        End Try
    End Function

    ''' <summary>
    ''' Invalidate the rain data cache (call when MeteoBridge settings change)
    ''' </summary>
    Friend Sub InvalidateRainDataCache()
        SyncLock _rainCacheLock
            _rainDataCache = Nothing
            _rainDataCacheTime = DateTime.MinValue
            Log.Write("[TempestDataRoutines] Rain data cache invalidated")
        End SyncLock
    End Sub

    Friend Structure RainAccumData
        Public Property TodayAccum As Single
        Public Property AllTimeAccum As Single
        Public Property YearAccum As Single
        Public Property MonthAccum As Single
        Public Property YesterdayAccum As Single
    End Structure

    ''' <summary>
    ''' Fetch rain accumulation data from MeteoBridge
    ''' Used by both WriteStationData and UDP listener
    ''' Uses bounded concurrency via Gwd3BatchAsyncResult.
    ''' Thread-safe with cache synchronization.
    ''' </summary>
    Friend Async Function FetchRainDataAsync() As Task(Of RainAccumData)
        ' Check cache first (thread-safe)
        SyncLock _rainCacheLock
            If _rainDataCache.HasValue Then
                Dim cacheAge = (DateTime.UtcNow - _rainDataCacheTime).TotalMinutes
                If cacheAge < _rainCacheTtlMinutes Then
                    Log.Write($"[FetchRainDataAsync] Using cached rain data (age: {cacheAge:F1} minutes)")
                    Return _rainDataCache.Value
                Else
                    Log.Write($"[FetchRainDataAsync] Cache expired (age: {cacheAge:F1} minutes), fetching fresh data")
                End If
            Else
                Log.Write("[FetchRainDataAsync] No cached data available, fetching fresh data")
            End If
        End SyncLock

        Dim result As New RainAccumData With {
            .TodayAccum = 0.0F,
            .AllTimeAccum = 0.0F,
            .YearAccum = 0.0F,
            .MonthAccum = 0.0F,
            .YesterdayAccum = 0.0F
        }

        Try
            Log.Write("[FetchRainDataAsync] Starting rain data fetch")

            Dim queries = CreateRainQueries()

            Log.Write($"[FetchRainDataAsync] Fetching {queries.Count} rain queries")
            For Each kvp In queries
                Log.Write($"[FetchRainDataAsync] Query '{kvp.Key}': {kvp.Value}")
            Next

            ' Fetch all rain queries (bounded parallelism)
            Dim queryResultsByTemplate = Await Modules.GWD.GwdRoutines.Gwd3BatchAsyncResult(queries.Values, maxConcurrency:=RainQueryMaxConcurrency).ConfigureAwait(False)

            Dim resultPairs As (Key As String, Value As Single, Success As Boolean)() = queries.Select(Function(kvp)
                                                                                                           Dim template = kvp.Value
                                                                                                           Dim qr As Modules.GWD.GwdRoutines.GwdResult = Nothing
                                                                                                           If Not queryResultsByTemplate.TryGetValue(template, qr) Then
                                                                                                               Return (kvp.Key, Value:=0.0F, Success:=False)
                                                                                                           End If

                                                                                                           If qr.Success Then
                                                                                                               Dim v As Single
                                                                                                               If TryParseSingleInvariant(qr.Value, v) Then
                                                                                                                   Return (kvp.Key, Value:=v, Success:=True)
                                                                                                               End If
                                                                                                           End If

                                                                                                           Return (kvp.Key, Value:=0.0F, Success:=False)
                                                                                                       End Function).ToArray()

            Log.Write($"[FetchRainDataAsync] Received {resultPairs.Length} results")

            ' Parse results by key name (order-independent)
            Dim successCount As Integer = 0
            Dim failureCount As Integer = 0

            For Each pair In resultPairs
                Dim key = pair.Key
                Dim value = pair.Value
                Dim success = pair.Success

                If success Then
                    successCount += 1
                Else
                    failureCount += 1
                    Log.Write($"[FetchRainDataAsync] {key} - Failed, using default: {value}")
                End If

                ' Assign value (will be 0.0F if failed)
                Select Case key
                    Case "Today"
                        result.TodayAccum = value
                    Case "AllTime"
                        result.AllTimeAccum = value
                    Case "Year"
                        result.YearAccum = value
                    Case "Month"
                        result.MonthAccum = value
                    Case "Yesterday"
                        result.YesterdayAccum = value
                End Select
            Next

            Log.Write($"[FetchRainDataAsync] Summary - Success: {successCount}, Failed: {failureCount}")

            ' Cache the result (thread-safe)
            SyncLock _rainCacheLock
                _rainDataCache = result
                _rainDataCacheTime = DateTime.UtcNow
                Log.Write("[FetchRainDataAsync] Rain data cached successfully")
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[FetchRainDataAsync] Error fetching rain data")

            ' Return stale cache if available (thread-safe)
            SyncLock _rainCacheLock
                If _rainDataCache.HasValue Then
                    Dim cacheAge = (DateTime.UtcNow - _rainDataCacheTime).TotalMinutes
                    Log.Write($"[FetchRainDataAsync] Returning stale cached data due to fetch error (age: {cacheAge:F1} minutes)")
                    Return _rainDataCache.Value
                End If
            End SyncLock
        End Try

        Return result
    End Function

    Private Function CreateRainQueries() As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {
            {"Today", "rain0total-daysum=In.2:*"},
            {"AllTime", "rain0total-allsum=In.2:*"},
            {"Year", "rain0total-yearsum=In.2:*"},
            {"Month", "rain0total-monthsum=In.2:*"},
            {"Yesterday", "rain0total-ydaysum=In.2:*"}
        }
    End Function

End Module