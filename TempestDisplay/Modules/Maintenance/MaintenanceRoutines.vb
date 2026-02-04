' Last Edit: January 15, 2026 (Fixed typos, added recursive cleanup, file lock handling, disk space reporting, dry-run mode)
Imports System.IO

Friend Module MaintenanceRoutines

    ' File size constants for better readability
    Private Const BytesPerKB As Long = 1024
    Private Const BytesPerMB As Long = 1048576
    Private Const BytesPerGB As Long = 1073741824

    ' Retry configuration for locked files
    Private Const MaxRetryAttempts As Integer = 3
    Private Const RetryDelayMs As Integer = 500

    ''' <summary>
    ''' Perform log file maintenance by cleaning old files from log and temp directories.
    ''' </summary>
    Friend Sub PerformLogMaintenance(Optional dryRun As Boolean = False)
        Try
            ' Load settings to get LogDays cutoff value
            Dim settings = SettingsRoutines.LoadSettings()
            If settings Is Nothing Then
                Log.Write("PerformLogMaintenance: Unable to load settings, skipping maintenance")
                Return
            End If

            Dim cutoffDays As Integer = settings.LogDays
            If cutoffDays <= 0 Then
                Log.Write($"PerformLogMaintenance: LogDays is {cutoffDays}, skipping maintenance")
                Return
            End If

            Dim cutoffDate As DateTime = DateTime.Now.AddDays(-cutoffDays)
            Dim mode As String = If(dryRun, "[DRY RUN] ", "")
            Log.Write($"PerformLogMaintenance: {mode}Deleting files older than {cutoffDate:yyyy-MM-dd HH:mm:ss} (cutoff: {cutoffDays} days)")

            Dim totalDeletedCount As Integer = 0
            Dim totalDeletedSize As Long = 0
            Dim totalFailedCount As Integer = 0

            ' Report disk space before cleanup
            ReportDiskSpace("Before cleanup")

            ' Clean LogDir
            If Directory.Exists(Globals.LogDir) Then
                Dim logResults = CleanDirectory(Globals.LogDir, cutoffDate, "LogDir", dryRun, recursive:=True)
                totalDeletedCount += logResults.DeletedCount
                totalDeletedSize += logResults.DeletedSize
                totalFailedCount += logResults.FailedCount
            Else
                Log.Write($"PerformLogMaintenance: LogDir does not exist: {Globals.LogDir}")
            End If

            ' Clean TempDir
            If Directory.Exists(Globals.TempDir) Then
                Dim tempResults = CleanDirectory(Globals.TempDir, cutoffDate, "TempDir", dryRun, recursive:=True)
                totalDeletedCount += tempResults.DeletedCount
                totalDeletedSize += tempResults.DeletedSize
                totalFailedCount += tempResults.FailedCount
            Else
                Log.Write($"PerformLogMaintenance: TempDir does not exist: {Globals.TempDir}")
            End If

            ' Report disk space after cleanup (only if not dry run)
            If Not dryRun Then
                ReportDiskSpace("After cleanup")
            End If

            ' Summary
            Log.Write($"PerformLogMaintenance: {mode}Maintenance complete - " &
                     $"Deleted: {totalDeletedCount} files ({FormatFileSize(totalDeletedSize)}), " &
                     $"Failed: {totalFailedCount}")

        Catch ex As Exception
            Log.WriteException(ex, "PerformLogMaintenance: Error during maintenance")
        End Try
    End Sub

    ''' <summary>
    ''' Result structure for directory cleaning operations.
    ''' </summary>
    Private Structure CleanResult
        Public DeletedCount As Integer
        Public DeletedSize As Long
        Public FailedCount As Integer
    End Structure

    ''' <summary>
    ''' Clean a directory by deleting files older than the cutoff date.
    ''' </summary>
    Private Function CleanDirectory(directoryPath As String, cutoffDate As DateTime, dirName As String,
                                    dryRun As Boolean, Optional recursive As Boolean = False) As CleanResult
        Dim result As New CleanResult()

        Try
            ' Get files in current directory
            Dim files = Directory.GetFiles(directoryPath)

            For Each filePath In files
                Try
                    Dim fileInfo As New FileInfo(filePath)

                    ' Use CreationTime (more reliable for logs than LastWriteTime)
                    If fileInfo.CreationTime < cutoffDate Then
                        Dim fileName As String = fileInfo.Name
                        Dim fileSize As Long = fileInfo.Length

                        If dryRun Then
                            ' Dry run: just log what would be deleted
                            Log.Write($"PerformLogMaintenance: [DRY RUN] Would delete from {dirName}: {fileName} " &
                                    $"(Size: {FormatFileSize(fileSize)}, Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss})")
                            result.DeletedCount += 1
                            result.DeletedSize += fileSize
                        Else
                            ' Attempt to delete with retry logic for locked files
                            If TryDeleteFileWithRetry(filePath) Then
                                result.DeletedCount += 1
                                result.DeletedSize += fileSize
                                Log.Write($"PerformLogMaintenance: Deleted from {dirName}: {fileName} " &
                                        $"(Size: {FormatFileSize(fileSize)}, Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss})")
                            Else
                                result.FailedCount += 1
                                Log.Write($"PerformLogMaintenance: Failed to delete (file locked): {filePath}")
                            End If
                        End If
                    End If
                Catch ex As Exception
                    result.FailedCount += 1
                    Log.WriteException(ex, $"PerformLogMaintenance: Error processing file {filePath}")
                End Try
            Next

            ' Process subdirectories recursively if requested
            If recursive Then
                Try
                    Dim subdirs = Directory.GetDirectories(directoryPath)
                    For Each subdir In subdirs
                        Dim subdirName As String = Path.GetFileName(subdir)
                        Dim subResult = CleanDirectory(subdir, cutoffDate, $"{dirName}/{subdirName}", dryRun, recursive:=True)
                        result.DeletedCount += subResult.DeletedCount
                        result.DeletedSize += subResult.DeletedSize
                        result.FailedCount += subResult.FailedCount
                    Next
                Catch ex As Exception
                    Log.WriteException(ex, $"PerformLogMaintenance: Error processing subdirectories in {directoryPath}")
                End Try
            End If

            If result.DeletedCount > 0 OrElse result.FailedCount > 0 Then
                Dim modeStr As String = If(dryRun, "[DRY RUN] ", "")
                Log.Write($"PerformLogMaintenance: {modeStr}{dirName} cleanup - " &
                         $"Deleted: {result.DeletedCount} files ({FormatFileSize(result.DeletedSize)}), " &
                         $"Failed: {result.FailedCount}")
            Else
                Log.Write($"PerformLogMaintenance: {dirName} cleanup - No files to delete")
            End If

        Catch ex As Exception
            Log.WriteException(ex, $"PerformLogMaintenance: Error cleaning directory {directoryPath}")
        End Try

        Return result
    End Function

    ''' <summary>
    ''' Attempt to delete a file with retry logic for handling locked files.
    ''' </summary>
    Private Function TryDeleteFileWithRetry(filePath As String) As Boolean
        For attempt As Integer = 1 To MaxRetryAttempts
            Try
                File.Delete(filePath)
                Return True
            Catch ex As IOException When attempt < MaxRetryAttempts
                ' File might be locked, wait and retry
                Threading.Thread.Sleep(RetryDelayMs)
            Catch ex As Exception
                ' Other exceptions, don't retry
                Return False
            End Try
        Next
        Return False
    End Function

    ''' <summary>
    ''' Report available disk space for the drive containing the data directories.
    ''' </summary>
    Private Sub ReportDiskSpace(label As String)
        Try
            Dim drive As New DriveInfo(Path.GetPathRoot(Globals.DataDir))
            If drive.IsReady Then
                Dim availableGB As Double = drive.AvailableFreeSpace / BytesPerGB
                Dim totalGB As Double = drive.TotalSize / BytesPerGB
                Dim usedPercent As Double = ((drive.TotalSize - drive.AvailableFreeSpace) / CDbl(drive.TotalSize)) * 100.0

                Log.Write($"PerformLogMaintenance: Disk Space {label} - " &
                         $"Available: {availableGB:F2} GB / {totalGB:F2} GB " &
                         $"(Used: {usedPercent:F1}%)")
            End If
        Catch ex As Exception
            ' Don't fail maintenance if disk space reporting fails
            Log.WriteException(ex, "PerformLogMaintenance: Error reporting disk space")
        End Try
    End Sub

    ''' <summary>
    ''' Format file size in human-readable format (B, KB, MB, GB).
    ''' </summary>
    Private Function FormatFileSize(bytes As Long) As String
        If bytes < BytesPerKB Then
            Return $"{bytes} B"
        ElseIf bytes < BytesPerMB Then
            Return $"{bytes / CDbl(BytesPerKB):F2} KB"
        ElseIf bytes < BytesPerGB Then
            Return $"{bytes / CDbl(BytesPerMB):F2} MB"
        Else
            Return $"{bytes / CDbl(BytesPerGB):F2} GB"
        End If
    End Function

End Module
