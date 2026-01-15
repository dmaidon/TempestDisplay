''' <summary>
''' Provides all help content for the TempestDisplay settings.
''' </summary>
Public Class SettingsHelpProvider

    Private _topics As List(Of HelpTopic)

    Public Sub New()
        InitializeHelpContent()
    End Sub

    ''' <summary>
    ''' Gets all available help topics.
    ''' </summary>
    Public ReadOnly Property AllTopics As List(Of HelpTopic)
        Get
            Return _topics
        End Get
    End Property

    ''' <summary>
    ''' Gets all unique categories.
    ''' </summary>
    Public ReadOnly Property Categories As List(Of String)
        Get
            Return _topics.Select(Function(t) t.Category).Distinct().OrderBy(Function(c) c).ToList()
        End Get
    End Property

    ''' <summary>
    ''' Gets all unique tab pages.
    ''' </summary>
    Public ReadOnly Property TabPages As List(Of String)
        Get
            Return _topics.Select(Function(t) t.TabPage).Distinct().OrderBy(Function(tp) tp).ToList()
        End Get
    End Property

    ''' <summary>
    ''' Gets topics for a specific category.
    ''' </summary>
    Public Function GetTopicsByCategory(category As String) As List(Of HelpTopic)
        Return _topics.Where(Function(t) t.Category = category).OrderBy(Function(t) t.SortOrder).ToList()
    End Function

    ''' <summary>
    ''' Gets topics for a specific tab page.
    ''' </summary>
    Public Function GetTopicsByTabPage(tabPage As String) As List(Of HelpTopic)
        Return _topics.Where(Function(t) t.TabPage = tabPage).OrderBy(Function(t) t.Category).ThenBy(Function(t) t.SortOrder).ToList()
    End Function

    ''' <summary>
    ''' Gets a topic by its ID.
    ''' </summary>
    Public Function GetTopicById(id As String) As HelpTopic
        Return _topics.FirstOrDefault(Function(t) t.Id = id)
    End Function

    ''' <summary>
    ''' Searches topics by text.
    ''' </summary>
    Public Function Search(searchText As String) As List(Of HelpTopic)
        If String.IsNullOrWhiteSpace(searchText) Then
            Return _topics
        End If

        Return _topics.Where(Function(t) t.MatchesSearch(searchText)).ToList()
    End Function

    Private Sub InitializeHelpContent()
        _topics = New List(Of HelpTopic)()

        ' ===== Application Overview =====
        AddApplicationOverviewTopics()

        ' ===== Tab Pages =====
        AddTabPageTopics()

        ' ===== General Settings =====
        AddGeneralSettingsTopics()

        ' ===== API Settings =====
        AddApiSettingsTopics()

        ' ===== Rain Gauge Settings =====
        AddRainGaugeTopics()

        ' ===== Logging Settings =====
        AddLoggingTopics()
    End Sub

    Private Sub AddApplicationOverviewTopics()
        _topics.Add(New HelpTopic(
            "app_overview",
            "TempestDisplay Overview",
            "Application",
            "Overview",
            CreateRtfContent(
                "TempestDisplay Overview",
                "TempestDisplay is a professional Windows desktop application for displaying real-time weather data from WeatherFlow Tempest weather stations." & vbCrLf & vbCrLf &
                "Key Features:" & vbCrLf &
                "• Real-time UDP data from Tempest hub (updates every 3-60 seconds)" & vbCrLf &
                "• Professional custom weather controls (gauges, charts, displays)" & vbCrLf &
                "• Historical records tracking (high/low temperatures, wind, rain)" & vbCrLf &
                "• Lightning strike detection and tracking" & vbCrLf &
                "• Barometric pressure trends and analysis" & vbCrLf &
                "• Rain accumulation tracking (daily, monthly, yearly, all-time)" & vbCrLf &
                "• Battery monitoring and history charts" & vbCrLf &
                "• Comprehensive logging system" & vbCrLf & vbCrLf &
                "Data Sources:" & vbCrLf &
                "• Primary: UDP broadcasts from WeatherFlow Tempest Hub (Port 50222)" & vbCrLf &
                "• Optional: MeteoBridge integration for historical rain data" & vbCrLf & vbCrLf &
                "Requirements:" & vbCrLf &
                "• WeatherFlow Tempest Weather Station with Hub" & vbCrLf &
                "• Windows 10/11 (version 10.0.22000.0 or higher)" & vbCrLf &
                "• Local network connection to Tempest Hub" & vbCrLf &
                "• .NET 10.0 Runtime"
            ),
            "overview", "introduction", "about", "features"
        ) With {.SortOrder = 1})
    End Sub

    Private Sub AddTabPageTopics()
        _topics.Add(New HelpTopic(
            "tab_data",
            "Data Tab",
            "Tab Pages",
            "Navigation",
            CreateRtfContent(
                "Data Tab - Main Weather Display",
                "The Data tab is the primary view showing real-time weather conditions." & vbCrLf & vbCrLf &
                "Displayed Information:" & vbCrLf & vbCrLf &
                "Temperature Section:" & vbCrLf &
                "• Current Temperature - Large display with color-coded gauge" & vbCrLf &
                "• Feels Like - Heat index or wind chill calculation" & vbCrLf &
                "• Dew Point - Condensation temperature" & vbCrLf &
                "• Relative Humidity - Percentage with semi-circular gauge" & vbCrLf & vbCrLf &
                "Wind Section:" & vbCrLf &
                "• Wind Rose - 360° compass showing wind direction and speed" & vbCrLf &
                "• Current Wind Speed - Updates every 3 seconds" & vbCrLf &
                "• Average Wind Speed - 1-minute average" & vbCrLf &
                "• Wind Gust - Peak speed detected" & vbCrLf &
                "• Wind Lull - Minimum speed" & vbCrLf & vbCrLf &
                "Rain Section:" & vbCrLf &
                "• Precipitation Towers - Visual gauges for Today, Yesterday, Month, Year, All-time" & vbCrLf &
                "• Rain accumulation in inches" & vbCrLf &
                "• Configurable limits for gauge scaling" & vbCrLf & vbCrLf &
                "Update Frequency:" & vbCrLf &
                "• Wind data: Every 3 seconds via UDP" & vbCrLf &
                "• Full observations: Every 60 seconds via UDP" & vbCrLf &
                "• Display updates: Real-time as data arrives"
            ),
            "data", "main", "display", "current", "weather"
        ) With {.SortOrder = 1})

        _topics.Add(New HelpTopic(
            "tab_extra_data",
            "Extra Data Tab",
            "Tab Pages",
            "Navigation",
            CreateRtfContent(
                "Extra Data Tab - Detailed Observations",
                "The Extra Data tab displays comprehensive weather observations and detailed sensor readings." & vbCrLf & vbCrLf &
                "Main Sections:" & vbCrLf & vbCrLf &
                "Precipitation Data:" & vbCrLf &
                "• Rain Today Minutes - Duration of rainfall today" & vbCrLf &
                "• Rain Yesterday Minutes - Duration of rainfall yesterday" & vbCrLf & vbCrLf &
                "Lightning Data:" & vbCrLf &
                "• Strike Count - Total lightning strikes detected" & vbCrLf &
                "• Last Hour Count - Strikes in past 60 minutes" & vbCrLf &
                "• Last 3hr Count - Strikes in past 3 hours" & vbCrLf &
                "• Last Distance - Distance to most recent strike (miles)" & vbCrLf &
                "• Last Strike Time - Timestamp of most recent detection" & vbCrLf & vbCrLf &
                "Wind Details:" & vbCrLf &
                "• Wind Speed - Current instantaneous reading" & vbCrLf &
                "• Average Wind Speed - 1-minute rolling average" & vbCrLf &
                "• Wind Gusts - Peak speeds detected" & vbCrLf &
                "• Wind Lull - Minimum speeds (in minutes)" & vbCrLf &
                "• Wind Direction - Cardinal direction and degrees" & vbCrLf & vbCrLf &
                "Atmospheric Data:" & vbCrLf &
                "• UV Index - Solar ultraviolet radiation level" & vbCrLf &
                "• Solar Radiation - W/m² reading" & vbCrLf &
                "• Brightness - Lux measurement" & vbCrLf &
                "• Barometric Pressure - Current reading in mb/inHg" & vbCrLf &
                "• Pressure Trend - 3-hour change (Rising/Falling/Steady)" & vbCrLf & vbCrLf &
                "Calculated Values:" & vbCrLf &
                "• Air Density - kg/m³ with category (Thin/Average/Dense)" & vbCrLf &
                "• Cloud Base - Estimated ceiling height in feet" & vbCrLf & vbCrLf &
                "Status Information:" & vbCrLf &
                "• Hub IP Address - Local network address of Tempest hub" & vbCrLf &
                "• Last Update - Timestamp of most recent data" & vbCrLf &
                "• Battery Voltage - Tempest station power level" & vbCrLf & vbCrLf &
                "Data Grids:" & vbCrLf &
                "• Observation Values (DgvObsSt) - Detailed sensor readings" & vbCrLf &
                "• Hub Status (DgvHubStatus) - Hub connectivity and status information"
            ),
            "extra", "detailed", "observations", "lightning", "atmospheric"
        ) With {.SortOrder = 2})

        _topics.Add(New HelpTopic(
            "tab_records",
            "Records Tab",
            "Tab Pages",
            "Navigation",
            CreateRtfContent(
                "Records Tab - High/Low Tracking",
                "The Records tab displays historical high and low records for your weather station." & vbCrLf & vbCrLf &
                "Record Categories:" & vbCrLf & vbCrLf &
                "Rain Records:" & vbCrLf &
                "• Highest daily rainfall" & vbCrLf &
                "• Date and amount in inches" & vbCrLf & vbCrLf &
                "Temperature Records:" & vbCrLf &
                "• Highest temperature recorded" & vbCrLf &
                "• Lowest temperature recorded" & vbCrLf &
                "• Date and temperature in °F" & vbCrLf & vbCrLf &
                "Wind Records:" & vbCrLf &
                "• Highest wind gust recorded" & vbCrLf &
                "• Date and speed in mph" & vbCrLf & vbCrLf &
                "Data Storage:" & vbCrLf &
                "• Records stored in HiLo.db SQLite database" & vbCrLf &
                "• Location: [Application Folder]\\Data\\HiLo.db" & vbCrLf &
                "• Automatically updated when new records are detected" & vbCrLf & vbCrLf &
                "Grid Display:" & vbCrLf &
                "• Three columns: Rain, Temperature, Wind" & vbCrLf &
                "• Each row shows record type (high/low)" & vbCrLf &
                "• Values displayed with date and time" & vbCrLf & vbCrLf &
                "Record Updates:" & vbCrLf &
                "• Checked automatically with each new observation" & vbCrLf &
                "• Records persist across application restarts" & vbCrLf &
                "• Can be manually reset if needed" & vbCrLf & vbCrLf &
                "Note: Records begin accumulating from the first time the application runs. Historical data before installation is not included."
            ),
            "records", "high", "low", "history", "tracking"
        ) With {.SortOrder = 3})

        _topics.Add(New HelpTopic(
            "tab_charts",
            "Charts Tab",
            "Tab Pages",
            "Navigation",
            CreateRtfContent(
                "Charts Tab - Battery History",
                "The Charts tab displays graphical representations of weather data over time." & vbCrLf & vbCrLf &
                "Battery Voltage Chart:" & vbCrLf &
                "• 24-Hour History - Shows battery voltage over the past day" & vbCrLf &
                "• X-Axis: Time (hourly intervals)" & vbCrLf &
                "• Y-Axis: Voltage (V)" & vbCrLf &
                "• Chart Type: Line chart with area fill" & vbCrLf & vbCrLf &
                "Battery Status Indicators:" & vbCrLf &
                "• Green: Good battery health (>2.7V)" & vbCrLf &
                "• Yellow: Moderate battery (2.5-2.7V)" & vbCrLf &
                "• Orange: Low battery (2.3-2.5V)" & vbCrLf &
                "• Red: Critical battery (<2.3V)" & vbCrLf & vbCrLf &
                "Data Collection:" & vbCrLf &
                "• Battery voltage recorded with each observation" & vbCrLf &
                "• Data stored in BatteryHistory.json" & vbCrLf &
                "• Location: [Application Folder]\\Data\\BatteryHistory.json" & vbCrLf &
                "• Automatic cleanup of data older than 24 hours" & vbCrLf & vbCrLf &
                "Chart Features:" & vbCrLf &
                "• Automatic scaling based on voltage range" & vbCrLf &
                "• Grid lines for easy reading" & vbCrLf &
                "• Legend showing data series" & vbCrLf &
                "• Title and axis labels" & vbCrLf & vbCrLf &
                "Why Monitor Battery:" & vbCrLf &
                "• Tempest is solar-powered with battery backup" & vbCrLf &
                "• Low battery can affect data accuracy" & vbCrLf &
                "• Helps identify charging issues or failing batteries" & vbCrLf &
                "• Typical battery life: 3-6 months" & vbCrLf & vbCrLf &
                "Troubleshooting:" & vbCrLf &
                "• If chart is empty, ensure data is being received via UDP" & vbCrLf &
                "• Chart initializes when Charts tab is first accessed" & vbCrLf &
                "• Data accumulates over time as observations arrive"
            ),
            "charts", "battery", "history", "graph", "voltage"
        ) With {.SortOrder = 4})

        _topics.Add(New HelpTopic(
            "tab_logs",
            "Logs Tab",
            "Tab Pages",
            "Navigation",
            CreateRtfContent(
                "Logs Tab - Application and UDP Logs",
                "The Logs tab provides access to application logs and UDP message history." & vbCrLf & vbCrLf &
                "Log File List (Left Panel):" & vbCrLf &
                "• Lists all log files in Logs folder" & vbCrLf &
                "• Sorted by date (most recent first)" & vbCrLf &
                "• Click a file to view its contents" & vbCrLf & vbCrLf &
                "Log Viewer (Right Panel):" & vbCrLf &
                "• Displays selected log file contents" & vbCrLf &
                "• Read-only view with scrolling" & vbCrLf &
                "• Syntax highlighting for easy reading" & vbCrLf & vbCrLf &
                "Search Functionality:" & vbCrLf &
                "• Search box: Enter text to find in logs" & vbCrLf &
                "• Find button: Locates first occurrence" & vbCrLf &
                "• Find Next button: Moves to next match" & vbCrLf &
                "• Case-insensitive search" & vbCrLf & vbCrLf &
                "Log File Types:" & vbCrLf & vbCrLf &
                "Application Logs (td_YYYY-MM-DD.log):" & vbCrLf &
                "• Application startup and shutdown" & vbCrLf &
                "• Settings changes" & vbCrLf &
                "• Errors and warnings" & vbCrLf &
                "• Data processing events" & vbCrLf &
                "• File operations" & vbCrLf & vbCrLf &
                "UDP Logs (udp_YYYY-MM-DD.log):" & vbCrLf &
                "• Raw UDP packets received from Tempest hub" & vbCrLf &
                "• obs_st messages (full observations)" & vbCrLf &
                "• rapid_wind messages (wind updates)" & vbCrLf &
                "• hub_status messages (hub connectivity)" & vbCrLf &
                "• evt_strike messages (lightning detections)" & vbCrLf & vbCrLf &
                "Log Entry Format:" & vbCrLf &
                "[YYYY-MM-DD HH:MM:SS] [LEVEL] Category: Message" & vbCrLf & vbCrLf &
                "Example Entries:" & vbCrLf &
                "[2026-01-09 14:23:45] [INFO] UDP: Observation received" & vbCrLf &
                "[2026-01-09 14:23:50] [WARN] API: Fetch timeout after 30s" & vbCrLf &
                "[2026-01-09 14:24:00] [ERROR] Network: Connection refused" & vbCrLf & vbCrLf &
                "Context Menu:" & vbCrLf &
                "• Right-click in log viewer for options" & vbCrLf &
                "• Copy - Copy selected text to clipboard" & vbCrLf &
                "• Select All - Select entire log contents" & vbCrLf &
                "• Open Log Folder - Open Logs directory in Explorer" & vbCrLf & vbCrLf &
                "Log Retention:" & vbCrLf &
                "• Configured in Settings tab (Log Days)" & vbCrLf &
                "• Default: 5 days" & vbCrLf &
                "• Older logs automatically deleted" & vbCrLf & vbCrLf &
                "Storage Location:" & vbCrLf &
                "[Application Folder]\\Logs\\" & vbCrLf & vbCrLf &
                "Troubleshooting Tips:" & vbCrLf &
                "• Check UDP logs to verify data is being received" & vbCrLf &
                "• Application logs show errors and warnings" & vbCrLf &
                "• Search for 'ERROR' to find problems quickly" & vbCrLf &
                "• Recent logs show current application status"
            ),
            "logs", "logging", "viewing", "search", "troubleshooting"
        ) With {.SortOrder = 5})

        _topics.Add(New HelpTopic(
            "tab_settings",
            "Settings Tab",
            "Tab Pages",
            "Navigation",
            CreateRtfContent(
                "Settings Tab - Configuration",
                "The Settings tab allows you to configure all application settings." & vbCrLf & vbCrLf &
                "Settings Groups:" & vbCrLf & vbCrLf &
                "MeteoBridge Settings:" & vbCrLf &
                "• Login - Username for MeteoBridge device" & vbCrLf &
                "• Password - Password for MeteoBridge device" & vbCrLf &
                "• IP Address - Local network IP of MeteoBridge" & vbCrLf &
                "• Optional: Only needed if using MeteoBridge for historical rain data" & vbCrLf & vbCrLf &
                "Tempest Settings:" & vbCrLf &
                "• Device ID - Your Tempest station device ID (ST-xxxxx)" & vbCrLf &
                "• API Token - Your WeatherFlow API token" & vbCrLf &
                "• Update Interval - Seconds between supplemental API calls (180-1800)" & vbCrLf &
                "• Required: Primary settings for Tempest integration" & vbCrLf & vbCrLf &
                "Rain Gauge Limits:" & vbCrLf &
                "• Today - Maximum daily rainfall for gauge display" & vbCrLf &
                "• Yesterday - Maximum yesterday rainfall" & vbCrLf &
                "• Month - Maximum monthly rainfall" & vbCrLf &
                "• Year - Maximum yearly rainfall" & vbCrLf &
                "• All-time - Maximum historical rainfall" & vbCrLf &
                "• Purpose: Scale visual gauges for your climate" & vbCrLf & vbCrLf &
                "Log Settings:" & vbCrLf &
                "• Days to Keep Log Files - Log retention period (1-30 days)" & vbCrLf &
                "• Controls automatic log file cleanup" & vbCrLf & vbCrLf &
                "Station Data:" & vbCrLf &
                "• Name - Friendly name for your weather station" & vbCrLf &
                "• Station ID - Your WeatherFlow station ID" & vbCrLf &
                "• Elevation (Ft) - Station elevation in feet above sea level" & vbCrLf &
                "• Used for pressure calculations and display" & vbCrLf & vbCrLf &
                "Settings File Preview:" & vbCrLf &
                "• Right panel shows current AppSettings.json contents" & vbCrLf &
                "• Read-only view of active configuration" & vbCrLf &
                "• Updates automatically when settings change" & vbCrLf & vbCrLf &
                "Saving Settings:" & vbCrLf &
                "• Settings are saved automatically as you make changes" & vbCrLf &
                "• Stored in: [Application Folder]\\appSettings\\TempestDisplay_Settings.json" & vbCrLf &
                "• Backup file created: TempestDisplay_Settings.bak.json" & vbCrLf & vbCrLf &
                "Settings Validation:" & vbCrLf &
                "• Values are validated when entered" & vbCrLf &
                "• Invalid values are rejected or corrected" & vbCrLf &
                "• Default values used if settings file is missing" & vbCrLf & vbCrLf &
                "Troubleshooting:" & vbCrLf &
                "• If settings don't save, check file permissions" & vbCrLf &
                "• Backup file can be restored if needed" & vbCrLf &
                "• Settings file is plain JSON (can be edited manually)" & vbCrLf &
                "• Changes take effect immediately (some may require restart)"
            ),
            "settings", "configuration", "setup", "preferences"
        ) With {.SortOrder = 6})
    End Sub

    Private Sub AddGeneralSettingsTopics()
        _topics.Add(New HelpTopic(
            "settings_overview",
            "TempestDisplay Settings Overview",
            "General Settings",
            "Settings",
            CreateRtfContent(
                "TempestDisplay Settings Overview",
                "TempestDisplay stores all settings in AppSettings.json file located in the appSettings folder." & vbCrLf & vbCrLf &
                "Key settings include:" & vbCrLf &
                "• Station Information - Name, ID, and elevation" & vbCrLf &
                "• Tempest Configuration - Device ID, API token, update intervals" & vbCrLf &
                "• MeteoBridge Integration - IP address and credentials" & vbCrLf &
                "• Rain Gauge Limits - Maximum values for visual displays" & vbCrLf &
                "• Logging - Log retention period" & vbCrLf & vbCrLf &
                "Settings are automatically saved when you make changes and are loaded when the application starts."
            ),
            "settings", "overview", "configuration", "general"
        ) With {.SortOrder = 1})

        _topics.Add(New HelpTopic(
            "settings_station_name",
            "Station Name",
            "General Settings",
            "Settings",
            CreateRtfContent(
                "Station Name",
                "Setting: Station Name" & vbCrLf &
                "Location: Settings tab" & vbCrLf & vbCrLf &
                "A friendly name for your weather station. This name:" & vbCrLf &
                "• Appears in the application title bar" & vbCrLf &
                "• Is shown in logs and reports" & vbCrLf &
                "• Helps identify your station if you have multiple locations" & vbCrLf & vbCrLf &
                "Examples:" & vbCrLf &
                "• ""CarolinaWx"" (default)" & vbCrLf &
                "• ""Home Weather Station""" & vbCrLf &
                "• ""Smith Family Tempest""" & vbCrLf & vbCrLf &
                "Tip: Keep it short and descriptive."
            ),
            "station", "name", "identifier", "title"
        ) With {.SortOrder = 10})

        _topics.Add(New HelpTopic(
            "settings_station_id",
            "Station ID",
            "General Settings",
            "Settings",
            CreateRtfContent(
                "Station ID",
                "Setting: Station ID" & vbCrLf &
                "Location: Settings tab" & vbCrLf & vbCrLf &
                "The Station ID is the unique identifier assigned by WeatherFlow to your weather station. This ID is used to retrieve data from the WeatherFlow API." & vbCrLf & vbCrLf &
                "Where to find your Station ID:" & vbCrLf &
                "1. Log in to tempestwx.com" & vbCrLf &
                "2. Click on your station" & vbCrLf &
                "3. Look in the URL: tempestwx.com/station/[STATION_ID]" & vbCrLf &
                "4. Or find it in Settings > Stations in the Tempest app" & vbCrLf & vbCrLf &
                "Format: Numeric ID (e.g., 12345)" & vbCrLf & vbCrLf &
                "Note: This is different from your Device ID."
            ),
            "station", "id", "identifier", "weatherflow", "station number"
        ) With {.SortOrder = 11})

        _topics.Add(New HelpTopic(
            "settings_elevation",
            "Station Elevation",
            "General Settings",
            "Settings",
            CreateRtfContent(
                "Station Elevation",
                "Setting: Station Elevation" & vbCrLf &
                "Location: Settings tab" & vbCrLf & vbCrLf &
                "The elevation of your weather station in feet above sea level. This value is used for barometric pressure calculations and weather trend analysis." & vbCrLf & vbCrLf &
                "Why elevation matters:" & vbCrLf &
                "• Accurate pressure readings require elevation correction" & vbCrLf &
                "• Sea level pressure calculations depend on elevation" & vbCrLf &
                "• Weather forecasting algorithms use elevation data" & vbCrLf & vbCrLf &
                "How to find your elevation:" & vbCrLf &
                "• Use Google Maps (right-click location → ""What's here?"")" & vbCrLf &
                "• Check USGS elevation finder" & vbCrLf &
                "• Use a GPS device" & vbCrLf &
                "• Check your Tempest station settings" & vbCrLf & vbCrLf &
                "Format: Numeric value in feet (e.g., 850)" & vbCrLf & vbCrLf &
                "Note: Use the elevation of your station's location, not your home's indoor elevation."
            ),
            "elevation", "altitude", "feet", "sea level", "pressure"
        ) With {.SortOrder = 12})

        _topics.Add(New HelpTopic(
            "settings_update_interval",
            "Update Interval",
            "General Settings",
            "Settings",
            CreateRtfContent(
                "Tempest Update Interval",
                "Setting: Update Interval" & vbCrLf &
                "Location: Settings tab" & vbCrLf &
                "Range: 180-1800 seconds (3-30 minutes)" & vbCrLf &
                "Default: 300 seconds (5 minutes)" & vbCrLf & vbCrLf &
                "This setting controls how often TempestDisplay fetches new data from the WeatherFlow API. More frequent updates provide fresher data but consume more API calls." & vbCrLf & vbCrLf &
                "Recommended intervals:" & vbCrLf &
                "• 180 seconds (3 min) - Real-time monitoring during storms" & vbCrLf &
                "• 300 seconds (5 min) - Normal daily use (Recommended)" & vbCrLf &
                "• 600 seconds (10 min) - Casual monitoring" & vbCrLf &
                "• 1800 seconds (30 min) - Minimal API usage" & vbCrLf & vbCrLf &
                "Note: The Tempest station broadcasts UDP data locally every minute, regardless of this API fetch interval. This setting only affects cloud data retrieval." & vbCrLf & vbCrLf &
                "API Considerations:" & vbCrLf &
                "• WeatherFlow has daily API call limits" & vbCrLf &
                "• Lower intervals = more API calls per day" & vbCrLf &
                "• Balance freshness with API quota"
            ),
            "update", "interval", "refresh", "api", "frequency", "polling"
        ) With {.SortOrder = 20})

        _topics.Add(New HelpTopic(
            "settings_rain_limits",
            "Rain Gauge Limits",
            "Rain Gauge Settings",
            "Settings",
            CreateRtfContent(
                "Rain Gauge Limits",
                "Setting: Rain Gauge Limits" & vbCrLf &
                "Location: Settings tab" & vbCrLf & vbCrLf &
                "These settings define the maximum values for the rain gauge displays (Precipitation Towers Control). " &
                "When actual rainfall exceeds these limits, the gauge will show the maximum value." & vbCrLf & vbCrLf &
                "Available Limits:" & vbCrLf &
                "• Today Limit: Daily rainfall maximum (default: 2.0 inches, range: 0.1-1000)" & vbCrLf &
                "• Yesterday Limit: Previous day maximum (default: 2.0 inches, range: 0.1-1000)" & vbCrLf &
                "• Month Limit: Monthly rainfall maximum (default: 15.0 inches, range: 1-1000)" & vbCrLf &
                "• Year Limit: Annual rainfall maximum (default: 60.0 inches, range: 1-1000)" & vbCrLf &
                "• All-time Limit: Historical maximum (default: 400.0 inches, range: 1-10000)" & vbCrLf & vbCrLf &
                "Regional Guidelines:" & vbCrLf &
                "• Arid climates (Phoenix, AZ): Daily=3, Month=5, Year=15" & vbCrLf &
                "• Tropical (Miami, FL): Daily=10, Month=25, Year=75" & vbCrLf &
                "• Pacific Northwest (Seattle, WA): Daily=5, Month=15, Year=50" & vbCrLf & vbCrLf &
                "Tip: Set limits based on your local climate for better gauge resolution."
            ),
            "rain", "rainfall", "gauge", "limits", "precipitation"
        ) With {.SortOrder = 30})
    End Sub

    Private Sub AddApiSettingsTopics()
        _topics.Add(New HelpTopic(
            "api_tempest_overview",
            "Tempest API Configuration",
            "API Settings",
            "Settings",
            CreateRtfContent(
                "Tempest API Configuration",
                "TempestDisplay receives real-time weather data via UDP broadcasts from your WeatherFlow Tempest hub." & vbCrLf & vbCrLf &
                "Required Settings:" & vbCrLf &
                "• Device ID - Your Tempest station device identifier (e.g., ST-00012345)" & vbCrLf &
                "• API Token - Your personal WeatherFlow API token" & vbCrLf &
                "• Station ID - Your station ID from tempestwx.com" & vbCrLf &
                "• Update Interval - How often to request additional data (180-1800 seconds)" & vbCrLf & vbCrLf &
                "Note: UDP broadcasts provide real-time data every 3-60 seconds. The update interval controls supplemental API calls only."
            ),
            "api", "tempest", "weatherflow", "configuration"
        ) With {.SortOrder = 1})

        _topics.Add(New HelpTopic(
            "api_tempest_token",
            "Tempest API Token",
            "API Settings",
            "Settings",
            CreateRtfContent(
                "Tempest API Token",
                "Setting: Tempest API Token" & vbCrLf &
                "Location: Settings tab" & vbCrLf &
                "Required: Yes" & vbCrLf & vbCrLf &
                "Your personal API token for accessing WeatherFlow Tempest data. This token authenticates your application with the WeatherFlow cloud services." & vbCrLf & vbCrLf &
                "How to obtain your API token:" & vbCrLf &
                "1. Go to tempestwx.com" & vbCrLf &
                "2. Log in to your account" & vbCrLf &
                "3. Click on Settings (gear icon)" & vbCrLf &
                "4. Select ""Data Authorizations""" & vbCrLf &
                "5. Click ""Create Token""" & vbCrLf &
                "6. Give it a name (e.g., ""TempestDisplay"")" & vbCrLf &
                "7. Copy the generated token" & vbCrLf & vbCrLf &
                "Token Format:" & vbCrLf &
                "• Long alphanumeric string" & vbCrLf &
                "• Example: ""a1b2c3d4-e5f6-7890-abcd-ef1234567890""" & vbCrLf & vbCrLf &
                "Security Tips:" & vbCrLf &
                "• Never share your token publicly" & vbCrLf &
                "• Don't post it in forums or screenshots" & vbCrLf &
                "• Regenerate if compromised" & vbCrLf &
                "• Each app should have its own token" & vbCrLf & vbCrLf &
                "Troubleshooting:" & vbCrLf &
                "• ""Invalid token"" error: Verify you copied the entire token" & vbCrLf &
                "• ""API limit exceeded"": Check your update interval" & vbCrLf &
                "• No data: Ensure token is active in tempestwx.com settings"
            ),
            "token", "api", "tempest", "weatherflow", "authentication", "key"
        ) With {.SortOrder = 10})

        _topics.Add(New HelpTopic(
            "api_device_id",
            "Device ID",
            "API Settings",
            "Settings",
            CreateRtfContent(
                "Tempest Device ID",
                "Setting: Device ID" & vbCrLf &
                "Location: Settings tab" & vbCrLf &
                "Required: Yes" & vbCrLf & vbCrLf &
                "The unique identifier for your physical Tempest weather station device. This is different from your Station ID." & vbCrLf & vbCrLf &
                "How to find your Device ID:" & vbCrLf &
                "Method 1 - Tempest App:" & vbCrLf &
                "1. Open the Tempest app on your phone" & vbCrLf &
                "2. Go to Settings" & vbCrLf &
                "3. Select ""Stations""" & vbCrLf &
                "4. Tap your station" & vbCrLf &
                "5. Select ""Status""" & vbCrLf &
                "6. Look for ""Device ID"" or ""Serial Number""" & vbCrLf & vbCrLf &
                "Method 2 - Web Portal:" & vbCrLf &
                "1. Log in to tempestwx.com" & vbCrLf &
                "2. Click Settings → Stations" & vbCrLf &
                "3. Select your station" & vbCrLf &
                "4. Device ID is shown in the device details" & vbCrLf & vbCrLf &
                "Method 3 - UDP Messages:" & vbCrLf &
                "• Check the TempestDisplay Logs tab" & vbCrLf &
                "• Look for ""obs_st"" messages" & vbCrLf &
                "• The device ID is in the ""serial_number"" field" & vbCrLf & vbCrLf &
                "Format:" & vbCrLf &
                "• Starts with ""ST-"" followed by numbers" & vbCrLf &
                "• Example: ""ST-00012345""" & vbCrLf & vbCrLf &
                "Common Issues:" & vbCrLf &
                "• Using Station ID instead of Device ID" & vbCrLf &
                "• Typos in the serial number" & vbCrLf &
                "• Wrong device if you have multiple stations"
            ),
            "device", "id", "serial", "number", "tempest", "identifier"
        ) With {.SortOrder = 11})

        _topics.Add(New HelpTopic(
            "api_meteobridge",
            "MeteoBridge Settings",
            "API Settings",
            "Settings",
            CreateRtfContent(
                "MeteoBridge Configuration",
                "Settings: MeteoBridge Login, Password, and IP Address" & vbCrLf &
                "Location: API Settings tab" & vbCrLf &
                "Required: No (alternative data source)" & vbCrLf & vbCrLf &
                "MeteoBridge is a hardware data logger that can aggregate data from multiple weather stations, including Tempest. If you have a MeteoBridge device, you can configure TempestDisplay to pull data from it." & vbCrLf & vbCrLf &
                "What is MeteoBridge?" & vbCrLf &
                "• Hardware device that collects weather data" & vbCrLf &
                "• Supports multiple station types" & vbCrLf &
                "• Local network access (no cloud required)" & vbCrLf &
                "• Can forward data to multiple services" & vbCrLf & vbCrLf &
                "Configuration Fields:" & vbCrLf & vbCrLf &
                "1. Login (Username):" & vbCrLf &
                "   • Your MeteoBridge admin username" & vbCrLf &
                "   • Default is often ""meteobridge""" & vbCrLf & vbCrLf &
                "2. Password:" & vbCrLf &
                "   • Your MeteoBridge admin password" & vbCrLf &
                "   • Set during initial MeteoBridge setup" & vbCrLf & vbCrLf &
                "3. IP Address:" & vbCrLf &
                "   • Local network IP of your MeteoBridge device" & vbCrLf &
                "   • Format: 192.168.1.XXX" & vbCrLf &
                "   • Find in your router's DHCP client list" & vbCrLf &
                "   • Or check MeteoBridge display/app" & vbCrLf & vbCrLf &
                "Do you need MeteoBridge?" & vbCrLf &
                "No, it's optional. Most users only need the Tempest API Token and Device ID. MeteoBridge is useful if you:" & vbCrLf &
                "• Want local-only data access (no internet required)" & vbCrLf &
                "• Have multiple weather stations to aggregate" & vbCrLf &
                "• Already own a MeteoBridge device" & vbCrLf &
                "• Need custom data processing/forwarding" & vbCrLf & vbCrLf &
                "Note: Leave these fields empty if you don't use MeteoBridge."
            ),
            "meteobridge", "login", "password", "ip", "address", "alternative"
        ) With {.SortOrder = 20})
    End Sub

    Private Sub AddRainGaugeTopics()
        _topics.Add(New HelpTopic(
            "rain_limits_overview",
            "Rain Gauge Limits",
            "Rain Gauge Settings",
            "Settings",
            CreateRtfContent(
                "Rain Gauge Limits",
                "Setting: Rain Gauge Limits" & vbCrLf &
                "Location: Settings tab" & vbCrLf & vbCrLf &
                "These settings define the maximum values for the rain gauge displays (Precipitation Towers Control). " &
                "When actual rainfall exceeds these limits, the gauge will show the maximum value." & vbCrLf & vbCrLf &
                "Available Limits:" & vbCrLf &
                "• Today Limit: Daily rainfall maximum (default: 2.0 inches, range: 0.1-1000)" & vbCrLf &
                "• Yesterday Limit: Previous day maximum (default: 2.0 inches, range: 0.1-1000)" & vbCrLf &
                "• Month Limit: Monthly rainfall maximum (default: 15.0 inches, range: 1-1000)" & vbCrLf &
                "• Year Limit: Annual rainfall maximum (default: 60.0 inches, range: 1-1000)" & vbCrLf &
                "• All-time Limit: Historical maximum (default: 400.0 inches, range: 1-10000)" & vbCrLf & vbCrLf &
                "Regional Guidelines:" & vbCrLf &
                "• Arid climates (Phoenix, AZ): Daily=3, Month=5, Year=15" & vbCrLf &
                "• Tropical (Miami, FL): Daily=10, Month=25, Year=75" & vbCrLf &
                "• Pacific Northwest (Seattle, WA): Daily=5, Month=15, Year=50" & vbCrLf & vbCrLf &
                "Tip: Set limits based on your local climate for better gauge resolution."
            ),
            "rain", "rainfall", "gauge", "limits", "precipitation"
        ) With {.SortOrder = 30})
    End Sub

    Private Sub AddLoggingTopics()
        _topics.Add(New HelpTopic(
            "logging_retention",
            "Log Retention Days",
            "Logging Settings",
            "Settings",
            CreateRtfContent(
                "Log Retention Period",
                "Setting: Days to Keep Log Files" & vbCrLf &
                "Location: Settings tab" & vbCrLf &
                "Range: 1-30 days" & vbCrLf &
                "Default: 5 days" & vbCrLf & vbCrLf &
                "This setting determines how long log files are kept before being automatically deleted. " &
                "Older log files are removed to prevent excessive disk usage." & vbCrLf & vbCrLf &
                "Recommended Values:" & vbCrLf &
                "• 5 days: Minimal disk usage (Recommended)" & vbCrLf &
                "• 7 days: One week of recent history" & vbCrLf &
                "• 14 days: Two weeks for troubleshooting" & vbCrLf &
                "• 30 days: Extended history (maximum)" & vbCrLf & vbCrLf &
                "Storage Location:" & vbCrLf &
                "Logs are stored in: [Application Folder]\\Logs\\" & vbCrLf & vbCrLf &
                "File Naming:" & vbCrLf &
                "td_YYYY-MM-DD.log (main application log)" & vbCrLf &
                "udp_YYYY-MM-DD.log (UDP message log)" & vbCrLf & vbCrLf &
                "Tip: If troubleshooting issues, temporarily increase to 14-30 days to capture more diagnostic data."
            ),
            "logging", "retention", "days", "logs", "maintenance"
        ) With {.SortOrder = 1})

        _topics.Add(New HelpTopic(
            "logging_level",
            "Log Detail Level",
            "Logging Settings",
            "Settings",
            CreateRtfContent(
                "Log Detail Level (Coming Soon)",
                "Control how much detail is recorded in log files." & vbCrLf & vbCrLf &
                "Available Levels:" & vbCrLf & vbCrLf &
                "1. Error Only:" & vbCrLf &
                "   • Records only errors and failures" & vbCrLf &
                "   • Minimal disk usage" & vbCrLf &
                "   • Use when everything is working normally" & vbCrLf & vbCrLf &
                "2. Warning:" & vbCrLf &
                "   • Errors plus warnings" & vbCrLf &
                "   • Catches potential issues" & vbCrLf &
                "   • Recommended for daily use" & vbCrLf & vbCrLf &
                "3. Info (Default):" & vbCrLf &
                "   • Errors, warnings, and informational messages" & vbCrLf &
                "   • Records data updates, connections, settings changes" & vbCrLf &
                "   • Good balance of detail and performance" & vbCrLf & vbCrLf &
                "4. Debug:" & vbCrLf &
                "   • Detailed technical information" & vbCrLf &
                "   • Shows API requests/responses" & vbCrLf &
                "   • Logs internal calculations" & vbCrLf &
                "   • Use for troubleshooting specific issues" & vbCrLf & vbCrLf &
                "5. Trace:" & vbCrLf &
                "   • Extremely verbose, every operation logged" & vbCrLf &
                "   • Large log files" & vbCrLf &
                "   • Only use when requested by support" & vbCrLf & vbCrLf &
                "Performance Impact:" & vbCrLf &
                "• Error/Warning: Negligible" & vbCrLf &
                "• Info: Minimal" & vbCrLf &
                "• Debug: Slight (may slow data updates)" & vbCrLf &
                "• Trace: Moderate (not recommended for continuous use)" & vbCrLf & vbCrLf &
                "Tip: Start with Info level. Change to Debug only when troubleshooting, then return to Info."
            ),
            "level", "detail", "verbose", "debug", "trace", "error"
        ) With {.SortOrder = 11})

        _topics.Add(New HelpTopic(
            "logging_viewing",
            "Viewing Logs",
            "Logging Settings",
            "Settings",
            CreateRtfContent(
                "How to View Log Files",
                "TempestDisplay provides multiple ways to view and analyze log files." & vbCrLf & vbCrLf &
                "Method 1: Logs Tab" & vbCrLf &
                "• Click the ""Logs"" tab in the main window" & vbCrLf &
                "• View real-time log entries" & vbCrLf &
                "• Filter by date, type, or keyword" & vbCrLf &
                "• Copy entries to clipboard" & vbCrLf & vbCrLf &
                "Method 2: Log Files Directly" & vbCrLf &
                "• Navigate to [Application Folder]\\Logs\\" & vbCrLf &
                "• Open .log files with any text editor" & vbCrLf &
                "• Files are plain text format" & vbCrLf & vbCrLf &
                "Method 3: Context Menu (in Logs Tab)" & vbCrLf &
                "• Right-click in the logs display" & vbCrLf &
                "• Options: Copy, Open Log Folder, Export" & vbCrLf & vbCrLf &
                "Log Entry Format:" & vbCrLf &
                "[YYYY-MM-DD HH:MM:SS] [LEVEL] Category: Message" & vbCrLf & vbCrLf &
                "Example:" & vbCrLf &
                "[2026-01-09 14:23:45] [INFO] API: Data fetched successfully" & vbCrLf &
                "[2026-01-09 14:23:50] [WARN] UDP: Packet timeout after 30s" & vbCrLf &
                "[2026-01-09 14:24:00] [ERROR] Network: Connection refused" & vbCrLf & vbCrLf &
                "Searching Logs:" & vbCrLf &
                "• Use the search box in the Logs tab" & vbCrLf &
                "• Search by: timestamp, level, category, message text" & vbCrLf &
                "• Regular expression support available" & vbCrLf & vbCrLf &
                "Exporting Logs:" & vbCrLf &
                "• Right-click → Export" & vbCrLf &
                "• Choose date range" & vbCrLf &
                "• Save as .txt or .csv" & vbCrLf &
                "• Useful for sending to support"
            ),
            "viewing", "reading", "logs", "display", "export", "search"
        ) With {.SortOrder = 20})
    End Sub

    Private Shared Function CreateRtfContent(title As String, content As String) As String
        ' Create properly formatted RTF content
        Dim rtf As New System.Text.StringBuilder()

        rtf.AppendLine("{\rtf1\ansi\deff0 {\fonttbl {\f0 Segoe UI;}{\f1 Consolas;}}")
        rtf.AppendLine("{\colortbl;\red0\green0\blue0;\red0\green102\blue204;\red102\green102\blue102;}")

        ' Title in bold, larger font
        rtf.AppendLine("\f0\fs28\b " & EscapeRtf(title) & "\b0\fs20")
        rtf.AppendLine("\par\par")

        ' Content with proper line breaks
        Dim lines = content.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
        For Each line In lines
            If String.IsNullOrWhiteSpace(line) Then
                rtf.AppendLine("\par")
            ElseIf line.StartsWith("•"c) Then
                ' Bullet point
                rtf.AppendLine("\bullet " & EscapeRtf(line.Substring(1).Trim()))
                rtf.AppendLine("\par")
            ElseIf line.EndsWith(":"c) Then
                ' Section header
                rtf.AppendLine("\b " & EscapeRtf(line) & "\b0")
                rtf.AppendLine("\par")
            Else
                ' Regular text
                rtf.AppendLine(EscapeRtf(line))
                rtf.AppendLine("\par")
            End If
        Next

        rtf.AppendLine("}"c)

        Return rtf.ToString()
    End Function

    Private Shared Function EscapeRtf(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return String.Empty
        End If

        Return text.Replace("\", "\\").Replace("{", "\{").Replace("}", "\}").Replace(vbCrLf, "\par ").Replace(vbLf, "\par ")
    End Function

End Class