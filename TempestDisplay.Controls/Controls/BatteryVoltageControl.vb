Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class BatteryVoltageControl
    Inherits Control

    ' Data
    Private _voltage As Single = 2.6F

    Private _minVoltage As Single = 2.255F
    Private _maxVoltage As Single = 2.8F

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(80, 120)
        Me.BackColor = Color.FromArgb(245, 235, 220)
    End Sub

    ' Properties
    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(2.6F)>
    Public Property Voltage As Single
        Get
            Return _voltage
        End Get
        Set(value As Single)
            _voltage = Math.Max(_minVoltage, Math.Min(_maxVoltage, value))
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(2.255F)>
    Public Property MinVoltage As Single
        Get
            Return _minVoltage
        End Get
        Set(value As Single)
            _minVoltage = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(2.8F)>
    Public Property MaxVoltage As Single
        Get
            Return _maxVoltage
        End Get
        Set(value As Single)
            _maxVoltage = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.CompositingQuality = CompositingQuality.HighQuality

        Dim w = ClientSize.Width
        Dim h = ClientSize.Height
        If w <= 0 OrElse h <= 0 Then Return

        ' Font setup
        Dim baseFont As Font = If(Me.Font, New Font("Segoe UI", 9.0F, FontStyle.Regular))
        Dim titleSize As Single = Math.Min(Math.Max(baseFont.Size * 0.95F, 8.0F), 11.0F)
        Dim valueSize As Single = Math.Min(Math.Max(baseFont.Size * 1.1F, 9.0F), 13.0F)
        Dim smallSize As Single = Math.Min(Math.Max(baseFont.Size * 0.75F, 6.5F), 8.5F)

        Using titleFont As New Font(baseFont.FontFamily, titleSize, FontStyle.Bold),
              valueFont As New Font(baseFont.FontFamily, valueSize, FontStyle.Bold),
              smallFont As New Font(baseFont.FontFamily, smallSize, FontStyle.Regular)

            ' Padding
            Dim topPad As Integer = CInt(Math.Max(8, h * 0.06))
            Dim sidePad As Integer = CInt(Math.Max(6, w * 0.08))

            ' Title
            Using titleBrush As New SolidBrush(Color.FromArgb(60, 60, 60))
                Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
                g.DrawString("Battery", titleFont, titleBrush, w \ 2, topPad, sf)
            End Using

            ' Tower area
            Dim towerTop As Integer = topPad + CInt(titleFont.Height * 1.3)
            Dim towerWidth As Integer = Math.Max(CInt(w * 0.45), 30)
            Dim towerHeight As Integer = Math.Max(h - towerTop - CInt(valueFont.Height * 2.5) - 10, 60)
            Dim towerLeft As Integer = (w - towerWidth) \ 2
            Dim towerRect As New Rectangle(towerLeft, towerTop, towerWidth, towerHeight)

            ' Draw battery tower
            DrawBatteryTower(g, towerRect, smallFont)

            ' Current voltage display below tower
            Dim voltageY As Integer = towerRect.Bottom + 8
            Using voltageBrush As New SolidBrush(GetVoltageColor(_voltage))
                Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
                g.DrawString($"{_voltage:0.00}V", valueFont, voltageBrush, w \ 2, voltageY, sf)
            End Using

            ' Status text
            Dim statusY As Integer = voltageY + CInt(valueFont.Height * 1.1)
            Using statusBrush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
                g.DrawString(GetStatusText(_voltage), smallFont, statusBrush, w \ 2, statusY, sf)
            End Using
        End Using
    End Sub

    Private Sub DrawBatteryTower(g As Graphics, rect As Rectangle, smallFont As Font)
        ' Background tower (empty)
        Using bgBrush As New LinearGradientBrush(rect, Color.FromArgb(220, 220, 220), Color.FromArgb(240, 240, 240), 0.0F)
            Using path As New GraphicsPath()
                path.AddRoundedRectangle(rect, 6)
                g.FillPath(bgBrush, path)
            End Using
        End Using

        ' Border
        Using borderPen As New Pen(Color.FromArgb(160, 160, 160), 1.5F)
            Using path As New GraphicsPath()
                path.AddRoundedRectangle(rect, 6)
                g.DrawPath(borderPen, path)
            End Using
        End Using

        ' Calculate fill level
        Dim voltageRange As Single = _maxVoltage - _minVoltage
        Dim fillPercent As Single = (_voltage - _minVoltage) / voltageRange
        fillPercent = Math.Max(0.0F, Math.Min(1.0F, fillPercent))

        ' Fill area (from bottom up)
        Dim fillHeight As Integer = CInt(rect.Height * fillPercent)
        If fillHeight > 0 Then
            Dim fillRect As New Rectangle(rect.Left, rect.Bottom - fillHeight, rect.Width, fillHeight)

            ' Get gradient colors based on voltage
            Dim fillColor As Color = GetVoltageColor(_voltage)
            Dim fillColorLight As Color = Color.FromArgb(fillColor.A,
                Math.Min(255, fillColor.R + 30),
                Math.Min(255, fillColor.G + 30),
                Math.Min(255, fillColor.B + 30))

            Using fillBrush As New LinearGradientBrush(fillRect, fillColorLight, fillColor, 0.0F)
                Using path As New GraphicsPath()
                    path.AddRoundedRectangle(New Rectangle(fillRect.Left + 2, fillRect.Top, fillRect.Width - 4, fillRect.Height - 2), 5)
                    g.FillPath(fillBrush, path)
                End Using
            End Using
        End If

        ' Draw voltage scale markings - Mode thresholds
        Dim markings() As Single = {2.355F, 2.375F, 2.41F, 2.455F, 2.6F, 2.8F}
        Using tickBrush As New SolidBrush(Color.FromArgb(100, 100, 100)),
              tickPen As New Pen(Color.FromArgb(140, 140, 140), 1.0F)

            For Each mark In markings
                If mark >= _minVoltage AndAlso mark <= _maxVoltage Then
                    Dim markPercent As Single = (mark - _minVoltage) / voltageRange
                    Dim markY As Integer = rect.Bottom - CInt(rect.Height * markPercent)

                    ' Tick line
                    g.DrawLine(tickPen, rect.Left - 3, markY, rect.Left + 2, markY)
                    g.DrawLine(tickPen, rect.Right - 2, markY, rect.Right + 3, markY)

                    ' Label on left
                    Dim sf As New StringFormat() With {.Alignment = StringAlignment.Far, .LineAlignment = StringAlignment.Center}
                    g.DrawString(mark.ToString("0.00"), New Font(smallFont.FontFamily, smallFont.Size * 0.75F), tickBrush, rect.Left - 5, markY, sf)
                End If
            Next
        End Using

        ' Battery terminal on top
        Dim terminalWidth As Integer = CInt(rect.Width * 0.4)
        Dim terminalHeight As Integer = CInt(rect.Height * 0.06)
        Dim terminalRect As New Rectangle(rect.Left + (rect.Width - terminalWidth) \ 2, rect.Top - terminalHeight, terminalWidth, terminalHeight)
        Using terminalBrush As New LinearGradientBrush(terminalRect, Color.FromArgb(180, 180, 180), Color.FromArgb(140, 140, 140), 90.0F)
            Using path As New GraphicsPath()
                path.AddRoundedRectangle(terminalRect, 2)
                g.FillPath(terminalBrush, path)
            End Using
        End Using
    End Sub

    Private Shared Function GetVoltageColor(voltage As Single) As Color
        ' Mode 3 (Critical): < 2.355V (Dark Red)
        ' Mode 2 (Further Reduced): 2.355 - 2.375V (Red)
        ' Mode 1 (Reduced): 2.375 - 2.41V (Orange/Red)
        ' Between Mode 1 and Mode 0: 2.41 - 2.455V (Orange/Yellow)
        ' Mode 0 (Normal): >= 2.455V (Green)

        If voltage < 2.355F Then
            Return Color.FromArgb(180, 0, 0)  ' Dark Red - Mode 3 Critical
        ElseIf voltage < 2.375F Then
            Return Color.FromArgb(220, 50, 50)  ' Red - Mode 2
        ElseIf voltage < 2.41F Then
            Return Color.FromArgb(255, 120, 0)  ' Orange-Red - Mode 1
        ElseIf voltage < 2.455F Then
            Return Color.FromArgb(255, 180, 0)  ' Orange-Yellow - Transitioning to Mode 0
        Else
            Return Color.FromArgb(50, 180, 50)  ' Green - Mode 0 Normal
        End If
    End Function

    Private Shared Function GetStatusText(voltage As Single) As String
        If voltage < 2.355F Then
            Return "Mode 3 - Critical"
        ElseIf voltage < 2.375F Then
            Return "Mode 2 - Reduced"
        ElseIf voltage < 2.41F Then
            Return "Mode 1 - Reduced"
        ElseIf voltage < 2.455F Then
            Return "Entering Mode 0"
        Else
            Return "Mode 0 - Normal"
        End If
    End Function

End Class

' Helper module for rounded rectangles
Module GraphicsPathBatteryExtensions

    <System.Runtime.CompilerServices.Extension>
    Public Sub AddRoundedRectangle(path As GraphicsPath, rect As Rectangle, radius As Integer)
        Dim r As Integer = Math.Max(1, radius)
        Dim d As Integer = r * 2
        path.Reset()
        path.AddArc(rect.Left, rect.Top, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
    End Sub

End Module