' Last Edit: February 17, 2026 (Reduce logging overhead, avoid queue writes after shutdown, optimize init wait)
Imports System.Collections.Concurrent
Imports System.IO
Imports System.Text

Public Enum LogLevel
    Debug = 0
    Info = 1
    Warning = 2
    [Error] = 3
    Critical = 4
End Enum

Public Class LogService
    Private Shared ReadOnly _instance As New LogService()

    Public Shared ReadOnly Property Instance As LogService
        Get
            Return _instance
        End Get
    End Property

    Private _queue As BlockingCollection(Of String)
    Private _writerTask As Task
    Private _sw As StreamWriter
    Private _swError As StreamWriter ' Separate error log file
    Private _initialized As Boolean
    Private ReadOnly _initLock As New Object()
    Private _minLogLevel As LogLevel = LogLevel.Info ' Default minimum log level

    ' Event to notify when error count changes
    Public Event ErrorCountChanged As EventHandler

    Private Sub New()
        ' private ctor
    End Sub

    ''' <summary>
    ''' Raises the ErrorCountChanged event to notify UI
    ''' </summary>
    Private Sub RaiseErrorCountChanged()
        Try
            RaiseEvent ErrorCountChanged(Me, EventArgs.Empty)
        Catch
            ' Ignore event raising errors
        End Try
    End Sub

    ''' <summary>
    ''' Public method to trigger error count display update (e.g., after midnight reset)
    ''' </summary>
    Public Sub TriggerErrorCountUpdate()
        RaiseErrorCountChanged()
    End Sub

    Public Sub Init()
        Threading.SpinWait.SpinUntil(Function() Globals.AppStarts > 0, 2000)
        SyncLock _initLock
            If _initialized Then Return

            ' Use LogRoutines for consistent file naming
            LogFile = LogRoutines.GetLogPath(LogRoutines.CreateLogFileName("td", includeAppStarts:=True))
            UdpLog = LogRoutines.GetLogPath(LogRoutines.CreateLogFileName("udp", includeAppStarts:=False))
            Dim errorLogFile = LogRoutines.GetLogPath(LogRoutines.CreateLogFileName("err", includeAppStarts:=True))

            Dim logPath = If(String.IsNullOrWhiteSpace(Globals.LogFile), LogFile, Globals.LogFile)

            ' Use LogRoutines for directory management
            Dim logDir = Path.GetDirectoryName(logPath)
            If Not String.IsNullOrEmpty(logDir) Then
                LogRoutines.EnsureDirectoryExists(logDir)
            End If

            ' Check if this is a new file (doesn't exist or is empty)
            Dim isNewFile As Boolean = Not File.Exists(logPath) OrElse New FileInfo(logPath).Length = 0
            Dim isNewErrorFile As Boolean = Not File.Exists(errorLogFile) OrElse New FileInfo(errorLogFile).Length = 0

            Try
                _sw = New StreamWriter(File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.Read)) With {.AutoFlush = True, .NewLine = vbCrLf}

                ' Write header to new files using LogRoutines
                If isNewFile AndAlso _sw IsNot Nothing Then
                    Dim header = LogRoutines.CreateLogHeader("Tempest Display Log File", logPath)
                    _sw.Write(header)
                    _sw.Flush()
                End If
            Catch ex As Exception
                ' If opening fails, fallback to null writer by leaving _sw as Nothing
                _sw = Nothing
            End Try

            ' Initialize error log file
            Try
                _swError = New StreamWriter(File.Open(errorLogFile, FileMode.Append, FileAccess.Write, FileShare.Read)) With {.AutoFlush = True, .NewLine = vbCrLf}

                ' Write header to new error log files using LogRoutines
                If isNewErrorFile AndAlso _swError IsNot Nothing Then
                    Dim errorHeader = LogRoutines.CreateLogHeader("Tempest Display Error Log File", errorLogFile)
                    _swError.Write(errorHeader)
                    _swError.Flush()
                End If
            Catch ex As Exception
                ' If opening fails, fallback to null writer by leaving _swError as Nothing
                _swError = Nothing
            End Try

            ' Limit queue capacity to prevent unbounded memory growth under heavy logging
            _queue = New BlockingCollection(Of String)(boundedCapacity:=10000)
            _writerTask = Task.Run(AddressOf ProcessQueueAsync)
            _initialized = True

            WriteLine("LogService initialized.")
        End SyncLock
    End Sub

    Private Sub ProcessQueueAsync()
        Dim writer = _sw
        Try
            For Each line In _queue.GetConsumingEnumerable()
                Try
                    writer?.WriteLine(line)
                Catch
                    ' swallow write exceptions to avoid crashing background task
                End Try
            Next
        Catch
            ' ignore
        Finally
            Try
                If writer IsNot Nothing Then
                    writer.Flush()
                    writer.Dispose()
                End If
            Catch
            End Try
        End Try
    End Sub

    ''' <summary>
    ''' Sets the minimum log level. Messages below this level will be ignored.
    ''' </summary>
    Public Sub SetMinimumLogLevel(level As LogLevel)
        _minLogLevel = level
    End Sub

    ''' <summary>
    ''' Writes a log message with the specified level
    ''' </summary>
    Public Sub WriteLog(level As LogLevel, message As String)
        If Not _initialized OrElse _queue Is Nothing OrElse _queue.IsAddingCompleted Then Return
        If level < _minLogLevel Then Return

        Try
            ' Thread-safe initialization check
            ' REMOVE auto-init here; Init must be called explicitly from FrmMain_Load
            'If Not _initialized Then
            '    SyncLock _initLock
            '        If Not _initialized Then Init()
            '    End SyncLock
            'End If

            Dim levelStr = GetLevelString(level)
            Dim timestamped = $"[{Now:yyyy-MM-dd HH:mm:ss.fff}] [{levelStr}] {message}"

            ' Try to add with timeout to avoid blocking indefinitely if queue is full
            If Not _queue.TryAdd(timestamped, millisecondsTimeout:=100) Then
                ' Queue is full, drop the message (best-effort logging)
                Diagnostics.Debug.WriteLine($"LogService queue full, dropped message: {message}")
            End If
        Catch
            ' Best-effort - ignore
        End Try
    End Sub

    Private Shared Function GetLevelString(level As LogLevel) As String
        Select Case level
            Case LogLevel.Debug
                Return "DEBUG"
            Case LogLevel.Info
                Return "INFO "
            Case LogLevel.Warning
                Return "WARN "
            Case LogLevel.Error
                Return "ERROR"
            Case LogLevel.Critical
                Return "CRIT "
            Case Else
                Return "UNKN "
        End Select
    End Function

    ' Convenience methods for different log levels
    Public Sub Debug(message As String)
        WriteLog(LogLevel.Debug, message)
    End Sub

    Public Sub Info(message As String)
        WriteLog(LogLevel.Info, message)
    End Sub

    Public Sub Warning(message As String)
        WriteLog(LogLevel.Warning, message)
    End Sub

    Public Sub [Error](message As String)
        WriteLog(LogLevel.Error, message)
    End Sub

    Public Sub Critical(message As String)
        WriteLog(LogLevel.Critical, message)
    End Sub

    ' Legacy methods for backward compatibility
    Public Sub WriteLine(message As String)
        WriteLog(LogLevel.Info, message)
    End Sub

    Public Sub Write(message As String)
        WriteLog(LogLevel.Info, message)
    End Sub

    Public Sub WriteException(ex As Exception, Optional context As String = "")
        If ex Is Nothing Then Return
        Dim sb As New StringBuilder()
        If Not String.IsNullOrEmpty(context) Then
            sb.AppendLine(context)
        End If
        sb.AppendLine(ex.ToString())

        ' Write to error log file
        Try
            If _swError IsNot Nothing Then
                SyncLock _swError
                    Dim timestamped = $"[{Now:yyyy-MM-dd HH:mm:ss.fff}] {sb}"
                    _swError.WriteLine(timestamped)
                End SyncLock

                ' Increment error count
                Threading.Interlocked.Increment(Globals.ErrCount)

                ' Notify UI to update error count display
                RaiseErrorCountChanged()
            End If
        Catch
            ' Swallow write exceptions
        End Try

        ' Also write to main log
        WriteLog(LogLevel.Error, sb.ToString())
    End Sub

    Public Sub Shutdown()
        SyncLock _initLock
            Try
                If _queue IsNot Nothing Then
                    _queue.CompleteAdding()
                    If _writerTask IsNot Nothing Then
                        Try
                            _writerTask.Wait(5000)
                        Catch
                        End Try
                    End If
                End If
            Catch
            Finally
                Try
                    If _sw IsNot Nothing Then
                        SyncLock _sw
                            _sw.Flush()
                            _sw.Dispose()
                        End SyncLock
                    End If
                Catch
                End Try

                Try
                    If _swError IsNot Nothing Then
                        SyncLock _swError
                            _swError.Flush()
                            _swError.Dispose()
                        End SyncLock
                    End If
                Catch
                End Try

                ' Reset initialization flag so Init() can be called again
                _initialized = False
            End Try
        End SyncLock
    End Sub

End Class