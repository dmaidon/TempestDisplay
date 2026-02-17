' Last Edit: February 17, 2026 (Reset daily UV/solar peaks at midnight)
Imports System.IO

Friend Module MidnightRoutines

    ' Configuration constants
    Private Const RainFetchTimeoutSeconds As Integer = 30
    Private Const StateFilePath As String = "Data\MidnightState.json"

    ' Cached JSON serializer options for performance (CA1869)
    Private ReadOnly JsonOptions As New System.Text.Json.JsonSerializerOptions With {
        .WriteIndented = True
    }

    ' Midnight routine state tracking
    Public Structure MidnightState
        Public LastSuccessfulRun As DateTime
        Public LastAttemptedRun As DateTime
        Public StepsCompleted As List(Of String)
        Public StepsFailed As List(Of String)
        Public TotalDurationMs As Long
    End Structure

    ''' <summary>
    ''' Perform midnight maintenance tasks asynchronously with proper async/await pattern.
    ''' </summary>
    Friend Async Function PerformMidnightUpdateAsync() As Task
        Dim state As New MidnightState With {
            .LastAttemptedRun = DateTime.Now,
            .StepsCompleted = New List(Of String)(),
            .StepsFailed = New List(Of String)()
        }

        Dim stopwatch As Stopwatch = Stopwatch.StartNew()

        Try
            Log.Write("PerformMidnightUpdate: Starting midnight maintenance...")

            ' Step 1: Reset error count for the new day
            Try
                Globals.ErrCount = 0
                Try
                    LogService.Instance.TriggerErrorCountUpdate()
                Catch ex As Exception
                    Log.WriteException(ex, "PerformMidnightUpdate: Warning - Failed to trigger error count update event")
                End Try
                Log.Write("PerformMidnightUpdate: Error count reset to 0")
                state.StepsCompleted.Add("ErrorCountReset")
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error resetting error count")
                state.StepsFailed.Add("ErrorCountReset")
            End Try

            ' Step 1b: Reset daily UV/solar peak markers
            Try
                Dim frm = Application.OpenForms.Cast(Of Form)().OfType(Of FrmMain)().FirstOrDefault()
                If frm IsNot Nothing AndAlso frm.SolarUvCombined IsNot Nothing Then
                    UIService.SafeInvoke(frm.SolarUvCombined, Sub()
                                                                frm.SolarUvCombined.UvPeak = 0.0F
                                                                frm.SolarUvCombined.SolarPeak = 0.0F
                                                            End Sub)
                    Log.Write("PerformMidnightUpdate: Daily UV/Solar peaks reset")
                    state.StepsCompleted.Add("UvSolarPeakReset")
                Else
                    Log.Write("PerformMidnightUpdate: UV/Solar peak reset skipped (control not available)")
                    state.StepsFailed.Add("UvSolarPeakReset")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error resetting UV/Solar peaks")
                state.StepsFailed.Add("UvSolarPeakReset")
            End Try

            ' Step 2 & 3: Close log files in parallel
            Try
                Log.Write("PerformMidnightUpdate: Closing current log files...")
                Await Task.WhenAll(
                    Task.Run(Sub()
                                 Try
                                     LogService.Instance.Shutdown()
                                 Catch ex As Exception
                                     Log.WriteException(ex, "PerformMidnightUpdate: Error shutting down LogService")
                                 End Try
                             End Sub),
                    Task.Run(Sub()
                                 Try
                                     UdpLogService.Instance.Shutdown()
                                 Catch ex As Exception
                                     Log.WriteException(ex, "PerformMidnightUpdate: Error shutting down UdpLogService")
                                 End Try
                             End Sub)
                )
                state.StepsCompleted.Add("LogShutdown")
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error during log shutdown")
                state.StepsFailed.Add("LogShutdown")
            End Try

            ' Step 4 & 5: Initialize new log files in parallel
            Try
                ' Initialize new logs
                Await Task.WhenAll(
                    Task.Run(Sub()
                                 Try
                                     LogService.Instance.Init()
                                     Log.Write("PerformMidnightUpdate: New log file created for " & Now.ToString("yyyy-MM-dd"))
                                 Catch ex As Exception
                                     ' Use fallback logging since main log might not be available
                                     Try
                                         File.AppendAllText(Path.Combine(Globals.LogDir, "midnight_error.log"),
                                             $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Error creating log file: {ex.Message}{Environment.NewLine}")
                                     Catch
                                         ' Last resort: do nothing, we tried
                                     End Try
                                 End Try
                             End Sub),
                    Task.Run(Sub()
                                 Try
                                     UdpLogService.Instance.Init()
                                     Log.Write("PerformMidnightUpdate: New UDP log file created for " & Now.ToString("yyyy-MM-dd"))
                                 Catch ex As Exception
                                     Log.WriteException(ex, "PerformMidnightUpdate: Error creating new UDP log file")
                                 End Try
                             End Sub)
                )

                ' Verify log files were created successfully
                If VerifyLogFilesCreated() Then
                    state.StepsCompleted.Add("LogInit")
                Else
                    Log.Write("PerformMidnightUpdate: Warning - Log file creation could not be fully verified")
                    state.StepsFailed.Add("LogInit")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error during log initialization")
                state.StepsFailed.Add("LogInit")
            End Try

            ' Step 6: Perform log maintenance
            Try
                MaintenanceRoutines.PerformLogMaintenance()
                state.StepsCompleted.Add("LogMaintenance")
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error performing log maintenance")
                state.StepsFailed.Add("LogMaintenance")
            End Try

            ' Step 7: Fetch yesterday's rain total with timeout
            Try
                Log.Write("PerformMidnightUpdate: Fetching yesterday's rain total...")

                Dim rainTask = TempestDataRoutines.FetchRainDataAsync()
                Dim timeoutTask = Task.Delay(TimeSpan.FromSeconds(RainFetchTimeoutSeconds))
                Dim completedTask = Await Task.WhenAny(rainTask, timeoutTask)

                If completedTask Is rainTask Then
                    ' Rain fetch completed successfully
                    Dim rainData = Await rainTask
                    Log.Write($"PerformMidnightUpdate: Yesterday's rain cached: {rainData.YesterdayAccum:F2} inches")
                    state.StepsCompleted.Add("RainFetch")
                Else
                    ' Timeout occurred
                    Log.Write($"PerformMidnightUpdate: Rain fetch timed out after {RainFetchTimeoutSeconds} seconds")
                    state.StepsFailed.Add("RainFetch")
                End If
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error fetching yesterday's rain")
                state.StepsFailed.Add("RainFetch")
            End Try

            ' NOTE: Daily rain accumulation for Tempest devices (_dailyRainAccumulation) is automatically
            ' reset in ParseAndDisplayObservation when the date changes (no explicit reset needed here)

            stopwatch.Stop()
            state.TotalDurationMs = stopwatch.ElapsedMilliseconds

            ' Update state tracking
            If state.StepsFailed.Count = 0 Then
                state.LastSuccessfulRun = state.LastAttemptedRun
            End If

            ' Save state to file for persistence
            SaveMidnightState(state)

            ' Log summary
            Dim summary As String = $"PerformMidnightUpdate: Midnight maintenance complete in {state.TotalDurationMs}ms - " &
                                   $"Completed: {state.StepsCompleted.Count} steps, " &
                                   $"Failed: {state.StepsFailed.Count} steps"
            If state.StepsFailed.Count > 0 Then
                summary &= $" (Failed steps: {String.Join(", ", state.StepsFailed)})"
            End If
            Log.Write(summary)

            ' Trigger UI notification if available
            TriggerMidnightCompleteNotification(state)
        Catch ex As Exception
            stopwatch.Stop()
            state.TotalDurationMs = stopwatch.ElapsedMilliseconds
            Log.WriteException(ex, "PerformMidnightUpdate: Critical error during midnight update")
            SaveMidnightState(state)
        End Try
    End Function

    ''' <summary>
    ''' Synchronous wrapper for backwards compatibility.
    ''' </summary>
    Friend Sub PerformMidnightUpdate()
        ' Run async method synchronously (safe because we're already on a background thread from Task.Run)
        PerformMidnightUpdateAsync().GetAwaiter().GetResult()
    End Sub

    ''' <summary>
    ''' Verify that new log files were created successfully.
    ''' </summary>
    Private Function VerifyLogFilesCreated() As Boolean
        Try
            Dim expectedLogFile = Path.Combine(Globals.LogDir, $"TempestDisplay_{DateTime.Now:yyyy-MM-dd}.log")
            Dim expectedUdpLogFile = Path.Combine(Globals.LogDir, $"UdpData_{DateTime.Now:yyyy-MM-dd}.log")

            Dim mainLogExists = File.Exists(expectedLogFile)
            Dim udpLogExists = File.Exists(expectedUdpLogFile)

            If Not mainLogExists Then
                Log.Write($"PerformMidnightUpdate: Warning - Main log file not found: {expectedLogFile}")
            End If

            If Not udpLogExists Then
                Log.Write($"PerformMidnightUpdate: Warning - UDP log file not found: {expectedUdpLogFile}")
            End If

            Return mainLogExists AndAlso udpLogExists
        Catch ex As Exception
            Log.WriteException(ex, "PerformMidnightUpdate: Error verifying log files")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Save midnight state to file for persistence across app restarts.
    ''' </summary>
    Private Sub SaveMidnightState(state As MidnightState)
        Try
            Dim fullStatePath As String = Path.Combine(Application.StartupPath, StateFilePath)
            Dim stateDir As String = Path.GetDirectoryName(fullStatePath)

            If Not Directory.Exists(stateDir) Then
                Directory.CreateDirectory(stateDir)
            End If

            Dim json As String = System.Text.Json.JsonSerializer.Serialize(state, JsonOptions)
            File.WriteAllText(fullStatePath, json)
        Catch ex As Exception
            Log.WriteException(ex, "PerformMidnightUpdate: Error saving midnight state")
        End Try
    End Sub

    ''' <summary>
    ''' Load the last midnight state from file.
    ''' </summary>
    Friend Function LoadMidnightState() As MidnightState?
        Try
            Dim fullStatePath As String = Path.Combine(Application.StartupPath, StateFilePath)
            If File.Exists(fullStatePath) Then
                Dim json As String = File.ReadAllText(fullStatePath)
                Return System.Text.Json.JsonSerializer.Deserialize(Of MidnightState)(json)
            End If
        Catch ex As Exception
            Log.WriteException(ex, "LoadMidnightState: Error loading midnight state")
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Check if midnight maintenance needs to be retried for failed steps.
    ''' </summary>
    Friend Function ShouldRetryMidnightMaintenance() As Boolean
        Try
            Dim state = LoadMidnightState()
            If state.HasValue Then
                ' Retry if last attempt failed and was within last 24 hours
                If state.Value.StepsFailed.Count > 0 AndAlso
                   (DateTime.Now - state.Value.LastAttemptedRun).TotalHours < 24 Then
                    Return True
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "ShouldRetryMidnightMaintenance: Error checking retry status")
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Trigger UI notification that midnight maintenance completed.
    ''' </summary>
    Private Sub TriggerMidnightCompleteNotification(state As MidnightState)
        Try
            ' You can implement UI notification here if needed
            ' For now, just ensure it's in the logs
            If state.StepsFailed.Count = 0 Then
                Log.Write("PerformMidnightUpdate: All maintenance steps completed successfully")
            Else
                Log.Write("PerformMidnightUpdate: Maintenance completed with errors - check logs for details")
            End If
        Catch ex As Exception
            ' Don't let notification failures affect maintenance
            Log.WriteException(ex, "PerformMidnightUpdate: Error triggering completion notification")
        End Try
    End Sub

End Module