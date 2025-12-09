Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading

Public Class HttpClientService
    Private Shared ReadOnly _instance As New HttpClientService()

    Public Shared ReadOnly Property Instance As HttpClientService
        Get
            Return _instance
        End Get
    End Property

    Private ReadOnly _client As HttpClient
    Private ReadOnly _rateGate As SemaphoreSlim
    Private ReadOnly _maxRequestsPerWindow As Integer = 5
    Private ReadOnly _window As TimeSpan = TimeSpan.FromSeconds(1)
    Private ReadOnly _requestTimestamps As New Queue(Of DateTime)
    Private Shared ReadOnly _jsonOptions As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}

    Private Sub New()
        Dim handler As New HttpClientHandler With {
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }

        _client = New HttpClient(handler) With {
            .Timeout = TimeSpan.FromSeconds(15)
        }

        _client.DefaultRequestHeaders.UserAgent.ParseAdd($"TempestDisplay/{Application.ProductVersion} (+https://carolinawx.com)")
        _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate")

        _rateGate = New SemaphoreSlim(1, 1)
    End Sub

    Private Async Function EnforceRateLimitAsync(ct As CancellationToken) As Task
        Await _rateGate.WaitAsync(ct).ConfigureAwait(False)
        Try
            Dim now = DateTime.UtcNow
            ' Remove timestamps outside window
            While _requestTimestamps.Count > 0 AndAlso now - _requestTimestamps.Peek() > _window
                _requestTimestamps.Dequeue()
            End While

            ' Defensive cleanup: limit queue size to prevent unbounded growth
            ' Keep only the most recent timestamps that fit within the window
            While _requestTimestamps.Count > _maxRequestsPerWindow * 2
                _requestTimestamps.Dequeue()
            End While

            If _requestTimestamps.Count >= _maxRequestsPerWindow Then
                Dim waitFor = _window - (now - _requestTimestamps.Peek())
                If waitFor < TimeSpan.Zero Then waitFor = TimeSpan.Zero
                Await Task.Delay(waitFor, ct).ConfigureAwait(False)
                ' After delay, cleanup again
                now = DateTime.UtcNow
                While _requestTimestamps.Count > 0 AndAlso now - _requestTimestamps.Peek() > _window
                    _requestTimestamps.Dequeue()
                End While
            End If

            _requestTimestamps.Enqueue(DateTime.UtcNow)
        Finally
            _rateGate.Release()
        End Try
    End Function

    Private Shared Function IsTransient(status As HttpStatusCode) As Boolean
        Select Case status
            Case HttpStatusCode.RequestTimeout,
                 CType(429, HttpStatusCode),
                 HttpStatusCode.InternalServerError,
                 HttpStatusCode.BadGateway,
                 HttpStatusCode.ServiceUnavailable,
                 HttpStatusCode.GatewayTimeout
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function IsTransient(ex As Exception) As Boolean
        Return TypeOf ex Is HttpRequestException OrElse TypeOf ex Is TaskCanceledException
    End Function

    Private Shared Function JitteredDelay(baseMs As Integer, attempt As Integer) As Integer
        Dim max As Integer = CInt(Math.Min(60000, baseMs * Math.Pow(2, attempt)))
        ' Use Random.Shared (thread-safe, available in .NET 6+) instead of creating new instance
        Return Random.Shared.Next(CInt(max * 0.7), max)
    End Function

    Public Async Function GetStringAsync(url As String, Optional ct As CancellationToken = Nothing) As Task(Of String)
        If ct = Nothing Then ct = Globals.AppCancellationToken
        Return Await SendAsync(Of String)(Function(resp, ctt) resp.Content.ReadAsStringAsync(ctt), url, Nothing, ct).ConfigureAwait(False)
    End Function

    Public Async Function GetStringWithAuthAsync(url As String, username As String, password As String, Optional ct As CancellationToken = Nothing) As Task(Of String)
        If ct = Nothing Then ct = Globals.AppCancellationToken
        Dim parser As Func(Of HttpResponseMessage, CancellationToken, Task(Of String)) = Function(resp, ctt) resp.Content.ReadAsStringAsync(ctt)

        ' Create request with Basic Auth
        Dim credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"))
        Dim authHeader = $"Basic {credentials}"

        Return Await SendAsyncWithAuth(Of String)(parser, url, Nothing, authHeader, ct).ConfigureAwait(False)
    End Function

    Public Async Function GetJsonAsync(Of T)(url As String, Optional ct As CancellationToken = Nothing) As Task(Of T)
        If ct = Nothing Then ct = Globals.AppCancellationToken
        Dim parser As Func(Of HttpResponseMessage, CancellationToken, Task(Of T)) = Async Function(resp, ctt)
                                                                                        Dim stream = Await resp.Content.ReadAsStreamAsync(ctt).ConfigureAwait(False)
                                                                                        Return Await JsonSerializer.DeserializeAsync(Of T)(stream, _jsonOptions, ctt).ConfigureAwait(False)
                                                                                    End Function
        Return Await SendAsync(parser, url, Nothing, ct).ConfigureAwait(False)
    End Function

    Public Async Function PostJsonAsync(Of TReq, TResp)(url As String, payload As TReq, Optional ct As CancellationToken = Nothing) As Task(Of TResp)
        If ct = Nothing Then ct = Globals.AppCancellationToken
        Dim json = JsonSerializer.Serialize(payload)
        Dim content As New StringContent(json, Encoding.UTF8, "application/json")
        Dim parser As Func(Of HttpResponseMessage, CancellationToken, Task(Of TResp)) = Async Function(resp, ctt)
                                                                                            Dim stream = Await resp.Content.ReadAsStreamAsync(ctt).ConfigureAwait(False)
                                                                                            Return Await JsonSerializer.DeserializeAsync(Of TResp)(stream, _jsonOptions, ctt).ConfigureAwait(False)
                                                                                        End Function
        Return Await SendAsync(parser, url, content, ct, HttpMethod.Post).ConfigureAwait(False)
    End Function

    Private Async Function SendAsync(Of T)(parser As Func(Of HttpResponseMessage, CancellationToken, Task(Of T)), url As String, content As HttpContent, ct As CancellationToken, Optional method As HttpMethod = Nothing) As Task(Of T)
        Return Await SendAsyncWithAuth(parser, url, content, Nothing, ct, method).ConfigureAwait(False)
    End Function

    Private Async Function SendAsyncWithAuth(Of T)(parser As Func(Of HttpResponseMessage, CancellationToken, Task(Of T)), url As String, content As HttpContent, authHeader As String, ct As CancellationToken, Optional method As HttpMethod = Nothing) As Task(Of T)
        If method Is Nothing Then method = HttpMethod.Get
        Dim attempts As Integer = 0
        Dim maxAttempts As Integer = 5
        Dim baseDelayMs As Integer = 250

        Do
            ct.ThrowIfCancellationRequested()
            Await EnforceRateLimitAsync(ct).ConfigureAwait(False)

            Dim req As New HttpRequestMessage(method, url)
            If content IsNot Nothing Then req.Content = content

            ' Add authentication header if provided
            If Not String.IsNullOrEmpty(authHeader) Then
                req.Headers.TryAddWithoutValidation("Authorization", authHeader)
            End If

            Dim shouldRetry As Boolean = False
            Dim retryDelay As Integer = 0

            Try
                Using resp = Await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(False)
                    If resp.IsSuccessStatusCode Then
                        Return Await parser(resp, ct).ConfigureAwait(False)
                    End If

                    If IsTransient(resp.StatusCode) AndAlso attempts < maxAttempts - 1 Then
                        shouldRetry = True
                        retryDelay = JitteredDelay(baseDelayMs, attempts)
                    Else
                        Dim body As String = String.Empty
                        Try
                            body = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        Catch
                        End Try
                        Throw New HttpRequestException($"HTTP {(CInt(resp.StatusCode))} {resp.ReasonPhrase}: {body}")
                    End If
                End Using
            Catch ex As Exception
                If IsTransient(ex) AndAlso attempts < maxAttempts - 1 Then
                    shouldRetry = True
                    retryDelay = JitteredDelay(baseDelayMs, attempts)
                Else
                    Throw
                End If
            End Try

            If shouldRetry Then
                attempts += 1
                Await Task.Delay(retryDelay, ct).ConfigureAwait(False)
                Continue Do
            End If

            Exit Do
        Loop

        Throw New HttpRequestException("Request failed after retries.")
    End Function

End Class