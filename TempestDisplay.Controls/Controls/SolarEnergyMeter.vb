Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class SolarEnergyMeter
    Inherits Control

    Private _solarRadiation As Single = 0
    Private _maxRadiation As Single = 1200.0F

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SolarRadiation As Single
        Get
            Return _solarRadiation
        End Get
        Set(value As Single)
            _solarRadiation = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(1200.0F)>
    Public Property MaxRadiation As Single
        Get
            Return _maxRadiation
        End Get
        Set(value As Single)
            _maxRadiation = Math.Max(100, value)
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(200, 100)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        Dim barY = 30.0F
        Dim barHeight = 25.0F
        Dim barX = 50.0F
        Dim barWidth = Me.Width - 70.0F

        DrawSunIcon(g, 25, barY + barHeight / 2)
        DrawRadiationBar(g, barX, barY, barWidth, barHeight)
        DrawFill(g, barX, barY, barWidth, barHeight, _solarRadiation)
        DrawScale(g, barX, barY + barHeight + 5, barWidth)
        DrawReadout(g, Me.Width / 2.0F, barY + barHeight + 30, _solarRadiation)
    End Sub

    Private Shared Sub DrawSunIcon(g As Graphics, cx As Single, cy As Single)
        Dim sunSize = 12.0F

        ' Use a proper path-based gradient for the sun disk to avoid PathGradientBrush ctor issues
        Using path As New GraphicsPath()
            path.AddEllipse(cx - sunSize, cy - sunSize, sunSize * 2, sunSize * 2)
            Using sunBrush As New PathGradientBrush(path)
                sunBrush.CenterColor = Color.FromArgb(255, 255, 255, 150)
                sunBrush.SurroundColors = {Color.FromArgb(255, 255, 200, 80)}
                g.FillEllipse(sunBrush, cx - sunSize, cy - sunSize, sunSize * 2, sunSize * 2)
            End Using
        End Using

        Using rayPen As New Pen(Color.FromArgb(255, 200, 50), 2)
            For angle = 0 To 315 Step 45
                Dim angleRad = angle * Math.PI / 180
                Dim x1 = CSng(cx + (sunSize + 2) * Math.Cos(angleRad))
                Dim y1 = CSng(cy + (sunSize + 2) * Math.Sin(angleRad))
                Dim x2 = CSng(cx + (sunSize + 6) * Math.Cos(angleRad))
                Dim y2 = CSng(cy + (sunSize + 6) * Math.Sin(angleRad))
                g.DrawLine(rayPen, x1, y1, x2, y2)
            Next
        End Using
    End Sub

    Private Shared Sub DrawRadiationBar(g As Graphics, x As Single, y As Single, width As Single, height As Single)
        Using bgBrush As New LinearGradientBrush(New PointF(x, y), New PointF(x, y + height), Color.FromArgb(245, 245, 245), Color.FromArgb(225, 225, 225))
            g.FillRoundedRectangle(bgBrush, New RectangleF(x, y, width, height), height / 2)
        End Using
        Using outlinePen As New Pen(Color.FromArgb(150, 150, 150), 1.5F)
            g.DrawRoundedRectangle(outlinePen, New RectangleF(x, y, width, height), height / 2)
        End Using
    End Sub

    Private Sub DrawFill(g As Graphics, x As Single, y As Single, width As Single, height As Single, radiation As Single)
        Dim fillWidth = (Math.Min(radiation, _maxRadiation) / _maxRadiation) * width
        If fillWidth > 0 Then
            Dim fillColor = If(radiation < 200, Color.FromArgb(150, 200, 150), If(radiation < 600, Color.FromArgb(255, 220, 100), Color.FromArgb(255, 150, 50)))
            Using fillBrush As New LinearGradientBrush(New PointF(x, y), New PointF(x, y + height),
                Color.FromArgb(fillColor.A, Math.Min(255, fillColor.R + 30), Math.Min(255, fillColor.G + 30), Math.Min(255, fillColor.B + 30)), fillColor)
                g.FillRoundedRectangle(fillBrush, New RectangleF(x + 2, y + 2, fillWidth - 4, height - 4), (height - 4) / 2)
            End Using
        End If
    End Sub

    Private Sub DrawScale(g As Graphics, x As Single, y As Single, width As Single)
        Using font As New Font("Arial", 7, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center}
                    For i = 0 To _maxRadiation Step CInt(_maxRadiation / 6)
                        Dim markX = x + (i / _maxRadiation) * width
                        g.DrawString(i.ToString(), font, brush, markX, y, fmt)
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawReadout(g As Graphics, cx As Single, cy As Single, radiation As Single)
        Dim intensity = If(radiation < 200, "Weak", If(radiation < 600, "Moderate", If(radiation < 900, "Strong", "Very Strong")))
        Using font1 As New Font("Segoe UI", 10, FontStyle.Bold)
            Using font2 As New Font("Segoe UI", 8, FontStyle.Regular)
                Using brush As New SolidBrush(Color.FromArgb(50, 50, 50))
                    Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center}
                        g.DrawString($"{radiation:0} W/m²", font1, brush, cx, cy, fmt)
                        g.DrawString($"({intensity})", font2, brush, cx, cy + 16, fmt)
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class