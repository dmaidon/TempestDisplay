Imports System.Net

''' <summary>
''' Event args for raw UDP messages
''' </summary>
Public Class RawMessageEventArgs
    Inherits EventArgs

    Public Property JsonMessage As String
    Public Property RemoteEndPoint As IPEndPoint

    Public Sub New(jsonMessage As String, remoteEndPoint As IPEndPoint)
        Me.JsonMessage = jsonMessage
        Me.RemoteEndPoint = remoteEndPoint
    End Sub
End Class

''' <summary>
''' Event args for rapid wind messages (updated every 3 seconds)
''' </summary>
Public Class RapidWindEventArgs
    Inherits EventArgs

    Public Property Timestamp As DateTime
    Public Property WindSpeed As Double ' meters per second
    Public Property WindDirection As Integer ' degrees
    Public Property RemoteEndPoint As IPEndPoint
End Class

''' <summary>
''' Event args for observation messages (updated every minute)
''' </summary>
Public Class ObservationEventArgs
    Inherits EventArgs

    Public Property RawJson As String
    Public Property RemoteEndPoint As IPEndPoint
End Class

''' <summary>
''' Event args for device status messages
''' </summary>
Public Class DeviceStatusEventArgs
    Inherits EventArgs

    Public Property RawJson As String
    Public Property RemoteEndPoint As IPEndPoint
End Class

''' <summary>
''' Event args for hub status messages
''' </summary>
Public Class HubStatusEventArgs
    Inherits EventArgs

    Public Property RawJson As String
    Public Property RemoteEndPoint As IPEndPoint
End Class

''' <summary>
''' Event args for rain start event (evt_precip)
''' </summary>
Public Class RainStartEventArgs
    Inherits EventArgs

    Public Property Timestamp As DateTime
    Public Property RemoteEndPoint As IPEndPoint
End Class

''' <summary>
''' Event args for lightning strike event (evt_strike)
''' </summary>
Public Class LightningStrikeEventArgs
    Inherits EventArgs

    Public Property Timestamp As DateTime
    Public Property Distance As Integer ' kilometers
    Public Property Energy As Integer ' strike energy
    Public Property RemoteEndPoint As IPEndPoint
End Class
