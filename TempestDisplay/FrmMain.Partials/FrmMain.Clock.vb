Partial Public Class FrmMain

    Private Sub TmrClock_Tick(sender As Object, e As EventArgs) Handles TmrClock.Tick
        Try
            ' Update clock display
            TsslClock.Text = Now.ToLongTimeString

            ' Update midnight countdown display
            UpdateMidnightCountdown()

            ' Check if battery history needs periodic save (every 5 minutes)
            CheckAndSaveBatteryHistory()

            ' Check for midnight update
            Dim currentTime = DateTime.Now
            If currentTime >= _nextMidnight Then
                Log.Write($"[TmrClock_Tick] Midnight reached, performing midnight update...")

                Try
                    ' Perform midnight maintenance
                    PerformMidnightUpdate()

                    ' Reset midnight tracking
                    _lastMidnightCheck = currentTime
                    _nextMidnight = CalculateNextMidnight()
                    Log.Write($"[TmrClock_Tick] Midnight update complete. Next midnight: {_nextMidnight:yyyy-MM-dd HH:mm:ss}")

                    ' Update display immediately after midnight reset
                    UpdateMidnightCountdown()
                Catch ex As Exception
                    Log.WriteException(ex, "[TmrClock_Tick] Error in PerformMidnightUpdate")
                End Try
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error in TmrClock_Tick")
        End Try
    End Sub

    Private Sub UpdateMidnightCountdown()
        Try
            Dim timeUntilMidnight = _nextMidnight - DateTime.Now

            If timeUntilMidnight.TotalSeconds <= 0 Then
                TsslMidnightCountdown.Text = "Midnight!"
                TsslMidnightCountdown.ForeColor = Color.Red
            ElseIf timeUntilMidnight.TotalMinutes >= 1 Then
                ' Show only minutes when 1 minute or more
                Dim totalMinutes = CInt(Math.Floor(timeUntilMidnight.TotalMinutes))
                TsslMidnightCountdown.Text = $"{totalMinutes} minutes"
                TsslMidnightCountdown.ForeColor = Color.ForestGreen
            Else
                ' Show only seconds when under 1 minute
                Dim seconds = CInt(Math.Ceiling(timeUntilMidnight.TotalSeconds))
                TsslMidnightCountdown.Text = $"{seconds} seconds"
                TsslMidnightCountdown.ForeColor = Color.DarkOrange
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error in UpdateMidnightCountdown")
        End Try
    End Sub

    Private Function CalculateNextMidnight() As DateTime
        ' Get tomorrow's date at midnight
        Dim tomorrow = DateTime.Today.AddDays(1)
        Return New DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0)
    End Function

End Class