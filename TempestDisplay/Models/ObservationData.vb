''' <summary>
''' Represents parsed observation data from a Tempest obs_st packet
''' </summary>
Public Class ObservationData
    Public Property Timestamp As Long
    Public Property WindLull As Double
    Public Property WindAvg As Double
    Public Property WindGust As Double
    Public Property WindDirection As Integer
    Public Property Pressure As Double
    Public Property Temperature As Double
    Public Property Humidity As Double
    Public Property Illuminance As Integer
    Public Property UvIndex As Double
    Public Property SolarRadiation As Integer
    Public Property RainAccum As Double
    Public Property PrecipType As Integer
    Public Property StrikeDistance As Double
    Public Property StrikeCount As Integer
    Public Property Battery As Double
    Public Property ReportInterval As Integer

    ''' <summary>
    ''' Temperature in Fahrenheit
    ''' </summary>
    Public ReadOnly Property TempF As Double
        Get
            Return (Temperature * 9.0 / 5.0) + 32
        End Get
    End Property

    ''' <summary>
    ''' Wind average speed in MPH
    ''' </summary>
    Public ReadOnly Property WindAvgMph As Double
        Get
            Return WindAvg * 2.23694
        End Get
    End Property

    ''' <summary>
    ''' Wind gust speed in MPH
    ''' </summary>
    Public ReadOnly Property WindGustMph As Double
        Get
            Return WindGust * 2.23694
        End Get
    End Property

    ''' <summary>
    ''' Wind lull speed in MPH
    ''' </summary>
    Public ReadOnly Property WindLullMph As Double
        Get
            Return WindLull * 2.23694
        End Get
    End Property

    ''' <summary>
    ''' Rain accumulation in inches
    ''' </summary>
    Public ReadOnly Property RainInches As Double
        Get
            Return RainAccum * 0.0393701
        End Get
    End Property

    ''' <summary>
    ''' Pressure in inHg
    ''' </summary>
    Public ReadOnly Property PressureInHg As Double
        Get
            Return Pressure * 0.02953
        End Get
    End Property

    ''' <summary>
    ''' Timestamp as DateTime
    ''' </summary>
    Public ReadOnly Property TimestampDateTime As DateTime
        Get
            Return DateTimeOffset.FromUnixTimeSeconds(Timestamp).LocalDateTime
        End Get
    End Property

    ''' <summary>
    ''' Precipitation type as text
    ''' </summary>
    Public ReadOnly Property PrecipTypeText As String
        Get
            Select Case PrecipType
                Case 0
                    Return "None"
                Case 1
                    Return "Rain"
                Case 2
                    Return "Hail"
                Case Else
                    Return $"Unknown ({PrecipType})"
            End Select
        End Get
    End Property
End Class
