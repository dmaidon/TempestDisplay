Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Professional Wind Rose Control - Displays wind direction in a circular compass rose
''' Shows cardinal directions (N, S, E, W) and intercardinal directions (NE, SE, SW, NW)
''' </summary>
<DefaultEvent("Click")>
Public Class WindRoseControl
    Inherits Control

    Private _windDirection As Single = 0 ' Degrees (0 = North, 90 = East, 180 = South, 270 = West)
    Private _windSpeed As Single = 0 ' Wind speed for arrow color coding
    Private _showSpeed As Boolean = True
    Private _label As String = "Wind"

    ' Color thresholds for wind speed (mph)
    Private Const LightWindThreshold As Single = 5.0F

    Private Const ModerateWindThreshold As Single = 15.0F
    Private Const StrongWindThreshold As Single = 25.0F

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(0.0F)>
    <Description("Wind direction in degrees (0 = North, 90 = East, 180 = South, 270 = West)")>
    Public Property WindDirection As Single
        Get
            Return _windDirection
        End Get
        Set(val As Single)
            ' Normalize to 0-360 range
            _windDirection = ((val Mod 360.0F) + 360.0F) Mod 360.0F
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(0.0F)>
    <Description("Wind speed (used for color coding)")>
    Public Property WindSpeed As Single
        Get
            Return _windSpeed
        End Get
        Set(val As Single)
            _windSpeed = Math.Max(0, val)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    <Description("Show wind speed in the center")>
    Public Property ShowSpeed As Boolean
        Get
            Return _showSpeed
        End Get
        Set(val As Boolean)
            _showSpeed = val
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue("Wind")>
    <Description("Label text displayed at the bottom")>
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
        Me.MinimumSize = New Size(150, 150)
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
        Dim radius As Single = (size / 2.0F) - 20

        ' Draw professional background
        DrawProfessionalBackground(g, cx, cy, radius)

        ' Draw outer bezel
        DrawOuterBezel(g, cx, cy, radius + 10)

        ' Draw compass rose markings
        DrawCompassRose(g, cx, cy, radius)

        ' Draw cardinal direction labels (N, S, E, W)
        DrawCardinalLabels(g, cx, cy, radius)

        ' Draw intercardinal direction labels (NE, SE, SW, NW)
        DrawIntercardinalLabels(g, cx, cy, radius)

        ' Draw wind direction arrow
        DrawWindArrow(g, cx, cy, radius * 0.7F)

        ' Draw center content (speed and label)
        DrawCenterContent(g, cx, cy)

        ' Glass overlay for final professional touch
        DrawGlassOverlay(g, cx, cy, radius)
    End Sub

    Private Shared Sub DrawProfessionalBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer shadow for depth
        Using shadowPath As New GraphicsPath()
            shadowPath.AddEllipse(cx - radius - 5, cy - radius - 5, (radius + 5) * 2, (radius + 5) * 2)
            Using shadowBrush As New PathGradientBrush(shadowPath)
                shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0)
                shadowBrush.SurroundColors = New Color() {Color.FromArgb(0, 0, 0, 0)}
                g.FillPath(shadowBrush, shadowPath)
            End Using
        End Using

        ' Main background with radial gradient (brushed metal effect)
        Using bgPath As New GraphicsPath()
            bgPath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using bgBrush As New PathGradientBrush(bgPath)
                bgBrush.CenterColor = Color.FromArgb(245, 245, 250)
                bgBrush.SurroundColors = New Color() {Color.FromArgb(200, 205, 215)}
                g.FillPath(bgBrush, bgPath)
            End Using
        End Using

        ' Inner shadow for depth
        Using innerShadowPath As New GraphicsPath()
            innerShadowPath.AddEllipse(cx - (radius * 0.95F), cy - (radius * 0.95F), radius * 1.9F, radius * 1.9F)
            Using innerShadow As New PathGradientBrush(innerShadowPath)
                innerShadow.CenterColor = Color.FromArgb(0, 0, 0, 0)
                innerShadow.SurroundColors = New Color() {Color.FromArgb(30, 0, 0, 0)}
                g.FillPath(innerShadow, innerShadowPath)
            End Using
        End Using

        ' Glossy highlight overlay
        Using glossPath As New GraphicsPath()
            glossPath.AddEllipse(cx - (radius * 0.8F), cy - (radius * 0.8F), radius * 1.6F, radius * 1.6F)
            Using glossBrush As New PathGradientBrush(glossPath)
                glossBrush.CenterColor = Color.FromArgb(0, 255, 255, 255)
                glossBrush.SurroundColors = New Color() {Color.FromArgb(60, 255, 255, 255)}
                glossBrush.FocusScales = New PointF(0.3F, 0.3F)
                g.FillPath(glossBrush, glossPath)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawOuterBezel(g As Graphics, cx As Single, cy As Single, radius As Single)
        Dim bezelWidth As Single = 4.0F

        ' Outer dark ring
        Using outerPen As New Pen(Color.FromArgb(80, 80, 85), bezelWidth)
            g.DrawEllipse(outerPen, cx - radius, cy - radius, radius * 2, radius * 2)
        End Using

        ' Middle metallic ring with gradient
        Using middlePath As New GraphicsPath()
            middlePath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using middleBrush As New PathGradientBrush(middlePath)
                middleBrush.CenterColor = Color.FromArgb(140, 145, 150)
                middleBrush.SurroundColors = New Color() {Color.FromArgb(90, 95, 100)}
                Using middlePen As New Pen(middleBrush, bezelWidth - 1)
                    g.DrawEllipse(middlePen, cx - radius, cy - radius, radius * 2, radius * 2)
                End Using
            End Using
        End Using

        ' Inner highlight ring
        Dim innerRadius As Single = radius - (bezelWidth * 1.2F)
        Using innerPen As New Pen(Color.FromArgb(120, 180, 185, 190), 1.5F)
            g.DrawEllipse(innerPen, cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2)
        End Using
    End Sub

    Private Shared Sub DrawCompassRose(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Draw degree markings (every 10 degrees)
        For degrees As Integer = 0 To 350 Step 10
            Dim angleRad As Double = (degrees - 90) * Math.PI / 180.0 ' -90 to start at North (top)
            Dim isCardinal As Boolean = (degrees Mod 90 = 0)
            Dim isMajor As Boolean = (degrees Mod 30 = 0)

            Dim tickLength As Single
            Dim tickWidth As Single
            Dim tickColor As Color

            If isCardinal Then
                tickLength = radius * 0.15F
                tickWidth = 3.0F
                tickColor = Color.FromArgb(60, 60, 60)
            ElseIf isMajor Then
                tickLength = radius * 0.1F
                tickWidth = 2.0F
                tickColor = Color.FromArgb(90, 90, 90)
            Else
                tickLength = radius * 0.06F
                tickWidth = 1.5F
                tickColor = Color.FromArgb(120, 120, 120)
            End If

            Dim innerX As Single = CSng(cx + ((radius - tickLength) * Math.Cos(angleRad)))
            Dim innerY As Single = CSng(cy + ((radius - tickLength) * Math.Sin(angleRad)))
            Dim outerX As Single = CSng(cx + (radius * Math.Cos(angleRad)))
            Dim outerY As Single = CSng(cy + (radius * Math.Sin(angleRad)))

            Using tickPen As New Pen(tickColor, tickWidth)
                tickPen.StartCap = LineCap.Round
                tickPen.EndCap = LineCap.Round
                g.DrawLine(tickPen, innerX, innerY, outerX, outerY)
            End Using
        Next
    End Sub

    Private Sub DrawCardinalLabels(g As Graphics, cx As Single, cy As Single, radius As Single)
        Dim labels() As String = {"N", "E", "S", "W"}
        Dim angles() As Integer = {0, 90, 180, 270}
        Dim labelRadius As Single = radius * 0.75F

        Using font As New Font("Arial", Math.Max(12, Me.Font.Size + 4), FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(40, 40, 40))
                Using sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Center

                    For i As Integer = 0 To labels.Length - 1
                        Dim angleRad As Double = (angles(i) - 90) * Math.PI / 180.0
                        Dim labelX As Single = CSng(cx + (labelRadius * Math.Cos(angleRad)))
                        Dim labelY As Single = CSng(cy + (labelRadius * Math.Sin(angleRad)))

                        ' Draw shadow
                        Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
                            g.DrawString(labels(i), font, shadowBrush, New PointF(labelX + 1, labelY + 1), sf)
                        End Using

                        ' Draw label with special highlight for North
                        If labels(i) = "N" Then
                            Using northBrush As New SolidBrush(Color.FromArgb(200, 40, 40))
                                g.DrawString(labels(i), font, northBrush, New PointF(labelX, labelY), sf)
                            End Using
                        Else
                            g.DrawString(labels(i), font, brush, New PointF(labelX, labelY), sf)
                        End If
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawIntercardinalLabels(g As Graphics, cx As Single, cy As Single, radius As Single)
        Dim labels() As String = {"NE", "SE", "SW", "NW"}
        Dim angles() As Integer = {45, 135, 225, 315}
        Dim labelRadius As Single = radius * 0.75F

        Using font As New Font("Arial", Math.Max(8, Me.Font.Size), FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Center

                    For i As Integer = 0 To labels.Length - 1
                        Dim angleRad As Double = (angles(i) - 90) * Math.PI / 180.0
                        Dim labelX As Single = CSng(cx + (labelRadius * Math.Cos(angleRad)))
                        Dim labelY As Single = CSng(cy + (labelRadius * Math.Sin(angleRad)))

                        ' Draw shadow
                        Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                            g.DrawString(labels(i), font, shadowBrush, New PointF(labelX + 1, labelY + 1), sf)
                        End Using

                        g.DrawString(labels(i), font, brush, New PointF(labelX, labelY), sf)
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawWindArrow(g As Graphics, cx As Single, cy As Single, length As Single)
        ' Convert wind direction to radians (subtract 90 to start from North)
        Dim angleRad As Double = (_windDirection - 90) * Math.PI / 180.0

        ' Calculate arrow endpoint
        Dim px As Single = CSng(cx + (length * Math.Cos(angleRad)))
        Dim py As Single = CSng(cy + (length * Math.Sin(angleRad)))

        ' Get color based on wind speed
        Dim arrowColor As Color = GetWindSpeedColor()

        ' Draw arrow shadow
        Using shadowPen As New Pen(Color.FromArgb(80, 0, 0, 0), 5.0F)
            shadowPen.StartCap = LineCap.Round
            shadowPen.EndCap = LineCap.Round
            g.DrawLine(shadowPen, cx + 2, cy + 2, px + 2, py + 2)
        End Using

        ' Draw main arrow shaft
        Using arrowPen As New Pen(arrowColor, 4.0F)
            arrowPen.StartCap = LineCap.Round
            arrowPen.EndCap = LineCap.Round
            g.DrawLine(arrowPen, cx, cy, px, py)
        End Using

        ' Draw arrowhead
        DrawArrowhead(g, px, py, angleRad, arrowColor)

        ' Draw center hub
        DrawCenterHub(g, cx, cy, arrowColor)
    End Sub

    Private Shared Sub DrawArrowhead(g As Graphics, px As Single, py As Single, angleRad As Double, arrowColor As Color)
        Dim arrowSize As Single = 15.0F
        Dim arrowAngle As Double = 25 * Math.PI / 180.0 ' 25 degree arrow wings

        ' Calculate arrowhead points
        Dim leftAngle As Double = angleRad + Math.PI - arrowAngle
        Dim rightAngle As Double = angleRad + Math.PI + arrowAngle

        Dim leftX As Single = CSng(px + (arrowSize * Math.Cos(leftAngle)))
        Dim leftY As Single = CSng(py + (arrowSize * Math.Sin(leftAngle)))
        Dim rightX As Single = CSng(px + (arrowSize * Math.Cos(rightAngle)))
        Dim rightY As Single = CSng(py + (arrowSize * Math.Sin(rightAngle)))

        Dim arrowPoints() As PointF = {
            New PointF(px, py),
            New PointF(leftX, leftY),
            New PointF(rightX, rightY)
        }

        ' Draw arrowhead shadow
        Dim shadowPoints() As PointF = {
            New PointF(px + 2, py + 2),
            New PointF(leftX + 2, leftY + 2),
            New PointF(rightX + 2, rightY + 2)
        }

        Using shadowBrush As New SolidBrush(Color.FromArgb(80, 0, 0, 0))
            g.FillPolygon(shadowBrush, shadowPoints)
        End Using

        ' Draw arrowhead with gradient
        Using arrowPath As New GraphicsPath()
            arrowPath.AddPolygon(arrowPoints)
            Using arrowBrush As New LinearGradientBrush(
                New PointF(px, py),
                New PointF((leftX + rightX) / 2, (leftY + rightY) / 2),
                Color.FromArgb(Math.Min(255, arrowColor.R + 30), Math.Min(255, arrowColor.G + 30), Math.Min(255, arrowColor.B + 30)),
                arrowColor)
                g.FillPath(arrowBrush, arrowPath)
            End Using
        End Using

        ' Draw arrowhead outline
        Using outlinePen As New Pen(Color.FromArgb(30, 30, 30), 1.5F)
            outlinePen.LineJoin = LineJoin.Round
            g.DrawPolygon(outlinePen, arrowPoints)
        End Using
    End Sub

    Private Shared Sub DrawCenterHub(g As Graphics, cx As Single, cy As Single, arrowColor As Color)
        Dim hubSize As Single = 16.0F

        ' Draw hub shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(100, 0, 0, 0))
            g.FillEllipse(shadowBrush, cx - (hubSize / 2.0F) + 2, cy - (hubSize / 2.0F) + 2, hubSize, hubSize)
        End Using

        ' Draw hub with gradient
        Using hubPath As New GraphicsPath()
            hubPath.AddEllipse(cx - (hubSize / 2.0F), cy - (hubSize / 2.0F), hubSize, hubSize)
            Using hubBrush As New PathGradientBrush(hubPath)
                hubBrush.CenterColor = Color.FromArgb(Math.Min(255, arrowColor.R + 40), Math.Min(255, arrowColor.G + 40), Math.Min(255, arrowColor.B + 40))
                hubBrush.SurroundColors = New Color() {arrowColor}
                g.FillPath(hubBrush, hubPath)
            End Using
        End Using

        ' Draw hub outline
        Using outlinePen As New Pen(Color.FromArgb(40, 40, 40), 2.0F)
            g.DrawEllipse(outlinePen, cx - (hubSize / 2.0F), cy - (hubSize / 2.0F), hubSize, hubSize)
        End Using

        ' Add highlight
        Using highlightBrush As New SolidBrush(Color.FromArgb(100, 255, 255, 255))
            Dim highlightSize As Single = hubSize * 0.3F
            g.FillEllipse(highlightBrush, cx - (hubSize / 3.5F), cy - (hubSize / 3.0F), highlightSize, highlightSize)
        End Using
    End Sub

    Private Function GetWindSpeedColor() As Color
        If _windSpeed < LightWindThreshold Then
            Return Color.FromArgb(100, 100, 100) ' Gray - calm
        ElseIf _windSpeed < ModerateWindThreshold Then
            Return Color.FromArgb(40, 160, 220) ' Blue - light wind
        ElseIf _windSpeed < StrongWindThreshold Then
            Return Color.FromArgb(220, 180, 0) ' Gold - moderate wind
        Else
            Return Color.FromArgb(220, 60, 0) ' Red - strong wind
        End If
    End Function

    Private Sub DrawCenterContent(g As Graphics, cx As Single, cy As Single)
        Using fmt As New StringFormat()
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Center

            ' Draw wind speed if enabled
            If _showSpeed Then
                Using fontSpeed As New Font("Arial", Math.Max(14, Me.Font.Size + 6), FontStyle.Bold)
                    Using brushSpeed As New SolidBrush(Color.FromArgb(40, 40, 40))
                        ' Shadow
                        Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
                            g.DrawString(_windSpeed.ToString("F1"), fontSpeed, shadowBrush, New PointF(cx + 1, cy - 34), fmt)
                        End Using
                        g.DrawString(_windSpeed.ToString("F1"), fontSpeed, brushSpeed, New PointF(cx, cy - 35), fmt)
                    End Using
                End Using

                ' Draw "mph" label
                Using fontUnit As New Font("Arial", Math.Max(7, Me.Font.Size - 1), FontStyle.Regular)
                    Using brushUnit As New SolidBrush(Color.FromArgb(100, 100, 100))
                        g.DrawString("mph", fontUnit, brushUnit, New PointF(cx, cy - 17), fmt)
                    End Using
                End Using
            End If

            ' Draw direction in degrees below center
            Using fontDir As New Font("Arial", Math.Max(8, Me.Font.Size), FontStyle.Regular)
                Using brushDir As New SolidBrush(Color.FromArgb(80, 80, 80))
                    Dim dirText As String = $"{CInt(_windDirection)}°"
                    g.DrawString(dirText, fontDir, brushDir, New PointF(cx, cy + 35), fmt)
                End Using
            End Using

            ' Draw label at bottom
            If Not String.IsNullOrEmpty(_label) Then
                Using fontLabel As New Font("Arial", Math.Max(8, Me.Font.Size), FontStyle.Bold)
                    Using brushLabel As New SolidBrush(Color.FromArgb(100, 100, 100))
                        ' Shadow
                        Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                            g.DrawString(_label, fontLabel, shadowBrush, New PointF(cx + 1, cy + 53), fmt)
                        End Using
                        g.DrawString(_label, fontLabel, brushLabel, New PointF(cx, cy + 52), fmt)
                    End Using
                End Using
            End If
        End Using
    End Sub

    Private Shared Sub DrawGlassOverlay(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Semi-transparent glass effect
        Using glassPath As New GraphicsPath()
            glassPath.AddEllipse(cx - (radius * 0.85F), cy - (radius * 0.85F), radius * 1.7F, radius * 1.7F)
            Using glassBrush As New PathGradientBrush(glassPath)
                glassBrush.CenterColor = Color.FromArgb(0, 255, 255, 255)
                glassBrush.SurroundColors = New Color() {Color.FromArgb(30, 255, 255, 255)}
                glassBrush.FocusScales = New PointF(0.5F, 0.3F)
                g.FillPath(glassBrush, glassPath)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class