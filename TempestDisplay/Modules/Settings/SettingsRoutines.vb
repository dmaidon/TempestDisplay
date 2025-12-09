Imports System.IO
Imports System.Text.Json

Friend Module SettingsRoutines

    Private Const SettingsFolderName As String = "appSettings"
    Private Const SettingsFileName As String = "TempestDisplay_Settings.json"
    Private Const SettingsBackupFileName As String = "TempestDisplay_Settings.bak.json"

    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Friend Function GetSettingsDirectory() As String
        Dim dir = Path.Combine(Application.StartupPath, SettingsFolderName)
        If Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If
        Return dir
    End Function

    Friend Function GetSettingsFilePath() As String
        Return Path.Combine(GetSettingsDirectory(), SettingsFileName)
    End Function

    Friend Function GetSettingsBackupFilePath() As String
        Return Path.Combine(GetSettingsDirectory(), SettingsBackupFileName)
    End Function

    Friend Function LoadSettings() As AppSettings
        Dim primaryPath = GetSettingsFilePath()
        Dim backupPath = GetSettingsBackupFilePath()

        Dim s As AppSettings = TryLoad(primaryPath)
        If s IsNot Nothing Then Return s

        s = TryLoad(backupPath)
        If s IsNot Nothing Then
            Try
                SaveSettings(s)
            Catch ex As Exception
                Log.WriteException(ex, "LoadSettings: Error saving restored backup settings")
            End Try
            Return s
        End If

        Return AppSettings.CreateDefault()
    End Function

    Private Function TryLoad(path As String) As AppSettings
        Try
            If Not File.Exists(path) Then Return Nothing
            Dim json = File.ReadAllText(path)
            If String.IsNullOrWhiteSpace(json) Then Return Nothing
            Dim s = JsonSerializer.Deserialize(Of AppSettings)(json)
            If s Is Nothing Then Return Nothing
            s.Normalize()
            Return s
        Catch
            Return Nothing
        End Try
    End Function

    'Friend Sub LoadSettings(settings As AppSettings)
    '    If settings Is Nothing Then Return
    '    Dim primaryPath = GetSettingsFilePath()
    '    Dim backupPath = GetSettingsBackupFilePath()

    'End Sub

    Friend Sub SaveSettings(settings As AppSettings)
        If settings Is Nothing Then Return
        Dim primaryPath = GetSettingsFilePath()
        Dim backupPath = GetSettingsBackupFilePath()

        Try
            settings.Normalize()
            Dim json = JsonSerializer.Serialize(settings, JsonOptions)
            Dim tempPath = primaryPath & ".tmp"
            File.WriteAllText(tempPath, json)

            ' Use File.Replace for atomic operation (available on Windows)
            ' This safely replaces primaryPath with tempPath and backs up the old file
            If File.Exists(primaryPath) Then
                Try
                    ' File.Replace(sourceFileName, destinationFileName, destinationBackupFileName)
                    ' This is atomic and will not lose data if it fails
                    File.Replace(tempPath, primaryPath, backupPath)
                Catch ex As Exception
                    Log.WriteException(ex, "SaveSettings: File.Replace failed, attempting fallback")
                    ' Fallback to manual copy if Replace fails (e.g., cross-volume)
                    Try
                        If File.Exists(primaryPath) Then
                            File.Copy(primaryPath, backupPath, True)
                        End If
                    Catch ex2 As Exception
                        Log.WriteException(ex2, "SaveSettings: Failed to backup existing settings")
                    End Try

                    File.Copy(tempPath, primaryPath, True)
                    File.Delete(tempPath)
                End Try
            Else
                ' No existing file, just move temp to primary
                File.Move(tempPath, primaryPath)

                ' Create initial backup
                Try
                    File.Copy(primaryPath, backupPath, True)
                Catch ex As Exception
                    Log.WriteException(ex, "SaveSettings: Failed to create initial backup")
                End Try
            End If
        Catch ex As Exception
            Log.WriteException(ex, "SaveSettings: Critical error during settings save")
        End Try
    End Sub

    Friend Function GetInitialFormLocation() As Point
        Dim s = LoadSettings()
        Return New Point(s.FormX, s.FormY)
    End Function

    Friend Sub ApplyInitialFormLocation(form As Form)
        If form Is Nothing Then Return
        Dim s = LoadSettings()
        Dim pt As New Point(s.FormX, s.FormY)
        form.StartPosition = FormStartPosition.Manual
        form.Location = pt

        ' increment Timesrun after applying initial location
        s.Timesrun += 1
        SaveSettings(s)
        Globals.AppStarts = s.Timesrun
    End Sub

    Friend Sub SaveFormLocation(location As Point)
        Dim s = LoadSettings()
        s.FormX = location.X
        s.FormY = location.Y
        SaveSettings(s)
    End Sub

    Friend Sub SaveFormLocation(form As Form)
        If form Is Nothing Then Return
        SaveFormLocation(form.Location)
    End Sub

    Friend Sub PopulateMeteoBridgeTextBoxes(txtLogin As TextBox, txtPassword As TextBox, txtIp As TextBox)
        Dim s = LoadSettings()
        If s.MeteoBridge Is Nothing Then s.MeteoBridge = MeteoBridgeSettings.CreateDefault()
        If txtLogin IsNot Nothing Then txtLogin.Text = s.MeteoBridge.Login
        If txtPassword IsNot Nothing Then txtPassword.Text = s.MeteoBridge.Password
        If txtIp IsNot Nothing Then txtIp.Text = s.MeteoBridge.IpAddress
    End Sub

    Friend Sub SaveMeteoBridgeFromTextBoxes(txtLogin As TextBox, txtPassword As TextBox, txtIp As TextBox)
        Dim s = LoadSettings()
        If s.MeteoBridge Is Nothing Then s.MeteoBridge = MeteoBridgeSettings.CreateDefault()
        If txtLogin IsNot Nothing Then s.MeteoBridge.Login = txtLogin.Text
        If txtPassword IsNot Nothing Then s.MeteoBridge.Password = txtPassword.Text
        If txtIp IsNot Nothing Then s.MeteoBridge.IpAddress = txtIp.Text
        SaveSettings(s)
    End Sub

    Friend Sub PopulateTempestSettings(txtStationId As TextBox, txtApiToken As TextBox, nudTempestInterval As NumericUpDown)
        Dim s = LoadSettings()
        If s.Tempest Is Nothing Then s.Tempest = TempestSettings.CreateDefaultTempest()
        If txtStationId IsNot Nothing Then txtStationId.Text = s.Tempest.StationId
        If txtApiToken IsNot Nothing Then txtApiToken.Text = s.Tempest.ApiToken
        If nudTempestInterval IsNot Nothing Then nudTempestInterval.Value = s.Tempest.UpdateIntervalSeconds
    End Sub

    Friend Sub SaveTempestSettings(txtStationId As TextBox, txtApiToken As TextBox, nudInterval As NumericUpDown)
        Dim s = LoadSettings()
        If s.Tempest Is Nothing Then s.Tempest = TempestSettings.CreateDefaultTempest()
        If txtStationId IsNot Nothing Then s.Tempest.StationId = txtStationId.Text
        If txtApiToken IsNot Nothing Then s.Tempest.ApiToken = txtApiToken.Text
        If nudInterval IsNot Nothing Then s.Tempest.UpdateIntervalSeconds = CInt(nudInterval.Value)
        SaveSettings(s)
    End Sub

    ' Load
    Friend Sub PopulateRainLimitSettings(settings As RainLimitSettings,
        nudToday As NumericUpDown, nudYday As NumericUpDown, nudMonth As NumericUpDown, nudYear As NumericUpDown, nudAllTime As NumericUpDown)
        nudToday.Value = CDec(settings.DailyLimitInches)
        nudYday.Value = CDec(settings.YesterdayLimitInches)
        nudMonth.Value = CDec(settings.MonthLimitInches)
        nudYear.Value = CDec(settings.YearLimitInches)
        nudAllTime.Value = CDec(settings.AllTimeLimitInches)
    End Sub

    ' Save
    Friend Sub SaveRainLimitSettings(settings As RainLimitSettings,
        nudToday As NumericUpDown, nudYday As NumericUpDown, nudMonth As NumericUpDown, nudYear As NumericUpDown, nudAllTime As NumericUpDown)
        settings.DailyLimitInches = CSng(nudToday.Value)
        settings.YesterdayLimitInches = CSng(nudYday.Value)
        settings.MonthLimitInches = CSng(nudMonth.Value)
        settings.YearLimitInches = CSng(nudYear.Value)
        settings.AllTimeLimitInches = CSng(nudAllTime.Value)
    End Sub

    ' Populate LogDays into TxtLogDays
    Friend Sub PopulateLogDays(txtLogDays As TextBox)
        If txtLogDays Is Nothing Then Return
        Dim s = LoadSettings()
        txtLogDays.Text = s.LogDays.ToString()
    End Sub

    ' Save LogDays from TxtLogDays
    Friend Sub SaveLogDays(txtLogDays As TextBox)
        If txtLogDays Is Nothing Then Return
        Dim s = LoadSettings()
        Dim days As Integer
        If Integer.TryParse(txtLogDays.Text, days) Then
            s.LogDays = days
            SaveSettings(s)
        End If
    End Sub

    ' Populate StationName into TxtStationName and update FrmMain title
    Friend Sub PopulateStationName(txtStationName As TextBox)
        If txtStationName Is Nothing Then Return
        Dim s = LoadSettings()
        txtStationName.Text = If(String.IsNullOrEmpty(s.StationName), "", s.StationName)

        ' Update FrmMain title with station name
        If FrmMain IsNot Nothing Then
            FrmMain.Text = If(String.IsNullOrEmpty(s.StationName), "TempestDisplay", $"TempestDisplay - {s.StationName}")
        End If
    End Sub

    ' Save StationName from TxtStationName and update FrmMain title
    Friend Sub SaveStationName(txtStationName As TextBox)
        If txtStationName Is Nothing Then Return
        Dim s = LoadSettings()
        s.StationName = If(txtStationName.Text, "")
        SaveSettings(s)

        ' Update FrmMain title immediately after saving
        If FrmMain IsNot Nothing Then
            FrmMain.Text = If(String.IsNullOrEmpty(s.StationName), "TempestDisplay", $"TempestDisplay - {s.StationName}")
        End If
    End Sub

    ' Populate StationID into TxtStationID
    Friend Sub PopulateStationID(txtStationID As TextBox)
        If txtStationID Is Nothing Then Return
        Dim s = LoadSettings()
        txtStationID.Text = If(String.IsNullOrEmpty(s.StationID), "", s.StationID)
    End Sub

    ' Save StationID from TxtStationID
    Friend Sub SaveStationID(txtStationID As TextBox)
        If txtStationID Is Nothing Then Return
        Dim s = LoadSettings()
        s.StationID = If(txtStationID.Text, "")
        SaveSettings(s)
    End Sub

    ' Populate StationElevation into TxtStationElevation
    Friend Sub PopulateStationElevation(txtStationElevation As TextBox)
        If txtStationElevation Is Nothing Then Return
        Dim s = LoadSettings()
        txtStationElevation.Text = s.StationElevation.ToString("F1")
    End Sub

    ' Save StationElevation from TxtStationElevation
    Friend Sub SaveStationElevation(txtStationElevation As TextBox)
        If txtStationElevation Is Nothing Then Return
        Dim s = LoadSettings()
        Dim elevation As Double
        If Double.TryParse(txtStationElevation.Text, elevation) Then
            s.StationElevation = elevation
            SaveSettings(s)
        End If
    End Sub

    ' Master routine to load ALL settings to their respective controls
    ' Call this once from FrmMain_Load to populate all settings
    Friend Sub LoadAllSettingsToControls(
        txtMBLogin As TextBox,
        txtMBPassword As TextBox,
        txtMBIp As TextBox,
        txtDeviceID As TextBox,
        txtApiToken As TextBox,
        nudTempestInterval As NumericUpDown,
        nudRainToday As NumericUpDown,
        nudRainYday As NumericUpDown,
        nudRainMonth As NumericUpDown,
        nudRainYear As NumericUpDown,
        nudRainAllTime As NumericUpDown,
        txtLogDays As TextBox,
        txtStationName As TextBox,
        txtStationID As TextBox,
        txtStationElevation As TextBox)

        Try
            PopulateMeteoBridgeTextBoxes(txtMBLogin, txtMBPassword, txtMBIp)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in PopulateMeteoBridgeTextBoxes")
        End Try

        Try
            PopulateTempestSettings(txtDeviceID, txtApiToken, nudTempestInterval)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in PopulateTempestSettings")
        End Try

        Try
            LoadRainLimitSettingsToNuds(nudRainToday, nudRainYday, nudRainMonth, nudRainYear, nudRainAllTime)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in LoadRainLimitSettingsToNuds")
        End Try

        Try
            PopulateLogDays(txtLogDays)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in PopulateLogDays")
        End Try

        Try
            PopulateStationName(txtStationName)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in PopulateStationName")
        End Try

        Try
            PopulateStationID(txtStationID)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in PopulateStationID")
        End Try

        Try
            PopulateStationElevation(txtStationElevation)
        Catch ex As Exception
            Log.WriteException(ex, "LoadAllSettingsToControls: Error in PopulateStationElevation")
        End Try
    End Sub

    ' Load rain limit settings to NumericUpDown controls
    Friend Sub LoadRainLimitSettingsToNuds(
        nudToday As NumericUpDown,
        nudYday As NumericUpDown,
        nudMonth As NumericUpDown,
        nudYear As NumericUpDown,
        nudAllTime As NumericUpDown)

        If nudToday Is Nothing OrElse nudYday Is Nothing OrElse
           nudMonth Is Nothing OrElse nudYear Is Nothing OrElse
           nudAllTime Is Nothing Then Return

        Dim s = LoadSettings()
        If s Is Nothing OrElse s.RainLimit Is Nothing Then
            ' Use hardcoded defaults if settings are missing
            nudToday.Value = 5
            nudYday.Value = 5
            nudMonth.Value = 15
            nudYear.Value = 50
            nudAllTime.Value = 400
            Return
        End If

        ' Clamp values to NUD min/max to avoid exceptions
        nudToday.Value = CDec(Math.Max(nudToday.Minimum, Math.Min(nudToday.Maximum, s.RainLimit.DailyLimitInches)))
        nudYday.Value = CDec(Math.Max(nudYday.Minimum, Math.Min(nudYday.Maximum, s.RainLimit.YesterdayLimitInches)))
        nudMonth.Value = CDec(Math.Max(nudMonth.Minimum, Math.Min(nudMonth.Maximum, s.RainLimit.MonthLimitInches)))
        nudYear.Value = CDec(Math.Max(nudYear.Minimum, Math.Min(nudYear.Maximum, s.RainLimit.YearLimitInches)))
        nudAllTime.Value = CDec(Math.Max(nudAllTime.Minimum, Math.Min(nudAllTime.Maximum, s.RainLimit.AllTimeLimitInches)))
    End Sub

    ' Save rain limit settings from NumericUpDown controls
    Friend Sub SaveRainLimitSettingsFromNuds(
        nudToday As NumericUpDown,
        nudYday As NumericUpDown,
        nudMonth As NumericUpDown,
        nudYear As NumericUpDown,
        nudAllTime As NumericUpDown)

        If nudToday Is Nothing OrElse nudYday Is Nothing OrElse
           nudMonth Is Nothing OrElse nudYear Is Nothing OrElse
           nudAllTime Is Nothing Then Return

        Dim s = LoadSettings()
        If s Is Nothing OrElse s.RainLimit Is Nothing Then Return

        s.RainLimit.DailyLimitInches = CSng(nudToday.Value)
        s.RainLimit.YesterdayLimitInches = CSng(nudYday.Value)
        s.RainLimit.MonthLimitInches = CSng(nudMonth.Value)
        s.RainLimit.YearLimitInches = CSng(nudYear.Value)
        s.RainLimit.AllTimeLimitInches = CSng(nudAllTime.Value)
        SaveSettings(s)
    End Sub

End Module