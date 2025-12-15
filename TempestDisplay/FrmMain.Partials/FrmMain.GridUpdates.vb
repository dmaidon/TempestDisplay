Imports System.Text.Json

Partial Public Class FrmMain

    ''' <summary>
    ''' Create and initialize the Hub Status DataGridView
    ''' </summary>
    Private Sub CreateHubStatusGrid()
        With DgvHubStatus
            .Columns.Clear()
            .Columns.Add("Property", "Property")
            .Columns.Add("Value", "Value")
            .Rows.Add("Serial #", "N/A")
            .Rows.Add("Hub Status", "N/A")
            .Rows.Add("Firmware Revision", "N/A")
            .Rows.Add("Hub Uptime", "N/A")
            .Rows.Add("RSSI", "N/A")
            .Rows.Add("Reset Flags", "N/A")
            .Rows.Add("Seq", "N/A")
            .Rows.Add("FS", "N/A")
            .Rows.Add("Radio Stats", "N/A")
            .Rows.Add("MQTT", "N/A")
        End With
    End Sub

    ''' <summary>
    ''' Create and initialize the Observation (obs_st) DataGridView
    ''' </summary>
    Private Sub CreateObsStGrid()
        With DgvObsSt
            .Columns.Clear()
            .Columns.Add("Property", "Property")
            .Columns.Add("Value", "Value")
            .Rows.Add("Serial Number", "N/A")
            .Rows.Add("Type", "N/A")
            .Rows.Add("Hub SN", "N/A")
            .Rows.Add("Epoch", "N/A")
            .Rows.Add("Wind Lull (m/s)", "N/A")
            .Rows.Add("Wind Avg (m/s)", "N/A")
            .Rows.Add("Wind Gust (m/s)", "N/A")
            .Rows.Add("Wind Direction (°)", "N/A")
            .Rows.Add("Station Pressure (MB)", "N/A")
            .Rows.Add("Air Temperature (°C)", "N/A")
            .Rows.Add("Relative Humidity (%)", "N/A")
            .Rows.Add("Illuminance (lux)", "N/A")
            .Rows.Add("UV Index", "N/A")
            .Rows.Add("Solar Radiation (W/m²)", "N/A")
            .Rows.Add("Rain Accumulated (mm)", "N/A")
            .Rows.Add("Precipitation Type", "N/A")
            .Rows.Add("Lightning Strike Distance (km)", "N/A")
            .Rows.Add("Lightning Strike Count", "N/A")
            .Rows.Add("Battery (volts)", "N/A")
            .Rows.Add("Report Interval (minutes)", "N/A")
            .Refresh()
        End With
    End Sub

    ''' <summary>
    ''' Update DgvObsSt grid with observation data
    ''' </summary>
    Private Sub UpdateObsStGrid(data As ObservationData, root As JsonElement)
        Try
            ' Ensure grid has been created with proper row count
            If DgvObsSt.Rows.Count < 20 Then
                Log.Write($"[Grid] DgvObsSt has {DgvObsSt.Rows.Count} rows, expected 20. Skipping update.")
                Return
            End If

            ' Row 0: Serial Number
            If root.TryGetProperty("serial_number", Nothing) Then
                DgvObsSt.Rows(0).Cells(1).Value = root.GetProperty("serial_number").GetString()
            End If

            ' Row 1: Type
            If root.TryGetProperty("type", Nothing) Then
                DgvObsSt.Rows(1).Cells(1).Value = root.GetProperty("type").GetString()
            End If

            ' Row 2: Hub SN
            If root.TryGetProperty("hub_sn", Nothing) Then
                DgvObsSt.Rows(2).Cells(1).Value = root.GetProperty("hub_sn").GetString()
            End If

            ' Row 3: Epoch (timestamp)
            DgvObsSt.Rows(3).Cells(1).Value = $"{data.Timestamp} ({data.TimestampDateTime:yyyy-MM-dd HH:mm:ss})"

            ' Rows 4-7: Wind data
            DgvObsSt.Rows(4).Cells(1).Value = $"{data.WindLull:F2}"
            DgvObsSt.Rows(5).Cells(1).Value = $"{data.WindAvg:F2}"
            DgvObsSt.Rows(6).Cells(1).Value = $"{data.WindGust:F2}"
            DgvObsSt.Rows(7).Cells(1).Value = $"{data.WindDirection} ({DegreesToCardinal(data.WindDirection)})"

            ' Rows 8-10: Atmospheric data
            DgvObsSt.Rows(8).Cells(1).Value = $"{data.Pressure:F2}"
            DgvObsSt.Rows(9).Cells(1).Value = $"{data.Temperature:F2}"
            DgvObsSt.Rows(10).Cells(1).Value = $"{data.Humidity:F1}"

            ' Rows 11-13: Light data
            DgvObsSt.Rows(11).Cells(1).Value = $"{data.Illuminance:N0}"
            DgvObsSt.Rows(12).Cells(1).Value = $"{data.UvIndex:F1}"
            DgvObsSt.Rows(13).Cells(1).Value = $"{data.SolarRadiation}"

            ' Rows 14-15: Precipitation data
            DgvObsSt.Rows(14).Cells(1).Value = $"{data.RainAccum:F2}"
            DgvObsSt.Rows(15).Cells(1).Value = data.PrecipTypeText

            ' Rows 16-17: Lightning data
            DgvObsSt.Rows(16).Cells(1).Value = $"{data.StrikeDistance:F1}"
            DgvObsSt.Rows(17).Cells(1).Value = $"{data.StrikeCount}"

            ' Rows 18-19: Device data
            DgvObsSt.Rows(18).Cells(1).Value = $"{data.Battery:F3}"
            DgvObsSt.Rows(19).Cells(1).Value = $"{data.ReportInterval}"

            ' Clear selection and refresh
            DgvObsSt.ClearSelection()
            DgvObsSt.Refresh()

            Log.Write("[Grid] DgvObsSt updated successfully")
        Catch ex As Exception
            Log.WriteException(ex, "[Grid] Error updating DgvObsSt")
        End Try
    End Sub

    ''' <summary>
    ''' Parse and update Hub Status grid from JSON
    ''' </summary>
    Private Sub ParseAndDisplayHubStatus(jsonData As String)
        Try
            Using document = JsonDocument.Parse(jsonData)
                Dim root = document.RootElement

                If DgvHubStatus IsNot Nothing AndAlso DgvHubStatus.Rows.Count >= 10 Then
                    If root.TryGetProperty("serial_number", Nothing) Then
                        DgvHubStatus.Rows(0).Cells(1).Value = root.GetProperty("serial_number").GetString()
                    End If

                    DgvHubStatus.Rows(1).Cells(1).Value = "OK"

                    If root.TryGetProperty("firmware_revision", Nothing) Then
                        DgvHubStatus.Rows(2).Cells(1).Value = root.GetProperty("firmware_revision").GetString()
                    End If

                    If root.TryGetProperty("uptime", Nothing) Then
                        Dim uptime = root.GetProperty("uptime").GetInt64()
                        Dim uptimeSpan = TimeSpan.FromSeconds(uptime)
                        DgvHubStatus.Rows(3).Cells(1).Value = $"{uptimeSpan.Days}d {uptimeSpan.Hours}h {uptimeSpan.Minutes}m"
                    End If

                    If root.TryGetProperty("rssi", Nothing) Then
                        DgvHubStatus.Rows(4).Cells(1).Value = $"{root.GetProperty("rssi").GetInt32()} dBm"
                    End If

                    If root.TryGetProperty("reset_flags", Nothing) Then
                        DgvHubStatus.Rows(5).Cells(1).Value = root.GetProperty("reset_flags").GetString()
                    End If

                    If root.TryGetProperty("seq", Nothing) Then
                        DgvHubStatus.Rows(6).Cells(1).Value = root.GetProperty("seq").GetInt32().ToString()
                    End If

                    If root.TryGetProperty("fs", Nothing) Then
                        Dim fs = root.GetProperty("fs")
                        If fs.GetArrayLength() >= 1 Then
                            DgvHubStatus.Rows(7).Cells(1).Value = $"{fs(0).GetInt32()} bytes free"
                        End If
                    End If

                    If root.TryGetProperty("radio_stats", Nothing) Then
                        Dim radioStats = root.GetProperty("radio_stats")
                        If radioStats.GetArrayLength() >= 2 Then
                            Dim version = radioStats(0).GetInt32()
                            Dim rebootCount = radioStats(1).GetInt32()
                            DgvHubStatus.Rows(8).Cells(1).Value = $"Ver: {version}, Reboots: {rebootCount}"
                        End If
                    End If

                    If root.TryGetProperty("mqtt_stats", Nothing) Then
                        Dim mqttStats = root.GetProperty("mqtt_stats")
                        If mqttStats.GetArrayLength() >= 1 Then
                            Dim mqttStatus = mqttStats(0).GetInt32()
                            DgvHubStatus.Rows(9).Cells(1).Value = If(mqttStatus = 1, "Connected", "Disconnected")
                        End If
                    End If

                    Log.Write("[Grid] Hub status grid updated")
                End If
            End Using
            DgvHubStatus.ClearSelection()
        Catch ex As JsonException
            Log.WriteException(ex, "[Grid] Error parsing hub status JSON")
        Catch ex As Exception
            Log.WriteException(ex, "[Grid] Error in ParseAndDisplayHubStatus")
        End Try
    End Sub

    ''' <summary>
    ''' Create and initialize the Records DataGridView on TpRecords
    ''' </summary>
    Private Sub CreateRecordsGrid()
        If DgvRecords Is Nothing Then
            Return
        End If

        With DgvRecords
            .Columns.Clear()
            .AutoGenerateColumns = False
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .ReadOnly = True
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .MultiSelect = False

            ' Example layout – change to match your schema
            .Columns.Add("DateTime", "Date / Time")
            .Columns("DateTime").Width = 140

            .Columns.Add("Parameter", "Parameter")
            .Columns("Parameter").Width = 120

            .Columns.Add("Value", "Value")
            .Columns("Value").Width = 80

            .Columns.Add("Units", "Units")
            .Columns("Units").Width = 60

            .Columns.Add("Source", "Source")
            .Columns("Source").Width = 100

            .Rows.Clear()
            .Refresh()
        End With
    End Sub

    ''' <summary>
    ''' Record DTO for binding to DgvRecords.
    ''' Replace/align with your actual record structure or HiLo model.
    ''' </summary>
    Private Class DisplayRecord
        Public Property DateTime As DateTime
        Public Property Parameter As String
        Public Property Value As String
        Public Property Units As String
        Public Property Source As String
    End Class

    ''' <summary>
    ''' Populate DgvRecords with the current collection of records.
    ''' Call this after you load or recalc records (e.g. from HiLoDatabase).
    ''' </summary>
    Private Sub UpdateRecordsGrid(records As IEnumerable(Of DisplayRecord))
        Try
            If DgvRecords Is Nothing Then
                Return
            End If

            ' Ensure grid is configured
            If DgvRecords.Columns.Count = 0 Then
                CreateRecordsGrid()
            End If

            DgvRecords.Rows.Clear()

            For Each r In records
                DgvRecords.Rows.Add(
                    r.DateTime.ToString("yyyy-MM-dd HH:mm"),
                    r.Parameter,
                    r.Value,
                    r.Units,
                    r.Source)
            Next

            DgvRecords.ClearSelection()
            DgvRecords.Refresh()

            Log.Write($"[Grid] DgvRecords updated with {DgvRecords.Rows.Count} rows")
        Catch ex As Exception
            Log.WriteException(ex, "[Grid] Error updating DgvRecords")
        End Try
    End Sub

End Class