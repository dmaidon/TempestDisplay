' Last Edit: January 15, 2026 (Made properties immutable, added validation, added ToString overrides, added timestamps)
Imports System.Net

''' <summary>
''' Event args for raw UDP messages
''' </summary>
Public Class RawMessageEventArgs
    Inherits EventArgs

    Private ReadOnly _jsonMessage As String
    Private ReadOnly _remoteEndPoint As IPEndPoint
    Private ReadOnly _timestamp As DateTime

    Public Sub New(jsonMessage As String, remoteEndPoint As IPEndPoint)
        If String.IsNullOrEmpty(jsonMessage) Then Throw New ArgumentNullException(NameOf(jsonMessage))
        ArgumentNullException.ThrowIfNull(remoteEndPoint)

        _jsonMessage = jsonMessage
        _remoteEndPoint = remoteEndPoint
        _timestamp = DateTime.Now
    End Sub

    Public ReadOnly Property JsonMessage As String
        Get
            Return _jsonMessage
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"RawMessage from {RemoteEndPoint.Address} at {Timestamp:HH:mm:ss.fff}"
    End Function

End Class

''' <summary>
''' Event args for rapid wind messages (updated every 3 seconds)
''' </summary>
Public Class RapidWindEventArgs
    Inherits EventArgs

    Private ReadOnly _timestamp As DateTime
    Private ReadOnly _windSpeed As Double
    Private ReadOnly _windDirection As Integer
    Private ReadOnly _remoteEndPoint As IPEndPoint

    Public Sub New(timestamp As DateTime, windSpeed As Double, windDirection As Integer, remoteEndPoint As IPEndPoint)
        ArgumentNullException.ThrowIfNull(remoteEndPoint)
        If windSpeed < 0 Then Throw New ArgumentOutOfRangeException(NameOf(windSpeed), "Wind speed cannot be negative")
        If windDirection < 0 OrElse windDirection > 360 Then Throw New ArgumentOutOfRangeException(NameOf(windDirection), "Wind direction must be 0-360 degrees")

        _timestamp = timestamp
        _windSpeed = windSpeed
        _windDirection = windDirection
        _remoteEndPoint = remoteEndPoint
    End Sub

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    Public ReadOnly Property WindSpeed As Double
        Get
            Return _windSpeed
        End Get
    End Property

    ''' <summary>Wind speed in miles per hour (converted from m/s)</summary>
    Public ReadOnly Property WindSpeedMph As Double
        Get
            Return _windSpeed * 2.23694
        End Get
    End Property

    Public ReadOnly Property WindDirection As Integer
        Get
            Return _windDirection
        End Get
    End Property

    ''' <summary>Cardinal direction (N, NE, E, SE, S, SW, W, NW)</summary>
    Public ReadOnly Property CardinalDirection As String
        Get
            Dim directions As String() = {"N", "NE", "E", "SE", "S", "SW", "W", "NW"}
            Dim index As Integer = CInt(Math.Round(_windDirection / 45.0)) Mod 8
            Return directions(index)
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"RapidWind: {WindSpeedMph:F1}mph {CardinalDirection} ({WindDirection}°) at {Timestamp:HH:mm:ss}"
    End Function

End Class

''' <summary>
''' Event args for observation messages (updated every minute)
''' </summary>
Public Class ObservationEventArgs
    Inherits EventArgs

    Private ReadOnly _rawJson As String
    Private ReadOnly _remoteEndPoint As IPEndPoint
    Private ReadOnly _timestamp As DateTime

    Public Sub New(rawJson As String, remoteEndPoint As IPEndPoint)
        If String.IsNullOrEmpty(rawJson) Then Throw New ArgumentNullException(NameOf(rawJson))
        ArgumentNullException.ThrowIfNull(remoteEndPoint)

        _rawJson = rawJson
        _remoteEndPoint = remoteEndPoint
        _timestamp = DateTime.Now
    End Sub

    Public ReadOnly Property RawJson As String
        Get
            Return _rawJson
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"Observation from {RemoteEndPoint.Address} at {Timestamp:HH:mm:ss}"
    End Function

End Class

''' <summary>
''' Event args for device status messages
''' </summary>
Public Class DeviceStatusEventArgs
    Inherits EventArgs

    Private ReadOnly _rawJson As String
    Private ReadOnly _remoteEndPoint As IPEndPoint
    Private ReadOnly _timestamp As DateTime

    Public Sub New(rawJson As String, remoteEndPoint As IPEndPoint)
        If String.IsNullOrEmpty(rawJson) Then Throw New ArgumentNullException(NameOf(rawJson))
        ArgumentNullException.ThrowIfNull(remoteEndPoint)

        _rawJson = rawJson
        _remoteEndPoint = remoteEndPoint
        _timestamp = DateTime.Now
    End Sub

    Public ReadOnly Property RawJson As String
        Get
            Return _rawJson
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"DeviceStatus from {RemoteEndPoint.Address} at {Timestamp:HH:mm:ss}"
    End Function

End Class

''' <summary>
''' Event args for hub status messages
''' </summary>
Public Class HubStatusEventArgs
    Inherits EventArgs

    Private ReadOnly _rawJson As String
    Private ReadOnly _remoteEndPoint As IPEndPoint
    Private ReadOnly _timestamp As DateTime

    Public Sub New(rawJson As String, remoteEndPoint As IPEndPoint)
        If String.IsNullOrEmpty(rawJson) Then Throw New ArgumentNullException(NameOf(rawJson))
        ArgumentNullException.ThrowIfNull(remoteEndPoint)

        _rawJson = rawJson
        _remoteEndPoint = remoteEndPoint
        _timestamp = DateTime.Now
    End Sub

    Public ReadOnly Property RawJson As String
        Get
            Return _rawJson
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"HubStatus from {RemoteEndPoint.Address} at {Timestamp:HH:mm:ss}"
    End Function

End Class

''' <summary>
''' Event args for rain start event (evt_precip)
''' </summary>
Public Class RainStartEventArgs
    Inherits EventArgs

    Private ReadOnly _timestamp As DateTime
    Private ReadOnly _remoteEndPoint As IPEndPoint

    Public Sub New(timestamp As DateTime, remoteEndPoint As IPEndPoint)
        ArgumentNullException.ThrowIfNull(remoteEndPoint)

        _timestamp = timestamp
        _remoteEndPoint = remoteEndPoint
    End Sub

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"RainStart at {Timestamp:HH:mm:ss} from {RemoteEndPoint.Address}"
    End Function

End Class

''' <summary>
''' Event args for lightning strike event (evt_strike)
''' </summary>
Public Class LightningStrikeEventArgs
    Inherits EventArgs

    Private ReadOnly _timestamp As DateTime
    Private ReadOnly _distance As Integer
    Private ReadOnly _energy As Integer
    Private ReadOnly _remoteEndPoint As IPEndPoint

    Public Sub New(timestamp As DateTime, distance As Integer, energy As Integer, remoteEndPoint As IPEndPoint)
        ArgumentNullException.ThrowIfNull(remoteEndPoint)
        If distance < 0 Then Throw New ArgumentOutOfRangeException(NameOf(distance), "Distance cannot be negative")
        If energy < 0 Then Throw New ArgumentOutOfRangeException(NameOf(energy), "Energy cannot be negative")

        _timestamp = timestamp
        _distance = distance
        _energy = energy
        _remoteEndPoint = remoteEndPoint
    End Sub

    Public ReadOnly Property Timestamp As DateTime
        Get
            Return _timestamp
        End Get
    End Property

    ''' <summary>Lightning strike distance in kilometers</summary>
    Public ReadOnly Property Distance As Integer
        Get
            Return _distance
        End Get
    End Property

    ''' <summary>Lightning strike distance in miles</summary>
    Public ReadOnly Property DistanceMiles As Double
        Get
            Return _distance * 0.621371
        End Get
    End Property

    ''' <summary>Strike energy (arbitrary units)</summary>
    Public ReadOnly Property Energy As Integer
        Get
            Return _energy
        End Get
    End Property

    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return _remoteEndPoint
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"Lightning at {Timestamp:HH:mm:ss}: {DistanceMiles:F1}mi ({Distance}km), energy={Energy}"
    End Function

End Class