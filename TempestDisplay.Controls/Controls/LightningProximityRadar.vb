Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Lightning proximity radar control displaying strike location and distance with concentric range rings.
''' Shows strike history with fading markers and approaching/receding indicators.
''' </summary>
<DefaultEvent("Click")>
Public Class LightningProximityRadar
    Inherits Control

    Private _lastStrikeDistance As Single = 0
    Private _lastStrikeTime As DateTime = DateTime.MinValue
    Private _strikeHistory As New List(Of Tuple(Of Single, DateTime))
    Private _maxDistance As Single = 30.0F
    Private _showRangeRings As Boolean = True

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property LastStrikeDistance As Single
        Get
            Return _lastStrikeDistance
        End Get
        Set(value As Single)
            _lastStrikeDistance = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property LastStrikeTime As DateTime
        Get
            Return _lastStrikeTime
        End Get
        Set(value As DateTime)
            _lastStrikeTime = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(30.0F)>
    Public Property MaxDistance As Single
        Get
            Return _maxDistance
        End Get
        Set(value As Single)
            _maxDistance = Math.Max(1.0F, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property ShowRangeRings As Boolean
        Get
            Return _showRangeRings
        End Get
        Set(value As Boolean)
            _showRangeRings = value
            Invalidate()
        End Set
    End Property

    Public Sub AddStrike(distanceMiles As Single, strikeTime As DateTime)
        _strikeHistory.Add(Tuple.Create(distanceMiles, strikeTime))
        _lastStrikeDistance = distanceMiles
        _lastStrikeTime = strikeTime

        ' Keep only recent strikes (last hour)
        Dim oneHourAgo As DateTime = DateTime.Now.AddHours(-1)
        _strikeHistory = _strikeHistory.Where(Function(s) s.Item2 > oneHourAgo).ToList()

        Invalidate()
    End Sub

    Public Sub ClearHistory()
        _strikeHistory.Clear()
        _lastStrikeDistance = 0
        _lastStrikeTime = DateTime.MinValue
        Invalidate()
    End Sub

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
        Dim radius As Single = size / 2.0F - 30.0F  '-35

        ' Draw components
        DrawRadarBackground(g, cx, cy, radius)
        DrawRangeRings(g, cx, cy, radius)
        DrawStrikeHistory(g, cx, cy, radius)
        DrawCenter(g, cx, cy)
        DrawDistanceLabels(g, cx, cy, radius)
        DrawStatusText(g, cx, cy + radius + 8) '15
    End Sub

    Private Shared Sub DrawRadarBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Outer shadow
        Using shadowPath As New GraphicsPath()
            shadowPath.AddEllipse(cx - radius - 3, cy - radius - 3, (radius + 3) * 2, (radius + 3) * 2)
            Using shadowBrush As New PathGradientBrush(shadowPath)
                shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0)
                shadowBrush.SurroundColors = {Color.FromArgb(0, 0, 0, 0)}
                g.FillEllipse(shadowBrush, cx - radius - 3, cy - radius - 3, (radius + 3) * 2, (radius + 3) * 2)
            End Using
        End Using

        ' Background gradient (radar screen style)
        Using bgPath As New GraphicsPath()
            bgPath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using bgBrush As New PathGradientBrush(bgPath)
                bgBrush.CenterColor = Color.FromArgb(30, 50, 40)
                bgBrush.SurroundColors = {Color.FromArgb(15, 25, 20)}
                g.FillEllipse(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2)
            End Using
        End Using

        ' Outer ring
        Using ringPen As New Pen(Color.FromArgb(100, 150, 100), 2.0F)
            g.DrawEllipse(ringPen, cx - radius, cy - radius, radius * 2, radius * 2)
        End Using
    End Sub

    Private Sub DrawRangeRings(g As Graphics, cx As Single, cy As Single, radius As Single)
        If Not _showRangeRings Then Return

        ' Define range rings and their colors (danger zones)
        Dim rings As New List(Of Tuple(Of Single, Color, String)) From {
            Tuple.Create(5.0F, Color.FromArgb(80, 220, 50, 50), "5 mi"),
            Tuple.Create(10.0F, Color.FromArgb(60, 220, 140, 50), "10"),
            Tuple.Create(20.0F, Color.FromArgb(60, 100, 180, 100), "20"),
            Tuple.Create(30.0F, Color.FromArgb(60, 100, 150, 100), "30")
        }

        Using font As New Font("Arial", 7.0F, FontStyle.Regular)
            For Each ring In rings
                If ring.Item1 <= _maxDistance Then
                    Dim ringRadius As Single = radius * (ring.Item1 / _maxDistance)

                    ' Draw ring circle
                    Using ringPen As New Pen(ring.Item2, 1.5F)
                        ringPen.DashStyle = DashStyle.Dot
                        g.DrawEllipse(ringPen, cx - ringRadius, cy - ringRadius, ringRadius * 2, ringRadius * 2)
                    End Using

                    ' Draw label at top
                    Using brush As New SolidBrush(Color.FromArgb(150, 180, 150))
                        Using fmt As New StringFormat()
                            fmt.Alignment = StringAlignment.Center
                            fmt.LineAlignment = StringAlignment.Far
                            g.DrawString(ring.Item3, font, brush, cx, cy - ringRadius - 2, fmt)
                        End Using
                    End Using
                End If
            Next
        End Using
    End Sub

    Private Sub DrawStrikeHistory(g As Graphics, cx As Single, cy As Single, radius As Single)
        If _strikeHistory.Count = 0 Then Return

        Dim now As DateTime = DateTime.Now
        Dim random As New Random(42) ' Fixed seed for consistent positioning

        For Each strike In _strikeHistory
            Dim distance As Single = strike.Item1
            Dim strikeTime As DateTime = strike.Item2
            Dim age As TimeSpan = now - strikeTime

            ' Skip strikes beyond max distance
            If distance > _maxDistance Then Continue For

            ' Calculate position (random angle for visualization)
            Dim angle As Double = random.NextDouble() * Math.PI * 2
            Dim strikeRadius As Single = radius * (distance / _maxDistance)
            Dim x As Single = CSng(cx + strikeRadius * Math.Cos(angle))
            Dim y As Single = CSng(cy + strikeRadius * Math.Sin(angle))

            ' Fade older strikes
            Dim fadeMinutes As Double = 60.0
            Dim alpha As Integer = CInt(Math.Max(0, 255 * (1.0 - age.TotalMinutes / fadeMinutes)))

            DrawLightningBolt(g, x, y, 12, alpha, distance)
        Next
    End Sub

    Private Shared Sub DrawLightningBolt(g As Graphics, cx As Single, cy As Single, size As Single, alpha As Integer, distance As Single)
        ' Lightning bolt shape
        Dim boltPoints() As PointF = {
            New PointF(cx, cy - size / 2),
            New PointF(cx - size * 0.2F, cy),
            New PointF(cx + size * 0.1F, cy),
            New PointF(cx - size * 0.3F, cy + size / 2),
            New PointF(cx, cy + size * 0.2F),
            New PointF(cx + size * 0.2F, cy - size * 0.3F)
        }

        ' Color based on distance (red=close, yellow=far)
        Dim boltColor As Color
        If distance < 5 Then
            boltColor = Color.FromArgb(alpha, 255, 50, 50)
        ElseIf distance < 10 Then
            boltColor = Color.FromArgb(alpha, 255, 140, 50)
        ElseIf distance < 20 Then
            boltColor = Color.FromArgb(alpha, 255, 220, 100)
        Else
            boltColor = Color.FromArgb(alpha, 150, 200, 150)
        End If

        ' Draw glow
        Using glowBrush As New PathGradientBrush(boltPoints)
            glowBrush.CenterColor = Color.FromArgb(alpha \ 2, 255, 255, 200)
            glowBrush.SurroundColors = {Color.FromArgb(0, boltColor), Color.FromArgb(0, boltColor),
                                         Color.FromArgb(0, boltColor), Color.FromArgb(0, boltColor),
                                         Color.FromArgb(0, boltColor), Color.FromArgb(0, boltColor)}
            Using glowPath As New GraphicsPath()
                glowPath.AddPolygon(boltPoints)
                Using glowRegion As New Region(glowPath)
                    glowRegion.Translate(2, 2)
                    g.FillRegion(glowBrush, glowRegion)
                End Using
            End Using
        End Using

        ' Draw bolt
        Using boltBrush As New SolidBrush(boltColor)
            g.FillPolygon(boltBrush, boltPoints)
        End Using

        ' Outline
        Using boltPen As New Pen(Color.FromArgb(alpha, 255, 255, 255), 1.0F)
            g.DrawPolygon(boltPen, boltPoints)
        End Using
    End Sub

    Private Shared Sub DrawCenter(g As Graphics, cx As Single, cy As Single)
        Dim centerSize As Single = 8.0F

        ' Crosshair
        Using centerPen As New Pen(Color.FromArgb(150, 200, 150), 1.5F)
            g.DrawLine(centerPen, cx - centerSize, cy, cx + centerSize, cy)
            g.DrawLine(centerPen, cx, cy - centerSize, cx, cy + centerSize)
        End Using

        ' Center circle
        Using centerBrush As New SolidBrush(Color.FromArgb(100, 180, 100))
            g.FillEllipse(centerBrush, cx - 3, cy - 3, 6, 6)
        End Using

        Using centerPen As New Pen(Color.FromArgb(150, 200, 150), 1.5F)
            g.DrawEllipse(centerPen, cx - 3, cy - 3, 6, 6)
        End Using

        ' "You" label
        Using font As New Font("Arial", 7.0F, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(180, 220, 180))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Far
                    g.DrawString("YOU", font, brush, cx, cy - centerSize - 2, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawDistanceLabels(g As Graphics, cx As Single, cy As Single, radius As Single)
        Using font As New Font("Segoe UI", 7.5F, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(150, 180, 150))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Center

                    ' Cardinal direction labels
                    g.DrawString("N", font, brush, cx, cy - radius * 0.85F, fmt)
                    g.DrawString("S", font, brush, cx, cy + radius * 0.85F, fmt)
                    g.DrawString("E", font, brush, cx + radius * 0.85F, cy, fmt)
                    g.DrawString("W", font, brush, cx - radius * 0.85F, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawStatusText(g As Graphics, cx As Single, cy As Single)
        Dim statusText As String
        Dim statusColor As Color

        If _lastStrikeDistance = 0 OrElse _lastStrikeTime = DateTime.MinValue Then
            statusText = "No strikes detected"
            statusColor = Color.FromArgb(100, 180, 100)
        Else
            Dim timeSince As TimeSpan = DateTime.Now - _lastStrikeTime
            Dim distText As String = _lastStrikeDistance.ToString("0.0") & " mi"
            Dim timeText As String = If(timeSince.TotalMinutes < 1, $"{CInt(timeSince.TotalSeconds)}s ago",
                                        If(timeSince.TotalHours < 1, $"{CInt(timeSince.TotalMinutes)}m ago",
                                           $"{CInt(timeSince.TotalHours)}h ago"))

            statusText = $"Last: {distText}, {timeText}"

            ' Color based on proximity
            If _lastStrikeDistance < 5 Then
                statusColor = Color.FromArgb(220, 50, 50)
            ElseIf _lastStrikeDistance < 10 Then
                statusColor = Color.FromArgb(220, 140, 50)
            ElseIf _lastStrikeDistance < 20 Then
                statusColor = Color.FromArgb(220, 200, 100)
            Else
                statusColor = Color.FromArgb(100, 180, 100)
            End If
        End If

        Using font As New Font("Segoe UI", 8.5F, FontStyle.Bold)
            Using brush As New SolidBrush(statusColor)
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Near

                    ' Shadow
                    Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                        g.DrawString(statusText, font, shadowBrush, cx + 1, cy + 1, fmt)
                    End Using

                    g.DrawString(statusText, font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using

        ' Strike count
        If _strikeHistory.Count > 0 Then
            Dim countText As String = $"{_strikeHistory.Count} strike{If(_strikeHistory.Count <> 1, "s", "")} (last hour)"
            Using font As New Font("Segoe UI", 7.5F, FontStyle.Regular)
                Using brush As New SolidBrush(Color.FromArgb(120, 150, 120))
                    Using fmt As New StringFormat()
                        fmt.Alignment = StringAlignment.Center
                        fmt.LineAlignment = StringAlignment.Near
                        g.DrawString(countText, font, brush, cx, cy + 15, fmt)
                    End Using
                End Using
            End Using
        End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class