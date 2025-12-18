Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Enhanced compass rose control for displaying wind direction with 16-point compass,
''' history trail, and speed indicators.
''' </summary>
<DefaultEvent("Click")>
Public Class CompassRoseControl
    Inherits Control

    Private _windDirection As Single = 0
    Private _windSpeed As Single = 0
    Private _gustSpeed As Single = 0
    Private ReadOnly _directionHistory As New Queue(Of Single)
    Private _showHistory As Boolean = True
    Private _historyLength As Integer = 10
    Private _show16Point As Boolean = True

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WindDirection As Single
        Get
            Return _windDirection
        End Get
        Set(value As Single)
            _windDirection = value Mod 360.0F
            If _windDirection < 0 Then _windDirection += 360.0F

            ' Add to history
            If _showHistory Then
                _directionHistory.Enqueue(_windDirection)
                If _directionHistory.Count > _historyLength Then
                    _directionHistory.Dequeue()
                End If
            End If

            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WindSpeed As Single
        Get
            Return _windSpeed
        End Get
        Set(value As Single)
            _windSpeed = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property GustSpeed As Single
        Get
            Return _gustSpeed
        End Get
        Set(value As Single)
            _gustSpeed = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    <Description("Show wind direction history trail")>
    Public Property ShowHistory As Boolean
        Get
            Return _showHistory
        End Get
        Set(value As Boolean)
            _showHistory = value
            If Not value Then _directionHistory.Clear()
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(10)>
    <Description("Number of history points to display")>
    Public Property HistoryLength As Integer
        Get
            Return _historyLength
        End Get
        Set(value As Integer)
            _historyLength = Math.Max(1, Math.Min(50, value))
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    <Description("Show 16-point compass (vs 8-point)")>
    Public Property Show16Point As Boolean
        Get
            Return _show16Point
        End Get
        Set(value As Boolean)
            _show16Point = value
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

        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height
        Dim size As Single = Math.Min(w, h)
        Dim cx As Single = w / 2.0F
        Dim cy As Single = h / 2.0F
        Dim radius As Single = size / 2.0F - 40

        ' Draw components
        DrawCompassBackground(g, cx, cy, radius)
        DrawCompassRings(g, cx, cy, radius)
        DrawDirectionMarks(g, cx, cy, radius)
        DrawWindHistory(g, cx, cy, radius)
        DrawWindArrow(g, cx, cy, radius, _windDirection, _windSpeed)
        DrawSpeedReadout(g, cx, cy + radius + 20, _windSpeed, _gustSpeed)
    End Sub

    Private Shared Sub DrawCompassBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer shadow
        Using shadowPath As New GraphicsPath()
            shadowPath.AddEllipse(cx - radius - 3, cy - radius - 3, (radius + 3) * 2, (radius + 3) * 2)
            Using shadowBrush As New PathGradientBrush(shadowPath)
                shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0)
                shadowBrush.SurroundColors = {Color.FromArgb(0, 0, 0, 0)}
                g.FillEllipse(shadowBrush, cx - radius - 3, cy - radius - 3, (radius + 3) * 2, (radius + 3) * 2)
            End Using
        End Using

        ' Background with gradient
        Using bgPath As New GraphicsPath()
            bgPath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using bgBrush As New PathGradientBrush(bgPath)
                bgBrush.CenterColor = Color.FromArgb(245, 250, 255)
                bgBrush.SurroundColors = {Color.FromArgb(215, 225, 235)}
                g.FillEllipse(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawCompassRings(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer ring
        Using ringPen As New Pen(Color.FromArgb(100, 100, 105), 2.5F)
            g.DrawEllipse(ringPen, cx - radius, cy - radius, radius * 2, radius * 2)
        End Using

        ' Speed zone rings (calm center)
        Dim rings() As Single = {0.25F, 0.5F, 0.75F}
        Using ringPen As New Pen(Color.FromArgb(60, 200, 220, 230), 1.0F)
            ringPen.DashStyle = DashStyle.Dot
            For Each ringFactor In rings
                Dim ringRadius As Single = radius * ringFactor
                g.DrawEllipse(ringPen, cx - ringRadius, cy - ringRadius, ringRadius * 2, ringRadius * 2)
            Next
        End Using
    End Sub

    Private Sub DrawDirectionMarks(g As Graphics, cx As Single, cy As Single, radius As Single)
        Dim cardinalDirs As New Dictionary(Of Single, String) From {
            {0, "N"}, {90, "E"}, {180, "S"}, {270, "W"}
        }

        Dim intercardinalDirs As New Dictionary(Of Single, String) From {
            {45, "NE"}, {135, "SE"}, {225, "SW"}, {315, "NW"}
        }

        Dim secondaryDirs As New Dictionary(Of Single, String) From {
            {22.5F, "NNE"}, {67.5F, "ENE"}, {112.5F, "ESE"}, {157.5F, "SSE"},
            {202.5F, "SSW"}, {247.5F, "WSW"}, {292.5F, "WNW"}, {337.5F, "NNW"}
        }

        Using cardinalFont As New Font("Arial", 12.0F, FontStyle.Bold)
            Using interFont As New Font("Arial", 9.0F, FontStyle.Bold)
                Using secondFont As New Font("Arial", 7.0F, FontStyle.Regular)
                    Using cardinalBrush As New SolidBrush(Color.FromArgb(30, 30, 150))
                        Using interBrush As New SolidBrush(Color.FromArgb(60, 60, 120))
                            Using secondBrush As New SolidBrush(Color.FromArgb(100, 100, 110))
                                Using fmt As New StringFormat()
                                    fmt.Alignment = StringAlignment.Center
                                    fmt.LineAlignment = StringAlignment.Center

                                    ' Cardinal directions
                                    For Each dir As KeyValuePair(Of Single, String) In cardinalDirs
                                        DrawDirectionLabel(g, cx, cy, radius, dir.Key, dir.Value, cardinalFont, cardinalBrush, fmt, 0.88F)
                                        DrawDirectionTick(g, cx, cy, radius, dir.Key, 15, 3.0F, Color.FromArgb(30, 30, 150))
                                    Next

                                    ' Intercardinal directions
                                    For Each dir As KeyValuePair(Of Single, String) In intercardinalDirs
                                        DrawDirectionLabel(g, cx, cy, radius, dir.Key, dir.Value, interFont, interBrush, fmt, 0.85F)
                                        DrawDirectionTick(g, cx, cy, radius, dir.Key, 12, 2.5F, Color.FromArgb(60, 60, 120))
                                    Next

                                    ' Secondary directions (16-point only)
                                    If _show16Point Then
                                        For Each dir As KeyValuePair(Of Single, String) In secondaryDirs
                                            DrawDirectionLabel(g, cx, cy, radius, dir.Key, dir.Value, secondFont, secondBrush, fmt, 0.82F)
                                            DrawDirectionTick(g, cx, cy, radius, dir.Key, 8, 1.5F, Color.FromArgb(100, 100, 110))
                                        Next
                                    End If

                                    ' Degree ticks every 10°
                                    For degree As Integer = 0 To 350 Step 10
                                        If Not cardinalDirs.ContainsKey(CSng(degree)) AndAlso Not intercardinalDirs.ContainsKey(CSng(degree)) AndAlso Not secondaryDirs.ContainsKey(CSng(degree)) Then
                                            DrawDirectionTick(g, cx, cy, radius, degree, 5, 1.0F, Color.FromArgb(150, 150, 150))
                                        End If
                                    Next
                                End Using
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawDirectionLabel(g As Graphics, cx As Single, cy As Single, radius As Single, degrees As Single, label As String, font As Font, brush As Brush, fmt As StringFormat, radiusFactor As Single)
        Dim angleRad As Double = (degrees - 90) * Math.PI / 180.0
        Dim labelRadius As Single = radius * radiusFactor
        Dim x As Single = CSng(cx + labelRadius * Math.Cos(angleRad))
        Dim y As Single = CSng(cy + labelRadius * Math.Sin(angleRad))

        ' Shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
            g.DrawString(label, font, shadowBrush, x + 1, y + 1, fmt)
        End Using

        g.DrawString(label, font, brush, x, y, fmt)
    End Sub

    Private Shared Sub DrawDirectionTick(g As Graphics, cx As Single, cy As Single, radius As Single, degrees As Single, length As Integer, width As Single, color As Color)
        Dim angleRad As Double = (degrees - 90) * Math.PI / 180.0
        Dim innerRadius As Single = radius * 0.92F
        Dim outerRadius As Single = innerRadius + length

        Dim innerX As Single = CSng(cx + innerRadius * Math.Cos(angleRad))
        Dim innerY As Single = CSng(cy + innerRadius * Math.Sin(angleRad))
        Dim outerX As Single = CSng(cx + outerRadius * Math.Cos(angleRad))
        Dim outerY As Single = CSng(cy + outerRadius * Math.Sin(angleRad))

        Using pen As New Pen(color, width)
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            g.DrawLine(pen, innerX, innerY, outerX, outerY)
        End Using
    End Sub

    Private Sub DrawWindHistory(g As Graphics, cx As Single, cy As Single, radius As Single)
        If Not _showHistory OrElse _directionHistory.Count < 2 Then Return

        Dim historyArray = _directionHistory.ToArray()
        Dim trailRadius As Single = radius * 0.7F

        For i As Integer = 0 To historyArray.Length - 1
            Dim direction As Single = historyArray(i)
            Dim angleRad As Double = (direction - 90) * Math.PI / 180.0
            Dim x As Single = CSng(cx + trailRadius * Math.Cos(angleRad))
            Dim y As Single = CSng(cy + trailRadius * Math.Sin(angleRad))

            ' Fade older points
            Dim alpha As Integer = CInt(50 + (i / historyArray.Length) * 150)
            Dim dotSize As Single = 3.0F + (i / historyArray.Length) * 3.0F

            Using trailBrush As New SolidBrush(Color.FromArgb(alpha, 100, 150, 200))
                g.FillEllipse(trailBrush, x - dotSize / 2, y - dotSize / 2, dotSize, dotSize)
            End Using
        Next
    End Sub

    Private Shared Sub DrawWindArrow(g As Graphics, cx As Single, cy As Single, radius As Single, direction As Single, speed As Single)
        Dim arrowColor As Color = GetSpeedColor(speed)
        Dim angleRad As Double = (direction - 90) * Math.PI / 180.0
        Dim arrowLength As Single = radius * 0.65F

        ' Arrow tip
        Dim tipX As Single = CSng(cx + arrowLength * Math.Cos(angleRad))
        Dim tipY As Single = CSng(cy + arrowLength * Math.Sin(angleRad))

        ' Arrow base
        Dim baseLength As Single = arrowLength * 0.3F
        Dim baseX As Single = CSng(cx - baseLength * Math.Cos(angleRad))
        Dim baseY As Single = CSng(cy - baseLength * Math.Sin(angleRad))

        ' Arrow wings
        Dim wingAngle As Double = Math.PI / 6
        Dim wingLength As Single = arrowLength * 0.3F
        Dim wing1X As Single = CSng(tipX - wingLength * Math.Cos(angleRad + wingAngle))
        Dim wing1Y As Single = CSng(tipY - wingLength * Math.Sin(angleRad + wingAngle))
        Dim wing2X As Single = CSng(tipX - wingLength * Math.Cos(angleRad - wingAngle))
        Dim wing2Y As Single = CSng(tipY - wingLength * Math.Sin(angleRad - wingAngle))

        ' Draw shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
            Dim offset As Single = 2.0F
            g.FillPolygon(shadowBrush, {
                New PointF(tipX + offset, tipY + offset),
                New PointF(wing1X + offset, wing1Y + offset),
                New PointF(baseX + offset, baseY + offset),
                New PointF(wing2X + offset, wing2Y + offset)
            })
        End Using

        ' Draw arrow with gradient
        Using arrowPath As New GraphicsPath()
            arrowPath.AddPolygon({
                New PointF(tipX, tipY),
                New PointF(wing1X, wing1Y),
                New PointF(baseX, baseY),
                New PointF(wing2X, wing2Y)
            })

            Using arrowBrush As New LinearGradientBrush(
                New PointF(baseX, baseY),
                New PointF(tipX, tipY),
                Color.FromArgb(arrowColor.A, CInt(arrowColor.R * 0.7), CInt(arrowColor.G * 0.7), CInt(arrowColor.B * 0.7)),
                arrowColor)
                g.FillPath(arrowBrush, arrowPath)
            End Using
        End Using

        ' Arrow outline
        Using arrowPen As New Pen(Color.FromArgb(30, 30, 30), 1.5F)
            g.DrawPolygon(arrowPen, {
                New PointF(tipX, tipY),
                New PointF(wing1X, wing1Y),
                New PointF(baseX, baseY),
                New PointF(wing2X, wing2Y)
            })
        End Using
    End Sub

    Private Shared Function GetSpeedColor(speed As Single) As Color
        If speed < 5 Then
            Return Color.FromArgb(150, 150, 150) ' Calm - Gray
        ElseIf speed < 15 Then
            Return Color.FromArgb(100, 150, 220) ' Light - Blue
        ElseIf speed < 25 Then
            Return Color.FromArgb(220, 180, 50) ' Moderate - Gold
        Else
            Return Color.FromArgb(220, 50, 50) ' Strong - Red
        End If
    End Function

    Private Shared Sub DrawSpeedReadout(g As Graphics, cx As Single, cy As Single, speed As Single, gust As Single)
        Using speedFont As New Font("Segoe UI", 10.0F, FontStyle.Bold)
            Using gustFont As New Font("Segoe UI", 8.0F, FontStyle.Regular)
                Using brush As New SolidBrush(Color.FromArgb(50, 50, 50))
                    Using fmt As New StringFormat()
                        fmt.Alignment = StringAlignment.Center
                        fmt.LineAlignment = StringAlignment.Near

                        Dim speedText As String = speed.ToString("0.0") & " mph"
                        g.DrawString(speedText, speedFont, brush, cx, cy, fmt)

                        If gust > speed Then
                            Dim gustText As String = "Gust: " & gust.ToString("0.0")
                            g.DrawString(gustText, gustFont, brush, cx, cy + 15, fmt)
                        End If
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