Namespace Weather

    ''' <summary>
    ''' Utility functions for weather data processing and formatting
    ''' </summary>
    Public Module WeatherUtilities

        ''' <summary>
        ''' Validates temperature value is within reasonable range
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <returns>True if temperature is valid (-100°F to 150°F)</returns>
        Public Function IsValidTemperature(tempF As Double) As Boolean
            Return tempF >= -100 AndAlso tempF <= 150
        End Function

        ''' <summary>
        ''' Validates humidity percentage is within valid range
        ''' </summary>
        ''' <param name="humidity">Relative humidity percentage</param>
        ''' <returns>True if humidity is valid (0-100%)</returns>
        Public Function IsValidHumidity(humidity As Double) As Boolean
            Return humidity >= 0 AndAlso humidity <= 100
        End Function

        ''' <summary>
        ''' Validates wind speed is within reasonable range
        ''' </summary>
        ''' <param name="windSpeedMph">Wind speed in mph</param>
        ''' <returns>True if wind speed is valid (0-200 mph)</returns>
        Public Function IsValidWindSpeed(windSpeedMph As Double) As Boolean
            Return windSpeedMph >= 0 AndAlso windSpeedMph <= 200
        End Function

        ''' <summary>
        ''' Validates wind direction is within valid range
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees</param>
        ''' <returns>True if direction is valid (0-360°)</returns>
        Public Function IsValidWindDirection(degrees As Integer) As Boolean
            Return degrees >= 0 AndAlso degrees <= 360
        End Function

        ''' <summary>
        ''' Validates barometric pressure is within reasonable range
        ''' </summary>
        ''' <param name="pressureInHg">Pressure in inches of mercury</param>
        ''' <returns>True if pressure is valid (26-32 inHg)</returns>
        Public Function IsValidPressure(pressureInHg As Double) As Boolean
            Return pressureInHg >= 26.0 AndAlso pressureInHg <= 32.0
        End Function

        ''' <summary>
        ''' Formats temperature with appropriate decimal places
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="decimals">Number of decimal places (default 1)</param>
        ''' <returns>Formatted temperature string with °F</returns>
        Public Function FormatTemperature(tempF As Double, Optional decimals As Integer = 1) As String
            Return $"{tempF.ToString($"F{decimals}")}°F"
        End Function

        ''' <summary>
        ''' Formats humidity percentage
        ''' </summary>
        ''' <param name="humidity">Relative humidity percentage</param>
        ''' <returns>Formatted humidity string with %</returns>
        Public Function FormatHumidity(humidity As Double) As String
            Return $"{humidity:F0}%"
        End Function

        ''' <summary>
        ''' Formats wind speed
        ''' </summary>
        ''' <param name="windSpeedMph">Wind speed in mph</param>
        ''' <param name="decimals">Number of decimal places (default 1)</param>
        ''' <returns>Formatted wind speed string with mph</returns>
        Public Function FormatWindSpeed(windSpeedMph As Double, Optional decimals As Integer = 1) As String
            Return $"{windSpeedMph.ToString($"F{decimals}")} mph"
        End Function

        ''' <summary>
        ''' Formats barometric pressure
        ''' </summary>
        ''' <param name="pressureInHg">Pressure in inches of mercury</param>
        ''' <param name="decimals">Number of decimal places (default 2)</param>
        ''' <returns>Formatted pressure string with inHg</returns>
        Public Function FormatPressure(pressureInHg As Double, Optional decimals As Integer = 2) As String
            Return $"{pressureInHg.ToString($"F{decimals}")} inHg"
        End Function

        ''' <summary>
        ''' Formats precipitation amount
        ''' </summary>
        ''' <param name="inches">Precipitation in inches</param>
        ''' <param name="decimals">Number of decimal places (default 2)</param>
        ''' <returns>Formatted precipitation string with inches or trace</returns>
        Public Function FormatPrecipitation(inches As Double, Optional decimals As Integer = 2) As String
            If inches < 0.01 Then
                Return "Trace"
            End If
            Return $"{inches.ToString($"F{decimals}")}"""
        End Function

        ''' <summary>
        ''' Gets description for comfort level based on temperature and humidity
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="humidity">Relative humidity percentage</param>
        ''' <returns>Comfort level description</returns>
        Public Function GetComfortLevel(tempF As Double, humidity As Double) As String
            ' Combine temperature and humidity for comfort assessment
            If tempF < 50 Then
                Return "Cold"
            ElseIf tempF > 85 Then
                If humidity > 60 Then
                    Return "Hot and Humid"
                Else
                    Return "Hot"
                End If
            ElseIf tempF >= 65 AndAlso tempF <= 75 Then
                If humidity < 30 Then
                    Return "Comfortable but Dry"
                ElseIf humidity <= 60 Then
                    Return "Comfortable"
                Else
                    Return "Comfortable but Humid"
                End If
            ElseIf tempF < 65 Then
                Return "Cool"
            Else ' 75-85°F
                If humidity > 65 Then
                    Return "Warm and Humid"
                Else
                    Return "Warm"
                End If
            End If
        End Function

        ''' <summary>
        ''' Gets description for dew point comfort level
        ''' </summary>
        ''' <param name="dewPointF">Dew point in Fahrenheit</param>
        ''' <returns>Dew point comfort description</returns>
        Public Function GetDewPointComfort(dewPointF As Double) As String
            If dewPointF < 50 Then
                Return "Dry and Comfortable"
            ElseIf dewPointF < 60 Then
                Return "Comfortable"
            ElseIf dewPointF < 65 Then
                Return "Slightly Humid"
            ElseIf dewPointF < 70 Then
                Return "Humid"
            ElseIf dewPointF < 75 Then
                Return "Very Humid"
            Else
                Return "Oppressively Humid"
            End If
        End Function

        ''' <summary>
        ''' Gets Beaufort scale description for wind speed
        ''' </summary>
        ''' <param name="windSpeedMph">Wind speed in mph</param>
        ''' <returns>Beaufort scale number and description</returns>
        Public Function GetBeaufortScale(windSpeedMph As Double) As String
            If windSpeedMph < 1 Then
                Return "0 - Calm"
            ElseIf windSpeedMph < 4 Then
                Return "1 - Light Air"
            ElseIf windSpeedMph < 8 Then
                Return "2 - Light Breeze"
            ElseIf windSpeedMph < 13 Then
                Return "3 - Gentle Breeze"
            ElseIf windSpeedMph < 19 Then
                Return "4 - Moderate Breeze"
            ElseIf windSpeedMph < 25 Then
                Return "5 - Fresh Breeze"
            ElseIf windSpeedMph < 32 Then
                Return "6 - Strong Breeze"
            ElseIf windSpeedMph < 39 Then
                Return "7 - Near Gale"
            ElseIf windSpeedMph < 47 Then
                Return "8 - Gale"
            ElseIf windSpeedMph < 55 Then
                Return "9 - Strong Gale"
            ElseIf windSpeedMph < 64 Then
                Return "10 - Storm"
            ElseIf windSpeedMph < 73 Then
                Return "11 - Violent Storm"
            Else
                Return "12 - Hurricane"
            End If
        End Function

        ''' <summary>
        ''' Gets UV index category and recommendation
        ''' </summary>
        ''' <param name="uvIndex">UV index value</param>
        ''' <returns>UV category and protection recommendation</returns>
        Public Function GetUVIndexCategory(uvIndex As Double) As String
            If uvIndex < 3 Then
                Return "Low - No protection needed"
            ElseIf uvIndex < 6 Then
                Return "Moderate - Wear sunscreen"
            ElseIf uvIndex < 8 Then
                Return "High - Sunscreen and hat recommended"
            ElseIf uvIndex < 11 Then
                Return "Very High - Extra protection required"
            Else
                Return "Extreme - Avoid sun exposure"
            End If
        End Function

        ''' <summary>
        ''' Calculates approximate visibility based on conditions
        ''' </summary>
        ''' <param name="humidity">Relative humidity percentage</param>
        ''' <param name="isRaining">Whether it's currently raining</param>
        ''' <returns>Visibility description</returns>
        Public Function GetVisibilityEstimate(humidity As Double, isRaining As Boolean) As String
            If isRaining Then
                Return "Reduced - Rain"
            ElseIf humidity > 95 Then
                Return "Poor - Fog Likely"
            ElseIf humidity > 85 Then
                Return "Fair - Haze Possible"
            Else
                Return "Good - Clear"
            End If
        End Function

    End Module

End Namespace
