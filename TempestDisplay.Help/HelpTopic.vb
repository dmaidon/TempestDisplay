''' <summary>
''' Represents a single help topic with title, content, and categorization.
''' </summary>
Public Class HelpTopic

    ''' <summary>
    ''' Unique identifier for the topic.
    ''' </summary>
    Public Property Id As String

    ''' <summary>
    ''' Display title of the help topic.
    ''' </summary>
    Public Property Title As String

    ''' <summary>
    ''' Category/section this topic belongs to (e.g., "General Options", "API Keys").
    ''' </summary>
    Public Property Category As String

    ''' <summary>
    ''' Tab page this topic is associated with.
    ''' </summary>
    Public Property TabPage As String

    ''' <summary>
    ''' Rich text content (supports RTF formatting).
    ''' </summary>
    Public Property Content As String

    ''' <summary>
    ''' Keywords for search functionality.
    ''' </summary>
    Public Property Keywords As List(Of String)

    ''' <summary>
    ''' Sort order within the category.
    ''' </summary>
    Public Property SortOrder As Integer

    Public Sub New()
        Keywords = New List(Of String)()
    End Sub

    Public Sub New(id As String, title As String, category As String, tabPage As String, content As String, ParamArray keywords As String())
        Me.Id = id
        Me.Title = title
        Me.Category = category
        Me.TabPage = tabPage
        Me.Content = content
        Me.Keywords = If(keywords IsNot Nothing, New List(Of String)(keywords), New List(Of String)())
    End Sub

    ''' <summary>
    ''' Determines if this topic matches the search text.
    ''' </summary>
    Public Function MatchesSearch(searchText As String) As Boolean
        If String.IsNullOrWhiteSpace(searchText) Then
            Return True
        End If

        Dim search = searchText.ToLower()

        ' Search in title
        If Title IsNot Nothing AndAlso Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) Then
            Return True
        End If

        ' Search in keywords
        If Keywords IsNot Nothing AndAlso Keywords.Any(Function(k) k.Contains(search, StringComparison.CurrentCultureIgnoreCase)) Then
            Return True
        End If

        ' Search in category
        If Category IsNot Nothing AndAlso Category.Contains(search, StringComparison.CurrentCultureIgnoreCase) Then
            Return True
        End If

        ' Search in content (strip RTF tags for better matching)
        If Content IsNot Nothing AndAlso StripRtfTags(Content).Contains(search, StringComparison.CurrentCultureIgnoreCase) Then
            Return True
        End If

        Return False
    End Function

    Private Shared Function StripRtfTags(rtfText As String) As String
        If String.IsNullOrEmpty(rtfText) Then
            Return String.Empty
        End If

        ' Simple RTF tag removal for search purposes
        Dim text = rtfText
        text = System.Text.RegularExpressions.Regex.Replace(text, "\\[a-z]+\d*\s*", " ")
        text = System.Text.RegularExpressions.Regex.Replace(text, "[{}]", "")
        Return text
    End Function

End Class