Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' UV Index meter displaying current UV radiation level with color-coded risk zones.
''' Includes sun icon, horizontal bar graph, and protection recommendations.
''' </summary>
<DefaultEvent("Click")>
Public Class UVIndexMeter
    Inherits Control

    Private _uvIndex As Single = 0
    Private _peakUVToday As Single = 0
    Private _maxUVIndex As Single = 12.0F
    Private _showSunIcon As Boolean = True
    Private _showRecommendations As Boolean = True

    Public Enum UVRiskLevel
        Low = 0          ' 0-2
        Moderate = 1     ' 3-5
        High = 2         ' 6-7
        VeryHigh = 3     ' 8-10
        Extreme = 4      ' 11+
    End Enum

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property UVIndex As Single
        Get
            Return _uvIndex
        End Get
        Set(value As Single)
            _uvIndex = Math.Max(0, value)
            If _uvIndex > _peakUVToday Then
                _peakUVToday = _uvIndex
            End If
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PeakUVToday As Single
        Get
            Return _peakUVToday
        End Get
        Set(value As Single)
            _peakUVToday = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(12.0F)>
    Public Property MaxUVIndex As Single
        Get
            Return _maxUVIndex
        End Get
        Set(value As Single)
            _maxUVIndex = Math.Max(1.0F, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property ShowSunIcon As Boolean
        Get
            Return _showSunIcon
        End Get
        Set(value As Boolean)
            _showSunIcon = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property ShowRecommendations As Boolean
        Get
            Return _showRecommendations
        End Get
        Set(value As Boolean)
            _showRecommendations = value
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

        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height

        Dim barY As Single = 30
        Dim barHeight As Single = 25
        Dim barMarginX As Single = If(_showSunIcon, 50, 20)
        Dim barWidth As Single = w - barMarginX - 20

        ' Draw components
        If _showSunIcon Then
            DrawSunIcon(g, 25, barY + barHeight / 2.0F)
        End If

        DrawUVBar(g, barMarginX, barY, barWidth, barHeight)
        DrawUVFill(g, barMarginX, barY, barWidth, barHeight, _uvIndex)
        DrawPeakMarker(g, barMarginX, barY, barWidth, barHeight, _peakUVToday)
        DrawUVScale(g, barMarginX, barY + barHeight + 5, barWidth)
        DrawCurrentReading(g, w / 2.0F, barY + barHeight + 25, _uvIndex)

        If _showRecommendations Then
            DrawRecommendation(g, w / 2.0F, h - 30, GetRiskLevel(_uvIndex))
        End If
    End Sub

    Private Shared Sub DrawSunIcon(g As Graphics, cx As Single, cy As Single)
        Dim sunRadius As Single = 12.0F
        Dim rayLength As Single = 8.0F

        ' Sun rays
        Using rayPen As New Pen(Color.FromArgb(255, 200, 50), 2.5F)
            rayPen.StartCap = LineCap.Round
            rayPen.EndCap = LineCap.Round

            For angle As Integer = 0 To 315 Step 45
                Dim angleRad As Double = angle * Math.PI / 180.0
                Dim innerX As Single = CSng(cx + (sunRadius + 2) * Math.Cos(angleRad))
                Dim innerY As Single = CSng(cy + (sunRadius + 2) * Math.Sin(angleRad))
                Dim outerX As Single = CSng(cx + (sunRadius + rayLength) * Math.Cos(angleRad))
                Dim outerY As Single = CSng(cy + (sunRadius + rayLength) * Math.Sin(angleRad))
                g.DrawLine(rayPen, innerX, innerY, outerX, outerY)
            Next
        End Using

        ' Sun body with gradient
        Using sunPath As New GraphicsPath()
            sunPath.AddEllipse(cx - sunRadius, cy - sunRadius, sunRadius * 2, sunRadius * 2)
            Using sunBrush As New PathGradientBrush(sunPath)
                sunBrush.CenterColor = Color.FromArgb(255, 255, 100)
                sunBrush.SurroundColors = {Color.FromArgb(255, 200, 50)}
                g.FillEllipse(sunBrush, cx - sunRadius, cy - sunRadius, sunRadius * 2, sunRadius * 2)
            End Using
        End Using

        ' Sun outline
        Using sunPen As New Pen(Color.FromArgb(200, 255, 180, 50), 1.5F)
            g.DrawEllipse(sunPen, cx - sunRadius, cy - sunRadius, sunRadius * 2, sunRadius * 2)
        End Using

        ' Highlight
        Using highlightBrush As New SolidBrush(Color.FromArgb(120, 255, 255, 255))
            g.FillEllipse(highlightBrush, cx - sunRadius / 2, cy - sunRadius / 2, sunRadius * 0.6F, sunRadius * 0.6F)
        End Using
    End Sub

    Private Sub DrawUVBar(g As Graphics, x As Single, y As Single, width As Single, height As Single)
        ' Shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
            g.FillRoundedRectangle(shadowBrush, New RectangleF(x + 2, y + 2, width, height), height / 2.0F)
        End Using

        ' Background
        Using bgBrush As New LinearGradientBrush(
            New PointF(x, y),
            New PointF(x, y + height),
            Color.FromArgb(245, 245, 245),
            Color.FromArgb(225, 225, 225))
            g.FillRoundedRectangle(bgBrush, New RectangleF(x, y, width, height), height / 2.0F)
        End Using

        ' Draw UV risk zones
        Dim zones As New List(Of Tuple(Of Single, Single, Color)) From {
            Tuple.Create(0.0F, 2.0F, Color.FromArgb(100, 200, 100)),    ' Low - Green
            Tuple.Create(2.0F, 5.0F, Color.FromArgb(255, 230, 100)),    ' Moderate - Yellow
            Tuple.Create(5.0F, 7.0F, Color.FromArgb(255, 150, 50)),     ' High - Orange
            Tuple.Create(7.0F, 10.0F, Color.FromArgb(220, 50, 50)),     ' Very High - Red
            Tuple.Create(10.0F, _maxUVIndex, Color.FromArgb(180, 50, 180)) ' Extreme - Purple
        }

        For Each zone In zones
            Dim zoneStartX As Single = x + (zone.Item1 / _maxUVIndex) * width
            Dim zoneEndX As Single = x + (Math.Min(zone.Item2, _maxUVIndex) / _maxUVIndex) * width
            Dim zoneWidth As Single = zoneEndX - zoneStartX

            If zoneWidth > 0 Then
                Using zoneBrush As New SolidBrush(Color.FromArgb(100, zone.Item3))
                    Dim zoneRect As New RectangleF(zoneStartX, y, zoneWidth, height)
                    If zoneStartX = x Then
                        ' First zone - round left edge
                        g.FillRoundedRectangle(zoneBrush, zoneRect, height / 2.0F)
                    ElseIf zoneEndX >= x + width Then
                        ' Last zone - round right edge
                        g.FillRoundedRectangle(zoneBrush, zoneRect, height / 2.0F)
                    Else
                        g.FillRectangle(zoneBrush, zoneRect)
                    End If
                End Using
            End If
        Next

        ' Outline
        Using outlinePen As New Pen(Color.FromArgb(150, 150, 150), 1.5F)
            g.DrawRoundedRectangle(outlinePen, New RectangleF(x, y, width, height), height / 2.0F)
        End Using
    End Sub

    Private Sub DrawUVFill(g As Graphics, x As Single, y As Single, width As Single, height As Single, uvIndex As Single)
        Dim clampedUV As Single = Math.Min(uvIndex, _maxUVIndex)
        Dim fillWidth As Single = (clampedUV / _maxUVIndex) * width

        If fillWidth > 0 Then
            Dim fillColor As Color = GetUVColor(uvIndex)

            ' Gradient fill
            Dim fillRect As New RectangleF(x + 2, y + 2, fillWidth - 4, height - 4)
            Using fillBrush As New LinearGradientBrush(
                New PointF(x, y),
                New PointF(x, y + height),
                Color.FromArgb(fillColor.A, Math.Min(255, fillColor.R + 40), Math.Min(255, fillColor.G + 40), Math.Min(255, fillColor.B + 40)),
                fillColor)
                g.FillRoundedRectangle(fillBrush, fillRect, (height - 4) / 2.0F)
            End Using

            ' Highlight on top
            Dim highlightRect As New RectangleF(x + 2, y + 2, fillWidth - 4, (height - 4) * 0.3F)
            Using highlightBrush As New LinearGradientBrush(
                New PointF(x, y + 2),
                New PointF(x, y + (height - 4) * 0.3F),
                Color.FromArgb(80, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255))
                g.FillRectangle(highlightBrush, highlightRect)
            End Using
        End If
    End Sub

    Private Sub DrawPeakMarker(g As Graphics, x As Single, y As Single, width As Single, height As Single, peak As Single)
        If peak <= 0 Then Return

        Dim peakX As Single = x + (Math.Min(peak, _maxUVIndex) / _maxUVIndex) * width

        Using peakPen As New Pen(Color.FromArgb(200, 100, 100, 100), 2.0F)
            peakPen.DashStyle = DashStyle.Dash
            g.DrawLine(peakPen, peakX, y - 3, peakX, y + height + 3)
        End Using

        ' Triangle marker at top
        Using markerBrush As New SolidBrush(Color.FromArgb(100, 100, 100))
            Dim markerSize As Single = 5.0F
            Dim points() As PointF = {
                New PointF(peakX, y - 5),
                New PointF(peakX - markerSize, y - 5 - markerSize),
                New PointF(peakX + markerSize, y - 5 - markerSize)
            }
            g.FillPolygon(markerBrush, points)
        End Using
    End Sub

    Private Sub DrawUVScale(g As Graphics, x As Single, y As Single, width As Single)
        Using font As New Font("Arial", 7.0F, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Near

                    ' Draw scale markers
                    For i As Single = 0 To _maxUVIndex Step 2
                        Dim markerX As Single = x + (i / _maxUVIndex) * width
                        g.DrawString(i.ToString("0"), font, brush, markerX, y, fmt)

                        ' Tick mark
                        Using tickPen As New Pen(Color.FromArgb(120, 120, 120), 1.0F)
                            g.DrawLine(tickPen, markerX, y - 3, markerX, y - 1)
                        End Using
                    Next

                    ' Draw risk level labels - move up a few pixels so they're vertically centered relative to the scale
                    fmt.LineAlignment = StringAlignment.Far
                    Dim labelYOffset As Single = -6.0F
                    Dim labels() As Tuple(Of Single, String) = {
                        Tuple.Create(1.0F, "Low"),
                        Tuple.Create(3.5F, "Mod"),
                        Tuple.Create(6.0F, "High"),
                        Tuple.Create(8.5F, "V.High"),
                        Tuple.Create(11.0F, "Ext")
                    }

                    For Each label In labels
                        If label.Item1 <= _maxUVIndex Then
                            Dim labelX As Single = x + (label.Item1 / _maxUVIndex) * width
                            g.DrawString(label.Item2, font, brush, labelX, y + labelYOffset, fmt)
                        End If
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawCurrentReading(g As Graphics, cx As Single, cy As Single, uvIndex As Single)
        Dim currentText As String = "Current: " & uvIndex.ToString("0.0")
        Dim peakText As String = "Peak: " & _peakUVToday.ToString("0.0")

        Using currentFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)
            Using peakFont As New Font("Segoe UI", 8.0F, FontStyle.Regular)
                Using currentBrush As New SolidBrush(GetUVColor(uvIndex))
                    Using peakBrush As New SolidBrush(Color.FromArgb(100, 100, 100))
                        Using fmt As New StringFormat()
                            fmt.Alignment = StringAlignment.Center
                            fmt.LineAlignment = StringAlignment.Near

                            ' Shadow
                            Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                                g.DrawString(currentText, currentFont, shadowBrush, cx + 1, cy + 1, fmt)
                            End Using

                            g.DrawString(currentText, currentFont, currentBrush, cx, cy, fmt)
                            g.DrawString(peakText, peakFont, peakBrush, cx, cy + 15, fmt)            '15
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawRecommendation(g As Graphics, cx As Single, cy As Single, risk As UVRiskLevel)
        Dim recommendation As String = GetRecommendation(risk)

        Using font As New Font("Segoe UI", 8.0F, FontStyle.Italic)
            Using brush As New SolidBrush(Color.FromArgb(90, 90, 90))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Center

                    g.DrawString(recommendation, font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Function GetRiskLevel(uvIndex As Single) As UVRiskLevel
        If uvIndex < 3 Then Return UVRiskLevel.Low
        If uvIndex < 6 Then Return UVRiskLevel.Moderate
        If uvIndex < 8 Then Return UVRiskLevel.High
        If uvIndex < 11 Then Return UVRiskLevel.VeryHigh
        Return UVRiskLevel.Extreme
    End Function

    Private Shared Function GetUVColor(uvIndex As Single) As Color
        Dim risk As UVRiskLevel = GetRiskLevel(uvIndex)
        Select Case risk
            Case UVRiskLevel.Low : Return Color.FromArgb(100, 200, 100)
            Case UVRiskLevel.Moderate : Return Color.FromArgb(255, 200, 50)
            Case UVRiskLevel.High : Return Color.FromArgb(255, 140, 40)
            Case UVRiskLevel.VeryHigh : Return Color.FromArgb(220, 50, 50)
            Case UVRiskLevel.Extreme : Return Color.FromArgb(180, 50, 180)
            Case Else : Return Color.Gray
        End Select
    End Function

    Private Shared Function GetRecommendation(risk As UVRiskLevel) As String
        Select Case risk
            Case UVRiskLevel.Low : Return "Minimal protection needed"
            Case UVRiskLevel.Moderate : Return "Wear sunscreen, hat recommended"
            Case UVRiskLevel.High : Return "Protection essential - sunscreen, hat, shade"
            Case UVRiskLevel.VeryHigh : Return "Extra protection required - avoid midday sun"
            Case UVRiskLevel.Extreme : Return "Stay indoors during midday hours"
            Case Else : Return ""
        End Select
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class