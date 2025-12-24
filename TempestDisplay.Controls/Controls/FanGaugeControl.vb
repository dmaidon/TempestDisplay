Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class FanGaugeControl
    Inherits Control

    Private _value As Single
    Private _minValue As Single = 0
    Private _maxValue As Single = 100
    Private _label As String = ""

    ' Arc configuration: 180° sweep from 9:00 (left) to 3:00 (right)
    ' 0 at 9:00 position (180°), max at 3:00 position (0°/360°)
    Private Const StartAngle As Single = 180.0F  '180

    Private Const EndAngle As Single = 0.0F
    Private Const SweepAngle As Single = 180.0F ' Half circle sweep from left to right

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(0.0F)>
    Public Property Value As Single
        Get
            Return _value
        End Get
        Set(val As Single)
            _value = Math.Max(_minValue, Math.Min(_maxValue, val))
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(0.0F)>
    Public Property MinValue As Single
        Get
            Return _minValue
        End Get
        Set(val As Single)
            _minValue = val
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(100.0F)>
    Public Property MaxValue As Single
        Get
            Return _maxValue
        End Get
        Set(val As Single)
            _maxValue = val
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue("")>
    Public Property Label As String
        Get
            Return _label
        End Get
        Set(val As String)
            _label = val
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(100, 100)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.CompositingQuality = CompositingQuality.HighQuality

        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height
        Dim size As Single = CSng(Math.Min(w, h))
        Dim cx As Single = w / 2.0F
        Dim cy As Single = h / 2.0F

        ' Arc width
        Dim arcWidth As Single = Math.Max(8, size * 0.08F)
        Dim radius As Single = (size / 2.0F) - (arcWidth / 2.0F) - 8  ' Maximized gauge - only 8px from edge

        ' Draw professional background first
        DrawProfessionalBackground(g, cx, cy, radius + (arcWidth / 2.0F) + 5)

        ' Draw outer bezel to contain scale
        DrawOuterBezel(g, cx, cy, radius + (arcWidth / 2.0F) + 10)

        ' Fill the gap between bezel and gauge face with gray (only 2px gap)
        DrawBezelFill(g, cx, cy, radius + (arcWidth / 2.0F) + 2, radius + (arcWidth / 2.0F) + 7)

        ' Draw shadow for background arc
        DrawArcShadow(g, cx, cy, radius, arcWidth, StartAngle, SweepAngle)

        ' Draw background arc (light gray with subtle gradient)
        DrawBackgroundArc(g, cx, cy, radius, arcWidth)

        ' Calculate value position
        Dim fraction As Single = (_value - _minValue) / Math.Max(0.0001F, _maxValue - _minValue)
        Dim valueSweep As Single = fraction * SweepAngle

        ' Draw value arc with gradient (colored)
        DrawValueArc(g, cx, cy, radius, arcWidth, valueSweep, _value)

        ' Draw tick marks
        DrawTickMarks(g, cx, cy, radius, arcWidth)

        ' Draw center text
        DrawCenterText(g, cx, cy)

        ' Draw needle with shadow - maximized length for large gauge
        DrawNeedle(g, cx, cy, radius - (arcWidth / 2.0F) - 15, fraction)

        ' Glass overlay for final professional touch
        DrawGlassOverlay(g, cx, cy, radius + (arcWidth / 2.0F))
    End Sub

    Private Shared Sub DrawProfessionalBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Create half-circle region for clipping
        Using halfCirclePath As New GraphicsPath()
            ' Create full circle then clip to top half
            Dim fullRect As New RectangleF(cx - radius - 5, cy - radius - 5, (radius + 5) * 2, (radius + 5) * 2)
            halfCirclePath.AddArc(fullRect, 180, 180)
            halfCirclePath.AddLine(New PointF(cx - radius - 5, cy), New PointF(cx + radius + 5, cy))
            halfCirclePath.CloseFigure()

            ' Outer bezel shadow
            Using shadowBrush As New PathGradientBrush(halfCirclePath)
                shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0)
                shadowBrush.SurroundColors = New Color() {Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0)}
                g.FillPath(shadowBrush, halfCirclePath)
            End Using
        End Using

        ' Brushed metal background with radial gradient
        Using bgPath As New GraphicsPath()
            Dim bgRect As New RectangleF(cx - radius, cy - radius, radius * 2, radius * 2)
            bgPath.AddArc(bgRect, 180, 180)
            bgPath.AddLine(New PointF(cx - radius, cy), New PointF(cx + radius, cy))
            bgPath.CloseFigure()

            Using bgBrush As New PathGradientBrush(bgPath)
                bgBrush.CenterColor = Color.FromArgb(245, 245, 250)
                bgBrush.SurroundColors = New Color() {Color.FromArgb(200, 205, 215), Color.FromArgb(200, 205, 215), Color.FromArgb(200, 205, 215)}
                g.FillPath(bgBrush, bgPath)
            End Using
        End Using

        ' Inner shadow for depth
        Using innerShadowPath As New GraphicsPath()
            Dim innerRect As New RectangleF(cx - (radius * 0.9F), cy - (radius * 0.9F), radius * 1.8F, radius * 1.8F)
            innerShadowPath.AddArc(innerRect, 180, 180)
            innerShadowPath.AddLine(New PointF(cx - (radius * 0.9F), cy), New PointF(cx + (radius * 0.9F), cy))
            innerShadowPath.CloseFigure()

            Using innerShadow As New PathGradientBrush(innerShadowPath)
                innerShadow.CenterColor = Color.FromArgb(0, 0, 0, 0)
                innerShadow.SurroundColors = New Color() {Color.FromArgb(30, 0, 0, 0), Color.FromArgb(30, 0, 0, 0), Color.FromArgb(30, 0, 0, 0)}
                g.FillPath(innerShadow, innerShadowPath)
            End Using
        End Using

        ' Glossy highlight overlay on top portion
        Dim glossRect As New RectangleF(cx - (radius * 0.8F), cy - radius, radius * 1.6F, radius * 0.8F)
        Using glossBrush As New LinearGradientBrush(
            New PointF(cx, cy - radius),
            New PointF(cx, cy - (radius * 0.2F)),
            Color.FromArgb(80, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255))
            g.FillEllipse(glossBrush, glossRect)
        End Using
    End Sub

    Private Shared Sub DrawGlassOverlay(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Semi-transparent glass effect for half circle
        Using glassPath As New GraphicsPath()
            Dim glassRect As New RectangleF(cx - (radius * 0.85F), cy - (radius * 0.85F), radius * 1.7F, radius * 1.7F)
            glassPath.AddArc(glassRect, 180, 180)
            glassPath.AddLine(New PointF(cx - (radius * 0.85F), cy), New PointF(cx + (radius * 0.85F), cy))
            glassPath.CloseFigure()

            Using glassBrush As New LinearGradientBrush(
                New PointF(cx, cy - radius),
                New PointF(cx, cy),
                Color.FromArgb(40, 255, 255, 255),
                Color.FromArgb(5, 255, 255, 255))
                g.FillPath(glassBrush, glassPath)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawArcShadow(g As Graphics, cx As Single, cy As Single, radius As Single, arcWidth As Single, startAngle As Single, sweepAngle As Single)
        Using shadowPen As New Pen(Color.FromArgb(30, 0, 0, 0), arcWidth + 3)
            shadowPen.LineJoin = LineJoin.Round
            shadowPen.StartCap = LineCap.Round
            shadowPen.EndCap = LineCap.Round
            Dim arcRect As New RectangleF(cx - radius + 2, cy - radius + 2, radius * 2.0F, radius * 2.0F)
            g.DrawArc(shadowPen, arcRect, startAngle, sweepAngle)
        End Using
    End Sub

    Private Shared Sub DrawBackgroundArc(g As Graphics, cx As Single, cy As Single, radius As Single, arcWidth As Single)
        Dim arcRect As New RectangleF(cx - radius, cy - radius, radius * 2.0F, radius * 2.0F)

        ' Subtle gradient for background
        Using gradBrush As New LinearGradientBrush(
            New PointF(cx - radius, cy),
            New PointF(cx + radius, cy),
            Color.FromArgb(220, 220, 225),
            Color.FromArgb(190, 190, 200))

            Using penBg As New Pen(gradBrush, arcWidth)
                penBg.LineJoin = LineJoin.Round
                penBg.StartCap = LineCap.Round
                penBg.EndCap = LineCap.Round
                g.DrawArc(penBg, arcRect, StartAngle, SweepAngle)
            End Using
        End Using
    End Sub

    Private Sub DrawValueArc(g As Graphics, cx As Single, cy As Single, radius As Single, arcWidth As Single, valueSweep As Single, currentValue As Single)
        If valueSweep <= 0 Then Return

        Dim arcRect As New RectangleF(cx - radius, cy - radius, radius * 2.0F, radius * 2.0F)
        Dim arcColor As Color = GetColorForValue(currentValue)
        Dim arcColorLight As Color = Color.FromArgb(
            Math.Min(255, arcColor.R + 40),
            Math.Min(255, arcColor.G + 40),
            Math.Min(255, arcColor.B + 40))

        ' Create gradient for value arc
        Using gradBrush As New LinearGradientBrush(
            New PointF(cx - radius, cy),
            New PointF(cx + radius, cy),
            arcColorLight,
            arcColor)

            Using penValue As New Pen(gradBrush, arcWidth)
                penValue.LineJoin = LineJoin.Round
                penValue.StartCap = LineCap.Round
                penValue.EndCap = LineCap.Round
                g.DrawArc(penValue, arcRect, StartAngle, valueSweep)
            End Using
        End Using
    End Sub

    Private Function GetColorForValue(val As Single) As Color
        Dim fraction As Single = (val - _minValue) / Math.Max(0.0001F, _maxValue - _minValue)
        If fraction < 0.33F Then
            Return Color.FromArgb(40, 180, 40) ' Professional green
        ElseIf fraction < 0.66F Then
            Return Color.FromArgb(220, 180, 0) ' Professional gold
        Else
            Return Color.FromArgb(220, 60, 0) ' Professional orange-red
        End If
    End Function

    Private Sub DrawTickMarks(g As Graphics, cx As Single, cy As Single, radius As Single, arcWidth As Single)
        ' Draw minor ticks
        Using penMinorTick As New Pen(Color.FromArgb(100, 100, 100), 1.5F)
            penMinorTick.StartCap = LineCap.Round
            penMinorTick.EndCap = LineCap.Round
            Dim minorTickLength As Single = arcWidth * 0.5F
            Dim minorStep As Integer = 2

            For val As Integer = CInt(_minValue) To CInt(_maxValue) Step minorStep
                If val Mod 10 = 0 Then Continue For

                Dim fraction As Single = (val - _minValue) / Math.Max(0.0001F, _maxValue - _minValue)
                Dim angleDeg As Single = StartAngle + (fraction * SweepAngle)
                Dim angleRad As Double = angleDeg * Math.PI / 180.0

                Dim innerX As Single = CSng(cx + ((radius - minorTickLength) * Math.Cos(angleRad)))
                Dim innerY As Single = CSng(cy + ((radius - minorTickLength) * Math.Sin(angleRad)))
                Dim outerX As Single = CSng(cx + (radius * Math.Cos(angleRad)))
                Dim outerY As Single = CSng(cy + (radius * Math.Sin(angleRad)))

                g.DrawLine(penMinorTick, innerX, innerY, outerX, outerY)
            Next
        End Using

        ' Draw major ticks and labels
        Using penMajorTick As New Pen(Color.FromArgb(50, 50, 50), 3.0F)
            penMajorTick.LineJoin = LineJoin.Round
            penMajorTick.StartCap = LineCap.Round
            penMajorTick.EndCap = LineCap.Round
            Dim majorTickLength As Single = arcWidth * 0.75F
            Dim majorStep As Integer = 10

            For val As Integer = CInt(_minValue) To CInt(_maxValue) Step majorStep
                Dim fraction As Single = (val - _minValue) / Math.Max(0.0001F, _maxValue - _minValue)
                Dim angleDeg As Single = StartAngle + (fraction * SweepAngle)
                Dim angleRad As Double = angleDeg * Math.PI / 180.0

                Dim innerX As Single = CSng(cx + ((radius - majorTickLength) * Math.Cos(angleRad)))
                Dim innerY As Single = CSng(cy + ((radius - majorTickLength) * Math.Sin(angleRad)))
                Dim outerX As Single = CSng(cx + (radius * Math.Cos(angleRad)))
                Dim outerY As Single = CSng(cy + (radius * Math.Sin(angleRad)))

                g.DrawLine(penMajorTick, innerX, innerY, outerX, outerY)

                ' Draw label inside the arc - maximized spacing for large gauge
                Dim labelRadius As Single = radius - majorTickLength - 30
                Dim labelX As Single = CSng(cx + (labelRadius * Math.Cos(angleRad)))
                Dim labelY As Single = CSng(cy + (labelRadius * Math.Sin(angleRad)))

                Using font As New Font("Segoe UI", Math.Max(8, Me.Font.Size), FontStyle.Bold)
                    Using brush As New SolidBrush(Color.FromArgb(70, 70, 70))
                        Using sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Center
                            ' Text shadow for depth
                            Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                                g.DrawString(val.ToString(), font, shadowBrush, New PointF(labelX + 1, labelY + 1), sf)
                            End Using
                            g.DrawString(val.ToString(), font, brush, New PointF(labelX, labelY), sf)
                        End Using
                    End Using
                End Using
            Next
        End Using
    End Sub

    Private Shared Sub DrawNeedle(g As Graphics, cx As Single, cy As Single, length As Single, fraction As Single)
        Dim angleDeg As Single = StartAngle + (fraction * SweepAngle)
        Dim angleRad As Double = angleDeg * Math.PI / 180.0

        Dim px As Single = CSng(cx + (length * Math.Cos(angleRad)))
        Dim py As Single = CSng(cy + (length * Math.Sin(angleRad)))

        ' Draw needle shadow
        Using shadowPen As New Pen(Color.FromArgb(60, 0, 0, 0), 3.5F)
            shadowPen.EndCap = LineCap.Round
            shadowPen.StartCap = LineCap.Round
            Dim shadowOffset As Single = 2.0F
            g.DrawLine(shadowPen, cx + shadowOffset, cy + shadowOffset, px + shadowOffset, py + shadowOffset)
        End Using

        ' Draw main needle
        Using pen As New Pen(Color.FromArgb(40, 40, 40), 3.0F)
            pen.EndCap = LineCap.Round
            pen.StartCap = LineCap.Round
            g.DrawLine(pen, cx, cy, px, py)
        End Using

        ' Draw enhanced hub with gradient
        Dim hubSize As Single = 12.0F
        Using hubPath As New GraphicsPath()
            hubPath.AddEllipse(cx - (hubSize / 2.0F), cy - (hubSize / 2.0F), hubSize, hubSize)
            Using hubGrad As New PathGradientBrush(hubPath)
                hubGrad.CenterColor = Color.FromArgb(220, 40, 40, 40)
                hubGrad.SurroundColors = New Color() {Color.FromArgb(160, 40, 40, 40)}
                g.FillEllipse(hubGrad, cx - (hubSize / 2.0F), cy - (hubSize / 2.0F), hubSize, hubSize)
            End Using
        End Using

        ' Add subtle highlight on hub
        Using highlightBrush As New SolidBrush(Color.FromArgb(60, 255, 255, 255))
            Dim highlightSize As Single = hubSize * 0.4F
            g.FillEllipse(highlightBrush, cx - (hubSize / 4.0F), cy - (hubSize / 3.0F), highlightSize, highlightSize)
        End Using

        ' Draw needle tip dot with glow
        Using glowBrush As New PathGradientBrush(New PointF() {
            New PointF(px - 10, py - 10),
            New PointF(px + 10, py - 10),
            New PointF(px + 10, py + 10),
            New PointF(px - 10, py + 10)
        })
            glowBrush.CenterPoint = New PointF(px, py)
            glowBrush.CenterColor = Color.FromArgb(100, 220, 60, 60)
            glowBrush.SurroundColors = New Color() {Color.FromArgb(0, 220, 60, 60), Color.FromArgb(0, 220, 60, 60), Color.FromArgb(0, 220, 60, 60), Color.FromArgb(0, 220, 60, 60)}
            g.FillEllipse(glowBrush, New RectangleF(px - 10, py - 10, 20, 20))
        End Using

        Using dotBrush As New SolidBrush(Color.FromArgb(220, 60, 60))
            Dim dotSize As Single = 7.0F
            g.FillEllipse(dotBrush, px - (dotSize / 2.0F), py - (dotSize / 2.0F), dotSize, dotSize)
        End Using

        ' Add highlight to dot
        Using highlightBrush As New SolidBrush(Color.FromArgb(150, 255, 255, 255))
            Dim highlightSize As Single = 3.0F
            g.FillEllipse(highlightBrush, px - 2, py - 2, highlightSize, highlightSize)
        End Using
    End Sub

    Private Sub DrawCenterText(g As Graphics, cx As Single, cy As Single)
        Using fmt As New StringFormat()
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Center

            ' Draw value with layered shadows
            Using fontValue As New Font("Segoe UI", Math.Max(12, Me.Font.Size + 8), FontStyle.Bold)
                Using brushValue As New SolidBrush(Color.FromArgb(40, 40, 40))
                    ' Outer shadow
                    Using shadowBrush As New SolidBrush(Color.FromArgb(50, 0, 0, 0))
                        g.DrawString(_value.ToString("0.#"), fontValue, shadowBrush, New PointF(cx + 2, cy + 21), fmt) '11
                    End Using
                    ' Inner shadow
                    Using shadowBrush2 As New SolidBrush(Color.FromArgb(25, 0, 0, 0))
                        g.DrawString(_value.ToString("0.#"), fontValue, shadowBrush2, New PointF(cx + 1, cy + 20), fmt)  '10
                    End Using
                    g.DrawString(_value.ToString("0.#"), fontValue, brushValue, New PointF(cx, cy + 20), fmt)  '10
                End Using
            End Using

            ' Draw label under the center pivot point
            If Not String.IsNullOrEmpty(_label) Then
                Using fontUnder As New Font("Segoe UI", Math.Max(7, Me.Font.Size), FontStyle.Bold)
                    Using brushUnder As New SolidBrush(Color.FromArgb(100, 100, 100))
                        ' Shadow for label
                        Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                            g.DrawString(_label, fontUnder, shadowBrush, New PointF(cx + 1, cy + 46), fmt)      '36
                        End Using
                        g.DrawString(_label, fontUnder, brushUnder, New PointF(cx, cy + 45), fmt)       '35
                    End Using
                End Using
            End If
        End Using
    End Sub

    Private Shared Sub DrawOuterBezel(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer bezel ring to contain scale and give professional appearance
        ' For half-circle gauge, only draw the top arc portion
        Dim bezelWidth As Single = 4.0F
        Dim bezelRadius As Single = radius - 2

        ' Outer dark ring (simulates metal frame depth)
        Using outerPen As New Pen(Color.FromArgb(80, 80, 85), bezelWidth)
            outerPen.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawArc(outerPen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, 180, 180)
        End Using

        ' Middle metallic ring with gradient
        Using middlePath As New GraphicsPath()
            middlePath.AddArc(cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, 180, 180)

            Using middleBrush As New PathGradientBrush(middlePath)
                middleBrush.CenterColor = Color.FromArgb(140, 145, 150)
                middleBrush.SurroundColors = {Color.FromArgb(90, 95, 100), Color.FromArgb(90, 95, 100)}

                Using middlePen As New Pen(middleBrush, bezelWidth - 1)
                    middlePen.Alignment = Drawing2D.PenAlignment.Center
                    g.DrawArc(middlePen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, 180, 180)
                End Using
            End Using
        End Using

        ' Inner highlight ring for depth
        Dim innerRadius As Single = bezelRadius - (bezelWidth * 1.2F)
        Using innerPen As New Pen(Color.FromArgb(120, 180, 185, 190), 1.5F)
            innerPen.Alignment = Drawing2D.PenAlignment.Center
            g.DrawArc(innerPen, cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2, 180, 180)
        End Using

        ' Subtle top-left highlight for 3D effect
        Using highlightPen As New Pen(Color.FromArgb(80, 220, 220, 220), 1.0F)
            highlightPen.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawArc(highlightPen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, 200, 60)
        End Using

        ' Subtle top-right shadow for 3D effect
        Using shadowPen As New Pen(Color.FromArgb(80, 40, 40, 40), 1.0F)
            shadowPen.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawArc(shadowPen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, 320, 60)
        End Using
    End Sub

    Private Shared Sub DrawBezelFill(g As Graphics, cx As Single, cy As Single, innerRadius As Single, outerRadius As Single)
        ' Fill the ring area between the gauge face and the bezel with a blended gray
        ' For half-circle gauge, create a half-ring shape
        Using ringPath As New GraphicsPath()
            ' Create the outer arc
            ringPath.AddArc(cx - outerRadius, cy - outerRadius, outerRadius * 2, outerRadius * 2, 180, 180)
            ' Line to inner arc start
            ringPath.AddLine(cx + outerRadius, cy, cx + innerRadius, cy)
            ' Inner arc (reverse direction)
            ringPath.AddArc(cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2, 0, -180)
            ' Close the path
            ringPath.CloseFigure()

            ' Fill with gradient
            Using fillBrush As New PathGradientBrush(ringPath)
                fillBrush.CenterColor = Color.FromArgb(230, 230, 235)
                fillBrush.SurroundColors = {Color.FromArgb(200, 205, 210), Color.FromArgb(200, 205, 210)}
                g.FillPath(fillBrush, ringPath)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class