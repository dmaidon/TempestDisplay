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

        ''' <summary>Converts Celsius to Fahrenheit (Single precision overload)</summary>
        Public Function CelsiusToFahrenheit(celsius As Single) As Single
            Return celsius * 9.0F / 5.0F + 32.0F
        End Function

        ''' <summary>Converts Fahrenheit to Celsius</summary>
        Public Function FahrenheitToCelsius(fahrenheit As Double) As Double
            Return (fahrenheit - 32.0) * 5.0 / 9.0
        End Function

        ''' <summary>Converts Fahrenheit to Celsius (Single precision overload)</summary>
        Public Function FahrenheitToCelsius(fahrenheit As Single) As Single
            Return (fahrenheit - 32.0F) * 5.0F / 9.0F
        End Function

        ' Wind speed conversions
        Private Const MetersPerSecondToMphFactor As Double = 2.23694
        Private Const MetersPerSecondToKphFactor As Double = 3.6
        Private Const KmToMilesFactor As Double = 0.621371
        Private Const MillibarsToInHgFactor As Double = 0.02953
        Private Const MillimetersToInchesFactor As Double = 0.0393701

        ''' <summary>Converts meters per second to miles per hour</summary>
        Public Function MetersPerSecondToMph(mps As Double) As Double
            Return mps * MetersPerSecondToMphFactor
        End Function

        ''' <summary>Converts miles per hour to meters per second</summary>
        Public Function MphToMetersPerSecond(mph As Double) As Double
            Return mph / MetersPerSecondToMphFactor
        End Function

        ''' <summary>Converts meters per second to kilometers per hour</summary>
        Public Function MetersPerSecondToKph(mps As Double) As Double
            Return mps * MetersPerSecondToKphFactor
        End Function

        ''' <summary>Converts kilometers per hour to miles per hour</summary>
        Public Function KphToMph(kph As Double) As Double
            Return kph * KmToMilesFactor
        End Function

        ' Pressure conversions
        ''' <summary>Converts millibars to inches of mercury</summary>
        Public Function MillibarsToInHg(mb As Double) As Double
            Return mb * MillibarsToInHgFactor
        End Function

        ''' <summary>Converts inches of mercury to millibars</summary>
        Public Function InHgToMillibars(inHg As Double) As Double
            Return inHg / MillibarsToInHgFactor
        End Function

        ''' <summary>Converts millibars to kilopascals</summary>
        Public Function MillibarsToKPa(mb As Double) As Double
            Return mb * 0.1
        End Function

        ' Precipitation conversions
        ''' <summary>Converts millimeters to inches</summary>
        Public Function MillimetersToInches(mm As Double) As Double
            Return mm * MillimetersToInchesFactor
        End Function

        ''' <summary>Converts inches to millimeters</summary>
        Public Function InchesToMillimeters(inches As Double) As Double
            Return inches / MillimetersToInchesFactor
        End Function

        ' Distance conversions
        ''' <summary>Converts kilometers to miles</summary>
        Public Function KilometersToMiles(km As Double) As Double
            Return km * KmToMilesFactor
        End Function

        ''' <summary>Converts miles to kilometers</summary>
        Public Function MilesToKilometers(miles As Double) As Double
            Return miles / KmToMilesFactor
        End Function

        ' Direction helpers
        Private ReadOnly Directions16 As String() = {"N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"}
        Private ReadOnly Directions8 As String() = {"N", "NE", "E", "SE", "S", "SW", "W", "NW"}
        Private ReadOnly FullDirections16 As String() = {
            "North", "North-Northeast", "Northeast", "East-Northeast",
            "East", "East-Southeast", "Southeast", "South-Southeast",
            "South", "South-Southwest", "Southwest", "West-Southwest",
            "West", "West-Northwest", "Northwest", "North-Northwest"
        }

        ''' <summary>
        ''' Converts degrees (0-360) to cardinal direction (N, NE, E, SE, S, SW, W, NW)
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees (0-360)</param>
        ''' <returns>Cardinal direction string</returns>
        Public Function DegreesToCardinal(degrees As Integer) As String
            Dim normalized = degrees Mod 360
            If normalized < 0 Then normalized += 360

            ' 16-point compass, center each bucket by adding half a sector (11.25°)
            Dim index = CInt(Math.Floor((normalized + 11.25) / 22.5)) And 15
            Return Directions16(index)
        End Function

        ''' <summary>
        ''' Converts degrees to 8-point cardinal direction (N, NE, E, SE, S, SW, W, NW)
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees (0-360)</param>
        ''' <returns>8-point cardinal direction string</returns>
        Public Function DegreesToCardinal8Point(degrees As Integer) As String
            Dim normalized = degrees Mod 360
            If normalized < 0 Then normalized += 360

            ' 8-point compass, center each bucket by adding half a sector (22.5°)
            Dim index = CInt(Math.Floor((normalized + 22.5) / 45.0)) And 7
            Return Directions8(index)
        End Function

        ''' <summary>
        ''' Converts degrees to full cardinal direction name
        ''' </summary>
        ''' <param name="degrees">Wind direction in degrees (0-360)</param>
        ''' <returns>Full direction name (e.g., "North", "North-Northeast")</returns>
        Public Function DegreesToFullCardinal(degrees As Integer) As String
            Dim normalized = degrees Mod 360
            If normalized < 0 Then normalized += 360

            Dim index = CInt(Math.Floor((normalized + 11.25) / 22.5)) And 15
            Return FullDirections16(index)
        End Function

    End Module

End Namespace
