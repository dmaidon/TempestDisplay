Imports System.IO

Partial Public Class FrmMain

    Private Sub LoadSettingsJsonToRtb()
        Try
            Dim settingsPath = SettingsRoutines.GetSettingsFilePath()
            If File.Exists(settingsPath) Then
                RtbSettings.Text = File.ReadAllText(settingsPath)
            Else
                RtbSettings.Text = "(Settings file not found)"
            End If
        Catch ex As Exception
            RtbSettings.Text = "Error loading settings: " & ex.Message
        End Try
    End Sub

    Private Sub SetupSettingsFileWatcher()
        Try
            Dim settingsPath = SettingsRoutines.GetSettingsFilePath()
            Dim dir = Path.GetDirectoryName(settingsPath)
            Dim file = Path.GetFileName(settingsPath)

            _settingsWatcher = New FileSystemWatcher(dir, file) With {
                .NotifyFilter = NotifyFilters.LastWrite Or NotifyFilters.Size Or NotifyFilters.FileName
            }
            AddHandler _settingsWatcher.Changed, AddressOf OnSettingsFileChanged
            AddHandler _settingsWatcher.Renamed, AddressOf OnSettingsFileChanged
            AddHandler _settingsWatcher.Created, AddressOf OnSettingsFileChanged
            AddHandler _settingsWatcher.Deleted, AddressOf OnSettingsFileChanged
            _settingsWatcher.EnableRaisingEvents = True

            ' Setup debounce timer (500ms delay)
            _settingsReloadTimer = New Timer With {
                .Interval = 500,
                .Enabled = False
            }
            AddHandler _settingsReloadTimer.Tick, AddressOf OnSettingsReloadTimerTick
        Catch ex As Exception
            Log.WriteException(ex, "SetupSettingsFileWatcher: Failed to setup file watcher")
        End Try
    End Sub

    Private Sub OnSettingsFileChanged(sender As Object, e As FileSystemEventArgs)
        ' Debounce rapid file changes - just mark as pending and restart timer
        _settingsReloadPending = True
        _settingsReloadTimer.Stop()
        _settingsReloadTimer.Start()
    End Sub

    Private Sub LoadSettingsJsonToRtbAndControls()
        LoadSettingsJsonToRtb()
        SettingsRoutines.LoadRainLimitSettingsToNuds(NudRainTodayLimit, NudRainYdayLimit, NumRainMonthLimit, NudRainYearLimit, NudRainAlltimeLimit)
        ' Temporarily remove event handlers to prevent triggering save during load
        RemoveHandler TxtLogDays.TextChanged, AddressOf TxtLogDays_TextChanged
        RemoveHandler TxtStationName.TextChanged, AddressOf TxtStationName_TextChanged
        RemoveHandler TxtStationID.TextChanged, AddressOf TxtStationID_TextChanged
        RemoveHandler TxtStationElevation.TextChanged, AddressOf TxtStationElevation_TextChanged
        SettingsRoutines.PopulateLogDays(TxtLogDays)
        SettingsRoutines.PopulateStationName(TxtStationName)
        SettingsRoutines.PopulateStationID(TxtStationID)
        SettingsRoutines.PopulateStationElevation(TxtStationElevation)
        AddHandler TxtLogDays.TextChanged, AddressOf TxtLogDays_TextChanged
        AddHandler TxtStationName.TextChanged, AddressOf TxtStationName_TextChanged
        AddHandler TxtStationID.TextChanged, AddressOf TxtStationID_TextChanged
        AddHandler TxtStationElevation.TextChanged, AddressOf TxtStationElevation_TextChanged
    End Sub

    Private Sub OnSettingsReloadTimerTick(sender As Object, e As EventArgs)
        ' Timer elapsed, perform the actual reload
        _settingsReloadTimer.Stop()
        If Not _settingsReloadPending Then Return

        _settingsReloadPending = False

        ' FileSystemWatcher events are raised on a separate thread, so marshal to UI
        If RtbSettings.InvokeRequired Then
            RtbSettings.Invoke(New MethodInvoker(AddressOf LoadSettingsJsonToRtbAndControls))
        Else
            LoadSettingsJsonToRtbAndControls()
        End If

        Log.Write("Settings file reloaded after debounce")
    End Sub

    Private Sub SaveRainLimitSettingsFromNuds()
        SettingsRoutines.SaveRainLimitSettingsFromNuds(NudRainTodayLimit, NudRainYdayLimit, NumRainMonthLimit, NudRainYearLimit, NudRainAlltimeLimit)
    End Sub

    Private Sub NudRainTodayLimit_ValueChanged(sender As Object, e As EventArgs) Handles NudRainTodayLimit.ValueChanged
        SaveRainLimitSettingsFromNuds()
    End Sub

    Private Sub NudRainYdayLimit_ValueChanged(sender As Object, e As EventArgs) Handles NudRainYdayLimit.ValueChanged
        SaveRainLimitSettingsFromNuds()
    End Sub

    Private Sub NumRainMonthLimit_ValueChanged(sender As Object, e As EventArgs) Handles NumRainMonthLimit.ValueChanged
        SaveRainLimitSettingsFromNuds()
    End Sub

    Private Sub NudRainyearrLimit_ValueChanged(sender As Object, e As EventArgs) Handles NudRainYearLimit.ValueChanged
        SaveRainLimitSettingsFromNuds()
    End Sub

    Private Sub NudRainAlltimeLimit_ValueChanged(sender As Object, e As EventArgs) Handles NudRainAlltimeLimit.ValueChanged
        SaveRainLimitSettingsFromNuds()
    End Sub

    ' Select all text in NumericUpDown when focused or clicked
    Private Sub SelectAllNudText(nud As NumericUpDown)
        If nud.Controls.Count > 0 AndAlso TypeOf nud.Controls(1) Is TextBox Then
            Dim tb = DirectCast(nud.Controls(1), TextBox)
            tb.SelectAll()
        End If
    End Sub

    Private Sub NudRainTodayLimit_Enter(sender As Object, e As EventArgs) Handles NudRainTodayLimit.Enter, NudRainTodayLimit.Click
        SelectAllNudText(NudRainTodayLimit)
    End Sub

    Private Sub NudRainYdayLimit_Enter(sender As Object, e As EventArgs) Handles NudRainYdayLimit.Enter, NudRainYdayLimit.Click
        SelectAllNudText(NudRainYdayLimit)
    End Sub

    Private Sub NumRainMonthLimit_Enter(sender As Object, e As EventArgs) Handles NumRainMonthLimit.Enter, NumRainMonthLimit.Click
        SelectAllNudText(NumRainMonthLimit)
    End Sub

    Private Sub NudRainyearrLimit_Enter(sender As Object, e As EventArgs) Handles NudRainYearLimit.Enter, NudRainYearLimit.Click
        SelectAllNudText(NudRainYearLimit)
    End Sub

    Private Sub NudRainAlltimeLimit_Enter(sender As Object, e As EventArgs) Handles NudRainAlltimeLimit.Enter, NudRainAlltimeLimit.Click
        SelectAllNudText(NudRainAlltimeLimit)
    End Sub

    Private Sub NudTempestInterval_Enter(sender As Object, e As EventArgs) Handles NudTempestInterval.Enter, NudTempestInterval.Click
        SelectAllNudText(NudTempestInterval)
    End Sub

    Private Sub NudTempestInterval_ValueChanged(sender As Object, e As EventArgs) Handles NudTempestInterval.ValueChanged
        SettingsRoutines.SaveTempestSettings(TxtDeviceID, TxtApiToken, NudTempestInterval)
        ' Update fetch interval immediately
        _dataFetchIntervalSeconds = CInt(NudTempestInterval.Value)
        Log.Write($"Data fetch interval updated to {_dataFetchIntervalSeconds} seconds from NudTempestInterval")
    End Sub

    Private Sub TxtLogDays_TextChanged(sender As Object, e As EventArgs) Handles TxtLogDays.TextChanged
        SettingsRoutines.SaveLogDays(TxtLogDays)
    End Sub

    Private Sub TxtStationName_TextChanged(sender As Object, e As EventArgs) Handles TxtStationName.TextChanged
        SettingsRoutines.SaveStationName(TxtStationName)
        ' FrmMain title is updated automatically in SaveStationName
    End Sub

    Private Sub TxtStationID_TextChanged(sender As Object, e As EventArgs) Handles TxtStationID.TextChanged
        SettingsRoutines.SaveStationID(TxtStationID)
    End Sub

    Private Sub TxtStationElevation_TextChanged(sender As Object, e As EventArgs) Handles TxtStationElevation.TextChanged
        SettingsRoutines.SaveStationElevation(TxtStationElevation)
        LoadSettingsJsonToRtb()  ' Update RtbSettings to show the new value
    End Sub

    ' Only allow numerals and one decimal point in TxtStationElevation
    Private Sub TxtStationElevation_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TxtStationElevation.KeyPress
        ' Allow control keys (backspace, delete, etc.)
        If Char.IsControl(e.KeyChar) Then
            Return
        End If

        ' Allow digits
        If Char.IsDigit(e.KeyChar) Then
            Return
        End If

        ' Allow one decimal point
        If e.KeyChar = "."c AndAlso Not TxtStationElevation.Text.Contains(".") Then
            Return
        End If

        ' Block all other characters
        e.Handled = True
    End Sub

End Class