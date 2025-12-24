Partial Public Class FrmMain

    ' Track current search position
    Private _lastSearchIndex As Integer = 0

    Private _lastSearchText As String = ""
    Private _searchMatches As New List(Of Integer)()

    ''' <summary>
    ''' Find button click - search from beginning
    ''' </summary>
    Private Sub BtnFind_Click(sender As Object, e As EventArgs) Handles BtnFind.Click
        Try
            Dim searchText = TxtLogSearch.Text.Trim()

            If String.IsNullOrEmpty(searchText) Then
                MessageBox.Show("Please enter text to search for.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information)
                TxtLogSearch.Focus()
                Return
            End If

            ' Reset search if text changed
            If searchText <> _lastSearchText Then
                _lastSearchIndex = 0
                _lastSearchText = searchText
                FindAndHighlightAll(searchText)
            End If

            ' Find first occurrence
            If _searchMatches.Count > 0 Then
                _lastSearchIndex = 0
                ScrollToMatch(_lastSearchIndex)
                Log.Write($"[LogSearch] Found {_searchMatches.Count} matches for '{searchText}'")
            Else
                MessageBox.Show($"No matches found for '{searchText}'", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Log.Write($"[LogSearch] No matches found for '{searchText}'")
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[LogSearch] Error in BtnFind_Click")
        End Try
    End Sub

    ''' <summary>
    ''' Find Next button click - search for next occurrence
    ''' </summary>
    Private Sub BtnFindNext_Click(sender As Object, e As EventArgs) Handles BtnFindNext.Click
        Try
            If String.IsNullOrEmpty(_lastSearchText) OrElse _searchMatches.Count = 0 Then
                ' No active search, perform initial search
                BtnFind_Click(sender, e)
                Return
            End If

            ' Move to next match
            _lastSearchIndex += 1
            If _lastSearchIndex >= _searchMatches.Count Then
                _lastSearchIndex = 0 ' Wrap around to beginning
            End If

            ScrollToMatch(_lastSearchIndex)
            Log.Write($"[LogSearch] Moved to match {_lastSearchIndex + 1} of {_searchMatches.Count}")
        Catch ex As Exception
            Log.WriteException(ex, "[LogSearch] Error in BtnFindNext_Click")
        End Try
    End Sub

    ''' <summary>
    ''' Handle Enter key in search textbox
    ''' </summary>
    Private Sub TxtLogSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtLogSearch.KeyDown
        Try
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True ' Prevent beep
                If String.IsNullOrEmpty(_lastSearchText) OrElse TxtLogSearch.Text.Trim() <> _lastSearchText Then
                    BtnFind_Click(sender, EventArgs.Empty)
                Else
                    BtnFindNext_Click(sender, EventArgs.Empty)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "[LogSearch] Error in TxtLogSearch_KeyDown")
        End Try
    End Sub

    ''' <summary>
    ''' Find and highlight all occurrences of search text
    ''' </summary>
    Private Sub FindAndHighlightAll(searchText As String)
        Try
            _searchMatches.Clear()

            If String.IsNullOrEmpty(searchText) Then
                Return
            End If

            ' Clear previous highlighting
            RtbLogs.SelectAll()
            RtbLogs.SelectionBackColor = RtbLogs.BackColor
            RtbLogs.SelectionLength = 0

            ' Find all matches (case-insensitive)
            Dim text = RtbLogs.Text
            Dim index = 0

            While index < text.Length
                index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)
                If index = -1 Then Exit While

                _searchMatches.Add(index)

                ' Highlight this match
                RtbLogs.Select(index, searchText.Length)
                RtbLogs.SelectionBackColor = Color.Yellow

                index += searchText.Length
            End While

            ' Clear selection
            RtbLogs.SelectionLength = 0
        Catch ex As Exception
            Log.WriteException(ex, "[LogSearch] Error in FindAndHighlightAll")
        End Try
    End Sub

    ''' <summary>
    ''' Scroll to and select a specific match
    ''' </summary>
    Private Sub ScrollToMatch(matchIndex As Integer)
        Try
            If matchIndex < 0 OrElse matchIndex >= _searchMatches.Count Then
                Return
            End If

            Dim index = _searchMatches(matchIndex)

            ' Select and scroll to this match
            RtbLogs.Select(index, _lastSearchText.Length)
            RtbLogs.ScrollToCaret()

            ' Update status
            TsslMessages.Text = $"Match {matchIndex + 1} of {_searchMatches.Count}"
        Catch ex As Exception
            Log.WriteException(ex, "[LogSearch] Error in ScrollToMatch")
        End Try
    End Sub

End Class