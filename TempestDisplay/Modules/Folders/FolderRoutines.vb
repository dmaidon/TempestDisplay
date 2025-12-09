Imports System.IO

Friend Module FolderRoutines

    Friend Sub CreateAppFolders()
        Dim allFolders As New List(Of String) From {
                Globals.LogDir,
                Globals.TempDir,
                Globals.DataDir,
                Globals.ImgDir
                            }

        For Each folder In allFolders
            If Not Directory.Exists(folder) Then
                Directory.CreateDirectory(folder)
            End If
        Next
    End Sub

End Module