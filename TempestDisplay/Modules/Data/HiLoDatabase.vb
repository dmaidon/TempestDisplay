' Last Edit: January 15, 2026 (Fixed wind averaging algorithms, added data validation, optimized migrations, added indexes)
Imports System.Data.SQLite
Imports System.IO
Imports System.Math

''' <summary>
''' SQLite-backed hi/lo tracking for core weather metrics.
''' DB file lives in Globals.DataDir (e.g. Data/HiLoWeather.sqlite).
''' </summary>
Friend Module HiLoDatabase

    Private ReadOnly DbFilePath As String = Path.Combine(Globals.DataDir, "HiLoWeather.sqlite")

    ' Data validation constants
    Private Const MinValidTempF As Double = -100.0
    Private Const MaxValidTempF As Double = 150.0
    Private Const MinValidWindMph As Double = 0.0
    Private Const MaxValidWindMph As Double = 250.0
    Private Const MinValidUvIndex As Double = 0.0
    Private Const MaxValidUvIndex As Double = 20.0
    Private Const MinValidSolarRadiation As Double = 0.0
    Private Const MaxValidSolarRadiation As Double = 2000.0
    Private Const MaxValidRainInches As Double = 100.0

    ' Helper structure for wind averaging
    Private Structure WindAverage
        Public SumX As Double
        Public SumY As Double
        Public Count As Integer

        Public Sub AddSample(directionDegrees As Double)
            Dim rad As Double = (directionDegrees * PI) / 180.0
            SumX += Cos(rad)
            SumY += Sin(rad)
            Count += 1
        End Sub

        Public ReadOnly Property AverageDegrees As Double
            Get
                If Count = 0 Then Return 0
                Dim avgRad As Double = Atan2(SumY, SumX)
                Dim avgDeg As Double = (avgRad * 180.0) / PI
                If avgDeg < 0 Then avgDeg += 360.0
                Return avgDeg
            End Get
        End Property
    End Structure

    ' Helper structure for running speed average
    Private Structure SpeedAverage
        Public Total As Double
        Public Count As Integer

        Public Sub AddSample(speed As Double)
            Total += speed
            Count += 1
        End Sub

        Public ReadOnly Property Average As Double
            Get
                If Count = 0 Then Return 0
                Return Total / Count
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' Validates sensor data is within reasonable bounds to prevent garbage data.
    ''' </summary>
    Private Function ValidateValue(value As Double?, minVal As Double, maxVal As Double) As Double?
        If Not value.HasValue Then Return Nothing
        If value.Value < minVal OrElse value.Value > maxVal Then
            Return Nothing ' Filter out invalid readings
        End If
        Return value
    End Function

    ''' <summary>
    ''' Helper to check if a column exists in a table and add it if missing.
    ''' </summary>
    Private Sub AddColumnIfMissing(conn As SQLiteConnection, tableName As String, columnName As String, columnType As String)
        Dim hasColumn As Boolean = False
        Using cmd As New SQLiteCommand($"PRAGMA table_info({tableName})", conn)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    If reader("name").ToString() = columnName Then
                        hasColumn = True
                        Exit While
                    End If
                End While
            End Using
        End Using

        If Not hasColumn Then
            Using cmd As New SQLiteCommand($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType}", conn)
                cmd.ExecuteNonQuery()
            End Using
        End If
    End Sub

    ''' <summary>
    ''' Migrate existing database to add new columns if they don't exist.
    ''' Safe to call on new or existing databases - checks for column existence first.
    ''' </summary>
    Private Sub MigrateDatabase(conn As SQLiteConnection)
        Try
            Log.Write("[HiLoDatabase] Checking for schema migrations...")

            ' Migrate HiLoDaily table
            AddColumnIfMissing(conn, "HiLoDaily", "UVIndexHigh", "REAL")
            AddColumnIfMissing(conn, "HiLoDaily", "UVIndexHighTime", "TEXT")
            AddColumnIfMissing(conn, "HiLoDaily", "SolarRadiationHigh", "REAL")
            AddColumnIfMissing(conn, "HiLoDaily", "SolarRadiationHighTime", "TEXT")
            ' Add proper wind averaging support (vector-based for direction, cumulative for speed)
            AddColumnIfMissing(conn, "HiLoDaily", "WindDirSumX", "REAL")
            AddColumnIfMissing(conn, "HiLoDaily", "WindDirSumY", "REAL")
            AddColumnIfMissing(conn, "HiLoDaily", "WindDirSampleCount", "INTEGER")
            AddColumnIfMissing(conn, "HiLoDaily", "WindSpeedTotal", "REAL")
            AddColumnIfMissing(conn, "HiLoDaily", "WindSpeedCount", "INTEGER")

            ' Migrate HiLoAllTime table
            AddColumnIfMissing(conn, "HiLoAllTime", "UVIndexHigh", "REAL")
            AddColumnIfMissing(conn, "HiLoAllTime", "UVIndexHighTime", "TEXT")
            AddColumnIfMissing(conn, "HiLoAllTime", "SolarRadiationHigh", "REAL")
            AddColumnIfMissing(conn, "HiLoAllTime", "SolarRadiationHighTime", "TEXT")

            ' Add indexes for better query performance on timestamp lookups
            Using cmd As New SQLiteCommand("CREATE INDEX IF NOT EXISTS IX_HiLoDaily_TempHighTime ON HiLoDaily(TempHighTime)", conn)
                cmd.ExecuteNonQuery()
            End Using
            Using cmd As New SQLiteCommand("CREATE INDEX IF NOT EXISTS IX_HiLoDaily_LastUpdated ON HiLoDaily(LastUpdated)", conn)
                cmd.ExecuteNonQuery()
            End Using

            Log.Write("[HiLoDatabase] Schema migration complete")
        Catch ex As Exception
            Log.WriteException(ex, "[HiLoDatabase] Error during database migration")
        End Try
    End Sub

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

                ' Migrate existing database to add UV and Solar Radiation columns if they don't exist
                MigrateDatabase(conn)

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
                                                "WindSpeedTotal REAL DEFAULT 0, WindSpeedCount INTEGER DEFAULT 0," &
                                                "WindDirSumX REAL DEFAULT 0, WindDirSumY REAL DEFAULT 0, WindDirSampleCount INTEGER DEFAULT 0," &
                                                "UVIndexHigh REAL, UVIndexHighTime TEXT," &
                                                "SolarRadiationHigh REAL, SolarRadiationHighTime TEXT," &
                                                "LastUpdated TEXT NOT NULL);" &
                                                "CREATE UNIQUE INDEX IF NOT EXISTS IX_HiLoDaily_ObsDate ON HiLoDaily(ObsDate);" &
                                                "CREATE INDEX IF NOT EXISTS IX_HiLoDaily_LastUpdated ON HiLoDaily(LastUpdated);" &
                                                "CREATE TABLE IF NOT EXISTS HiLoAllTime (" &
                                                "Id INTEGER PRIMARY KEY CHECK (Id = 1)," &
                                                "TempHigh REAL, TempHighTime TEXT," &
                                                "TempLow REAL, TempLowTime TEXT," &
                                                "HeatIndexHigh REAL, HeatIndexHighTime TEXT," &
                                                "WindChillLow REAL, WindChillLowTime TEXT," &
                                                "RainDayMax REAL, RainDayMaxDate TEXT," &
                                                "RainMonthMax REAL, RainMonthMaxYear INTEGER, RainMonthMaxMonth INTEGER," &
                                                "WindSpeedHigh REAL, WindSpeedHighTime TEXT," &
                                                "UVIndexHigh REAL, UVIndexHighTime TEXT," &
                                                "SolarRadiationHigh REAL, SolarRadiationHighTime TEXT," &
                                                "WindDirSumX REAL DEFAULT 0, WindDirSumY REAL DEFAULT 0, WindDirSampleCount INTEGER DEFAULT 0," &
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
        windDirDeg As Double?,
        uvIndex As Double?,
        solarRadiation As Double?)

        Try
            ' Validate all input values to prevent garbage data
            tempF = ValidateValue(tempF, MinValidTempF, MaxValidTempF)
            feelsLikeF = ValidateValue(feelsLikeF, MinValidTempF, MaxValidTempF)
            heatIndexF = ValidateValue(heatIndexF, MinValidTempF, MaxValidTempF)
            windChillF = ValidateValue(windChillF, MinValidTempF, MaxValidTempF)
            rainDayIn = ValidateValue(rainDayIn, 0, MaxValidRainInches)
            rainMonthIn = ValidateValue(rainMonthIn, 0, MaxValidRainInches)
            rainYearIn = ValidateValue(rainYearIn, 0, MaxValidRainInches * 365)
            windSpeedMph = ValidateValue(windSpeedMph, MinValidWindMph, MaxValidWindMph)
            uvIndex = ValidateValue(uvIndex, MinValidUvIndex, MaxValidUvIndex)
            solarRadiation = ValidateValue(solarRadiation, MinValidSolarRadiation, MaxValidSolarRadiation)
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
                Dim uvHigh As Double? = GetNullableDouble(existing, "UVIndexHigh")
                Dim solarHigh As Double? = GetNullableDouble(existing, "SolarRadiationHigh")

                ' Load wind averaging accumulators (proper vector-based for direction, cumulative for speed)
                Dim windDirAccum As New WindAverage With {
                    .SumX = GetNullableDouble(existing, "WindDirSumX").GetValueOrDefault(0.0),
                    .SumY = GetNullableDouble(existing, "WindDirSumY").GetValueOrDefault(0.0),
                    .Count = CInt(GetNullableDouble(existing, "WindDirSampleCount").GetValueOrDefault(0.0))
                }
                Dim windSpeedAccum As New SpeedAverage With {
                    .Total = GetNullableDouble(existing, "WindSpeedTotal").GetValueOrDefault(0.0),
                    .Count = CInt(GetNullableDouble(existing, "WindSpeedCount").GetValueOrDefault(0.0))
                }

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

                ' Proper wind averaging: vector-based for direction, arithmetic for speed
                If windSpeedMph.HasValue AndAlso windSpeedMph.Value > 0 Then
                    windSpeedAccum.AddSample(windSpeedMph.Value)
                End If

                If windDirDeg.HasValue AndAlso windSpeedMph.HasValue AndAlso windSpeedMph.Value > 0 Then
                    ' Only accumulate direction when wind is actually blowing (speed > 0)
                    windDirAccum.AddSample(windDirDeg.Value)
                End If

                ' UV Index high (daily)
                If uvIndex.HasValue Then
                    If Not uvHigh.HasValue OrElse uvIndex.Value > uvHigh.Value Then
                        uvHigh = uvIndex
                        existing("UVIndexHighTime") = obsTimeStr
                    End If
                End If

                ' Solar Radiation high (daily)
                If solarRadiation.HasValue Then
                    If Not solarHigh.HasValue OrElse solarRadiation.Value > solarHigh.Value Then
                        solarHigh = solarRadiation
                        existing("SolarRadiationHighTime") = obsTimeStr
                    End If
                End If

                ' Upsert daily row
                Using cmd As New SQLiteCommand(conn)
                    If isNew Then
                        cmd.CommandText = "INSERT INTO HiLoDaily (ObsDate, TempHigh, TempHighTime, TempLow, TempLowTime, " &
                                          "FeelsLikeHigh, FeelsLikeHighTime, FeelsLikeLow, FeelsLikeLowTime, " &
                                          "HeatIndexHigh, HeatIndexHighTime, WindChillLow, WindChillLowTime, " &
                                          "RainDay, RainMonth, RainYear, WindSpeedHigh, WindSpeedHighTime, " &
                                          "WindSpeedAvg, WindDirAvg, WindSpeedTotal, WindSpeedCount, " &
                                          "WindDirSumX, WindDirSumY, WindDirSampleCount, " &
                                          "UVIndexHigh, UVIndexHighTime, SolarRadiationHigh, SolarRadiationHighTime, LastUpdated) " &
                                          "VALUES (@date, @th, @tht, @tl, @tlt, @flh, @flht, @fll, @fllt, " &
                                          "@hih, @hiht, @wcl, @wclt, @rd, @rm, @ry, @wsh, @wsht, @wsa, @wda, " &
                                          "@wst, @wsc, @wdx, @wdy, @wdsc, @uvh, @uvht, @solh, @solht, @lu)"
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
                                          "WindSpeedTotal=@wst, WindSpeedCount=@wsc, " &
                                          "WindDirSumX=@wdx, WindDirSumY=@wdy, WindDirSampleCount=@wdsc, " &
                                          "UVIndexHigh=@uvh, UVIndexHighTime=@uvht, " &
                                          "SolarRadiationHigh=@solh, SolarRadiationHighTime=@solht, " &
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
                    cmd.Parameters.AddWithValue("@wsa", windSpeedAccum.Average)
                    cmd.Parameters.AddWithValue("@wda", windDirAccum.AverageDegrees)
                    cmd.Parameters.AddWithValue("@wst", windSpeedAccum.Total)
                    cmd.Parameters.AddWithValue("@wsc", windSpeedAccum.Count)
                    cmd.Parameters.AddWithValue("@wdx", windDirAccum.SumX)
                    cmd.Parameters.AddWithValue("@wdy", windDirAccum.SumY)
                    cmd.Parameters.AddWithValue("@wdsc", windDirAccum.Count)
                    cmd.Parameters.AddWithValue("@uvh", CType(If(uvHigh, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@uvht", CType(If(existing.GetValueOrDefault("UVIndexHighTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@solh", CType(If(solarHigh, CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@solht", CType(If(existing.GetValueOrDefault("SolarRadiationHighTime"), CType(DBNull.Value, Object)), Object))
                    cmd.Parameters.AddWithValue("@lu", nowIso)

                    cmd.ExecuteNonQuery()
                End Using

                ' Also update all-time records from this observation
                UpdateAllTimeRecords(conn, obsTimeLocal, tempF, heatIndexF, windChillF, rainDayIn, rainMonthIn, windSpeedMph, windDirDeg, uvIndex, solarRadiation)
            End Using
        Catch ex As Exception
            Log.WriteException(ex, "[HiLoDatabase] Failed to update daily hi/lo")
        End Try
    End Sub

    ''' <summary>
    ''' Update global all-time records (single-row table HiLoAllTime).
    ''' Tracks: record high/low temp, record high heat index, record low wind chill,
    ''' max daily rain, max monthly rain, highest wind speed, highest UV index, and
    ''' highest solar radiation. Wind direction is accumulated as a vector for long-term averaging.
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
        windDirDeg As Double?,
        uvIndex As Double?,
        solarRadiation As Double?)

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
            Dim uvHigh As Double? = GetNullableDouble(existing, "UVIndexHigh")
            Dim solarHigh As Double? = GetNullableDouble(existing, "SolarRadiationHigh")

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

            ' Record max UV index
            If uvIndex.HasValue Then
                If Not uvHigh.HasValue OrElse uvIndex.Value > uvHigh.Value Then
                    uvHigh = uvIndex
                    existing("UVIndexHighTime") = obsTimeStr
                End If
            End If

            ' Record max solar radiation
            If solarRadiation.HasValue Then
                If Not solarHigh.HasValue OrElse solarRadiation.Value > solarHigh.Value Then
                    solarHigh = solarRadiation
                    existing("SolarRadiationHighTime") = obsTimeStr
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
                                           "UVIndexHigh=@uvh, UVIndexHighTime=@uvht, " &
                                           "SolarRadiationHigh=@solh, SolarRadiationHighTime=@solht, " &
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
                cmd.Parameters.AddWithValue("@uvh", CType(If(uvHigh, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@uvht", CType(If(existing.GetValueOrDefault("UVIndexHighTime"), CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@solh", CType(If(solarHigh, CType(DBNull.Value, Object)), Object))
                cmd.Parameters.AddWithValue("@solht", CType(If(existing.GetValueOrDefault("SolarRadiationHighTime"), CType(DBNull.Value, Object)), Object))
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