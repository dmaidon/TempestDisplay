Imports System.Data.SQLite
Imports System.IO
Imports System.Math

''' <summary>
''' SQLite-backed hi/lo tracking for core weather metrics.
''' DB file lives in Globals.DataDir (e.g. Data/HiLoWeather.sqlite).
''' </summary>
Friend Module HiLoDatabase

    Private ReadOnly DbFilePath As String = Path.Combine(Globals.DataDir, "HiLoWeather.sqlite")

    ''' <summary>
    ''' Ensure database file and schema exist.
    ''' Non-throwing: logs and returns on failure.
    ''' </summary>
    Friend Sub EnsureHiLoDatabase()
        Try
            If Not Directory.Exists(Globals.DataDir) Then
                Directory.CreateDirectory(Globals.DataDir)
            End If

            Dim createNew As Boolean = Not File.Exists(DbFilePath)

            If createNew Then
                SQLiteConnection.CreateFile(DbFilePath)
            End If

            Using conn As New SQLiteConnection($"Data Source={DbFilePath};Version=3;Journal Mode=WAL;Synchronous=NORMAL;")
                conn.Open()

                ' Apply schema from embedded .sql file on first creation or when empty
                Dim sqlPath = Path.Combine(Application.StartupPath, "Data", "HiLoWeather.sqlite.sql")
                If File.Exists(sqlPath) Then
                    Dim schemaSql = File.ReadAllText(sqlPath)
                    Using cmd As New SQLiteCommand(schemaSql, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                Else
                    ' Fallback: minimal inline schema if .sql file missing
                    Const inlineSql As String = "CREATE TABLE IF NOT EXISTS HiLoDaily (" &
                                                "Id INTEGER PRIMARY KEY AUTOINCREMENT," &
                                                "ObsDate TEXT NOT NULL," &
                                                "TempHigh REAL, TempHighTime TEXT," &
                                                "TempLow REAL, TempLowTime TEXT," &
                                                "FeelsLikeHigh REAL, FeelsLikeHighTime TEXT," &
                                                "FeelsLikeLow REAL, FeelsLikeLowTime TEXT," &
                                                "HeatIndexHigh REAL, HeatIndexHighTime TEXT," &
                                                "WindChillLow REAL, WindChillLowTime TEXT," &
                                                "RainDay REAL, RainMonth REAL, RainYear REAL," &
                                                "WindSpeedHigh REAL, WindSpeedHighTime TEXT," &
                                                "WindSpeedAvg REAL, WindDirAvg REAL," &
                                                "LastUpdated TEXT NOT NULL);" &
                                                "CREATE UNIQUE INDEX IF NOT EXISTS IX_HiLoDaily_ObsDate ON HiLoDaily(ObsDate);" &
                                                "CREATE TABLE IF NOT EXISTS HiLoAllTime (" &
                                                "Id INTEGER PRIMARY KEY CHECK (Id = 1)," &
                                                "TempHigh REAL, TempHighTime TEXT," &
                                                "TempLow REAL, TempLowTime TEXT," &
                                                "HeatIndexHigh REAL, HeatIndexHighTime TEXT," &
                                                "WindChillLow REAL, WindChillLowTime TEXT," &
                                                "RainDayMax REAL, RainDayMaxDate TEXT," &
                                                "RainMonthMax REAL, RainMonthMaxYear INTEGER, RainMonthMaxMonth INTEGER," &
                                                "WindSpeedHigh REAL, WindSpeedHighTime TEXT," &
                                                "WindDirSumX REAL, WindDirSumY REAL, WindDirSampleCount INTEGER," &
                                                "LastUpdated TEXT);" &
                                                "INSERT OR IGNORE INTO HiLoAllTime (Id, WindDirSumX, WindDirSumY, WindDirSampleCount) VALUES (1, 0.0, 0.0, 0);"

                    Using cmd As New SQLiteCommand(inlineSql, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using

            Log.Write($"[HiLoDatabase] Initialized at {DbFilePath}")
        Catch ex As Exception
            Log.WriteException(ex, "[HiLoDatabase] Failed to initialize hi/lo database")
        End Try
    End Sub

    ''' <summary>
    ''' Update (or insert) daily hi/lo record for the given observation.
    ''' Values are in Fahrenheit (temps), inches (rain), mph (wind), degrees (direction).
    ''' </summary>
    Friend Sub UpdateDailyHiLo(
        obsTimeLocal As DateTime,
        tempF As Double?,
        feelsLikeF As Double?,
        heatIndexF As Double?,
        windChillF As Double?,
        rainDayIn As Double?,
        rainMonthIn As Double?,
        rainYearIn As Double?,
        windSpeedMph As Double?,
        windDirDeg As Double?)

        Try
            Dim obsDate As String = obsTimeLocal.ToString("yyyy-MM-dd")
            Dim nowIso As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

            Using conn As New SQLiteConnection($"Data Source={DbFilePath};Version=3;Journal Mode=WAL;Synchronous=NORMAL;")
                conn.Open()

                ' Load existing row if any
                Dim existing As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                Using selectCmd As New SQLiteCommand("SELECT * FROM HiLoDaily WHERE ObsDate = @date", conn)
                    selectCmd.Parameters.AddWithValue("@date", obsDate)
                    Using reader = selectCmd.ExecuteReader()
                        If reader.Read() Then
                            For i = 0 To reader.FieldCount - 1
                                existing(reader.GetName(i)) = reader.GetValue(i)
                            Next
                        End If
                    End Using
                End Using

                Dim isNew As Boolean = existing.Count = 0

                ' Helper locals to read existing numeric values
                Dim tempHigh As Double? = GetNullableDouble(existing, "TempHigh")
                Dim tempLow As Double? = GetNullableDouble(existing, "TempLow")
                Dim feelsHigh As Double? = GetNullableDouble(existing, "FeelsLikeHigh")
                Dim feelsLow As Double? = GetNullableDouble(existing, "FeelsLikeLow")
                Dim hiHigh As Double? = GetNullableDouble(existing, "HeatIndexHigh")
                Dim wcLow As Double? = GetNullableDouble(existing, "WindChillLow")
                Dim rainDay As Double? = GetNullableDouble(existing, "RainDay")
                Dim rainMonth As Double? = GetNullableDouble(existing, "RainMonth")
                Dim rainYear As Double? = GetNullableDouble(existing, "RainYear")
                Dim windHigh As Double? = GetNullableDouble(existing, "WindSpeedHigh")
                Dim windDirAvg As Double? = GetNullableDouble(existing, "WindDirAvg")
                Dim windAvg As Double? = GetNullableDouble(existing, "WindSpeedAvg")

                Dim obsTimeStr As String = obsTimeLocal.ToString("yyyy-MM-dd HH:mm:ss")

                ' Temperature hi/lo (daily)
                If tempF.HasValue Then
                    If Not tempHigh.HasValue OrElse tempF.Value > tempHigh.Value Then
                        tempHigh = tempF
                        existing("TempHighTime") = obsTimeStr
                    End If
                    If Not tempLow.HasValue OrElse tempF.Value < tempLow.Value Then
                        tempLow = tempF
                        existing("TempLowTime") = obsTimeStr
                    End If
                End If

                ' Feels like hi/lo (daily)
                If feelsLikeF.HasValue Then
                    If Not feelsHigh.HasValue OrElse feelsLikeF.Value > feelsHigh.Value Then
                        feelsHigh = feelsLikeF
                        existing("FeelsLikeHighTime") = obsTimeStr
                    End If
                    If Not feelsLow.HasValue OrElse feelsLikeF.Value < feelsLow.Value Then
                        feelsLow = feelsLikeF
                        existing("FeelsLikeLowTime") = obsTimeStr
                    End If
                End If

                ' Heat index high (daily)
                If heatIndexF.HasValue Then
                    If Not hiHigh.HasValue OrElse heatIndexF.Value > hiHigh.Value Then
                        hiHigh = heatIndexF
                        existing("HeatIndexHighTime") = obsTimeStr
                    End If
                End If

                ' Wind chill low (daily)
                If windChillF.HasValue Then
                    If Not wcLow.HasValue OrElse windChillF.Value < wcLow.Value Then
                        wcLow = windChillF
                        existing("WindChillLowTime") = obsTimeStr
                    End If
                End If

                ' Rain totals (daily): last value wins (cumulative)
                If rainDayIn.HasValue Then rainDay = rainDayIn
                If rainMonthIn.HasValue Then rainMonth = rainMonthIn
                If rainYearIn.HasValue Then rainYear = rainYearIn

                ' Wind speed high (daily)
                If windSpeedMph.HasValue Then
                    If Not windHigh.HasValue OrElse windSpeedMph.Value > windHigh.Value Then
                        windHigh = windSpeedMph
                        existing("WindSpeedHighTime") = obsTimeStr
                    End If
                End If

                ' Very simple running averages for wind speed and direction (daily)
                If windSpeedMph.HasValue Then
                    If Not windAvg.HasValue Then
                        windAvg = windSpeedMph
                    Else
                        windAvg = (windAvg.Value + windSpeedMph.Value) / 2.0
                    End If
                End If

                If windDirDeg.HasValue Then
                    If Not windDirAvg.HasValue Then
                        windDirAvg = windDirDeg
                    Else
                        windDirAvg = (windDirAvg.Value + windDirDeg.Value) / 2.0
                    End If
                End If

                ' Upsert daily row
                Using cmd As New SQLiteCommand(conn)
                    If isNew Then
                        cmd.CommandText = "INSERT INTO HiLoDaily (ObsDate, TempHigh, TempHighTime, TempLow, TempLowTime, " &
                                          "FeelsLikeHigh, FeelsLikeHighTime, FeelsLikeLow, FeelsLikeLowTime, " &
                                          "HeatIndexHigh, HeatIndexHighTime, WindChillLow, WindChillLowTime, " &
                                          "RainDay, RainMonth, RainYear, WindSpeedHigh, WindSpeedHighTime, " &
                                          "WindSpeedAvg, WindDirAvg, LastUpdated) " &
                                          "VALUES (@date, @th, @tht, @tl, @tlt, @flh, @flht, @fll, @fllt, " &
                                          "@hih, @hiht, @wcl, @wclt, @rd, @rm, @ry, @wsh, @wsht, @wsa, @wda, @lu)"
                    Else
                        cmd.CommandText = "UPDATE HiLoDaily SET " &
                                          "TempHigh=@th, TempHighTime=@tht, " &
                                          "TempLow=@tl, TempLowTime=@tlt, " &
                                          "FeelsLikeHigh=@flh, FeelsLikeHighTime=@flht, " &
                                          "FeelsLikeLow=@fll, FeelsLikeLowTime=@fllt, " &
                                          "HeatIndexHigh=@hih, HeatIndexHighTime=@hiht, " &
                                          "WindChillLow=@wcl, WindChillLowTime=@wclt, " &
                                          "RainDay=@rd, RainMonth=@rm, RainYear=@ry, " &
                                          "WindSpeedHigh=@wsh, WindSpeedHighTime=@wsht, " &
                                          "WindSpeedAvg=@wsa, WindDirAvg=@wda, " &
                                          "LastUpdated=@lu WHERE ObsDate=@date"
                    End If

                    cmd.Parameters.AddWithValue("@date", obsDate)
                    cmd.Parameters.AddWithValue("@th", CType(If(tempHigh, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@tht", CType(If(existing.GetValueOrDefault("TempHighTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@tl", CType(If(tempLow, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@tlt", CType(If(existing.GetValueOrDefault("TempLowTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@flh", CType(If(feelsHigh, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@flht", CType(If(existing.GetValueOrDefault("FeelsLikeHighTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@fll", CType(If(feelsLow, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@fllt", CType(If(existing.GetValueOrDefault("FeelsLikeLowTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@hih", CType(If(hiHigh, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@hiht", CType(If(existing.GetValueOrDefault("HeatIndexHighTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@wcl", CType(If(wcLow, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@wclt", CType(If(existing.GetValueOrDefault("WindChillLowTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@rd", CType(If(rainDay, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@rm", CType(If(rainMonth, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@ry", CType(If(rainYear, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@wsh", CType(If(windHigh, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@wsht", CType(If(existing.GetValueOrDefault("WindSpeedHighTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@wsa", CType(If(windAvg, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@wda", CType(If(windDirAvg, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@lu", nowIso)

                    cmd.ExecuteNonQuery()
                End Using

                ' Also update all-time records from this observation
                UpdateAllTimeRecords(conn, obsTimeLocal, tempF, heatIndexF, windChillF, rainDayIn, rainMonthIn, windSpeedMph, windDirDeg)
            End Using
        Catch ex As Exception
            Log.WriteException(ex, "[HiLoDatabase] Failed to update daily hi/lo")
        End Try
    End Sub

    ''' <summary>
    ''' Update global all-time records (single-row table HiLoAllTime).
    ''' Tracks: record high/low temp, record high heat index, record low wind chill,
    ''' max daily rain, max monthly rain, and highest wind speed. Wind direction is
    ''' accumulated as a vector for long-term averaging.
    ''' </summary>
    Private Sub UpdateAllTimeRecords(
        conn As SQLiteConnection,
        obsTimeLocal As DateTime,
        tempF As Double?,
        heatIndexF As Double?,
        windChillF As Double?,
        rainDayIn As Double?,
        rainMonthIn As Double?,
        windSpeedMph As Double?,
        windDirDeg As Double?)

        Try
            Dim nowIso As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim obsTimeStr As String = obsTimeLocal.ToString("yyyy-MM-dd HH:mm:ss")
            Dim obsDateStr As String = obsTimeLocal.ToString("yyyy-MM-dd")

            ' Load existing all-time row (Id=1)
            Dim existing As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
            Using selectCmd As New SQLiteCommand("SELECT * FROM HiLoAllTime WHERE Id = 1", conn)
                Using reader = selectCmd.ExecuteReader()
                    If reader.Read() Then
                        For i = 0 To reader.FieldCount - 1
                            existing(reader.GetName(i)) = reader.GetValue(i)
                        Next
                    End If
                End Using
            End Using

            ' Make sure row exists
            If existing.Count = 0 Then
                Using insertCmd As New SQLiteCommand("INSERT OR IGNORE INTO HiLoAllTime (Id, WindDirSumX, WindDirSumY, WindDirSampleCount, LastUpdated) VALUES (1, 0.0, 0.0, 0, @lu)", conn)
                    insertCmd.Parameters.AddWithValue("@lu", nowIso)
                    insertCmd.ExecuteNonQuery()
                End Using

                Using reloadCmd As New SQLiteCommand("SELECT * FROM HiLoAllTime WHERE Id = 1", conn)
                    Using reader = reloadCmd.ExecuteReader()
                        If reader.Read() Then
                            For i = 0 To reader.FieldCount - 1
                                existing(reader.GetName(i)) = reader.GetValue(i)
                            Next
                        End If
                    End Using
                End Using
            End If

            Dim tempHigh As Double? = GetNullableDouble(existing, "TempHigh")
            Dim tempLow As Double? = GetNullableDouble(existing, "TempLow")
            Dim hiHigh As Double? = GetNullableDouble(existing, "HeatIndexHigh")
            Dim wcLow As Double? = GetNullableDouble(existing, "WindChillLow")
            Dim rainDayMax As Double? = GetNullableDouble(existing, "RainDayMax")
            Dim rainMonthMax As Double? = GetNullableDouble(existing, "RainMonthMax")
            Dim windHigh As Double? = GetNullableDouble(existing, "WindSpeedHigh")

            Dim windDirSumX As Double = GetNullableDouble(existing, "WindDirSumX").GetValueOrDefault(0.0)
            Dim windDirSumY As Double = GetNullableDouble(existing, "WindDirSumY").GetValueOrDefault(0.0)
            Dim windDirCount As Integer = CInt(GetNullableDouble(existing, "WindDirSampleCount").GetValueOrDefault(0.0))

            ' Record temperature high/low
            If tempF.HasValue Then
                If Not tempHigh.HasValue OrElse tempF.Value > tempHigh.Value Then
                    tempHigh = tempF
                    existing("TempHighTime") = obsTimeStr
                End If
                If Not tempLow.HasValue OrElse tempF.Value < tempLow.Value Then
                    tempLow = tempF
                    existing("TempLowTime") = obsTimeStr
                End If
            End If

            ' Record heat index high
            If heatIndexF.HasValue Then
                If Not hiHigh.HasValue OrElse heatIndexF.Value > hiHigh.Value Then
                    hiHigh = heatIndexF
                    existing("HeatIndexHighTime") = obsTimeStr
                End If
            End If

            ' Record wind chill low
            If windChillF.HasValue Then
                If Not wcLow.HasValue OrElse windChillF.Value < wcLow.Value Then
                    wcLow = windChillF
                    existing("WindChillLowTime") = obsTimeStr
                End If
            End If

            ' Record max daily rain (by absolute daily total)
            If rainDayIn.HasValue Then
                If Not rainDayMax.HasValue OrElse rainDayIn.Value > rainDayMax.Value Then
                    rainDayMax = rainDayIn
                    existing("RainDayMaxDate") = obsDateStr
                End If
            End If

            ' Record max monthly rain (by absolute monthly total)
            If rainMonthIn.HasValue Then
                If Not rainMonthMax.HasValue OrElse rainMonthIn.Value > rainMonthMax.Value Then
                    rainMonthMax = rainMonthIn
                    existing("RainMonthMaxYear") = obsTimeLocal.Year
                    existing("RainMonthMaxMonth") = obsTimeLocal.Month
                End If
            End If

            ' Record max wind speed
            If windSpeedMph.HasValue Then
                If Not windHigh.HasValue OrElse windSpeedMph.Value > windHigh.Value Then
                    windHigh = windSpeedMph
                    existing("WindSpeedHighTime") = obsTimeStr
                End If
            End If

            ' Vector-accumulate wind direction for long-term average
            If windDirDeg.HasValue Then
                Dim rad As Double = (windDirDeg.Value * PI) / 180.0
                windDirSumX += Cos(rad)
                windDirSumY += Sin(rad)
                windDirCount += 1
            End If

            ' Persist all-time updates
            Using cmd As New SQLiteCommand("UPDATE HiLoAllTime SET " &
                                           "TempHigh=@th, TempHighTime=@tht, " &
                                           "TempLow=@tl, TempLowTime=@tlt, " &
                                           "HeatIndexHigh=@hih, HeatIndexHighTime=@hiht, " &
                                           "WindChillLow=@wcl, WindChillLowTime=@wclt, " &
                                           "RainDayMax=@rdm, RainDayMaxDate=@rdmd, " &
                                           "RainMonthMax=@rmm, RainMonthMaxYear=@rmmy, RainMonthMaxMonth=@rmmm, " &
                                           "WindSpeedHigh=@wsh, WindSpeedHighTime=@wsht, " &
                                           "WindDirSumX=@wdx, WindDirSumY=@wdy, WindDirSampleCount=@wdc, " &
                                           "LastUpdated=@lu WHERE Id = 1", conn)

                cmd.Parameters.AddWithValue("@th", CType(If(tempHigh, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@tht", CType(If(existing.GetValueOrDefault("TempHighTime"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@tl", CType(If(tempLow, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@tlt", CType(If(existing.GetValueOrDefault("TempLowTime"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@hih", CType(If(hiHigh, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@hiht", CType(If(existing.GetValueOrDefault("HeatIndexHighTime"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@wcl", CType(If(wcLow, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@wclt", CType(If(existing.GetValueOrDefault("WindChillLowTime"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@rdm", CType(If(rainDayMax, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@rdmd", CType(If(existing.GetValueOrDefault("RainDayMaxDate"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@rmm", CType(If(rainMonthMax, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@rmmy", CType(If(existing.GetValueOrDefault("RainMonthMaxYear"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@rmmm", CType(If(existing.GetValueOrDefault("RainMonthMaxMonth"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@wsh", CType(If(windHigh, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@wsht", CType(If(existing.GetValueOrDefault("WindSpeedHighTime"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@wdx", windDirSumX)
                cmd.Parameters.AddWithValue("@wdy", windDirSumY)
                cmd.Parameters.AddWithValue("@wdc", windDirCount)
                cmd.Parameters.AddWithValue("@lu", nowIso)

                cmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Log.WriteException(ex, "[HiLoDatabase] Failed to update all-time records")
        End Try
    End Sub

    Private Function GetNullableDouble(dict As Dictionary(Of String, Object), key As String) As Double?
        Dim v As Object = Nothing
        If dict Is Nothing OrElse Not dict.TryGetValue(key, v) Then Return Nothing
        If v Is Nothing OrElse v Is DBNull.Value Then Return Nothing
        Dim d As Double
        If Double.TryParse(v.ToString(), d) Then Return d
        Return Nothing
    End Function

End Module