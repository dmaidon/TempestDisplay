Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class StatusLEDPanel
    Inherits Control

    Private ReadOnly _statuses As New Dictionary(Of String, LEDStatus) From {
        {"UDP Active", LEDStatus.Green},
        {"Battery Good", LEDStatus.Green},
        {"API Connected", LEDStatus.Gray},
        {"Data Fresh", LEDStatus.Green}
    }

    Private ReadOnly _animationTimer As Timer
    Private _pulsePhase As Single = 0

    Public Enum LEDStatus
        Green = 0
        Yellow = 1
        Red = 2
        Gray = 3
    End Enum

    Public Sub SetStatus(label As String, status As LEDStatus)
        ' Prefer TryAdd over ContainsKey+Add
        If Not _statuses.TryAdd(label, status) Then
            _statuses(label) = status
        End If
        Invalidate()
    End Sub

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(150, 100)

        _animationTimer = New Timer() With {
            .Interval = 50
        }
        AddHandler _animationTimer.Tick, AddressOf OnAnimationTick
        _animationTimer.Start()
    End Sub

    Private Sub OnAnimationTick(sender As Object, e As EventArgs)
        _pulsePhase += 0.1F
        If _pulsePhase > Math.PI * 2 Then _pulsePhase = 0
        Invalidate()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _animationTimer?.Stop()
            _animationTimer?.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        Dim y = 10.0F
        Using font As New Font("Segoe UI", 9, FontStyle.Regular)
            For Each kvp As KeyValuePair(Of String, LEDStatus) In _statuses
                DrawStatusLine(g, 10, y, kvp.Key, kvp.Value, font)
                y += 25
            Next
        End Using
    End Sub

    Private Sub DrawStatusLine(g As Graphics, x As Single, y As Single, label As String, status As LEDStatus, font As Font)
        Dim ledSize = 10.0F
        Dim ledColor = GetLEDColor(status)

        ' LED glow (animate for active statuses)
        If status <> LEDStatus.Gray Then
            Dim pulseAlpha = CInt(30 + 30 * Math.Sin(_pulsePhase))
            Using glowPath As New GraphicsPath()
                glowPath.AddEllipse(x - 3, y - 3, ledSize + 6, ledSize + 6)
                Using glowBrush As New PathGradientBrush(glowPath)
                    glowBrush.CenterColor = Color.FromArgb(pulseAlpha, ledColor)
                    glowBrush.SurroundColors = {Color.FromArgb(0, ledColor)}
                    g.FillEllipse(glowBrush, x - 3, y - 3, ledSize + 6, ledSize + 6)
                End Using
            End Using
        End If

        ' LED body
        Using ledPath As New GraphicsPath()
            ledPath.AddEllipse(x, y, ledSize, ledSize)
            Using ledBrush As New PathGradientBrush(ledPath)
                ledBrush.CenterColor = Color.FromArgb(255, Math.Min(255, ledColor.R + 60), Math.Min(255, ledColor.G + 60), Math.Min(255, ledColor.B + 60))
                ledBrush.SurroundColors = {ledColor}
                g.FillEllipse(ledBrush, x, y, ledSize, ledSize)
            End Using
        End Using

        ' LED outline
        Using ledPen As New Pen(Color.FromArgb(80, 80, 80), 1)
            g.DrawEllipse(ledPen, x, y, ledSize, ledSize)
        End Using

        ' Label
        Using labelBrush As New SolidBrush(Color.FromArgb(80, 80, 80))
            g.DrawString(label, font, labelBrush, x + ledSize + 8, y - 2)
        End Using
    End Sub

    Private Shared Function GetLEDColor(status As LEDStatus) As Color
        Select Case status
            Case LEDStatus.Green : Return Color.FromArgb(50, 220, 50)
            Case LEDStatus.Yellow : Return Color.FromArgb(255, 220, 50)
            Case LEDStatus.Red : Return Color.FromArgb(220, 50, 50)
            Case LEDStatus.Gray : Return Color.FromArgb(120, 120, 120)
            Case Else : Return Color.Gray
        End Select
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class