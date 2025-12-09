Imports System.Text.Json

''' <summary>
''' Parses Tempest obs_st UDP packets into structured data
''' </summary>
Public Class ObservationParser

    ''' <summary>
    ''' Parse obs_st JSON packet into ObservationData object
    ''' Returns Nothing if parsing fails or observation type is not obs_st
    ''' </summary>
    Public Shared Function ParseObsStPacket(jsonData As String) As ObservationData
        Try
            Using document = JsonDocument.Parse(jsonData)
                Dim root = document.RootElement

                ' Verify this is an obs_st packet
                If Not root.TryGetProperty("type", Nothing) Then
                    Log.Write("[Parser] No 'type' property found in observation JSON")
                    Return Nothing
                End If

                Dim observationType = root.GetProperty("type").GetString()
                If observationType <> "obs_st" Then
                    Log.Write($"[Parser] Skipping non-obs_st observation type: {observationType}")
                    Return Nothing
                End If

                ' Get the observation array
                If Not root.TryGetProperty("obs", Nothing) Then
                    Log.Write("[Parser] No 'obs' property found in observation JSON")
                    Return Nothing
                End If

                Dim obsArray = root.GetProperty("obs")
                If obsArray.GetArrayLength() = 0 Then
                    Log.Write("[Parser] Empty observation array")
                    Return Nothing
                End If

                Dim ob = obsArray(0) ' First observation

                ' Parse all fields from the observation array
                ' obs_st format: [timestamp, wind_lull, wind_avg, wind_gust, wind_dir, wind_interval,
                '                 pressure, temp, humidity, illuminance, uv, solar_rad, rain_accumulated,
                '                 precip_type, strike_dist, strike_count, battery, report_interval]

                Dim data As New ObservationData With {
                    .Timestamp = ob(0).GetInt64(),
                    .WindLull = ob(1).GetDouble(),
                    .WindAvg = ob(2).GetDouble(),
                    .WindGust = ob(3).GetDouble(),
                    .WindDirection = ob(4).GetInt32(),
                    .Pressure = ob(6).GetDouble(),
                    .Temperature = ob(7).GetDouble(),
                    .Humidity = ob(8).GetDouble(),
                    .Illuminance = ob(9).GetInt32(),
                    .UvIndex = ob(10).GetDouble(),
                    .SolarRadiation = ob(11).GetInt32(),
                    .RainAccum = ob(12).GetDouble(),
                    .PrecipType = ob(13).GetInt32(),
                    .StrikeDistance = ob(14).GetDouble(),
                    .StrikeCount = ob(15).GetInt32(),
                    .Battery = ob(16).GetDouble(),
                    .ReportInterval = If(ob.GetArrayLength() >= 18, ob(17).GetInt32(), 60)
                }

                Return data
            End Using
        Catch ex As JsonException
            Log.WriteException(ex, "[Parser] Error parsing observation JSON")
            Return Nothing
        Catch ex As Exception
            Log.WriteException(ex, "[Parser] Unexpected error in ParseObsStPacket")
            Return Nothing
        End Try
    End Function

End Class
