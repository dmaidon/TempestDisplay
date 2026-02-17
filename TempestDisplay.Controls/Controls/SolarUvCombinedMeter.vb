' Last Edit: February 17, 2026 (Add daily peak markers for UV and solar)
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class SolarUvCombinedMeter
    Inherits Control

    ' Data
    Private _uvIndex As Single

    Private _uvPeak As Single
    Private _solarRadiation As Single
    Private _solarPeak As Single
    Private _maxRadiation As Single = 1200.0F
    Private _showPeak As Boolean = True

    ' Property change thresholds to avoid excessive repaints
    Private Const UV_CHANGE_THRESHOLD As Single = 0.1F

    Private Const SOLAR_CHANGE_THRESHOLD As Single = 5.0F

    ' Cached fonts for performance (avoid creating on every paint)
    Private _cachedTitleFont As Font

    Private _cachedValueFont As Font
    Private _cachedSmallFont As Font
    Private _lastFontSize As Single = 0

    ' Dispose tracking
    Private _disposed As Boolean = False

    ' Static readonly UV segments to avoid allocation on every paint
    Private Shared ReadOnly UvSegments As (Color As Color, Label As String, Max As Single)() = {
        (Color.FromArgb(167, 215, 150), "Low", 2.0F),
        (Color.FromArgb(230, 207, 140), "Mod", 5.0F),
        (Color.FromArgb(233, 170, 120), "High", 7.0F),
        (Color.FromArgb(220, 130, 170), "V.High", 10.0F),
        (Color.FromArgb(210, 150, 210), "Ext", 12.0F)
    }

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(220, 160)
        Me.BackColor = Color.FromArgb(245, 235, 220)
        UpdateCachedFonts()
    End Sub

    ''' <summary>
    ''' Update cached fonts when control font changes
    ''' </summary>
    Private Sub UpdateCachedFonts()
        _cachedTitleFont?.Dispose()
        _cachedValueFont?.Dispose()
        _cachedSmallFont?.Dispose()

        Dim baseFont As Font = If(Me.Font, SystemFonts.DefaultFont)
        Dim titleSize As Single = Math.Min(Math.Max(baseFont.Size * 0.95F, 8.0F), 11.0F)
        Dim valueSize As Single = Math.Min(Math.Max(baseFont.Size * 0.9F, 7.5F), 10.0F)
        Dim smallSize As Single = Math.Min(Math.Max(baseFont.Size * 0.85F, 7.0F), 9.0F)

        _cachedTitleFont = New Font(baseFont.FontFamily, titleSize, FontStyle.Bold)
        _cachedValueFont = New Font(baseFont.FontFamily, valueSize, FontStyle.Bold)
        _cachedSmallFont = New Font(baseFont.FontFamily, smallSize, FontStyle.Regular)
        _lastFontSize = baseFont.Size
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        UpdateCachedFonts()
        Invalidate()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                ' Dispose managed resources (cached fonts)
                _cachedTitleFont?.Dispose()
                _cachedValueFont?.Dispose()
                _cachedSmallFont?.Dispose()
            End If
            _disposed = True
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' Properties
    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(0.0F)>
    Public Property UvIndex As Single
        Get
            Return _uvIndex
        End Get
        Set(value As Single)
            Dim clampedValue = Math.Max(0.0F, Math.Min(12.0F, value))
            ' Only invalidate if change exceeds threshold
            If Math.Abs(_uvIndex - clampedValue) < UV_CHANGE_THRESHOLD Then Return
            _uvIndex = clampedValue
            If clampedValue > _uvPeak Then _uvPeak = clampedValue
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(0.0F)>
    Public Property UvPeak As Single
        Get
            Return _uvPeak
        End Get
        Set(value As Single)
            Dim clampedValue = Math.Max(0.0F, Math.Min(12.0F, value))
            ' Only invalidate if change exceeds threshold
            If Math.Abs(_uvPeak - clampedValue) < UV_CHANGE_THRESHOLD Then Return
            _uvPeak = clampedValue
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(0.0F)>
    Public Property SolarRadiation As Single
        Get
            Return _solarRadiation
        End Get
        Set(value As Single)
            Dim clampedValue = Math.Max(0.0F, value)
            ' Only invalidate if change exceeds threshold
            If Math.Abs(_solarRadiation - clampedValue) < SOLAR_CHANGE_THRESHOLD Then Return
            _solarRadiation = clampedValue
            If clampedValue > _solarPeak Then _solarPeak = clampedValue
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(0.0F)>
    Public Property SolarPeak As Single
        Get
            Return _solarPeak
        End Get
        Set(value As Single)
            Dim clampedValue = Math.Max(0.0F, value)
            ' Only invalidate if change exceeds threshold
            If Math.Abs(_solarPeak - clampedValue) < SOLAR_CHANGE_THRESHOLD Then Return
            _solarPeak = clampedValue
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(1200.0F)>
    Public Property MaxRadiation As Single
        Get
            Return _maxRadiation
        End Get
        Set(value As Single)
            _maxRadiation = Math.Max(600.0F, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property ShowPeak As Boolean
        Get
            Return _showPeak
        End Get
        Set(value As Boolean)
            _showPeak = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.CompositingQuality = CompositingQuality.HighQuality

        Dim w = ClientSize.Width
        Dim h = ClientSize.Height
        If w <= 0 OrElse h <= 0 Then Return

        ' Recreate fonts if control font changed
        If Me.Font IsNot Nothing AndAlso Me.Font.Size <> _lastFontSize Then
            UpdateCachedFonts()
        End If

        ' Use cached fonts instead of creating new ones every paint
        Dim sidePad As Integer = CInt(Math.Max(8, w * 0.04))
        Dim topPad As Integer = CInt(Math.Max(8, h * 0.05))
        Dim midPad As Integer = CInt(Math.Max(8, h * 0.05))
        Dim bottomPad As Integer = CInt(Math.Max(6, h * 0.04))

        Dim availH = h - topPad - bottomPad - midPad
        Dim uvH = Math.Max(CInt(availH * 0.5), 80)
        Dim solH = Math.Max(availH - uvH, 70)
        Dim contentW = w - sidePad * 2

        Dim uvRect As New Rectangle(sidePad, topPad, contentW, uvH)
        Dim solRect As New Rectangle(sidePad, topPad + uvH + midPad, contentW, solH)

        DrawUV(g, uvRect)
        DrawSolar(g, solRect)
    End Sub

    Private Sub DrawUV(g As Graphics, rect As Rectangle)
        ' Title - use cached font
        Using titleBrush As New SolidBrush(Color.FromArgb(60, 60, 60))
            g.DrawString("UV Index", _cachedTitleFont, titleBrush, rect.X, rect.Y)
        End Using

        ' Bar area
        Dim barTop As Integer = rect.Y + CInt(_cachedTitleFont.Height * 1.0)
        Dim barHeight As Integer = Math.Max(CInt(rect.Height * 0.32), CInt(_cachedSmallFont.Size * 2.8F))
        Dim barRect As New Rectangle(rect.X + CInt(rect.Width * 0.1), barTop, rect.Width - CInt(rect.Width * 0.12), barHeight)

        ' Use static readonly segments instead of creating array every paint
        Dim x0 = barRect.Left
        Dim total = 12.0F
        Dim currentMax As Single = 0.0F
        For i = 0 To UvSegments.Length - 1
            Dim segment = UvSegments(i)
            Dim minVal As Single = currentMax
            Dim maxVal As Single = segment.Max
            Dim segW As Integer = CInt((maxVal - minVal) / total * barRect.Width)
            Dim segRect As New Rectangle(x0, barRect.Top, segW, barRect.Height)
            Using b As New SolidBrush(segment.Color)
                Using path As New GraphicsPath()
                    path.AddRoundedRect(segRect, CInt(barRect.Height / 2))
                    g.FillPath(b, path)
                End Using
            End Using

            ' Smaller labels - use cached font
            Using lblBrush As New SolidBrush(Color.FromArgb(60, 60, 60))
                Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                Using labelFont As New Font(_cachedSmallFont.FontFamily, _cachedSmallFont.Size * 0.8F)
                    g.DrawString(segment.Label, labelFont, lblBrush, segRect, sf)
                End Using
            End Using

            x0 += segW
            currentMax = maxVal
        Next

        ' Scale numbers 0..12 - use cached font
        Dim tickY = barRect.Bottom + 1
        Using tickBrush As New SolidBrush(Color.FromArgb(80, 80, 80))
            Using tickFont As New Font(_cachedSmallFont.FontFamily, _cachedSmallFont.Size * 0.8F)
                For v As Integer = 0 To 12 Step 2
                    Dim tx = barRect.Left + CInt(v / total * barRect.Width)
                    g.DrawString(v.ToString(), tickFont, tickBrush, tx - 6, tickY)
                Next
            End Using
        End Using

        ' Sun icon on left, smaller
        Dim iconSize As Integer = Math.Max(CInt(barRect.Height * 0.65), 12)
        Dim iconX As Integer = rect.Left + 2
        Dim iconY As Integer = barRect.Top + (barRect.Height - iconSize) \ 2
        DrawSun(g, New Rectangle(iconX, iconY, iconSize, iconSize))

        ' Current/peak block - use cached font
        Dim textRight As Integer = rect.Right - CInt(rect.Width * 0.04)
        Dim baseY As Integer = barRect.Bottom + CInt(_cachedValueFont.Height * 0.1) + 15
        Using greenBrush As New SolidBrush(Color.FromArgb(0, 120, 0))
            Dim fmt As New StringFormat() With {.Alignment = StringAlignment.Far}
            Using boldFont As New Font(_cachedValueFont, FontStyle.Bold)
                g.DrawString($"Current: {_uvIndex:0.0}", boldFont, greenBrush, textRight, baseY, fmt)
            End Using
        End Using
        If _showPeak Then
            Using darkBrush As New SolidBrush(Color.FromArgb(70, 70, 70))
                Dim fmt As New StringFormat() With {.Alignment = StringAlignment.Far}
                g.DrawString($"Peak: {_uvPeak:0.0}", _cachedSmallFont, darkBrush, textRight, baseY + CInt(_cachedValueFont.Height * 0.75), fmt)
            End Using
        End If

        ' Protection message - use cached font
        Using msgBrush As New SolidBrush(Color.FromArgb(100, 100, 100))
            Dim msgY = baseY + CInt(_cachedValueFont.Height * If(_showPeak, 1.5, 0.7)) + 15
            Using msgFont As New Font(_cachedSmallFont, FontStyle.Regular)
                g.DrawString(GetProtectionMessage(_uvIndex), msgFont, msgBrush, rect.Left + CInt(rect.Width * 0.1), msgY)
            End Using
        End Using

        ' Pointer - larger, more visible indicator
        Dim pointerX As Integer = barRect.Left + CInt((_uvIndex / total) * barRect.Width)
        Dim bubbleR As Integer = Math.Max(CInt(barRect.Height * 0.35), 7)

        ' Daily peak marker
        Dim peakX As Integer = barRect.Left + CInt((_uvPeak / total) * barRect.Width)
        Using peakPen As New Pen(Color.Black, 1.5F)
            g.DrawLine(peakPen, peakX, barRect.Top, peakX, barRect.Bottom)
        End Using

        ' Draw indicator with white outline for contrast
        Using outlinePen As New Pen(Color.White, 2.0F)
            g.DrawEllipse(outlinePen, pointerX - bubbleR, barRect.Top + (barRect.Height \ 2) - bubbleR, bubbleR * 2, bubbleR * 2)
        End Using
        Using bubbleBrush As New SolidBrush(Color.FromArgb(220, 0, 180, 0))
            g.FillEllipse(bubbleBrush, pointerX - bubbleR, barRect.Top + (barRect.Height \ 2) - bubbleR, bubbleR * 2, bubbleR * 2)
        End Using
        Using outlinePen As New Pen(Color.FromArgb(0, 100, 0), 1.5F)
            g.DrawEllipse(outlinePen, pointerX - bubbleR, barRect.Top + (barRect.Height \ 2) - bubbleR, bubbleR * 2, bubbleR * 2)
        End Using
    End Sub

    Private Sub DrawSolar(g As Graphics, rect As Rectangle)
        ' Offset entire solar section down to give UV more top space
        Dim offsetY As Integer = 25

        ' Title - use cached font
        Using titleBrush As New SolidBrush(Color.FromArgb(60, 60, 60))
            g.DrawString("Solar Radiation", _cachedTitleFont, titleBrush, rect.X, rect.Y + offsetY)
        End Using

        ' Bar area
        Dim barTop As Integer = rect.Y + offsetY + CInt(_cachedTitleFont.Height * 1.0)
        Dim barHeight As Integer = Math.Max(CInt(rect.Height * 0.32), CInt(_cachedSmallFont.Size * 2.6F))
        Dim barRect As New Rectangle(rect.X + CInt(rect.Width * 0.1), barTop, rect.Width - CInt(rect.Width * 0.12), barHeight)

        Using lg As New LinearGradientBrush(barRect, Color.FromArgb(235, 235, 235), Color.FromArgb(210, 210, 210), 90.0F)
            Using path As New GraphicsPath()
                path.AddRoundedRect(barRect, CInt(barRect.Height / 2))
                g.FillPath(lg, path)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(180, 180, 180))
            Using path As New GraphicsPath()
                path.AddRoundedRect(barRect, CInt(barRect.Height / 2))
                g.DrawPath(pen, path)
            End Using
        End Using

        ' Sun icon
        Dim iconSize As Integer = Math.Max(CInt(barRect.Height * 0.65), 12)
        Dim iconX As Integer = rect.Left + 2
        Dim iconY As Integer = barRect.Top + (barRect.Height - iconSize) \ 2
        DrawSun(g, New Rectangle(iconX, iconY, iconSize, iconSize))

        ' Scale numbers - use cached font
        Using tickBrush As New SolidBrush(Color.FromArgb(80, 80, 80))
            Using tickFont As New Font(_cachedSmallFont.FontFamily, _cachedSmallFont.Size * 0.8F)
                Dim steps() As Integer = {0, 200, 400, 600, 800, 1000, 1200}
                For Each v In steps
                    Dim tx = barRect.Left + CInt((v / _maxRadiation) * barRect.Width)
                    g.DrawString(v.ToString(), tickFont, tickBrush, tx - 10, barRect.Bottom + 1)
                Next
            End Using
        End Using

        ' Pointer - larger, more visible indicator
        Dim clamped = Math.Min(_maxRadiation, Math.Max(0.0F, _solarRadiation))
        Dim clampedPeak = Math.Min(_maxRadiation, Math.Max(0.0F, _solarPeak))
        Dim pointerX As Integer = barRect.Left + CInt((clamped / _maxRadiation) * barRect.Width)
        Dim bubbleR As Integer = Math.Max(CInt(barRect.Height * 0.35), 7)

        ' Daily peak marker
        Dim peakX As Integer = barRect.Left + CInt((clampedPeak / _maxRadiation) * barRect.Width)
        Using peakPen As New Pen(Color.Black, 1.5F)
            g.DrawLine(peakPen, peakX, barRect.Top, peakX, barRect.Bottom)
        End Using

        ' Draw indicator with white outline for contrast
        Using outlinePen As New Pen(Color.White, 2.0F)
            g.DrawEllipse(outlinePen, pointerX - bubbleR, barRect.Top + (barRect.Height \ 2) - bubbleR, bubbleR * 2, bubbleR * 2)
        End Using
        Using bubbleBrush As New SolidBrush(Color.FromArgb(220, 255, 140, 0))
            g.FillEllipse(bubbleBrush, pointerX - bubbleR, barRect.Top + (barRect.Height \ 2) - bubbleR, bubbleR * 2, bubbleR * 2)
        End Using
        Using outlinePen As New Pen(Color.FromArgb(200, 100, 0), 1.5F)
            g.DrawEllipse(outlinePen, pointerX - bubbleR, barRect.Top + (barRect.Height \ 2) - bubbleR, bubbleR * 2, bubbleR * 2)
        End Using

        ' Value text closer to scale - use cached font
        Using blackBrush As New SolidBrush(Color.Black)
            Dim txt = $"{_solarRadiation:0} W/m²"
            Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
            Dim valueY As Integer = barRect.Bottom + CInt(_cachedSmallFont.Height * 0.75)
            Using boldFont As New Font(_cachedValueFont, FontStyle.Bold)
                g.DrawString(txt, boldFont, blackBrush, rect.Left + rect.Width \ 2, valueY)
            End Using
        End Using
    End Sub

    Private Shared Function GetProtectionMessage(uv As Single) As String
        If uv < 3.0F Then Return "Minimal protection needed"
        If uv < 6.0F Then Return "Moderate: Hat, sunglasses"
        If uv < 8.0F Then Return "High: SPF 30+, shade"
        If uv < 11.0F Then Return "Very high: Limit exposure"
        Return "Extreme: Avoid midday sun"
    End Function

End Class

' Helpers
Module GraphicsPathExtensions

    <System.Runtime.CompilerServices.Extension>
    Public Sub AddRoundedRect(path As GraphicsPath, rect As Rectangle, radius As Integer)
        Dim r As Integer = Math.Max(1, radius)
        Dim d As Integer = r * 2
        path.Reset()
        path.AddArc(rect.Left, rect.Top, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
    End Sub

    Public Sub DrawSun(g As Graphics, rect As Rectangle)
        Dim cx As Single = rect.Left + rect.Width / 2.0F
        Dim cy As Single = rect.Top + rect.Height / 2.0F
        Dim r As Single = rect.Width / 2.6F
        Using b As New SolidBrush(Color.FromArgb(255, 210, 80))
            g.FillEllipse(b, rect)
        End Using
        Using p As New Pen(Color.FromArgb(240, 180, 60), Math.Max(1.0F, rect.Width / 18.0F))
            For i As Integer = 0 To 7
                Dim ang As Single = i * (360.0F / 8.0F)
                Dim rad As Single = ang * Math.PI / 180.0F
                Dim x1 As Single = cx + Math.Cos(rad) * r
                Dim y1 As Single = cy + Math.Sin(rad) * r
                Dim x2 As Single = cx + Math.Cos(rad) * (r + rect.Width / 4.0F)
                Dim y2 As Single = cy + Math.Sin(rad) * (r + rect.Width / 4.0F)
                g.DrawLine(p, x1, y1, x2, y2)
            Next
        End Using
    End Sub

End Module