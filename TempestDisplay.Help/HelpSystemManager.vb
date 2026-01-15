Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Windows.Forms

''' <summary>
''' Manages the integration of help system with Windows Forms controls.
''' Provides a form-agnostic API for initializing and controlling the help system.
''' </summary>
Public Class HelpSystemManager

    Private ReadOnly _helpProvider As SettingsHelpProvider
    Private _currentTopic As HelpTopic
    Private _treeView As TreeView
    Private _richTextBox As RichTextBox
    Private _searchTextBox As TextBox
    Private _printButton As Button
    Private _exportButton As Button
    Private ReadOnly _logAction As Action(Of String)

    ''' <summary>
    ''' Creates a new HelpSystemManager instance
    ''' </summary>
    ''' <param name="logAction">Optional logging callback</param>
    Public Sub New(Optional logAction As Action(Of String) = Nothing)
        _helpProvider = New SettingsHelpProvider()
        _logAction = logAction
    End Sub

    ''' <summary>
    ''' Initializes the help system with the provided controls
    ''' </summary>
    Public Sub Initialize(treeView As TreeView,
                        richTextBox As RichTextBox,
                        searchTextBox As TextBox,
                        printButton As Button,
                        exportButton As Button)

        ArgumentNullException.ThrowIfNull(treeView)
        ArgumentNullException.ThrowIfNull(richTextBox)
        ArgumentNullException.ThrowIfNull(searchTextBox)
        ArgumentNullException.ThrowIfNull(printButton)
        ArgumentNullException.ThrowIfNull(exportButton)

        _treeView = treeView
        _richTextBox = richTextBox
        _searchTextBox = searchTextBox
        _printButton = printButton
        _exportButton = exportButton

        ' Wire up event handlers
        AddHandler _treeView.AfterSelect, AddressOf TreeView_AfterSelect
        AddHandler _searchTextBox.TextChanged, AddressOf SearchTextBox_TextChanged
        AddHandler _searchTextBox.KeyDown, AddressOf SearchTextBox_KeyDown
        AddHandler _printButton.Click, AddressOf PrintButton_Click
        AddHandler _exportButton.Click, AddressOf ExportButton_Click

        ' Load content
        LoadHelpTopicsIntoTreeView()
        ShowWelcomeTopic()

        WriteLog("[Help] Help system initialized successfully")
    End Sub

    ''' <summary>
    ''' Navigates to a specific help topic by ID
    ''' </summary>
    Public Sub NavigateToTopic(topicId As String)
        Try
            Dim topic = _helpProvider.GetTopicById(topicId)
            If topic Is Nothing Then
                WriteLog($"[Help] Topic not found: {topicId}")
                Return
            End If

            DisplayTopic(topic)

            ' Find and select the node in the tree
            For Each tabNode As TreeNode In _treeView.Nodes
                For Each categoryNode As TreeNode In tabNode.Nodes
                    For Each topicNode As TreeNode In categoryNode.Nodes
                        If TypeOf topicNode.Tag Is HelpTopic Then
                            Dim nodeTopic = DirectCast(topicNode.Tag, HelpTopic)
                            If nodeTopic.Id = topicId Then
                                _treeView.SelectedNode = topicNode
                                topicNode.EnsureVisible()
                                Return
                            End If
                        End If
                    Next
                Next
            Next

            WriteLog($"[Help] Topic found but not in tree: {topicId}")
        Catch ex As Exception
            WriteLog($"[Help] Error navigating to topic: {topicId} - {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Gets the topic ID for a given control name
    ''' </summary>
    Public Shared Function GetTopicIdForControl(controlName As String) As String
        Select Case controlName
            Case "TxtStationName"
                Return "setoptions_station_name"
            Case "TxtStationID"
                Return "setoptions_station_id"
            Case "TxtStationElevation"
                Return "setoptions_elevation"
            Case "NudTempestInterval"
                Return "setoptions_update_interval"
            Case "TxtApiToken"
                Return "apikeys_tempest_token"
            Case "TxtDeviceID"
                Return "apikeys_device_id"
            Case "TxtLogin", "TxtPassword", "TxtIp"
                Return "apikeys_meteobridge"
            Case "TxtLogDays"
                Return "logsettings_retention"
            Case Else
                Return Nothing
        End Select
    End Function

    Private Sub LoadHelpTopicsIntoTreeView()
        _treeView.Nodes.Clear()
        _treeView.BeginUpdate()

        Try
            ' Welcome node
            Dim welcomeNode As New TreeNode("Welcome to TempestDisplay Help") With {
                .Tag = "welcome",
                .NodeFont = New Font(_treeView.Font, FontStyle.Bold)
            }
            _treeView.Nodes.Add(welcomeNode)

            ' Group by Tab Page, then by Category
            Dim tabPages = _helpProvider.TabPages

            For Each tabPage In tabPages
                Dim tabNode As New TreeNode(FormatTabPageName(tabPage)) With {
                    .Tag = New With {.Type = "TabPage", .Name = tabPage},
                    .NodeFont = New Font(_treeView.Font, FontStyle.Bold)
                }

                Dim topicsForTab = _helpProvider.GetTopicsByTabPage(tabPage)
                Dim categories = topicsForTab.Select(Function(t) t.Category).Distinct()

                For Each category In categories
                    Dim categoryNode As New TreeNode(category) With {
                        .Tag = New With {.Type = "Category", .Name = category}
                    }

                    Dim topicsForCategory = topicsForTab.Where(Function(t) t.Category = category).OrderBy(Function(t) t.SortOrder)

                    For Each topic In topicsForCategory
                        Dim topicNode As New TreeNode(topic.Title) With {
                            .Tag = topic
                        }
                        categoryNode.Nodes.Add(topicNode)
                    Next

                    tabNode.Nodes.Add(categoryNode)
                Next

                _treeView.Nodes.Add(tabNode)
            Next

            ' About node
            Dim aboutNode As New TreeNode("About TempestDisplay") With {
                .Tag = "about",
                .NodeFont = New Font(_treeView.Font, FontStyle.Bold)
            }
            _treeView.Nodes.Add(aboutNode)

            ' Expand first node
            If _treeView.Nodes.Count > 0 Then
                _treeView.Nodes(0).Expand()
            End If
        Finally
            _treeView.EndUpdate()
        End Try
    End Sub

    Private Shared Function FormatTabPageName(tabPage As String) As String
        Select Case tabPage
            Case "TpSetOptions"
                Return "? General Options"
            Case "TpApiKeys"
                Return "?? API Keys & Authentication"
            Case "TpOptVacation"
                Return "? Vacation Mode"
            Case "TpColorScale"
                Return "?? Color Scales"
            Case "TpLogSettings"
                Return "?? Logging Settings"
            Case Else
                Return tabPage
        End Select
    End Function

    Private Sub TreeView_AfterSelect(sender As Object, e As TreeViewEventArgs)
        If e.Node Is Nothing Then Return

        Try
            If TypeOf e.Node.Tag Is HelpTopic Then
                Dim topic = DirectCast(e.Node.Tag, HelpTopic)
                DisplayTopic(topic)
            ElseIf TypeOf e.Node.Tag Is String Then
                Dim tag = CStr(e.Node.Tag)
                If tag = "welcome" Then
                    ShowWelcomeTopic()
                ElseIf tag = "about" Then
                    ShowAboutTopic()
                End If
            End If
        Catch ex As Exception
            WriteLog($"[Help] Error displaying topic: {ex.Message}")
        End Try
    End Sub

    Private Sub DisplayTopic(topic As HelpTopic)
        If topic Is Nothing Then
            _richTextBox.Clear()
            Return
        End If

        Try
            _richTextBox.Rtf = topic.Content
            _currentTopic = topic
            WriteLog($"[Help] Displayed topic: {topic.Id}")
        Catch ex As Exception
            _richTextBox.Clear()
            _richTextBox.AppendText(topic.Title & vbCrLf & vbCrLf)
            _richTextBox.AppendText(StripRtf(topic.Content))
            WriteLog($"[Help] Error displaying RTF, showing plain text: {ex.Message}")
        End Try
    End Sub

    Private Sub ShowWelcomeTopic()
        Dim welcome As String = CreateRtfDocument(
            "Welcome to TempestDisplay Help",
            "Thank you for using TempestDisplay!" & vbCrLf & vbCrLf &
            "This help system provides comprehensive documentation for all settings and features." & vbCrLf & vbCrLf &
            "Getting Started:" & vbCrLf &
            "• Use the tree on the left to browse help topics" & vbCrLf &
            "• Type keywords in the search box to find specific information" & vbCrLf &
            "• Click Print to print the current help topic" & vbCrLf &
            "• Click Export to save help topics to a file" & vbCrLf & vbCrLf &
            "Quick Access:" & vbCrLf &
            "• General Options - Configure your station and basic settings" & vbCrLf &
            "• API Keys - Set up WeatherFlow Tempest and MeteoBridge connections" & vbCrLf &
            "• Logging Settings - Control diagnostic logging" & vbCrLf & vbCrLf &
            "Need more help?" & vbCrLf &
            "Visit: https://github.com/dmaidon/TempestDisplay/wiki" & vbCrLf &
            "Or check the About section for version and support information."
        )

        _richTextBox.Rtf = welcome
        _currentTopic = Nothing
    End Sub

    Private Sub ShowAboutTopic()
        Dim version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        Dim about As String = CreateRtfDocument(
            "About TempestDisplay",
            $"TempestDisplay Version {version}" & vbCrLf & vbCrLf &
            "A comprehensive weather station display application for WeatherFlow Tempest." & vbCrLf & vbCrLf &
            "Features:" & vbCrLf &
            "• Real-time weather data from Tempest station" & vbCrLf &
            "• UDP broadcast listener for instant updates" & vbCrLf &
            "• WeatherFlow REST API integration" & vbCrLf &
            "• MeteoBridge support" & vbCrLf &
            "• Customizable display and alerts" & vbCrLf &
            "• Historical data charts and records" & vbCrLf &
            "• Comprehensive logging for troubleshooting" & vbCrLf & vbCrLf &
            "Developed by:" & vbCrLf &
            "GitHub: https://github.com/dmaidon/TempestDisplay" & vbCrLf & vbCrLf &
            "Support:" & vbCrLf &
            "• GitHub Issues: https://github.com/dmaidon/TempestDisplay/issues" & vbCrLf &
            "• Wiki: https://github.com/dmaidon/TempestDisplay/wiki" & vbCrLf & vbCrLf &
            "License:" & vbCrLf &
            "This software is open source." & vbCrLf & vbCrLf &
            "Third-Party Libraries:" & vbCrLf &
            "• WeatherFlow Tempest API" & vbCrLf &
            "• .NET Windows Forms" & vbCrLf &
            "• System.Text.Json" & vbCrLf & vbCrLf &
            "Special Thanks:" & vbCrLf &
            "• WeatherFlow for the excellent Tempest weather station" & vbCrLf &
            "• The open-source community" & vbCrLf &
            "• All contributors and users"
        )

        _richTextBox.Rtf = about
        _currentTopic = Nothing
    End Sub

    Private Sub SearchTextBox_TextChanged(sender As Object, e As EventArgs)
        Try
            Dim searchText = _searchTextBox.Text.Trim()

            If String.IsNullOrWhiteSpace(searchText) Then
                LoadHelpTopicsIntoTreeView()
                Return
            End If

            Dim results = _helpProvider.Search(searchText)
            DisplaySearchResults(results, searchText)
        Catch ex As Exception
            WriteLog($"[Help] Error performing search: {ex.Message}")
        End Try
    End Sub

    Private Sub SearchTextBox_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Escape Then
            _searchTextBox.Clear()
        End If
    End Sub

    Private Sub DisplaySearchResults(results As List(Of HelpTopic), searchText As String)
        _treeView.Nodes.Clear()
        _treeView.BeginUpdate()

        Try
            Dim searchNode As New TreeNode($"?? Search Results for ""{searchText}"" ({results.Count} found)") With {
                .Tag = "search",
                .NodeFont = New Font(_treeView.Font, FontStyle.Bold)
            }

            If results.Count = 0 Then
                Dim noResultsNode As New TreeNode("No results found") With {
                    .ForeColor = Color.Gray
                }
                searchNode.Nodes.Add(noResultsNode)
            Else
                For Each topic In results
                    Dim resultNode As New TreeNode($"{topic.Title} ({topic.Category})") With {
                        .Tag = topic,
                        .ToolTipText = $"Tab: {FormatTabPageName(topic.TabPage)}" & vbCrLf & $"Category: {topic.Category}"
                    }
                    searchNode.Nodes.Add(resultNode)
                Next
            End If

            _treeView.Nodes.Add(searchNode)
            searchNode.Expand()
        Finally
            _treeView.EndUpdate()
        End Try
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As EventArgs)
        If _currentTopic Is Nothing Then
            MessageBox.Show("Please select a help topic first.", "Print Help", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Dim printDialog As New PrintDialog()
            Dim printDocument As New PrintDocument()

            AddHandler printDocument.PrintPage, AddressOf PrintDocument_PrintPage

            printDialog.Document = printDocument

            If printDialog.ShowDialog() = DialogResult.OK Then
                printDocument.Print()
            End If
        Catch ex As Exception
            MessageBox.Show($"Error printing: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            WriteLog($"[Help] Error printing topic: {ex.Message}")
        End Try
    End Sub

    Private Sub PrintDocument_PrintPage(sender As Object, e As PrintPageEventArgs)
        If _currentTopic Is Nothing Then Return

        Dim font As New Font("Segoe UI", 10)
        Dim titleFont As New Font("Segoe UI", 14, FontStyle.Bold)
        Dim brush As New SolidBrush(Color.Black)
        Dim leftMargin As Single = e.MarginBounds.Left
        Dim topMargin As Single = e.MarginBounds.Top
        Dim yPosition As Single = topMargin

        e.Graphics.DrawString(_currentTopic.Title, titleFont, brush, leftMargin, yPosition)
        yPosition += titleFont.GetHeight(e.Graphics) * 2

        Dim content = StripRtf(_currentTopic.Content)
        Dim lines = content.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)

        For Each line In lines
            If yPosition + font.GetHeight(e.Graphics) > e.MarginBounds.Bottom Then
                e.HasMorePages = True
                Exit For
            End If

            e.Graphics.DrawString(line, font, brush, leftMargin, yPosition)
            yPosition += font.GetHeight(e.Graphics)
        Next

        brush.Dispose()
        font.Dispose()
        titleFont.Dispose()
    End Sub

    Private Sub ExportButton_Click(sender As Object, e As EventArgs)
        If _currentTopic Is Nothing Then
            MessageBox.Show("Please select a help topic first.", "Export Help", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Using saveDialog As New SaveFileDialog()
                saveDialog.FileName = $"{_currentTopic.Id}.txt"
                saveDialog.Filter = "Text Files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All Files (*.*)|*.*"
                saveDialog.DefaultExt = "txt"
                saveDialog.Title = "Export Help Topic"

                If saveDialog.ShowDialog() = DialogResult.OK Then
                    If saveDialog.FileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) Then
                        IO.File.WriteAllText(saveDialog.FileName, _currentTopic.Content)
                    Else
                        Dim content = _currentTopic.Title & vbCrLf &
                                    New String("="c, _currentTopic.Title.Length) & vbCrLf & vbCrLf &
                                    StripRtf(_currentTopic.Content)
                        IO.File.WriteAllText(saveDialog.FileName, content)
                    End If

                    MessageBox.Show("Help topic exported successfully.", "Export Complete",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information)
                    WriteLog($"[Help] Exported topic: {_currentTopic.Id} to {saveDialog.FileName}")
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error exporting: {ex.Message}", "Export Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
            WriteLog($"[Help] Error exporting topic: {ex.Message}")
        End Try
    End Sub

    Private Shared Function StripRtf(rtfText As String) As String
        If String.IsNullOrEmpty(rtfText) Then Return String.Empty

        Try
            Using tempRtb As New RichTextBox()
                tempRtb.Rtf = rtfText
                Return tempRtb.Text
            End Using
        Catch
            Dim text = rtfText
            text = System.Text.RegularExpressions.Regex.Replace(text, "\\[a-z]+\d*\s*", " ")
            text = System.Text.RegularExpressions.Regex.Replace(text, "[{}]", "")
            Return text
        End Try
    End Function

    Private Shared Function CreateRtfDocument(title As String, content As String) As String
        Dim rtf As New System.Text.StringBuilder()

        rtf.AppendLine("{\rtf1\ansi\deff0 {\fonttbl {\f0 Segoe UI;}}")
        rtf.AppendLine("{\colortbl;\red0\green0\blue0;\red0\green102\blue204;}")
        rtf.AppendLine("\f0\fs32\b\cf2 " & EscapeRtf(title) & "\b0\cf0\fs20")
        rtf.AppendLine("\par\par")

        Dim lines = content.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
        For Each line In lines
            If String.IsNullOrWhiteSpace(line) Then
                rtf.AppendLine("\par")
            ElseIf line.StartsWith("•"c) Then
                rtf.AppendLine("\bullet " & EscapeRtf(line.Substring(1).Trim()))
                rtf.AppendLine("\par")
            ElseIf line.EndsWith(":"c) Then
                rtf.AppendLine("\b " & EscapeRtf(line) & "\b0")
                rtf.AppendLine("\par")
            Else
                rtf.AppendLine(EscapeRtf(line))
                rtf.AppendLine("\par")
            End If
        Next

        rtf.AppendLine("}")
        Return rtf.ToString()
    End Function

    Private Shared Function EscapeRtf(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Return text.Replace("\", "\\").Replace("{", "\{").Replace("}", "\}")
    End Function

    Private Sub WriteLog(message As String)
        _logAction?.Invoke(message)
    End Sub

End Class