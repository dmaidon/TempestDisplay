Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class TempGaugeControl
    Inherits Control

    Private _tempF As Single
    Private _tempC As Single
    Private _label As String = ""
    Private _minF As Single = -5
    Private _maxF As Single = 110

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property TempF As Single
        Get
            Return _tempF
        End Get
        Set(value As Single)
            _tempF = value
            _tempC = (value - 32.0F) * 5.0F / 9.0F
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property TempC As Single
        Get
            Return _tempC
        End Get
        Set(value As Single)
            _tempC = value
            _tempF = value * 9.0F / 5.0F + 32.0F
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
        Set(value As String)
            _label = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(-5.0F)>
    Public Property MinF As Single
        Get
            Return _minF
        End Get
        Set(value As Single)
            _minF = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(110.0F)>
    Public Property MaxF As Single
        Get
            Return _maxF
        End Get
        Set(value As Single)
            _maxF = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(80, 80)
    End Sub

    ' Map value to angle using startAngle and sweepAngle
    Private Shared Function ValueToAngleDeg(value As Single, minVal As Single, maxVal As Single, startAngle As Double, sweepAngle As Double) As Double
        Dim fraction As Double = (value - minVal) / (maxVal - minVal)
        Return startAngle + fraction * sweepAngle
    End Function

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

        ' ring widths (narrower)
        Dim ringOuter As Single = Math.Max(2, size * 0.018F)
        Dim outerRadius As Single = size / 2.0F - ringOuter - 30

        ' Speedometer arc angles
        Dim startAngle As Double = 240.0
        Dim sweepAngle As Double = 240.0

        ' Draw professional background first
        DrawProfessionalBackground(g, cx, cy, outerRadius + ringOuter + 5)

        ' Draw outer bezel to contain scale numbers
        DrawOuterBezel(g, cx, cy, outerRadius + ringOuter + 30)

        ' Fill the gap between bezel and gauge face with gray
        DrawBezelFill(g, cx, cy, outerRadius + ringOuter + 5, outerRadius + ringOuter + 25)

        ' Draw subtle shadow for depth
        DrawArcShadow(g, cx, cy, outerRadius, ringOuter, startAngle, sweepAngle)

        ' Draw arc with gradient (outer ring, speedometer style)
        DrawGradientArc(g, cx, cy, outerRadius, ringOuter, startAngle, sweepAngle)

        ' Draw minor ticks (Fahrenheit only, inside arc)
        DrawMinorTicks(g, cx, cy, outerRadius - ringOuter * 2, ringOuter, _minF, _maxF, 5, 15, Color.FromArgb(80, 80, 80), startAngle, sweepAngle)

        ' Draw scale ticks and labels (Fahrenheit only, inside arc)
        DrawScaleTicks(g, cx, cy, outerRadius - ringOuter * 2, ringOuter, _minF, _maxF, 15, Color.FromArgb(30, 105, 210), startAngle, sweepAngle)

        ' Single black needle with shadow (outer geometry)
        DrawPointer(g, cx, cy, outerRadius, TempF, _minF, _maxF, Color.FromArgb(40, 40, 40), ringOuter, startAngle, sweepAngle)

        ' small dot on arc with glow (Fahrenheit only)
        DrawPointOnArc(g, cx, cy, outerRadius, ringOuter, TempF, _minF, _maxF, Color.FromArgb(30, 105, 210), startAngle, sweepAngle)

        ' center text (F and C)
        DrawCenterText(g, cx, cy, TempF, TempC)

        ' Draw special tick at 32°F (outside the ring, IndianRed)
        DrawSpecialTick(g, cx, cy, outerRadius, ringOuter, 32.0F, _minF, _maxF, startAngle, sweepAngle)

        ' Glass overlay for final professional touch
        DrawGlassOverlay(g, cx, cy, outerRadius)
    End Sub

    Private Shared Sub DrawProfessionalBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer bezel shadow
        Using shadowPath As New GraphicsPath()
            shadowPath.AddEllipse(cx - radius - 5, cy - radius - 5, (radius + 5) * 2, (radius + 5) * 2)
            Using shadowBrush As New PathGradientBrush(shadowPath)
                shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0)
                shadowBrush.SurroundColors = {Color.FromArgb(0, 0, 0, 0)}
                g.FillEllipse(shadowBrush, cx - radius - 5, cy - radius - 5, (radius + 5) * 2, (radius + 5) * 2)
            End Using
        End Using

        ' Brushed metal background with radial gradient
        Using bgPath As New GraphicsPath()
            bgPath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using bgBrush As New PathGradientBrush(bgPath)
                bgBrush.CenterColor = Color.FromArgb(245, 245, 250)
                bgBrush.SurroundColors = {Color.FromArgb(200, 205, 215)}
                g.FillEllipse(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2)
            End Using
        End Using

        ' Inner shadow for depth
        Using innerShadow As New PathGradientBrush(New PointF() {
            New PointF(cx - radius * 0.9F, cy - radius * 0.9F),
            New PointF(cx + radius * 0.9F, cy - radius * 0.9F),
            New PointF(cx + radius * 0.9F, cy + radius * 0.9F),
            New PointF(cx - radius * 0.9F, cy + radius * 0.9F)
        })
            innerShadow.CenterPoint = New PointF(cx, cy)
            innerShadow.CenterColor = Color.FromArgb(0, 0, 0, 0)
            innerShadow.SurroundColors = {Color.FromArgb(30, 0, 0, 0), Color.FromArgb(30, 0, 0, 0),
                                           Color.FromArgb(30, 0, 0, 0), Color.FromArgb(30, 0, 0, 0)}
            g.FillEllipse(innerShadow, cx - radius * 0.9F, cy - radius * 0.9F, radius * 1.8F, radius * 1.8F)
        End Using

        ' Glossy highlight overlay on top half
        Using glossPath As New GraphicsPath()
            glossPath.AddEllipse(cx - radius * 0.8F, cy - radius, radius * 1.6F, radius * 1.5F)
            Using glossBrush As New LinearGradientBrush(
                New PointF(cx, cy - radius),
                New PointF(cx, cy),
                Color.FromArgb(80, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255))
                g.FillPath(glossBrush, glossPath)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawOuterBezel(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer bezel ring to contain scale numbers and give professional appearance
        Dim bezelWidth As Single = 4.0F
        Dim bezelRadius As Single = radius

        ' Outer dark ring (simulates metal frame depth)
        Using outerPen As New Pen(Color.FromArgb(80, 80, 85), bezelWidth)
            outerPen.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawEllipse(outerPen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2)
        End Using

        ' Middle metallic ring with gradient
        Using middlePath As New GraphicsPath()
            Dim middleRadius As Single = bezelRadius - bezelWidth / 2
            middlePath.AddEllipse(cx - middleRadius, cy - middleRadius, middleRadius * 2, middleRadius * 2)

            Using middleBrush As New PathGradientBrush(middlePath)
                middleBrush.CenterColor = Color.FromArgb(140, 145, 150)
                middleBrush.SurroundColors = {Color.FromArgb(90, 95, 100)}

                Using middlePen As New Pen(middleBrush, bezelWidth - 1)
                    middlePen.Alignment = Drawing2D.PenAlignment.Center
                    g.DrawEllipse(middlePen, cx - middleRadius, cy - middleRadius, middleRadius * 2, middleRadius * 2)
                End Using
            End Using
        End Using

        ' Inner highlight ring for depth
        Dim innerRadius As Single = bezelRadius - bezelWidth * 1.2F
        Using innerPen As New Pen(Color.FromArgb(120, 180, 185, 190), 1.5F)
            innerPen.Alignment = Drawing2D.PenAlignment.Center
            g.DrawEllipse(innerPen, cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2)
        End Using

        ' Subtle top-left highlight for 3D effect
        Using highlightPen As New Pen(Color.FromArgb(80, 220, 220, 220), 1.0F)
            highlightPen.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawArc(highlightPen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, 135, 90)
        End Using

        ' Subtle bottom-right shadow for 3D effect
        Using shadowPen As New Pen(Color.FromArgb(80, 40, 40, 40), 1.0F)
            shadowPen.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawArc(shadowPen, cx - bezelRadius, cy - bezelRadius, bezelRadius * 2, bezelRadius * 2, -45, 90)
        End Using
    End Sub

    Private Shared Sub DrawBezelFill(g As Graphics, cx As Single, cy As Single, innerRadius As Single, outerRadius As Single)
        ' Fill the ring area between the gauge face and the bezel with a blended gray
        Using fillPath As New GraphicsPath()
            fillPath.AddEllipse(cx - outerRadius, cy - outerRadius, outerRadius * 2, outerRadius * 2)

            Using fillBrush As New PathGradientBrush(fillPath)
                ' Subtle gradient from lighter center to darker edge for depth
                fillBrush.CenterColor = Color.FromArgb(230, 230, 235)
                fillBrush.SurroundColors = {Color.FromArgb(200, 205, 210)}

                ' Create a donut/ring shape using GraphicsPath
                Using ringPath As New GraphicsPath()
                    ' Outer circle
                    ringPath.AddEllipse(cx - outerRadius, cy - outerRadius, outerRadius * 2, outerRadius * 2)
                    ' Inner circle (subtract this to create ring)
                    ringPath.AddEllipse(cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2)

                    g.FillPath(fillBrush, ringPath)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawGlassOverlay(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Semi-transparent glass effect
        Using glassPath As New GraphicsPath()
            glassPath.AddEllipse(cx - radius * 0.85F, cy - radius * 0.85F, radius * 1.7F, radius * 1.7F)
            Using glassBrush As New LinearGradientBrush(
                New PointF(cx, cy - radius),
                New PointF(cx, cy + radius),
                Color.FromArgb(40, 255, 255, 255),
                Color.FromArgb(5, 255, 255, 255))
                g.FillEllipse(glassBrush, cx - radius * 0.85F, cy - radius * 0.85F, radius * 1.7F, radius * 1.7F)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawArcShadow(g As Graphics, cx As Single, cy As Single, radius As Single, ringWidth As Single, startAngle As Double, sweepAngle As Double)
        Using shadowPen As New Pen(Color.FromArgb(30, 0, 0, 0), ringWidth + 2)
            shadowPen.LineJoin = LineJoin.Round
            Dim rect As New RectangleF(cx - radius + 2, cy - radius + 2, radius * 2.0F, radius * 2.0F)
            g.DrawArc(shadowPen, rect, CSng(startAngle), CSng(sweepAngle))
        End Using
    End Sub

    Private Shared Sub DrawGradientArc(g As Graphics, cx As Single, cy As Single, radius As Single, ringWidth As Single, startAngle As Double, sweepAngle As Double)
        Dim rect As New RectangleF(cx - radius, cy - radius, radius * 2.0F, radius * 2.0F)

        ' Create gradient brush from top to bottom - professional blue tones
        Using gradBrush As New LinearGradientBrush(
            New PointF(cx, cy - radius),
            New PointF(cx, cy + radius),
            Color.FromArgb(70, 130, 220),
            Color.FromArgb(30, 105, 210))

            Using penF As New Pen(gradBrush, ringWidth)
                penF.LineJoin = LineJoin.Round
                penF.StartCap = LineCap.Round
                penF.EndCap = LineCap.Round
                g.DrawArc(penF, rect, CSng(startAngle), CSng(sweepAngle))
            End Using
        End Using
    End Sub

    Private Sub DrawSpecialTick(g As Graphics, cx As Single, cy As Single, radius As Single, ringWidth As Single, specialValue As Single, minVal As Single, maxVal As Single, startAngle As Double, sweepAngle As Double)
        Dim specialAngleDeg As Double = ValueToAngleDeg(specialValue, minVal, maxVal, startAngle, sweepAngle)
        Dim specialAngleRad As Double = specialAngleDeg * Math.PI / 180.0
        Dim tickLength As Single = Math.Max(2, ringWidth * 2.5F)
        Dim tickWidth As Single = 3.5F
        Dim outerX As Single = CSng(cx + (radius + tickLength) * Math.Cos(specialAngleRad))
        Dim outerY As Single = CSng(cy + (radius + tickLength) * Math.Sin(specialAngleRad))
        Dim innerX As Single = CSng(cx + radius * Math.Cos(specialAngleRad))
        Dim innerY As Single = CSng(cy + radius * Math.Sin(specialAngleRad))

        Using penSpecial As New Pen(Color.FromArgb(180, 60, 60), tickWidth)
            penSpecial.LineJoin = LineJoin.Round
            penSpecial.StartCap = LineCap.Round
            penSpecial.EndCap = LineCap.Round
            g.DrawLine(penSpecial, innerX, innerY, outerX, outerY)
        End Using

        ' Draw label for special tick at 32°F
        Dim labelOffset As Single = tickLength + 4      '10
        Dim labelX As Single = CSng(cx + (radius + labelOffset) * Math.Cos(specialAngleRad))
        Dim labelY As Single = CSng(cy + (radius + labelOffset) * Math.Sin(specialAngleRad))
        Using fontLabel As New Font("Arial", Math.Max(7, Me.Font.Size - 1), FontStyle.Bold)
            Using brushLabel As New SolidBrush(Color.FromArgb(180, 60, 60))
                Dim sf As New StringFormat With {
                    .Alignment = StringAlignment.Center,
                    .LineAlignment = StringAlignment.Center
                }
                ' Shadow for label
                Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                    g.DrawString("32°", fontLabel, shadowBrush, New PointF(labelX + 1, labelY + 1), sf)
                End Using
                g.DrawString("32°", fontLabel, brushLabel, New PointF(labelX, labelY), sf)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawMinorTicks(g As Graphics, cx As Single, cy As Single, radius As Single, ringWidth As Single, minVal As Single, maxVal As Single, minorStep As Integer, majorStep As Integer, color As Color, startAngle As Double, sweepAngle As Double)
        Dim minorTickLength As Single = Math.Max(4, ringWidth * 1.1F)
        Dim minorTickWidth As Single = 2.0F
        Using penTick As New Pen(color, minorTickWidth)
            penTick.StartCap = LineCap.Round
            penTick.EndCap = LineCap.Round
            Dim vMin As Integer = CInt(Math.Ceiling(minVal))
            Dim vMax As Integer = CInt(Math.Floor(maxVal))
            For v As Integer = vMin To vMax Step minorStep
                If (v - minVal) Mod majorStep = 0 Then Continue For
                Dim angleDeg As Double = ValueToAngleDeg(CSng(v), minVal, maxVal, startAngle, sweepAngle)
                Dim angleRad As Double = angleDeg * Math.PI / 180.0
                Dim innerX As Single = CSng(cx + radius * Math.Cos(angleRad))
                Dim innerY As Single = CSng(cy + radius * Math.Sin(angleRad))
                Dim outerX As Single = CSng(cx + (radius + minorTickLength) * Math.Cos(angleRad))
                Dim outerY As Single = CSng(cy + (radius + minorTickLength) * Math.Sin(angleRad))
                g.DrawLine(penTick, innerX, innerY, outerX, outerY)
            Next
        End Using
    End Sub

    Private Sub DrawScaleTicks(g As Graphics, cx As Single, cy As Single, radius As Single, ringWidth As Single, minVal As Single, maxVal As Single, majorStep As Integer, color As Color, startAngle As Double, sweepAngle As Double)
        Dim majorTickLength As Single = Math.Max(8, ringWidth * 2.0F)
        Dim majorTickWidth As Single = 3.5F
        Using penTick As New Pen(color, majorTickWidth)
            penTick.LineJoin = LineJoin.Round
            penTick.StartCap = LineCap.Round
            penTick.EndCap = LineCap.Round
            Using font As New Font("arial", Math.Max(7, Me.Font.Size - 1), FontStyle.Bold)  'set font size of number here
                Using brush As New SolidBrush(color)
                    Dim labelOffset As Single = majorTickLength + (ringWidth / 2.0F) + (font.Height / 3.0F) + 4
                    Dim steps As Integer = CInt(Math.Floor((maxVal - minVal) / majorStep))
                    For i As Integer = 0 To steps
                        Dim v As Single = minVal + i * majorStep
                        Dim angleDeg As Double = ValueToAngleDeg(v, minVal, maxVal, startAngle, sweepAngle)
                        Dim angleRad As Double = angleDeg * Math.PI / 180.0
                        Dim innerX As Single = CSng(cx + radius * Math.Cos(angleRad))
                        Dim innerY As Single = CSng(cy + radius * Math.Sin(angleRad))
                        Dim outerX As Single = CSng(cx + (radius + majorTickLength) * Math.Cos(angleRad))
                        Dim outerY As Single = CSng(cy + (radius + majorTickLength) * Math.Sin(angleRad))
                        g.DrawLine(penTick, innerX, innerY, outerX, outerY)
                        Dim labelX As Single = CSng(cx + (radius + labelOffset) * Math.Cos(angleRad))
                        Dim labelY As Single = CSng(cy + (radius + labelOffset) * Math.Sin(angleRad))
                        Using sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Center
                            ' Text shadow for depth
                            Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                                g.DrawString(v.ToString(), font, shadowBrush, New PointF(labelX + 1, labelY + 1), sf)
                            End Using
                            g.DrawString(v.ToString(), font, brush, New PointF(labelX, labelY), sf)
                        End Using
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawPointer(g As Graphics, cx As Single, cy As Single, radius As Single, value As Single, minVal As Single, maxVal As Single, color As Color, ringWidth As Single, startAngle As Double, sweepAngle As Double)
        Dim v As Single = CSng(Math.Max(minVal, Math.Min(maxVal, value)))
        Dim angleDeg As Double = ValueToAngleDeg(v, minVal, maxVal, startAngle, sweepAngle)
        Dim angleRad As Double = angleDeg * Math.PI / 180.0
        Dim pointerLength As Single = radius - ringWidth - 6
        Dim px As Single = CSng(cx + pointerLength * Math.Cos(angleRad))
        Dim py As Single = CSng(cy + pointerLength * Math.Sin(angleRad))

        ' Draw needle shadow for depth
        Using shadowPen As New Pen(Color.FromArgb(50, 0, 0, 0), Math.Max(1.5F, ringWidth * 0.6F))
            shadowPen.EndCap = LineCap.Round
            shadowPen.StartCap = LineCap.Round
            Dim shadowOffset As Single = 2.0F
            g.DrawLine(shadowPen, cx + shadowOffset, cy + shadowOffset, px + shadowOffset, py + shadowOffset)
        End Using

        ' Draw main needle
        Using pen As New Pen(color, Math.Max(1.5F, ringWidth * 0.6F))
            pen.EndCap = LineCap.Round
            pen.StartCap = LineCap.Round
            g.DrawLine(pen, cx, cy, px, py)
        End Using

        ' Draw enhanced center hub with gradient
        Dim hubSize As Single = Math.Max(5, ringWidth * 1.4F)
        Using hubPath As New GraphicsPath()
            hubPath.AddEllipse(cx - hubSize / 2.0F, cy - hubSize / 2.0F, hubSize, hubSize)
            Using hubGrad As New PathGradientBrush(hubPath)
                hubGrad.CenterColor = Color.FromArgb(220, color)
                hubGrad.SurroundColors = New Color() {Color.FromArgb(160, color)}
                g.FillEllipse(hubGrad, cx - hubSize / 2.0F, cy - hubSize / 2.0F, hubSize, hubSize)
            End Using
        End Using

        ' Add subtle highlight on hub
        Using highlightBrush As New SolidBrush(Color.FromArgb(60, 255, 255, 255))
            Dim highlightSize As Single = hubSize * 0.4F
            g.FillEllipse(highlightBrush, cx - hubSize / 4.0F, cy - hubSize / 3.0F, highlightSize, highlightSize)
        End Using
    End Sub

    Private Shared Sub DrawPointOnArc(g As Graphics, cx As Single, cy As Single, radius As Single, ringWidth As Single, value As Single, minVal As Single, maxVal As Single, color As Color, startAngle As Double, sweepAngle As Double)
        Dim v As Single = CSng(Math.Max(minVal, Math.Min(maxVal, value)))
        Dim angleDeg As Double = ValueToAngleDeg(v, minVal, maxVal, startAngle, sweepAngle)
        Dim angleRad As Double = angleDeg * Math.PI / 180.0
        Dim pxD As Double = cx + radius * Math.Cos(angleRad)
        Dim pyD As Double = cy + radius * Math.Sin(angleRad)
        Dim px As Single = CSng(pxD)
        Dim py As Single = CSng(pyD)
        Dim pointSize As Single = Math.Max(4, ringWidth * 0.9F)

        ' Draw glow effect
        Using glowBrush As New PathGradientBrush(New PointF() {
            New PointF(px - pointSize * 1.5F, py - pointSize * 1.5F),
            New PointF(px + pointSize * 1.5F, py - pointSize * 1.5F),
            New PointF(px + pointSize * 1.5F, py + pointSize * 1.5F),
            New PointF(px - pointSize * 1.5F, py + pointSize * 1.5F)
        })
            glowBrush.CenterPoint = New PointF(px, py)
            glowBrush.CenterColor = Color.FromArgb(100, color)
            glowBrush.SurroundColors = New Color() {Color.FromArgb(0, color), Color.FromArgb(0, color), Color.FromArgb(0, color), Color.FromArgb(0, color)}
            g.FillEllipse(glowBrush, New RectangleF(px - pointSize * 1.5F, py - pointSize * 1.5F, pointSize * 3.0F, pointSize * 3.0F))
        End Using

        ' Draw main point
        Using brush As New SolidBrush(color)
            g.FillEllipse(brush, New RectangleF(px - pointSize / 2.0F, py - pointSize / 2.0F, pointSize, pointSize))
        End Using

        ' Draw highlight
        Using highlightBrush As New SolidBrush(Color.FromArgb(150, 255, 255, 255))
            Dim highlightSize As Single = pointSize * 0.4F
            g.FillEllipse(highlightBrush, New RectangleF(px - pointSize / 4.0F, py - pointSize / 3.0F, highlightSize, highlightSize))
        End Using

        ' Draw subtle outline
        Using pen As New Pen(Color.FromArgb(80, color), 1)
            g.DrawEllipse(pen, New RectangleF(px - pointSize / 2.0F - 1.0F, py - pointSize / 2.0F - 1.0F, pointSize + 2.0F, pointSize + 2.0F))
        End Using
    End Sub

    Private Sub DrawCenterText(g As Graphics, cx As Single, cy As Single, f As Single, c As Single)
        Using fmt As New StringFormat()
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Center
            Using fontLabel As New Font("Segoe UI", Math.Max(7, Me.Font.Size), FontStyle.Bold),
                  fontF As New Font("Segoe UI", Math.Max(10, Me.Font.Size + 6), FontStyle.Bold),
                  fontC As New Font("Segoe UI", Math.Max(8, Me.Font.Size + 2), FontStyle.Regular),
                  brushLabel As New SolidBrush(Color.FromArgb(100, 100, 100)),
                  brushF As New SolidBrush(Color.FromArgb(30, 105, 210)),
                  brushC As New SolidBrush(Color.FromArgb(180, 60, 60))

                Dim labelHeight As Single = If(String.IsNullOrEmpty(_label), 0, fontLabel.Height)
                Dim labelOffset As Single = If(String.IsNullOrEmpty(_label), 0, labelHeight * 0.8F)

                ' Draw label above the temperature if provided
                If Not String.IsNullOrEmpty(_label) Then
                    ' Outer shadow
                    Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
                        g.DrawString(_label, fontLabel, shadowBrush, New PointF(cx + 2, cy - (fontC.Height / 1.5F) - labelOffset), fmt)
                    End Using
                    ' Inner shadow
                    Using shadowBrush2 As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
                        g.DrawString(_label, fontLabel, shadowBrush2, New PointF(cx + 1, cy - (fontC.Height / 1.5F) - labelOffset - 1), fmt)
                    End Using
                    g.DrawString(_label, fontLabel, brushLabel, New PointF(cx, cy - (fontC.Height / 1.5F) - labelOffset - 2), fmt)
                End If

                ' Temperature F with layered shadows
                Using shadowBrush As New SolidBrush(Color.FromArgb(50, 0, 0, 0))
                    g.DrawString(String.Format("{0:0.#}°F", f), fontF, shadowBrush, New PointF(cx + 2, cy - (fontC.Height / 1.5F) + 1), fmt)
                End Using
                Using shadowBrush2 As New SolidBrush(Color.FromArgb(25, 0, 0, 0))
                    g.DrawString(String.Format("{0:0.#}°F", f), fontF, shadowBrush2, New PointF(cx + 1, cy - (fontC.Height / 1.5F)), fmt)
                End Using
                g.DrawString(String.Format("{0:0.#}°F", f), fontF, brushF, New PointF(cx, cy - (fontC.Height / 1.5F)), fmt)

                ' Temperature C with shadow
                Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                    g.DrawString(String.Format("{0:0.#}°C", c), fontC, shadowBrush, New PointF(cx + 1, cy + (fontF.Height / 2.0F) + 1), fmt)
                End Using
                g.DrawString(String.Format("{0:0.#}°C", c), fontC, brushC, New PointF(cx, cy + (fontF.Height / 2.0F)), fmt)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class