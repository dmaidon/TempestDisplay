''' <summary>
''' FrmMain partial class for RtbLogs context menu functionality
''' Handles right-click menu operations for read-only log viewer
''' </summary>
Partial Public Class FrmMain

    Private WithEvents RtbLogsContextMenu As ContextMenuStrip
    Private WithEvents MnuCopy As ToolStripMenuItem
    Private WithEvents MnuSeparator1 As ToolStripSeparator
    Private WithEvents MnuSelectAll As ToolStripMenuItem
    Private WithEvents MnuSeparator2 As ToolStripSeparator
    Private WithEvents MnuClearAll As ToolStripMenuItem

    ''' <summary>
    ''' Initialize the context menu for RtbLogs
    ''' Call this from FrmMain_Load
    ''' </summary>
    Private Sub InitializeRtbLogsContextMenu()
        Try
            ' Create context menu
            RtbLogsContextMenu = New ContextMenuStrip()

            ' Create menu items
            MnuCopy = New ToolStripMenuItem("Copy", Nothing, AddressOf MnuCopy_Click) With {
                .ShortcutKeys = Keys.Control Or Keys.C
            }

            MnuSeparator1 = New ToolStripSeparator()

            MnuSelectAll = New ToolStripMenuItem("Select All", Nothing, AddressOf MnuSelectAll_Click) With {
                .ShortcutKeys = Keys.Control Or Keys.A
            }

            MnuSeparator2 = New ToolStripSeparator()

            MnuClearAll = New ToolStripMenuItem("Clear All", Nothing, AddressOf MnuClearAll_Click)

            ' Add items to context menu
            RtbLogsContextMenu.Items.AddRange(New ToolStripItem() {
                MnuCopy,
                MnuSeparator1,
                MnuSelectAll,
                MnuSeparator2,
                MnuClearAll
            })

            ' Attach context menu to RtbLogs
            RtbLogs.ContextMenuStrip = RtbLogsContextMenu

            ' Handle Opening event to enable/disable menu items
            AddHandler RtbLogsContextMenu.Opening, AddressOf RtbLogsContextMenu_Opening

            Log.Write("[LogContextMenu] Context menu initialized for RtbLogs")
        Catch ex As Exception
            Log.WriteException(ex, "[LogContextMenu] Error initializing context menu")
        End Try
    End Sub

    ''' <summary>
    ''' Update menu item states when context menu opens
    ''' </summary>
    Private Sub RtbLogsContextMenu_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs)
        Try
            Dim hasSelection = RtbLogs.SelectionLength > 0
            Dim hasText = RtbLogs.TextLength > 0

            ' Enable/disable items based on state
            MnuCopy.Enabled = hasSelection
            MnuSelectAll.Enabled = hasText
            MnuClearAll.Enabled = hasText
        Catch ex As Exception
            ' Don't log here to avoid recursion
        End Try
    End Sub

    ''' <summary>
    ''' Copy selected text to clipboard
    ''' </summary>
    Private Sub MnuCopy_Click(sender As Object, e As EventArgs)
        Try
            If RtbLogs.SelectionLength > 0 Then
                Clipboard.SetText(RtbLogs.SelectedText)
                Log.Write("[LogContextMenu] Text copied to clipboard")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[LogContextMenu] Error in Copy operation")
        End Try
    End Sub

    ''' <summary>
    ''' Select all text in RtbLogs
    ''' </summary>
    Private Sub MnuSelectAll_Click(sender As Object, e As EventArgs)
        Try
            If RtbLogs.TextLength > 0 Then
                RtbLogs.SelectAll()
                Log.Write("[LogContextMenu] All text selected")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[LogContextMenu] Error in Select All operation")
        End Try
    End Sub

    ''' <summary>
    ''' Clear all text from RtbLogs
    ''' </summary>
    Private Sub MnuClearAll_Click(sender As Object, e As EventArgs)
        Try
            If RtbLogs.TextLength > 0 Then
                Dim result = MessageBox.Show("Are you sure you want to clear all log text from the viewer?",
                                           "Clear Log Viewer",
                                           MessageBoxButtons.YesNo,
                                           MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    RtbLogs.Clear()
                    Log.Write("[LogContextMenu] Log viewer cleared by user")
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[LogContextMenu] Error in Clear All operation")
        End Try
    End Sub

End Class
