Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Vertical precipitation intensity gauge showing rain rate with color-coded zones.
''' Displays real-time rain rate with animated effects.
''' </summary>
<DefaultEvent("Click")>
Public Class RainRateGauge
    Inherits Control

    Private _rainRateInchesPerHour As Single = 0
    Private _peakRate As Single = 0
    Private _maxRate As Single = 2.0F
    Private _showPeakMarker As Boolean = True
    Private ReadOnly _animationTimer As Timer
    Private ReadOnly _dropletPositions As New List(Of PointF)
    Private ReadOnly _random As New Random()

    Public Enum RainIntensity
        None = 0
        Trace = 1
        Light = 2
        Moderate = 3
        Heavy = 4
        Violent = 5
    End Enum

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RainRateInchesPerHour As Single
        Get
            Return _rainRateInchesPerHour
        End Get
        Set(value As Single)
            _rainRateInchesPerHour = Math.Max(0, value)
            If _rainRateInchesPerHour > _peakRate Then
                _peakRate = _rainRateInchesPerHour
            End If
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PeakRate As Single
        Get
            Return _peakRate
        End Get
        Set(value As Single)
            _peakRate = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(2.0F)>
    Public Property MaxRate As Single
        Get
            Return _maxRate
        End Get
        Set(value As Single)
            _maxRate = Math.Max(0.1F, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    <Description("Show peak rate marker")>
    Public Property ShowPeakMarker As Boolean
        Get
            Return _showPeakMarker
        End Get
        Set(value As Boolean)
            _showPeakMarker = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(100, 200)

        ' Animation timer for rain drops
        _animationTimer = New Timer With {
            .Interval = 50
        }
        AddHandler _animationTimer.Tick, AddressOf OnAnimationTick
        _animationTimer.Start()
    End Sub

    Private Sub OnAnimationTick(sender As Object, e As EventArgs)
        If _rainRateInchesPerHour > 0 Then
            ' Add new droplets based on rain intensity
            Dim dropCount As Integer = CInt(Math.Min(5, _rainRateInchesPerHour * 3))
            For i As Integer = 0 To dropCount - 1
                _dropletPositions.Add(New PointF(_random.Next(0, Me.Width), 0))
            Next

            ' Move existing droplets down
            For i As Integer = _dropletPositions.Count - 1 To 0 Step -1
                Dim drop As PointF = _dropletPositions(i)
                drop.Y += 5.0F
                If drop.Y > Me.Height Then
                    _dropletPositions.RemoveAt(i)
                Else
                    _dropletPositions(i) = drop
                End If
            Next

            Invalidate()
        Else
            _dropletPositions.Clear()
        End If
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

        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height

        ' Calculate gauge dimensions
        Dim gaugeWidth As Single = Math.Min(w * 0.5F, 60)
        Dim gaugeHeight As Single = h - 100
        Dim gaugeX As Single = (w - gaugeWidth) / 2.0F
        Dim gaugeY As Single = 40

        ' Draw components
        DrawGaugeBackground(g, gaugeX, gaugeY, gaugeWidth, gaugeHeight)
        DrawIntensityZones(g, gaugeX, gaugeY, gaugeWidth, gaugeHeight)
        DrawGaugeFill(g, gaugeX, gaugeY, gaugeWidth, gaugeHeight, _rainRateInchesPerHour)
        DrawPeakMarker(g, gaugeX, gaugeY, gaugeWidth, gaugeHeight, _peakRate)
        DrawGaugeOutline(g, gaugeX, gaugeY, gaugeWidth, gaugeHeight)
        DrawRateIndicator(g, gaugeX + gaugeWidth, gaugeY, gaugeHeight, _rainRateInchesPerHour)
        DrawDigitalReadout(g, w / 2.0F, gaugeY + gaugeHeight + 10, _rainRateInchesPerHour)
        DrawTitle(g, w / 2.0F, 20)
        DrawDroplets(g)
    End Sub

    Private Shared Sub DrawGaugeBackground(g As Graphics, x As Single, y As Single, width As Single, height As Single)
        ' Shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
            g.FillRoundedRectangle(shadowBrush, New RectangleF(x + 2, y + 2, width, height), 8)
        End Using

        ' Background
        Using bgBrush As New LinearGradientBrush(
            New PointF(x, y),
            New PointF(x + width, y),
            Color.FromArgb(240, 240, 245),
            Color.FromArgb(220, 220, 230))
            g.FillRoundedRectangle(bgBrush, New RectangleF(x, y, width, height), 8)
        End Using
    End Sub

    Private Sub DrawIntensityZones(g As Graphics, x As Single, y As Single, width As Single, height As Single)
        Dim zones As New List(Of Tuple(Of Single, Single, Color, String)) From {
            Tuple.Create(0.0F, 0.01F, Color.FromArgb(230, 230, 230), "None"),
            Tuple.Create(0.01F, 0.1F, Color.FromArgb(180, 220, 255), "Trace"),
            Tuple.Create(0.1F, 0.3F, Color.FromArgb(100, 180, 255), "Light"),
            Tuple.Create(0.3F, 1.0F, Color.FromArgb(50, 120, 220), "Moderate"),
            Tuple.Create(1.0F, 2.0F, Color.FromArgb(220, 140, 50), "Heavy"),
            Tuple.Create(2.0F, _maxRate, Color.FromArgb(200, 50, 50), "Violent")
        }

        Using font As New Font("Arial", 7.0F, FontStyle.Regular)
            Using darkBrush As New SolidBrush(Color.FromArgb(60, 60, 60))
                Using lightBrush As New SolidBrush(Color.FromArgb(255, 255, 255))
                    Using fmt As New StringFormat()
                        fmt.Alignment = StringAlignment.Center
                        fmt.LineAlignment = StringAlignment.Center

                        For Each zone In zones
                            Dim zoneStartY As Single = y + height * (1.0F - zone.Item1 / _maxRate)
                            Dim zoneEndY As Single = y + height * (1.0F - Math.Min(zone.Item2, _maxRate) / _maxRate)
                            Dim zoneHeight As Single = zoneStartY - zoneEndY

                            If zoneHeight > 0 Then
                                ' Draw zone color bar
                                Using zoneBrush As New SolidBrush(Color.FromArgb(100, zone.Item3))
                                    g.FillRectangle(zoneBrush, x + 2, zoneEndY, width - 4, zoneHeight)
                                End Using

                                ' Draw zone label if enough space
                                If zoneHeight > 15 Then
                                    Dim labelY As Single = zoneEndY + zoneHeight / 2.0F
                                    Dim brush As Brush = If(zone.Item1 < 0.1F, darkBrush, lightBrush)
                                    g.DrawString(zone.Item4, font, brush, x + width / 2.0F, labelY, fmt)
                                End If

                                ' Draw zone boundary line
                                Using linePen As New Pen(Color.FromArgb(80, 150, 150, 150), 1.0F)
                                    linePen.DashStyle = DashStyle.Dot
                                    g.DrawLine(linePen, x + 2, zoneEndY, x + width - 2, zoneEndY)
                                End Using
                            End If
                        Next
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawGaugeFill(g As Graphics, x As Single, y As Single, width As Single, height As Single, rate As Single)
        Dim clampedRate As Single = Math.Min(rate, _maxRate)
        Dim fillHeight As Single = (clampedRate / _maxRate) * height
        Dim fillY As Single = y + height - fillHeight

        If fillHeight > 0 Then
            Dim intensity As RainIntensity = GetIntensity(rate)
            Dim fillColor As Color = GetIntensityColor(intensity)

            ' Gradient fill
            Dim fillRect As New RectangleF(x + 2, fillY, width - 4, fillHeight)
            Using fillBrush As New LinearGradientBrush(
                New PointF(x, fillY),
                New PointF(x + width, fillY),
                Color.FromArgb(fillColor.A, CInt(fillColor.R * 0.6), CInt(fillColor.G * 0.6), CInt(fillColor.B * 0.6)),
                fillColor)

                Dim blend As New ColorBlend With {
                    .Positions = {0.0F, 0.3F, 0.7F, 1.0F},
                    .Colors = {
                        Color.FromArgb(fillColor.A, CInt(fillColor.R * 0.6), CInt(fillColor.G * 0.6), CInt(fillColor.B * 0.6)),
                        fillColor,
                        fillColor,
                        Color.FromArgb(fillColor.A, CInt(fillColor.R * 0.7), CInt(fillColor.G * 0.7), CInt(fillColor.B * 0.7))
                    }
                }
                fillBrush.InterpolationColors = blend

                g.FillRectangle(fillBrush, fillRect)
            End Using

            ' Highlight
            Dim highlightRect As New RectangleF(x + 3, fillY, width * 0.2F, fillHeight)
            Using highlightBrush As New LinearGradientBrush(
                New PointF(x + 3, fillY),
                New PointF(x + width * 0.3F, fillY),
                Color.FromArgb(80, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255))
                g.FillRectangle(highlightBrush, highlightRect)
            End Using
        End If
    End Sub

    Private Sub DrawPeakMarker(g As Graphics, x As Single, y As Single, width As Single, height As Single, peak As Single)
        If Not _showPeakMarker OrElse peak <= 0 Then Return

        Dim peakY As Single = y + height * (1.0F - Math.Min(peak, _maxRate) / _maxRate)

        Using peakPen As New Pen(Color.FromArgb(200, 220, 50, 50), 2.0F)
            peakPen.DashStyle = DashStyle.Dash
            g.DrawLine(peakPen, x, peakY, x + width, peakY)
        End Using

        ' Arrow marker
        Using markerBrush As New SolidBrush(Color.FromArgb(220, 50, 50))
            Dim arrowSize As Single = 5.0F
            Dim points() As PointF = {
                New PointF(x + width + 2, peakY),
                New PointF(x + width + 2 + arrowSize, peakY - arrowSize),
                New PointF(x + width + 2 + arrowSize, peakY + arrowSize)
            }
            g.FillPolygon(markerBrush, points)
        End Using
    End Sub

    Private Shared Sub DrawGaugeOutline(g As Graphics, x As Single, y As Single, width As Single, height As Single)
        Using outlinePen As New Pen(Color.FromArgb(120, 120, 130), 2.0F)
            g.DrawRoundedRectangle(outlinePen, New RectangleF(x, y, width, height), 8)
        End Using
    End Sub

    Private Sub DrawRateIndicator(g As Graphics, x As Single, y As Single, height As Single, rate As Single)
        Dim indicatorY As Single = y + height * (1.0F - Math.Min(rate, _maxRate) / _maxRate)

        Using indicatorPen As New Pen(Color.FromArgb(50, 50, 50), 2.0F)
            indicatorPen.EndCap = LineCap.ArrowAnchor
            g.DrawLine(indicatorPen, x + 5, indicatorY, x + 15, indicatorY)
        End Using
    End Sub

    Private Shared Sub DrawDigitalReadout(g As Graphics, cx As Single, cy As Single, rate As Single)
        Dim intensity As RainIntensity = GetIntensity(rate)
        Dim intensityText As String = intensity.ToString()
        Dim rateText As String = rate.ToString("0.00") & " in/hr"

        Using rateFont As New Font("Segoe UI", 10.0F, FontStyle.Bold)
            Using intensityFont As New Font("Segoe UI", 8.5F, FontStyle.Regular)
                Using rateBrush As New SolidBrush(Color.FromArgb(30, 105, 210))
                    Using intensityBrush As New SolidBrush(GetIntensityColor(intensity))
                        Using fmt As New StringFormat()
                            fmt.Alignment = StringAlignment.Center
                            fmt.LineAlignment = StringAlignment.Near

                            ' Shadow
                            Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                                g.DrawString(rateText, rateFont, shadowBrush, cx + 1, cy + 1, fmt)
                            End Using

                            g.DrawString(rateText, rateFont, rateBrush, cx, cy, fmt)
                            g.DrawString(intensityText, intensityFont, intensityBrush, cx, cy + 20, fmt)
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawTitle(g As Graphics, cx As Single, cy As Single)
        Using font As New Font("Segoe UI", 9.0F, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Center

                    g.DrawString("Rain Rate", font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawDroplets(g As Graphics)
        Using dropBrush As New SolidBrush(Color.FromArgb(150, 100, 150, 220))
            For Each drop In _dropletPositions
                g.FillEllipse(dropBrush, drop.X, drop.Y, 3, 4)
            Next
        End Using
    End Sub

    Private Shared Function GetIntensity(rate As Single) As RainIntensity
        If rate < 0.01F Then Return RainIntensity.None
        If rate < 0.1F Then Return RainIntensity.Trace
        If rate < 0.3F Then Return RainIntensity.Light
        If rate < 1.0F Then Return RainIntensity.Moderate
        If rate < 2.0F Then Return RainIntensity.Heavy
        Return RainIntensity.Violent
    End Function

    Private Shared Function GetIntensityColor(intensity As RainIntensity) As Color
        Select Case intensity
            Case RainIntensity.None : Return Color.FromArgb(150, 150, 150)
            Case RainIntensity.Trace : Return Color.FromArgb(180, 220, 255)
            Case RainIntensity.Light : Return Color.FromArgb(100, 180, 255)
            Case RainIntensity.Moderate : Return Color.FromArgb(50, 120, 220)
            Case RainIntensity.Heavy : Return Color.FromArgb(220, 140, 50)
            Case RainIntensity.Violent : Return Color.FromArgb(200, 50, 50)
            Case Else : Return Color.Gray
        End Select
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class