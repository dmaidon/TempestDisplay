Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class SkyConditionsPanel
    Inherits Control

    Private _cloudBaseHeight As Single = 0
    Private _visibility As Single = 10.0F
    Private _isDaytime As Boolean = True

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CloudBaseHeight As Single
        Get
            Return _cloudBaseHeight
        End Get
        Set(value As Single)
            _cloudBaseHeight = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property VisibilityMiles As Single
        Get
            Return _visibility
        End Get
        Set(value As Single)
            _visibility = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Browsable(True)>
    <Category("Appearance")>
    <DefaultValue(True)>
    Public Property IsDaytime As Boolean
        Get
            Return _isDaytime
        End Get
        Set(value As Boolean)
            _isDaytime = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(200, 150)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        DrawSky(g, Me.ClientRectangle)
        DrawClouds(g, Me.ClientRectangle)
        DrawSun(g, Me.ClientRectangle)
        DrawHorizon(g, Me.ClientRectangle)
        DrawData(g, Me.ClientRectangle)
    End Sub

    Private Sub DrawSky(g As Graphics, rect As Rectangle)
        Using skyBrush As New LinearGradientBrush(rect, If(_isDaytime, Color.FromArgb(135, 206, 250), Color.FromArgb(25, 25, 112)),
                                                   If(_isDaytime, Color.FromArgb(200, 230, 255), Color.FromArgb(10, 10, 50)), 90.0F)
            g.FillRectangle(skyBrush, rect)
        End Using
    End Sub

    Private Sub DrawClouds(g As Graphics, rect As Rectangle)
        Dim cloudY = rect.Height * 0.25F
        Using cloudBrush As New SolidBrush(Color.FromArgb(150, 240, 240, 245))
            DrawCloud(g, rect.Width * 0.2F, cloudY, 40)
            DrawCloud(g, rect.Width * 0.5F, cloudY + 10, 35)
            DrawCloud(g, rect.Width * 0.8F, cloudY - 5, 30)
        End Using
    End Sub

    Private Shared Sub DrawCloud(g As Graphics, x As Single, y As Single, size As Single)
        g.FillEllipse(Brushes.White, x, y, size, size * 0.6F)
        g.FillEllipse(Brushes.White, x + size * 0.4F, y - size * 0.2F, size * 0.8F, size * 0.6F)
        g.FillEllipse(Brushes.White, x + size * 0.6F, y, size * 0.7F, size * 0.5F)
    End Sub

    Private Sub DrawSun(g As Graphics, rect As Rectangle)
        If Not _isDaytime Then Return
        Dim sunX = rect.Width * 0.8F
        Dim sunY = rect.Height * 0.25F
        Using sunBrush As New PathGradientBrush(New PointF() {New PointF(sunX, sunY)})
            sunBrush.CenterColor = Color.FromArgb(255, 255, 150)
            sunBrush.SurroundColors = {Color.FromArgb(0, 255, 255, 100)}
            g.FillEllipse(sunBrush, sunX - 30, sunY - 30, 60, 60)
        End Using
    End Sub

    Private Shared Sub DrawHorizon(g As Graphics, rect As Rectangle)
        Dim horizonY = rect.Height * 0.65F
        Using groundBrush As New SolidBrush(Color.FromArgb(100, 150, 100))
            g.FillRectangle(groundBrush, 0, horizonY, rect.Width, rect.Height - horizonY)
        End Using
        Using horizonPen As New Pen(Color.FromArgb(80, 120, 80), 2)
            g.DrawLine(horizonPen, 0, horizonY, rect.Width, horizonY)
        End Using
    End Sub

    Private Sub DrawData(g As Graphics, rect As Rectangle)
        Using font As New Font("Segoe UI", 9, FontStyle.Regular)
            Using brush As New SolidBrush(Color.FromArgb(255, 255, 255))
                Dim dataY = rect.Height * 0.7F
                g.DrawString($"Cloud Base: {If(_cloudBaseHeight > 0, _cloudBaseHeight.ToString("0") & " ft AGL", "N/A")}", font, brush, 10, dataY)
                g.DrawString($"Visibility: {_visibility:0.0} mi", font, brush, 10, dataY + 20)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub
End Class
