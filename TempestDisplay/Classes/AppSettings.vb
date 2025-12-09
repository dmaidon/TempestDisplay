Public Class AppSettings
    Public Property FormX As Integer
    Public Property FormY As Integer

    ' Number of times the app has been started
    Public Property Timesrun As Long

    Public Property LogDays As Integer

    Public Property StationName As String

    Public Property StationID As String

    Public Property StationElevation As Double

    ' MeteoBridge related settings
    Public Property MeteoBridge As MeteoBridgeSettings = New MeteoBridgeSettings()

    Public Property Tempest As TempestSettings = New TempestSettings()

    Public Property RainLimit As RainLimitSettings = RainLimitSettings.CreateDefault()

    Public Shared Function CreateDefault() As AppSettings
        Return New AppSettings With {
            .FormX = 250,
            .FormY = 250,
            .Timesrun = 0,
            .LogDays = 5,
            .StationName = "CarolinaWx",
            .StationID = "146672",
            .StationElevation = 232.0,
            .MeteoBridge = MeteoBridgeSettings.CreateDefault(),
            .Tempest = TempestSettings.CreateDefaultTempest(),
            .RainLimit = RainLimitSettings.CreateDefault()
        }
    End Function

    Public Sub Normalize()
        ' Validate and constrain form position to reasonable values
        If FormX < -100 OrElse FormX > 5000 Then FormX = 250
        If FormY < -100 OrElse FormY > 5000 Then FormY = 250
        If Timesrun < 0 Then Timesrun = 0
        If LogDays < 1 OrElse LogDays > 30 Then LogDays = 5

        ' Ensure child settings objects exist
        If MeteoBridge Is Nothing Then MeteoBridge = MeteoBridgeSettings.CreateDefault()
        If Tempest Is Nothing Then Tempest = TempestSettings.CreateDefaultTempest()
        If RainLimit Is Nothing Then RainLimit = RainLimitSettings.CreateDefault()

        MeteoBridge.Normalize()
        Tempest.Normalize()
        RainLimit.Normalize()
    End Sub

End Class

Public Class RainLimitSettings

    'Public Property Enabled As Boolean = False
    Public Property DailyLimitInches As Single = 2.0F

    Public Property YesterdayLimitInches As Single = 2.0F
    Public Property MonthLimitInches As Single = 15.0F
    Public Property YearLimitInches As Single = 60.0F
    Public Property AllTimeLimitInches As Single = 400.0F

    Public Shared Function CreateDefault() As RainLimitSettings
        Return New RainLimitSettings With {
            .DailyLimitInches = 2.0F,
            .YesterdayLimitInches = 2.0F,
            .MonthLimitInches = 15.0F,
            .YearLimitInches = 60.0F,
            .AllTimeLimitInches = 400.0F
        }
    End Function

    Public Sub Normalize()
        ' Validate rain limits are positive and reasonable (max 1000 inches)
        If DailyLimitInches <= 0 OrElse DailyLimitInches > 1000 Then DailyLimitInches = 2.0F
        If YesterdayLimitInches <= 0 OrElse YesterdayLimitInches > 1000 Then YesterdayLimitInches = 2.0F
        If MonthLimitInches <= 0 OrElse MonthLimitInches > 1000 Then MonthLimitInches = 15.0F
        If YearLimitInches <= 0 OrElse YearLimitInches > 1000 Then YearLimitInches = 60.0F
        If AllTimeLimitInches <= 0 OrElse AllTimeLimitInches > 10000 Then AllTimeLimitInches = 400.0F
    End Sub

End Class

Public Class TempestSettings
    Public Property StationId As String
    Public Property ApiToken As String
    Public Property UpdateIntervalSeconds As Integer = 180

    Public Shared Function CreateDefaultTempest() As TempestSettings
        Return New TempestSettings With {
            .StationId = "146672",
            .ApiToken = "a76ccf85-7164-4cd2-b2f2-75159d9a9d20",
            .UpdateIntervalSeconds = 180
        }
    End Function

    Public Sub Normalize()
        ' Validate and clean station ID and API token
        If StationId Is Nothing OrElse StationId.Trim().Length = 0 Then
            StationId = "146672"
        Else
            StationId = StationId.Trim()
        End If

        If ApiToken Is Nothing OrElse ApiToken.Trim().Length = 0 Then
            ApiToken = "a76ccf85-7164-4cd2-b2f2-75159d9a9d20"
        Else
            ApiToken = ApiToken.Trim()
        End If

        ' Validate update interval: minimum 180 seconds (3 minutes), maximum 86400 (24 hours)
        If UpdateIntervalSeconds < 180 OrElse UpdateIntervalSeconds > 86400 Then
            UpdateIntervalSeconds = 180
        End If
    End Sub

End Class

Public Class MeteoBridgeSettings
    Public Property Login As String
    Public Property Password As String
    Public Property IpAddress As String

    Public Shared Function CreateDefault() As MeteoBridgeSettings
        Return New MeteoBridgeSettings With {
            .Login = "meteobridge",
            .Password = "meteobridge",
            .IpAddress = "192.168.68.87"
        }
    End Function

    Public Sub Normalize()
        ' Validate and clean MeteoBridge credentials
        If Login Is Nothing OrElse Login.Trim().Length = 0 Then
            Login = "meteobridge"
        Else
            Login = Login.Trim()
        End If

        If Password Is Nothing Then
            Password = "meteobridge"
        Else
            Password = Password.Trim()
        End If

        If IpAddress Is Nothing OrElse IpAddress.Trim().Length = 0 Then
            IpAddress = "192.168.68.87"
        Else
            IpAddress = IpAddress.Trim()
            ' Basic IP address validation (simple check for valid format)
            If Not System.Text.RegularExpressions.Regex.IsMatch(IpAddress, "^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$") Then
                IpAddress = "192.168.68.87"
            End If
        End If
    End Sub

End Class