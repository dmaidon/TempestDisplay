Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

<DefaultEvent("Click")>
Public Class TrendStatusCombinedControl
    Inherits Control

    Private ReadOnly _trend As New TrendArrowsControl()
    Private ReadOnly _status As New StatusLEDPanel()

    ' Reserve a small right-side gutter to visually separate from the next control
    Private Const RightBoundaryGutter As Integer = 6

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.MinimumSize = New Size(200, 140)

        ' Provide spacing to the right so adjacent controls don't visually run together
        Me.Margin = New Padding(0, 0, RightBoundaryGutter, 0)

        _trend.Margin = New Padding(0)
        _status.Margin = New Padding(0)
        _trend.Dock = DockStyle.None
        _status.Dock = DockStyle.None

        Controls.Add(_trend)
        Controls.Add(_status)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        LayoutChildren()
        Invalidate()
    End Sub

    Private Sub LayoutChildren()
        Dim w = Me.ClientSize.Width
        Dim h = Me.ClientSize.Height

        ' Split 50/50 with a 2px gap for the separator line
        Dim topHeight As Integer = h \ 2 - 1
        Dim bottomTop As Integer = topHeight + 2
        Dim bottomHeight As Integer = h - bottomTop

        Dim contentWidth As Integer = Math.Max(0, w - RightBoundaryGutter)

        _trend.SetBounds(0, 0, contentWidth, topHeight)
        _status.SetBounds(0, bottomTop, contentWidth, bottomHeight)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        Dim w = Me.ClientSize.Width
        Dim h = Me.ClientSize.Height

        ' Crisp 2px separator centered and fully inside the control to avoid bleeding
        Dim separatorTop As Integer = h \ 2 - 1
        Dim length As Integer = Math.Max(0, w \ 2) ' half the control width
        Dim startX As Integer = (w - length) \ 2   ' center horizontally

        e.Graphics.SmoothingMode = SmoothingMode.None

        Using brush As New SolidBrush(Color.FromArgb(120, 120, 120))
            e.Graphics.FillRectangle(brush, startX, separatorTop, length, 2)
        End Using

        ' Draw a subtle right-side boundary line to visually separate this control
        ' and keep content away from the next control to the right.
        Dim boundaryX As Integer = Math.Max(0, w - RightBoundaryGutter)
        Using boundaryPen As New Pen(Color.FromArgb(170, 170, 170), 1)
            e.Graphics.DrawLine(boundaryPen, boundaryX, 0, boundaryX, h)
        End Using
    End Sub

    ' Forwarding helpers
    <Browsable(False)>
    Public ReadOnly Property TrendControl As TrendArrowsControl
        Get
            Return _trend
        End Get
    End Property

    <Browsable(False)>
    Public ReadOnly Property StatusPanel As StatusLEDPanel
        Get
            Return _status
        End Get
    End Property

    Public Sub SetTrend(metric As String, direction As Integer)
        _trend.SetTrend(metric, direction)
    End Sub

    Public Sub SetStatus(label As String, status As StatusLEDPanel.LEDStatus)
        _status.SetStatus(label, status)
    End Sub

End Class