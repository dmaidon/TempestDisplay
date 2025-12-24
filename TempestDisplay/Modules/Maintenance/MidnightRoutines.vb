Friend Module MidnightRoutines

    Friend Sub PerformMidnightUpdate()
        Try
            Log.Write("PerformMidnightUpdate: Starting midnight maintenance...")

            ' Reset error count for the new day
            Globals.ErrCount = 0
            ' Trigger UI update for error count reset
            Try
                LogService.Instance.TriggerErrorCountUpdate()
            Catch
                ' Ignore if event raising fails
            End Try
            Log.Write("PerformMidnightUpdate: Error count reset to 0")

            ' Close current log file
            Try
                Log.Write("PerformMidnightUpdate: Closing current log file")
                LogService.Instance.Shutdown()
            Catch ex As Exception
                ' Can't log this since log is shut down, just continue
            End Try

            ' Close current UDP log file
            Try
                UdpLogService.Instance.Shutdown()
            Catch ex As Exception
                ' Can't log this since log is shut down, just continue
            End Try

            ' Create new log file with current date
            Try
                ' Reinitialize LogService which will create new log file
                LogService.Instance.Init()
                Log.Write("PerformMidnightUpdate: New log file created for " & Now.ToString("yyyy-MM-dd"))
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error creating new log file")
            End Try

            ' Create new UDP log file with current date
            Try
                UdpLogService.Instance.Init()
                Log.Write("PerformMidnightUpdate: New UDP log file created for " & Now.ToString("yyyy-MM-dd"))
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error creating new UDP log file")
            End Try

            ' Perform log maintenance to clean old files
            Try
                MaintenanceRoutines.PerformLogMaintenance()
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error performing log maintenance")
            End Try

            ' Fetch yesterday's rain total (today is now yesterday)
            Try
                Log.Write("PerformMidnightUpdate: Fetching yesterday's rain total...")
                ' Synchronous wait for the async method (we're already in a background thread)
                Dim rainData = TempestDataRoutines.FetchRainDataAsync().GetAwaiter().GetResult()
                Log.Write($"PerformMidnightUpdate: Yesterday's rain cached: {rainData.YesterdayAccum:F2} inches")
            Catch ex As Exception
                Log.WriteException(ex, "PerformMidnightUpdate: Error fetching yesterday's rain")
            End Try

            ' NOTE: Daily rain accumulation for Tempest devices (_dailyRainAccumulation) is automatically
            ' reset in ParseAndDisplayObservation when the date changes (no explicit reset needed here)

            Log.Write("PerformMidnightUpdate: Midnight maintenance complete")
        Catch ex As Exception
            Log.WriteException(ex, "PerformMidnightUpdate: Error during midnight update")
        End Try
    End Sub

End Module