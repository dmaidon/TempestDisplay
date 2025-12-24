Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class CloudBasePanel
    Inherits Control

    Private _cloudBaseFeet As Single = 0
    Private _stationElevationFeet As Single = 0

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CloudBaseFeet As Single
        Get
            Return _cloudBaseFeet
        End Get
        Set(value As Single)
            _cloudBaseFeet = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property StationElevationFeet As Single
        Get
            Return _stationElevationFeet
        End Get
        Set(value As Single)
            _stationElevationFeet = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        MinimumSize = New Size(150, 100)
        BackColor = Color.AntiqueWhite
        Margin = New Padding(0)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim w = ClientSize.Width
        Dim h = ClientSize.Height

        ' Use client rect to contain drawing strictly within bounds
        Dim clientRect As New Rectangle(0, 0, w, h)

        ' Sky background
        Using skyBrush As New LinearGradientBrush(clientRect, Color.SkyBlue, Color.LightSteelBlue, LinearGradientMode.Vertical)
            g.FillRectangle(skyBrush, clientRect)
        End Using

        ' Ground
        Using groundBrush As New SolidBrush(Color.ForestGreen)
            g.FillRectangle(groundBrush, clientRect.Left, CInt(h * 0.75), clientRect.Width, CInt(h * 0.25))
        End Using

        ' Scale mapping: show up to 10,000 ft cloud base (auto clamps)
        Dim maxFeet As Single = 10000.0F
        Dim cb = Math.Min(_cloudBaseFeet, maxFeet)
        Dim cloudY As Single = CSng((1.0F - (cb / maxFeet)) * (h * 0.75)) + 10.0F

        ' Cloud layer
        DrawCloudLayer(g, clientRect, cloudY)

        ' Tower marker representing station elevation
        Dim towerBaseY As Integer = CInt(h * 0.75)
        Dim towerHeight As Integer = Math.Min(CInt((_stationElevationFeet / maxFeet) * (h * 0.75)), CInt(h * 0.6))
        Using towerBrush As New SolidBrush(Color.DarkSlateGray)
            Dim towerX As Integer = CInt(clientRect.Left + clientRect.Width * 0.1)
            g.FillRectangle(towerBrush, towerX, towerBaseY - towerHeight, 8, towerHeight)
        End Using

        ' Readouts
        Using fontTitle As New Font("Segoe UI", 9.0F, FontStyle.Bold)
            Using fontInfo As New Font("Segoe UI", 8.0F, FontStyle.Regular)
                Using txtBrush As New SolidBrush(Color.White)
                    Dim title = "Cloud Base"
                    g.DrawString(title, fontTitle, txtBrush, clientRect.Left + clientRect.Width / 2.0F, 6.0F, New StringFormat() With {.Alignment = StringAlignment.Center})
                    g.DrawString($"{_cloudBaseFeet:0} ft MSL", fontInfo, txtBrush, clientRect.Left + clientRect.Width / 2.0F, 24.0F, New StringFormat() With {.Alignment = StringAlignment.Center})
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub DrawCloudLayer(g As Graphics, bounds As Rectangle, y As Single)
        Using cloudBrush As New SolidBrush(Color.FromArgb(240, 245, 250))
            Dim cloudWidth As Single = bounds.Width * 0.8F
            Dim cloudHeight As Single = Math.Max(16.0F, bounds.Width * 0.06F)
            Dim x As Single = bounds.Left + (bounds.Width - cloudWidth) / 2.0F
            Dim rect As New RectangleF(x, y, cloudWidth, cloudHeight)
            g.FillEllipse(cloudBrush, rect)
            g.FillEllipse(cloudBrush, rect.X - cloudWidth * 0.15F, y + 6, cloudWidth * 0.5F, cloudHeight * 0.8F)
            g.FillEllipse(cloudBrush, rect.X + cloudWidth * 0.65F, y + 4, cloudWidth * 0.45F, cloudHeight * 0.9F)
        End Using

        ' Thin dashed line to indicate altitude
        Using pen As New Pen(Color.WhiteSmoke, 1.5F)
            pen.DashStyle = DashStyle.Dot
            g.DrawLine(pen, bounds.Left + 10, y + 2, bounds.Right - 10, y + 2)
        End Using
    End Sub

End Class