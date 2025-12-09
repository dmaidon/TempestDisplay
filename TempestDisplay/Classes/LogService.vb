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

    Private Sub New()
        ' private ctor
    End Sub

    Public Sub Init()
        Dim waitMs As Integer = 0
        While Globals.AppStarts <= 0 AndAlso waitMs < 2000
            Threading.Thread.Sleep(50)
            waitMs += 50
        End While
        SyncLock _initLock
            If _initialized Then Return
            LogFile = Path.Combine(Globals.LogDir, $"td_{Now:MMMdd}-{Globals.AppStarts}.log")
            UdpLog = Path.Combine(Globals.LogDir, $"udp_{Now:MMMdd}.log")
            Dim errorLogFile = Path.Combine(Globals.LogDir, $"err_{Now:MMMdd}-{Globals.AppStarts}.log")

            Dim logPath = If(String.IsNullOrWhiteSpace(Globals.LogFile), LogFile, Globals.LogFile)

            Dim logDir = Path.GetDirectoryName(logPath)
            If Not String.IsNullOrEmpty(logDir) AndAlso Not Directory.Exists(logDir) Then
                Directory.CreateDirectory(logDir)
            End If

            ' Check if this is a new file (doesn't exist or is empty)
            Dim isNewFile As Boolean = Not File.Exists(logPath) OrElse New FileInfo(logPath).Length = 0
            Dim isNewErrorFile As Boolean = Not File.Exists(errorLogFile) OrElse New FileInfo(errorLogFile).Length = 0

            Try
                _sw = New StreamWriter(File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.Read)) With {.AutoFlush = True, .NewLine = vbCrLf}

                ' Write header to new files
                If isNewFile AndAlso _sw IsNot Nothing Then
                    Dim header = CreateLogHeader()
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

                ' Write header to new error log files
                If isNewErrorFile AndAlso _swError IsNot Nothing Then
                    Dim errorHeader = CreateErrorLogHeader()
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
        Try
            For Each line In _queue.GetConsumingEnumerable()
                Try
                    If _sw IsNot Nothing Then
                        SyncLock _sw
                            _sw.WriteLine(line)
                        End SyncLock
                    End If
                Catch
                    ' swallow write exceptions to avoid crashing background task
                End Try
            Next
        Catch
            ' ignore
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
        If Not _initialized OrElse _queue Is Nothing Then Return
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

    Private Shared Function CreateLogHeader() As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("Tempest Display Log File")
        sb.AppendLine($"LogFile: {Globals.LogFile}")
        sb.AppendLine($"Generated on: {Now:F}")
        sb.AppendLine($"Application Version: {Application.ProductVersion}")
        sb.AppendLine(New String("-"c, 50))
        Return sb.ToString()
    End Function

    Private Shared Function CreateErrorLogHeader() As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("Tempest Display Error Log File")
        sb.AppendLine($"LogFile: {Path.Combine(Globals.LogDir, $"err_{Now:MMMdd}-{Globals.AppStarts}.log")}")
        sb.AppendLine($"Generated on: {Now:F}")
        sb.AppendLine($"Application Version: {Application.ProductVersion}")
        sb.AppendLine(New String("-"c, 50))
        Return sb.ToString()
    End Function

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