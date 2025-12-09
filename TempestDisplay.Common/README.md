# TempestDisplay.Common DLL

## Overview

`TempestDisplay.Common` is a shared class library containing common calculations, unit conversions, and utility functions used by the TempestDisplay weather application. By moving these functions to a DLL, they can be:

- **Reused** across multiple projects
- **Tested** independently
- **Maintained** in one location
- **Versioned** separately from the main application

## Namespace Structure

```
TempestDisplay.Common
??? Weather
    ??? WeatherCalculations (Module)
    ??? UnitConversions (Module)
    ??? WeatherUtilities (Module)
```

## Modules

### 1. WeatherCalculations

Contains NWS (National Weather Service) standard weather calculation formulas.

#### Functions:

**Temperature Calculations:**
- `CalculateHeatIndex(tempF, humidity)` - Heat index using Rothfusz regression
- `CalculateWindChill(tempF, windSpeedMph)` - Wind chill using NWS 2001 formula
- `CalculateFeelsLike(tempF, humidity, windSpeedMph)` - Smart selection of heat index or wind chill
- `CalculateDewPoint(tempF, humidity)` - Dew point using Magnus-Tetens formula

**Air Density Calculations:**
- `CalculateAirDensity(tempF, pressureMb, humidity)` - Air density using Ideal Gas Law
- `GetAirDensityCategory(density)` - Human-readable density description
- `CalculateDensityAltitude(tempF, pressureMb, humidity, actualAltitudeFt)` - Aviation density altitude

**Category Functions:**
- `GetHeatIndexCategory(heatIndex)` - Heat index severity levels
- `GetWindChillCategory(windChill)` - Wind chill severity levels  
- `GetFeelsLikeLabel(tempF, windSpeedMph)` - Label for feels-like display

#### Example Usage:

```vb
Imports TempestDisplay.Common.Weather

' Calculate heat index
Dim tempF As Double = 95.0
Dim humidity As Double = 60.0
Dim heatIndex = WeatherCalculations.CalculateHeatIndex(tempF, humidity)
' Result: 112.0°F

' Calculate air density
Dim pressure As Double = 1013.25
Dim density = WeatherCalculations.CalculateAirDensity(tempF, pressure, humidity)
Dim category = WeatherCalculations.GetAirDensityCategory(density)
' Result: 1.204 kg/m³, "Average"
```

---

### 2. UnitConversions

Contains unit conversion functions for weather data.

#### Temperature Conversions:
- `CelsiusToFahrenheit(celsius)` - °C to °F
- `FahrenheitToCelsius(fahrenheit)` - °F to °C

#### Wind Speed Conversions:
- `MetersPerSecondToMph(mps)` - m/s to mph
- `MphToMetersPerSecond(mph)` - mph to m/s
- `MetersPerSecondToKph(mps)` - m/s to km/h
- `KphToMph(kph)` - km/h to mph

#### Pressure Conversions:
- `MillibarsToInHg(mb)` - Millibars to inches of mercury
- `InHgToMillibars(inHg)` - Inches of mercury to millibars
- `MillibarsToKPa(mb)` - Millibars to kilopascals

#### Precipitation Conversions:
- `MillimetersToInches(mm)` - mm to inches
- `InchesToMillimeters(inches)` - Inches to mm

#### Distance Conversions:
- `KilometersToMiles(km)` - km to miles
- `MilesToKilometers(miles)` - Miles to km

#### Direction Conversions:
- `DegreesToCardinal(degrees)` - 16-point compass (N, NNE, NE, etc.)
- `DegreesToCardinal8Point(degrees)` - 8-point compass (N, NE, E, etc.)
- `DegreesToFullCardinal(degrees)` - Full names (North, North-Northeast, etc.)

#### Example Usage:

```vb
Imports TempestDisplay.Common.Weather

' Convert temperature
Dim tempC As Double = 25.0
Dim tempF = UnitConversions.CelsiusToFahrenheit(tempC)
' Result: 77.0°F

' Convert wind speed
Dim windMps As Double = 5.0
Dim windMph = UnitConversions.MetersPerSecondToMph(windMps)
' Result: 11.2 mph

' Convert wind direction to cardinal
Dim degrees As Integer = 135
Dim direction = UnitConversions.DegreesToCardinal(degrees)
' Result: "SE"
```

---

### 3. WeatherUtilities

Contains validation, formatting, and helper functions.

#### Validation Functions:
- `IsValidTemperature(tempF)` - Range: -100°F to 150°F
- `IsValidHumidity(humidity)` - Range: 0% to 100%
- `IsValidWindSpeed(windSpeedMph)` - Range: 0 to 200 mph
- `IsValidWindDirection(degrees)` - Range: 0° to 360°
- `IsValidPressure(pressureInHg)` - Range: 26 to 32 inHg

#### Formatting Functions:
- `FormatTemperature(tempF, decimals)` - Returns "72.5°F"
- `FormatHumidity(humidity)` - Returns "65%"
- `FormatWindSpeed(windSpeedMph, decimals)` - Returns "15.2 mph"
- `FormatPressure(pressureInHg, decimals)` - Returns "29.92 inHg"
- `FormatPrecipitation(inches, decimals)` - Returns "0.25\"" or "Trace"

#### Descriptive Functions:
- `GetComfortLevel(tempF, humidity)` - Comfort assessment
- `GetDewPointComfort(dewPointF)` - Dew point comfort level
- `GetBeaufortScale(windSpeedMph)` - Beaufort wind scale
- `GetUVIndexCategory(uvIndex)` - UV index with recommendations
- `GetVisibilityEstimate(humidity, isRaining)` - Visibility estimate

#### Example Usage:

```vb
Imports TempestDisplay.Common.Weather

' Validate data
Dim temp As Double = 72.5
If WeatherUtilities.IsValidTemperature(temp) Then
    Dim formatted = WeatherUtilities.FormatTemperature(temp)
    ' Result: "72.5°F"
End If

' Get comfort level
Dim humidity As Double = 65.0
Dim comfort = WeatherUtilities.GetComfortLevel(temp, humidity)
' Result: "Comfortable"

' Get Beaufort scale
Dim windSpeed As Double = 25.0
Dim beaufort = WeatherUtilities.GetBeaufortScale(windSpeed)
' Result: "5 - Fresh Breeze"
```

---

## Integration with TempestDisplay

The main `TempestDisplay` project now references `TempestDisplay.Common.dll`. For backward compatibility, the existing `WeatherCalculations` module in `TempestDisplay` acts as a thin wrapper that calls the DLL functions.

### Adding the Import:

```vb
Imports TempestDisplay.Common.Weather
```

### Direct Usage:

```vb
' Use DLL functions directly
Dim density = WeatherCalculations.CalculateAirDensity(tempF, pressure, humidity)
Dim cardinal = UnitConversions.DegreesToCardinal(180)
Dim formatted = WeatherUtilities.FormatTemperature(72.5)
```

### Backward Compatibility:

Existing code continues to work without modification:

```vb
' Old code still works (calls are forwarded to DLL)
Dim heatIndex = CalculateHeatIndex(95.0, 60.0)
Dim dewPoint = CalculateDewPoint(72.0, 55.0)
```

---

## Building the DLL

```bash
cd TempestDisplay.Common
dotnet build
```

Output: `bin\Debug\net10.0-windows\TempestDisplay.Common.dll`

---

## Testing

Create a test project:

```bash
dotnet new xunit -n TempestDisplay.Common.Tests
cd TempestDisplay.Common.Tests
dotnet add reference ..\TempestDisplay.Common\TempestDisplay.Common.vbproj
dotnet build
dotnet test
```

---

## Version History

### v1.0.0 (Initial Release)
- WeatherCalculations module with NWS formulas
- UnitConversions module with weather unit conversions
- WeatherUtilities module with validation and formatting
- Full XML documentation for IntelliSense

---

## Future Enhancements

Potential additions:
- Sunrise/sunset calculations
- Moon phase calculations
- Precipitation probability models
- Storm tracking utilities
- Historical data analysis functions
- Advanced meteorological calculations

---

## References

- **NWS Heat Index**: [NOAA Heat Index Calculator](https://www.wpc.ncep.noaa.gov/html/heatindex.shtml)
- **NWS Wind Chill**: [NOAA Wind Chill Calculator](https://www.weather.gov/safety/cold-wind-chill-chart)
- **Magnus Formula**: Accurate dew point calculation
- **Ideal Gas Law**: Air density calculations
- **Standard Atmosphere**: ICAO/NOAA standards

---

## License

© 2024 CarolinaWx - All rights reserved.

---

## Contact

For questions or issues with the TempestDisplay.Common library, please contact the development team.
