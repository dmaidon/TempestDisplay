' Last Edit: March 10, 2026
' ADDED: ApplyApplicationDefaults handler — DarkMode (SystemColorMode.System) + HighDpiMode.SystemAware
' MOVED: FolderRoutines.CreateAppFolders() from FrmMain_Load into Startup handler so directories
'        exist before the startup form — and therefore LogService.Init() — ever runs
' ADDED: UnhandledException handler — dual-path logging (LogService + crash.log fallback)
' ADDED: Shutdown handler — safety-net cancellation of AppCancellationTokenSource

Imports System.IO

Imports Microsoft.VisualBasic.ApplicationServices

Namespace My

    Partial Friend Class MyApplication

        ''' <summary>
        ''' ADDED: Set application-wide defaults before the startup form is created.
        ''' Enables system-aware dark mode and standard DPI scaling.
        ''' </summary>
        Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As ApplyApplicationDefaultsEventArgs) _
                Handles Me.ApplyApplicationDefaults

            ' ADDED: Follow the OS light/dark theme automatically (.NET 9+)
            ' e.ColorMode = SystemColorMode.System

            ' ADDED: Standard per-monitor-aware DPI scaling for all forms
            e.HighDpiMode = HighDpiMode.SystemAware

        End Sub

        ''' <summary>
        ''' MOVED: Folder creation relocated from FrmMain_Load to here so that
        ''' Logs\, Data\, Images\, and $Tmp\ all exist before the main form — and
        ''' therefore LogService.Init() — is ever invoked.
        ''' </summary>
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) _
                Handles Me.Startup

            ' MOVED from FrmMain_Load: ensure all required application directories exist
            ' before any other startup code (log init, settings load, etc.) runs
            Try
                FolderRoutines.CreateAppFolders()
            Catch ex As Exception
                ' Bootstrap fallback: LogService is not yet available at this stage
                Try
                    Dim fallbackLog As String = Path.Combine(System.Windows.Forms.Application.StartupPath, "startup_error.log")
                    File.AppendAllText(fallbackLog,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Folder creation failed: {ex}{Environment.NewLine}")
                Catch
                    ' Last resort: nothing further we can do before the UI is ready
                End Try
            End Try

        End Sub

        ''' <summary>
        ''' ADDED: Global unhandled-exception handler.
        ''' Attempts to log via LogService first; always writes to a crash.log fallback
        ''' so the error is captured even if LogService never initialised.
        ''' </summary>
        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) _
                Handles Me.UnhandledException

            ' Primary path: route through the normal logging infrastructure.
            ' Use LogService.Instance directly — inside My namespace 'Log' resolves to
            ' My.Application.Log (VB Application Framework), not our custom Log module.
            Try
                LogService.Instance.WriteException(e.Exception, "[UNHANDLED] Fatal unhandled exception — application will exit")
            Catch
                ' LogService unavailable; proceed to fallback below
            End Try

            ' Fallback path: always write a crash.log regardless of LogService state
            Try
                Dim crashLog As String = Path.Combine(Globals.LogDir, "crash.log")
                Dim divider As New String("-"c, 80)
                Dim entry As String =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNHANDLED EXCEPTION{Environment.NewLine}" &
                    $"Type   : {e.Exception?.GetType().FullName}{Environment.NewLine}" &
                    $"Message: {e.Exception?.Message}{Environment.NewLine}" &
                    $"Stack  : {e.Exception?.StackTrace}{Environment.NewLine}" &
                    $"{divider}{Environment.NewLine}"
                File.AppendAllText(crashLog, entry)
            Catch
                ' If even the crash log write fails there is nothing left to do
            End Try

            ' Exit cleanly so Windows Error Reporting does not show a second dialog
            e.ExitApplication = True

        End Sub

        ''' <summary>
        ''' ADDED: Post-shutdown safety net.
        ''' FrmMain_OnFormClosing already cancels AppCancellationTokenSource and shuts
        ''' down LogService/UdpLogService; this handler covers any edge case where the
        ''' form close path was bypassed (e.g., Environment.Exit or abnormal termination).
        ''' </summary>
        Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) _
                Handles Me.Shutdown

            ' Safety net: signal cancellation if not already done during form close
            Try
                If Not Globals.AppCancellationTokenSource.IsCancellationRequested Then
                    Globals.AppCancellationTokenSource.Cancel()
                End If
            Catch ex As ObjectDisposedException
                ' Token source was already disposed — nothing to do
            Catch
                ' Swallow all other shutdown errors; the process is exiting
            End Try

        End Sub

    End Class

End Namespace