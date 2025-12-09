Imports System.IO

Friend Module MaintenanceRoutines

    Friend Sub PerformLogMaintenance()
        Try
            ' Load settings to get LogDays cutoff value
            Dim settings = SettingsRoutines.LoadSettings()
            If settings Is Nothing Then
                Log.Write("PerformLogMaintenence: Unable to load settings, skipping maintenance")
                Return
            End If

            Dim cutoffDays As Integer = settings.LogDays
            If cutoffDays <= 0 Then
                Log.Write($"PerformLogMaintenence: LogDays is {cutoffDays}, skipping maintenance")
                Return
            End If

            Dim cutoffDate As DateTime = DateTime.Now.AddDays(-cutoffDays)
            Log.Write($"PerformLogMaintenence: Deleting files older than {cutoffDate:yyyy-MM-dd HH:mm:ss} (cutoff: {cutoffDays} days)")

            ' Clean LogDir
            If Directory.Exists(Globals.LogDir) Then
                CleanDirectory(Globals.LogDir, cutoffDate, "LogDir")
            Else
                Log.Write($"PerformLogMaintenence: LogDir does not exist: {Globals.LogDir}")
            End If

            ' Clean TempDir
            If Directory.Exists(Globals.TempDir) Then
                CleanDirectory(Globals.TempDir, cutoffDate, "TempDir")
            Else
                Log.Write($"PerformLogMaintenence: TempDir does not exist: {Globals.TempDir}")
            End If

            Log.Write("PerformLogMaintenence: Maintenance complete")
        Catch ex As Exception
            Log.WriteException(ex, "PerformLogMaintenence: Error during maintenance")
        End Try
    End Sub

    Private Sub CleanDirectory(directoryPath As String, cutoffDate As DateTime, dirName As String)
        Try
            Dim files = Directory.GetFiles(directoryPath)
            Dim deletedCount As Integer = 0
            Dim totalSize As Long = 0

            For Each filePath In files
                Try
                    Dim fileInfo As New FileInfo(filePath)

                    ' Check if file is older than cutoff date
                    If fileInfo.LastWriteTime < cutoffDate Then
                        Dim fileName As String = fileInfo.Name
                        Dim fileSize As Long = fileInfo.Length

                        ' Delete the file
                        File.Delete(filePath)

                        deletedCount += 1
                        totalSize += fileSize

                        Log.Write($"PerformLogMaintenence: Deleted from {dirName}: {fileName} (Size: {FormatFileSize(fileSize)}, Last Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss})")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, $"PerformLogMaintenence: Error deleting file {filePath}")
                End Try
            Next

            If deletedCount > 0 Then
                Log.Write($"PerformLogMaintenence: {dirName} cleanup complete - Deleted {deletedCount} file(s), Total size: {FormatFileSize(totalSize)}")
            Else
                Log.Write($"PerformLogMaintenence: {dirName} cleanup complete - No files to delete")
            End If
        Catch ex As Exception
            Log.WriteException(ex, $"PerformLogMaintenence: Error cleaning directory {directoryPath}")
        End Try
    End Sub

    Private Function FormatFileSize(bytes As Long) As String
        If bytes < 1024 Then
            Return $"{bytes} B"
        ElseIf bytes < 1048576 Then
            Return $"{bytes / 1024.0:F2} KB"
        ElseIf bytes < 1073741824 Then
            Return $"{bytes / 1048576.0:F2} MB"
        Else
            Return $"{bytes / 1073741824.0:F2} GB"
        End If
    End Function

End Module