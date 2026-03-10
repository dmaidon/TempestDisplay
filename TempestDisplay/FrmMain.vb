' Last Edit: March 10, 2026 (REMOVED FolderRoutines.CreateAppFolders() — moved to ApplicationEvents.Startup)
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
        ' REMOVED: FolderRoutines.CreateAppFolders() — now called in ApplicationEvents.Startup
        '          so directories exist before this form — and LogService.Init() — ever runs
        LogService.Instance.Init()

        ' Initialize UDP log
        UdpLogService.Instance.Init()

        ' Subscribe to error count changes
        AddHandler LogService.Instance.ErrorCountChanged, AddressOf OnErrorCountChanged

        Try
            Text = System.Windows.Forms.Application.ProductName
            Dim ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            TsslVer.Text = $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}"
            TsslCpy.Text = Cpy
            TsslTimesrun.Text = Globals.AppStarts.ToString

            ' Load lightning icon from Resources folder
            Dim iconPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Resources", "lightning_64.png")
            If File.Exists(iconPath) Then
                Using img = Drawing.Image.FromFile(iconPath)
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

        ' Ensure HiLo database exists
        Try
            HiLoDatabase.EnsureHiLoDatabase()
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize HiLo database")
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

        ' Initialize Settings Help System
        Try
            InitializeSettingsHelp()
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize settings help system")
        End Try

        ' Initialize RtbLogs context menu
        Try
            InitializeRtbLogsContextMenu()
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize RtbLogs context menu")
        End Try

        ' Initialize Help System
        Try
            InitializeHelpTab()
            SetupHelpTooltips()
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize help system")
        End Try

        ' Start timer last (after everything is initialized)
        TmrClock.Start()

        Log.Write("FrmMain_Load: Exited")
    End Sub

    Private Sub Tc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Tc.SelectedIndexChanged
        ' Capture all control values BEFORE any Await to avoid cross-thread issues
        Dim currentTabName As String = If(Tc.SelectedTab?.Name, "(unknown)")
        Dim current As TabPage = Tc.SelectedTab
        Dim previousTabName As String = If(_previousTab?.Name, "")

        Log.Write($"Tab changed: {currentTabName} (checking for TpRecords)")

        Try
            If current IsNot Nothing AndAlso current.Name = "TpLogs" Then
                Log.Write("DEBUG: Entering TpLogs tab handler.")
                Log.Write("DEBUG: Globals.LogFile = " & Globals.LogFile)
                Log.Write("DEBUG: Globals.UdpLog = " & Globals.UdpLog)
                Log.Write("DEBUG: RtbLogs is Nothing? " & (RtbLogs Is Nothing).ToString())
                Log.Write("TpLogs tab entered, populating LbLogs from LogDir and clearing RtbLogs.")

                ' Populate LbLogs with files from LogDir
                PopulateLogList()

                ' Clear RtbLogs to disconnect auto-population
                Try
                    UIService.SafeSetText(RtbLogs, "")
                Catch ex As Exception
                    Log.WriteException(ex, "Error clearing RtbLogs text on TpLogs enter.")
                End Try
            End If

            ' Initialize battery chart when Charts tab is selected
            If current IsNot Nothing AndAlso current.Name = "TpCharts" Then
                Log.Write("[Charts Tab] Entered Charts tab, attempting to initialize battery chart")
                Try
                    InitializeBatteryChart()
                    Log.Write("[Charts Tab] Battery chart initialization completed")
                Catch ex As Exception
                    Log.WriteException(ex, "[Charts Tab] CRITICAL: Error initializing battery chart - this may cause issues")
                End Try
            End If

            ' Load HiLo records when Records tab is accessed
            Log.Write($"[DEBUG] Checking Records tab: current IsNot Nothing = {current IsNot Nothing}, Name = '{current?.Name}'")
            If current IsNot Nothing AndAlso current.Name = "TpRecords" Then
                Log.Write("[Records Tab] Entered Records tab, loading HiLo records")
                Try
                    LoadHiLoRecords()
                    Log.Write("[Records Tab] HiLo records loaded successfully")
                Catch ex As Exception
                    Log.WriteException(ex, "[Records Tab] Error loading HiLo records")
                End Try
            Else
                Log.Write($"[DEBUG] Not Records tab - Name comparison failed: '{current?.Name}' vs 'TpRecords'")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "Error in Tc_SelectedIndexChanged")
        End Try

        ' Update _previousTab
        _previousTab = current

        ' Clear both RichTextBoxes when leaving TpLogs tab (use captured values)
        If previousTabName = "TpLogs" AndAlso currentTabName <> "TpLogs" Then
            UIService.SafeSetText(RtbLogs, "")
        End If
    End Sub

    ''' <summary>
    ''' Populate LbLogs with files from Globals.LogDir (sorted by LastWriteTime descending)
    ''' </summary>
    Private Sub PopulateLogList()
        Try
            LbLogs.BeginUpdate()
            LbLogs.Items.Clear()

            If Directory.Exists(Globals.LogDir) Then
                Dim logDirInfo As New DirectoryInfo(Globals.LogDir)
                Dim sorted As IEnumerable(Of FileInfo) = logDirInfo.
                    EnumerateFiles("*.log", SearchOption.TopDirectoryOnly).
                    OrderByDescending(Function(fi) fi.LastWriteTime)
                For Each fileInfo In sorted
                    ' Add only the filename for display
                    LbLogs.Items.Add(fileInfo.Name)
                Next
            Else
                Log.Write("PopulateLogList: LogDir does not exist: " & Globals.LogDir)
            End If
        Catch ex As Exception
            Log.WriteException(ex, "PopulateLogList: Error populating log list")
        Finally
            LbLogs.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' When user selects a log in LbLogs, load the file into RtbLogs
    ''' </summary>
    Private Sub LbLogs_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LbLogs.SelectedIndexChanged
        Try
            Dim fileName As String = TryCast(LbLogs.SelectedItem, String)
            If String.IsNullOrEmpty(fileName) Then Return

            Dim filePath As String = Path.Combine(Globals.LogDir, fileName)
            If Not File.Exists(filePath) Then
                Log.Write("LbLogs_SelectedIndexChanged: Selected file does not exist: " & filePath)
                Return
            End If

            Dim text As String = String.Empty
            Try
                Using fs = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using sr = New StreamReader(fs)
                        text = sr.ReadToEnd()
                    End Using
                End Using
            Catch ex As Exception
                Log.WriteException(ex, "LbLogs_SelectedIndexChanged: Error reading selected log file")
            End Try

            Try
                UIService.SafeSetText(RtbLogs, text)
            Catch ex As Exception
                Log.WriteException(ex, "LbLogs_SelectedIndexChanged: Error setting RtbLogs text")
            End Try
        Catch ex As Exception
            Log.WriteException(ex, "LbLogs_SelectedIndexChanged: Unexpected error")
        End Try
    End Sub

    ''' <summary>
    ''' Handle tab changes in the nested TcLogs tab control
    ''' </summary>
    Private Sub TcLogs_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TcLogs.SelectedIndexChanged
        Try
            ' TcLogs tab control handler - TpCharts is now in the main Tc tab control
            ' Add any future TcLogs-specific tab handling here if needed
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

    Private Sub TableLayoutPanel1_Paint(sender As Object, e As PaintEventArgs) Handles TableLayoutPanel1.Paint

    End Sub

    ''' <summary>
    ''' Handle error count changes from LogService
    ''' </summary>
    Private Sub OnErrorCountChanged(sender As Object, e As EventArgs)
        Try
            If InvokeRequired Then
                Invoke(New Action(Sub() UpdateErrorCountDisplay()))
            Else
                UpdateErrorCountDisplay()
            End If
        Catch ex As Exception
            ' Can't log this as it might cause recursion
        End Try
    End Sub

    ''' <summary>
    ''' Update the error count display on the status strip
    ''' </summary>
    Private Sub UpdateErrorCountDisplay()
        Try
            Dim count = Globals.ErrCount
            TsslErrCount.Text = count.ToString()

            ' Change color to red if there are errors, green otherwise
            If count > 0 Then
                TsslErrCount.ForeColor = Color.Red
            Else
                TsslErrCount.ForeColor = Color.ForestGreen
            End If
        Catch ex As Exception
            ' Can't log this as it might cause recursion
        End Try
    End Sub

    Private Sub TpHelp_Click(sender As Object, e As EventArgs) Handles TpHelp.Click

    End Sub
End Class