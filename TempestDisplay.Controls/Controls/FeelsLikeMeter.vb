Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Feels-like temperature meter showing heat index or wind chill with comfort zones.
''' </summary>
<DefaultEvent("Click")>
Public Class FeelsLikeMeter
    Inherits Control

    Private _actualTemp As Single = 70
    Private _feelsLike As Single = 70
    Private _minTemp As Single = -10
    Private _maxTemp As Single = 120

    Public Enum ComfortLevel
        Dangerous = 0
        VeryUncomfortable = 1
        Uncomfortable = 2
        Comfortable = 3
    End Enum

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ActualTemp As Single
        Get
            Return _actualTemp
        End Get
        Set(value As Single)
            _actualTemp = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property FeelsLike As Single
        Get
            Return _feelsLike
        End Get
        Set(value As Single)
            _feelsLike = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(-10.0F)>
    Public Property MinTemp As Single
        Get
            Return _minTemp
        End Get
        Set(value As Single)
            _minTemp = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue(120.0F)>
    Public Property MaxTemp As Single
        Get
            Return _maxTemp
        End Get
        Set(value As Single)
            _maxTemp = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(100, 250)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        Dim w = Me.ClientSize.Width
        Dim h = Me.ClientSize.Height
        Dim barWidth = Math.Min(w * 0.4F, 50)
        Dim barHeight = h - 100
        Dim barX = (w - barWidth) / 2.0F
        Dim barY = 40

        DrawComfortZones(g, barX, barY, barWidth, barHeight)
        DrawThermometerBar(g, barX, barY, barWidth, barHeight, _feelsLike)
        DrawActualTempMarker(g, barX, barY, barWidth, barHeight, _actualTemp)
        DrawScale(g, barX + barWidth + 5, barY, barHeight)
        DrawReadout(g, w / 2.0F, h - 45, _actualTemp, _feelsLike)
        DrawTitle(g, w / 2.0F, 20)
    End Sub

    Private Sub DrawComfortZones(g As Graphics, x As Single, y As Single, width As Single, height As Single)
        Dim zones As New List(Of Tuple(Of Single, Single, Color, String)) From {
            Tuple.Create(105.0F, _maxTemp, Color.FromArgb(180, 50, 50), "Danger"),
            Tuple.Create(90.0F, 105.0F, Color.FromArgb(220, 120, 50), "Very Hot"),
            Tuple.Create(80.0F, 90.0F, Color.FromArgb(220, 180, 80), "Hot"),
            Tuple.Create(60.0F, 80.0F, Color.FromArgb(100, 200, 100), "Comfortable"),
            Tuple.Create(32.0F, 60.0F, Color.FromArgb(100, 180, 220), "Cool"),
            Tuple.Create(10.0F, 32.0F, Color.FromArgb(100, 150, 220), "Cold"),
            Tuple.Create(_minTemp, 10.0F, Color.FromArgb(100, 120, 200), "Very Cold")
        }

        Using bgBrush As New SolidBrush(Color.FromArgb(240, 240, 245))
            g.FillRoundedRectangle(bgBrush, New RectangleF(x, y, width, height), width / 2)
        End Using

        For Each zone In zones
            Dim zoneStartY = y + height * (1 - (zone.Item1 - _minTemp) / (_maxTemp - _minTemp))
            Dim zoneEndY = y + height * (1 - (Math.Min(zone.Item2, _maxTemp) - _minTemp) / (_maxTemp - _minTemp))
            Dim zoneHeight = zoneStartY - zoneEndY

            If zoneHeight > 0 Then
                Using zoneBrush As New SolidBrush(Color.FromArgb(60, zone.Item3))
                    g.FillRectangle(zoneBrush, x + 2, zoneEndY, width - 4, zoneHeight)
                End Using
            End If
        Next

        Using outlinePen As New Pen(Color.FromArgb(150, 150, 155), 2)
            g.DrawRoundedRectangle(outlinePen, New RectangleF(x, y, width, height), width / 2)
        End Using
    End Sub

    Private Sub DrawThermometerBar(g As Graphics, x As Single, y As Single, width As Single, height As Single, temp As Single)
        Dim clampedTemp = Math.Max(_minTemp, Math.Min(_maxTemp, temp))
        Dim fillHeight = ((clampedTemp - _minTemp) / (_maxTemp - _minTemp)) * height
        Dim fillY = y + height - fillHeight

        If fillHeight > 0 Then
            Dim fillColor = GetTempColor(temp)
            Using fillBrush As New LinearGradientBrush(New PointF(x, fillY), New PointF(x + width, fillY),
                                                       Color.FromArgb(fillColor.A, CInt(fillColor.R * 0.7), CInt(fillColor.G * 0.7), CInt(fillColor.B * 0.7)), fillColor)
                g.FillRectangle(fillBrush, x + 3, fillY, width - 6, fillHeight)
            End Using
        End If
    End Sub

    Private Sub DrawActualTempMarker(g As Graphics, x As Single, y As Single, width As Single, height As Single, temp As Single)
        Dim markerY = y + height * (1 - (temp - _minTemp) / (_maxTemp - _minTemp))
        Using markerPen As New Pen(Color.FromArgb(200, 60, 60, 60), 2)
            markerPen.DashStyle = DashStyle.Dash
            g.DrawLine(markerPen, x - 5, markerY, x + width + 5, markerY)
        End Using
    End Sub

    Private Sub DrawScale(g As Graphics, x As Single, y As Single, height As Single)
        Using font As New Font("Arial", 7, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                For temp = _minTemp To _maxTemp Step 20
                    Dim markY = y + height * (1 - (temp - _minTemp) / (_maxTemp - _minTemp))
                    g.DrawString(temp.ToString("0") & "°", font, brush, x, markY - 7)
                Next
            End Using
        End Using
    End Sub

    Private Sub DrawReadout(g As Graphics, cx As Single, cy As Single, actual As Single, feels As Single)
        Using actualFont As New Font("Segoe UI", 9, FontStyle.Bold)
            Using feelsFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Using brush1 As New SolidBrush(Color.FromArgb(80, 80, 80))
                    Using brush2 As New SolidBrush(GetTempColor(feels))
                        Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center}
                            g.DrawString($"Actual: {actual:0}°F", actualFont, brush1, cx, cy, fmt)
                            g.DrawString($"Feels: {feels:0}°F", feelsFont, brush2, cx, cy + 16, fmt)
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawTitle(g As Graphics, cx As Single, cy As Single)
        Using font As New Font("Segoe UI", 9, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString("Feels Like", font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Function GetTempColor(temp As Single) As Color
        If temp < 32 Then Return Color.FromArgb(100, 150, 220)
        If temp < 60 Then Return Color.FromArgb(100, 180, 220)
        If temp < 80 Then Return Color.FromArgb(100, 200, 100)
        If temp < 90 Then Return Color.FromArgb(220, 180, 80)
        If temp < 105 Then Return Color.FromArgb(220, 120, 50)
        Return Color.FromArgb(180, 50, 50)
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub
End Class
