Imports System.ComponentModel
Imports System.IO

Public Class FrmMain

    ''DO NOT DELETE: https://weatherflow.github.io/Tempest/api/udp/v171/

    Private _previousTab As TabPage
    Private _lastDataFetchTime As DateTime = DateTime.MinValue
    Private _dataFetchIntervalSeconds As Integer = 180 ' Default 3 minutes
    Private _settingsWatcher As FileSystemWatcher
    Private _settingsReloadTimer As Timer
    Private _settingsReloadPending As Boolean = False
    Private _lastMidnightCheck As DateTime = DateTime.MinValue
    Private _nextMidnight As DateTime
    Private _udpListener As WeatherFlowUdpListener ' UDP listener for real-time weather data
    Private _dailyRainAccumulation As Double = 0.0 ' Accumulates rain for Tempest obs_st throughout the day
    Private _lastRainResetDate As Date = Date.MinValue ' Track when we last reset rain accumulation

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        '  Log.Write("FrmMain_Load: Entered")

        ' Apply initial location and increment AppStarts before any log file creation
        ApplyInitialFormLocation(Me)
        ' FolderRoutines.CreateAppFolders() should remain before log creation
        FolderRoutines.CreateAppFolders()
        ' REMOVE redundant log file creation here
        LogService.Instance.Init()

        ' Initialize UDP log
        UdpLogService.Instance.Init()

        Try
            Text = Application.ProductName
            TsslVer.Text = Application.ProductVersion
            TsslCpy.Text = Cpy
            TsslTimesrun.Text = Globals.AppStarts.ToString

            ' Load lightning icon from Resources folder
            Dim iconPath = Path.Combine(Application.StartupPath, "Resources", "lightning_64.png")
            If File.Exists(iconPath) Then
                Using img = Image.FromFile(iconPath)
                    ' Create a 16x16 version for the label
                    LblLightningDataTitle.Image = New Bitmap(img, New Size(16, 16))
                End Using
            Else
                Log.Write($"Lightning icon not found at: {iconPath}")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error setting form text/status")
        End Try

        InitializeCustomcontrols()
        ' Load all settings to controls in one call
        LoadAllSettingsToControls(TxtLogin, TxtPassword, TxtIp, TxtDeviceID, TxtApiToken, NudTempestInterval,
                                  NudRainTodayLimit, NudRainYdayLimit, NumRainMonthLimit, NudRainYearLimit, NudRainAlltimeLimit,
                                  TxtLogDays, TxtStationName, TxtStationID, TxtStationElevation)

        PerformLogMaintenance()

        ' Initialize midnight tracking
        _lastMidnightCheck = DateTime.Now
        _nextMidnight = CalculateNextMidnight()
        Log.Write($"Next midnight update scheduled for: {_nextMidnight:yyyy-MM-dd HH:mm:ss}")

        ' Initialize UDP listener for real-time weather data
        Try
            InitializeUdpListener()
            CreateHubStatusGrid()
            CreateObsStGrid()
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize UDP listener, will use REST API fallback")
        End Try

        ' Initialize battery history file and load data (chart will be initialized when Settings tab is accessed)
        Try
            InitializeBatteryHistoryFile()
            LoadBatteryHistory()
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize battery history")
        End Try

        ' Load settings JSON into RtbSettings
        LoadSettingsJsonToRtb()

        ' Setup watcher
        SetupSettingsFileWatcher()

        ' Start timer last (after everything is initialized)
        TmrClock.Start()

        Log.Write("FrmMain_Load: Exited")
    End Sub

    Private Sub InitializeCustomcontrols()
        Try
            ' Temperature gauges
            If TgCurrentTemp IsNot Nothing Then
                TgCurrentTemp.Label = "Current Temperature"
                TgCurrentTemp.BackColor = Color.AntiqueWhite
                TgCurrentTemp.Font = New Font(TgCurrentTemp.Font, FontStyle.Bold)
            Else
                ' Create TgCurrentTemp control dynamically if it doesn't exist
                Log.Write("TgCurrentTemp control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        TgCurrentTemp = New Controls.TempGaugeControl With {
                            .Label = "Current Temperature",
                            .BackColor = Color.AntiqueWhite,
                            .Dock = DockStyle.Fill
                        }
                        TgCurrentTemp.Font = New Font(TgCurrentTemp.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 0 with column span of 2
                        TlpData.Controls.Add(TgCurrentTemp, 0, 0)
                        TlpData.SetColumnSpan(TgCurrentTemp, 2)

                        Log.Write("TgCurrentTemp control created and added to TlpData[0,0] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add TgCurrentTemp")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating TgCurrentTemp control dynamically")
                End Try
            End If

            If TgFeelsLike IsNot Nothing Then
                TgFeelsLike.Label = "Feels Like"
                TgFeelsLike.BackColor = Color.AntiqueWhite
                TgFeelsLike.Font = New Font(TgFeelsLike.Font, FontStyle.Bold)
            Else
                ' Create TgFeelsLike control dynamically if it doesn't exist
                Log.Write("TgFeelsLike control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        TgFeelsLike = New Controls.TempGaugeControl With {
                            .Label = "Feels Like",
                            .BackColor = Color.AntiqueWhite,
                            .Dock = DockStyle.Fill
                        }
                        TgFeelsLike.Font = New Font(TgFeelsLike.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 1 with column span of 2
                        TlpData.Controls.Add(TgFeelsLike, 0, 1)
                        TlpData.SetColumnSpan(TgFeelsLike, 2)

                        Log.Write("TgFeelsLike control created and added to TlpData[0,1] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add TgFeelsLike")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating TgFeelsLike control dynamically")
                End Try
            End If

            If TgDewpoint IsNot Nothing Then
                TgDewpoint.Label = "Dew Point"
                TgDewpoint.BackColor = Color.AntiqueWhite
                TgDewpoint.Font = New Font(TgDewpoint.Font, FontStyle.Bold)
            Else
                ' Create TgDewpoint control dynamically if it doesn't exist
                Log.Write("TgDewpoint control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        TgDewpoint = New Controls.TempGaugeControl With {
                            .Label = "Dew Point",
                            .BackColor = Color.AntiqueWhite,
                            .Dock = DockStyle.Fill
                        }
                        TgDewpoint.Font = New Font(TgDewpoint.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 2 with column span of 2
                        TlpData.Controls.Add(TgDewpoint, 0, 2)
                        TlpData.SetColumnSpan(TgDewpoint, 2)

                        Log.Write("TgDewpoint control created and added to TlpData[0,2] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add TgDewpoint")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating TgDewpoint control dynamically")
                End Try
            End If

            ' Humidity gauge
            If FgRH IsNot Nothing Then
                FgRH.Label = "Relative Humidity"
                FgRH.BackColor = Color.AntiqueWhite
                FgRH.Font = New Font(FgRH.Font, FontStyle.Bold)
            Else
                ' Create FgRH control dynamically if it doesn't exist
                Log.Write("FgRH control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        FgRH = New Controls.FanGaugeControl With {
                            .Label = "Relative Humidity",
                            .BackColor = Color.AntiqueWhite,
                            .Dock = DockStyle.Fill
                        }
                        FgRH.Font = New Font(FgRH.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 3 with column span of 2
                        TlpData.Controls.Add(FgRH, 2, 0)
                        TlpData.SetColumnSpan(FgRH, 2)

                        Log.Write("FgRH control created and added to TlpData[2,0] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add FgRH")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating FgRH control dynamically")
                End Try
            End If

            ' Precipitation control
            If PTC IsNot Nothing Then
                PTC.BackColor = Color.AntiqueWhite
                PTC.Font = New Font("Arial", 7.75, FontStyle.Bold)
            Else
                ' Create PTC control dynamically if it doesn't exist
                Log.Write("PTC control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        PTC = New Controls.PrecipTowersControl With {
                            .BackColor = Color.AntiqueWhite,
                            .Dock = DockStyle.Fill
                        }
                        PTC.Font = New Font(PTC.Font.FontFamily, 8, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 4 with column span of 2
                        TlpData.Controls.Add(PTC, 6, 0)
                        TlpData.SetColumnSpan(PTC, 2)

                        Log.Write("PTC control created and added to TlpData[6,0] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add PTC")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating PTC control dynamically")
                End Try
            End If

            If WrWindSpeed IsNot Nothing Then
                WrWindSpeed.BackColor = Color.AntiqueWhite
                WrWindSpeed.Font = New Font(WrWindSpeed.Font, FontStyle.Bold)
                WrWindSpeed.Label = "Wind"
            Else
                Log.Write("WrWindSpeed control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        WrWindSpeed = New Controls.WindRoseControl With {
                            .BackColor = Color.AntiqueWhite,
                            .Dock = DockStyle.Fill,
                            .Label = "Wind"
                        }
                        WrWindSpeed.Font = New Font(WrWindSpeed.Font.FontFamily, 8, FontStyle.Bold)
                        ' Add to TableLayoutPanel at column 4, row 2 with column span of 2
                        TlpData.Controls.Add(WrWindSpeed, 4, 2)
                        TlpData.SetColumnSpan(WrWindSpeed, 2)
                        Log.Write("WrWindSpeed control created and added to TlpData[4,2] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add WrWindSpeed")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating WrWindSpeed control dynamically")
                End Try
            End If

            Log.Write("Initialized custom controls.")
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize custom controls")
        End Try
    End Sub

    Private Async Sub Tc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Tc.SelectedIndexChanged
        ' Capture all control values BEFORE any Await to avoid cross-thread issues
        Dim currentTabName As String = If(Tc.SelectedTab?.Name, "(unknown)")
        Dim current As TabPage = Tc.SelectedTab
        Dim previousTabName As String = If(_previousTab?.Name, "")

        Log.Write("Tab changed: " & currentTabName)

        Try
            If current IsNot Nothing AndAlso current.Name = "TpLogs" Then
                Log.Write("DEBUG: Entering TpLogs tab handler.")
                Log.Write("DEBUG: Globals.LogFile = " & Globals.LogFile)
                Log.Write("DEBUG: Globals.UdpLog = " & Globals.UdpLog)
                Log.Write("DEBUG: RtbLogs is Nothing? " & (RtbLogs Is Nothing).ToString())
                Log.Write("TpLogs tab entered, attempting to load log files.")

                ' Load main log file into RtbLogs
                Dim logText As String = String.Empty
                Dim logPath = Globals.LogFile

                If Not String.IsNullOrEmpty(logPath) AndAlso File.Exists(logPath) Then
                    Try
                        Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                            Using sr = New StreamReader(fs)
                                ' ConfigureAwait(True) ensures we return to UI thread
                                logText = Await sr.ReadToEndAsync().ConfigureAwait(True)
                            End Using
                        End Using
                        Log.Write("Main log file loaded successfully.")
                    Catch ex As Exception
                        Log.WriteException(ex, "Error reading main log file in TpLogs tab.")
                    End Try
                Else
                    Log.Write("Main log file path is empty or file does not exist: " & logPath)
                End If

                Try
                    UIService.SafeSetText(RtbLogs, logText)
                Catch ex As Exception
                    Log.WriteException(ex, "Error setting RtbLogs text.")
                End Try

                ' Load UDP log file into RtbUdp (assuming you have this control)
                Dim udpText As String = String.Empty
                Dim udpPath = Globals.UdpLog

                If Not String.IsNullOrEmpty(udpPath) AndAlso File.Exists(udpPath) Then
                    Try
                        Using fs = New FileStream(udpPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                            Using sr = New StreamReader(fs)
                                udpText = Await sr.ReadToEndAsync().ConfigureAwait(True)
                            End Using
                        End Using
                        Log.Write("UDP log file loaded successfully.")
                    Catch ex As Exception
                        Log.WriteException(ex, "Error reading UDP log file in TpLogs tab.")
                    End Try
                Else
                    Log.Write("UDP log file path is empty or file does not exist: " & udpPath)
                End If

                Try
                    ' Assuming you have a RichTextBox named RtbUdp on TpLogs tab
                    UIService.SafeSetText(RtbUDP, udpText)
                Catch ex As Exception
                    Log.WriteException(ex, "Error setting RtbUdp text.")
                End Try
            End If

            ' Initialize battery chart when Charts tab is first accessed
            ' Battery chart is on TpCharts, which is on TcLogs, which is on TpLogs
            If current IsNot Nothing AndAlso current.Name = "TpLogs" Then
                ' Check if the nested TcLogs tab control has TpCharts selected
                If TcLogs IsNot Nothing AndAlso TcLogs.SelectedTab IsNot Nothing AndAlso TcLogs.SelectedTab.Name = "TpCharts" Then
                    Log.Write("[Charts Tab] Entered Charts tab (nested in Logs), attempting to initialize battery chart")
                    Try
                        InitializeBatteryChart()
                        Log.Write("[Charts Tab] Battery chart initialization completed")
                    Catch ex As Exception
                        Log.WriteException(ex, "[Charts Tab] CRITICAL: Error initializing battery chart - this may cause issues")
                        ' Don't rethrow - allow tab to load even if chart fails
                    End Try
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error in Tc_SelectedIndexChanged")
        End Try

        ' Update _previousTab (safe because we captured 'current' before Await)
        _previousTab = current

        ' Clear both RichTextBoxes when leaving TpLogs tab (use captured values)
        If previousTabName = "TpLogs" AndAlso currentTabName <> "TpLogs" Then
            UIService.SafeSetText(RtbLogs, "")
            UIService.SafeSetText(RtbUDP, "")
        End If
    End Sub

    ''' <summary>
    ''' Handle tab changes in the nested TcLogs tab control
    ''' </summary>
    Private Sub TcLogs_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TcLogs.SelectedIndexChanged
        Try
            ' When switching to TpCharts tab within TcLogs, initialize the battery chart
            If TcLogs.SelectedTab IsNot Nothing AndAlso TcLogs.SelectedTab.Name = "TpCharts" Then
                Log.Write("[TcLogs] Switched to Charts tab, attempting to initialize battery chart")
                Try
                    InitializeBatteryChart()
                    Log.Write("[TcLogs] Battery chart initialization completed")
                Catch ex As Exception
                    Log.WriteException(ex, "[TcLogs] Error initializing battery chart")
                End Try
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error in TcLogs_SelectedIndexChanged")
        End Try
    End Sub

    Private Sub FrmMain_OnFormClosing(sender As Object, e As CancelEventArgs) Handles Me.FormClosing
        Log.Write("FrmMain_OnFormClosing: Entered")

        ' Cancel all pending async operations
        Try
            If Not AppCancellationTokenSource.IsCancellationRequested Then
                AppCancellationTokenSource.Cancel()
                Log.Write("Application cancellation token signaled")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error canceling application token")
        End Try

        ' Save battery history before shutdown
        Try
            SaveBatteryHistory()
        Catch ex As Exception
            Log.WriteException(ex, "Error saving battery history")
        End Try

        ' Cleanup UDP listener
        Try
            CleanupUdpListener()
        Catch ex As Exception
            Log.WriteException(ex, "Error cleaning up UDP listener")
        End Try

        Try
            SaveFormLocation(Me)
        Catch ex As Exception
            Log.WriteException(ex, "Error in SaveFormLocation")
        End Try
        Try
            SaveMeteoBridgeFromTextBoxes(TxtLogin, TxtPassword, TxtIp)
        Catch ex As Exception
            Log.WriteException(ex, "Error in SaveMeteoBridgeFromTextBoxes")
        End Try
        Try
            SaveTempestSettings(TxtDeviceID, TxtApiToken, NudTempestInterval)
        Catch ex As Exception
            Log.WriteException(ex, "Error in SaveTempestSettings")
        End Try
        Try
            SaveLogDays(TxtLogDays)
        Catch ex As Exception
            Log.WriteException(ex, "Error in SaveLogDays")
        End Try

        ' Shutdown UDP log service
        Try
            UdpLogService.Instance.Shutdown()
        Catch ex As Exception
            Log.WriteException(ex, "Error in UdpLogService.Instance.Shutdown")
        End Try

        Try
            LogService.Instance.Shutdown()
        Catch ex As Exception
            Log.WriteException(ex, "Error in LogService.Instance.Shutdown")
        End Try

        Try
            If _udpListener IsNot Nothing Then
                _udpListener.StopListening()
                _udpListener.Dispose()
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error stopping UDP listener")
        End Try

        Log.Write("FrmMain_OnFormClosing: Exited")
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _settingsWatcher IsNot Nothing Then
            _settingsWatcher.EnableRaisingEvents = False
            _settingsWatcher.Dispose()
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub SsTempest_Obs_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles SsObs_St.ItemClicked

    End Sub

    Private Sub SsSky_Obs_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs)

    End Sub

End Class