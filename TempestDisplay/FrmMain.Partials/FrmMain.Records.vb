Imports System.Data.SQLite
Imports System.IO

Partial Public Class FrmMain

    ''' <summary>
    ''' Load and display HiLo weather records from the SQLite database
    ''' </summary>
    Private Sub LoadHiLoRecords()
        Try
            Dim dbPath As String = Path.Combine(DataDir, "HiLoWeather.sqlite")
            Log.Write($"[Records] Looking for database at: {dbPath}")

            ' Check if database file exists
            If Not File.Exists(dbPath) Then
                Log.Write("[Records] HiLoWeather.sqlite database not found at: " & dbPath)
                ' Add message to grid
                DgvRecords.Rows.Clear()
                DgvRecords.Rows.Add("No database found", "Database will be created", "when data is available")
                Return
            End If

            Log.Write("[Records] Database file found, loading records...")

            ' Clear existing rows
            DgvRecords.Rows.Clear()

            Using conn As New SQLiteConnection($"Data Source={dbPath};Version=3;Read Only=True;")
                conn.Open()

                ' Check if tables exist
                Dim tableCount As Integer = 0
                Using cmd As New SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('HiLoAllTime', 'HiLoDaily')", conn)
                    tableCount = CInt(cmd.ExecuteScalar())
                End Using

                If tableCount < 2 Then
                    Log.Write($"[Records] Database tables not found. Found {tableCount} of 2 required tables")
                    DgvRecords.Rows.Add("Database exists but", "tables not initialized", "Waiting for data...")
                    Return
                End If

                Log.Write($"[Records] Found {tableCount} tables, loading records...")

                ' Load All-Time records
                LoadAllTimeRecords(conn)

                ' Load recent daily records (last 30 days)
                LoadRecentDailyRecords(conn)

                Log.Write($"[Records] Grid now has {DgvRecords.Rows.Count} rows")
            End Using

            DgvRecords.ClearSelection()
            Log.Write("[Records] HiLo records loaded successfully")
        Catch ex As Exception
            Log.WriteException(ex, "[Records] Error loading HiLo records")
            DgvRecords.Rows.Clear()
            DgvRecords.Rows.Add("Error loading records", ex.Message, "Check log for details")
        End Try
    End Sub

    ''' <summary>
    ''' Load all-time records from HiLoAllTime table
    ''' </summary>
    Private Sub LoadAllTimeRecords(conn As SQLiteConnection)
        Try
            Log.Write("[Records] Loading all-time records...")
            Using cmd As New SQLiteCommand("SELECT * FROM HiLoAllTime WHERE Id = 1", conn)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Log.Write("[Records] All-time record found, adding to grid")

                        ' Add section header row with header text in row header
                        Dim headerRow As Integer = DgvRecords.Rows.Add("", "", "")
                        DgvRecords.Rows(headerRow).HeaderCell.Value = "═══ ALL-TIME RECORDS ═══"

                        ' Add blank spacer
                        DgvRecords.Rows.Add("", "", "")

                        ' Temperature Records Row
                        Dim tempHighStr As String = FormatValueTime(reader, "TempHigh", "TempHighTime", "°F")
                        Dim tempLowStr As String = FormatValueTime(reader, "TempLow", "TempLowTime", "°F")
                        Dim tempRow As Integer = DgvRecords.Rows.Add("", $"High: {tempHighStr}" & vbCrLf & $"Low: {tempLowStr}", "")
                        DgvRecords.Rows(tempRow).HeaderCell.Value = "Temperature"

                        ' Heat Index & Wind Chill Row
                        Dim heatIndexStr As String = FormatValueTime(reader, "HeatIndexHigh", "HeatIndexHighTime", "°F")
                        Dim windChillStr As String = FormatValueTime(reader, "WindChillLow", "WindChillLowTime", "°F")
                        Dim feelsRow As Integer = DgvRecords.Rows.Add("", $"Heat Index: {heatIndexStr}" & vbCrLf & $"Wind Chill: {windChillStr}", "")
                        DgvRecords.Rows(feelsRow).HeaderCell.Value = "Feels Like"

                        ' Rain Records Row
                        Dim rainDayStr As String = FormatValueDate(reader, "RainDayMax", "RainDayMaxDate", "in")

                        Dim rainMonthMax As Double? = If(IsDBNull(reader("RainMonthMax")), CType(Nothing, Double?), CDbl(reader("RainMonthMax")))
                        Dim rainMonthYear As Integer? = If(IsDBNull(reader("RainMonthMaxYear")), CType(Nothing, Integer?), CInt(reader("RainMonthMaxYear")))
                        Dim rainMonthMonth As Integer? = If(IsDBNull(reader("RainMonthMaxMonth")), CType(Nothing, Integer?), CInt(reader("RainMonthMaxMonth")))

                        Dim rainMonthStr As String
                        If rainMonthMax.HasValue AndAlso rainMonthYear.HasValue AndAlso rainMonthMonth.HasValue Then
                            rainMonthStr = $"Max Month: {rainMonthMax.Value:F2} in ({rainMonthYear.Value}-{rainMonthMonth.Value:D2})"
                        Else
                            rainMonthStr = "Max Month: N/A"
                        End If

                        Dim rainRow As Integer = DgvRecords.Rows.Add($"Max Day: {rainDayStr}" & vbCrLf & rainMonthStr, "", "")
                        DgvRecords.Rows(rainRow).HeaderCell.Value = "Rainfall"

                        ' Wind Records Row
                        Dim windSpeedStr As String = FormatValueTime(reader, "WindSpeedHigh", "WindSpeedHighTime", "mph")
                        Dim windRow As Integer = DgvRecords.Rows.Add("", "", $"Max Speed: {windSpeedStr}")
                        DgvRecords.Rows(windRow).HeaderCell.Value = "Wind"

                        ' Add separator and daily records header
                        DgvRecords.Rows.Add("", "", "")
                        Dim dailyHeaderRow As Integer = DgvRecords.Rows.Add("", "", "")
                        DgvRecords.Rows(dailyHeaderRow).HeaderCell.Value = "═══ RECENT DAILY RECORDS ═══"
                        DgvRecords.Rows.Add("", "", "")
                    Else
                        Log.Write("[Records] No all-time records found in database")
                        DgvRecords.Rows.Add("No records yet", "Data will appear", "as observations arrive")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Log.WriteException(ex, "[Records] Error loading all-time records")
            DgvRecords.Rows.Add("Error loading all-time", ex.Message, "")
        End Try
    End Sub

    ''' <summary>
    ''' Load recent daily records from HiLoDaily table
    ''' </summary>
    Private Sub LoadRecentDailyRecords(conn As SQLiteConnection)
        Try
            Log.Write("[Records] Loading recent daily records...")
            Dim sql As String = "SELECT * FROM HiLoDaily ORDER BY ObsDate DESC LIMIT 10"

            Using cmd As New SQLiteCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    Dim recordCount As Integer = 0
                    While reader.Read()
                        recordCount += 1
                        Log.Write($"[Records] Processing daily record #{recordCount}")
                        Dim obsDate = If(IsDBNull(reader("ObsDate")), "", reader("ObsDate").ToString())

                        ' Build rain string
                        Dim rainStr As String = FormatValue(reader, "RainDay", "in")

                        ' Build temperature string
                        Dim tempHighStr As String = FormatValueTime(reader, "TempHigh", "TempHighTime", "°F")
                        Dim tempLowStr As String = FormatValueTime(reader, "TempLow", "TempLowTime", "°F")
                        Dim tempStr As String = $"High: {tempHighStr}" & vbCrLf & $"Low: {tempLowStr}"

                        ' Build wind string
                        Dim windStr As String = FormatValueTime(reader, "WindSpeedHigh", "WindSpeedHighTime", "mph")

                        ' Add data row with date in row header
                        Dim dataRow As Integer = DgvRecords.Rows.Add(rainStr, tempStr, windStr)
                        DgvRecords.Rows(dataRow).HeaderCell.Value = obsDate

                        ' Add spacing
                        If recordCount < 10 Then ' Don't add extra space after last record
                            DgvRecords.Rows.Add("", "", "")
                        End If
                    End While

                    If recordCount = 0 Then
                        Log.Write("[Records] No daily records found in database")
                    Else
                        Log.Write($"[Records] Loaded {recordCount} daily records")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Log.WriteException(ex, "[Records] Error loading daily records")
            DgvRecords.Rows.Add("Error loading daily", ex.Message, "")
        End Try
    End Sub

    ''' <summary>
    ''' Format a numeric value with units
    ''' </summary>
    Private Function FormatValue(reader As SQLiteDataReader, fieldName As String, units As String) As String
        If IsDBNull(reader(fieldName)) Then
            Return "N/A"
        End If

        Dim value As Double = CDbl(reader(fieldName))
        Return $"{value:F2} {units}"
    End Function

    ''' <summary>
    ''' Format a value with its time
    ''' </summary>
    Private Function FormatValueTime(reader As SQLiteDataReader, valueField As String, timeField As String, units As String) As String
        If IsDBNull(reader(valueField)) Then
            Return "N/A"
        End If

        Dim value As Double = CDbl(reader(valueField))
        Dim timeStr As String = ""

        If Not IsDBNull(reader(timeField)) Then
            Dim dateStr = reader(timeField).ToString()
            Dim dt As DateTime
            If DateTime.TryParse(dateStr, dt) Then
                timeStr = " @ " & dt.ToString("HH:mm")
            End If
        End If

        Return $"{value:F2} {units}{timeStr}"
    End Function

    ''' <summary>
    ''' Format a value with its date
    ''' </summary>
    Private Function FormatValueDate(reader As SQLiteDataReader, valueField As String, dateField As String, units As String) As String
        If IsDBNull(reader(valueField)) Then
            Return "N/A"
        End If

        Dim value As Double = CDbl(reader(valueField))
        Dim dateStr As String = ""

        If Not IsDBNull(reader(dateField)) Then
            Dim dtStr = reader(dateField).ToString()
            Dim dt As DateTime
            If DateTime.TryParse(dtStr, dt) Then
                dateStr = " on " & dt.ToString("yyyy-MM-dd")
            End If
        End If

        Return $"{value:F2} {units}{dateStr}"
    End Function

End Class
