Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Professional analog barometer control displaying atmospheric pressure with trend indicators.
''' Classic barometer face with color-coded weather zones and pressure trend arrows.
''' </summary>
<DefaultEvent("Click")>
Partial Public Class BarometerControl
    Inherits Control

    Private _pressureInHg As Single = 29.92F
    Private _trendDirection As PressureTrend = PressureTrend.Steady
    Private _minPressure As Single = 28.5F
    Private _maxPressure As Single = 31.0F
    Private _showTrendText As Boolean = True

    Public Enum PressureTrend
        FallingRapidly = -2
        Falling = -1
        Steady = 0
        Rising = 1
        RisingRapidly = 2
    End Enum

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PressureInHg As Single
        Get
            Return _pressureInHg
        End Get
        Set(value As Single)
            _pressureInHg = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PressureMb As Single
        Get
            Return _pressureInHg * 33.8639F
        End Get
        Set(value As Single)
            _pressureInHg = value / 33.8639F
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(PressureTrend.Steady)>
    Public Property Trend As PressureTrend
        Get
            Return _trendDirection
        End Get
        Set(value As PressureTrend)
            _trendDirection = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(28.5F)>
    Public Property MinPressure As Single
        Get
            Return _minPressure
        End Get
        Set(value As Single)
            _minPressure = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(31.0F)>
    Public Property MaxPressure As Single
        Get
            Return _maxPressure
        End Get
        Set(value As Single)
            _maxPressure = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property ShowTrendText As Boolean
        Get
            Return _showTrendText
        End Get
        Set(value As Boolean)
            _showTrendText = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(150, 150)
    End Sub

    Private Shared Function ValueToAngle(value As Single, minVal As Single, maxVal As Single, startAngle As Double, sweepAngle As Double) As Double
        Dim fraction As Double = (value - minVal) / (maxVal - minVal)
        Return startAngle + fraction * sweepAngle
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height
        Dim size As Single = Math.Min(w, h)
        Dim cx As Single = w / 2.0F
        Dim cy As Single = h / 2.0F
        Dim radius As Single = size / 2.0F - 10

        ' Draw components
        DrawBarometerBackground(g, cx, cy, radius)
        DrawWeatherZones(g, cx, cy, radius)
        DrawScaleMarks(g, cx, cy, radius)
        DrawNeedle(g, cx, cy, radius, _pressureInHg)

        ' Draw center hub and pressure text together, with text below the center dot
        DrawCenterAndText(g, cx, cy, radius, _pressureInHg)

        If _showTrendText Then
            DrawTrendIndicator(g, cx, cy + radius * 0.6F, _trendDirection)
        End If
    End Sub

    Private Shared Sub DrawBarometerBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer shadow
        Using shadowPath As New GraphicsPath()
            shadowPath.AddEllipse(cx - radius - 3, cy - radius - 3, (radius + 3) * 2, (radius + 3) * 2)
            Using shadowBrush As New PathGradientBrush(shadowPath)
                shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0)
                shadowBrush.SurroundColors = {Color.FromArgb(0, 0, 0, 0)}
                g.FillEllipse(shadowBrush, cx - radius - 3, cy - radius - 3, (radius + 3) * 2, (radius + 3) * 2)
            End Using
        End Using

        ' Background face with gradient
        Using bgPath As New GraphicsPath()
            bgPath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using bgBrush As New PathGradientBrush(bgPath)
                bgBrush.CenterColor = Color.FromArgb(250, 250, 245)
                bgBrush.SurroundColors = {Color.FromArgb(220, 220, 215)}
                g.FillEllipse(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2)
            End Using
        End Using

        ' Bezel
        Using bezelPen As New Pen(Color.FromArgb(100, 100, 105), 3.0F)
            g.DrawEllipse(bezelPen, cx - radius, cy - radius, radius * 2, radius * 2)
        End Using
    End Sub

    Private Sub DrawWeatherZones(g As Graphics, cx As Single, cy As Single, radius As Single)
        Dim startAngle As Double = 135.0
        Dim sweepAngle As Double = 270.0
        Dim arcRadius As Single = radius * 0.85F
        Dim arcWidth As Single = radius * 0.15F

        ' Define weather zones (inHg values)
        Dim zones As New List(Of Tuple(Of Single, Single, Color, String)) From {
            Tuple.Create(28.5F, 29.0F, Color.FromArgb(200, 50, 50), "Storm"),
            Tuple.Create(29.0F, 29.5F, Color.FromArgb(220, 140, 50), "Rain"),
            Tuple.Create(29.5F, 30.0F, Color.FromArgb(200, 180, 100), "Change"),
            Tuple.Create(30.0F, 30.5F, Color.FromArgb(100, 180, 100), "Fair"),
            Tuple.Create(30.5F, 31.0F, Color.FromArgb(100, 150, 220), "Dry")
        }

        For Each zone In zones
            Dim zoneStart As Double = ValueToAngle(zone.Item1, _minPressure, _maxPressure, startAngle, sweepAngle)
            Dim zoneEnd As Double = ValueToAngle(zone.Item2, _minPressure, _maxPressure, startAngle, sweepAngle)
            Dim zoneSweep As Double = zoneEnd - zoneStart

            Using zonePen As New Pen(zone.Item3, arcWidth)
                zonePen.StartCap = LineCap.Flat
                zonePen.EndCap = LineCap.Flat
                Dim rect As New RectangleF(cx - arcRadius, cy - arcRadius, arcRadius * 2, arcRadius * 2)
                g.DrawArc(zonePen, rect, CSng(zoneStart), CSng(zoneSweep))
            End Using
        Next
    End Sub

    Private Sub DrawScaleMarks(g As Graphics, cx As Single, cy As Single, radius As Single)
        Dim startAngle As Double = 135.0
        Dim sweepAngle As Double = 270.0
        Dim majorStep As Single = 0.2F
        Dim minorStep As Single = 0.05F

        Using majorPen As New Pen(Color.FromArgb(60, 60, 60), 2.5F)
            Using minorPen As New Pen(Color.FromArgb(100, 100, 100), 1.5F)
                Using font As New Font("Arial", 8.0F, FontStyle.Bold)
                    Using brush As New SolidBrush(Color.FromArgb(50, 50, 50))
                        Dim pressure As Single = _minPressure
                        While pressure <= _maxPressure
                            Dim isMajor As Boolean = Math.Abs(pressure - Math.Round(pressure / majorStep) * majorStep) < 0.001F
                            Dim angleDeg As Double = ValueToAngle(pressure, _minPressure, _maxPressure, startAngle, sweepAngle)
                            Dim angleRad As Double = angleDeg * Math.PI / 180.0

                            Dim tickLength As Single = If(isMajor, 12, 6)
                            Dim innerRadius As Single = radius * 0.75F
                            Dim outerRadius As Single = innerRadius + tickLength

                            Dim innerX As Single = CSng(cx + innerRadius * Math.Cos(angleRad))
                            Dim innerY As Single = CSng(cy + innerRadius * Math.Sin(angleRad))
                            Dim outerX As Single = CSng(cx + outerRadius * Math.Cos(angleRad))
                            Dim outerY As Single = CSng(cy + outerRadius * Math.Sin(angleRad))

                            g.DrawLine(If(isMajor, majorPen, minorPen), innerX, innerY, outerX, outerY)

                            ' Draw label for major ticks
                            If isMajor Then
                                Dim labelRadius As Single = innerRadius - 10
                                Dim labelX As Single = CSng(cx + labelRadius * Math.Cos(angleRad))
                                Dim labelY As Single = CSng(cy + labelRadius * Math.Sin(angleRad))
                                Using fmt As New StringFormat()
                                    fmt.Alignment = StringAlignment.Center
                                    fmt.LineAlignment = StringAlignment.Center
                                    g.DrawString(pressure.ToString("0.0"), font, brush, labelX, labelY, fmt)
                                End Using
                            End If

                            pressure += minorStep
                        End While
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawNeedle(g As Graphics, cx As Single, cy As Single, radius As Single, pressure As Single)
        Dim startAngle As Double = 135.0
        Dim sweepAngle As Double = 270.0
        Dim clampedPressure As Single = Math.Max(_minPressure, Math.Min(_maxPressure, pressure))
        Dim angleDeg As Double = ValueToAngle(clampedPressure, _minPressure, _maxPressure, startAngle, sweepAngle)
        Dim angleRad As Double = angleDeg * Math.PI / 180.0

        Dim needleLength As Single = radius * 0.7F
        Dim needleWidth As Single = 6.0F

        ' Needle points
        Dim tipX As Single = CSng(cx + needleLength * Math.Cos(angleRad))
        Dim tipY As Single = CSng(cy + needleLength * Math.Sin(angleRad))
        Dim perpAngle As Double = angleRad + Math.PI / 2.0
        Dim baseX1 As Single = CSng(cx + needleWidth / 2.0F * Math.Cos(perpAngle))
        Dim baseY1 As Single = CSng(cy + needleWidth / 2.0F * Math.Sin(perpAngle))
        Dim baseX2 As Single = CSng(cx - needleWidth / 2.0F * Math.Cos(perpAngle))
        Dim baseY2 As Single = CSng(cy - needleWidth / 2.0F * Math.Sin(perpAngle))

        ' Draw needle shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
            Dim shadowOffset As Single = 2.0F
            Dim shadowPoints() As PointF = {
                New PointF(tipX + shadowOffset, tipY + shadowOffset),
                New PointF(baseX1 + shadowOffset, baseY1 + shadowOffset),
                New PointF(baseX2 + shadowOffset, baseY2 + shadowOffset)
            }
            g.FillPolygon(shadowBrush, shadowPoints)
        End Using

        ' Draw needle with gradient
        Using needlePath As New GraphicsPath()
            needlePath.AddPolygon({New PointF(tipX, tipY), New PointF(baseX1, baseY1), New PointF(baseX2, baseY2)})
            Using needleBrush As New LinearGradientBrush(
                New PointF(cx, cy),
                New PointF(tipX, tipY),
                Color.FromArgb(80, 80, 80),
                Color.FromArgb(40, 40, 40))
                g.FillPath(needleBrush, needlePath)
            End Using
        End Using

        ' Needle outline
        Using needlePen As New Pen(Color.FromArgb(20, 20, 20), 1.0F)
            g.DrawPolygon(needlePen, {New PointF(tipX, tipY), New PointF(baseX1, baseY1), New PointF(baseX2, baseY2)})
        End Using
    End Sub

    Private Shared Sub DrawCenterHub(g As Graphics, cx As Single, cy As Single)
        Dim hubSize As Single = 12.0F
        Using hubPath As New GraphicsPath()
            hubPath.AddEllipse(cx - hubSize / 2, cy - hubSize / 2, hubSize, hubSize)
            Using hubBrush As New PathGradientBrush(hubPath)
                hubBrush.CenterColor = Color.FromArgb(180, 180, 180)
                hubBrush.SurroundColors = {Color.FromArgb(80, 80, 80)}
                g.FillEllipse(hubBrush, cx - hubSize / 2, cy - hubSize / 2, hubSize, hubSize)
            End Using
        End Using

        Using hubPen As New Pen(Color.FromArgb(60, 60, 60), 1.5F)
            g.DrawEllipse(hubPen, cx - hubSize / 2, cy - hubSize / 2, hubSize, hubSize)
        End Using
    End Sub

    Private Shared Sub DrawDigitalReadout(g As Graphics, cx As Single, cy As Single, pressure As Single)
        Using font As New Font("Segoe UI", 11.0F, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(30, 105, 210))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Center

                    Dim text As String = pressure.ToString("0.00") & " inHg"

                    ' Shadow
                    Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                        g.DrawString(text, font, shadowBrush, cx + 1, cy + 1, fmt)
                    End Using

                    g.DrawString(text, font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawTrendIndicator(g As Graphics, cx As Single, cy As Single, trend As PressureTrend)
        Dim trendText As String = ""
        Dim trendColor As Color = Color.Gray
        Dim arrowChar As String = ""

        Select Case trend
            Case PressureTrend.RisingRapidly
                trendText = "Rising Rapidly"
                trendColor = Color.FromArgb(50, 150, 50)
                arrowChar = "⤴"
            Case PressureTrend.Rising
                trendText = "Rising"
                trendColor = Color.FromArgb(100, 180, 100)
                arrowChar = "↗"
            Case PressureTrend.Steady
                trendText = "Steady"
                trendColor = Color.FromArgb(120, 120, 120)
                arrowChar = "→"
            Case PressureTrend.Falling
                trendText = "Falling"
                trendColor = Color.FromArgb(200, 140, 50)
                arrowChar = "↘"
            Case PressureTrend.FallingRapidly
                trendText = "Falling Rapidly"
                trendColor = Color.FromArgb(200, 50, 50)
                arrowChar = "⤵"
        End Select

        Using font As New Font("Segoe UI", 9.0F, FontStyle.Bold)
            Using arrowFont As New Font("Segoe UI", 14.0F, FontStyle.Bold)
                Using brush As New SolidBrush(trendColor)
                    Using fmt As New StringFormat()
                        fmt.Alignment = StringAlignment.Center
                        fmt.LineAlignment = StringAlignment.Center

                        ' Draw arrow
                        g.DrawString(arrowChar, arrowFont, brush, cx, cy - 8, fmt)

                        ' Draw text
                        g.DrawString(trendText, font, brush, cx, cy + 10, fmt)
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawCenterAndText(g As Graphics, cx As Single, cy As Single, radius As Single, pressure As Single)
        ' Center hub remains at dial center
        DrawCenterHub(g, cx, cy)

        ' Place the barometric pressure text clearly below the needle center dot,
        ' away from scale numbers (slightly below center)
        Dim textY As Single = cy + radius * 0.18F
        DrawPressureText(g, cx, textY, pressure)
    End Sub

    Private Shared Sub DrawPressureText(g As Graphics, cx As Single, cy As Single, pressure As Single)
        Using font As New Font("Segoe UI", 11.0F, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(30, 105, 210))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Center

                    Dim text As String = pressure.ToString("0.00") & " inHg"

                    ' Shadow
                    Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                        g.DrawString(text, font, shadowBrush, cx + 1, cy + 1, fmt)
                    End Using

                    g.DrawString(text, font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class