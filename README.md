Last Edit: February 17, 2026 (Documented UV/Solar daily peak reset)
# TempestDisplay

A professional Windows desktop application for displaying real-time weather data from WeatherFlow Tempest weather stations.

![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

### Performance Optimizations
- **Batched UI Updates** - Reduced cross-thread invokes during station updates
- **UDP Raw Message Dispatch** - Skips per-packet dispatch when no subscribers
- **Logging Pipeline** - Reduced locking and avoided queue writes after shutdown
- **UI Marshaling** - Avoids handle creation and skips disposed controls
- **Log List Population** - Uses `EnumerateFiles` to reduce allocations
- **Unified Cancellation** - UDP listener linked to global app token

### Real-Time Weather Monitoring
- **UDP Broadcast Listener** - Receives live data directly from your Tempest hub on your local network
- **Instant Updates** - Wind data every 3 seconds, full observations every minute
- **No Internet Required** - Works entirely on your local network

### Professional Custom Controls
- **Temperature Gauges** - Current temperature, feels-like, and dew point displays
- **Wind Rose** - 360° compass with color-coded wind speed indicator
- **Fan Gauge** - Semi-circular relative humidity display
- **Precipitation Towers** - Visual rain accumulation for today, yesterday, month, year, and all-time
- **Lightning Tracker** - Real-time lightning strike detection with distance
- **UV/Solar Meter** - Daily peak markers for UV index and solar radiation
- **Daily Reset** - UV/Solar daily peaks reset at midnight

### Advanced Weather Calculations
- **Air Density** - Calculated and categorized (Thin, Below Average, Average, Above Average, Dense)
- **Cloud Base** - Estimated cloud ceiling height
- **Pressure Trends** - 3-hour pressure change tracking with trend analysis
- **Heat Index** - Feels-like temperature calculations

### Data Integration
- **WeatherFlow Tempest Hub** - Direct UDP broadcasts (primary data source)
- **MeteoBridge** - Historical rain data integration
- **Automatic Fallback** - Graceful handling if UDP is unavailable

## Requirements

### Hardware
- **WeatherFlow Tempest Weather Station** with hub
- Windows PC on the same local network as the Tempest hub
- Recommended: 1920x1080 or higher display resolution

### Software
- **Windows 10/11** (version 10.0.22000.0 or higher)
- **.NET 10.0 Runtime** (included with Windows 11, or download from Microsoft)
- **Network Access** - UDP port 50222 must be open for Tempest broadcasts

### Optional
- **MeteoBridge** device for enhanced rain data tracking

## Installation

### Option 1: Build from Source

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TempestDisplay
   ```

2. **Open in Visual Studio 2022**
   - Double-click `TempestDisplay.slnx`
   - Or use Visual Studio Code with VB.NET extensions

3. **Build the solution**
   ```bash
   dotnet build TempestDisplay.slnx
   ```

4. **Run the application**
   ```bash
   dotnet run --project TempestDisplay\TempestDisplay.vbproj
   ```

### Option 2: Run Pre-built Release
1. Download the latest release from the Releases page
2. Extract to a folder
3. Run `TempestDisplay.exe`

## Configuration

### First-Time Setup

1. **Launch TempestDisplay**
   - The app will create a `Data` folder in the application directory
   - Settings file: `Data\AppSettings.json`

2. **Configure Tempest Settings**
   - Go to the **Settings** tab
   - Enter your **Station ID** (found in Tempest app)
   - Enter your **Device ID** (found in Tempest app)
   - Enter your **API Token** (from tempestwx.com)
   - Enter **Station Name** and **Elevation**

3. **Configure MeteoBridge (Optional)**
   - Enter MeteoBridge **IP Address**
   - Enter **Username** and **Password**
   - This enables historical rain data tracking

4. **Firewall Configuration**
   - The app needs to receive UDP broadcasts on port 50222
   - Run the included firewall script (requires Administrator):
     ```powershell
     cd TempestDisplay\Modules\UdpListener
     .\Configure-Firewall.bat
     ```
   - Or manually allow `TempestDisplay.exe` through Windows Firewall

### Settings File Location
```
C:\VB18\TempestDisplay\Data\AppSettings.json
```

### Settings File Format
```json
{
  "Tempest": {
    "StationId": "12345",
    "DeviceId": "67890",
    "ApiToken": "your-api-token-here",
    "StationName": "My Weather Station",
    "StationElevation": 500,
    "UpdateIntervalSeconds": 60
  },
  "MeteoBridge": {
    "IpAddress": "192.168.1.100",
    "Username": "meteobridge",
    "Password": "your-password"
  },
  "Logging": {
    "RetentionDays": 7
  },
  "RainGaugeLimits": {
    "TodayLimit": 2.0,
    "YesterdayLimit": 2.0,
    "MonthLimit": 10.0,
    "YearLimit": 50.0,
    "AllTimeLimit": 200.0
  }
}
```

## Usage

### Main Dashboard (Data Tab)

The main screen displays:

#### Temperature Section
- **Current Temperature** - Large display with color-coded gauge
- **Feels Like** - Heat index or wind chill
- **Dew Point** - Condensation temperature
- **Relative Humidity** - Percentage with semi-circular gauge

#### Wind Section
- **Wind Rose** - 360° compass showing wind direction and speed
  - Gray arrow: Calm (< 5 mph)
  - Blue arrow: Light wind (5-15 mph)
  - Gold arrow: Moderate wind (15-25 mph)
  - Red arrow: Strong wind (> 25 mph)
- **Current Wind Speed** - Instantaneous (updates every 3 seconds)
- **Average Wind Speed** - 1-minute average
- **Wind Gust** - Peak speed
- **Wind Lull** - Minimum speed
- **Wind Direction** - Cardinal direction and degrees

#### Atmospheric Section
- **Barometric Pressure** - Current reading in inHg or mb
- **Pressure Trend** - 3-hour change (Rising/Falling/Steady)
- **Air Density** - Calculated density with category
- **Cloud Base** - Estimated ceiling height

#### Light & Solar Section
- **UV Index** - Current UV level
- **Solar Radiation** - W/m²
- **Brightness** - Lux reading

#### Precipitation Section
- **Rain Towers** - Visual gauges for:
  - Today's accumulation
  - Yesterday's accumulation
  - Month total
  - Year total
  - All-time total
- **Rain Minutes** - Time it rained today/yesterday

#### Lightning Section
- **Last Strike** - Time of most recent detection
- **Distance** - How far away in miles
- **Strike Counts** - Total, last hour, last 3 hours

#### Status Information
- **Hub IP Address** - Local network address of Tempest hub
- **Battery Status** - Voltage and level
- **Last Update** - Timestamp of most recent data

### Logs Tab

View system activity:
- **Application Logs** - Startup, errors, and general operations
- **UDP Message History** - Raw observation packets received
- **Charts** - Battery voltage over time

### Settings Tab

Configure all application settings:
- **Station Information** - Name, ID, elevation
- **API Credentials** - Tempest API token
- **MeteoBridge** - IP, username, password
- **Rain Gauge Limits** - Maximum values for visual gauges
- **Log Retention** - How many days to keep logs

## Network Requirements

### UDP Port 50222
TempestDisplay listens for UDP broadcasts on port **50222** from the Tempest hub.

**Ensure:**
- Your PC and Tempest hub are on the same local network
- Windows Firewall allows UDP port 50222 inbound
- No router firewall blocks UDP broadcasts on the LAN

### Testing Connectivity

1. **Check Hub Status**
   - Look at the **Hub IP** display on the main screen
   - If it shows an IP address, you're receiving data

2. **View Raw Messages**
   - Go to **Logs** ? **UDP History**
   - You should see JSON packets appearing in real-time

3. **Check Windows Firewall**
   ```powershell
   Get-NetFirewallRule -DisplayName "TempestDisplay*"
   ```

## Troubleshooting

### No Data Received

**Problem:** Hub IP shows nothing, no data updates

**Solutions:**
1. Verify Tempest hub is powered on and connected to your network
2. Check that your PC is on the same network as the hub
3. Run the firewall configuration script
4. Manually check Windows Firewall settings
5. Try disabling firewall temporarily to test

### Control Not Showing

**Problem:** WindRoseControl or other custom control is blank

**Solutions:**
1. Check logs for initialization errors
2. Verify control is added to TlpData in Designer
3. Rebuild solution: `dotnet build --no-incremental`
4. Check that TempestDisplay.Controls.dll is in output folder

### MeteoBridge Connection Failed

**Problem:** Rain data not showing historical values

**Solutions:**
1. Verify MeteoBridge IP address is correct
2. Test credentials by accessing MeteoBridge web interface
3. Check that MeteoBridge is on same network
4. Review logs for specific error messages

### High CPU Usage

**Problem:** Application uses excessive CPU

**Solutions:**
1. Check that control refresh rates are appropriate
2. Verify no infinite loops in event handlers
3. Consider increasing UDP processing interval
4. Check for memory leaks in logs

## Architecture

### Project Structure

```
TempestDisplay/
??? TempestDisplay/                  # Main application
?   ??? FrmMain.vb                  # Main form
?   ??? FrmMain.Designer.vb         # Designer code
?   ??? FrmMain.Partials/           # Partial classes
?   ?   ??? FrmMain.UdpListener.vb  # UDP event handlers
?   ?   ??? FrmMain.ObservationUI.vb # UI update methods
?   ?   ??? FrmMain.Settings.vb     # Settings management
?   ??? Modules/
?   ?   ??? UdpListener/            # UDP broadcast listener
?   ?   ??? DataFetch/              # Data retrieval routines
?   ?   ??? Settings/               # Settings management
?   ?   ??? Logs/                   # Logging system
?   ?   ??? Weather/                # Weather calculations
?   ??? Models/                     # Data models
?   ??? Classes/                    # Service classes
??? TempestDisplay.Controls/         # Custom controls library
?   ??? Controls/
?       ??? TempGaugeControl.vb     # Temperature gauge
?       ??? FanGaugeControl.vb      # Humidity gauge
?       ??? WindRoseControl.vb      # Wind compass
?       ??? PrecipTowersControl.vb  # Rain gauge
??? TempestDisplay.Common/           # Shared utilities
    ??? Weather/
        ??? WeatherCalculations.vb  # Weather formulas
```

### Data Flow

```
Tempest Hub (UDP:50222)
    ?
WeatherFlowUdpListener
    ?
FrmMain.OnObservationReceived
    ?
ObservationParser.ParseObsStPacket
    ?
UpdateWeatherUI
    ?
[Individual Update Methods]
    ?
Custom Controls (WindRose, TempGauge, etc.)
```

### Key Technologies

- **VB.NET** - Primary language
- **.NET 10.0** - Framework
- **Windows Forms** - UI framework
- **System.Text.Json** - JSON parsing
- **UDP Sockets** - Network communication
- **GDI+** - Custom control rendering

## Development

### Prerequisites for Development

- **Visual Studio 2022** (Community or higher)
- **.NET 10.0 SDK**
- **Git** for version control

### Building

```bash
# Clean build
dotnet clean
dotnet build --configuration Release

# Debug build
dotnet build --configuration Debug

# Run tests (if available)
dotnet test
```

### Adding New Custom Controls

1. Create new control class in `TempestDisplay.Controls/Controls/`
2. Inherit from `Control` class
3. Override `OnPaint` for custom rendering
4. Add properties with `Browsable` attribute
5. Build the Controls project
6. Add control to `FrmMain.Designer.vb`
7. Initialize in `FrmMain.vb` ? `InitializeCustomControls()`

### Code Style

- Use meaningful variable names
- Comment complex calculations
- Follow existing naming conventions
- Keep methods focused and small
- Use `Try/Catch` for error handling
- Log all errors with `Log.WriteException`

## API References

### WeatherFlow Tempest API
- **UDP Protocol**: https://weatherflow.github.io/Tempest/api/udp/v171/
- **Derived Metrics**: https://apidocs.tempestwx.com/reference/derived-metrics
- **REST API**: https://weatherflow.github.io/Tempest/api/

### Weather Formulas
- **Heat Index**: National Weather Service formula
- **Wind Chill**: NWS wind chill formula (2001)
- **Dew Point**: Magnus formula
- **Air Density**: Ideal gas law with humidity correction
- **Cloud Base**: Espy's estimation

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow existing code style
- Add comments for complex logic
- Test on real Tempest hardware if possible
- Update documentation for new features
- Include screenshots for UI changes

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- **WeatherFlow** - For creating the excellent Tempest weather station
- **MeteoBridge** - For additional weather data integration
- Community contributors and testers

## Support

### Getting Help

- Check the **Troubleshooting** section above
- Review application logs in the **Logs** tab
- Search existing issues on GitHub
- Create a new issue with:
  - Description of the problem
  - Steps to reproduce
  - Log file contents
  - Screenshots (if applicable)

### Useful Links

- [WeatherFlow Community Forums](https://community.weatherflow.com/)
- [Tempest API Documentation](https://weatherflow.github.io/Tempest/api/)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)

## Roadmap

### Planned Features

- [ ] Historical data charts (temperature, pressure, rainfall)
- [ ] Weather alerts and notifications
- [ ] Export data to CSV/Excel
- [ ] Multiple station support
- [ ] Customizable dashboard layouts
- [ ] Dark mode theme
- [ ] Mobile companion app

### Version History

**v1.0.0** (Current)
- Initial release
- Real-time UDP monitoring
- Custom weather controls
- MeteoBridge integration
- Pressure trend tracking
- Lightning detection
- WindRoseControl for visual wind direction display

---

**Made with ?? for weather enthusiasts**

For questions or feedback, please open an issue on GitHub.
