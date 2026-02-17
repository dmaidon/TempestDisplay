' Last Edit: February 17, 2026 (Avoid handle creation during marshaling and skip disposed controls)
Public NotInheritable Class UIService

    Private Sub New()
    End Sub

    Public Shared Sub SafeInvoke(control As Control, action As Action)
        If control Is Nothing OrElse action Is Nothing Then Return

        If control.IsDisposed OrElse control.Disposing Then Return

        If control.InvokeRequired Then
            If Not control.IsHandleCreated Then Return
            Try
                control.Invoke(action)
            Catch ex As ObjectDisposedException
                ' Control was disposed during invoke - safe to ignore
            Catch ex As InvalidOperationException
                ' Handle creation failed or control is disposing - safe to ignore
            End Try
        Else
            If Not control.IsHandleCreated Then Return
            Try
                action()
            Catch ex As ObjectDisposedException
                ' Control was disposed - safe to ignore
            Catch ex As InvalidOperationException
                ' Handle creation failed - safe to ignore
            End Try
        End If
    End Sub

    Public Shared Sub SafeSetText(control As Control, text As String)
        SafeInvoke(control, Sub() control.Text = text)
    End Sub

    Public Shared Sub SafeSetTextArray(controls As Control(), texts As String())
        If controls Is Nothing OrElse texts Is Nothing Then Return
        Dim count = Math.Min(controls.Length, texts.Length)
        For i As Integer = 0 To count - 1
            SafeSetText(controls(i), texts(i))
        Next
    End Sub

    Public Shared Sub SafeSetEnabled(control As Control, enabled As Boolean)
        SafeInvoke(control, Sub() control.Enabled = enabled)
    End Sub

    Public Shared Sub SafeSetLocation(form As Form, location As Point)
        SafeInvoke(form, Sub()
                             form.StartPosition = FormStartPosition.Manual
                             form.Location = location
                         End Sub)
    End Sub

    Public Shared Function ShowError(owner As IWin32Window, message As String, Optional caption As String = "Error") As DialogResult
        Return MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Function

    Public Shared Function ShowInfo(owner As IWin32Window, message As String, Optional caption As String = "Information") As DialogResult
        Return MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Function

    Public Shared Function ShowWarning(owner As IWin32Window, message As String, Optional caption As String = "Warning") As DialogResult
        Return MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Function

    Public Shared Sub SafeSetTextFormat(control As Control, format As String, ParamArray args As Object())
        SafeInvoke(control, Sub()
                                Try
                                    control.Text = String.Format(format, args)
                                Catch ex As FormatException
                                    ' Fallback: show raw format string and log error
                                    control.Text = format
                                    Log.WriteException(ex, $"[SafeSetTextFormat] Format error for control '{control.Name}' with format '{format}' and args [{String.Join(",", args)}]")
                                End Try
                            End Sub)
    End Sub

End Class