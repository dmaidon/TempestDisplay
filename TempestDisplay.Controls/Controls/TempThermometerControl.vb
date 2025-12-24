Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Professional vertical thermometer control for displaying temperature with classic mercury column design.
''' Features include: dual scale (F/C), color-coded temperature zones, freezing point marker, and glass tube effect.
''' </summary>
<DefaultEvent("Click")>
Public Class TempThermometerControl
    Inherits Control

    Private _tempF As Single
    Private _tempC As Single
    Private _label As String = ""
    Private _minF As Single = -5
    Private _maxF As Single = 110
    Private _showFreezeMarker As Boolean = True
    Private _showDualScale As Boolean = True

    ' New: choose which unit to show in the bulb text readout
    Private _displayUnit As TemperatureUnit = TemperatureUnit.Fahrenheit

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

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    <Description("Show the 32°F freezing point marker")>
    Public Property ShowFreezeMarker As Boolean
        Get
            Return _showFreezeMarker
        End Get
        Set(value As Boolean)
            _showFreezeMarker = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    <Description("Show both Fahrenheit and Celsius scales")>
    Public Property ShowDualScale As Boolean
        Get
            Return _showDualScale
        End Get
        Set(value As Boolean)
            _showDualScale = value
            Invalidate()
        End Set
    End Property

    ' New: unit selection for the single text temperature under the bulb
    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(TemperatureUnit.Fahrenheit)>
    <Description("Unit for the temperature text displayed under the bulb.")>
    Public Property DisplayUnit As TemperatureUnit
        Get
            Return _displayUnit
        End Get
        Set(value As TemperatureUnit)
            _displayUnit = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(80, 150)
    End Sub

    ''' <summary>
    ''' Maps a temperature value to a vertical position (0.0 = bottom, 1.0 = top)
    ''' </summary>
    Private Shared Function ValueToPosition(value As Single, minVal As Single, maxVal As Single) As Single
        Return (value - minVal) / (maxVal - minVal)
    End Function

    ''' <summary>
    ''' Returns a color based on temperature value for the mercury column
    ''' </summary>
    Private Shared Function GetTemperatureColor(tempF As Single) As Color
        ' Cold: Blue tones (below 32°F)
        If tempF < 32.0F Then
            Return Color.FromArgb(70, 130, 220)
        ElseIf tempF < 50.0F Then
            ' Cool: Cyan to Green
            Dim blend As Single = (tempF - 32.0F) / 18.0F
            Return BlendColors(Color.FromArgb(70, 180, 220), Color.FromArgb(50, 200, 150), blend)
        ElseIf tempF < 70.0F Then
            ' Moderate: Green to Yellow
            Dim blend As Single = (tempF - 50.0F) / 20.0F
            Return BlendColors(Color.FromArgb(100, 200, 100), Color.FromArgb(230, 200, 50), blend)
        ElseIf tempF < 85.0F Then
            ' Warm: Yellow to Orange
            Dim blend As Single = (tempF - 70.0F) / 15.0F
            Return BlendColors(Color.FromArgb(240, 190, 50), Color.FromArgb(255, 140, 40), blend)
        Else
            ' Hot: Orange to Red
            Dim blend As Single = Math.Min(1.0F, (tempF - 85.0F) / 15.0F)
            Return BlendColors(Color.FromArgb(255, 120, 40), Color.FromArgb(220, 50, 50), blend)
        End If
    End Function

    ''' <summary>
    ''' Blends two colors based on a blend factor (0.0 = color1, 1.0 = color2)
    ''' </summary>
    Private Shared Function BlendColors(color1 As Color, color2 As Color, blend As Single) As Color
        blend = Math.Max(0.0F, Math.Min(1.0F, blend))
        ' Calculate as Single first to avoid byte arithmetic overflow
        Dim rCalc As Single = CSng(color1.R) + (CSng(color2.R) - CSng(color1.R)) * blend
        Dim gCalc As Single = CSng(color1.G) + (CSng(color2.G) - CSng(color1.G)) * blend
        Dim bCalc As Single = CSng(color1.B) + (CSng(color2.B) - CSng(color1.B)) * blend
        ' Clamp to valid byte range and convert
        Dim r As Integer = CInt(Math.Max(0.0F, Math.Min(255.0F, rCalc)))
        Dim g As Integer = CInt(Math.Max(0.0F, Math.Min(255.0F, gCalc)))
        Dim b As Integer = CInt(Math.Max(0.0F, Math.Min(255.0F, bCalc)))
        Return Color.FromArgb(r, g, b)
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

        ' Calculate dimensions
        Dim labelHeight As Single = If(String.IsNullOrEmpty(_label), 0, 25)

        ' Detect screen resolution: consider 4K or better as >= 3840x2160
        Dim screenSize = Screen.PrimaryScreen.Bounds.Size
        Dim is4KorBetter As Boolean = (screenSize.Width >= 3840 AndAlso screenSize.Height >= 2160)

        ' Bulb size: smaller on sub-4K to avoid disproportion
        Dim bulbSize As Single
        If is4KorBetter Then
            bulbSize = Math.Min(w * 0.4F, 35)
        Else
            bulbSize = Math.Min(w * 0.3F, 24)
        End If

        Dim tubeWidth As Single = Math.Min(w * 0.25F, 22)
        Dim scaleWidth As Single = Math.Min(w * 0.2F, 30)

        ' Reserve more bottom space so bulb labels are visible
        Dim bottomMargin As Single = bulbSize * 2.1F

        ' Vertical bounds for the tube (excluding label at top and bulb at bottom)
        Dim tubeTop As Single = labelHeight + 10
        Dim tubeBottom As Single = h - bottomMargin
        Dim tubeHeight As Single = Math.Max(10.0F, tubeBottom - tubeTop)

        ' Horizontal center
        Dim centerX As Single = w / 2.0F

        ' Draw label at top
        If Not String.IsNullOrEmpty(_label) Then
            DrawLabel(g, _label, centerX, labelHeight / 2.0F)
        End If

        ' Draw the thermometer components
        DrawThermometerBackground(g, centerX, tubeTop, tubeWidth, tubeHeight, bulbSize)
        DrawScale(g, centerX, tubeTop, tubeHeight, tubeWidth, scaleWidth, _minF, _maxF)
        DrawMercuryColumn(g, centerX, tubeTop, tubeWidth, tubeHeight, bulbSize, _tempF, _minF, _maxF)
        DrawGlassTube(g, centerX, tubeTop, tubeWidth, tubeHeight)

        Dim bulbCenterY As Single = tubeTop + tubeHeight + bulbSize / 2.0F
        DrawBulb(g, centerX, bulbCenterY, bulbSize, _tempF)

        ' Move the across-the-bulb temp text into the former bottom label position. the larger the number
        ' the further the number drops below the bulb.
        Dim bulbTextY As Single = bulbCenterY + bulbSize * 0.95F
        DrawBulbTextReadout(g, centerX, bulbTextY, _tempF, _tempC, _displayUnit)
    End Sub

    Private Shared Sub DrawLabel(g As Graphics, label As String, cx As Single, cy As Single)
        Using font As New Font("Segoe UI", 9.0F, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Center
                    fmt.LineAlignment = StringAlignment.Center

                    ' Shadow
                    Using shadowBrush As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
                        g.DrawString(label, font, shadowBrush, cx + 1, cy + 1, fmt)
                    End Using

                    g.DrawString(label, font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawThermometerBackground(g As Graphics, cx As Single, tubeTop As Single, tubeWidth As Single, tubeHeight As Single, bulbSize As Single)
        ' Draw a subtle shadow behind the thermometer for depth
        Dim shadowOffset As Single = 2.0F
        Dim tubeRect As New RectangleF(cx - tubeWidth / 2.0F + shadowOffset, tubeTop + shadowOffset, tubeWidth, tubeHeight)

        Using shadowBrush As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
            g.FillRoundedRectangle(shadowBrush, tubeRect, tubeWidth / 2.0F)
        End Using

        ' Draw bulb shadow
        Dim bulbY As Single = tubeTop + tubeHeight + bulbSize / 2.0F
        Using shadowBrush As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
            g.FillEllipse(shadowBrush, cx - bulbSize / 2.0F + shadowOffset, bulbY - bulbSize / 2.0F + shadowOffset, bulbSize, bulbSize)
        End Using
    End Sub

    Private Sub DrawScale(g As Graphics, cx As Single, tubeTop As Single, tubeHeight As Single, tubeWidth As Single, scaleWidth As Single, minVal As Single, maxVal As Single)
        Using font As New Font("Arial", 7.5F, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(60, 60, 60))
                Using fmt As New StringFormat()
                    fmt.Alignment = StringAlignment.Near
                    fmt.LineAlignment = StringAlignment.Center

                    ' Determine step size for scale markers
                    Dim tempRange As Single = maxVal - minVal
                    Dim majorStep As Integer = If(tempRange > 80, 20, If(tempRange > 40, 10, 5))

                    ' Draw Fahrenheit scale on left
                    Dim tickLeft As Single = cx - tubeWidth / 2.0F - 5
                    Dim labelLeft As Single = tickLeft - scaleWidth

                    Dim steps As Integer = CInt(Math.Floor((maxVal - minVal) / majorStep))
                    For i As Integer = 0 To steps
                        Dim temp As Single = minVal + i * majorStep
                        Dim pos As Single = ValueToPosition(temp, minVal, maxVal)
                        Dim y As Single = tubeTop + tubeHeight * (1.0F - pos)

                        ' Draw tick mark
                        Using tickPen As New Pen(Color.FromArgb(80, 80, 80), 1.5F)
                            g.DrawLine(tickPen, tickLeft, y, tickLeft - 5, y)
                        End Using

                        ' Draw temperature label
                        Dim labelText As String = temp.ToString("0")
                        Using shadowBrush As New SolidBrush(Color.FromArgb(20, 0, 0, 0))
                            g.DrawString(labelText, font, shadowBrush, labelLeft + 1, y + 1, fmt)
                        End Using
                        g.DrawString(labelText, font, brush, labelLeft, y, fmt)
                    Next

                    ' Draw Celsius scale on right if enabled
                    If _showDualScale Then
                        fmt.Alignment = StringAlignment.Far
                        Dim tickRight As Single = cx + tubeWidth / 2.0F + 5
                        Dim labelRight As Single = tickRight + scaleWidth

                        For i As Integer = 0 To steps
                            Dim tempF As Single = minVal + i * majorStep
                            Dim tempC As Single = (tempF - 32.0F) * 5.0F / 9.0F
                            Dim pos As Single = ValueToPosition(tempF, minVal, maxVal)
                            Dim y As Single = tubeTop + tubeHeight * (1.0F - pos)

                            ' Draw tick mark
                            Using tickPen As New Pen(Color.FromArgb(120, 60, 60), 1.5F)
                                g.DrawLine(tickPen, tickRight, y, tickRight + 5, y)
                            End Using

                            ' Draw temperature label
                            Dim labelText As String = tempC.ToString("0")
                            Using shadowBrush As New SolidBrush(Color.FromArgb(20, 0, 0, 0))
                                g.DrawString(labelText, font, shadowBrush, labelRight + 1, y + 1, fmt)
                            End Using
                            Using cBrush As New SolidBrush(Color.FromArgb(140, 60, 60))
                                g.DrawString(labelText, font, cBrush, labelRight, y, fmt)
                            End Using
                        Next
                    End If

                    ' Draw freezing point marker if enabled
                    If _showFreezeMarker AndAlso 32.0F >= minVal AndAlso 32.0F <= maxVal Then
                        Dim freezePos As Single = ValueToPosition(32.0F, minVal, maxVal)
                        Dim freezeY As Single = tubeTop + tubeHeight * (1.0F - freezePos)

                        Using freezePen As New Pen(Color.FromArgb(180, 60, 60), 2.0F)
                            freezePen.DashStyle = DashStyle.Dash
                            g.DrawLine(freezePen, tickLeft - 8, freezeY, cx + tubeWidth / 2.0F + 8, freezeY)
                        End Using
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawMercuryColumn(g As Graphics,
                                         cx As Single,
                                         tubeTop As Single,
                                         tubeWidth As Single,
                                         tubeHeight As Single,
                                         bulbSize As Single,
                                         tempF As Single,
                                         minVal As Single,
                                         maxVal As Single)
        ' Clamp temperature to valid range
        Dim clampedTemp As Single = Math.Max(minVal, Math.Min(maxVal, tempF))
        Dim fillPos As Single = ValueToPosition(clampedTemp, minVal, maxVal)
        Dim fillHeight As Single = tubeHeight * fillPos
        Dim fillBottom As Single = tubeTop + tubeHeight
        Dim fillTop As Single = fillBottom - fillHeight

        ' Get temperature-based color
        Dim mercuryColor As Color = GetTemperatureColor(clampedTemp)

        ' Create gradient for mercury column (darker at edges, lighter in center)
        Dim fillRect As New RectangleF(cx - tubeWidth / 2.0F + 3, fillTop, tubeWidth - 6, fillHeight)

        If fillHeight > 0 Then
            Using mercuryBrush As New LinearGradientBrush(
                New PointF(fillRect.Left, fillRect.Top),
                New PointF(fillRect.Right, fillRect.Top),
                Color.FromArgb(mercuryColor.A, CInt(mercuryColor.R * 0.7), CInt(mercuryColor.G * 0.7), CInt(mercuryColor.B * 0.7)),
                mercuryColor)

                ' Make it lighter in the center for cylindrical glass effect
                Dim blend As New ColorBlend With {
                    .Positions = {0.0F, 0.3F, 0.5F, 0.7F, 1.0F},
                    .Colors = {
                        Color.FromArgb(mercuryColor.A, CInt(mercuryColor.R * 0.6), CInt(mercuryColor.G * 0.6), CInt(mercuryColor.B * 0.6)),
                        Color.FromArgb(mercuryColor.A, CInt(mercuryColor.R * 0.9), CInt(mercuryColor.G * 0.9), CInt(mercuryColor.B * 0.9)),
                        mercuryColor,
                        Color.FromArgb(mercuryColor.A, CInt(mercuryColor.R * 0.9), CInt(mercuryColor.G * 0.9), CInt(mercuryColor.B * 0.9)),
                        Color.FromArgb(mercuryColor.A, CInt(mercuryColor.R * 0.6), CInt(mercuryColor.G * 0.6), CInt(mercuryColor.B * 0.6))
                    }
                }
                mercuryBrush.InterpolationColors = blend

                g.FillRoundedRectangle(mercuryBrush, fillRect, (tubeWidth - 6) / 2.0F)
            End Using

            ' Add highlight on left side of mercury for glass reflection
            Dim highlightWidth As Single = (tubeWidth - 6) / 4.0F
            Dim highlightRect As New RectangleF(fillRect.Left + 2, fillTop, highlightWidth, fillHeight)
            Using highlightBrush As New LinearGradientBrush(
                New PointF(highlightRect.Left, highlightRect.Top),
                New PointF(highlightRect.Right, highlightRect.Top),
                Color.FromArgb(80, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255))
                g.FillRectangle(highlightBrush, highlightRect)
            End Using
        End If
    End Sub

    'Private Shared Sub DrawGlassTube(g As Graphics, cx As Single, tubeTop As Single, tubeWidth As Single)
    '    ArgumentNullException.ThrowIfNull(g)
    '    Dim tubeRect As New RectangleF(cx - tubeWidth / 2.0F, tubeTop, tubeWidth, 0) ' placeholder to keep method signature changes minimal
    'End Sub

    Private Shared Sub DrawGlassTube(g As Graphics, cx As Single, tubeTop As Single, tubeWidth As Single, tubeHeight As Single)
        Dim tubeRect As New RectangleF(cx - tubeWidth / 2.0F, tubeTop, tubeWidth, tubeHeight)

        ' Glass tube outline
        Using tubePen As New Pen(Color.FromArgb(150, 180, 190, 200), 2.0F)
            g.DrawRoundedRectangle(tubePen, tubeRect, tubeWidth / 2.0F)
        End Using

        ' Inner shadow for depth
        Dim innerRect As New RectangleF(tubeRect.X + 2, tubeRect.Y + 2, tubeRect.Width - 4, tubeRect.Height - 4)
        Using innerPen As New Pen(Color.FromArgb(60, 0, 0, 0), 1.0F)
            g.DrawRoundedRectangle(innerPen, innerRect, (tubeWidth - 4) / 2.0F)
        End Using

        ' Glass highlight on left edge
        Dim highlightRect As New RectangleF(tubeRect.X + 2, tubeRect.Y + 5, tubeWidth / 5.0F, tubeHeight - 10)
        Using highlightBrush As New LinearGradientBrush(
            New PointF(highlightRect.Left, highlightRect.Top),
            New PointF(highlightRect.Right, highlightRect.Top),
            Color.FromArgb(100, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255))
            g.FillRectangle(highlightBrush, highlightRect)
        End Using
    End Sub

    Private Shared Sub DrawBulb(g As Graphics, cx As Single, cy As Single, bulbSize As Single, tempF As Single)
        ' Get temperature-based color
        Dim bulbColor As Color = GetTemperatureColor(tempF)

        ' Draw bulb with gradient
        Using bulbPath As New GraphicsPath()
            bulbPath.AddEllipse(cx - bulbSize / 2.0F, cy - bulbSize / 2.0F, bulbSize, bulbSize)

            Using bulbBrush As New PathGradientBrush(bulbPath)
                bulbBrush.CenterPoint = New PointF(cx - bulbSize / 6.0F, cy - bulbSize / 6.0F)
                bulbBrush.CenterColor = Color.FromArgb(255, Math.Min(255, bulbColor.R + 40), Math.Min(255, bulbColor.G + 40), Math.Min(255, bulbColor.B + 40))
                bulbBrush.SurroundColors = {bulbColor}

                g.FillEllipse(bulbBrush, cx - bulbSize / 2.0F, cy - bulbSize / 2.0F, bulbSize, bulbSize)
            End Using
        End Using

        ' Glass outline
        Using bulbPen As New Pen(Color.FromArgb(150, 180, 190, 200), 2.0F)
            g.DrawEllipse(bulbPen, cx - bulbSize / 2.0F, cy - bulbSize / 2.0F, bulbSize, bulbSize)
        End Using

        ' Highlight for glass effect
        Using highlightBrush As New SolidBrush(Color.FromArgb(120, 255, 255, 255))
            Dim highlightSize As Single = bulbSize / 3.0F
            g.FillEllipse(highlightBrush, cx - bulbSize / 3.0F, cy - bulbSize / 3.0F, highlightSize, highlightSize)
        End Using
    End Sub

    ' New helper to draw the single temperature text under the bulb
    Private Shared Sub DrawBulbTextReadout(g As Graphics, cx As Single, cy As Single, tempF As Single, tempC As Single, unit As TemperatureUnit)
        Dim displayValue As Single
        Dim unitSuffix As String

        If unit = TemperatureUnit.Celsius Then
            displayValue = tempC
            unitSuffix = "C"
        Else
            displayValue = tempF
            unitSuffix = "F"
        End If

        ' Use solid black for better readability instead of mercury color
        Dim textColor As Color = Color.Black

        Using fmt As New StringFormat()
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Center

            Using fontVal As New Font("Segoe UI", 9.0F, FontStyle.Bold)
                Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                    g.DrawString(String.Format("{0:0.#}°{1}", displayValue, unitSuffix), fontVal, shadowBrush, cx + 1, cy + 1, fmt)
                End Using
                Using valueBrush As New SolidBrush(textColor)
                    g.DrawString(String.Format("{0:0.#}°{1}", displayValue, unitSuffix), fontVal, valueBrush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class

''' <summary>
''' Which unit to use for the single bulb text readout.
''' </summary>
Public Enum TemperatureUnit
    Fahrenheit = 0
    Celsius = 1
End Enum

''' <summary>
''' Extension methods for Graphics to support rounded rectangles
''' </summary>
Module GraphicsExtensions

    <System.Runtime.CompilerServices.Extension>
    Public Sub FillRoundedRectangle(g As Graphics, brush As Brush, rect As RectangleF, radius As Single)
        Using path As GraphicsPath = CreateRoundedRectanglePath(rect, radius)
            g.FillPath(brush, path)
        End Using
    End Sub

    <System.Runtime.CompilerServices.Extension>
    Public Sub DrawRoundedRectangle(g As Graphics, pen As Pen, rect As RectangleF, radius As Single)
        Using path As GraphicsPath = CreateRoundedRectanglePath(rect, radius)
            g.DrawPath(pen, path)
        End Using
    End Sub

    Private Function CreateRoundedRectanglePath(rect As RectangleF, radius As Single) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim diameter As Single = radius * 2.0F

        ' Top left arc
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90)
        ' Top right arc
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90)
        ' Bottom right arc
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90)
        ' Bottom left arc
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90)
        path.CloseFigure()

        Return path
    End Function

End Module