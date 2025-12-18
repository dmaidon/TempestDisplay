Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Net.Http
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class SunriseSunsetPanel
    Inherits Control

    Private ReadOnly _lblTitle As New Label()
    Private ReadOnly _lblSpacer As New Label()
    Private ReadOnly _lblSunrise As New Label()
    Private ReadOnly _lblSunset As New Label()
    Private ReadOnly _lblDayLength As New Label()
    Private ReadOnly _lblUntilSunrise As New Label()

    ' Gutter to preserve the TableLayoutPanel cell border and visual separation
    Private Const RightBoundaryGutter As Integer = 4

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        'MinimumSize = New Size(200, 120)

        ' Ensure spacing so we don't paint right up to the cell border
        Me.Margin = New Padding(0, 0, RightBoundaryGutter, 0)

        Dim fontTitle As New Font("Segoe UI", 9.5F, FontStyle.Bold)
        Dim fontData As New Font("Segoe UI", 9.0F, FontStyle.Regular)

        _lblTitle.Text = "Sunrise / Sunset"
        _lblTitle.Font = fontTitle
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter
        _lblTitle.Dock = DockStyle.Top
        _lblTitle.Height = 28

        _lblSunrise.Font = fontData
        _lblSunset.Font = fontData
        _lblDayLength.Font = fontData
        _lblUntilSunrise.Font = fontData

        _lblSpacer.Dock = DockStyle.Top
        _lblSunrise.Dock = DockStyle.Top
        _lblSunset.Dock = DockStyle.Top
        _lblDayLength.Dock = DockStyle.Top
        _lblUntilSunrise.Dock = DockStyle.Top

        _lblSpacer.Height = 12
        _lblSunrise.Height = 24
        _lblSunset.Height = 24
        _lblDayLength.Height = 24
        _lblUntilSunrise.Height = 24

        Controls.Add(_lblUntilSunrise)
        Controls.Add(_lblDayLength)
        Controls.Add(_lblSunset)
        Controls.Add(_lblSunrise)
        Controls.Add(_lblSpacer)
        Controls.Add(_lblTitle)
        BackColor = Color.Beige
        Padding = New Padding(8)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        ' Draw a subtle right-side boundary within the control to keep separation visible
        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height
        Dim boundaryX As Integer = Math.Max(0, w - RightBoundaryGutter)
        e.Graphics.SmoothingMode = SmoothingMode.None
        Using boundaryPen As New Pen(Color.FromArgb(170, 170, 170), 1)
            e.Graphics.DrawLine(boundaryPen, boundaryX, 0, boundaryX, h)
        End Using
    End Sub

    Public Async Function FetchAndUpdateAsync(latitude As Double, longitude As Double, Optional dateParam As String = "today") As Task
        Dim url As String = String.Format("https://api.sunrise-sunset.org/json?lat={0}&lng={1}&date={2}&formatted=0", latitude, longitude, dateParam)

        Try
            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromSeconds(10)
                Dim json As String = Await client.GetStringAsync(url)
                Dim sr As String = GetJsonField(json, "sunrise")
                Dim ss As String = GetJsonField(json, "sunset")
                Dim dl As String = GetJsonField(json, "day_length")

                Dim sunriseUtc As DateTime
                Dim sunsetUtc As DateTime

                If DateTime.TryParse(sr, sunriseUtc) Then sunriseUtc = DateTime.SpecifyKind(sunriseUtc, DateTimeKind.Utc)
                If DateTime.TryParse(ss, sunsetUtc) Then sunsetUtc = DateTime.SpecifyKind(sunsetUtc, DateTimeKind.Utc)

                Dim sunriseLocal As DateTime = sunriseUtc.ToLocalTime()
                Dim sunsetLocal As DateTime = sunsetUtc.ToLocalTime()

                _lblSunrise.Text = String.Format("Sunrise: {0}", sunriseLocal.ToString("hh\:mm tt"))
                _lblSunset.Text = String.Format("Sunset: {0}", sunsetLocal.ToString("hh\:mm tt"))
                _lblDayLength.Text = String.Format("Day Length: {0}", FormatDayLength(dl))
                _lblUntilSunrise.Text = String.Format("Until Sunrise: {0}", FormatTimeRemaining(sunriseLocal))
            End Using
        Catch ex As Exception
            _lblTitle.Text = "Sunrise / Sunset (Error)"
            _lblUntilSunrise.Text = ex.Message
        End Try
    End Function

    Private Shared Function GetJsonField(json As String, field As String) As String
        ' Very simple JSON extraction without Regex/JSON libs
        ' Looks for: "field": "value"  OR  "field": number
        Dim key As String = """"c & field & """"c
        Dim idx As Integer = json.IndexOf(key, StringComparison.OrdinalIgnoreCase)
        If idx < 0 Then Return String.Empty
        idx = json.IndexOf(":"c, idx)
        If idx < 0 Then Return String.Empty
        idx += 1
        ' Skip whitespace
        While idx < json.Length AndAlso Char.IsWhiteSpace(json(idx))
            idx += 1
        End While
        If idx >= json.Length Then Return String.Empty
        If json(idx) = """"c Then
            ' Quoted string
            idx += 1
            Dim qEnd As Integer = json.IndexOf(""""c, idx)
            If qEnd > idx Then
                Return json.Substring(idx, qEnd - idx)
            End If
        Else
            ' Numeric value until comma/close brace
            Dim nEnd As Integer = idx
            While nEnd < json.Length AndAlso Char.IsDigit(json(nEnd))
                nEnd += 1
            End While
            If nEnd > idx Then
                Return json.Substring(idx, nEnd - idx)
            End If
        End If
        Return String.Empty
    End Function

    Private Shared Function FormatDayLength(dayLengthRaw As String) As String
        If String.IsNullOrWhiteSpace(dayLengthRaw) Then Return "-"
        Dim seconds As Integer
        If Integer.TryParse(dayLengthRaw, seconds) Then
            Dim ts As TimeSpan = TimeSpan.FromSeconds(seconds)
            Return String.Format("{0}h {1}m", CInt(Math.Floor(ts.TotalHours)), ts.Minutes)
        End If
        ' Fallback if API returns already formatted string
        Return dayLengthRaw
    End Function

    Private Shared Function FormatTimeRemaining(targetLocal As DateTime) As String
        Dim nowLocal As DateTime = DateTime.Now
        If targetLocal <= nowLocal Then
            Return "0m"
        End If
        Dim ts As TimeSpan = targetLocal - nowLocal
        Return String.Format("{0}h {1}m", CInt(Math.Floor(ts.TotalHours)), ts.Minutes)
    End Function

End Class