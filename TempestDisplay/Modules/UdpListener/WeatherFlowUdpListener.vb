Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Json
Imports System.Threading

''' <summary>
''' Listens for UDP broadcast messages from WeatherFlow Smart Weather Station hub on port 50222
''' </summary>
Public Class WeatherFlowUdpListener
    Implements IDisposable

    Private Const UDP_PORT As Integer = 50222
    Private _udpClient As UdpClient
    Private _isListening As Boolean = False
    Private _listenerTask As Task
    Private _cancellationTokenSource As CancellationTokenSource

    ' Events for different message types
    Public Event RapidWindReceived As EventHandler(Of RapidWindEventArgs)
    Public Event ObservationReceived As EventHandler(Of ObservationEventArgs)
    Public Event DeviceStatusReceived As EventHandler(Of DeviceStatusEventArgs)
    Public Event HubStatusReceived As EventHandler(Of HubStatusEventArgs)
    Public Event RainStartReceived As EventHandler(Of RainStartEventArgs)
    Public Event LightningStrikeReceived As EventHandler(Of LightningStrikeEventArgs)
    Public Event RawMessageReceived As EventHandler(Of RawMessageEventArgs)

    Public Sub New()
        _cancellationTokenSource = New CancellationTokenSource()
    End Sub

    ''' <summary>
    ''' Starts listening for UDP broadcasts on port 50222
    ''' </summary>
    Public Sub StartListening()
        If _isListening Then
            Log.Write("WeatherFlowUdpListener: Already listening")
            Return
        End If

        Try
            ' Create UDP client that listens on port 50222
            _udpClient = New UdpClient(UDP_PORT) With {
                .EnableBroadcast = True
            }

            _isListening = True
            _listenerTask = Task.Run(AddressOf ListenForMessagesAsync, _cancellationTokenSource.Token)

            Log.Write($"WeatherFlowUdpListener: Started listening on UDP port {UDP_PORT}")

        Catch ex As Exception
            _isListening = False
            Log.WriteException(ex, "WeatherFlowUdpListener: Error starting UDP listener")
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Stops listening for UDP broadcasts
    ''' </summary>
    Public Sub StopListening()
        If Not _isListening Then Return

        Try
            _isListening = False
            _cancellationTokenSource.Cancel()

            If _udpClient IsNot Nothing Then
                _udpClient.Close()
                _udpClient.Dispose()
                _udpClient = Nothing
            End If

            If _listenerTask IsNot Nothing Then
                Try
                    _listenerTask.Wait(2000) ' Wait up to 2 seconds
                Catch ex As AggregateException
                    ' Task was cancelled, this is expected
                End Try
            End If

            Log.Write("WeatherFlowUdpListener: Stopped listening")

        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error stopping UDP listener")
        End Try
    End Sub

    Private Async Function ListenForMessagesAsync() As Task
        Dim remoteEP As IPEndPoint = Nothing

        Try
            While _isListening AndAlso Not _cancellationTokenSource.Token.IsCancellationRequested
                Try
                    ' Receive UDP message asynchronously
                    Dim result = Await _udpClient.ReceiveAsync().ConfigureAwait(False)
                    Dim receivedBytes = result.Buffer
                    remoteEP = result.RemoteEndPoint

                    ' Convert bytes to string
                    Dim jsonMessage = Encoding.UTF8.GetString(receivedBytes)

                    ' Raise raw message event
                    RaiseEvent RawMessageReceived(Me, New RawMessageEventArgs(jsonMessage, remoteEP))

                    ' Parse and route the message
                    ParseAndRouteMessage(jsonMessage, remoteEP)

                Catch ex As ObjectDisposedException
                    ' UDP client was disposed, exit loop
                    Exit While
                Catch ex As OperationCanceledException
                    ' Cancellation requested, exit loop
                    Exit While
                Catch ex As SocketException When ex.ErrorCode = 995 OrElse ex.ErrorCode = 10004
                    ' Socket operation was aborted (995) or interrupted (10004) - normal shutdown
                    Exit While
                Catch ex As Exception
                    ' Log error but continue listening
                    Log.WriteException(ex, "WeatherFlowUdpListener: Error receiving UDP message")
                End Try
            End While

        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Fatal error in listener loop")
        Finally
            _isListening = False
        End Try
    End Function

    Private Sub ParseAndRouteMessage(jsonMessage As String, remoteEP As IPEndPoint)
        Try
            ' Parse the JSON to determine message type
            Using document = JsonDocument.Parse(jsonMessage)
                Dim root = document.RootElement

                ' Check for "type" property to determine message type
                If root.TryGetProperty("type", Nothing) Then
                    Dim messageType = root.GetProperty("type").GetString()

                    Select Case messageType
                        Case "rapid_wind"
                            ' Rapid wind message (every 3 seconds)
                            ProcessRapidWind(root, remoteEP)

                        Case "obs_st"
                            ' Tempest observation (every minute)
                            ProcessObservation(root, remoteEP)

                        Case "obs_air", "obs_sky"
                            ' Air/Sky observation (legacy devices)
                            ProcessObservation(root, remoteEP)

                        Case "device_status"
                            ' Device status message
                            ProcessDeviceStatus(root, remoteEP)

                        Case "hub_status"
                            ' Hub status message
                            ProcessHubStatus(root, remoteEP)

                        Case "evt_precip"
                            ' Rain start event
                            ProcessRainStart(root, remoteEP)

                        Case "evt_strike"
                            ' Lightning strike event
                            ProcessLightningStrike(root, remoteEP)

                        Case Else
                            Log.Write($"WeatherFlowUdpListener: Unknown message type '{messageType}' from {remoteEP.Address}")
                    End Select
                End If
            End Using

        Catch ex As JsonException
            Log.WriteException(ex, $"WeatherFlowUdpListener: JSON parse error from {remoteEP.Address}")
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error parsing message")
        End Try
    End Sub

    Private Sub ProcessRapidWind(root As JsonElement, remoteEP As IPEndPoint)
        Try
            ' rapid_wind format: {"serial_number":"...", "type":"rapid_wind", "hub_sn":"...", "ob":[timestamp, wind_speed, wind_direction]}
            If root.TryGetProperty("ob", Nothing) Then
                Dim ob = root.GetProperty("ob")
                If ob.GetArrayLength() >= 3 Then
                    Dim timestamp = ob(0).GetInt64()
                    Dim windSpeed = ob(1).GetDouble()
                    Dim windDirection = ob(2).GetInt32()

                    Dim args As New RapidWindEventArgs With {
                        .Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                        .WindSpeed = windSpeed,
                        .WindDirection = windDirection,
                        .RemoteEndPoint = remoteEP
                    }

                    RaiseEvent RapidWindReceived(Me, args)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error processing rapid_wind")
        End Try
    End Sub

    Private Sub ProcessObservation(root As JsonElement, remoteEP As IPEndPoint)
        Try
            ' obs_st format: {"serial_number":"...", "type":"obs_st", "hub_sn":"...", "obs":[[timestamp, ...data...]]}
            If root.TryGetProperty("obs", Nothing) Then
                Dim obsArray = root.GetProperty("obs")
                If obsArray.GetArrayLength() > 0 Then
                    Dim ob = obsArray(0) ' First observation

                    Dim args As New ObservationEventArgs With {
                        .RawJson = root.GetRawText(),
                        .RemoteEndPoint = remoteEP
                    }

                    RaiseEvent ObservationReceived(Me, args)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error processing observation")
        End Try
    End Sub

    Private Sub ProcessDeviceStatus(root As JsonElement, remoteEP As IPEndPoint)
        Try
            Dim args As New DeviceStatusEventArgs With {
                .RawJson = root.GetRawText(),
                .RemoteEndPoint = remoteEP
            }

            RaiseEvent DeviceStatusReceived(Me, args)
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error processing device_status")
        End Try
    End Sub

    Private Sub ProcessHubStatus(root As JsonElement, remoteEP As IPEndPoint)
        Try
            Dim args As New HubStatusEventArgs With {
                .RawJson = root.GetRawText(),
                .RemoteEndPoint = remoteEP
            }

            RaiseEvent HubStatusReceived(Me, args)
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error processing hub_status")
        End Try
    End Sub

    Private Sub ProcessRainStart(root As JsonElement, remoteEP As IPEndPoint)
        Try
            ' evt_precip format: {"serial_number":"...", "type":"evt_precip", "hub_sn":"...", "evt":[timestamp]}
            If root.TryGetProperty("evt", Nothing) Then
                Dim evt = root.GetProperty("evt")
                If evt.GetArrayLength() >= 1 Then
                    Dim timestamp = evt(0).GetInt64()

                    Dim args As New RainStartEventArgs With {
                        .Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                        .RemoteEndPoint = remoteEP
                    }

                    Log.Write($"WeatherFlowUdpListener: Rain start event at {args.Timestamp:yyyy-MM-dd HH:mm:ss}")
                    RaiseEvent RainStartReceived(Me, args)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error processing evt_precip")
        End Try
    End Sub

    Private Sub ProcessLightningStrike(root As JsonElement, remoteEP As IPEndPoint)
        Try
            ' evt_strike format: {"serial_number":"...", "type":"evt_strike", "hub_sn":"...", "evt":[timestamp, distance, energy]}
            If root.TryGetProperty("evt", Nothing) Then
                Dim evt = root.GetProperty("evt")
                If evt.GetArrayLength() >= 3 Then
                    Dim timestamp = evt(0).GetInt64()
                    Dim distance = evt(1).GetInt32()
                    Dim energy = evt(2).GetInt32()

                    Dim args As New LightningStrikeEventArgs With {
                        .Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                        .Distance = distance,
                        .Energy = energy,
                        .RemoteEndPoint = remoteEP
                    }

                    Log.Write($"WeatherFlowUdpListener: Lightning strike at {args.Timestamp:yyyy-MM-dd HH:mm:ss}, distance: {distance}km, energy: {energy}")
                    RaiseEvent LightningStrikeReceived(Me, args)
                End If
            End If
        Catch ex As Exception
            Log.WriteException(ex, "WeatherFlowUdpListener: Error processing evt_strike")
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        StopListening()
        _cancellationTokenSource?.Dispose()
    End Sub

End Class

