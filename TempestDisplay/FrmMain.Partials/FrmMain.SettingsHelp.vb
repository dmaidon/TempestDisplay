Imports TempestDisplay.Help

Partial Public Class FrmMain

    Private _settingsHelpViewer As HelpViewer
    Private _settingsTabControl As TabControl
    Private _tpSetHelp As TabPage

    ''' <summary>
    ''' Initializes the settings help system by reorganizing the settings tab
    ''' to use sub-tabs with a dedicated help tab.
    ''' </summary>
    Private Sub InitializeSettingsHelp()
        Try
            ' Check if already initialized
            If _settingsTabControl IsNot Nothing Then
                Return
            End If

            ' Create a new TabControl to hold settings sub-tabs
            _settingsTabControl = New TabControl With {
                .Dock = DockStyle.Fill,
                .Name = "TcSettings"
            }

            ' Move existing PnlSettings to a new "General" tab
            Dim tpGeneral As New TabPage("General") With {
                .Name = "TpSetOptions"
            }

            ' Remove PnlSettings from TpSettings and add to tpGeneral
            If TpSettings.Controls.Contains(PnlSettings) Then
                TpSettings.Controls.Remove(PnlSettings)
                tpGeneral.Controls.Add(PnlSettings)
                PnlSettings.Dock = DockStyle.Fill
            End If

            ' Create placeholder tabs for future use
            Dim tpApiKeys As New TabPage("API Keys") With {
                .Name = "TpApiKeys"
            }
            Dim lblApiKeys As New Label With {
                .Text = "API Keys configuration will be moved here in a future update." & vbCrLf & vbCrLf &
                        "For now, please use the General tab for all settings.",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Segoe UI", 10, FontStyle.Italic)
            }
            tpApiKeys.Controls.Add(lblApiKeys)

            Dim tpVacation As New TabPage("Vacation Mode") With {
                .Name = "TpOptVacation"
            }
            Dim lblVacation As New Label With {
                .Text = "Vacation Mode settings (coming soon)" & vbCrLf & vbCrLf &
                        "This feature will allow you to configure automatic behaviors when away from home.",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Segoe UI", 10, FontStyle.Italic)
            }
            tpVacation.Controls.Add(lblVacation)

            Dim tpColorScale As New TabPage("Color Scales") With {
                .Name = "TpColorScale"
            }
            Dim lblColorScale As New Label With {
                .Text = "Color Scale configuration (coming soon)" & vbCrLf & vbCrLf &
                        "This feature will allow you to customize color gradients for visualizing weather data.",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Segoe UI", 10, FontStyle.Italic)
            }
            tpColorScale.Controls.Add(lblColorScale)

            Dim tpLogSettings As New TabPage("Logging") With {
                .Name = "TpLogSettings"
            }
            Dim lblLogSettings As New Label With {
                .Text = "Advanced logging settings (coming soon)" & vbCrLf & vbCrLf &
                        "For now, configure log retention days in the General tab.",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Segoe UI", 10, FontStyle.Italic)
            }
            tpLogSettings.Controls.Add(lblLogSettings)

            ' Create the Help tab with HelpViewer control
            _tpSetHelp = New TabPage("Help") With {
                .Name = "TpSetHelp"
            }

            _settingsHelpViewer = New HelpViewer With {
                .Dock = DockStyle.Fill
            }

            _tpSetHelp.Controls.Add(_settingsHelpViewer)

            ' Add all tabs to the TabControl
            _settingsTabControl.TabPages.Add(tpGeneral)
            _settingsTabControl.TabPages.Add(tpApiKeys)
            _settingsTabControl.TabPages.Add(tpVacation)
            _settingsTabControl.TabPages.Add(tpColorScale)
            _settingsTabControl.TabPages.Add(tpLogSettings)
            _settingsTabControl.TabPages.Add(_tpSetHelp)

            ' Add the TabControl to TpSettings
            TpSettings.Controls.Add(_settingsTabControl)

            ' Wire up context help (F1 key support)
            AddHandler _settingsTabControl.SelectedIndexChanged, AddressOf OnSettingsTabChanged
        Catch ex As Exception
            ' Log error but don't crash the application
            Debug.WriteLine($"Error initializing settings help: {ex.Message}")
            MessageBox.Show($"Warning: Could not initialize help system.{vbCrLf}{ex.Message}",
                          "Help System", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Handles tab changes to provide context-sensitive help.
    ''' </summary>
    Private Sub OnSettingsTabChanged(sender As Object, e As EventArgs)
        ' This can be used to automatically show relevant help when switching tabs
        ' For now, just a placeholder for future context-sensitive help
    End Sub

    ''' <summary>
    ''' Shows help for a specific topic by ID.
    ''' </summary>
    Public Sub ShowSettingsHelp(topicId As String)
        Try
            If _settingsHelpViewer IsNot Nothing Then
                ' Switch to help tab
                If _settingsTabControl IsNot Nothing AndAlso _tpSetHelp IsNot Nothing Then
                    _settingsTabControl.SelectedTab = _tpSetHelp
                End If

                ' Navigate to specific topic
                _settingsHelpViewer.NavigateToTopic(topicId)
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error showing settings help: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Shows the help tab.
    ''' </summary>
    Public Sub ShowSettingsHelpTab()
        Try
            If _settingsTabControl IsNot Nothing AndAlso _tpSetHelp IsNot Nothing Then
                ' Switch to main Settings tab first
                If Tc IsNot Nothing AndAlso TpSettings IsNot Nothing Then
                    Tc.SelectedTab = TpSettings
                End If

                ' Then switch to Help sub-tab
                _settingsTabControl.SelectedTab = _tpSetHelp
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error showing settings help tab: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Gets the HelpViewer control for direct access.
    ''' </summary>
    Public ReadOnly Property SettingsHelpViewer As HelpViewer
        Get
            Return _settingsHelpViewer
        End Get
    End Property

    ''' <summary>
    ''' Handles F1 key press for context-sensitive help.
    ''' </summary>
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        ' Check for F1 key
        If keyData = Keys.F1 Then
            ' If we're on the Settings tab, show help
            If Tc IsNot Nothing AndAlso Tc.SelectedTab Is TpSettings Then
                ShowSettingsHelpTab()
                Return True
            End If
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class