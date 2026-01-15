Imports TempestDisplay.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New DataVisualization.Charting.Series()
        Dim Title1 As System.Windows.Forms.DataVisualization.Charting.Title = New DataVisualization.Charting.Title()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmMain))
        SsBottom = New StatusStrip()
        TsslVer = New ToolStripStatusLabel()
        TsslMidnightCountdown = New ToolStripStatusLabel()
        TsslCpy = New ToolStripStatusLabel()
        TsslClock = New ToolStripStatusLabel()
        SsTop = New StatusStrip()
        TsslTimesrun = New ToolStripStatusLabel()
        TsslMessages = New ToolStripStatusLabel()
        TsslErrCount = New ToolStripStatusLabel()
        Tc = New TabControl()
        TpData = New TabPage()
        PnlData = New Panel()
        TlpData = New TableLayoutPanel()
        TlpTemp = New TableLayoutPanel()
        Label26 = New Label()
        LblCurTemp = New Label()
        Label27 = New Label()
        LblCurWinds = New Label()
        TpExtraData = New TabPage()
        TlpExtraData = New TableLayoutPanel()
        DgvObsSt = New DataGridView()
        TlpPrecipLight = New TableLayoutPanel()
        Label13 = New Label()
        Label14 = New Label()
        TxtRainTodayMinutes = New TextBox()
        TxtRainYesterdayMinutes = New TextBox()
        Label15 = New Label()
        LblLightningDataTitle = New Label()
        Label17 = New Label()
        Label18 = New Label()
        Label19 = New Label()
        Label20 = New Label()
        LblLightLastStrike = New Label()
        TxtStrikeCount = New TextBox()
        TxtLightHrCount = New TextBox()
        TxtLight3hrCount = New TextBox()
        TxtLightDistance = New TextBox()
        TableLayoutPanel1 = New TableLayoutPanel()
        LblAirDensity = New Label()
        LblAirDensityCat = New Label()
        LblCloudBase = New Label()
        LblIP = New Label()
        LblUpdate = New Label()
        LblBatteryStatus = New Label()
        TlpWindSun = New TableLayoutPanel()
        LblPressTrend = New Label()
        LblBaroPress = New Label()
        LblBrightness = New Label()
        LblSolRad = New Label()
        LblUV = New Label()
        LblWindDir = New Label()
        LblWindLull = New Label()
        LblWindGust = New Label()
        LblAvgWindSpd = New Label()
        LblWindSpd = New Label()
        DgvHubStatus = New DataGridView()
        TpRecords = New TabPage()
        DgvRecords = New DataGridView()
        rRain = New DataGridViewTextBoxColumn()
        tTemp = New DataGridViewTextBoxColumn()
        wWind = New DataGridViewTextBoxColumn()
        TpCharts = New TabPage()
        ChtBattery = New DataVisualization.Charting.Chart()
        TpLogs = New TabPage()
        TcLogs = New TabControl()
        TpLogFiles = New TabPage()
        Panel2 = New Panel()
        BtnFindNext = New Button()
        BtnFind = New Button()
        TxtLogSearch = New TextBox()
        LbLogs = New ListBox()
        PnlLogs = New Panel()
        Label24 = New Label()
        RtbLogs = New RichTextBox()
        TpSpares = New TabPage()
        TpHelp = New TabPage()
        PnlHelp = New Panel()
        BtnHelpExport = New Button()
        BtnHelpPrint = New Button()
        TxtHelpSearch = New TextBox()
        Label25 = New Label()
        ScHelp = New SplitContainer()
        TvHelp = New TreeView()
        RtbHelp = New RichTextBox()
        TpSettings = New TabPage()
        PnlSettings = New Panel()
        GbStationData = New GroupBox()
        TxtStationElevation = New TextBox()
        Label16 = New Label()
        TxtStationID = New TextBox()
        TxtStationName = New TextBox()
        Label23 = New Label()
        Label22 = New Label()
        GbLogSettings = New GroupBox()
        TxtLogDays = New TextBox()
        Label21 = New Label()
        Panel1 = New Panel()
        Label12 = New Label()
        RtbSettings = New RichTextBox()
        GroupBox1 = New GroupBox()
        TlpRainGaugeLimits = New TableLayoutPanel()
        Label7 = New Label()
        Label8 = New Label()
        Label9 = New Label()
        Label10 = New Label()
        Label11 = New Label()
        NudRainTodayLimit = New NumericUpDown()
        NudRainYdayLimit = New NumericUpDown()
        NumRainMonthLimit = New NumericUpDown()
        NudRainYearLimit = New NumericUpDown()
        NudRainAlltimeLimit = New NumericUpDown()
        GbTempestSettings = New GroupBox()
        NudTempestInterval = New NumericUpDown()
        TxtApiToken = New TextBox()
        TxtDeviceID = New TextBox()
        Label6 = New Label()
        Label5 = New Label()
        Label4 = New Label()
        GbMeteoBridge = New GroupBox()
        TxtIp = New TextBox()
        TxtPassword = New TextBox()
        TxtLogin = New TextBox()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        TmrClock = New Timer(components)
        SsObs_St = New StatusStrip()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        TsslObs_St = New ToolStripStatusLabel()
        TTip = New ToolTip(components)
        SsBottom.SuspendLayout()
        SsTop.SuspendLayout()
        Tc.SuspendLayout()
        TpData.SuspendLayout()
        PnlData.SuspendLayout()
        TlpData.SuspendLayout()
        TlpTemp.SuspendLayout()
        TpExtraData.SuspendLayout()
        TlpExtraData.SuspendLayout()
        CType(DgvObsSt, ComponentModel.ISupportInitialize).BeginInit()
        TlpPrecipLight.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        TlpWindSun.SuspendLayout()
        CType(DgvHubStatus, ComponentModel.ISupportInitialize).BeginInit()
        TpRecords.SuspendLayout()
        CType(DgvRecords, ComponentModel.ISupportInitialize).BeginInit()
        TpCharts.SuspendLayout()
        CType(ChtBattery, ComponentModel.ISupportInitialize).BeginInit()
        TpLogs.SuspendLayout()
        TcLogs.SuspendLayout()
        TpLogFiles.SuspendLayout()
        Panel2.SuspendLayout()
        PnlLogs.SuspendLayout()
        TpHelp.SuspendLayout()
        PnlHelp.SuspendLayout()
        CType(ScHelp, ComponentModel.ISupportInitialize).BeginInit()
        ScHelp.Panel1.SuspendLayout()
        ScHelp.Panel2.SuspendLayout()
        ScHelp.SuspendLayout()
        TpSettings.SuspendLayout()
        PnlSettings.SuspendLayout()
        GbStationData.SuspendLayout()
        GbLogSettings.SuspendLayout()
        Panel1.SuspendLayout()
        GroupBox1.SuspendLayout()
        TlpRainGaugeLimits.SuspendLayout()
        CType(NudRainTodayLimit, ComponentModel.ISupportInitialize).BeginInit()
        CType(NudRainYdayLimit, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumRainMonthLimit, ComponentModel.ISupportInitialize).BeginInit()
        CType(NudRainYearLimit, ComponentModel.ISupportInitialize).BeginInit()
        CType(NudRainAlltimeLimit, ComponentModel.ISupportInitialize).BeginInit()
        GbTempestSettings.SuspendLayout()
        CType(NudTempestInterval, ComponentModel.ISupportInitialize).BeginInit()
        GbMeteoBridge.SuspendLayout()
        SsObs_St.SuspendLayout()
        SuspendLayout()
        ' 
        ' SsBottom
        ' 
        SsBottom.BackColor = Color.AliceBlue
        SsBottom.GripMargin = New Padding(0)
        SsBottom.ImageScalingSize = New Size(24, 24)
        SsBottom.Items.AddRange(New ToolStripItem() {TsslVer, TsslMidnightCountdown, TsslCpy, TsslClock})
        SsBottom.Location = New Point(0, 1056)
        SsBottom.Name = "SsBottom"
        SsBottom.Size = New Size(1563, 32)
        SsBottom.SizingGrip = False
        SsBottom.TabIndex = 0
        ' 
        ' TsslVer
        ' 
        TsslVer.Name = "TsslVer"
        TsslVer.Size = New Size(21, 25)
        TsslVer.Text = "v"
        ' 
        ' TsslMidnightCountdown
        ' 
        TsslMidnightCountdown.Font = New Font("Segoe UI", 8F, FontStyle.Bold Or FontStyle.Italic, GraphicsUnit.Point, CByte(0))
        TsslMidnightCountdown.ForeColor = Color.ForestGreen
        TsslMidnightCountdown.Name = "TsslMidnightCountdown"
        TsslMidnightCountdown.Size = New Size(35, 25)
        TsslMidnightCountdown.Text = "mn"
        ' 
        ' TsslCpy
        ' 
        TsslCpy.Font = New Font("Segoe UI", 7F, FontStyle.Italic)
        TsslCpy.ForeColor = Color.Brown
        TsslCpy.Name = "TsslCpy"
        TsslCpy.Size = New Size(1474, 25)
        TsslCpy.Spring = True
        TsslCpy.Text = "cpy"
        ' 
        ' TsslClock
        ' 
        TsslClock.Name = "TsslClock"
        TsslClock.Size = New Size(18, 25)
        TsslClock.Text = "t"
        ' 
        ' SsTop
        ' 
        SsTop.BackColor = Color.FloralWhite
        SsTop.GripMargin = New Padding(0)
        SsTop.ImageScalingSize = New Size(24, 24)
        SsTop.Items.AddRange(New ToolStripItem() {TsslTimesrun, TsslMessages, TsslErrCount})
        SsTop.Location = New Point(0, 1024)
        SsTop.Name = "SsTop"
        SsTop.Size = New Size(1563, 32)
        SsTop.SizingGrip = False
        SsTop.TabIndex = 1
        ' 
        ' TsslTimesrun
        ' 
        TsslTimesrun.Font = New Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TsslTimesrun.ForeColor = Color.FromArgb(CByte(192), CByte(0), CByte(0))
        TsslTimesrun.Name = "TsslTimesrun"
        TsslTimesrun.Size = New Size(22, 25)
        TsslTimesrun.Text = "tr"
        TsslTimesrun.ToolTipText = "# times app started"
        ' 
        ' TsslMessages
        ' 
        TsslMessages.Name = "TsslMessages"
        TsslMessages.Size = New Size(1458, 25)
        TsslMessages.Spring = True
        TsslMessages.Text = "-"
        ' 
        ' TsslErrCount
        ' 
        TsslErrCount.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        TsslErrCount.ForeColor = Color.ForestGreen
        TsslErrCount.Name = "TsslErrCount"
        TsslErrCount.Size = New Size(22, 25)
        TsslErrCount.Text = "0"
        TsslErrCount.ToolTipText = "# Errors in App"
        ' 
        ' Tc
        ' 
        Tc.Controls.Add(TpData)
        Tc.Controls.Add(TpExtraData)
        Tc.Controls.Add(TpRecords)
        Tc.Controls.Add(TpCharts)
        Tc.Controls.Add(TpLogs)
        Tc.Controls.Add(TpHelp)
        Tc.Controls.Add(TpSettings)
        Tc.Dock = DockStyle.Top
        Tc.Location = New Point(0, 0)
        Tc.Name = "Tc"
        Tc.SelectedIndex = 0
        Tc.Size = New Size(1563, 984)
        Tc.TabIndex = 2
        ' 
        ' TpData
        ' 
        TpData.BorderStyle = BorderStyle.Fixed3D
        TpData.Controls.Add(PnlData)
        TpData.Location = New Point(4, 30)
        TpData.Name = "TpData"
        TpData.Padding = New Padding(3)
        TpData.Size = New Size(1555, 950)
        TpData.TabIndex = 0
        TpData.Text = "Data"
        TpData.UseVisualStyleBackColor = True
        ' 
        ' PnlData
        ' 
        PnlData.BackColor = Color.SeaShell
        PnlData.BorderStyle = BorderStyle.Fixed3D
        PnlData.Controls.Add(TlpData)
        PnlData.Dock = DockStyle.Fill
        PnlData.Location = New Point(3, 3)
        PnlData.Name = "PnlData"
        PnlData.Size = New Size(1545, 940)
        PnlData.TabIndex = 1
        ' 
        ' TlpData
        ' 
        TlpData.AutoSizeMode = AutoSizeMode.GrowAndShrink
        TlpData.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble
        TlpData.ColumnCount = 8
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpData.Controls.Add(TlpTemp, 0, 1)
        TlpData.Dock = DockStyle.Top
        TlpData.Location = New Point(0, 0)
        TlpData.Name = "TlpData"
        TlpData.RowCount = 3
        TlpData.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TlpData.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TlpData.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TlpData.Size = New Size(1541, 937)
        TlpData.TabIndex = 5
        ' 
        ' TlpTemp
        ' 
        TlpTemp.BackColor = Color.Gainsboro
        TlpTemp.ColumnCount = 2
        TlpData.SetColumnSpan(TlpTemp, 2)
        TlpTemp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TlpTemp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TlpTemp.Controls.Add(Label26, 0, 0)
        TlpTemp.Controls.Add(LblCurTemp, 0, 1)
        TlpTemp.Controls.Add(Label27, 0, 2)
        TlpTemp.Controls.Add(LblCurWinds, 0, 3)
        TlpTemp.Dock = DockStyle.Fill
        TlpTemp.Location = New Point(6, 317)
        TlpTemp.Name = "TlpTemp"
        TlpTemp.RowCount = 4
        TlpTemp.RowStyles.Add(New RowStyle(SizeType.Percent, 19.29881F))
        TlpTemp.RowStyles.Add(New RowStyle(SizeType.Percent, 31.1836586F))
        TlpTemp.RowStyles.Add(New RowStyle(SizeType.Percent, 18.33387F))
        TlpTemp.RowStyles.Add(New RowStyle(SizeType.Percent, 31.1836586F))
        TlpTemp.Size = New Size(375, 302)
        TlpTemp.TabIndex = 5
        ' 
        ' Label26
        ' 
        Label26.AutoSize = True
        TlpTemp.SetColumnSpan(Label26, 2)
        Label26.Dock = DockStyle.Fill
        Label26.Font = New Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label26.Location = New Point(3, 0)
        Label26.Name = "Label26"
        Label26.Size = New Size(369, 58)
        Label26.TabIndex = 33
        Label26.Text = "Current Temperature"
        Label26.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblCurTemp
        ' 
        LblCurTemp.AutoSize = True
        TlpTemp.SetColumnSpan(LblCurTemp, 2)
        LblCurTemp.Dock = DockStyle.Fill
        LblCurTemp.Font = New Font("Segoe UI Black", 24F, FontStyle.Bold Or FontStyle.Italic, GraphicsUnit.Point, CByte(0))
        LblCurTemp.ForeColor = Color.Firebrick
        LblCurTemp.Location = New Point(3, 58)
        LblCurTemp.Name = "LblCurTemp"
        LblCurTemp.Size = New Size(369, 94)
        LblCurTemp.TabIndex = 34
        LblCurTemp.Tag = "{0}°"
        LblCurTemp.Text = "-"
        LblCurTemp.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label27
        ' 
        Label27.AutoSize = True
        TlpTemp.SetColumnSpan(Label27, 2)
        Label27.Dock = DockStyle.Fill
        Label27.Font = New Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label27.Location = New Point(3, 152)
        Label27.Name = "Label27"
        Label27.Size = New Size(369, 55)
        Label27.TabIndex = 35
        Label27.Text = "Winds"
        Label27.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblCurWinds
        ' 
        LblCurWinds.AutoSize = True
        TlpTemp.SetColumnSpan(LblCurWinds, 2)
        LblCurWinds.Dock = DockStyle.Fill
        LblCurWinds.Font = New Font("Segoe UI", 18F, FontStyle.Bold Or FontStyle.Italic, GraphicsUnit.Point, CByte(0))
        LblCurWinds.ForeColor = Color.Purple
        LblCurWinds.Location = New Point(3, 207)
        LblCurWinds.Name = "LblCurWinds"
        LblCurWinds.Size = New Size(369, 95)
        LblCurWinds.TabIndex = 36
        LblCurWinds.Text = "-"
        LblCurWinds.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TpExtraData
        ' 
        TpExtraData.BackColor = Color.Linen
        TpExtraData.BorderStyle = BorderStyle.Fixed3D
        TpExtraData.Controls.Add(TlpExtraData)
        TpExtraData.Location = New Point(4, 34)
        TpExtraData.Name = "TpExtraData"
        TpExtraData.Size = New Size(1555, 946)
        TpExtraData.TabIndex = 5
        TpExtraData.Text = "Extra Data"
        ' 
        ' TlpExtraData
        ' 
        TlpExtraData.ColumnCount = 8
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        TlpExtraData.Controls.Add(DgvObsSt, 2, 0)
        TlpExtraData.Controls.Add(TlpPrecipLight, 0, 0)
        TlpExtraData.Controls.Add(TableLayoutPanel1, 0, 2)
        TlpExtraData.Controls.Add(TlpWindSun, 0, 1)
        TlpExtraData.Controls.Add(DgvHubStatus, 4, 0)
        TlpExtraData.Dock = DockStyle.Fill
        TlpExtraData.Location = New Point(0, 0)
        TlpExtraData.Name = "TlpExtraData"
        TlpExtraData.RowCount = 3
        TlpExtraData.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TlpExtraData.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TlpExtraData.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TlpExtraData.Size = New Size(1551, 942)
        TlpExtraData.TabIndex = 14
        ' 
        ' DgvObsSt
        ' 
        DgvObsSt.AllowUserToAddRows = False
        DgvObsSt.AllowUserToDeleteRows = False
        DgvObsSt.AllowUserToResizeRows = False
        DataGridViewCellStyle1.BackColor = Color.Honeydew
        DgvObsSt.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        DgvObsSt.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle2.BackColor = Color.PaleTurquoise
        DataGridViewCellStyle2.Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle2.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.True
        DgvObsSt.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        DgvObsSt.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        TlpExtraData.SetColumnSpan(DgvObsSt, 2)
        DgvObsSt.Location = New Point(389, 3)
        DgvObsSt.MultiSelect = False
        DgvObsSt.Name = "DgvObsSt"
        DgvObsSt.ReadOnly = True
        DgvObsSt.RowHeadersVisible = False
        DgvObsSt.RowHeadersWidth = 62
        DgvObsSt.Size = New Size(378, 307)
        DgvObsSt.TabIndex = 33
        TTip.SetToolTip(DgvObsSt, "Observation Values (Obs_St)")
        ' 
        ' TlpPrecipLight
        ' 
        TlpPrecipLight.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        TlpPrecipLight.ColumnCount = 2
        TlpExtraData.SetColumnSpan(TlpPrecipLight, 2)
        TlpPrecipLight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70F))
        TlpPrecipLight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30F))
        TlpPrecipLight.Controls.Add(Label13, 0, 0)
        TlpPrecipLight.Controls.Add(Label14, 0, 1)
        TlpPrecipLight.Controls.Add(TxtRainTodayMinutes, 1, 1)
        TlpPrecipLight.Controls.Add(TxtRainYesterdayMinutes, 1, 2)
        TlpPrecipLight.Controls.Add(Label15, 0, 2)
        TlpPrecipLight.Controls.Add(LblLightningDataTitle, 0, 3)
        TlpPrecipLight.Controls.Add(Label17, 0, 4)
        TlpPrecipLight.Controls.Add(Label18, 0, 5)
        TlpPrecipLight.Controls.Add(Label19, 0, 6)
        TlpPrecipLight.Controls.Add(Label20, 0, 7)
        TlpPrecipLight.Controls.Add(LblLightLastStrike, 0, 8)
        TlpPrecipLight.Controls.Add(TxtStrikeCount, 1, 4)
        TlpPrecipLight.Controls.Add(TxtLightHrCount, 1, 5)
        TlpPrecipLight.Controls.Add(TxtLight3hrCount, 1, 6)
        TlpPrecipLight.Controls.Add(TxtLightDistance, 1, 7)
        TlpPrecipLight.Dock = DockStyle.Fill
        TlpPrecipLight.Location = New Point(3, 3)
        TlpPrecipLight.Name = "TlpPrecipLight"
        TlpPrecipLight.RowCount = 9
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.RowStyles.Add(New RowStyle(SizeType.Percent, 11.1111116F))
        TlpPrecipLight.Size = New Size(380, 307)
        TlpPrecipLight.TabIndex = 6
        ' 
        ' Label13
        ' 
        Label13.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label13.AutoSize = True
        Label13.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        TlpPrecipLight.SetColumnSpan(Label13, 2)
        Label13.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label13.Location = New Point(4, 1)
        Label13.Name = "Label13"
        Label13.Size = New Size(372, 32)
        Label13.TabIndex = 0
        Label13.Text = "Precipitation Minutes"
        Label13.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label14
        ' 
        Label14.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label14.AutoSize = True
        Label14.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        Label14.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label14.Location = New Point(4, 34)
        Label14.Name = "Label14"
        Label14.Size = New Size(257, 32)
        Label14.TabIndex = 1
        Label14.Text = "Today"
        Label14.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' TxtRainTodayMinutes
        ' 
        TxtRainTodayMinutes.Dock = DockStyle.Fill
        TxtRainTodayMinutes.Font = New Font("Segoe UI", 8F, FontStyle.Bold)
        TxtRainTodayMinutes.Location = New Point(266, 35)
        TxtRainTodayMinutes.Margin = New Padding(1)
        TxtRainTodayMinutes.MaxLength = 15
        TxtRainTodayMinutes.Name = "TxtRainTodayMinutes"
        TxtRainTodayMinutes.ReadOnly = True
        TxtRainTodayMinutes.Size = New Size(112, 29)
        TxtRainTodayMinutes.TabIndex = 3
        TxtRainTodayMinutes.Tag = "0"
        TxtRainTodayMinutes.TextAlign = HorizontalAlignment.Center
        ' 
        ' TxtRainYesterdayMinutes
        ' 
        TxtRainYesterdayMinutes.Dock = DockStyle.Fill
        TxtRainYesterdayMinutes.Font = New Font("Segoe UI", 8F, FontStyle.Bold)
        TxtRainYesterdayMinutes.Location = New Point(266, 68)
        TxtRainYesterdayMinutes.Margin = New Padding(1)
        TxtRainYesterdayMinutes.MaxLength = 15
        TxtRainYesterdayMinutes.Name = "TxtRainYesterdayMinutes"
        TxtRainYesterdayMinutes.ReadOnly = True
        TxtRainYesterdayMinutes.Size = New Size(112, 29)
        TxtRainYesterdayMinutes.TabIndex = 4
        TxtRainYesterdayMinutes.Tag = "1"
        TxtRainYesterdayMinutes.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label15
        ' 
        Label15.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label15.AutoSize = True
        Label15.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        Label15.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label15.Location = New Point(4, 67)
        Label15.Name = "Label15"
        Label15.Size = New Size(257, 32)
        Label15.TabIndex = 2
        Label15.Text = "Yesterday"
        Label15.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' LblLightningDataTitle
        ' 
        LblLightningDataTitle.AutoSize = True
        LblLightningDataTitle.BackColor = Color.LemonChiffon
        TlpPrecipLight.SetColumnSpan(LblLightningDataTitle, 2)
        LblLightningDataTitle.Dock = DockStyle.Fill
        LblLightningDataTitle.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblLightningDataTitle.ImageAlign = ContentAlignment.MiddleLeft
        LblLightningDataTitle.Location = New Point(4, 100)
        LblLightningDataTitle.Name = "LblLightningDataTitle"
        LblLightningDataTitle.Size = New Size(372, 32)
        LblLightningDataTitle.TabIndex = 5
        LblLightningDataTitle.Text = "Lightning Data"
        LblLightningDataTitle.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label17
        ' 
        Label17.AutoSize = True
        Label17.BackColor = Color.LemonChiffon
        Label17.Dock = DockStyle.Fill
        Label17.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label17.Location = New Point(4, 133)
        Label17.Name = "Label17"
        Label17.Size = New Size(257, 32)
        Label17.TabIndex = 6
        Label17.Text = "Strike Count"
        Label17.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label18
        ' 
        Label18.AutoSize = True
        Label18.BackColor = Color.LemonChiffon
        Label18.Dock = DockStyle.Fill
        Label18.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label18.Location = New Point(4, 166)
        Label18.Name = "Label18"
        Label18.Size = New Size(257, 32)
        Label18.TabIndex = 7
        Label18.Text = "Last Hour Count"
        Label18.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label19
        ' 
        Label19.AutoSize = True
        Label19.BackColor = Color.LemonChiffon
        Label19.Dock = DockStyle.Fill
        Label19.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label19.Location = New Point(4, 199)
        Label19.Name = "Label19"
        Label19.Size = New Size(257, 32)
        Label19.TabIndex = 8
        Label19.Text = "Last 3hr Count"
        Label19.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label20
        ' 
        Label20.AutoSize = True
        Label20.BackColor = Color.LemonChiffon
        Label20.Dock = DockStyle.Fill
        Label20.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        Label20.Location = New Point(4, 232)
        Label20.Name = "Label20"
        Label20.Size = New Size(257, 32)
        Label20.TabIndex = 9
        Label20.Text = "Last Distance (Miles)"
        Label20.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' LblLightLastStrike
        ' 
        LblLightLastStrike.AutoEllipsis = True
        LblLightLastStrike.AutoSize = True
        LblLightLastStrike.BackColor = Color.LemonChiffon
        TlpPrecipLight.SetColumnSpan(LblLightLastStrike, 2)
        LblLightLastStrike.Dock = DockStyle.Fill
        LblLightLastStrike.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblLightLastStrike.Location = New Point(4, 265)
        LblLightLastStrike.Name = "LblLightLastStrike"
        LblLightLastStrike.Size = New Size(372, 41)
        LblLightLastStrike.TabIndex = 10
        LblLightLastStrike.Tag = "Last strike: {0}"
        LblLightLastStrike.Text = "Last Strike"
        LblLightLastStrike.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TxtStrikeCount
        ' 
        TxtStrikeCount.Dock = DockStyle.Fill
        TxtStrikeCount.Font = New Font("Segoe UI", 8F, FontStyle.Bold)
        TxtStrikeCount.Location = New Point(266, 134)
        TxtStrikeCount.Margin = New Padding(1)
        TxtStrikeCount.MaxLength = 15
        TxtStrikeCount.Name = "TxtStrikeCount"
        TxtStrikeCount.ReadOnly = True
        TxtStrikeCount.Size = New Size(112, 29)
        TxtStrikeCount.TabIndex = 11
        TxtStrikeCount.Tag = "0"
        TxtStrikeCount.TextAlign = HorizontalAlignment.Center
        ' 
        ' TxtLightHrCount
        ' 
        TxtLightHrCount.Dock = DockStyle.Fill
        TxtLightHrCount.Font = New Font("Segoe UI", 8F, FontStyle.Bold)
        TxtLightHrCount.Location = New Point(266, 167)
        TxtLightHrCount.Margin = New Padding(1)
        TxtLightHrCount.MaxLength = 15
        TxtLightHrCount.Name = "TxtLightHrCount"
        TxtLightHrCount.ReadOnly = True
        TxtLightHrCount.Size = New Size(112, 29)
        TxtLightHrCount.TabIndex = 12
        TxtLightHrCount.Tag = "1"
        TxtLightHrCount.TextAlign = HorizontalAlignment.Center
        ' 
        ' TxtLight3hrCount
        ' 
        TxtLight3hrCount.Dock = DockStyle.Fill
        TxtLight3hrCount.Font = New Font("Segoe UI", 8F, FontStyle.Bold)
        TxtLight3hrCount.Location = New Point(266, 200)
        TxtLight3hrCount.Margin = New Padding(1)
        TxtLight3hrCount.MaxLength = 15
        TxtLight3hrCount.Name = "TxtLight3hrCount"
        TxtLight3hrCount.ReadOnly = True
        TxtLight3hrCount.Size = New Size(112, 29)
        TxtLight3hrCount.TabIndex = 13
        TxtLight3hrCount.Tag = "2"
        TxtLight3hrCount.TextAlign = HorizontalAlignment.Center
        ' 
        ' TxtLightDistance
        ' 
        TxtLightDistance.Dock = DockStyle.Fill
        TxtLightDistance.Font = New Font("Segoe UI", 8F, FontStyle.Bold)
        TxtLightDistance.Location = New Point(266, 233)
        TxtLightDistance.Margin = New Padding(1)
        TxtLightDistance.MaxLength = 15
        TxtLightDistance.Name = "TxtLightDistance"
        TxtLightDistance.ReadOnly = True
        TxtLightDistance.Size = New Size(112, 29)
        TxtLightDistance.TabIndex = 14
        TxtLightDistance.Tag = "3"
        TxtLightDistance.TextAlign = HorizontalAlignment.Center
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        TableLayoutPanel1.ColumnCount = 2
        TlpExtraData.SetColumnSpan(TableLayoutPanel1, 2)
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.Controls.Add(LblAirDensity, 0, 1)
        TableLayoutPanel1.Controls.Add(LblAirDensityCat, 0, 2)
        TableLayoutPanel1.Controls.Add(LblCloudBase, 0, 0)
        TableLayoutPanel1.Controls.Add(LblIP, 0, 9)
        TableLayoutPanel1.Controls.Add(LblUpdate, 0, 8)
        TableLayoutPanel1.Controls.Add(LblBatteryStatus, 0, 6)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(3, 629)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 10
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TableLayoutPanel1.Size = New Size(380, 310)
        TableLayoutPanel1.TabIndex = 13
        ' 
        ' LblAirDensity
        ' 
        LblAirDensity.AutoSize = True
        LblAirDensity.BackColor = Color.Aqua
        TableLayoutPanel1.SetColumnSpan(LblAirDensity, 2)
        LblAirDensity.Dock = DockStyle.Fill
        LblAirDensity.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblAirDensity.Location = New Point(4, 31)
        LblAirDensity.Name = "LblAirDensity"
        LblAirDensity.Size = New Size(372, 29)
        LblAirDensity.TabIndex = 9
        LblAirDensity.Tag = "Air Density: {0:N3} kg/m3"
        LblAirDensity.Text = "Air Density"
        LblAirDensity.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblAirDensityCat
        ' 
        LblAirDensityCat.AutoSize = True
        LblAirDensityCat.BackColor = Color.Cyan
        TableLayoutPanel1.SetColumnSpan(LblAirDensityCat, 2)
        LblAirDensityCat.Dock = DockStyle.Fill
        LblAirDensityCat.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblAirDensityCat.Location = New Point(2, 62)
        LblAirDensityCat.Margin = New Padding(1)
        LblAirDensityCat.Name = "LblAirDensityCat"
        LblAirDensityCat.Size = New Size(376, 27)
        LblAirDensityCat.TabIndex = 10
        LblAirDensityCat.Tag = "ADC: {0}"
        LblAirDensityCat.Text = "Air Density Category"
        LblAirDensityCat.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblCloudBase
        ' 
        LblCloudBase.AutoSize = True
        LblCloudBase.BackColor = Color.Cyan
        TableLayoutPanel1.SetColumnSpan(LblCloudBase, 2)
        LblCloudBase.Dock = DockStyle.Fill
        LblCloudBase.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblCloudBase.Location = New Point(2, 2)
        LblCloudBase.Margin = New Padding(1)
        LblCloudBase.Name = "LblCloudBase"
        LblCloudBase.Size = New Size(376, 27)
        LblCloudBase.TabIndex = 11
        LblCloudBase.Tag = "Cloud Base: {0} ft"
        LblCloudBase.Text = "Cloud Base"
        LblCloudBase.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblIP
        ' 
        LblIP.BorderStyle = BorderStyle.Fixed3D
        TableLayoutPanel1.SetColumnSpan(LblIP, 2)
        LblIP.Dock = DockStyle.Fill
        LblIP.Location = New Point(4, 271)
        LblIP.Name = "LblIP"
        LblIP.Size = New Size(372, 38)
        LblIP.TabIndex = 15
        LblIP.Text = "ip"
        LblIP.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblUpdate
        ' 
        LblUpdate.BorderStyle = BorderStyle.Fixed3D
        TableLayoutPanel1.SetColumnSpan(LblUpdate, 2)
        LblUpdate.Dock = DockStyle.Fill
        LblUpdate.Location = New Point(4, 241)
        LblUpdate.Name = "LblUpdate"
        LblUpdate.Size = New Size(372, 29)
        LblUpdate.TabIndex = 16
        LblUpdate.Tag = "Updated: {0}"
        LblUpdate.Text = "last update"
        LblUpdate.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblBatteryStatus
        ' 
        LblBatteryStatus.BorderStyle = BorderStyle.Fixed3D
        TableLayoutPanel1.SetColumnSpan(LblBatteryStatus, 2)
        LblBatteryStatus.Dock = DockStyle.Fill
        LblBatteryStatus.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblBatteryStatus.Location = New Point(4, 181)
        LblBatteryStatus.Name = "LblBatteryStatus"
        LblBatteryStatus.Size = New Size(372, 29)
        LblBatteryStatus.TabIndex = 17
        LblBatteryStatus.Tag = "Battery Voltage: {0:N2}v"
        LblBatteryStatus.Text = "Battery Voltage"
        LblBatteryStatus.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TlpWindSun
        ' 
        TlpWindSun.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        TlpWindSun.ColumnCount = 1
        TlpExtraData.SetColumnSpan(TlpWindSun, 2)
        TlpWindSun.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TlpWindSun.Controls.Add(LblPressTrend, 0, 9)
        TlpWindSun.Controls.Add(LblBaroPress, 0, 8)
        TlpWindSun.Controls.Add(LblBrightness, 0, 7)
        TlpWindSun.Controls.Add(LblSolRad, 0, 6)
        TlpWindSun.Controls.Add(LblUV, 0, 5)
        TlpWindSun.Controls.Add(LblWindDir, 0, 4)
        TlpWindSun.Controls.Add(LblWindLull, 0, 3)
        TlpWindSun.Controls.Add(LblWindGust, 0, 2)
        TlpWindSun.Controls.Add(LblAvgWindSpd, 0, 1)
        TlpWindSun.Controls.Add(LblWindSpd, 0, 0)
        TlpWindSun.Dock = DockStyle.Fill
        TlpWindSun.Location = New Point(3, 316)
        TlpWindSun.Name = "TlpWindSun"
        TlpWindSun.RowCount = 10
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.RowStyles.Add(New RowStyle(SizeType.Percent, 10F))
        TlpWindSun.Size = New Size(380, 307)
        TlpWindSun.TabIndex = 7
        ' 
        ' LblPressTrend
        ' 
        LblPressTrend.AutoSize = True
        LblPressTrend.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(255))
        LblPressTrend.Dock = DockStyle.Fill
        LblPressTrend.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblPressTrend.Location = New Point(4, 271)
        LblPressTrend.Name = "LblPressTrend"
        LblPressTrend.Size = New Size(372, 35)
        LblPressTrend.TabIndex = 7
        LblPressTrend.Tag = "Trend: {0}"
        LblPressTrend.Text = "Pressure Trend"
        LblPressTrend.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblBaroPress
        ' 
        LblBaroPress.AutoSize = True
        LblBaroPress.BackColor = Color.FromArgb(CByte(192), CByte(192), CByte(255))
        LblBaroPress.Dock = DockStyle.Fill
        LblBaroPress.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblBaroPress.Location = New Point(4, 241)
        LblBaroPress.Name = "LblBaroPress"
        LblBaroPress.Size = New Size(372, 29)
        LblBaroPress.TabIndex = 6
        LblBaroPress.Tag = "Barometric Pressure: {0:N2} mb"
        LblBaroPress.Text = "Barometric Pressure"
        LblBaroPress.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblBrightness
        ' 
        LblBrightness.AutoSize = True
        LblBrightness.BackColor = Color.Khaki
        LblBrightness.Dock = DockStyle.Fill
        LblBrightness.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblBrightness.Location = New Point(4, 211)
        LblBrightness.Name = "LblBrightness"
        LblBrightness.Size = New Size(372, 29)
        LblBrightness.TabIndex = 8
        LblBrightness.Tag = "Brightness: {0} lux"
        LblBrightness.Text = "Brightness"
        LblBrightness.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblSolRad
        ' 
        LblSolRad.AutoSize = True
        LblSolRad.BackColor = Color.Khaki
        LblSolRad.Dock = DockStyle.Fill
        LblSolRad.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblSolRad.Location = New Point(4, 181)
        LblSolRad.Name = "LblSolRad"
        LblSolRad.Size = New Size(372, 29)
        LblSolRad.TabIndex = 5
        LblSolRad.Tag = "Solar Radiation: {0} W/m2"
        LblSolRad.Text = "Solar Radiation"
        LblSolRad.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblUV
        ' 
        LblUV.AutoSize = True
        LblUV.BackColor = Color.Khaki
        LblUV.Dock = DockStyle.Fill
        LblUV.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblUV.Location = New Point(4, 151)
        LblUV.Name = "LblUV"
        LblUV.Size = New Size(372, 29)
        LblUV.TabIndex = 4
        LblUV.Tag = "UV: {0:N1}"
        LblUV.Text = "UV"
        LblUV.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblWindDir
        ' 
        LblWindDir.AutoSize = True
        LblWindDir.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        LblWindDir.Dock = DockStyle.Fill
        LblWindDir.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblWindDir.Location = New Point(4, 121)
        LblWindDir.Name = "LblWindDir"
        LblWindDir.Size = New Size(372, 29)
        LblWindDir.TabIndex = 3
        LblWindDir.Tag = "Wind direction: ({0}°) {1}"
        LblWindDir.Text = "Wind Direction"
        LblWindDir.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblWindLull
        ' 
        LblWindLull.AutoSize = True
        LblWindLull.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        LblWindLull.Dock = DockStyle.Fill
        LblWindLull.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblWindLull.Location = New Point(4, 91)
        LblWindLull.Name = "LblWindLull"
        LblWindLull.Size = New Size(372, 29)
        LblWindLull.TabIndex = 2
        LblWindLull.Tag = "Wind Lull: {0:N2} minutes"
        LblWindLull.Text = "Wind Lull (Minutes)"
        LblWindLull.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblWindGust
        ' 
        LblWindGust.AutoSize = True
        LblWindGust.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        LblWindGust.Dock = DockStyle.Fill
        LblWindGust.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblWindGust.Location = New Point(4, 61)
        LblWindGust.Name = "LblWindGust"
        LblWindGust.Size = New Size(372, 29)
        LblWindGust.TabIndex = 1
        LblWindGust.Tag = "Wind gusting to {0:N2} mph"
        LblWindGust.Text = "Wind Gusts (Mph)"
        LblWindGust.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblAvgWindSpd
        ' 
        LblAvgWindSpd.AutoSize = True
        LblAvgWindSpd.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        LblAvgWindSpd.Dock = DockStyle.Fill
        LblAvgWindSpd.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblAvgWindSpd.Location = New Point(4, 31)
        LblAvgWindSpd.Name = "LblAvgWindSpd"
        LblAvgWindSpd.Size = New Size(372, 29)
        LblAvgWindSpd.TabIndex = 0
        LblAvgWindSpd.Tag = "Average wind speed: {0:N2} mph"
        LblAvgWindSpd.Text = "Wind Average Speed (Mph)"
        LblAvgWindSpd.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LblWindSpd
        ' 
        LblWindSpd.AutoSize = True
        LblWindSpd.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        LblWindSpd.Dock = DockStyle.Fill
        LblWindSpd.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold)
        LblWindSpd.Location = New Point(4, 1)
        LblWindSpd.Name = "LblWindSpd"
        LblWindSpd.Size = New Size(372, 29)
        LblWindSpd.TabIndex = 9
        LblWindSpd.Tag = "Wind Speed: {0:F1}mph"
        LblWindSpd.Text = "Wind Speed"
        LblWindSpd.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' DgvHubStatus
        ' 
        DgvHubStatus.AllowUserToAddRows = False
        DgvHubStatus.AllowUserToDeleteRows = False
        DgvHubStatus.AllowUserToResizeRows = False
        DataGridViewCellStyle3.BackColor = Color.LightCyan
        DgvHubStatus.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle3
        DgvHubStatus.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        DataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle4.BackColor = SystemColors.ActiveCaption
        DataGridViewCellStyle4.Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle4.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle4.WrapMode = DataGridViewTriState.True
        DgvHubStatus.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle4
        DgvHubStatus.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        TlpExtraData.SetColumnSpan(DgvHubStatus, 2)
        DgvHubStatus.Location = New Point(775, 3)
        DgvHubStatus.MultiSelect = False
        DgvHubStatus.Name = "DgvHubStatus"
        DgvHubStatus.ReadOnly = True
        DgvHubStatus.RowHeadersVisible = False
        DgvHubStatus.RowHeadersWidth = 62
        DgvHubStatus.Size = New Size(378, 306)
        DgvHubStatus.TabIndex = 32
        ' 
        ' TpRecords
        ' 
        TpRecords.BackColor = Color.OldLace
        TpRecords.BorderStyle = BorderStyle.Fixed3D
        TpRecords.Controls.Add(DgvRecords)
        TpRecords.Location = New Point(4, 34)
        TpRecords.Name = "TpRecords"
        TpRecords.Size = New Size(1555, 946)
        TpRecords.TabIndex = 4
        TpRecords.Text = "Records"
        ' 
        ' DgvRecords
        ' 
        DgvRecords.AllowUserToAddRows = False
        DgvRecords.AllowUserToDeleteRows = False
        DataGridViewCellStyle5.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        DgvRecords.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle5
        DgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        DgvRecords.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        DataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle6.BackColor = SystemColors.Control
        DataGridViewCellStyle6.Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle6.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle6.WrapMode = DataGridViewTriState.True
        DgvRecords.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle6
        DgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DgvRecords.Columns.AddRange(New DataGridViewColumn() {rRain, tTemp, wWind})
        DataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle8.BackColor = SystemColors.Window
        DataGridViewCellStyle8.Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle8.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle8.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle8.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle8.WrapMode = DataGridViewTriState.True
        DgvRecords.DefaultCellStyle = DataGridViewCellStyle8
        DgvRecords.Dock = DockStyle.Left
        DgvRecords.Location = New Point(0, 0)
        DgvRecords.MultiSelect = False
        DgvRecords.Name = "DgvRecords"
        DgvRecords.ReadOnly = True
        DataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle9.BackColor = SystemColors.Control
        DataGridViewCellStyle9.Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle9.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle9.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle9.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle9.WrapMode = DataGridViewTriState.False
        DgvRecords.RowHeadersDefaultCellStyle = DataGridViewCellStyle9
        DgvRecords.RowHeadersWidth = 350
        DgvRecords.RowTemplate.Height = 50
        DgvRecords.RowTemplate.ReadOnly = True
        DgvRecords.RowTemplate.Resizable = DataGridViewTriState.True
        DgvRecords.Size = New Size(1327, 942)
        DgvRecords.TabIndex = 0
        ' 
        ' rRain
        ' 
        rRain.HeaderText = "Rain"
        rRain.MinimumWidth = 8
        rRain.Name = "rRain"
        rRain.ReadOnly = True
        ' 
        ' tTemp
        ' 
        tTemp.HeaderText = "Temperature"
        tTemp.MinimumWidth = 8
        tTemp.Name = "tTemp"
        tTemp.ReadOnly = True
        ' 
        ' wWind
        ' 
        DataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle7.Font = New Font("Microsoft Sans Serif", 8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        wWind.DefaultCellStyle = DataGridViewCellStyle7
        wWind.HeaderText = "Wind"
        wWind.MinimumWidth = 8
        wWind.Name = "wWind"
        wWind.ReadOnly = True
        ' 
        ' TpCharts
        ' 
        TpCharts.BorderStyle = BorderStyle.Fixed3D
        TpCharts.Controls.Add(ChtBattery)
        TpCharts.Location = New Point(4, 34)
        TpCharts.Name = "TpCharts"
        TpCharts.Size = New Size(1555, 946)
        TpCharts.TabIndex = 6
        TpCharts.Text = "Charts"
        TpCharts.UseVisualStyleBackColor = True
        ' 
        ' ChtBattery
        ' 
        ChtBattery.BackColor = Color.PeachPuff
        ChtBattery.BackGradientStyle = DataVisualization.Charting.GradientStyle.DiagonalLeft
        ChartArea1.Name = "ChartArea1"
        ChtBattery.ChartAreas.Add(ChartArea1)
        Legend1.Name = "BatteryLegend"
        ChtBattery.Legends.Add(Legend1)
        ChtBattery.Location = New Point(8, 8)
        ChtBattery.Name = "ChtBattery"
        Series1.ChartArea = "ChartArea1"
        Series1.Legend = "BatteryLegend"
        Series1.Name = "Series1"
        ChtBattery.Series.Add(Series1)
        ChtBattery.Size = New Size(1529, 290)
        ChtBattery.TabIndex = 11
        ChtBattery.Text = "Tempest Battery Values"
        Title1.Font = New Font("Arial Narrow", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Title1.Name = "title 1"
        Title1.Text = "24-Hour Battery Voltage History"
        ChtBattery.Titles.Add(Title1)
        ' 
        ' TpLogs
        ' 
        TpLogs.BorderStyle = BorderStyle.Fixed3D
        TpLogs.Controls.Add(TcLogs)
        TpLogs.Location = New Point(4, 34)
        TpLogs.Name = "TpLogs"
        TpLogs.Size = New Size(1555, 946)
        TpLogs.TabIndex = 3
        TpLogs.Text = "Logs"
        TpLogs.UseVisualStyleBackColor = True
        ' 
        ' TcLogs
        ' 
        TcLogs.Controls.Add(TpLogFiles)
        TcLogs.Controls.Add(TpSpares)
        TcLogs.Dock = DockStyle.Top
        TcLogs.Location = New Point(0, 0)
        TcLogs.Name = "TcLogs"
        TcLogs.SelectedIndex = 0
        TcLogs.Size = New Size(1551, 943)
        TcLogs.TabIndex = 3
        ' 
        ' TpLogFiles
        ' 
        TpLogFiles.BackColor = Color.Black
        TpLogFiles.BorderStyle = BorderStyle.Fixed3D
        TpLogFiles.Controls.Add(Panel2)
        TpLogFiles.Controls.Add(PnlLogs)
        TpLogFiles.Location = New Point(4, 30)
        TpLogFiles.Name = "TpLogFiles"
        TpLogFiles.Padding = New Padding(3)
        TpLogFiles.Size = New Size(1543, 909)
        TpLogFiles.TabIndex = 0
        TpLogFiles.Text = "Logs"
        ' 
        ' Panel2
        ' 
        Panel2.BackColor = Color.AntiqueWhite
        Panel2.BorderStyle = BorderStyle.Fixed3D
        Panel2.Controls.Add(BtnFindNext)
        Panel2.Controls.Add(BtnFind)
        Panel2.Controls.Add(TxtLogSearch)
        Panel2.Controls.Add(LbLogs)
        Panel2.Dock = DockStyle.Left
        Panel2.Location = New Point(3, 3)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(759, 899)
        Panel2.TabIndex = 2
        ' 
        ' BtnFindNext
        ' 
        BtnFindNext.Location = New Point(504, 681)
        BtnFindNext.Name = "BtnFindNext"
        BtnFindNext.Size = New Size(112, 34)
        BtnFindNext.TabIndex = 5
        BtnFindNext.Text = "Find Next"
        BtnFindNext.UseVisualStyleBackColor = True
        ' 
        ' BtnFind
        ' 
        BtnFind.Location = New Point(340, 681)
        BtnFind.Name = "BtnFind"
        BtnFind.Size = New Size(112, 34)
        BtnFind.TabIndex = 4
        BtnFind.Text = "Find"
        BtnFind.UseVisualStyleBackColor = True
        ' 
        ' TxtLogSearch
        ' 
        TxtLogSearch.Location = New Point(138, 684)
        TxtLogSearch.Name = "TxtLogSearch"
        TxtLogSearch.Size = New Size(150, 29)
        TxtLogSearch.TabIndex = 3
        ' 
        ' LbLogs
        ' 
        LbLogs.Font = New Font("Segoe UI", 9F, FontStyle.Bold Or FontStyle.Italic, GraphicsUnit.Point, CByte(0))
        LbLogs.FormattingEnabled = True
        LbLogs.Location = New Point(220, 179)
        LbLogs.Name = "LbLogs"
        LbLogs.Size = New Size(314, 454)
        LbLogs.TabIndex = 2
        ' 
        ' PnlLogs
        ' 
        PnlLogs.BackColor = Color.AntiqueWhite
        PnlLogs.BorderStyle = BorderStyle.Fixed3D
        PnlLogs.Controls.Add(Label24)
        PnlLogs.Controls.Add(RtbLogs)
        PnlLogs.Dock = DockStyle.Right
        PnlLogs.Location = New Point(777, 3)
        PnlLogs.Name = "PnlLogs"
        PnlLogs.Size = New Size(759, 899)
        PnlLogs.TabIndex = 1
        ' 
        ' Label24
        ' 
        Label24.AutoSize = True
        Label24.Font = New Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label24.Location = New Point(302, 17)
        Label24.Name = "Label24"
        Label24.Size = New Size(150, 48)
        Label24.TabIndex = 1
        Label24.Text = "Log File"
        ' 
        ' RtbLogs
        ' 
        RtbLogs.BackColor = Color.FloralWhite
        RtbLogs.Location = New Point(44, 73)
        RtbLogs.Name = "RtbLogs"
        RtbLogs.ReadOnly = True
        RtbLogs.ScrollBars = RichTextBoxScrollBars.ForcedBoth
        RtbLogs.ShowSelectionMargin = True
        RtbLogs.Size = New Size(667, 799)
        RtbLogs.TabIndex = 0
        RtbLogs.Text = ""
        ' 
        ' TpSpares
        ' 
        TpSpares.BackColor = Color.AntiqueWhite
        TpSpares.BorderStyle = BorderStyle.Fixed3D
        TpSpares.Location = New Point(4, 34)
        TpSpares.Name = "TpSpares"
        TpSpares.Size = New Size(1543, 905)
        TpSpares.TabIndex = 2
        TpSpares.Text = "Spare"
        ' 
        ' TpHelp
        ' 
        TpHelp.BackColor = Color.Azure
        TpHelp.BorderStyle = BorderStyle.Fixed3D
        TpHelp.Controls.Add(PnlHelp)
        TpHelp.Controls.Add(ScHelp)
        TpHelp.Location = New Point(4, 34)
        TpHelp.Name = "TpHelp"
        TpHelp.Size = New Size(1555, 946)
        TpHelp.TabIndex = 7
        TpHelp.Text = "Help"
        ' 
        ' PnlHelp
        ' 
        PnlHelp.BackColor = Color.Turquoise
        PnlHelp.BorderStyle = BorderStyle.Fixed3D
        PnlHelp.Controls.Add(BtnHelpExport)
        PnlHelp.Controls.Add(BtnHelpPrint)
        PnlHelp.Controls.Add(TxtHelpSearch)
        PnlHelp.Controls.Add(Label25)
        PnlHelp.Dock = DockStyle.Top
        PnlHelp.Location = New Point(0, 0)
        PnlHelp.Name = "PnlHelp"
        PnlHelp.Size = New Size(1551, 74)
        PnlHelp.TabIndex = 1
        ' 
        ' BtnHelpExport
        ' 
        BtnHelpExport.Location = New Point(1260, 18)
        BtnHelpExport.Name = "BtnHelpExport"
        BtnHelpExport.Size = New Size(112, 34)
        BtnHelpExport.TabIndex = 3
        BtnHelpExport.Text = "Export"
        BtnHelpExport.UseVisualStyleBackColor = True
        ' 
        ' BtnHelpPrint
        ' 
        BtnHelpPrint.Location = New Point(995, 18)
        BtnHelpPrint.Name = "BtnHelpPrint"
        BtnHelpPrint.Size = New Size(112, 34)
        BtnHelpPrint.TabIndex = 2
        BtnHelpPrint.Text = "Print"
        BtnHelpPrint.UseVisualStyleBackColor = True
        ' 
        ' TxtHelpSearch
        ' 
        TxtHelpSearch.Location = New Point(279, 21)
        TxtHelpSearch.Name = "TxtHelpSearch"
        TxtHelpSearch.Size = New Size(593, 29)
        TxtHelpSearch.TabIndex = 1
        ' 
        ' Label25
        ' 
        Label25.AutoSize = True
        Label25.Font = New Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label25.Location = New Point(174, 25)
        Label25.Name = "Label25"
        Label25.Size = New Size(105, 21)
        Label25.TabIndex = 0
        Label25.Text = "Search Text: "
        ' 
        ' ScHelp
        ' 
        ScHelp.BackColor = Color.PaleTurquoise
        ScHelp.BorderStyle = BorderStyle.Fixed3D
        ScHelp.Dock = DockStyle.Bottom
        ScHelp.Location = New Point(0, 76)
        ScHelp.Name = "ScHelp"
        ' 
        ' ScHelp.Panel1
        ' 
        ScHelp.Panel1.Controls.Add(TvHelp)
        ' 
        ' ScHelp.Panel2
        ' 
        ScHelp.Panel2.Controls.Add(RtbHelp)
        ScHelp.Size = New Size(1551, 866)
        ScHelp.SplitterDistance = 400
        ScHelp.TabIndex = 0
        ' 
        ' TvHelp
        ' 
        TvHelp.Font = New Font("Segoe UI", 9F)
        TvHelp.Location = New Point(31, 116)
        TvHelp.Name = "TvHelp"
        TvHelp.Size = New Size(333, 631)
        TvHelp.TabIndex = 0
        ' 
        ' RtbHelp
        ' 
        RtbHelp.Font = New Font("Segoe UI", 10F)
        RtbHelp.Location = New Point(100, 72)
        RtbHelp.Name = "RtbHelp"
        RtbHelp.ReadOnly = True
        RtbHelp.ShowSelectionMargin = True
        RtbHelp.Size = New Size(943, 719)
        RtbHelp.TabIndex = 0
        RtbHelp.Text = ""
        ' 
        ' TpSettings
        ' 
        TpSettings.BorderStyle = BorderStyle.Fixed3D
        TpSettings.Controls.Add(PnlSettings)
        TpSettings.Location = New Point(4, 34)
        TpSettings.Name = "TpSettings"
        TpSettings.Padding = New Padding(3)
        TpSettings.Size = New Size(1555, 946)
        TpSettings.TabIndex = 1
        TpSettings.Text = "Settings"
        TpSettings.UseVisualStyleBackColor = True
        ' 
        ' PnlSettings
        ' 
        PnlSettings.BackColor = Color.Gainsboro
        PnlSettings.BorderStyle = BorderStyle.Fixed3D
        PnlSettings.Controls.Add(GbStationData)
        PnlSettings.Controls.Add(GbLogSettings)
        PnlSettings.Controls.Add(Panel1)
        PnlSettings.Controls.Add(GroupBox1)
        PnlSettings.Controls.Add(GbTempestSettings)
        PnlSettings.Controls.Add(GbMeteoBridge)
        PnlSettings.Dock = DockStyle.Fill
        PnlSettings.Location = New Point(3, 3)
        PnlSettings.Name = "PnlSettings"
        PnlSettings.Size = New Size(1545, 936)
        PnlSettings.TabIndex = 0
        ' 
        ' GbStationData
        ' 
        GbStationData.BackColor = Color.AntiqueWhite
        GbStationData.Controls.Add(TxtStationElevation)
        GbStationData.Controls.Add(Label16)
        GbStationData.Controls.Add(TxtStationID)
        GbStationData.Controls.Add(TxtStationName)
        GbStationData.Controls.Add(Label23)
        GbStationData.Controls.Add(Label22)
        GbStationData.Location = New Point(27, 410)
        GbStationData.Name = "GbStationData"
        GbStationData.Size = New Size(316, 150)
        GbStationData.TabIndex = 8
        GbStationData.TabStop = False
        GbStationData.Text = "Station Data"
        ' 
        ' TxtStationElevation
        ' 
        TxtStationElevation.Location = New Point(117, 104)
        TxtStationElevation.MaxLength = 15
        TxtStationElevation.Name = "TxtStationElevation"
        TxtStationElevation.Size = New Size(96, 29)
        TxtStationElevation.TabIndex = 5
        ' 
        ' Label16
        ' 
        Label16.AutoSize = True
        Label16.Location = New Point(17, 108)
        Label16.Name = "Label16"
        Label16.Size = New Size(100, 21)
        Label16.TabIndex = 4
        Label16.Text = "Elevation (Ft)"
        ' 
        ' TxtStationID
        ' 
        TxtStationID.Location = New Point(117, 68)
        TxtStationID.Name = "TxtStationID"
        TxtStationID.Size = New Size(181, 29)
        TxtStationID.TabIndex = 3
        ' 
        ' TxtStationName
        ' 
        TxtStationName.Location = New Point(117, 32)
        TxtStationName.Name = "TxtStationName"
        TxtStationName.Size = New Size(181, 29)
        TxtStationName.TabIndex = 2
        ' 
        ' Label23
        ' 
        Label23.AutoSize = True
        Label23.Location = New Point(40, 72)
        Label23.Name = "Label23"
        Label23.Size = New Size(77, 21)
        Label23.TabIndex = 1
        Label23.Text = "Station ID"
        ' 
        ' Label22
        ' 
        Label22.AutoSize = True
        Label22.Location = New Point(65, 36)
        Label22.Name = "Label22"
        Label22.Size = New Size(52, 21)
        Label22.TabIndex = 0
        Label22.Text = "Name"
        ' 
        ' GbLogSettings
        ' 
        GbLogSettings.BackColor = Color.AntiqueWhite
        GbLogSettings.Controls.Add(TxtLogDays)
        GbLogSettings.Controls.Add(Label21)
        GbLogSettings.Location = New Point(27, 311)
        GbLogSettings.Name = "GbLogSettings"
        GbLogSettings.Size = New Size(300, 89)
        GbLogSettings.TabIndex = 7
        GbLogSettings.TabStop = False
        GbLogSettings.Text = "Log Settings"
        ' 
        ' TxtLogDays
        ' 
        TxtLogDays.Location = New Point(204, 38)
        TxtLogDays.Name = "TxtLogDays"
        TxtLogDays.Size = New Size(65, 29)
        TxtLogDays.TabIndex = 1
        ' 
        ' Label21
        ' 
        Label21.AutoSize = True
        Label21.Location = New Point(21, 41)
        Label21.Name = "Label21"
        Label21.Size = New Size(157, 21)
        Label21.TabIndex = 0
        Label21.Text = "Days to keep log files"
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.AntiqueWhite
        Panel1.BorderStyle = BorderStyle.Fixed3D
        Panel1.Controls.Add(Label12)
        Panel1.Controls.Add(RtbSettings)
        Panel1.Location = New Point(845, 25)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(684, 846)
        Panel1.TabIndex = 6
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        Label12.Location = New Point(217, 12)
        Label12.Name = "Label12"
        Label12.Size = New Size(246, 28)
        Label12.TabIndex = 6
        Label12.Text = "TempestDisplay_Settings"
        ' 
        ' RtbSettings
        ' 
        RtbSettings.BackColor = SystemColors.Info
        RtbSettings.Location = New Point(26, 54)
        RtbSettings.Name = "RtbSettings"
        RtbSettings.ReadOnly = True
        RtbSettings.ShowSelectionMargin = True
        RtbSettings.Size = New Size(629, 761)
        RtbSettings.TabIndex = 5
        RtbSettings.TabStop = False
        RtbSettings.Text = ""
        ' 
        ' GroupBox1
        ' 
        GroupBox1.BackColor = Color.AntiqueWhite
        GroupBox1.Controls.Add(TlpRainGaugeLimits)
        GroupBox1.Location = New Point(513, 132)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(237, 241)
        GroupBox1.TabIndex = 4
        GroupBox1.TabStop = False
        GroupBox1.Text = "Rain Gauge Limits"
        ' 
        ' TlpRainGaugeLimits
        ' 
        TlpRainGaugeLimits.ColumnCount = 2
        TlpRainGaugeLimits.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TlpRainGaugeLimits.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TlpRainGaugeLimits.Controls.Add(Label7, 0, 0)
        TlpRainGaugeLimits.Controls.Add(Label8, 0, 1)
        TlpRainGaugeLimits.Controls.Add(Label9, 0, 2)
        TlpRainGaugeLimits.Controls.Add(Label10, 0, 3)
        TlpRainGaugeLimits.Controls.Add(Label11, 0, 4)
        TlpRainGaugeLimits.Controls.Add(NudRainTodayLimit, 1, 0)
        TlpRainGaugeLimits.Controls.Add(NudRainYdayLimit, 1, 1)
        TlpRainGaugeLimits.Controls.Add(NumRainMonthLimit, 1, 2)
        TlpRainGaugeLimits.Controls.Add(NudRainYearLimit, 1, 3)
        TlpRainGaugeLimits.Controls.Add(NudRainAlltimeLimit, 1, 4)
        TlpRainGaugeLimits.Location = New Point(11, 23)
        TlpRainGaugeLimits.Name = "TlpRainGaugeLimits"
        TlpRainGaugeLimits.RowCount = 5
        TlpRainGaugeLimits.RowStyles.Add(New RowStyle(SizeType.Percent, 20F))
        TlpRainGaugeLimits.RowStyles.Add(New RowStyle(SizeType.Percent, 20F))
        TlpRainGaugeLimits.RowStyles.Add(New RowStyle(SizeType.Percent, 20F))
        TlpRainGaugeLimits.RowStyles.Add(New RowStyle(SizeType.Percent, 20F))
        TlpRainGaugeLimits.RowStyles.Add(New RowStyle(SizeType.Percent, 20F))
        TlpRainGaugeLimits.Size = New Size(215, 195)
        TlpRainGaugeLimits.TabIndex = 0
        ' 
        ' Label7
        ' 
        Label7.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label7.AutoSize = True
        Label7.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Label7.Location = New Point(2, 2)
        Label7.Margin = New Padding(2)
        Label7.Name = "Label7"
        Label7.Size = New Size(103, 35)
        Label7.TabIndex = 0
        Label7.Text = "Today"
        Label7.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label8
        ' 
        Label8.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label8.AutoSize = True
        Label8.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Label8.Location = New Point(2, 41)
        Label8.Margin = New Padding(2)
        Label8.Name = "Label8"
        Label8.Size = New Size(103, 35)
        Label8.TabIndex = 1
        Label8.Text = "Yesterday"
        Label8.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label9
        ' 
        Label9.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label9.AutoSize = True
        Label9.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Label9.Location = New Point(2, 80)
        Label9.Margin = New Padding(2)
        Label9.Name = "Label9"
        Label9.Size = New Size(103, 35)
        Label9.TabIndex = 2
        Label9.Text = "Month"
        Label9.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label10
        ' 
        Label10.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label10.AutoSize = True
        Label10.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Label10.Location = New Point(2, 119)
        Label10.Margin = New Padding(2)
        Label10.Name = "Label10"
        Label10.Size = New Size(103, 35)
        Label10.TabIndex = 3
        Label10.Text = "Year"
        Label10.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label11
        ' 
        Label11.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Label11.AutoSize = True
        Label11.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Label11.Location = New Point(2, 158)
        Label11.Margin = New Padding(2)
        Label11.Name = "Label11"
        Label11.Size = New Size(103, 35)
        Label11.TabIndex = 4
        Label11.Text = "All-time"
        Label11.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' NudRainTodayLimit
        ' 
        NudRainTodayLimit.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        NudRainTodayLimit.Location = New Point(110, 5)
        NudRainTodayLimit.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        NudRainTodayLimit.Name = "NudRainTodayLimit"
        NudRainTodayLimit.Size = New Size(102, 29)
        NudRainTodayLimit.TabIndex = 0
        NudRainTodayLimit.Tag = "0"
        NudRainTodayLimit.Value = New Decimal(New Integer() {5, 0, 0, 0})
        ' 
        ' NudRainYdayLimit
        ' 
        NudRainYdayLimit.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        NudRainYdayLimit.Location = New Point(110, 44)
        NudRainYdayLimit.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        NudRainYdayLimit.Name = "NudRainYdayLimit"
        NudRainYdayLimit.Size = New Size(102, 29)
        NudRainYdayLimit.TabIndex = 1
        NudRainYdayLimit.Tag = "1"
        NudRainYdayLimit.Value = New Decimal(New Integer() {5, 0, 0, 0})
        ' 
        ' NumRainMonthLimit
        ' 
        NumRainMonthLimit.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        NumRainMonthLimit.Location = New Point(110, 83)
        NumRainMonthLimit.Minimum = New Decimal(New Integer() {15, 0, 0, 0})
        NumRainMonthLimit.Name = "NumRainMonthLimit"
        NumRainMonthLimit.Size = New Size(102, 29)
        NumRainMonthLimit.TabIndex = 2
        NumRainMonthLimit.Tag = "2"
        NumRainMonthLimit.Value = New Decimal(New Integer() {15, 0, 0, 0})
        ' 
        ' NudRainYearLimit
        ' 
        NudRainYearLimit.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        NudRainYearLimit.Location = New Point(110, 122)
        NudRainYearLimit.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        NudRainYearLimit.Name = "NudRainYearLimit"
        NudRainYearLimit.Size = New Size(102, 29)
        NudRainYearLimit.TabIndex = 3
        NudRainYearLimit.Tag = "3"
        NudRainYearLimit.Value = New Decimal(New Integer() {50, 0, 0, 0})
        ' 
        ' NudRainAlltimeLimit
        ' 
        NudRainAlltimeLimit.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        NudRainAlltimeLimit.Location = New Point(110, 161)
        NudRainAlltimeLimit.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NudRainAlltimeLimit.Minimum = New Decimal(New Integer() {400, 0, 0, 0})
        NudRainAlltimeLimit.Name = "NudRainAlltimeLimit"
        NudRainAlltimeLimit.Size = New Size(102, 29)
        NudRainAlltimeLimit.TabIndex = 4
        NudRainAlltimeLimit.Tag = "4"
        NudRainAlltimeLimit.Value = New Decimal(New Integer() {400, 0, 0, 0})
        ' 
        ' GbTempestSettings
        ' 
        GbTempestSettings.BackColor = Color.AntiqueWhite
        GbTempestSettings.Controls.Add(NudTempestInterval)
        GbTempestSettings.Controls.Add(TxtApiToken)
        GbTempestSettings.Controls.Add(TxtDeviceID)
        GbTempestSettings.Controls.Add(Label6)
        GbTempestSettings.Controls.Add(Label5)
        GbTempestSettings.Controls.Add(Label4)
        GbTempestSettings.Location = New Point(27, 132)
        GbTempestSettings.Name = "GbTempestSettings"
        GbTempestSettings.Size = New Size(474, 169)
        GbTempestSettings.TabIndex = 3
        GbTempestSettings.TabStop = False
        GbTempestSettings.Text = "Tempest Settings"
        ' 
        ' NudTempestInterval
        ' 
        NudTempestInterval.Location = New Point(229, 124)
        NudTempestInterval.Maximum = New Decimal(New Integer() {1800, 0, 0, 0})
        NudTempestInterval.Minimum = New Decimal(New Integer() {180, 0, 0, 0})
        NudTempestInterval.Name = "NudTempestInterval"
        NudTempestInterval.Size = New Size(87, 29)
        NudTempestInterval.TabIndex = 2
        NudTempestInterval.Value = New Decimal(New Integer() {180, 0, 0, 0})
        ' 
        ' TxtApiToken
        ' 
        TxtApiToken.Location = New Point(104, 77)
        TxtApiToken.Name = "TxtApiToken"
        TxtApiToken.Size = New Size(356, 29)
        TxtApiToken.TabIndex = 1
        ' 
        ' TxtDeviceID
        ' 
        TxtDeviceID.Location = New Point(101, 30)
        TxtDeviceID.Name = "TxtDeviceID"
        TxtDeviceID.Size = New Size(150, 29)
        TxtDeviceID.TabIndex = 0
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(14, 127)
        Label6.Name = "Label6"
        Label6.Size = New Size(188, 21)
        Label6.TabIndex = 2
        Label6.Text = "Update Interval {Seconds}"
        Label6.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(14, 80)
        Label5.Name = "Label5"
        Label5.Size = New Size(77, 21)
        Label5.TabIndex = 1
        Label5.Text = "API Token"
        Label5.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(14, 33)
        Label4.Name = "Label4"
        Label4.Size = New Size(75, 21)
        Label4.TabIndex = 0
        Label4.Text = "Device ID"
        Label4.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' GbMeteoBridge
        ' 
        GbMeteoBridge.BackColor = Color.AntiqueWhite
        GbMeteoBridge.Controls.Add(TxtIp)
        GbMeteoBridge.Controls.Add(TxtPassword)
        GbMeteoBridge.Controls.Add(TxtLogin)
        GbMeteoBridge.Controls.Add(Label3)
        GbMeteoBridge.Controls.Add(Label2)
        GbMeteoBridge.Controls.Add(Label1)
        GbMeteoBridge.Location = New Point(27, 25)
        GbMeteoBridge.Name = "GbMeteoBridge"
        GbMeteoBridge.Size = New Size(757, 97)
        GbMeteoBridge.TabIndex = 2
        GbMeteoBridge.TabStop = False
        GbMeteoBridge.Text = "MeteoBridge"
        ' 
        ' TxtIp
        ' 
        TxtIp.Location = New Point(573, 33)
        TxtIp.Name = "TxtIp"
        TxtIp.Size = New Size(150, 29)
        TxtIp.TabIndex = 2
        TxtIp.Tag = "2"
        ' 
        ' TxtPassword
        ' 
        TxtPassword.Location = New Point(354, 33)
        TxtPassword.Name = "TxtPassword"
        TxtPassword.Size = New Size(150, 29)
        TxtPassword.TabIndex = 1
        TxtPassword.Tag = "1"
        ' 
        ' TxtLogin
        ' 
        TxtLogin.Location = New Point(89, 33)
        TxtLogin.Name = "TxtLogin"
        TxtLogin.Size = New Size(150, 29)
        TxtLogin.TabIndex = 0
        TxtLogin.Tag = "0"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(546, 36)
        Label3.Name = "Label3"
        Label3.Size = New Size(23, 21)
        Label3.TabIndex = 2
        Label3.Text = "IP"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(267, 36)
        Label2.Name = "Label2"
        Label2.Size = New Size(76, 21)
        Label2.TabIndex = 1
        Label2.Text = "Password"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(33, 36)
        Label1.Name = "Label1"
        Label1.Size = New Size(49, 21)
        Label1.TabIndex = 0
        Label1.Text = "Login"
        ' 
        ' TmrClock
        ' 
        TmrClock.Interval = 999
        ' 
        ' SsObs_St
        ' 
        SsObs_St.GripMargin = New Padding(0)
        SsObs_St.ImageScalingSize = New Size(24, 24)
        SsObs_St.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel1, TsslObs_St})
        SsObs_St.Location = New Point(0, 992)
        SsObs_St.Name = "SsObs_St"
        SsObs_St.Size = New Size(1563, 32)
        SsObs_St.SizingGrip = False
        SsObs_St.TabIndex = 4
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(78, 25)
        ToolStripStatusLabel1.Text = "Station:"
        ' 
        ' TsslObs_St
        ' 
        TsslObs_St.Font = New Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        TsslObs_St.Name = "TsslObs_St"
        TsslObs_St.Size = New Size(1470, 25)
        TsslObs_St.Spring = True
        TsslObs_St.Text = "tempest_obs"
        ' 
        ' TTip
        ' 
        TTip.IsBalloon = True
        TTip.ToolTipIcon = ToolTipIcon.Info
        TTip.ToolTipTitle = "Tempest Display"
        ' 
        ' FrmMain
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        ClientSize = New Size(1563, 1088)
        Controls.Add(SsObs_St)
        Controls.Add(Tc)
        Controls.Add(SsTop)
        Controls.Add(SsBottom)
        DoubleBuffered = True
        Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "FrmMain"
        SizeGripStyle = SizeGripStyle.Show
        Text = "Form1"
        SsBottom.ResumeLayout(False)
        SsBottom.PerformLayout()
        SsTop.ResumeLayout(False)
        SsTop.PerformLayout()
        Tc.ResumeLayout(False)
        TpData.ResumeLayout(False)
        PnlData.ResumeLayout(False)
        TlpData.ResumeLayout(False)
        TlpTemp.ResumeLayout(False)
        TlpTemp.PerformLayout()
        TpExtraData.ResumeLayout(False)
        TlpExtraData.ResumeLayout(False)
        CType(DgvObsSt, ComponentModel.ISupportInitialize).EndInit()
        TlpPrecipLight.ResumeLayout(False)
        TlpPrecipLight.PerformLayout()
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        TlpWindSun.ResumeLayout(False)
        TlpWindSun.PerformLayout()
        CType(DgvHubStatus, ComponentModel.ISupportInitialize).EndInit()
        TpRecords.ResumeLayout(False)
        CType(DgvRecords, ComponentModel.ISupportInitialize).EndInit()
        TpCharts.ResumeLayout(False)
        CType(ChtBattery, ComponentModel.ISupportInitialize).EndInit()
        TpLogs.ResumeLayout(False)
        TcLogs.ResumeLayout(False)
        TpLogFiles.ResumeLayout(False)
        Panel2.ResumeLayout(False)
        Panel2.PerformLayout()
        PnlLogs.ResumeLayout(False)
        PnlLogs.PerformLayout()
        TpHelp.ResumeLayout(False)
        PnlHelp.ResumeLayout(False)
        PnlHelp.PerformLayout()
        ScHelp.Panel1.ResumeLayout(False)
        ScHelp.Panel2.ResumeLayout(False)
        CType(ScHelp, ComponentModel.ISupportInitialize).EndInit()
        ScHelp.ResumeLayout(False)
        TpSettings.ResumeLayout(False)
        PnlSettings.ResumeLayout(False)
        GbStationData.ResumeLayout(False)
        GbStationData.PerformLayout()
        GbLogSettings.ResumeLayout(False)
        GbLogSettings.PerformLayout()
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        GroupBox1.ResumeLayout(False)
        TlpRainGaugeLimits.ResumeLayout(False)
        TlpRainGaugeLimits.PerformLayout()
        CType(NudRainTodayLimit, ComponentModel.ISupportInitialize).EndInit()
        CType(NudRainYdayLimit, ComponentModel.ISupportInitialize).EndInit()
        CType(NumRainMonthLimit, ComponentModel.ISupportInitialize).EndInit()
        CType(NudRainYearLimit, ComponentModel.ISupportInitialize).EndInit()
        CType(NudRainAlltimeLimit, ComponentModel.ISupportInitialize).EndInit()
        GbTempestSettings.ResumeLayout(False)
        GbTempestSettings.PerformLayout()
        CType(NudTempestInterval, ComponentModel.ISupportInitialize).EndInit()
        GbMeteoBridge.ResumeLayout(False)
        GbMeteoBridge.PerformLayout()
        SsObs_St.ResumeLayout(False)
        SsObs_St.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents SsBottom As StatusStrip
    Friend WithEvents TsslVer As ToolStripStatusLabel
    Friend WithEvents TsslCpy As ToolStripStatusLabel
    Friend WithEvents TsslClock As ToolStripStatusLabel
    Friend WithEvents SsTop As StatusStrip
    Friend WithEvents Tc As TabControl
    Friend WithEvents TpData As TabPage
    Friend WithEvents TpLogs As TabPage
    Friend WithEvents TpRecords As TabPage
    Friend WithEvents TpSettings As TabPage
    Friend WithEvents PnlData As Panel
    Friend WithEvents PnlLogs As Panel
    Friend WithEvents RtbLogs As RichTextBox
    Friend WithEvents PnlSettings As Panel
    Friend WithEvents TmrClock As Timer
    Friend WithEvents GbTempestSettings As GroupBox
    Friend WithEvents NudTempestInterval As NumericUpDown
    Friend WithEvents TxtApiToken As TextBox
    Friend WithEvents TxtDeviceID As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents GbMeteoBridge As GroupBox
    Friend WithEvents TxtIp As TextBox
    Friend WithEvents TxtPassword As TextBox
    Friend WithEvents TxtLogin As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents TsslTimesrun As ToolStripStatusLabel
    Friend WithEvents TsslMessages As ToolStripStatusLabel
    Friend WithEvents TlpData As TableLayoutPanel
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents TlpRainGaugeLimits As TableLayoutPanel
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents NudRainTodayLimit As NumericUpDown
    Friend WithEvents NudRainYdayLimit As NumericUpDown
    Friend WithEvents NumRainMonthLimit As NumericUpDown
    Friend WithEvents NudRainYearLimit As NumericUpDown
    Friend WithEvents NudRainAlltimeLimit As NumericUpDown
    Friend WithEvents RtbSettings As RichTextBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Label12 As Label
    Friend WithEvents GbLogSettings As GroupBox
    Friend WithEvents TxtLogDays As TextBox
    Friend WithEvents Label21 As Label
    Friend WithEvents TsslMidnightCountdown As ToolStripStatusLabel
    Friend WithEvents GbStationData As GroupBox
    Friend WithEvents TxtStationID As TextBox
    Friend WithEvents TxtStationName As TextBox
    Friend WithEvents Label23 As Label
    Friend WithEvents Label22 As Label
    Friend WithEvents SsObs_St As StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents Label24 As Label
    Friend WithEvents TTip As ToolTip
    Friend WithEvents TsslObs_St As ToolStripStatusLabel
    Friend WithEvents DgvRecords As DataGridView
    Friend WithEvents rRain As DataGridViewTextBoxColumn
    Friend WithEvents tTemp As DataGridViewTextBoxColumn
    Friend WithEvents wWind As DataGridViewTextBoxColumn
    Friend WithEvents TgCurrentTemp As TempGaugeControl
    Friend WithEvents TgFeelsLike As TempGaugeControl
    Friend WithEvents TgDewpoint As TempGaugeControl
    Friend WithEvents FgRH As FanGaugeControl
    Friend WithEvents PTC As PrecipTowersControl
    Friend WithEvents Label26 As Label
    Friend WithEvents TlpTemp As TableLayoutPanel
    Friend WithEvents LblCurTemp As Label
    Friend WithEvents Label27 As Label
    Friend WithEvents LblCurWinds As Label
    Friend WithEvents TxtStationElevation As TextBox
    Friend WithEvents Label16 As Label
    Friend WithEvents TcLogs As TabControl
    Friend WithEvents TpLogFiles As TabPage
    Friend WithEvents WrWindSpeed As WindRoseControl
    Friend WithEvents ThermCurrentTemp As TempThermometerControl
    Friend WithEvents ThermFeelsLike As TempThermometerControl
    Friend WithEvents ThermDewpoint As TempThermometerControl
    Friend WithEvents BaroPressure As BarometerControl
    Friend WithEvents AltAirDensity As AirDensityAltimeter
    Friend WithEvents RainRate As RainRateGauge
    Friend WithEvents LightningRadar As LightningProximityRadar
    Friend WithEvents UvMeter As UVIndexMeter
    Friend WithEvents SolarMeter As SolarEnergyMeter
    Friend WithEvents StatusLeds As StatusLEDPanel
    Friend WithEvents TrendArrows As TrendArrowsControl
    Friend WithEvents TpCharts As TabPage
    Friend WithEvents ChtBattery As DataVisualization.Charting.Chart
    Friend WithEvents TpExtraData As TabPage
    Friend WithEvents TlpExtraData As TableLayoutPanel
    Friend WithEvents DgvObsSt As DataGridView
    Friend WithEvents DgvHubStatus As DataGridView
    Friend WithEvents TlpPrecipLight As TableLayoutPanel
    Friend WithEvents Label13 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents Label15 As Label
    Friend WithEvents TxtRainTodayMinutes As TextBox
    Friend WithEvents TxtRainYesterdayMinutes As TextBox
    Friend WithEvents LblLightningDataTitle As Label
    Friend WithEvents Label17 As Label
    Friend WithEvents Label18 As Label
    Friend WithEvents Label19 As Label
    Friend WithEvents Label20 As Label
    Friend WithEvents LblLightLastStrike As Label
    Friend WithEvents TxtStrikeCount As TextBox
    Friend WithEvents TxtLightHrCount As TextBox
    Friend WithEvents TxtLight3hrCount As TextBox
    Friend WithEvents TxtLightDistance As TextBox
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents LblAirDensity As Label
    Friend WithEvents LblAirDensityCat As Label
    Friend WithEvents LblCloudBase As Label
    Friend WithEvents LblIP As Label
    Friend WithEvents LblUpdate As Label
    Friend WithEvents LblBatteryStatus As Label
    Friend WithEvents TlpWindSun As TableLayoutPanel
    Friend WithEvents LblPressTrend As Label
    Friend WithEvents LblBaroPress As Label
    Friend WithEvents LblBrightness As Label
    Friend WithEvents LblSolRad As Label
    Friend WithEvents LblUV As Label
    Friend WithEvents LblWindDir As Label
    Friend WithEvents LblWindLull As Label
    Friend WithEvents LblWindGust As Label
    Friend WithEvents LblAvgWindSpd As Label
    Friend WithEvents LblWindSpd As Label
    Friend WithEvents TpHelp As TabPage
    Friend WithEvents PnlHelp As Panel
    Friend WithEvents BtnHelpExport As Button
    Friend WithEvents BtnHelpPrint As Button
    Friend WithEvents TxtHelpSearch As TextBox
    Friend WithEvents Label25 As Label
    Friend WithEvents ScHelp As SplitContainer
    Friend WithEvents TvHelp As TreeView
    Friend WithEvents RtbHelp As RichTextBox
    Friend WithEvents TsslErrCount As ToolStripStatusLabel
    Friend WithEvents TpSpares As TabPage
    Friend WithEvents Panel2 As Panel
    Friend WithEvents BtnFindNext As Button
    Friend WithEvents BtnFind As Button
    Friend WithEvents TxtLogSearch As TextBox
    Friend WithEvents LbLogs As ListBox

End Class
