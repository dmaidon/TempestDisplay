Imports System.IO
Imports System.Threading

Friend Module Globals

    Friend ReadOnly ImgDir As String = Path.Combine(Application.StartupPath, "Images")
    Friend ReadOnly TempDir As String = Path.Combine(Application.StartupPath, "$Tmp")
    Friend ReadOnly DataDir As String = Path.Combine(Application.StartupPath, "Data")
    Friend ReadOnly LogDir As String = Path.Combine(Application.StartupPath, "Logs")

    Friend LogFile As String

    Friend UdpLog As String

    Friend BatteryHistoryFile As String

    Friend AppStarts As Long = 0

    ' Global cancellation token for graceful shutdown
    Friend ReadOnly AppCancellationTokenSource As New CancellationTokenSource()

    Friend ReadOnly Property AppCancellationToken As CancellationToken
        Get
            Return AppCancellationTokenSource.Token
        End Get
    End Property

    ' Health check tracking
    Friend LastSuccessfulTempestFetch As DateTime = DateTime.MinValue

    Friend LastSuccessfulMeteoBridgeFetch As DateTime = DateTime.MinValue
    Friend TempestFetchFailureCount As Integer = 0
    Friend MeteoBridgeFetchFailureCount As Integer = 0

    'Friend Const APIToken As String = "a76ccf85-7164-4cd2-b2f2-75159d9a9d20"
    'Friend Const StationID As String = "146672"

    Friend ReadOnly Cpy As String = $"© {Date.Now.Year} CarolinaWx - All rights reserved."

    Friend ReadOnly nl As String = Environment.NewLine

End Module