Namespace Weather

    ''' <summary>
    ''' Common unit conversion functions for weather data
    ''' </summary>
    Public Module UnitConversions

        ' Temperature conversions
        ''' <summary>Converts Celsius to Fahrenheit</summary>
        Public Function CelsiusToFahrenheit(celsius As Double) As Double
            Return (celsius * 9.0 / 5.0) + 32.0
        End Function

        ''' <summary>Converts Fahrenheit to Celsius</summary>
        Public Function FahrenheitToCelsius(fahrenheit As Double) As Double
            Return (fahrenheit - 32.0) * 5.0 / 9.0
        End Function

        ' Wind speed conversions
        ''' <summary>Converts meters per second to miles per hour</summary>
        Public Function MetersPerSecondToMph(mps As Double) As Double
            Return mps * 2.23694
        End Function

        ''' <summary>Converts miles per hour to meters per second</summary>
        Public Function MphToMetersPerSecond(mph As Double) As Double
            Return mph / 2.23694
        End Function

        ''' <summary>Converts meters per second to kilometers per hour</summary>
        Public Function MetersPerSecondToKph(mps As Double) As Double
            Return mps * 3.6
        End Function

        ''' <summary>Converts kilometers per hour to miles per hour</summary>
        Public Function KphToMph(kph As Double) As Double
            Return kph * 0.621371
        End Function

        ' Pressure conversions
        ''' <summary>Converts millibars to inches of mercury</summary>
        Public Function MillibarsToInHg(mb As Double) As Double
            Return mb * 0.02953
        End Function

        ''' <summary>Converts inches of mercury to millibars</summary>
        Public Function InHgToMillibars(inHg As Double) As Double
            Return inHg / 0.02953
        End Function

        ''' <summary>Converts millibars to kilopascals</summary>
        Public Function MillibarsToKPa(mb As Double) As Double
            Return mb * 0.1
        End Function

        ' Precipitation conversions
        ''' <summary>Converts millimeters to inches</summary>
        Public Function MillimetersToInches(mm As Double) As Double
            Return mm * 0.0393701
        End Function

        ''' <summary>Converts inches to millimeters</summary>
        Public Function InchesToMillimeters(inches As Double) As Double
            Return inches / 0.0393701
        End Function

        ' Distance conversions
        ''' <summary>Converts kilometers to miles</summary>
        Public Function KilometersToMiles(km As Double) As Double
            Return km * 0.621371
        End Function

        ''' <summary>Converts miles to kilometers</summary>
        Public Function MilesToKilometers(miles As Double) As Double
            Return miles / 0.621371
        End Function

        ' Direction helpers
        ''' <summary>
        ''' Converts degrees (0-360) to cardinal direction (N, NE, E, SE, S, SW, W, NW)
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees (0-360)</param>
        ''' <returns>Cardinal direction string</returns>
        Public Function DegreesToCardinal(degrees As Integer) As String
            ' Normalize degrees to 0-360 range
            Dim normalized = degrees Mod 360
            If normalized < 0 Then normalized += 360

            ' 16-point compass with 22.5° per direction
            Dim directions As String() = {"N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"}
            Dim index = CInt(Math.Round(normalized / 22.5)) Mod 16
            Return directions(index)
        End Function

        ''' <summary>
        ''' Converts degrees to 8-point cardinal direction (N, NE, E, SE, S, SW, W, NW)
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees (0-360)</param>
        ''' <returns>8-point cardinal direction string</returns>
        Public Function DegreesToCardinal8Point(degrees As Integer) As String
            ' Normalize degrees to 0-360 range
            Dim normalized = degrees Mod 360
            If normalized < 0 Then normalized += 360

            ' 8-point compass with 45° per direction
            Dim directions As String() = {"N", "NE", "E", "SE", "S", "SW", "W", "NW"}
            Dim index = CInt(Math.Round(normalized / 45.0)) Mod 8
            Return directions(index)
        End Function

        ''' <summary>
        ''' Converts degrees to full cardinal direction name
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees (0-360)</param>
        ''' <returns>Full direction name (e.g., "North", "North-Northeast")</returns>
        Public Function DegreesToFullCardinal(degrees As Integer) As String
            ' Normalize degrees to 0-360 range
            Dim normalized = degrees Mod 360
            If normalized < 0 Then normalized += 360

            ' 16-point compass with full names
            Dim directions As String() = {
                "North", "North-Northeast", "Northeast", "East-Northeast",
                "East", "East-Southeast", "Southeast", "South-Southeast",
                "South", "South-Southwest", "Southwest", "West-Southwest",
                "West", "West-Northwest", "Northwest", "North-Northwest"
            }
            Dim index = CInt(Math.Round(normalized / 22.5)) Mod 16
            Return directions(index)
        End Function

    End Module

End Namespace
