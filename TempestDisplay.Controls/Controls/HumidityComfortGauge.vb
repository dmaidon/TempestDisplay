Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Enhanced humidity gauge with comfort zones (40-60% ideal, mold risk, static risk).
''' </summary>
<DefaultEvent("Click")>
Public Class HumidityComfortGauge
    Inherits Control

    Private _humidity As Single = 50
    Private _showComfortZones As Boolean = True
    Private _labelText As String = "Relative Humidity"

    ' Property change threshold to avoid excessive repaints
    Private Const HUMIDITY_CHANGE_THRESHOLD As Single = 0.5F

    ' Cached fonts for performance (avoid creating on every paint)
    Private _cachedFont As Font
    Private _cachedSmallFont As Font
    Private _cachedLabelFont As Font
    Private _lastFontSize As Single = 0

    ' Dispose tracking
    Private _disposed As Boolean = False

    ' Static readonly comfort zones to avoid allocation on every paint
    Private Shared ReadOnly ComfortZones As (Start As Single, [End] As Single, Color As Color)() = {
        (0.0F, 30.0F, Color.FromArgb(100, 220, 180, 120)),
        (30.0F, 40.0F, Color.FromArgb(100, 220, 220, 150)),
        (40.0F, 60.0F, Color.FromArgb(100, 100, 200, 100)),
        (60.0F, 70.0F, Color.FromArgb(100, 150, 200, 220)),
        (70.0F, 100.0F, Color.FromArgb(100, 100, 150, 220))
    }

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Humidity As Single
        Get
            Return _humidity
        End Get
        Set(value As Single)
            Dim clampedValue = Math.Max(0, Math.Min(100, value))
            ' Only invalidate if change exceeds threshold
            If Math.Abs(_humidity - clampedValue) < HUMIDITY_CHANGE_THRESHOLD Then Return
            _humidity = clampedValue
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property ShowComfortZones As Boolean
        Get
            Return _showComfortZones
        End Get
        Set(value As Boolean)
            _showComfortZones = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue("Relative Humidity")>
    Public Property LabelText As String
        Get
            Return _labelText
        End Get
        Set(value As String)
            _labelText = If(String.IsNullOrWhiteSpace(value), "Relative Humidity", value)
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(150, 100)
        UpdateCachedFonts()
    End Sub

    ''' <summary>
    ''' Update cached fonts when control font changes
    ''' </summary>
    Private Sub UpdateCachedFonts()
        _cachedFont?.Dispose()
        _cachedSmallFont?.Dispose()
        _cachedLabelFont?.Dispose()

        Dim baseFont As Font = If(Me.Font, SystemFonts.DefaultFont)
        _cachedFont = New Font("Arial", 8, FontStyle.Regular)
        _cachedSmallFont = New Font("Segoe UI", 12, FontStyle.Bold)
        _cachedLabelFont = New Font("Segoe UI", 9.0F, FontStyle.Bold)
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
                _cachedFont?.Dispose()
                _cachedSmallFont?.Dispose()
                _cachedLabelFont?.Dispose()
            End If
            _disposed = True
        End If
        MyBase.Dispose(disposing)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        ' Recreate fonts if control font changed
        If Me.Font IsNot Nothing AndAlso Me.Font.Size <> _lastFontSize Then
            UpdateCachedFonts()
        End If

        Dim w = Me.ClientSize.Width
        Dim h = Me.ClientSize.Height
        Dim cx = w / 2.0F
        Dim cy = h * 0.6F
        Dim radius = Math.Min(w, h * 1.5F) / 2.0F - 30

        DrawArcBackground(g, cx, cy, radius)
        DrawComfortZones(g, cx, cy, radius)
        DrawArcFill(g, cx, cy, radius, _humidity)
        DrawNeedle(g, cx, cy, radius, _humidity)
        DrawScale(g, cx, cy, radius)
        DrawCenterHub(g, cx, cy)
        DrawReadout(g, cx, cy, _humidity)
        DrawLabel(g, cx, h, _labelText)
    End Sub

    Private Shared Sub DrawArcBackground(g As Graphics, cx As Single, cy As Single, radius As Single)
        Using bgBrush As New SolidBrush(Color.FromArgb(240, 245, 250))
            g.FillPie(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2, 180, 180)
        End Using
    End Sub

    Private Sub DrawComfortZones(g As Graphics, cx As Single, cy As Single, radius As Single)
        If Not _showComfortZones Then Return
        
        ' Use static readonly array instead of creating new list every paint
        For Each zone In ComfortZones
            Dim startAngle = 180 + (zone.Start / 100) * 180
            Dim sweepAngle = ((zone.End - zone.Start) / 100) * 180
            Using zonePen As New Pen(zone.Color, radius * 0.15F)
                g.DrawArc(zonePen, cx - radius * 0.85F, cy - radius * 0.85F, radius * 1.7F, radius * 1.7F, startAngle, sweepAngle)
            End Using
        Next
    End Sub

    Private Shared Sub DrawArcFill(g As Graphics, cx As Single, cy As Single, radius As Single, humidity As Single)
        Dim fillAngle = (humidity / 100) * 180
        Dim fillColor = If(humidity < 30, Color.FromArgb(220, 180, 120),
                          If(humidity < 40, Color.FromArgb(220, 220, 150),
                             If(humidity < 60, Color.FromArgb(100, 200, 100),
                                If(humidity < 70, Color.FromArgb(100, 180, 220), Color.FromArgb(100, 150, 220)))))
        Using fillPen As New Pen(fillColor, radius * 0.12F)
            g.DrawArc(fillPen, cx - radius * 0.7F, cy - radius * 0.7F, radius * 1.4F, radius * 1.4F, 180, fillAngle)
        End Using
    End Sub

    Private Shared Sub DrawNeedle(g As Graphics, cx As Single, cy As Single, radius As Single, humidity As Single)
        Dim angle = 180 + (humidity / 100) * 180
        Dim angleRad = angle * Math.PI / 180
        Dim needleLength = radius * 0.65F
        Dim tipX = CSng(cx + needleLength * Math.Cos(angleRad))
        Dim tipY = CSng(cy + needleLength * Math.Sin(angleRad))
        Using needlePen As New Pen(Color.FromArgb(60, 60, 60), 3)
            needlePen.StartCap = LineCap.Round
            needlePen.EndCap = LineCap.ArrowAnchor
            g.DrawLine(needlePen, cx, cy, tipX, tipY)
        End Using
    End Sub

    Private Sub DrawScale(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Use cached font instead of creating new one
        Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
            Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center}
                For i = 0 To 100 Step 20
                    Dim angle = 180 + (i / 100.0) * 180
                    Dim angleRad = angle * Math.PI / 180
                    Dim labelRadius = radius * 0.88F
                    Dim x = CSng(cx + labelRadius * Math.Cos(angleRad))
                    Dim y = CSng(cy + labelRadius * Math.Sin(angleRad))
                    g.DrawString(i.ToString(), _cachedFont, brush, x, y, fmt)
                Next
            End Using
        End Using
    End Sub

    Private Shared Sub DrawCenterHub(g As Graphics, cx As Single, cy As Single)
        Using hubBrush As New SolidBrush(Color.FromArgb(120, 120, 120))
            g.FillEllipse(hubBrush, cx - 6, cy - 6, 12, 12)
        End Using
    End Sub

    Private Sub DrawReadout(g As Graphics, cx As Single, cy As Single, humidity As Single)
        ' Use cached font instead of creating new one
        Using brush As New SolidBrush(Color.FromArgb(50, 50, 50))
            Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Far}
                g.DrawString($"{humidity:0}%", _cachedSmallFont, brush, cx, cy - 10, fmt)
            End Using
        End Using
    End Sub

    Private Sub DrawLabel(g As Graphics, cx As Single, h As Single, text As String)
        ' Use cached font instead of creating new one
        Using brush As New SolidBrush(Color.FromArgb(70, 70, 70))
            Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Far}
                Dim bottomMargin As Single = 44.0F
                g.DrawString(text, _cachedLabelFont, brush, cx, h - bottomMargin, fmt)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class