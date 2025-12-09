Imports TempestDisplay.Common.Weather

''' <summary>
''' Tempest data routines - Now primarily supports UDP listener
''' REST API download functionality has been removed; all real-time data comes from UDP broadcasts
''' This module now handles data display and rain data fetching from MeteoBridge
''' </summary>
Friend Module TempestDataRoutines

    Friend Property LastTempestData As TempestModel

    '''' <summary>
    '''' Get update interval in minutes from settings (used for cache management)
    '''' </summary>
    'Private Function GetTempestUpdateIntervalMinutes() As Integer
    '    Dim settings = LoadSettings()
    '    Dim seconds As Integer = 300 ' Default to 5 min if missing
    '    If settings IsNot Nothing AndAlso settings.Tempest IsNot Nothing AndAlso settings.Tempest.UpdateIntervalSeconds > 0 Then
    '        seconds = settings.Tempest.UpdateIntervalSeconds
    '    End If
    '    Return Math.Max(1, CInt(Math.Round(seconds / 60.0)))
    'End Function

    ''' <summary>
    ''' Write station data to UI controls
    ''' Called by REST API (legacy) or can be used for other purposes
    ''' Note: UDP listener updates controls directly in FrmMain.UdpListener.vb
    ''' </summary>
    Friend Async Sub WriteStationData(tNfo As TempestModel)
        Try
            Log.Write("[WriteStationData] Starting station data write")
            If tNfo Is Nothing Then Return
            Dim name As String = If(tNfo.station_name, "")
            Dim frm = Application.OpenForms.Cast(Of Form)().OfType(Of FrmMain)().FirstOrDefault()
            If frm Is Nothing Then Return

            ' Station name is now displayed in form title via SettingsRoutines.PopulateStationName()
            ' UIService.SafeSetText(frm.LblStationName, name) ' Label has been deleted

            ' Set temperatures using first observation if available
            Dim hasObs As Boolean = tNfo.obs IsNot Nothing AndAlso tNfo.obs.Length > 0
            If hasObs Then
                Dim first = tNfo.obs(0)
                If first IsNot Nothing AndAlso first.air_temperature.HasValue Then
                    Dim tempC As Single = first.air_temperature.Value
                    Dim tempF As Single = tempC * 9.0F / 5.0F + 32.0F

                    If frm.TgCurrentTemp IsNot Nothing Then
                        UIService.SafeInvoke(frm.TgCurrentTemp, Sub()
                                                                    frm.TgCurrentTemp.TempF = tempF
                                                                    frm.TgCurrentTemp.TempC = tempC
                                                                End Sub)
                    End If

                    ' Feels like (nullable-safe)
                    If first.feels_like.HasValue Then
                        Dim feelsC As Single = first.feels_like.Value
                        Dim feelsF As Single = feelsC * 9.0F / 5.0F + 32.0F
                        If frm.TgFeelsLike IsNot Nothing Then
                            UIService.SafeInvoke(frm.TgFeelsLike, Sub()
                                                                      frm.TgFeelsLike.TempF = feelsF
                                                                      frm.TgFeelsLike.TempC = feelsC
                                                                  End Sub)
                        End If
                    End If

                    If first.dew_point.HasValue Then
                        Dim dewC As Single = first.dew_point.Value
                        Dim dewF As Single = dewC * 9.0F / 5.0F + 32.0F
                        If frm.TgDewpoint IsNot Nothing Then
                            UIService.SafeInvoke(frm.TgDewpoint, Sub()
                                                                     frm.TgDewpoint.TempF = dewF
                                                                     frm.TgDewpoint.TempC = dewC
                                                                 End Sub)
                        End If
                    End If
                End If

                If frm.LblWindDir IsNot Nothing AndAlso first.wind_direction.HasValue Then
                    Dim winDir As String = first.wind_direction.Value.ToString()
                    If frm.LblWindDir.Tag IsNot Nothing AndAlso TypeOf frm.LblWindDir.Tag Is String AndAlso frm.LblWindDir.Tag.ToString().Contains("{"c) Then
                        Dim tagFormat As String = frm.LblWindDir.Tag.ToString()
                        Dim textArgs As Object() = {winDir, UnitConversions.DegreesToCardinal(CInt(CDbl(winDir)))}
                        UIService.SafeSetTextFormat(frm.LblWindDir, tagFormat, textArgs)
                    End If
                End If

                ' Populate wind speed labels using Tag property for formatting
                If frm.LblAvgWindSpd IsNot Nothing AndAlso first.wind_avg.HasValue Then
                    Dim formatStr = If(frm.LblAvgWindSpd.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblAvgWindSpd, formatStr, first.wind_avg.Value)
                    Else
                        UIService.SafeSetText(frm.LblAvgWindSpd, first.wind_avg.Value.ToString("F1"))
                    End If
                End If
                If frm.LblWindGust IsNot Nothing AndAlso first.wind_gust.HasValue Then
                    Dim formatStr = If(frm.LblWindGust.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblWindGust, formatStr, first.wind_gust.Value)
                    Else
                        UIService.SafeSetText(frm.LblWindGust, first.wind_gust.Value.ToString("F1"))
                    End If
                End If
                If frm.LblWindLull IsNot Nothing AndAlso first.wind_lull.HasValue Then
                    Dim formatStr = If(frm.LblWindLull.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblWindLull, formatStr, first.wind_lull.Value)
                    Else
                        UIService.SafeSetText(frm.LblWindLull, first.wind_lull.Value.ToString("F1"))
                    End If
                End If

                ' Set relative humidity (thread-safe with null check)
                If first.relative_humidity.HasValue AndAlso frm.FgRH IsNot Nothing Then
                    Dim rhValue As Integer = CInt(first.relative_humidity.Value)
                    UIService.SafeInvoke(frm.FgRH, Sub() frm.FgRH.Value = rhValue)
                End If

                ' Set rain minutes (thread-safe with null checks)
                If frm.TxtRainTodayMinutes IsNot Nothing Then
                    UIService.SafeSetText(frm.TxtRainTodayMinutes, CStr(If(first.precip_minutes_local_day, 0)))
                End If
                If frm.TxtRainYesterdayMinutes IsNot Nothing Then
                    UIService.SafeSetText(frm.TxtRainYesterdayMinutes, CStr(If(first.precip_minutes_local_yesterday_final, 0)))
                End If

                ' Populate LblBaroPress using Tag property for formatting
                If frm.LblBaroPress IsNot Nothing AndAlso first.barometric_pressure.HasValue Then
                    Dim formatStr = If(frm.LblBaroPress.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblBaroPress, formatStr, first.barometric_pressure.Value)
                    Else
                        UIService.SafeSetText(frm.LblBaroPress, first.barometric_pressure.Value.ToString)
                    End If
                End If

                ' Populate LblPressTrend using Tag property for formatting
                If frm.LblPressTrend IsNot Nothing AndAlso first.pressure_trend <> "" Then
                    Dim formatStr = If(frm.LblPressTrend.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblPressTrend, formatStr, first.pressure_trend)
                    Else
                        UIService.SafeSetText(frm.LblPressTrend, first.pressure_trend)
                    End If
                End If

                ' Populate LblUV using Tag property for formatting
                If frm.LblUV IsNot Nothing AndAlso first.uv.HasValue Then
                    Dim formatStr = If(frm.LblUV.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblUV, formatStr, first.uv.Value)
                    Else
                        UIService.SafeSetText(frm.LblUV, first.uv.Value.ToString("F1"))
                    End If
                End If

                ' Populate LblSolRad using Tag property for formatting
                If frm.LblSolRad IsNot Nothing AndAlso first.solar_radiation.HasValue Then
                    Dim formatStr = If(frm.LblSolRad.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblSolRad, formatStr, first.solar_radiation.Value)
                    Else
                        UIService.SafeSetText(frm.LblSolRad, first.solar_radiation.Value.ToString("F1"))
                    End If
                End If

                ' Populate Lblbrightness using Tag property for formatting
                If frm.LblBrightness IsNot Nothing AndAlso first.brightness.HasValue Then
                    Dim formatStr = If(frm.LblBrightness.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblBrightness, formatStr, first.brightness.Value)
                    Else
                        UIService.SafeSetText(frm.LblBrightness, first.brightness.Value.ToString("F1"))
                    End If
                End If

                ' Populate LblAirDensity using Tag property for formatting
                If frm.LblAirDensity IsNot Nothing AndAlso first.air_density.HasValue Then
                    Dim formatStr = If(frm.LblAirDensity.Tag, "").ToString()
                    If Not String.IsNullOrWhiteSpace(formatStr) AndAlso formatStr.Contains("{"c) Then
                        UIService.SafeSetTextFormat(frm.LblAirDensity, formatStr, first.air_density.Value)
                    Else
                        UIService.SafeSetText(frm.LblAirDensity, first.air_density.Value.ToString("F1"))
                    End If
                End If

                ' Set lightning data (thread-safe with null checks)
                If frm.TxtStrikeCount IsNot Nothing Then
                    UIService.SafeSetText(frm.TxtStrikeCount, CStr(If(first.lightning_strike_count, 0)))
                End If
                If frm.TxtLightHrCount IsNot Nothing Then
                    UIService.SafeSetText(frm.TxtLightHrCount, CStr(If(first.lightning_strike_count_last_1hr, 0)))
                End If
                If frm.TxtLight3hrCount IsNot Nothing Then
                    UIService.SafeSetText(frm.TxtLight3hrCount, CStr(If(first.lightning_strike_count_last_3hr, 0)))
                End If
                If frm.TxtLightDistance IsNot Nothing Then
                    ' Distance is in kilometers, convert to miles
                    Dim distKm As Integer = If(first.lightning_strike_last_distance, 0)
                    Dim distMiles As Single = distKm * 0.621371F
                    UIService.SafeSetText(frm.TxtLightDistance, distMiles.ToString("F1"))
                End If
                If frm.LblLightLastStrike IsNot Nothing Then
                    ' Convert epoch timestamp to readable format
                    If first.lightning_strike_last_epoch.HasValue AndAlso first.lightning_strike_last_epoch.Value > 0 Then
                        Dim lastStrike As DateTime = DateTimeOffset.FromUnixTimeSeconds(first.lightning_strike_last_epoch.Value).ToLocalTime().DateTime
                        Dim labelFormat As String = If(frm.LblLightLastStrike.Tag IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(frm.LblLightLastStrike.Tag.ToString()), frm.LblLightLastStrike.Tag.ToString(), "{0}")
                        UIService.SafeSetText(frm.LblLightLastStrike, String.Format(labelFormat, lastStrike.ToString("MMM d H:mm")))
                    Else
                        UIService.SafeSetText(frm.LblLightLastStrike, "Last Strike: N/A")
                    End If
                End If

                ' Update lblupdate with timestamp from json
                If frm.LblUpdate IsNot Nothing AndAlso first.timestamp.HasValue Then
                    Dim dt As DateTime = DateTimeOffset.FromUnixTimeSeconds(first.timestamp.Value).ToLocalTime().DateTime
                    UIService.SafeSetText(frm.LblUpdate, String.Format(CStr(frm.LblUpdate.Tag), dt.ToString("MMMM d @ h:mm:ss tt")))
                End If

                ' Fetch rain data asynchronously
                Log.Write("[WriteStationData] About to call FetchRainDataAsync")
                Dim startTime = DateTime.UtcNow
                Dim rainData = Await FetchRainDataAsync().ConfigureAwait(False)
                Dim elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds
                Log.Write($"[WriteStationData] FetchRainDataAsync completed in {elapsed:F0}ms")

                Dim monthAccum As Single = rainData.MonthAccum
                Dim yearAccum As Single = rainData.YearAccum
                Dim allTimeAccum As Single = rainData.AllTimeAccum

                ' Prepare the values array with null-safe conversions
                ' Tempest API returns precipitation in millimeters, convert to inches (divide by 25.4)
                Dim todayIn As Single = If(first.precip_accum_local_day.HasValue, first.precip_accum_local_day.Value / 25.4F, 0.0F)
                Dim yesterdayIn As Single = If(first.precip_accum_local_yesterday_final.HasValue, first.precip_accum_local_yesterday_final.Value / 25.4F, 0.0F)

                Dim precipValues As Single() = {
                    todayIn,
                    yesterdayIn,
                    monthAccum,
                    yearAccum,
                    allTimeAccum
                }

                Log.Write($"[WriteStationData] Setting PTC values - Day: {precipValues(0)}, Yesterday: {precipValues(1)}, Month: {precipValues(2)}, Year: {precipValues(3)}, AllTime: {precipValues(4)}")

                ' Update PTC on UI thread
                If frm.PTC IsNot Nothing Then
                    Log.Write($"[WriteStationData] PTC.IsHandleCreated: {frm.PTC.IsHandleCreated}")
                    UIService.SafeInvoke(frm.PTC, Sub()
                                                      frm.PTC.Values = precipValues
                                                  End Sub)
                    Log.Write("[WriteStationData] PTC values set successfully")
                Else
                    Log.Write("[WriteStationData] ERROR: frm.PTC is Nothing!")
                End If

            End If
            Log.Write("[WriteStationData] Completed successfully")
        Catch ex As Exception
            Log.WriteException(ex, "[WriteStationData] Error writing Tempest station data to UI")
        End Try
    End Sub

    Friend Structure RainAccumData
        Public Property TodayAccum As Single
        Public Property AllTimeAccum As Single
        Public Property YearAccum As Single
        Public Property MonthAccum As Single
        Public Property YesterdayAccum As Single
    End Structure

    ' Cache for historical rain data (changes infrequently)
    Private _rainDataCache As RainAccumData?

    Private _rainDataCacheTime As DateTime = DateTime.MinValue
    Private ReadOnly _rainCacheTtlMinutes As Integer = 15 ' Cache for 15 minutes

    ' Separate cache for yesterday's rain (only changes at midnight)
    Private _yesterdayRainCache As Single = 0.0F

    Private _yesterdayRainCacheTime As DateTime = DateTime.MinValue

    ' Retry configuration
    Private Const MaxRetries As Integer = 3 ' Maximum number of retry attempts per query

    Private Const RetryDelayMs As Integer = 500 ' Delay between retries in milliseconds

    ''' <summary>
    ''' Fetch a single rain query with retry logic
    ''' </summary>
    Private Async Function FetchRainQueryWithRetry(key As String, query As String) As Task(Of (Key As String, Value As Single, Success As Boolean))
        Dim attempt As Integer = 0
        Dim lastError As String = ""
        Dim needsRetry As Boolean

        While attempt < MaxRetries
            attempt += 1
            'needsRetry

            Try
                Log.Write($"[FetchRainQueryWithRetry] {key} - Attempt {attempt}/{MaxRetries}")

                Dim queryResult = Await Modules.GWD.GwdRoutines.Gwd3AsyncResult(query)

                If queryResult.Success Then
                    Dim value As Single
                    If Single.TryParse(queryResult.Value, value) Then
                        Log.Write($"[FetchRainQueryWithRetry] {key} - Success on attempt {attempt}: {value}")
                        Return (key, value, True)
                    Else
                        lastError = $"Parse failed for value: '{queryResult.Value}'"
                        Log.Write($"[FetchRainQueryWithRetry] {key} - {lastError}")
                        needsRetry = True
                    End If
                Else
                    lastError = If(String.IsNullOrEmpty(queryResult.ErrorMessage), "Unknown error", queryResult.ErrorMessage)
                    Log.Write($"[FetchRainQueryWithRetry] {key} - Query failed on attempt {attempt}: {lastError}")
                    needsRetry = True
                End If
            Catch ex As Exception
                lastError = ex.Message
                Log.WriteException(ex, $"[FetchRainQueryWithRetry] {key} - Exception on attempt {attempt}")
                needsRetry = True
            End Try

            ' Wait before retry (outside of Try-Catch to avoid VB limitation)
            If needsRetry AndAlso attempt < MaxRetries Then
                Log.Write($"[FetchRainQueryWithRetry] {key} - Waiting {RetryDelayMs}ms before retry...")
                Await Task.Delay(RetryDelayMs)
            End If
        End While

        ' All retries exhausted, return 0.0F as default
        Log.Write($"[FetchRainQueryWithRetry] {key} - All {MaxRetries} attempts exhausted. Last error: {lastError}. Defaulting to 0.0")
        Return (key, 0.0F, False)
    End Function

    ''' <summary>
    ''' Fetch rain accumulation data from MeteoBridge
    ''' Used by both WriteStationData and UDP listener
    ''' Includes retry logic - will attempt each query up to MaxRetries times before defaulting to 0.0
    ''' </summary>
    Friend Async Function FetchRainDataAsync(Optional fetchYesterday As Boolean = False) As Task(Of RainAccumData)
        ' Check cache first
        If _rainDataCache.HasValue Then
            Dim cacheAge = (DateTime.UtcNow - _rainDataCacheTime).TotalMinutes
            If cacheAge < _rainCacheTtlMinutes Then
                Log.Write($"[FetchRainDataAsync] Using cached rain data (age: {cacheAge:F1} minutes)")
                Dim cached = _rainDataCache.Value
                cached.YesterdayAccum = _yesterdayRainCache
                Return cached
            End If
        End If

        ' Check if we need to fetch yesterday's rain
        Dim shouldFetchYesterday As Boolean = fetchYesterday OrElse
                                              _yesterdayRainCacheTime = DateTime.MinValue OrElse
                                              (DateTime.UtcNow - _yesterdayRainCacheTime).TotalHours > 24

        Dim result As New RainAccumData With {
            .TodayAccum = 0.0F,
            .AllTimeAccum = 0.0F,
            .YearAccum = 0.0F,
            .MonthAccum = 0.0F,
            .YesterdayAccum = 0.0F
        }

        Try
            If shouldFetchYesterday Then
                Log.Write($"[FetchRainDataAsync] Starting rain data fetch (including yesterday) with {MaxRetries} max retries")
            Else
                Log.Write($"[FetchRainDataAsync] Starting rain data fetch (yesterday cached) with {MaxRetries} max retries")
            End If

            Dim queries = CreateRainQueries(shouldFetchYesterday)

            Log.Write($"[FetchRainDataAsync] Fetching {queries.Count} rain queries")
            For Each kvp In queries
                Log.Write($"[FetchRainDataAsync] Query '{kvp.Key}': {kvp.Value}")
            Next

            ' Fetch all rain queries with retry logic in parallel
            Dim tasks = queries.Select(Function(kvp) FetchRainQueryWithRetry(kvp.Key, kvp.Value)).ToList()

            Dim resultPairs = Await Task.WhenAll(tasks).ConfigureAwait(False)

            Log.Write($"[FetchRainDataAsync] Received {resultPairs.Length} results")

            ' Parse results by key name (order-independent!)
            Dim successCount As Integer = 0
            Dim failureCount As Integer = 0

            For Each pair In resultPairs
                Dim key = pair.Key
                Dim value = pair.Value
                Dim success = pair.Success

                If success Then
                    successCount += 1
                    Log.Write($"[FetchRainDataAsync] {key} - Final value: {value}")
                Else
                    failureCount += 1
                    Log.Write($"[FetchRainDataAsync] {key} - Failed after retries, using default: {value}")
                End If

                ' Assign value (will be 0.0F if all retries failed)
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
                        If success Then
                            _yesterdayRainCache = value
                            _yesterdayRainCacheTime = DateTime.UtcNow
                            Log.Write($"[FetchRainDataAsync] Yesterday cached: {value}")
                        End If
                End Select
            Next

            ' If we didn't fetch yesterday, use cached value
            If Not shouldFetchYesterday Then
                result.YesterdayAccum = _yesterdayRainCache
                Log.Write($"[FetchRainDataAsync] Using cached yesterday value: {_yesterdayRainCache}")
            End If

            Log.Write($"[FetchRainDataAsync] Final results - Today: {result.TodayAccum}, AllTime: {result.AllTimeAccum}, Year: {result.YearAccum}, Month: {result.MonthAccum}, Yesterday: {result.YesterdayAccum}")
            Log.Write($"[FetchRainDataAsync] Summary - Success: {successCount}, Failed: {failureCount}")

            ' Cache the result (even if some queries failed, we cache what we got)
            _rainDataCache = result
            _rainDataCacheTime = DateTime.UtcNow
            Log.Write("[FetchRainDataAsync] Rain data cached successfully")
        Catch ex As Exception
            Log.WriteException(ex, "[FetchRainDataAsync] Error fetching rain data")
            If _rainDataCache.HasValue Then
                Log.Write("[FetchRainDataAsync] Returning stale cached data due to fetch error")
                Return _rainDataCache.Value
            End If
        End Try

        Return result
    End Function

    Private Function CreateRainQueries(Optional includeYesterday As Boolean = False) As Dictionary(Of String, String)
        Dim queries = New Dictionary(Of String, String) From {
            {"Today", "rain0total-daysum=In.2:*"},
            {"AllTime", "rain0total-allsum=In.2:*"},
            {"Year", "rain0total-yearsum=In.2:*"},
            {"Month", "rain0total-monthsum=In.2:*"}
        }

        If includeYesterday Then
            queries.Add("Yesterday", "rain0total-ydaysum=In.2:*")
        End If

        Return queries
    End Function

End Module