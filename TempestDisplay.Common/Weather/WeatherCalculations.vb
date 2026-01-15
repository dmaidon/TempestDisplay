Namespace Weather

    ''' <summary>
    ''' Weather calculation routines for Heat Index, Wind Chill, Dew Point, and Air Density
    ''' All formulas use NWS (National Weather Service) standards for accuracy
    ''' </summary>
    Public Module WeatherCalculations

        ''DO NOT delete: https://apidocs.tempestwx.com/reference/derived-metrics

        Private Const MinHumidityPercent As Double = 0.0
        Private Const MaxHumidityPercent As Double = 100.0
        Private Const MinPressureMb As Double = 0.1

        Private Function Clamp(value As Double, minValue As Double, maxValue As Double) As Double
            If value < minValue Then Return minValue
            If value > maxValue Then Return maxValue
            Return value
        End Function

        ''' <summary>
        ''' Calculate Heat Index in Fahrenheit
        ''' Valid when temperature >= 80°F and humidity >= 40%
        ''' Formula: Rothfusz regression (NWS standard)
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="humidity">Relative humidity as percentage (0-100)</param>
        ''' <returns>Heat Index in Fahrenheit, or temperature if conditions don't warrant heat index</returns>
        Public Function CalculateHeatIndex(tempF As Double, humidity As Double) As Double
            humidity = Clamp(humidity, MinHumidityPercent, MaxHumidityPercent)

            ' Heat Index only applies when temp >= 80°F
            If tempF < 80 Then
                Return tempF
            End If

            ' Simple formula for initial estimate
            Dim heatIndex = 0.5 * (tempF + 61.0 + ((tempF - 68.0) * 1.2) + (humidity * 0.094))

            ' If average of temperature and heat index >= 80°F, use full Rothfusz regression
            If (heatIndex + tempF) / 2.0 >= 80.0 Then
                heatIndex = -42.379 +
                            2.04901523 * tempF +
                            10.14333127 * humidity -
                            0.22475541 * tempF * humidity -
                            0.00683783 * tempF * tempF -
                            0.05481717 * humidity * humidity +
                            0.00122874 * tempF * tempF * humidity +
                            0.00085282 * tempF * humidity * humidity -
                            0.00000199 * tempF * tempF * humidity * humidity

                ' Adjustments for extreme conditions
                If humidity < 13 AndAlso tempF >= 80 AndAlso tempF <= 112 Then
                    ' Dry conditions adjustment
                    Dim adjustment = ((13 - humidity) / 4.0) * Math.Sqrt((17 - Math.Abs(tempF - 95.0)) / 17.0)
                    heatIndex -= adjustment
                ElseIf humidity > 85 AndAlso tempF >= 80 AndAlso tempF <= 87 Then
                    ' High humidity adjustment
                    Dim adjustment = ((humidity - 85.0) / 10.0) * ((87.0 - tempF) / 5.0)
                    heatIndex += adjustment
                End If
            End If

            Return Math.Round(heatIndex, 1)
        End Function

        ''' <summary>
        ''' Calculate Wind Chill in Fahrenheit
        ''' Valid when temperature is 50F or below and wind speed is 3 mph or above
        ''' Formula: NWS standard (2001)
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="windSpeedMph">Wind speed in mph</param>
        ''' <returns>Wind Chill in Fahrenheit, or temperature if conditions don't warrant wind chill</returns>
        Public Function CalculateWindChill(tempF As Double, windSpeedMph As Double) As Double
            If Double.IsNaN(windSpeedMph) OrElse Double.IsInfinity(windSpeedMph) Then
                Return tempF
            End If

            ' Wind chill only applies when temp <= 50°F and wind >= 3 mph
            If tempF > 50 OrElse windSpeedMph < 3 Then
                Return tempF
            End If

            ' NWS Wind Chill formula (valid for temps <= 50°F and wind >= 3 mph)
            Dim windChill = 35.74 +
                            0.6215 * tempF -
                            35.75 * Math.Pow(windSpeedMph, 0.16) +
                            0.4275 * tempF * Math.Pow(windSpeedMph, 0.16)

            Return Math.Round(windChill, 1)
        End Function

        ''' <summary>
        ''' Calculate "Feels Like" temperature
        ''' Returns Heat Index if hot, Wind Chill if cold, or actual temperature otherwise
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="humidity">Relative humidity as percentage (0-100)</param>
        ''' <param name="windSpeedMph">Wind speed in mph</param>
        ''' <returns>"Feels Like" temperature in Fahrenheit</returns>
        Public Function CalculateFeelsLike(tempF As Double, humidity As Double, windSpeedMph As Double) As Double
            humidity = Clamp(humidity, MinHumidityPercent, MaxHumidityPercent)

            If tempF >= 80 Then
                ' Hot weather - use heat index
                Return CalculateHeatIndex(tempF, humidity)
            ElseIf tempF <= 50 AndAlso windSpeedMph >= 3 Then
                ' Cold and windy - use wind chill
                Return CalculateWindChill(tempF, windSpeedMph)
            Else
                ' Comfortable range - actual temperature
                Return tempF
            End If
        End Function

        ''' <summary>
        ''' Calculate Dew Point in Fahrenheit
        ''' Uses Magnus-Tetens formula (accurate to ±0.4°F)
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="humidity">Relative humidity as percentage (0-100)</param>
        ''' <returns>Dew Point in Fahrenheit</returns>
        Public Function CalculateDewPoint(tempF As Double, humidity As Double) As Double
            humidity = Clamp(humidity, MinHumidityPercent, MaxHumidityPercent)

            ' Guard against Log(0) when humidity is 0; 0.1% keeps output finite
            Dim humidityForLog = Math.Max(0.1, humidity)

            ' Convert to Celsius for calculation
            Dim tempC = (tempF - 32.0) * 5.0 / 9.0

            ' Magnus-Tetens formula constants
            Const a As Double = 17.27
            Const b As Double = 237.7

            ' Calculate alpha (intermediate value)
            Dim alpha = ((a * tempC) / (b + tempC)) + Math.Log(humidityForLog / 100.0)

            ' Calculate dew point in Celsius
            Dim dewPointC = (b * alpha) / (a - alpha)

            ' Convert back to Fahrenheit
            Dim dewPointF = (dewPointC * 9.0 / 5.0) + 32.0

            Return Math.Round(dewPointF, 1)
        End Function

        ''' <summary>
        ''' Calculate Air Density in kg/m³
        ''' Uses Ideal Gas Law with corrections for humidity
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="pressureMb">Barometric pressure in millibars (hPa)</param>
        ''' <param name="humidity">Relative humidity as percentage (0-100)</param>
        ''' <returns>Air density in kg/m³</returns>
        Public Function CalculateAirDensity(tempF As Double, pressureMb As Double, humidity As Double) As Double
            humidity = Clamp(humidity, MinHumidityPercent, MaxHumidityPercent)
            If pressureMb <= 0 Then Return 0

            ' Convert temperature to Kelvin
            Dim tempC = (tempF - 32.0) * 5.0 / 9.0
            Dim tempK = tempC + 273.15
            If tempK <= 0 Then Return 0

            ' Convert pressure from millibars to Pascals (1 mb = 100 Pa)
            Dim pressurePa = pressureMb * 100.0

            ' Gas constants
            Const Rd As Double = 287.05      ' Specific gas constant for dry air (J/(kg·K))
            Const Rv As Double = 461.495     ' Specific gas constant for water vapor (J/(kg·K))

            ' Calculate saturation vapor pressure using Magnus formula (in Pa)
            Dim es = 611.2 * Math.Exp((17.67 * tempC) / (tempC + 243.5))

            ' Calculate actual vapor pressure from relative humidity
            Dim Pv = (humidity / 100.0) * es
            If Pv < 0 Then Pv = 0
            If Pv > pressurePa Then Pv = pressurePa

            ' Calculate partial pressure of dry air
            Dim Pd = pressurePa - Pv

            ' Calculate air density using ideal gas law for mixture
            Dim density = (Pd / (Rd * tempK)) + (Pv / (Rv * tempK))

            Return Math.Round(density, 4)
        End Function

        ''' <summary>
        ''' Get descriptive text for air density
        ''' Standard air density at sea level (59°F, 29.92 inHg, 0% humidity) = 1.225 kg/m³
        ''' </summary>
        ''' <param name="density">Air density in kg/m³</param>
        ''' <returns>Description of air density</returns>
        Public Function GetAirDensityCategory(density As Double) As String
            If density < 1.0 Then
                Return "Very Thin Air"        ' High altitude or very hot
            ElseIf density < 1.15 Then
                Return "Thin Air"             ' Hot day or moderate altitude
            ElseIf density < 1.2 Then
                Return "Below Average"        ' Warm conditions
            ElseIf density < 1.25 Then
                Return "Average"              ' Standard conditions
            ElseIf density < 1.3 Then
                Return "Above Average"        ' Cool conditions
            ElseIf density < 1.35 Then
                Return "Dense Air"            ' Cold day
            Else
                Return "Very Dense Air"       ' Very cold or high pressure
            End If
        End Function

        ''' <summary>
        ''' Calculate density altitude in feet
        ''' </summary>
        ''' <param name="tempF">Temperature in Fahrenheit</param>
        ''' <param name="pressureMb">Barometric pressure in millibars</param>
        ''' <param name="humidity">Relative humidity as percentage (0-100)</param>
        ''' <param name="actualAltitudeFt">Actual elevation above sea level in feet (optional, default 0)</param>
        ''' <returns>Density altitude in feet</returns>
        Public Function CalculateDensityAltitude(tempF As Double, pressureMb As Double, humidity As Double) As Double
            humidity = Clamp(humidity, MinHumidityPercent, MaxHumidityPercent)

            Dim pressureMbSafe = Math.Max(MinPressureMb, pressureMb)

            ' Convert pressure to inHg for calculation (1 mb = 0.02953 inHg)
            Dim pressureInHg = pressureMbSafe * 0.02953

            ' Calculate dew point for vapor pressure correction
            Dim dewPointF = CalculateDewPoint(tempF, humidity)

            ' Calculate vapor pressure in inHg using dew point
            Dim dewPointC = (dewPointF - 32.0) * 5.0 / 9.0
            Dim vaporPressureMb = 6.11 * Math.Pow(10, (7.5 * dewPointC) / (237.3 + dewPointC))
            Dim vaporPressureInHg = vaporPressureMb * 0.02953

            ' Avoid divide by 0 / negative denominators in virtual temp calc
            Dim denom = 1 - (vaporPressureInHg / pressureInHg) * (1 - 0.622)
            If denom <= 0 Then denom = 0.000001

            ' Calculate virtual temperature (temperature corrected for moisture)
            Dim virtualTempK = ((tempF - 32.0) * 5.0 / 9.0 + 273.15) / denom

            ' Standard atmosphere constants
            Const stdTempK As Double = 288.15        ' Standard temp at sea level (15°C = 59°F)
            Const stdPressureInHg As Double = 29.92  ' Standard pressure at sea level
            Const lapseRate As Double = 0.0019812    ' Temperature lapse rate (K/ft)

            ' Calculate pressure altitude (altitude for current pressure in standard atmosphere)
            Dim pressureAltitude = (1 - Math.Pow(pressureInHg / stdPressureInHg, 0.190284)) * 145442.16

            ' Calculate density altitude using virtual temperature
            Dim densityAltitude = pressureAltitude + (virtualTempK - stdTempK) / lapseRate

            Return Math.Round(densityAltitude, 0)
        End Function

        ''' <summary>
        ''' Get descriptive text for Heat Index severity
        ''' </summary>
        Public Function GetHeatIndexCategory(heatIndex As Double) As String
            If heatIndex < 80 Then
                Return "No Heat Advisory"
            ElseIf heatIndex < 90 Then
                Return "Caution"
            ElseIf heatIndex < 103 Then
                Return "Extreme Caution"
            ElseIf heatIndex < 125 Then
                Return "Danger"
            Else
                Return "Extreme Danger"
            End If
        End Function

        ''' <summary>
        ''' Get descriptive text for Wind Chill severity
        ''' </summary>
        Public Function GetWindChillCategory(windChill As Double) As String
            If windChill > 50 Then
                Return "No Wind Chill"
            ElseIf windChill > 32 Then
                Return "Cool"
            ElseIf windChill > 15 Then
                Return "Cold"
            ElseIf windChill > -20 Then
                Return "Very Cold"
            ElseIf windChill > -50 Then
                Return "Extreme Cold"
            Else
                Return "Dangerous Cold"
            End If
        End Function

        ''' <summary>
        ''' Determine which "Feels Like" calculation to display
        ''' </summary>
        Public Function GetFeelsLikeLabel(tempF As Double, windSpeedMph As Double) As String
            If tempF >= 80 Then
                Return "Heat Index"
            ElseIf tempF <= 50 AndAlso windSpeedMph >= 3 Then
                Return "Wind Chill"
            Else
                Return "Feels Like"
            End If
        End Function

        ''' <summary>
        ''' Cloud Base in feet AGL (Above Ground Level)
        ''' Rule of thumb using temperature (°F) and dew point (°F).
        ''' </summary>
        Public Function CalculateCloudBase(tempF As Double, humidity As Double) As Double
            humidity = Clamp(humidity, MinHumidityPercent, MaxHumidityPercent)

            Dim dewPoint = CalculateDewPoint(tempF, humidity)
            Dim cloudBaseFeet = ((tempF - dewPoint) / 4.4) * 1000.0

            Return Math.Round(Math.Max(0.0, cloudBaseFeet), 0)
        End Function

    End Module

End Namespace