Imports System.IO
Imports System.Text.Json
Imports System.Windows.Forms.DataVisualization.Charting

Partial Public Class FrmMain

    ''' <summary>
    ''' Stores battery reading with timestamp
    ''' </summary>
    Private Structure BatteryReading
        Public Property Timestamp As DateTime
        Public Property Voltage As Double
    End Structure

    ' Collection to store 24 hours of battery readings
    Private _batteryHistory As New List(Of BatteryReading)

    ' Lock object for thread-safe access to battery history
    Private _batteryHistoryLock As New Object()

    ' Track last battery history save time
    Private _lastBatterySaveTime As DateTime = DateTime.MinValue

    ' Track if battery chart has been initialized
    Private _batteryChartInitialized As Boolean = False

    ' Cached JsonSerializerOptions for performance
    Private Shared ReadOnly _batteryJsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    ''' <summary>
    ''' Initialize the battery chart with proper settings
    ''' Called when Charts tab is first accessed
    ''' </summary>
    Private Sub InitializeBatteryChart()
        Try
            ' Skip if already initialized
            If _batteryChartInitialized Then
                Log.Write("[Battery] Chart already initialized, skipping")
                Return
            End If

            If ChtBattery Is Nothing Then
                Log.Write("[Battery] ChtBattery control is Nothing, cannot initialize")
                Return
            End If

            If ChtBattery.IsDisposed OrElse ChtBattery.Disposing Then
                Log.Write("[Battery] ChtBattery control is disposed, cannot initialize")
                Return
            End If

            Log.Write("[Battery] Starting chart initialization...")

            ' Clear existing data safely
            Try
                ChtBattery.Series.Clear()
                ChtBattery.ChartAreas.Clear()
                ChtBattery.Legends.Clear()
                ChtBattery.Titles.Clear()
            Catch ex As Exception
                Log.WriteException(ex, "[Battery] Error clearing chart")
                Return
            End Try

            ' Create chart area
            Dim chartArea As New ChartArea("BatteryChartArea")
            chartArea.AxisX.Title = "Time"
            chartArea.AxisX.LabelStyle.Format = "HH:mm"
            chartArea.AxisX.IntervalType = DateTimeIntervalType.Hours
            chartArea.AxisX.Interval = 2
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot

            chartArea.AxisY.Title = "Voltage (V)"
            chartArea.AxisY.LabelStyle.Format = "N2"
            chartArea.AxisY.Minimum = 2.0
            chartArea.AxisY.Maximum = 3.0
            chartArea.AxisY.Interval = 0.1
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot

            ' Add horizontal lines for battery thresholds
            Dim stripLow As New StripLine With {
                .IntervalOffset = 2.375,
                .StripWidth = 0.035,
                .BackColor = Color.FromArgb(100, Color.Red)
            }
            chartArea.AxisY.StripLines.Add(stripLow)

            Dim stripMedium As New StripLine With {
                .IntervalOffset = 2.41,
                .StripWidth = 0.045,
                .BackColor = Color.FromArgb(100, Color.Orange)
            }
            chartArea.AxisY.StripLines.Add(stripMedium)

            Dim stripGood As New StripLine With {
                .IntervalOffset = 2.455,
                .StripWidth = 0.545,
                .BackColor = Color.FromArgb(100, Color.LightGreen)
            }
            chartArea.AxisY.StripLines.Add(stripGood)

            ChtBattery.ChartAreas.Add(chartArea)

            ' Create series
            Dim series As New Series("Battery Voltage") With {
                .ChartType = SeriesChartType.Line,
                .BorderWidth = 2,
                .Color = Color.DarkBlue,
                .XValueType = ChartValueType.DateTime,
                .MarkerStyle = MarkerStyle.Circle,
                .MarkerSize = 2,  'set line size in chart
                .MarkerColor = Color.DarkBlue
            }

            ChtBattery.Series.Add(series)

            ' Configure legend
            Dim legend As New Legend("BatteryLegend") With {
                .Docking = Docking.Top,
                .Alignment = StringAlignment.Center
            }
            ChtBattery.Legends.Add(legend)

            ' Set chart title
            Dim title As New Title("24-Hour Battery Voltage History") With {
                .Font = New Font("Arial", 10, FontStyle.Bold)
            }
            ChtBattery.Titles.Add(title)

            ' Mark as initialized and populate with existing data
            _batteryChartInitialized = True
            Log.Write("[Battery] Chart initialized successfully")

            ' Update chart with any existing battery history
            UpdateBatteryChart()
        Catch ex As Exception
            Log.WriteException(ex, "[Battery] Error initializing battery chart")
        End Try
    End Sub

    ''' <summary>
    ''' Add a battery reading to the history and update the chart
    ''' </summary>
    Private Sub AddBatteryReading(voltage As Double, timestamp As DateTime)
        Try
            Dim removedCount As Integer = 0

            SyncLock _batteryHistoryLock
                ' Add new reading
                _batteryHistory.Add(New BatteryReading With {
                    .Timestamp = timestamp,
                    .Voltage = voltage
                })

                ' Remove readings older than 24 hours
                Dim cutoffTime = timestamp.AddHours(-24)
                Dim countBefore = _batteryHistory.Count
                _batteryHistory.RemoveAll(Function(r) r.Timestamp < cutoffTime)
                removedCount = countBefore - _batteryHistory.Count

                Log.Write($"[Battery] Reading added: {voltage:N2}V at {timestamp:HH:mm:ss}. History count: {_batteryHistory.Count}")
                If removedCount > 0 Then
                    Log.Write($"[Battery] Removed {removedCount} old reading(s) older than 24 hours")
                End If
            End SyncLock

            ' Update the chart on UI thread
            UpdateBatteryChart()

            ' Note: Not saving on every reading to avoid UI freezing
            ' Battery history is saved on app shutdown via CleanupUdpListener
        Catch ex As Exception
            Log.WriteException(ex, "[Battery] Error adding battery reading")
        End Try
    End Sub

    ''' <summary>
    ''' Update the battery chart with current history data
    ''' </summary>
    Private Sub UpdateBatteryChart()
        Try
            ' Skip update if chart hasn't been initialized yet
            If Not _batteryChartInitialized Then
                Return
            End If

            ' Check if chart exists and is initialized
            If ChtBattery Is Nothing Then
                Return
            End If

            If ChtBattery.IsDisposed OrElse ChtBattery.Disposing Then
                Return
            End If

            If ChtBattery.Series.Count = 0 OrElse ChtBattery.ChartAreas.Count = 0 Then
                Return
            End If

            ' Ensure we're on the UI thread
            If ChtBattery.InvokeRequired Then
                Try
                    ChtBattery.Invoke(Sub() UpdateBatteryChart())
                Catch
                    ' Control may be disposed during invoke
                End Try
                Return
            End If

            Dim series = ChtBattery.Series("Battery Voltage")
            If series Is Nothing Then Return

            series.Points.Clear()

            SyncLock _batteryHistoryLock
                ' Only update if we have data
                If _batteryHistory.Count = 0 Then
                    Return
                End If

                ' Add all points to the chart
                For Each reading In _batteryHistory.OrderBy(Function(r) r.Timestamp)
                    series.Points.AddXY(reading.Timestamp, reading.Voltage)
                Next

                ' Update X-axis range to show last 24 hours
                Dim latestTime = _batteryHistory.Max(Function(r) r.Timestamp)
                Dim earliestTime = latestTime.AddHours(-24)

                Dim chartArea = ChtBattery.ChartAreas("BatteryChartArea")
                If chartArea IsNot Nothing Then
                    chartArea.AxisX.Minimum = earliestTime.ToOADate()
                    chartArea.AxisX.Maximum = latestTime.ToOADate()
                End If
            End SyncLock

            Log.Write($"[Battery] Chart updated with {series.Points.Count} data points")
        Catch ex As Exception
            ' Silently catch chart update errors - they're usually from disposed controls
            ' Only log if it's not a disposed/disposing exception
            If Not (TypeOf ex Is ObjectDisposedException OrElse TypeOf ex Is InvalidOperationException) Then
                Log.WriteException(ex, "[Battery] Error updating battery chart")
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Clear all battery history data and reset the chart
    ''' </summary>
    Private Sub ClearBatteryHistory()
        Try
            SyncLock _batteryHistoryLock
                _batteryHistory.Clear()
            End SyncLock

            If ChtBattery IsNot Nothing AndAlso ChtBattery.Series.Count > 0 Then
                UIService.SafeInvoke(ChtBattery, Sub()
                                                     ChtBattery.Series("Battery Voltage").Points.Clear()
                                                 End Sub)
            End If

            Log.Write("Battery history cleared")
        Catch ex As Exception
            Log.WriteException(ex, "Error clearing battery history")
        End Try
    End Sub

    ''' <summary>
    ''' Check if battery history needs periodic save and save if necessary
    ''' Called from timer tick - saves every 5 minutes
    ''' </summary>
    Private Sub CheckAndSaveBatteryHistory()
        Try
            Dim now = DateTime.Now
            Dim minutesSinceLastSave = (now - _lastBatterySaveTime).TotalMinutes

            ' Save every 5 minutes
            If minutesSinceLastSave >= 5 Then
                SaveBatteryHistory()
                _lastBatterySaveTime = now
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[Battery] Error in CheckAndSaveBatteryHistory")
        End Try
    End Sub

    ''' <summary>
    ''' Save battery history to file
    ''' </summary>
    Private Sub SaveBatteryHistory()
        Try
            If String.IsNullOrEmpty(Globals.BatteryHistoryFile) Then
                Log.Write("[Battery] History file path not initialized")
                Return
            End If

            SyncLock _batteryHistoryLock
                ' Clean up old readings before saving
                Dim cutoffTime = DateTime.Now.AddHours(-24)
                Dim validReadings = _batteryHistory.Where(Function(r) r.Timestamp >= cutoffTime).OrderBy(Function(r) r.Timestamp).ToList()

                If Not Directory.Exists(Globals.DataDir) Then
                    Directory.CreateDirectory(Globals.DataDir)
                End If

                If validReadings.Count > 0 Then
                    Dim json = JsonSerializer.Serialize(validReadings, _batteryJsonOptions)
                    File.WriteAllText(Globals.BatteryHistoryFile, json)
                    Log.Write($"[Battery] Saved {validReadings.Count} battery readings to history file")
                Else
                    ' Delete file if no valid readings
                    If File.Exists(Globals.BatteryHistoryFile) Then
                        File.Delete(Globals.BatteryHistoryFile)
                        Log.Write("[Battery] Deleted history file - no valid readings to save")
                    End If
                End If
            End SyncLock
        Catch ex As Exception
            Log.WriteException(ex, "[Battery] Error saving battery history")
        End Try
    End Sub

    ''' <summary>
    ''' Load battery history from file and populate the chart
    ''' </summary>
    Private Sub LoadBatteryHistory()
        Try
            If String.IsNullOrEmpty(Globals.BatteryHistoryFile) Then
                Log.Write("[Battery] History file path not initialized")
                Return
            End If

            If Not File.Exists(Globals.BatteryHistoryFile) Then
                Log.Write("[Battery] No existing history file found - starting fresh")
                Return
            End If

            Dim json = File.ReadAllText(Globals.BatteryHistoryFile)
            Dim loadedReadings = JsonSerializer.Deserialize(Of List(Of BatteryReading))(json)

            If loadedReadings IsNot Nothing AndAlso loadedReadings.Count > 0 Then
                ' Filter to only keep readings from last 24 hours
                Dim cutoffTime = DateTime.Now.AddHours(-24)
                Dim validReadings = loadedReadings.Where(Function(r) r.Timestamp >= cutoffTime).OrderBy(Function(r) r.Timestamp).ToList()

                SyncLock _batteryHistoryLock
                    If validReadings.Count > 0 Then
                        _batteryHistory.Clear()
                        _batteryHistory.AddRange(validReadings)

                        Dim oldestReading = validReadings.First()
                        Dim newestReading = validReadings.Last()
                        Dim removedCount = loadedReadings.Count - validReadings.Count

                        Log.Write($"[Battery] Loaded {validReadings.Count} battery readings from history file")
                        Log.Write($"[Battery] Range: {oldestReading.Timestamp:yyyy-MM-dd HH:mm:ss} ({oldestReading.Voltage:F2}V) to {newestReading.Timestamp:yyyy-MM-dd HH:mm:ss} ({newestReading.Voltage:F2}V)")

                        If removedCount > 0 Then
                            Log.Write($"[Battery] Removed {removedCount} old readings (older than 24 hours)")
                        End If
                    Else
                        Log.Write($"[Battery] All {loadedReadings.Count} readings were older than 24 hours - starting fresh")
                        _batteryHistory.Clear()
                    End If
                End SyncLock

                ' Don't update chart here - it will be updated when the Charts tab is first accessed
                ' and InitializeBatteryChart() is called

                ' Save cleaned-up history if old readings were removed
                If loadedReadings.Count > validReadings.Count Then
                    SaveBatteryHistory()
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[Battery] Error loading battery history")
        End Try
    End Sub

End Class