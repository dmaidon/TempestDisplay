Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class SolarUvCombinedMeter
    Inherits Control

    Private ReadOnly _uv As New UVIndexMeter()
    Private ReadOnly _solar As New SolarEnergyMeter()

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(200, 140)

        ' Compose existing controls vertically: UV on top, Solar below
        _uv.Margin = New Padding(0)
        _solar.Margin = New Padding(0)
        _uv.Dock = DockStyle.None
        _solar.Dock = DockStyle.None

        Controls.Add(_uv)
        Controls.Add(_solar)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        LayoutChildren()
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        _uv.Font = Me.Font
        _solar.Font = Me.Font
    End Sub

    Private Sub LayoutChildren()
        ' Allocate top region to UV, bottom to Solar. UV on top.
        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height

        ' Ratio: UV ~60%, Solar ~40% (adjustable if needed)
        Dim uvHeight As Integer = CInt(h * 0.6)
        Dim solarHeight As Integer = h - uvHeight

        _uv.SetBounds(0, 0, w, uvHeight)
        _solar.SetBounds(0, uvHeight, w, solarHeight)
    End Sub

    ' Forwarded properties to simplify external usage

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(0.0F)>
    <Description("UV index value shown in the upper meter.")>
    Public Property UvIndex As Single
        Get
            Return If(_uv IsNot Nothing, _uv.UvIndex, 0F)
        End Get
        Set(value As Single)
            If _uv IsNot Nothing Then
                _uv.UvIndex = value
            End If
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(0.0F)>
    <Description("Solar radiation (W/m²) shown in the lower meter.")>
    Public Property SolarRadiation As Single
        Get
            Return If(_solar IsNot Nothing, _solar.SolarRadiation, 0F)
        End Get
        Set(value As Single)
            If _solar IsNot Nothing Then
                _solar.SolarRadiation = value
            End If
        End Set
    End Property

    <Browsable(True)>
    <Category("Data")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <DefaultValue(1200.0F)>
    <Description("Maximum solar radiation scale for the lower meter.")>
    Public Property MaxRadiation As Single
        Get
            Return If(_solar IsNot Nothing, _solar.MaxRadiation, 1200.0F)
        End Get
        Set(value As Single)
            If _solar IsNot Nothing Then
                _solar.MaxRadiation = value
            End If
        End Set
    End Property

    ' Appearance passthroughs
    <Browsable(True)>
    <Category("Appearance")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            MyBase.BackColor = value
            If _uv IsNot Nothing Then _uv.BackColor = value
            If _solar IsNot Nothing Then _solar.BackColor = value
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        ' No custom painting; children render themselves
    End Sub

End Class
