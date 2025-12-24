''' <summary>
''' Weather calculation routines - now using TempestDisplay.Common DLL
''' This module provides backward compatibility by re-exporting functions from the DLL
''' </summary>
Public Module WeatherCalculations

    ' Re-export all functions from the DLL for backward compatibility
    ' This allows existing code to continue working without modification

    Public Function CalculateHeatIndex(tempF As Double, humidity As Double) As Double
        Return TempestDisplay.Common.Weather.WeatherCalculations.CalculateHeatIndex(tempF, humidity)
    End Function

    Public Function CalculateWindChill(tempF As Double, windSpeedMph As Double) As Double
        Return TempestDisplay.Common.Weather.WeatherCalculations.CalculateWindChill(tempF, windSpeedMph)
    End Function

    Public Function CalculateFeelsLike(tempF As Double, humidity As Double, windSpeedMph As Double) As Double
        Return TempestDisplay.Common.Weather.WeatherCalculations.CalculateFeelsLike(tempF, humidity, windSpeedMph)
    End Function

    Public Function CalculateDewPoint(tempF As Double, humidity As Double) As Double
        Return TempestDisplay.Common.Weather.WeatherCalculations.CalculateDewPoint(tempF, humidity)
    End Function

    Public Function CalculateAirDensity(tempF As Double, pressureMb As Double, humidity As Double) As Double
        Return TempestDisplay.Common.Weather.WeatherCalculations.CalculateAirDensity(tempF, pressureMb, humidity)
    End Function

    Public Function GetAirDensityCategory(density As Double) As String
        Return TempestDisplay.Common.Weather.WeatherCalculations.GetAirDensityCategory(density)
    End Function

    Public Function CalculateDensityAltitude(tempF As Double, pressureMb As Double, humidity As Double) As Double
        Return TempestDisplay.Common.Weather.WeatherCalculations.CalculateDensityAltitude(tempF, pressureMb, humidity)
    End Function

    Public Function GetHeatIndexCategory(heatIndex As Double) As String
        Return TempestDisplay.Common.Weather.WeatherCalculations.GetHeatIndexCategory(heatIndex)
    End Function

    Public Function GetWindChillCategory(windChill As Double) As String
        Return TempestDisplay.Common.Weather.WeatherCalculations.GetWindChillCategory(windChill)
    End Function

    Public Function GetFeelsLikeLabel(tempF As Double, windSpeedMph As Double) As String
        Return TempestDisplay.Common.Weather.WeatherCalculations.GetFeelsLikeLabel(tempF, windSpeedMph)
    End Function

    ' Unit Conversion Functions from the DLL
    Public Function DegreesToCardinal(degrees As Integer) As String
        Return TempestDisplay.Common.Weather.UnitConversions.DegreesToCardinal(degrees)
    End Function

End Module