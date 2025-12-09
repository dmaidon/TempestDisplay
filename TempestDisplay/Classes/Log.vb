Public Module Log

    Public Sub Write(message As String)
        Try
            LogService.Instance.Write(message)
        Catch
        End Try
    End Sub

    Public Sub WriteLine(message As String)
        Try
            LogService.Instance.WriteLine(message)
        Catch
        End Try
    End Sub

    Public Sub WriteException(ex As Exception, Optional context As String = "")
        Try
            LogService.Instance.WriteException(ex, context)
        Catch
        End Try
    End Sub

    Public Sub Init()
        Try
            LogService.Instance.Init()
        Catch
        End Try
    End Sub

    Public Sub Shutdown()
        Try
            LogService.Instance.Shutdown()
        Catch
        End Try
    End Sub

End Module