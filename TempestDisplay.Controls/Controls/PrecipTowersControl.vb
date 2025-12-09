Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class PrecipTowersControl
    Inherits Control

    Private _values As Single() = {0, 0, 0, 0, 0}
    Private ReadOnly _labels As String() = {"Today", "Yesterday", "Month", "Year", "All Time"}
    Private _units As String = "in"

    ' Default max height per tower in inches: Day=5, Yesterday=5, Month=30, Year=75, AllTime=400
    Private _defaultScales As Single() = {5.0F, 5.0F, 15.0F, 60.0F, 350.0F}

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Values As Single()
        Get
            Return _values
        End Get
        Set(value As Single())
            If value IsNot Nothing AndAlso value.Length = 5 Then
                _values = value
                Invalidate()
            End If
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Units As String
        Get
            Return _units
        End Get
        Set(value As String)
            _units = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DefaultScales As Single()
        Get
            Return _defaultScales
        End Get
        Set(value As Single())
            If value IsNot Nothing AndAlso value.Length = 5 AndAlso value.All(Function(v) v > 0) Then
                _defaultScales = value
                Invalidate()
            End If
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(200, 120)
        Me.Size = New Size(420, 200)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim towerCount = _values.Length
        If towerCount = 0 Then Return

        Dim marginX As Integer = 16
        Dim topMargin As Integer = 22
        Dim bottomMargin As Integer = 50

        Dim availWidth As Integer = ClientSize.Width - marginX * 2
        Dim availHeight As Integer = ClientSize.Height - topMargin - bottomMargin
        If availWidth <= 0 OrElse availHeight <= 0 Then Return

        Dim towerWidth As Integer = Math.Max(20, availWidth \ (towerCount * 2))
        Dim spacing As Integer = 0
        If towerCount > 1 Then
            spacing = Math.Max(8, (availWidth - towerWidth * towerCount) \ (towerCount - 1))
        End If

        Dim towerAreaTop As Integer = topMargin
        Dim towerAreaBottom As Integer = topMargin + availHeight
        Dim towerFullHeight As Integer = availHeight

        For i As Integer = 0 To towerCount - 1
            Dim x As Integer = marginX + i * (towerWidth + spacing)

            ' Determine effective max for this tower
            Dim maxInches As Single = Math.Max(_defaultScales(i), _values(i))
            Dim pixelsPerInch As Single = towerFullHeight / Math.Max(0.0001F, maxInches)

            ' Water level
            Dim waterHeight As Integer = CInt(Math.Round(_values(i) * pixelsPerInch))

            ' Draw tower with professional styling
            DrawTower(g, x, towerAreaTop, towerWidth, towerFullHeight, waterHeight, towerAreaBottom)

            ' Draw text labels
            DrawTowerLabels(g, x, towerWidth, towerAreaBottom, _values(i), _labels(i), maxInches)
        Next
    End Sub

    Private Shared Sub DrawTower(g As Graphics, x As Integer, towerTop As Integer, towerWidth As Integer, towerHeight As Integer, waterHeight As Integer, towerBottom As Integer)
        Dim cornerRadius As Integer = Math.Min(6, towerWidth \ 4)

        ' Create tower rectangle with rounded corners
        Dim towerPath As GraphicsPath = CreateRoundedRectangle(x, towerTop, towerWidth, towerHeight, cornerRadius)

        ' Draw shadow
        Using shadowPath As GraphicsPath = CreateRoundedRectangle(x + 2, towerTop + 2, towerWidth, towerHeight, cornerRadius)
            Using shadowBrush As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
                g.FillPath(shadowBrush, shadowPath)
            End Using
        End Using

        ' Draw tower background with subtle gradient
        Using gradBrush As New LinearGradientBrush(
            New Point(x, towerTop),
            New Point(x + towerWidth, towerTop),
            Color.FromArgb(240, 240, 240),
            Color.FromArgb(220, 220, 220))
            g.FillPath(gradBrush, towerPath)
        End Using

        ' Draw tower border
        Using borderPen As New Pen(Color.FromArgb(160, 160, 160), 1.5F)
            g.DrawPath(borderPen, towerPath)
        End Using

        ' Draw water with gradient if there's any water
        If waterHeight > 0 Then
            Dim waterTop As Integer = towerBottom - waterHeight
            Dim waterPath As GraphicsPath = CreateRoundedRectangle(
                x + 2,
                waterTop,
                towerWidth - 4,
                waterHeight - 2,
                Math.Max(1, cornerRadius - 2))

            ' Water gradient (lighter at top, darker at bottom)
            Using waterGrad As New LinearGradientBrush(
                New Point(x, waterTop),
                New Point(x, towerBottom),
                Color.FromArgb(135, 206, 235), ' Light sky blue
                Color.FromArgb(70, 130, 180))  ' Steel blue
                g.FillPath(waterGrad, waterPath)
            End Using

            ' Add subtle highlight to water surface
            If waterHeight > 5 Then
                Using highlightBrush As New SolidBrush(Color.FromArgb(80, 255, 255, 255))
                    Dim highlightRect As New Rectangle(x + 2, waterTop, towerWidth - 4, 3)
                    g.FillRectangle(highlightBrush, highlightRect)
                End Using
            End If

            ' Draw water border
            Using waterPen As New Pen(Color.FromArgb(100, 70, 130, 180), 1.0F)
                g.DrawPath(waterPen, waterPath)
            End Using
        End If

        ' Add subtle inner glow to tower
        Using innerGlowPen As New Pen(Color.FromArgb(40, 255, 255, 255), 2.0F)
            Dim innerPath As GraphicsPath = CreateRoundedRectangle(
                x + 2,
                towerTop + 2,
                towerWidth - 4,
                towerHeight - 4,
                Math.Max(1, cornerRadius - 1))
            g.DrawPath(innerGlowPen, innerPath)
        End Using
    End Sub

    Private Shared Function CreateRoundedRectangle(x As Integer, y As Integer, width As Integer, height As Integer, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim diameter As Integer = radius * 2

        ' Ensure we have positive dimensions
        If width <= 0 OrElse height <= 0 Then
            path.AddRectangle(New Rectangle(x, y, Math.Max(1, width), Math.Max(1, height)))
            Return path
        End If

        ' Top-left corner
        path.AddArc(x, y, diameter, diameter, 180, 90)
        ' Top-right corner
        path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90)
        ' Bottom-right corner
        path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90)
        ' Bottom-left corner
        path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90)
        path.CloseFigure()

        Return path
    End Function

    Private Sub DrawTowerLabels(g As Graphics, x As Integer, towerWidth As Integer, towerBottom As Integer, value As Single, label As String, maxScale As Single)
        Using fmt As New StringFormat()
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Near

            ' Amount text with shadow
            Dim amountText As String = String.Format("{0:0.##} {1}", value, _units)
            Using fontAmount As New Font(Me.Font.FontFamily, Math.Max(8, Me.Font.Size), FontStyle.Bold)
                Dim amtSize = g.MeasureString(amountText, fontAmount)
                Dim amtX As Single = x + towerWidth / 2.0F
                Dim amtY As Single = towerBottom + 2

                ' Shadow
                Using shadowBrush As New SolidBrush(Color.FromArgb(30, 0, 0, 0))
                    g.DrawString(amountText, fontAmount, shadowBrush, amtX + 1, amtY + 1, fmt)
                End Using

                ' Main text
                Using amountBrush As New SolidBrush(Color.FromArgb(40, 40, 40))
                    g.DrawString(amountText, fontAmount, amountBrush, amtX, amtY, fmt)
                End Using

                ' Category label
                Using fontLabel As New Font(Me.Font.FontFamily, Math.Max(7, Me.Font.Size - 1), FontStyle.Regular)
                    Using labelBrush As New SolidBrush(Color.FromArgb(120, 120, 120))
                        Dim lblY As Single = amtY + amtSize.Height - 2
                        g.DrawString(label, fontLabel, labelBrush, amtX, lblY, fmt)
                    End Using
                End Using
            End Using

            ' Max scale at top (small, subtle)
            Dim maxText As String = String.Format("{0:0.#} {1}", maxScale, _units)
            Using fontMax As New Font(Me.Font.FontFamily, Math.Max(6, Me.Font.Size - 2), FontStyle.Regular)
                Using maxBrush As New SolidBrush(Color.FromArgb(140, 140, 140))
                    Dim maxSize = g.MeasureString(maxText, fontMax)
                    Dim maxX As Single = x + towerWidth / 2.0F
                    Dim maxY As Single = Math.Max(2, 22 - maxSize.Height)
                    g.DrawString(maxText, fontMax, maxBrush, maxX, maxY, fmt)
                End Using
            End Using
        End Using
    End Sub

End Class