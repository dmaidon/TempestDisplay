Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Windows.Forms

''' <summary>
''' User control that provides a complete help viewing interface with TreeView navigation,
''' RichTextBox display, search, print, and export capabilities.
''' </summary>
<DefaultEvent("TopicSelected")>
Public Class HelpViewer
        Inherits UserControl

        Private _helpProvider As SettingsHelpProvider
        Private _currentTopic As HelpTopic
        Private WithEvents _treeView As TreeView
        Private WithEvents _richTextBox As RichTextBox
        Private WithEvents _searchTextBox As TextBox
        Private WithEvents _printButton As Button
        Private WithEvents _exportButton As Button
        Private WithEvents _printDocument As PrintDocument
        Private _printContent As String

        Public Event TopicSelected As EventHandler(Of HelpTopicEventArgs)

        Public Sub New()
            InitializeComponent()
            _helpProvider = New SettingsHelpProvider()
            LoadHelpTopics()
        End Sub

        Private Sub InitializeComponent()
            ' Create the control layout
            Me.SuspendLayout()

            ' Main split container
            Dim mainSplit As New SplitContainer()
            mainSplit.Dock = DockStyle.Fill
            mainSplit.SplitterDistance = 250
            mainSplit.FixedPanel = FixedPanel.Panel1
            mainSplit.BorderStyle = BorderStyle.Fixed3D

            ' Left panel - TreeView with search
            Dim leftPanel As New Panel()
            leftPanel.Dock = DockStyle.Fill

            ' Search panel at top
            Dim searchPanel As New Panel()
            searchPanel.Dock = DockStyle.Top
            searchPanel.Height = 60
            searchPanel.Padding = New Padding(5)

            Dim searchLabel As New Label()
            searchLabel.Text = "Search:"
            searchLabel.Dock = DockStyle.Top
            searchLabel.Height = 20

            _searchTextBox = New TextBox()
            _searchTextBox.Dock = DockStyle.Top
            _searchTextBox.PlaceholderText = "Type to search help topics..."

            searchPanel.Controls.Add(_searchTextBox)
            searchPanel.Controls.Add(searchLabel)

            ' TreeView
            _treeView = New TreeView()
            _treeView.Dock = DockStyle.Fill
            _treeView.HideSelection = False
            _treeView.ShowLines = True
            _treeView.ShowPlusMinus = True
            _treeView.Font = New Font("Segoe UI", 9.0F)

            leftPanel.Controls.Add(_treeView)
            leftPanel.Controls.Add(searchPanel)

            mainSplit.Panel1.Controls.Add(leftPanel)

            ' Right panel - RichTextBox with toolbar
            Dim rightPanel As New Panel()
            rightPanel.Dock = DockStyle.Fill

            ' Toolbar at top
            Dim toolbarPanel As New Panel()
            toolbarPanel.Dock = DockStyle.Top
            toolbarPanel.Height = 40
            toolbarPanel.Padding = New Padding(5)

            _printButton = New Button()
            _printButton.Text = "Print"
            _printButton.Width = 80
            _printButton.Height = 30
            _printButton.Location = New Point(5, 5)

            _exportButton = New Button()
            _exportButton.Text = "Export"
            _exportButton.Width = 80
            _exportButton.Height = 30
            _exportButton.Location = New Point(90, 5)

            toolbarPanel.Controls.Add(_exportButton)
            toolbarPanel.Controls.Add(_printButton)

            ' RichTextBox
            _richTextBox = New RichTextBox()
            _richTextBox.Dock = DockStyle.Fill
            _richTextBox.ReadOnly = True
            _richTextBox.BorderStyle = BorderStyle.None
            _richTextBox.Font = New Font("Segoe UI", 10.0F)
            _richTextBox.BackColor = SystemColors.Window
            _richTextBox.Padding = New Padding(10)

            rightPanel.Controls.Add(_richTextBox)
            rightPanel.Controls.Add(toolbarPanel)

            mainSplit.Panel2.Controls.Add(rightPanel)

            ' Add main split to control
            Me.Controls.Add(mainSplit)

            ' Set control properties
            Me.Size = New Size(800, 600)
            Me.BackColor = SystemColors.Control

            Me.ResumeLayout(False)

            ' Initialize print document
            _printDocument = New PrintDocument()
        End Sub

        ''' <summary>
        ''' Gets or sets the current help topic being displayed.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property CurrentTopic As HelpTopic
            Get
                Return _currentTopic
            End Get
            Set(value As HelpTopic)
                _currentTopic = value
                DisplayTopic(value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the TreeView control for external customization.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property TreeViewControl As TreeView
            Get
                Return _treeView
            End Get
        End Property

        ''' <summary>
        ''' Gets the RichTextBox control for external customization.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property RichTextBoxControl As RichTextBox
            Get
                Return _richTextBox
            End Get
        End Property

        ''' <summary>
        ''' Gets the search TextBox control for external customization.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property SearchTextBoxControl As TextBox
            Get
                Return _searchTextBox
            End Get
        End Property

        ''' <summary>
        ''' Gets the Print button for external customization.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property PrintButtonControl As Button
            Get
                Return _printButton
            End Get
        End Property

        ''' <summary>
        ''' Gets the Export button for external customization.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property ExportButtonControl As Button
            Get
                Return _exportButton
            End Get
        End Property

        Private Sub LoadHelpTopics()
            _treeView.Nodes.Clear()

            ' Group by Tab Page, then by Category
            Dim tabPages = _helpProvider.TabPages

            For Each tabPage In tabPages
                Dim tabNode As New TreeNode(FormatTabPageName(tabPage))
                tabNode.Tag = tabPage

                Dim topicsForTab = _helpProvider.GetTopicsByTabPage(tabPage)
                Dim categories = topicsForTab.Select(Function(t) t.Category).Distinct()

                For Each category In categories
                    Dim categoryNode As New TreeNode(category)
                    categoryNode.Tag = category

                    Dim topicsForCategory = topicsForTab.Where(Function(t) t.Category = category).OrderBy(Function(t) t.SortOrder)

                    For Each topic In topicsForCategory
                        Dim topicNode As New TreeNode(topic.Title)
                        topicNode.Tag = topic
                        categoryNode.Nodes.Add(topicNode)
                    Next

                    tabNode.Nodes.Add(categoryNode)
                Next

                _treeView.Nodes.Add(tabNode)
            Next

            ' Expand first level
            For Each node As TreeNode In _treeView.Nodes
                node.Expand()
            Next
        End Sub

        Private Function FormatTabPageName(tabPage As String) As String
            ' Convert "TpSetOptions" to "General Options"
            Select Case tabPage
                Case "TpSetOptions"
                    Return "General Options"
                Case "TpApiKeys"
                    Return "API Keys"
                Case "TpOptVacation"
                    Return "Vacation Mode"
                Case "TpColorScale"
                    Return "Color Scales"
                Case "TpLogSettings"
                    Return "Logging Settings"
                Case Else
                    Return tabPage
            End Select
        End Function

        Private Sub DisplayTopic(topic As HelpTopic)
            If topic Is Nothing Then
                _richTextBox.Clear()
                Return
            End If

            _richTextBox.Rtf = topic.Content
        End Sub

        Private Sub TreeView_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles _treeView.AfterSelect
            If e.Node IsNot Nothing AndAlso TypeOf e.Node.Tag Is HelpTopic Then
                Dim topic = DirectCast(e.Node.Tag, HelpTopic)
                CurrentTopic = topic
                RaiseEvent TopicSelected(Me, New HelpTopicEventArgs(topic))
            End If
        End Sub

        Private Sub SearchTextBox_TextChanged(sender As Object, e As EventArgs) Handles _searchTextBox.TextChanged
            FilterTopics(_searchTextBox.Text)
        End Sub

        Private Sub FilterTopics(searchText As String)
            _treeView.Nodes.Clear()

            Dim filteredTopics = _helpProvider.Search(searchText)

            If String.IsNullOrWhiteSpace(searchText) Then
                ' Show all organized by tab page
                LoadHelpTopics()
                Return
            End If

            ' Show search results
            Dim searchNode As New TreeNode($"Search Results ({filteredTopics.Count})")
            searchNode.Tag = "SearchResults"

            For Each topic In filteredTopics
                Dim topicNode As New TreeNode($"{topic.Title} ({topic.Category})")
                topicNode.Tag = topic
                searchNode.Nodes.Add(topicNode)
            Next

            _treeView.Nodes.Add(searchNode)
            searchNode.Expand()
        End Sub

        Private Sub PrintButton_Click(sender As Object, e As EventArgs) Handles _printButton.Click
            If _currentTopic Is Nothing Then
                MessageBox.Show("No topic selected to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Try
                ' Prepare content for printing (strip RTF)
                _printContent = _richTextBox.Text

                Dim printDialog As New PrintDialog()
                printDialog.Document = _printDocument

                If printDialog.ShowDialog() = DialogResult.OK Then
                    _printDocument.Print()
                End If
            Catch ex As Exception
                MessageBox.Show($"Error printing: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub PrintDocument_PrintPage(sender As Object, e As PrintPageEventArgs) Handles _printDocument.PrintPage
            ' Simple text printing
            Dim font As New Font("Segoe UI", 10)
            Dim brush As New SolidBrush(Color.Black)
            Dim leftMargin As Single = e.MarginBounds.Left
            Dim topMargin As Single = e.MarginBounds.Top
            Dim linesPerPage As Integer = CInt(e.MarginBounds.Height / font.GetHeight(e.Graphics))

            Dim lines = _printContent.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            Dim yPosition As Single = topMargin
            Dim count As Integer = 0

            For Each line In lines
                If count >= linesPerPage Then
                    e.HasMorePages = True
                    Exit For
                End If

                e.Graphics.DrawString(line, font, brush, leftMargin, yPosition)
                yPosition += font.GetHeight(e.Graphics)
                count += 1
            Next

            brush.Dispose()
            font.Dispose()
        End Sub

        Private Sub ExportButton_Click(sender As Object, e As EventArgs) Handles _exportButton.Click
            If _currentTopic Is Nothing Then
                MessageBox.Show("No topic selected to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Try
                Dim saveDialog As New SaveFileDialog()
                saveDialog.FileName = $"{_currentTopic.Id}.txt"
                saveDialog.Filter = "Text Files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All Files (*.*)|*.*"
                saveDialog.DefaultExt = "txt"

                If saveDialog.ShowDialog() = DialogResult.OK Then
                    If saveDialog.FileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) Then
                        System.IO.File.WriteAllText(saveDialog.FileName, _richTextBox.Rtf)
                    Else
                        System.IO.File.WriteAllText(saveDialog.FileName, _richTextBox.Text)
                    End If

                    MessageBox.Show("Help topic exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        ''' <summary>
        ''' Navigates to a specific help topic by ID.
        ''' </summary>
        Public Sub NavigateToTopic(topicId As String)
            Dim topic = _helpProvider.GetTopicById(topicId)
            If topic IsNot Nothing Then
                CurrentTopic = topic

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
            End If
        End Sub

        ''' <summary>
        ''' Exports all help topics to a single file.
        ''' </summary>
        Public Sub ExportAllTopics()
            Try
                Dim saveDialog As New SaveFileDialog()
                saveDialog.FileName = "TempestDisplay_Help.txt"
                saveDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
                saveDialog.DefaultExt = "txt"

                If saveDialog.ShowDialog() = DialogResult.OK Then
                    Dim content As New System.Text.StringBuilder()

                    content.AppendLine("TempestDisplay Settings Help")
                    content.AppendLine("Generated: " & DateTime.Now.ToString())
                    content.AppendLine("=" & New String("="c, 80))
                    content.AppendLine()

                    For Each tabPage In _helpProvider.TabPages
                        content.AppendLine()
                        content.AppendLine(FormatTabPageName(tabPage))
                        content.AppendLine(New String("="c, FormatTabPageName(tabPage).Length))
                        content.AppendLine()

                        Dim topics = _helpProvider.GetTopicsByTabPage(tabPage)
                        For Each topic In topics
                            content.AppendLine()
                            content.AppendLine(topic.Title)
                            content.AppendLine(New String("-"c, topic.Title.Length))
                            content.AppendLine()

                            ' Strip RTF and write content
                            Dim tempRtb As New RichTextBox()
                            tempRtb.Rtf = topic.Content
                            content.AppendLine(tempRtb.Text)
                            tempRtb.Dispose()
                        Next
                    Next

                    System.IO.File.WriteAllText(saveDialog.FileName, content.ToString())
                    MessageBox.Show("All help topics exported successfully.", "Export All", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error exporting all topics: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

    End Class

''' <summary>
''' Event args for topic selection events.
''' </summary>
Public Class HelpTopicEventArgs
    Inherits EventArgs

    Public Property Topic As HelpTopic

    Public Sub New(topic As HelpTopic)
        Me.Topic = topic
    End Sub
End Class
