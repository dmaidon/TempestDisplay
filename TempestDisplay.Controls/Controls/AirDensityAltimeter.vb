Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class AirDensityAltimeter
    Inherits Control

    Private _airDensity As Single = 1.2F
    Private _densityAltitude As Single = 0
    Private _category As String = "Average"

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property AirDensity As Single
        Get
            Return _airDensity
        End Get
        Set(value As Single)
            _airDensity = Math.Max(0.8F, Math.Min(1.4F, value))
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DensityAltitude As Single
        Get
            Return _densityAltitude
        End Get
        Set(value As Single)
            _densityAltitude = value
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DefaultValue("Average")>
    Public Property Category As String
        Get
            Return _category
        End Get
        Set(value As String)
            _category = value
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

        Dim cx = Me.Width / 2.0F
        Dim cy = Me.Height / 2.0F
        Dim radius = Math.Min(Me.Width, Me.Height) / 2.0F - 30

        ' Draw title at top
        DrawTitle(g, cx, 15)

        DrawAltimeterFace(g, cx, cy, radius)
        DrawDensityValue(g, cx, cy - radius * 0.3F, _airDensity)
        DrawCategory(g, cx, cy + radius * 0.2F, _category)
        DrawDensityAltitude(g, cx, cy + radius * 0.5F, _densityAltitude)
    End Sub

    Private Shared Sub DrawAltimeterFace(g As Graphics, cx As Single, cy As Single, radius As Single)
        ' Create a circular path for the gradient brush
        Using path As New GraphicsPath()
            path.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2)
            Using bgBrush As New PathGradientBrush(path)
                bgBrush.CenterPoint = New PointF(cx, cy)
                bgBrush.CenterColor = Color.FromArgb(245, 250, 255)
                bgBrush.SurroundColors = {Color.FromArgb(215, 225, 235)}
                g.FillPath(bgBrush, path)
            End Using
        End Using
        Using outlinePen As New Pen(Color.FromArgb(100, 100, 105), 2.5F)
            g.DrawEllipse(outlinePen, cx - radius, cy - radius, radius * 2, radius * 2)
        End Using
    End Sub

    Private Shared Sub DrawDensityValue(g As Graphics, cx As Single, cy As Single, density As Single)
        Using font As New Font("Segoe UI", 14, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(30, 105, 210))
                Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString($"{density:0.00} kg/m³", font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawCategory(g As Graphics, cx As Single, cy As Single, category As String)
        Dim catColor = If(category = "Dense" OrElse category = "Above Average", Color.FromArgb(220, 180, 50),
                         If(category = "Thin" OrElse category = "Below Average", Color.FromArgb(100, 150, 220), Color.FromArgb(100, 180, 100)))
        Using font As New Font("Segoe UI", 10, FontStyle.Bold)
            Using brush As New SolidBrush(catColor)
                Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString(category, font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawDensityAltitude(g As Graphics, cx As Single, cy As Single, da As Single)
        Using font As New Font("Segoe UI", 9, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(80, 80, 80))
                Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString($"DA: {If(da >= 0, "+", "")}{da:0} ft", font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawTitle(g As Graphics, cx As Single, cy As Single)
        Using font As New Font("Segoe UI", 10, FontStyle.Bold)
            Using brush As New SolidBrush(Color.FromArgb(60, 60, 60))
                Using fmt As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    g.DrawString("Air Density Altimeter", font, brush, cx, cy, fmt)
                End Using
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

End Class