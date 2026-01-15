Partial Class FrmMain

    Private _helpManager As TempestDisplay.Help.HelpSystemManager

    ''' <summary>
    ''' Initializes the help system when the Help tab is first accessed
    ''' </summary>
    Private Sub InitializeHelpTab()
        Try
            ' Create help manager with logging callback
            _helpManager = New TempestDisplay.Help.HelpSystemManager(AddressOf Log.Write)

            ' Initialize with TpHelp tab controls
            _helpManager.Initialize(
                TvHelp,
                RtbHelp,
                TxtHelpSearch,
                BtnHelpPrint,
                BtnHelpExport
            )

            ' Set up F1 key for context-sensitive help
            Me.KeyPreview = True
            AddHandler Me.KeyDown, AddressOf FrmMain_KeyDown_Help

            Log.Write("[Help] Help tab initialized successfully")
        Catch ex As Exception
            Log.WriteException(ex, "[Help] Error initializing help tab")
        End Try
    End Sub

    ''' <summary>
    ''' Handle F1 key for context-sensitive help
    ''' </summary>
    Private Sub FrmMain_KeyDown_Help(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.F1 Then
            e.Handled = True
            e.SuppressKeyPress = True

            ' Show help for the focused control
            Dim focusedControl = Me.ActiveControl
            If focusedControl IsNot Nothing Then
                ShowContextSensitiveHelp(focusedControl)
            Else
                ' Show general help
                Tc.SelectedTab = TpHelp
            End If
        End If
    End Sub

    ''' <summary>
    ''' Shows context-sensitive help for a specific control
    ''' </summary>
    Private Sub ShowContextSensitiveHelp(control As Control)
        Try
            ' Switch to help tab
            Tc.SelectedTab = TpHelp

            ' Get topic ID for this control
            Dim topicId = TempestDisplay.Help.HelpSystemManager.GetTopicIdForControl(control.Name)

            If topicId IsNot Nothing Then
                _helpManager?.NavigateToTopic(topicId)
            Else
                ' Try parent control if no direct mapping
                If control.Parent IsNot Nothing Then
                    ShowContextSensitiveHelp(control.Parent)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[Help] Error showing context help")
        End Try
    End Sub

    ''' <summary>
    ''' Navigates to a specific help topic by ID
    ''' </summary>
    Public Sub ShowHelpTopic(topicId As String)
        Try
            ' Switch to help tab
            Tc.SelectedTab = TpHelp

            ' Navigate to the topic
            _helpManager?.NavigateToTopic(topicId)
        Catch ex As Exception
            Log.WriteException(ex, $"[Help] Error showing help topic: {topicId}")
        End Try
    End Sub

    ''' <summary>
    ''' Adds tooltips with help hints to controls
    ''' </summary>
    Private Sub SetupHelpTooltips()
        Try
            ' Station Data
            TTip.SetToolTip(TxtStationName, "Enter a friendly name for your weather station. Press F1 for help.")
            TTip.SetToolTip(TxtStationID, "Your WeatherFlow station ID from tempestwx.com. Press F1 for help.")
            TTip.SetToolTip(TxtStationElevation, "Station elevation in feet above sea level. Press F1 for help.")

            ' Tempest Settings
            TTip.SetToolTip(TxtDeviceID, "Your Tempest device serial number (starts with ST-). Press F1 for help.")
            TTip.SetToolTip(TxtApiToken, "Your personal WeatherFlow API token. Press F1 for help.")
            TTip.SetToolTip(NudTempestInterval, "How often to fetch data from the API (seconds). Press F1 for help.")

            ' MeteoBridge
            TTip.SetToolTip(TxtLogin, "MeteoBridge login username. Press F1 for help.")
            TTip.SetToolTip(TxtPassword, "MeteoBridge password. Press F1 for help.")
            TTip.SetToolTip(TxtIp, "MeteoBridge IP address on your local network. Press F1 for help.")

            ' Rain Gauge Limits
            TTip.SetToolTip(NudRainTodayLimit, "Maximum value for today's rain gauge display. Press F1 for help.")
            TTip.SetToolTip(NudRainYdayLimit, "Maximum value for yesterday's rain gauge display. Press F1 for help.")
            TTip.SetToolTip(NumRainMonthLimit, "Maximum value for monthly rain gauge display. Press F1 for help.")
            TTip.SetToolTip(NudRainYearLimit, "Maximum value for yearly rain gauge display. Press F1 for help.")
            TTip.SetToolTip(NudRainAlltimeLimit, "Maximum value for all-time rain gauge display. Press F1 for help.")

            ' Log Settings
            TTip.SetToolTip(TxtLogDays, "Number of days to keep log files before deletion. Press F1 for help.")

            Log.Write("[Help] Help tooltips configured")
        Catch ex As Exception
            Log.WriteException(ex, "[Help] Error setting up help tooltips")
        End Try
    End Sub

End Class