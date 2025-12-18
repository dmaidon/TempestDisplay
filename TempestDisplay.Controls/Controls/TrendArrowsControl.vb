Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class TrendArrowsControl
    Inherits Control

    Private _trends As New Dictionary(Of String, Integer) From {{"Temp", 0}, {"Press", 0}, {"Humid", 0}, {"Wind", 0}}

    Public Sub SetTrend(metric As String, direction As Integer)
        If _trends.ContainsKey(metric) Then
            _trends(metric) = Math.Max(-2, Math.Min(2, direction))
            Invalidate()
        End If
    End Sub

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(150, 100)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        Dim y = 10.0F
        Using font As New Font("Segoe UI", 9, FontStyle.Regular)
            Using arrowFont As New Font("Segoe UI", 14, FontStyle.Bold)
                For Each kvp As KeyValuePair(Of String, Integer) In _trends
                    DrawTrendLine(g, 10, y, kvp.Key, kvp.Value, font, arrowFont)
                    y += 22
                Next
            End Using
        End Using
    End Sub

    Private Shared Sub DrawTrendLine(g As Graphics, x As Single, y As Single, label As String, direction As Integer, font As Font, arrowFont As Font)
        Using labelBrush As New SolidBrush(Color.FromArgb(80, 80, 80))
            g.DrawString(label & ":", font, labelBrush, x, y)
        End Using

        Dim arrow As String = If(direction = 2, "⤴", If(direction = 1, "↗", If(direction = 0, "→", If(direction = -1, "↘", "⤵"))))
        Dim arrowColor = If(direction > 0, Color.FromArgb(100, 180, 100), If(direction < 0, Color.FromArgb(200, 100, 100), Color.Gray))

        Using arrowBrush As New SolidBrush(arrowColor)
            g.DrawString(arrow, arrowFont, arrowBrush, x + 70, y - 5)
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub
End Class
