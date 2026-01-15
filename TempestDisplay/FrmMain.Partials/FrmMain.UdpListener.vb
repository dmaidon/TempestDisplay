Imports System.IO
Imports System.Net.Sockets
Imports System.Text.Json

Partial Public Class FrmMain

    ''DO NOT DELETE: https://weatherflow.github.io/Tempest/api/udp/v171/
    ''DO NOT delete: https://apidocs.tempestwx.com/reference/derived-metrics

    ' Track first observation to fetch yesterday's rain once at startup
    Private _firstObservationReceived As Boolean = False

    ' Barometric pressure trend tracking
    Private ReadOnly _pressureHistory As New List(Of PressureReading)()

    Private ReadOnly _pressureHistoryLock As New Object()
    Private Const PressureTrendHours As Integer = 3
    Private ReadOnly _pressureHistoryFile As String = Path.Combine(DataDir, "PressureHistory.json")

    ' Lightning strike tracking
    Private ReadOnly _lightningStrikeFile As String = Path.Combine(DataDir, "LastLightningStrike.json")

    Private ReadOnly _lightningStrikeLock As New Object()

    ' Message clearing timer tracking
    Private _messageTimer As Timer

    Private _currentMessageType As String = "" ' "rain" or "lightning"
    Private _currentMessageTimestamp As DateTime = DateTime.MinValue

    ' NEW: Temperature/Humidity/Wind trend tracking (1 hour window)
    Private Structure ValueReading
        Public Property Timestamp As DateTime
        Public Property Value As Double
    End Structure

    Private ReadOnly _tempHistory As New List(Of ValueReading)()
    Private ReadOnly _humidHistory As New List(Of ValueReading)()
    Private ReadOnly _windHistory As New List(Of ValueReading)()

    Private ReadOnly _tempLock As New Object()
    Private ReadOnly _humidLock As New Object()
    Private ReadOnly _windLock As New Object()

    Private Const TrendHours As Integer = 1 ' 1-hour window for temp, humidity, wind

    Private ReadOnly _tempHistoryFile As String = Path.Combine(DataDir, "TemperatureHistory.json")
    Private ReadOnly _humidHistoryFile As String = Path.Combine(DataDir, "HumidityHistory.json")
    Private ReadOnly _windHistoryFile As String = Path.Combine(DataDir, "WindHistory.json")

    ' Thresholds to consider a metric steady within noise
    Private Const TempSteadyThresholdF As Double = 0.5

    Private Const HumidSteadyThresholdPct As Double = 1.0
    Private Const WindSteadyThresholdMph As Double = 1.0

    Private Const KmToMilesFactor As Double = 0.621371

    ' Cached JsonSerializerOptions for performance
    Private Shared ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    ''' <summary>
    ''' Structure to hold pressure reading with timestamp
    ''' </summary>
    Private Structure PressureReading
        Public Property Timestamp As DateTime
        Public Property PressureMb As Double
    End Structure

    ''' <summary>
    ''' Structure to hold lightning strike data
    ''' </summary>
    Private Structure LightningStrikeData
        Public Property Timestamp As DateTime
        Public Property DistanceKm As Double
        Public Property Energy As Integer
    End Structure

#Region "UDP Listener Initialization and Cleanup"

    Private Sub InitializeUdpListener()
        Try
            Log.Write("Initializing UDP listener for WeatherFlow hub...")

            ' Load pressure history from file
            LoadPressureHistory()

            ' NEW: Load temp/humidity/wind history from files
            LoadTempHistory()
            LoadHumidityHistory()
            LoadWindHistory()

            ' Load last lightning strike from file
            LoadLastLightningStrike()

            _udpListener = New WeatherFlowUdpListener()

            ' Subscribe to events
            AddHandler _udpListener.RapidWindReceived, AddressOf OnRapidWindReceived
            AddHandler _udpListener.ObservationReceived, AddressOf OnObservationReceived
            AddHandler _udpListener.DeviceStatusReceived, AddressOf OnDeviceStatusReceived
            AddHandler _udpListener.HubStatusReceived, AddressOf OnHubStatusReceived
            AddHandler _udpListener.RainStartReceived, AddressOf OnRainStartReceived
            AddHandler _udpListener.LightningStrikeReceived, AddressOf OnLightningStrikeReceived

            ' Raw message logging disabled (creates many log entries):
            ' AddHandler _udpListener.RawMessageReceived, AddressOf OnRawMessageReceived

            ' Start listening
            _udpListener.StartListening()
            Log.Write("UDP listener started successfully on port 50222")
        Catch ex As InvalidOperationException When ex.Message.Contains("Port") AndAlso ex.Message.Contains("already in use")
            ' Port already in use - likely another instance running
            Log.Write("═══════════════════════════════════════════════════════════")
            Log.Write("ERROR: Port 50222 is already in use!")
            Log.Write("This usually means:")
            Log.Write("  1. Another instance of TempestDisplay is already running")
            Log.Write("  2. Another application is using port 50222")
            Log.Write("Check Task Manager for multiple TempestDisplay.exe instances")
            Log.Write("═══════════════════════════════════════════════════════════")
            Log.WriteException(ex, "Port conflict - UDP listener NOT started")
        Catch ex As SocketException
            ' Detailed socket error logging
            Log.Write("═══════════════════════════════════════════════════════════")
            Log.Write($"ERROR: SocketException starting UDP listener")
            Log.Write($"  ErrorCode: {ex.ErrorCode}")
            Log.Write($"  SocketErrorCode: {ex.SocketErrorCode}")
            Log.Write($"  NativeErrorCode: {ex.NativeErrorCode}")
            Log.Write($"  Message: {ex.Message}")
            Log.Write("Common causes:")
            Select Case ex.SocketErrorCode
                Case SocketError.AddressAlreadyInUse
                    Log.Write("  - Port 50222 is already in use by another application")
                Case SocketError.AccessDenied
                    Log.Write("  - Firewall or permissions blocking port 50222")
                Case SocketError.NetworkUnreachable, SocketError.NetworkDown
                    Log.Write("  - No network adapter available")
                Case Else
                    Log.Write($"  - Unexpected socket error (see error code {ex.SocketErrorCode})")
            End Select
            Log.Write("═══════════════════════════════════════════════════════════")
            Log.WriteException(ex, "SocketException - UDP listener NOT started, will use REST API fallback")
        Catch ex As Exception
            Log.Write("═══════════════════════════════════════════════════════════")
            Log.WriteException(ex, "Unexpected error starting UDP listener - will use REST API fallback")
            Log.Write("═══════════════════════════════════════════════════════════")
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

            ' Cleanup message timer
            If _messageTimer IsNot Nothing Then
                _messageTimer.Stop()
                _messageTimer.Dispose()
                _messageTimer = Nothing
            End If

            ' Save pressure history before shutdown
            SavePressureHistory()

            ' NEW: Save histories
            SaveTempHistory()
            SaveHumidityHistory()
            SaveWindHistory()
        Catch ex As Exception
            Log.WriteException(ex, "Error stopping UDP listener")
        End Try
    End Sub

#End Region

#Region "Event Handlers"

    Private Sub OnRapidWindReceived(sender As Object, e As RapidWindEventArgs)
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                                      Try
                                          Dim windSpeedMph = e.WindSpeed * 2.23694

                                          LblWindSpd.Text = String.Format(LblWindSpd.Tag.ToString, windSpeedMph)
                                          LblWindDir.Text = String.Format(LblWindDir.Tag.ToString, e.WindDirection, DegreesToCardinal(e.WindDirection))

                                          Log.Write($"[UDP] Rapid Wind: {e.WindSpeed:F1} m/s ({windSpeedMph:F1} mph) @ {e.WindDirection}°")
                                      Catch ex As Exception
                                          Log.WriteException(ex, "Error updating wind controls from UDP")
                                      End Try
                                  End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing rapid wind data")
        End Try
    End Sub

    Private Sub OnObservationReceived(sender As Object, e As ObservationEventArgs)
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                                      Log.Write($"[UDP] Observation received from {e.RemoteEndPoint.Address}")

                                      ' Update hub IP address display
                                      UpdateHubIpAddress(e.RemoteEndPoint.Address.ToString())

                                      ' Write to UDP log
                                      UdpLogService.Instance.WriteObservation(e.RawJson)

                                      ' Parse and display observation
                                      ProcessObservation(e.RawJson)
                                  End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing observation data")
        End Try
    End Sub

    Private Sub OnDeviceStatusReceived(sender As Object, e As DeviceStatusEventArgs)
        Try
            Log.Write($"[UDP] Device status received from {e.RemoteEndPoint.Address}")
            ' TODO: Parse battery level, signal strength, sensor status from e.RawJson
        Catch ex As Exception
            Log.WriteException(ex, "Error processing device status")
        End Try
    End Sub

    Private Sub OnHubStatusReceived(sender As Object, e As HubStatusEventArgs)
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                                      Try
                                          Log.Write($"[UDP] Hub status received from {e.RemoteEndPoint.Address}")
                                          ParseAndDisplayHubStatus(e.RawJson)
                                      Catch ex As Exception
                                          Log.WriteException(ex, "Error handling hub status")
                                      End Try
                                  End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing hub status")
        End Try
    End Sub

    Private Sub OnRainStartReceived(sender As Object, e As RainStartEventArgs)
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                                      Try
                                          Dim message = $"Rain Start Event at {e.Timestamp:yyyy-MM-dd HH:mm:ss}"
                                          Log.Write($"[UDP EVENT] {message}")
                                          If TsslMessages IsNot Nothing Then
                                              TsslMessages.Text = message
                                          End If

                                          ' Set up timer to clear message after 15 minutes from rain start time
                                          SetupMessageClearTimer("rain", e.Timestamp, 15)
                                      Catch ex As Exception
                                          Log.WriteException(ex, "Error handling rain start event")
                                      End Try
                                  End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing rain start event")
        End Try
    End Sub

    Private Sub OnLightningStrikeReceived(sender As Object, e As LightningStrikeEventArgs)
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub()
                                      Try
                                          Dim message = $"Lightning Strike at {e.Timestamp:yyyy-MM-dd HH:mm:ss}, Distance: {e.Distance}km, Energy: {e.Energy}"
                                          Log.Write($"[UDP EVENT] {message}")

                                          Dim distanceMiles = e.Distance * KmToMilesFactor

                                          If LblLightLastStrike IsNot Nothing Then
                                              LblLightLastStrike.Text = $"Last Strike: {e.Timestamp:MMM d h:mm tt}"
                                          End If

                                          If TxtLightDistance IsNot Nothing Then
                                              TxtLightDistance.Text = $"{distanceMiles:F1} mi"
                                          End If

                                          If TsslMessages IsNot Nothing Then
                                              TsslMessages.Text = message
                                          End If

                                          ' Set up timer to clear message after 30 minutes from lightning strike time
                                          SetupMessageClearTimer("lightning", e.Timestamp, 30)

                                          Task.Run(Sub() SaveLightningStrike(e.Timestamp, e.Distance, e.Energy))
                                      Catch ex As Exception
                                          Log.WriteException(ex, "Error handling lightning strike event")
                                      End Try
                                  End Sub))
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error processing lightning strike event")
        End Try
    End Sub

    Private Sub OnRawMessageReceived(sender As Object, e As RawMessageEventArgs)
        Log.Write($"[UDP RAW] {e.RemoteEndPoint.Address}: {e.JsonMessage}")
    End Sub

#End Region

#Region "Observation Processing"

    ''' <summary>
    ''' Main observation processing method - coordinates parsing and UI updates
    ''' REFACTORED: Much cleaner and easier to follow
    ''' </summary>
    Private Async Sub ProcessObservation(jsonData As String)
        Try
            Log.Write($"[UDP] ProcessObservation called, JSON length: {jsonData.Length}")

            ' Parse observation using dedicated parser class
            Dim data = ObservationParser.ParseObsStPacket(jsonData)
            If data Is Nothing Then
                Return ' Parser logs the issue
            End If

            ' Update DataGridView with raw JSON root
            Using document = JsonDocument.Parse(jsonData)
                Dim root = document.RootElement
                If DgvObsSt IsNot Nothing Then
                    UpdateObsStGrid(data, root)
                End If
            End Using

            ' Update all UI controls
            UpdateWeatherUI(data, jsonData)

            ' Track hi/lo data in SQLite
            Try
                Dim feels = CalculateFeelsLike(data.TempF, data.Humidity, data.WindAvgMph)
                Dim heatIndex = CalculateHeatIndex(data.TempF, data.Humidity)
                Dim windChill = CalculateWindChill(data.TempF, data.WindAvgMph)

                ' Rain totals in inches: Today from UDP, Month/Year from MeteoBridge cache if available
                Dim rainDayIn As Double = data.RainInches
                Dim rainMonthIn As Double? = Nothing
                Dim rainYearIn As Double? = Nothing

                Dim cachedRain = Await TempestDataRoutines.FetchRainDataAsync().ConfigureAwait(False)
                rainMonthIn = cachedRain.MonthAccum
                rainYearIn = cachedRain.YearAccum

                HiLoDatabase.UpdateDailyHiLo(
                    data.TimestampDateTime,
                    data.TempF,
                    feels,
                    heatIndex,
                    windChill,
                    rainDayIn,
                    rainMonthIn,
                    rainYearIn,
                    data.WindAvgMph,
                    data.WindDirection,
                    data.UvIndex,
                    data.SolarRadiation)
            Catch ex As Exception
                Log.WriteException(ex, "[HiLo] Error updating daily hi/lo from UDP observation")
            End Try

            ' Update rain gauges (async operation)
            Try
                If PTC IsNot Nothing Then
                    Await UpdateRainGaugesAsync()
                End If
            Catch ex As Exception
                Log.WriteException(ex, "[UDP] Error updating rain gauges")
            End Try
        Catch ex As Exception
            Log.WriteException(ex, "Error in ProcessObservation")
        End Try
    End Sub

#End Region

#Region "Rain Gauge Updates"

    Private Async Function UpdateRainGaugesAsync() As Task
        Try
            If Not _firstObservationReceived Then
                _firstObservationReceived = True
                Log.Write("[UDP] First observation - fetching rain data")
            End If

            Dim rainData = Await TempestDataRoutines.FetchRainDataAsync().ConfigureAwait(False)

            Dim precipValues As Single() = {
                rainData.TodayAccum,
                rainData.YesterdayAccum,
                rainData.MonthAccum,
                rainData.YearAccum,
                rainData.AllTimeAccum
            }

            If PTC IsNot Nothing Then
                UIService.SafeInvoke(PTC, Sub()
                                              PTC.Values = precipValues
                                          End Sub)
                Log.Write($"[UDP] Rain gauges updated - Today: {rainData.TodayAccum:F2}, Yesterday: {rainData.YesterdayAccum:F2}, Month: {rainData.MonthAccum:F2}, Year: {rainData.YearAccum:F2}, AllTime: {rainData.AllTimeAccum:F2}")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UDP] Error in UpdateRainGaugesAsync")
        End Try
    End Function

#End Region

#Region "Lightning Strike Persistence"

    Private Sub LoadLastLightningStrike()
        Try
            If Not File.Exists(_lightningStrikeFile) Then
                Log.Write("[Lightning] No existing lightning strike file found")
                Return
            End If

            Dim json = File.ReadAllText(_lightningStrikeFile)
            Dim lastStrike = JsonSerializer.Deserialize(Of LightningStrikeData)(json)

            If lastStrike.Timestamp <> DateTime.MinValue Then
                Dim distanceMiles = lastStrike.DistanceKm * KmToMilesFactor

                UIService.SafeInvoke(Me, Sub()
                                             If LblLightLastStrike IsNot Nothing Then
                                                 LblLightLastStrike.Text = $"Last Strike: {lastStrike.Timestamp:MMM d h:mm tt}"
                                             End If
                                             If TxtLightDistance IsNot Nothing Then
                                                 TxtLightDistance.Text = $"{distanceMiles:F1} mi"
                                             End If
                                         End Sub)

                Log.Write($"[Lightning] Loaded last strike: {lastStrike.Timestamp:yyyy-MM-dd HH:mm:ss}, {distanceMiles:F1} mi ({lastStrike.DistanceKm:F1} km), Energy: {lastStrike.Energy}")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[Lightning] Error loading last lightning strike")
        End Try
    End Sub

    Private Sub SaveLightningStrike(timestamp As DateTime, distanceKm As Double, energy As Integer)
        Try
            SyncLock _lightningStrikeLock
                If Not Directory.Exists(DataDir) Then
                    Directory.CreateDirectory(DataDir)
                End If

                Dim strikeData = New LightningStrikeData With {
                    .Timestamp = timestamp,
                    .DistanceKm = distanceKm,
                    .Energy = energy
                }

                Dim json = JsonSerializer.Serialize(strikeData, _jsonOptions)
                File.WriteAllText(_lightningStrikeFile, json)

                Dim distanceMiles = distanceKm * KmToMilesFactor
                Log.Write($"[Lightning] Saved strike: {timestamp:yyyy-MM-dd HH:mm:ss}, {distanceMiles:F1} mi ({distanceKm:F1} km), Energy: {energy}")
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Lightning] Error saving lightning strike")
        End Try
    End Sub

#End Region

#Region "Pressure Trend Tracking"

    Private Sub LoadPressureHistory()
        Try
            If Not File.Exists(_pressureHistoryFile) Then
                Log.Write("[Pressure] No existing pressure history file found")
                Return
            End If

            Dim json = File.ReadAllText(_pressureHistoryFile)
            Dim loadedReadings = JsonSerializer.Deserialize(Of List(Of PressureReading))(json)

            If loadedReadings IsNot Nothing AndAlso loadedReadings.Count > 0 Then
                SyncLock _pressureHistoryLock
                    Dim cutoffTime = DateTime.Now.AddHours(-PressureTrendHours)
                    Dim validReadings = loadedReadings.Where(Function(r) r.Timestamp >= cutoffTime).OrderBy(Function(r) r.Timestamp).ToList()

                    If validReadings.Count > 0 Then
                        _pressureHistory.Clear()
                        _pressureHistory.AddRange(validReadings)

                        Dim oldestReading = validReadings.First()
                        Dim newestReading = validReadings.Last()
                        Dim timeSpan = DateTime.Now - oldestReading.Timestamp

                        Log.Write($"[Pressure] Loaded {validReadings.Count} pressure readings from history file (removed {loadedReadings.Count - validReadings.Count} old readings)")
                        Log.Write($"[Pressure] Oldest reading is {timeSpan.TotalHours:F1} hours old from {oldestReading.Timestamp:yyyy-MM-dd HH:mm:ss}")
                        Log.Write($"[Pressure] Newest reading is from {newestReading.Timestamp:yyyy-MM-dd HH:mm:ss}")

                        If timeSpan.TotalHours < PressureTrendHours Then
                            Dim hoursRemaining = PressureTrendHours - timeSpan.TotalHours
                            Log.Write($"[Pressure] Need {hoursRemaining:F1} more hours of data for trend calculation")
                        Else
                            Log.Write($"[Pressure] Enough data available for immediate trend calculation")

                            Dim currentPressure = newestReading.PressureMb
                            Dim currentTime = newestReading.Timestamp
                            Dim trendResult = CalculatePressureTrend(currentPressure, currentTime)

                            If trendResult.HasData AndAlso LblPressTrend IsNot Nothing Then
                                UIService.SafeInvoke(Me, Sub()
                                                             If LblPressTrend IsNot Nothing Then
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
                                                             End If
                                                         End Sub)
                            End If
                        End If
                    Else
                        Log.Write($"[Pressure] All {loadedReadings.Count} readings were older than 3 hours - starting fresh")
                        _pressureHistory.Clear()
                    End If
                End SyncLock
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[Pressure] Error loading pressure history")
        End Try
    End Sub

    Private Sub SavePressureHistory()
        Try
            SyncLock _pressureHistoryLock
                Dim cutoffTime = DateTime.Now.AddHours(-PressureTrendHours)
                Dim validReadings = _pressureHistory.Where(Function(r) r.Timestamp >= cutoffTime).OrderBy(Function(r) r.Timestamp).ToList()

                If Not Directory.Exists(DataDir) Then
                    Directory.CreateDirectory(DataDir)
                End If

                If validReadings.Count > 0 Then
                    Dim json = JsonSerializer.Serialize(validReadings, _jsonOptions)
                    File.WriteAllText(_pressureHistoryFile, json)
                    Log.Write($"[Pressure] Saved {validReadings.Count} pressure readings to history file (removed {_pressureHistory.Count - validReadings.Count} old readings)")
                Else
                    If File.Exists(_pressureHistoryFile) Then
                        File.Delete(_pressureHistoryFile)
                        Log.Write("[Pressure] Deleted pressure history file - no valid readings to save")
                    End If
                End If
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Pressure] Error saving pressure history")
        End Try
    End Sub

    Private Sub AddPressureReading(pressureMb As Double, timestamp As DateTime)
        SyncLock _pressureHistoryLock
            _pressureHistory.Add(New PressureReading With {
                .Timestamp = timestamp,
                .PressureMb = pressureMb
            })

            Dim cutoffTime = timestamp.AddHours(-PressureTrendHours)
            Dim removedCount = _pressureHistory.RemoveAll(Function(r) r.Timestamp < cutoffTime)

            Log.Write($"[UDP] Pressure history: {_pressureHistory.Count} readings over {PressureTrendHours} hours (removed {removedCount} old readings)")

            Task.Run(Sub() SavePressureHistory())
        End SyncLock
    End Sub

    Private Function CalculatePressureTrend(currentPressureMb As Double, currentTime As DateTime) As (Trend As String, Delta As Double, HasData As Boolean)
        SyncLock _pressureHistoryLock
            If _pressureHistory.Count < 2 Then
                Return ("N/A", 0.0, False)
            End If

            Dim oldestReading = _pressureHistory.OrderBy(Function(r) r.Timestamp).First()
            Dim timeSpan = currentTime - oldestReading.Timestamp

            Const hoursTolerance As Double = 0.02
            If timeSpan.TotalHours < (PressureTrendHours - hoursTolerance) Then
                Dim hoursRemaining = PressureTrendHours - timeSpan.TotalHours
                Log.Write($"[UDP] Pressure trend: Need {hoursRemaining:F1} more hours of data (have {timeSpan.TotalHours:F1} hours)")
                Return ("N/A", 0.0, False)
            End If

            Dim deltaPressure = currentPressureMb - oldestReading.PressureMb

            Dim trend As String
            If deltaPressure <= -1.0 Then
                trend = "Falling"
            ElseIf deltaPressure >= 1.0 Then
                trend = "Rising"
            Else
                trend = "Steady"
            End If

            Log.Write($"[UDP] Pressure trend calculated: {trend} (ΔP = {deltaPressure:F2} mb over {timeSpan.TotalHours:F1} hours)")
            Return (trend, deltaPressure, True)
        End SyncLock
    End Function

#End Region

#Region "Temp/Humidity/Wind Trend Tracking"

    Private Sub LoadTempHistory()
        Try
            If Not File.Exists(_tempHistoryFile) Then Return
            Dim json = File.ReadAllText(_tempHistoryFile)
            Dim loaded = JsonSerializer.Deserialize(Of List(Of ValueReading))(json)
            If loaded Is Nothing OrElse loaded.Count = 0 Then Return
            SyncLock _tempLock
                Dim cutoff = DateTime.Now.AddHours(-TrendHours)
                Dim valid = loaded.Where(Function(r) r.Timestamp >= cutoff).OrderBy(Function(r) r.Timestamp).ToList()
                _tempHistory.Clear()
                _tempHistory.AddRange(valid)
                Log.Write($"[Temp] Loaded {valid.Count} readings (1hr window)")
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Temp] Error loading temperature history")
        End Try
    End Sub

    Private Sub SaveTempHistory()
        Try
            SyncLock _tempLock
                Dim cutoff = DateTime.Now.AddHours(-TrendHours)
                Dim valid = _tempHistory.Where(Function(r) r.Timestamp >= cutoff).OrderBy(Function(r) r.Timestamp).ToList()
                If Not Directory.Exists(DataDir) Then Directory.CreateDirectory(DataDir)
                If valid.Count > 0 Then
                    File.WriteAllText(_tempHistoryFile, JsonSerializer.Serialize(valid, _jsonOptions))
                ElseIf File.Exists(_tempHistoryFile) Then
                    File.Delete(_tempHistoryFile)
                End If
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Temp] Error saving temperature history")
        End Try
    End Sub

    Private Sub AddTemperatureReading(tempF As Double, timestamp As DateTime)
        SyncLock _tempLock
            _tempHistory.Add(New ValueReading With {.Timestamp = timestamp, .Value = tempF})
            Dim cutoff = timestamp.AddHours(-TrendHours)
            _tempHistory.RemoveAll(Function(r) r.Timestamp < cutoff)
            Task.Run(Sub() SaveTempHistory())
        End SyncLock
    End Sub

    Private Function CalculateTempTrend(currentTempF As Double, currentTime As DateTime) As (Trend As String, Delta As Double, HasData As Boolean)
        SyncLock _tempLock
            If _tempHistory.Count < 2 Then Return ("N/A", 0, False)
            Dim oldest = _tempHistory.OrderBy(Function(r) r.Timestamp).First()
            Dim span = currentTime - oldest.Timestamp
            If span.TotalHours < 0.95 Then Return ("N/A", 0, False)
            Dim delta = currentTempF - oldest.Value
            Dim trend As String
            If delta <= -TempSteadyThresholdF Then
                trend = "Falling"
            ElseIf delta >= TempSteadyThresholdF Then
                trend = "Rising"
            Else
                trend = "Steady"
            End If
            Log.Write($"[Temp] Trend: {trend} (ΔT = {delta:F2}°F over {span.TotalMinutes:F0} min)")
            Return (trend, delta, True)
        End SyncLock
    End Function

    Private Sub LoadHumidityHistory()
        Try
            If Not File.Exists(_humidHistoryFile) Then Return
            Dim json = File.ReadAllText(_humidHistoryFile)
            Dim loaded = JsonSerializer.Deserialize(Of List(Of ValueReading))(json)
            If loaded Is Nothing OrElse loaded.Count = 0 Then Return
            SyncLock _humidLock
                Dim cutoff = DateTime.Now.AddHours(-TrendHours)
                Dim valid = loaded.Where(Function(r) r.Timestamp >= cutoff).OrderBy(Function(r) r.Timestamp).ToList()
                _humidHistory.Clear()
                _humidHistory.AddRange(valid)
                Log.Write($"[Humid] Loaded {valid.Count} readings (1hr window)")
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Humid] Error loading humidity history")
        End Try
    End Sub

    Private Sub SaveHumidityHistory()
        Try
            SyncLock _humidLock
                Dim cutoff = DateTime.Now.AddHours(-TrendHours)
                Dim valid = _humidHistory.Where(Function(r) r.Timestamp >= cutoff).OrderBy(Function(r) r.Timestamp).ToList()
                If Not Directory.Exists(DataDir) Then Directory.CreateDirectory(DataDir)
                If valid.Count > 0 Then
                    File.WriteAllText(_humidHistoryFile, JsonSerializer.Serialize(valid, _jsonOptions))
                ElseIf File.Exists(_humidHistoryFile) Then
                    File.Delete(_humidHistoryFile)
                End If
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Humid] Error saving humidity history")
        End Try
    End Sub

    Private Sub AddHumidityReading(humidityPct As Double, timestamp As DateTime)
        SyncLock _humidLock
            _humidHistory.Add(New ValueReading With {.Timestamp = timestamp, .Value = humidityPct})
            Dim cutoff = timestamp.AddHours(-TrendHours)
            _humidHistory.RemoveAll(Function(r) r.Timestamp < cutoff)
            Task.Run(Sub() SaveHumidityHistory())
        End SyncLock
    End Sub

    Private Function CalculateHumidityTrend(currentHumidityPct As Double, currentTime As DateTime) As (Trend As String, Delta As Double, HasData As Boolean)
        SyncLock _humidLock
            If _humidHistory.Count < 2 Then Return ("N/A", 0, False)
            Dim oldest = _humidHistory.OrderBy(Function(r) r.Timestamp).First()
            Dim span = currentTime - oldest.Timestamp
            If span.TotalHours < 0.95 Then Return ("N/A", 0, False)
            Dim delta = currentHumidityPct - oldest.Value
            Dim trend As String
            If delta <= -HumidSteadyThresholdPct Then
                trend = "Falling"
            ElseIf delta >= HumidSteadyThresholdPct Then
                trend = "Rising"
            Else
                trend = "Steady"
            End If
            Log.Write($"[Humid] Trend: {trend} (ΔH = {delta:F1}% over {span.TotalMinutes:F0} min)")
            Return (trend, delta, True)
        End SyncLock
    End Function

    Private Sub LoadWindHistory()
        Try
            If Not File.Exists(_windHistoryFile) Then Return
            Dim json = File.ReadAllText(_windHistoryFile)
            Dim loaded = JsonSerializer.Deserialize(Of List(Of ValueReading))(json)
            If loaded Is Nothing OrElse loaded.Count = 0 Then Return
            SyncLock _windLock
                Dim cutoff = DateTime.Now.AddHours(-TrendHours)
                Dim valid = loaded.Where(Function(r) r.Timestamp >= cutoff).OrderBy(Function(r) r.Timestamp).ToList()
                _windHistory.Clear()
                _windHistory.AddRange(valid)
                Log.Write($"[Wind] Loaded {valid.Count} readings (1hr window)")
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Wind] Error loading wind history")
        End Try
    End Sub

    Private Sub SaveWindHistory()
        Try
            SyncLock _windLock
                Dim cutoff = DateTime.Now.AddHours(-TrendHours)
                Dim valid = _windHistory.Where(Function(r) r.Timestamp >= cutoff).OrderBy(Function(r) r.Timestamp).ToList()
                If Not Directory.Exists(DataDir) Then Directory.CreateDirectory(DataDir)
                If valid.Count > 0 Then
                    File.WriteAllText(_windHistoryFile, JsonSerializer.Serialize(valid, _jsonOptions))
                ElseIf File.Exists(_windHistoryFile) Then
                    File.Delete(_windHistoryFile)
                End If
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Wind] Error saving wind history")
        End Try
    End Sub

    Private Sub AddWindReading(windMph As Double, timestamp As DateTime)
        SyncLock _windLock
            _windHistory.Add(New ValueReading With {.Timestamp = timestamp, .Value = windMph})
            Dim cutoff = timestamp.AddHours(-TrendHours)
            _windHistory.RemoveAll(Function(r) r.Timestamp < cutoff)
            Task.Run(Sub() SaveWindHistory())
        End SyncLock
    End Sub

    Private Function CalculateWindTrend(currentWindMph As Double, currentTime As DateTime) As (Trend As String, Delta As Double, HasData As Boolean)
        SyncLock _windLock
            If _windHistory.Count < 2 Then Return ("N/A", 0, False)
            Dim oldest = _windHistory.OrderBy(Function(r) r.Timestamp).First()
            Dim span = currentTime - oldest.Timestamp
            If span.TotalHours < 0.95 Then Return ("N/A", 0, False)
            Dim delta = currentWindMph - oldest.Value
            Dim trend As String
            If delta <= -WindSteadyThresholdMph Then
                trend = "Falling"
            ElseIf delta >= WindSteadyThresholdMph Then
                trend = "Rising"
            Else
                trend = "Steady"
            End If
            Log.Write($"[Wind] Trend: {trend} (ΔW = {delta:F2} mph over {span.TotalMinutes:F0} min)")
            Return (trend, delta, True)
        End SyncLock
    End Function

#End Region

#Region "Hub IP Address Display"

    ''' <summary>
    ''' Update the hub IP address label
    ''' </summary>
    Private Sub UpdateHubIpAddress(ipAddress As String)
        Try
            If LblIP IsNot Nothing Then
                LblIP.Text = $"Hub IP: {ipAddress}"
                Log.Write($"[UDP] Hub IP address updated: {ipAddress}")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[UDP] Error updating hub IP address")
        End Try
    End Sub

#End Region

#Region "Message Clearing Timer"

    ''' <summary>
    ''' Sets up a timer to clear the TsslMessages text after a specified duration.
    ''' For rain: clears 15 minutes after the rain start time.
    ''' For lightning: clears 30 minutes after the strike time.
    ''' </summary>
    Private Sub SetupMessageClearTimer(messageType As String, eventTimestamp As DateTime, durationMinutes As Integer)
        Try
            ' Stop existing timer if any
            If _messageTimer IsNot Nothing Then
                _messageTimer.Stop()
                _messageTimer.Dispose()
                _messageTimer = Nothing
            End If

            ' Track current message
            _currentMessageType = messageType
            _currentMessageTimestamp = eventTimestamp

            ' Calculate when to clear the message (event time + duration)
            Dim clearTime = eventTimestamp.AddMinutes(durationMinutes)
            Dim delay = clearTime - DateTime.Now

            ' If the clear time is in the past, clear immediately
            If delay.TotalMilliseconds <= 0 Then
                Log.Write($"[Message Timer] {messageType} message clear time is in the past, clearing now")
                ClearMessage()
                Return
            End If

            ' Create and start timer
            _messageTimer = New Timer With {
                .Interval = CInt(delay.TotalMilliseconds)
            }
            AddHandler _messageTimer.Tick, AddressOf OnMessageTimerTick
            _messageTimer.Start()

            Log.Write($"[Message Timer] Set timer to clear {messageType} message in {delay.TotalMinutes:F1} minutes (at {clearTime:yyyy-MM-dd HH:mm:ss})")
        Catch ex As Exception
            Log.WriteException(ex, "[Message Timer] Error setting up message clear timer")
        End Try
    End Sub

    ''' <summary>
    ''' Timer callback to clear the message
    ''' </summary>
    Private Sub OnMessageTimerTick(sender As Object, e As EventArgs)
        Try
            Log.Write($"[Message Timer] Timer elapsed, clearing {_currentMessageType} message")
            ClearMessage()
        Catch ex As Exception
            Log.WriteException(ex, "[Message Timer] Error in timer tick handler")
        End Try
    End Sub

    ''' <summary>
    ''' Clears the TsslMessages text and stops the timer
    ''' </summary>
    Private Sub ClearMessage()
        Try
            ' Stop and dispose timer
            If _messageTimer IsNot Nothing Then
                _messageTimer.Stop()
                _messageTimer.Dispose()
                _messageTimer = Nothing
            End If

            ' Clear message on UI thread
            If TsslMessages IsNot Nothing Then
                UIService.SafeInvoke(Me, Sub()
                                             If TsslMessages IsNot Nothing Then
                                                 TsslMessages.Text = "-"
                                             End If
                                         End Sub)
            End If

            ' Clear tracking variables
            _currentMessageType = ""
            _currentMessageTimestamp = DateTime.MinValue

            Log.Write("[Message Timer] Message cleared")
        Catch ex As Exception
            Log.WriteException(ex, "[Message Timer] Error clearing message")
        End Try
    End Sub

#End Region

End Class