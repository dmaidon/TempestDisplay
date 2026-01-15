Imports System.IO
Imports System.Threading

''' <summary>
''' Global application state and configuration
''' NOTE: This module contains mutable shared state. Access from multiple threads requires synchronization.
''' </summary>
Friend Module Globals

#Region "Directory Paths (Immutable)"

    ''' <summary>
    ''' Directory containing application image resources
    ''' </summary>
    Friend ReadOnly ImgDir As String = Path.Combine(Application.StartupPath, "Images")

    ''' <summary>
    ''' Directory for temporary files
    ''' </summary>
    Friend ReadOnly TempDir As String = Path.Combine(Application.StartupPath, "$Tmp")

    ''' <summary>
    ''' Directory for application data files (battery history, databases, etc.)
    ''' </summary>
    Friend ReadOnly DataDir As String = Path.Combine(Application.StartupPath, "Data")

    ''' <summary>
    ''' Directory for log files
    ''' </summary>
    Friend ReadOnly LogDir As String = Path.Combine(Application.StartupPath, "Logs")

#End Region

#Region "File Paths (Set at Runtime)"

    ''' <summary>
    ''' Path to the main application log file (set by LogService.Init)
    ''' </summary>
    Friend LogFile As String

    ''' <summary>
    ''' Path to the UDP observation log file (set by UdpLogService.Init)
    ''' </summary>
    Friend UdpLog As String

    ''' <summary>
    ''' Path to the battery history JSON file (set by LogRoutines.InitializeBatteryHistoryFile)
    ''' </summary>
    Friend BatteryHistoryFile As String

#End Region

#Region "Application State"

    ''' <summary>
    ''' Number of times the application has started (mirrors AppSettings.Timesrun)
    ''' Used for log file naming to ensure unique logs per session
    ''' </summary>
    Friend AppStarts As Long = 0

    ''' <summary>
    ''' Count of errors logged to error log file since application start
    ''' Thread-safe: Use Interlocked.Increment when modifying
    ''' </summary>
    Friend ErrCount As Integer = 0

#End Region

#Region "Cancellation Token (Graceful Shutdown)"

    ''' <summary>
    ''' Global cancellation token source for graceful application shutdown
    ''' Call Cancel() to signal all async operations to stop
    ''' </summary>
    Friend ReadOnly AppCancellationTokenSource As New CancellationTokenSource()

    ''' <summary>
    ''' Global cancellation token for graceful application shutdown
    ''' Pass this to async operations that should respond to shutdown requests
    ''' </summary>
    Friend ReadOnly Property AppCancellationToken As CancellationToken
        Get
            Return AppCancellationTokenSource.Token
        End Get
    End Property

#End Region

#Region "Health Check Tracking"

    ''' <summary>
    ''' Timestamp of the last successful data fetch from Tempest API
    ''' Used for health monitoring and stale data detection
    ''' </summary>
    Friend LastSuccessfulTempestFetch As DateTime = DateTime.MinValue

    ''' <summary>
    ''' Timestamp of the last successful data fetch from MeteoBridge
    ''' Used for health monitoring and stale data detection
    ''' </summary>
    Friend LastSuccessfulMeteoBridgeFetch As DateTime = DateTime.MinValue

    ''' <summary>
    ''' Count of consecutive failures fetching from Tempest API
    ''' Reset to 0 on successful fetch
    ''' </summary>
    Friend TempestFetchFailureCount As Integer = 0

    ''' <summary>
    ''' Count of consecutive failures fetching from MeteoBridge
    ''' Reset to 0 on successful fetch
    ''' </summary>
    Friend MeteoBridgeFetchFailureCount As Integer = 0

#End Region

#Region "Application Constants"

    ''' <summary>
    ''' Copyright notice displayed in the application
    ''' </summary>
    Friend ReadOnly Property Cpy As String
        Get
            Return $"© 2025 - {Date.Now.Year} CarolinaWx - All rights reserved."
        End Get
    End Property

    ''' <summary>
    ''' Station latitude (Raleigh, NC area)
    ''' Fixed location - weather station doesn't move
    ''' </summary>
    Friend Lat As Double = 35.625556

    ''' <summary>
    ''' Station longitude (Raleigh, NC area)
    ''' Fixed location - weather station doesn't move
    ''' </summary>
    Friend Lng As Double = -78.328611

    ''' <summary>
    ''' Station timezone (Eastern Time)
    ''' Fixed to match station location
    ''' </summary>
    Friend ReadOnly TimeZone As String = "America/New_York"

#End Region

#Region "Helper Methods"

    ''' <summary>
    ''' Resets health check tracking for Tempest API
    ''' </summary>
    Friend Sub ResetTempestHealth()
        LastSuccessfulTempestFetch = DateTime.Now
        TempestFetchFailureCount = 0
    End Sub

    ''' <summary>
    ''' Resets health check tracking for MeteoBridge
    ''' </summary>
    Friend Sub ResetMeteoBridgeHealth()
        LastSuccessfulMeteoBridgeFetch = DateTime.Now
        MeteoBridgeFetchFailureCount = 0
    End Sub

    ''' <summary>
    ''' Increments Tempest fetch failure count and returns the new count
    ''' Thread-safe
    ''' </summary>
    Friend Function IncrementTempestFailure() As Integer
        Return Threading.Interlocked.Increment(TempestFetchFailureCount)
    End Function

    ''' <summary>
    ''' Increments MeteoBridge fetch failure count and returns the new count
    ''' Thread-safe
    ''' </summary>
    Friend Function IncrementMeteoBridgeFailure() As Integer
        Return Threading.Interlocked.Increment(MeteoBridgeFetchFailureCount)
    End Function

    ''' <summary>
    ''' Gets the time since last successful Tempest fetch
    ''' </summary>
    Friend Function GetTempestDataAge() As TimeSpan
        If LastSuccessfulTempestFetch = DateTime.MinValue Then
            Return TimeSpan.MaxValue
        End If
        Return DateTime.Now - LastSuccessfulTempestFetch
    End Function

    ''' <summary>
    ''' Gets the time since last successful MeteoBridge fetch
    ''' </summary>
    Friend Function GetMeteoBridgeDataAge() As TimeSpan
        If LastSuccessfulMeteoBridgeFetch = DateTime.MinValue Then
            Return TimeSpan.MaxValue
        End If
        Return DateTime.Now - LastSuccessfulMeteoBridgeFetch
    End Function

    ''' <summary>
    ''' Determines if Tempest data is stale based on threshold
    ''' </summary>
    ''' <param name="staleThreshold">TimeSpan threshold for considering data stale</param>
    Friend Function IsTempestDataStale(staleThreshold As TimeSpan) As Boolean
        Return GetTempestDataAge() > staleThreshold
    End Function

    ''' <summary>
    ''' Determines if MeteoBridge data is stale based on threshold
    ''' </summary>
    ''' <param name="staleThreshold">TimeSpan threshold for considering data stale</param>
    Friend Function IsMeteoBridgeDataStale(staleThreshold As TimeSpan) As Boolean
        Return GetMeteoBridgeDataAge() > staleThreshold
    End Function

    ''' <summary>
    ''' Resets all health tracking to initial state
    ''' </summary>
    Friend Sub ResetAllHealthTracking()
        LastSuccessfulTempestFetch = DateTime.MinValue
        LastSuccessfulMeteoBridgeFetch = DateTime.MinValue
        TempestFetchFailureCount = 0
        MeteoBridgeFetchFailureCount = 0
    End Sub

    ''' <summary>
    ''' Gets health status information as a formatted string for diagnostics
    ''' </summary>
    Friend Function GetHealthStatusSummary() As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("=== Health Status ===")
        sb.AppendLine($"Tempest Last Fetch: {If(LastSuccessfulTempestFetch = DateTime.MinValue, "Never", LastSuccessfulTempestFetch.ToString("yyyy-MM-dd HH:mm:ss"))}")
        sb.AppendLine($"Tempest Data Age: {If(LastSuccessfulTempestFetch = DateTime.MinValue, "N/A", GetTempestDataAge().ToString())}")
        sb.AppendLine($"Tempest Failures: {TempestFetchFailureCount}")
        sb.AppendLine($"MeteoBridge Last Fetch: {If(LastSuccessfulMeteoBridgeFetch = DateTime.MinValue, "Never", LastSuccessfulMeteoBridgeFetch.ToString("yyyy-MM-dd HH:mm:ss"))}")
        sb.AppendLine($"MeteoBridge Data Age: {If(LastSuccessfulMeteoBridgeFetch = DateTime.MinValue, "N/A", GetMeteoBridgeDataAge().ToString())}")
        sb.AppendLine($"MeteoBridge Failures: {MeteoBridgeFetchFailureCount}")
        sb.AppendLine($"Error Count: {ErrCount}")
        Return sb.ToString()
    End Function

#End Region

End Module