Imports System.Collections.Concurrent
Imports System.IO

''' <summary>
''' Separate logging service specifically for UDP observation packets (obs_st)
''' Creates daily log files in format: udp_MMMdd.log
''' </summary>
Public Class UdpLogService
    Private Shared ReadOnly _instance As New UdpLogService()

    Public Shared ReadOnly Property Instance As UdpLogService
        Get
            Return _instance
        End Get
    End Property

    Private _queue As BlockingCollection(Of String)
    Private _writerTask As Task
    Private _sw As StreamWriter
    Private _initialized As Boolean
    Private ReadOnly _initLock As New Object()

    Private Sub New()
        ' private ctor
    End Sub

    Public Sub Init()
        SyncLock _initLock
            If _initialized Then Return

            ' Create log directory if needed
            If Not Directory.Exists(Globals.LogDir) Then
                Directory.CreateDirectory(Globals.LogDir)
            End If

            ' Set UDP log path with date-based naming (one per day)
            Globals.UdpLog = Path.Combine(Globals.LogDir, $"udp_{Now:MMMdd}.log")

            Try
                ' Open/create UDP log file
                _sw = New StreamWriter(File.Open(Globals.UdpLog, FileMode.Append, FileAccess.Write, FileShare.Read)) With {
                    .AutoFlush = True,
                    .NewLine = vbCrLf
                }

                ' Write header if new file
                If New FileInfo(Globals.UdpLog).Length = 0 Then
                    _sw.WriteLine(CreateUdpLogHeader())
                End If
            Catch ex As Exception
                ' If opening fails, fallback to null writer
                _sw = Nothing
                Log.WriteException(ex, "UdpLogService: Failed to initialize UDP log file")
            End Try

            ' Limit queue capacity to prevent unbounded memory growth
            _queue = New BlockingCollection(Of String)(boundedCapacity:=10000)
            _writerTask = Task.Run(AddressOf ProcessQueueAsync)
            _initialized = True

            Log.Write($"UdpLogService initialized. Log file: {Globals.UdpLog}")
        End SyncLock
    End Sub

    Private Shared Function CreateUdpLogHeader() As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("═══════════════════════════════════════════════════════════════")
        sb.AppendLine("Tempest Display UDP Observation Log (obs_st packets)")
        sb.AppendLine($"Log File: {Globals.UdpLog}")
        sb.AppendLine($"Generated on: {Now:F}")
        sb.AppendLine("═══════════════════════════════════════════════════════════════")
        sb.AppendLine()
        Return sb.ToString()
    End Function

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
    ''' Writes an obs_st packet to the UDP log
    ''' </summary>
    Public Sub WriteObservation(jsonPacket As String)
        If Not _initialized OrElse _queue Is Nothing Then Return

        Try
            Dim timestamped = $"[{Now:yyyy-MM-dd HH:mm:ss.fff}] {jsonPacket}"

            ' Try to add with timeout to avoid blocking indefinitely if queue is full
            If Not _queue.TryAdd(timestamped, millisecondsTimeout:=100) Then
                ' Queue is full, drop the message (best-effort logging)
                Diagnostics.Debug.WriteLine($"UdpLogService queue full, dropped packet")
            End If
        Catch ex As Exception
            ' Best-effort - log error to main log
            Log.WriteException(ex, "UdpLogService.WriteObservation failed")
        End Try
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

                ' Reset initialization flag so Init() can be called again
                _initialized = False
            End Try
        End SyncLock
    End Sub

End Class