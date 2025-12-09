Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading

Namespace Modules.GWD
    Friend Module GwdRoutines

        ' Constants for better maintainability
        Private Const DefaultTimeoutSeconds As Integer = 30

        Private Const ContentType As String = "text/plain;charset=iso-8859-1"
        Private Const DefaultErrorValue As String = "*"c
        Private Const SettingsCacheTtlSeconds As Integer = 60

        ' Track last successful response (UTC)
        Private _lastSuccessUtc As Date?

        ' Lightweight injectable providers for testability
        Friend Interface IWeatherbridgeSettingsProvider

            Function GetLogin() As String

            Function GetPassword() As String

            Function GetIP() As String

        End Interface

        Private Class DefaultSettingsProvider
            Implements IWeatherbridgeSettingsProvider

            Public Function GetLogin() As String Implements IWeatherbridgeSettingsProvider.GetLogin
                Dim settings = LoadSettings()
                Return settings.MeteoBridge.Login
            End Function

            Public Function GetPassword() As String Implements IWeatherbridgeSettingsProvider.GetPassword
                Dim settings = LoadSettings()
                Return settings.MeteoBridge.Password
            End Function

            Public Function GetIP() As String Implements IWeatherbridgeSettingsProvider.GetIP
                Dim settings = LoadSettings()
                Return settings.MeteoBridge.IpAddress
            End Function

        End Class

        Friend Property SettingsProvider As IWeatherbridgeSettingsProvider = New DefaultSettingsProvider()

        ' Settings cache
        Private _settingsCache As WeatherbridgeSettingsValidation

        Private _settingsCacheAtUtc As Date
        Private _settingsCacheHasValue As Boolean

        ' Case-insensitive error indicators
        Private ReadOnly ErrorIndicators As New System.Collections.Generic.HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "error", "failed", "not found", "invalid", "timeout", "unavailable"
        }

        Friend Structure GwdResult
            Public ReadOnly Success As Boolean
            Public ReadOnly Value As String
            Public ReadOnly ErrorMessage As String
            Public ReadOnly StatusCode As HttpStatusCode?
            Public ReadOnly Template As String
            Public ReadOnly TimestampUtc As Date

            Public Sub New(success As Boolean, value As String, errorMessage As String, template As String, statusCode As HttpStatusCode?)
                Me.Success = success
                Me.Value = value
                Me.ErrorMessage = errorMessage
                Me.Template = template
                Me.StatusCode = statusCode
                Me.TimestampUtc = Date.UtcNow
            End Sub

            Public Shared Function Ok(value As String, template As String, Optional statusCode As HttpStatusCode? = Nothing) As GwdResult
                Return New GwdResult(True, value, Nothing, template, statusCode)
            End Function

            Public Shared Function Fail(errorMessage As String, template As String, Optional statusCode As HttpStatusCode? = Nothing) As GwdResult
                Return New GwdResult(False, Nothing, errorMessage, template, statusCode)
            End Function

        End Structure

        ''' <summary>
        ''' HTTP ONLY (per user requirement) retrieval of weatherbridge data.
        ''' HTTPS disabled and no HTTPS->HTTP fallback logic remains.
        ''' </summary>
        Friend Async Function Gwd3AsyncResult(templateStr As String,
                                              Optional perRequestTimeoutSeconds As Integer? = Nothing,
                                              Optional cancellationToken As CancellationToken = Nothing) As Task(Of GwdResult)
            If String.IsNullOrWhiteSpace(templateStr) Then Throw New ArgumentException("Template string cannot be null or empty", NameOf(templateStr))

            Log.Write($"Gwd3 Start: {templateStr}")
            ' Dim url As String = String.Empty
            Dim linkedCts As CancellationTokenSource = Nothing
            Dim effectiveToken As CancellationToken = If(cancellationToken = Nothing, CancellationToken.None, cancellationToken)

            Try
                Dim validationResult = ValidateWeatherbridgeSettings()
                If Not validationResult.IsValid Then
                    Log.Write($"Invalid weatherbridge settings: {validationResult.ErrorMessage}")
                    Return GwdResult.Fail(validationResult.ErrorMessage, templateStr)
                End If

                Dim url As String = BuildWeatherbridgeUrl(validationResult.IP, templateStr)
                If String.IsNullOrWhiteSpace(url) Then
                    Log.Write("[Gwd3] Failed to build request URL")
                    Return GwdResult.Fail("Failed to build request URL", templateStr)
                End If
                Log.Write($"Request URL: {url}")

                If perRequestTimeoutSeconds.HasValue AndAlso perRequestTimeoutSeconds.Value > 0 Then
                    linkedCts = CancellationTokenSource.CreateLinkedTokenSource(effectiveToken)
                    linkedCts.CancelAfter(TimeSpan.FromSeconds(perRequestTimeoutSeconds.Value))
                    effectiveToken = linkedCts.Token
                End If

                ' Use HttpClientService with authentication for HTTP request
                Dim responseResult As GwdResult
                Try
                    Dim respStr = Await HttpClientService.Instance.GetStringWithAuthAsync(url, validationResult.Login, validationResult.Password, effectiveToken).ConfigureAwait(False)
                    responseResult = ProcessResponseContentResult(respStr, templateStr, Nothing)
                    If responseResult.Success Then
                        Dim snippet = If(responseResult.Value, String.Empty)
                        If snippet.Length > 256 Then snippet = String.Concat(snippet.AsSpan(0, 256), "...")
                        Log.Write($"Response Content (snippet): {snippet}")
                    End If
                Catch ex As HttpRequestException
                    Log.Write($"[Gwd3] HTTP error for template: {templateStr} - {ex.Message}")
                    responseResult = GwdResult.Fail($"HTTP error: {ex.Message}", templateStr)
                End Try
                Return responseResult
            Catch ex As OperationCanceledException
                Log.Write($"[Gwd3] Request cancelled for template: {templateStr}")
                Globals.MeteoBridgeFetchFailureCount += 1
                Return GwdResult.Fail("Request cancelled", templateStr)
            Catch ex As HttpRequestException
                Log.Write($"[Gwd3] HTTP error for template: {templateStr} - {ex.Message}")
                Globals.MeteoBridgeFetchFailureCount += 1
                Return GwdResult.Fail($"HTTP error: {ex.Message}", templateStr)
            Catch ex As ArgumentException
                Log.Write($"[Gwd3] Invalid argument for template: {templateStr} - {ex.Message}")
                Globals.MeteoBridgeFetchFailureCount += 1
                Return GwdResult.Fail($"Invalid argument: {ex.Message}", templateStr)
            Catch ex As Exception
                Log.WriteException(ex, $"[Gwd3] Unexpected exception for template: {templateStr}")
                Globals.MeteoBridgeFetchFailureCount += 1
                Return GwdResult.Fail($"Unexpected exception: {ex.Message}", templateStr)
            Finally
                linkedCts?.Dispose()
                Log.Write($"[Gwd3] Finished processing template: {templateStr}")
            End Try
        End Function

        Friend Async Function Gwd3Async(templateStr As String,
                                        Optional perRequestTimeoutSeconds As Integer? = Nothing,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Dim result = Await Gwd3AsyncResult(templateStr, perRequestTimeoutSeconds, cancellationToken).ConfigureAwait(False)
            Return If(result.Success, result.Value, DefaultErrorValue)
        End Function

        Friend Async Function Gwd3Async(templateStr As String, cancellationToken As CancellationToken) As Task(Of String)
            Dim result = Await Gwd3AsyncResult(templateStr, cancellationToken:=cancellationToken).ConfigureAwait(False)
            Return If(result.Success, result.Value, DefaultErrorValue)
        End Function

        Private Structure WeatherbridgeSettingsValidation
            Public Property IsValid As Boolean
            Public Property Login As String
            Public Property Password As String
            Public Property IP As String
            Public Property ErrorMessage As String

            Public Sub New(isValid As Boolean, Optional login As String = "", Optional password As String = "", Optional ip As String = "", Optional errorMessage As String = "")
                Me.IsValid = isValid : Me.Login = login : Me.Password = password : Me.IP = ip : Me.ErrorMessage = errorMessage
            End Sub

        End Structure

        Friend Sub InvalidateWeatherbridgeSettingsCache()
            _settingsCacheHasValue = False
        End Sub

        Private Function ValidateWeatherbridgeSettings() As WeatherbridgeSettingsValidation
            Try
                If _settingsCacheHasValue AndAlso (Date.UtcNow - _settingsCacheAtUtc).TotalSeconds < SettingsCacheTtlSeconds Then Return _settingsCache
                Dim login = SettingsProvider.GetLogin()?.Trim()
                Dim password = SettingsProvider.GetPassword()?.Trim()
                Dim ip = SettingsProvider.GetIP()?.Trim()
                If String.IsNullOrEmpty(login) Then Return New WeatherbridgeSettingsValidation(False, errorMessage:="Weatherbridge login is required")
                If String.IsNullOrEmpty(password) Then Return New WeatherbridgeSettingsValidation(False, errorMessage:="Weatherbridge password is required")
                If String.IsNullOrEmpty(ip) Then Return New WeatherbridgeSettingsValidation(False, errorMessage:="Weatherbridge IP address is required")
                If Not IsValidIPOrHostname(ip) Then Return New WeatherbridgeSettingsValidation(False, errorMessage:=$"Invalid IP address or hostname: {ip}")
                Dim validated = New WeatherbridgeSettingsValidation(True, login, password, ip)
                _settingsCache = validated : _settingsCacheAtUtc = Date.UtcNow : _settingsCacheHasValue = True
                Return validated
            Catch ex As Exception
                Log.WriteException(ex, "Error validating Weatherbridge settings")
                Return New WeatherbridgeSettingsValidation(False, errorMessage:=$"Error reading settings: {ex.Message}")
            End Try
        End Function

        ' Force HTTP: ignore preferHttps flag removed.
        Private Function BuildWeatherbridgeUrl(ipOrUrl As String, templateStr As String) As String
            Try
                Dim baseUri As Uri
                Dim hostInput = ipOrUrl.Trim()
                If hostInput.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse hostInput.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                    ' Normalize any https:// to http:// per requirement
                    If hostInput.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                        hostInput = String.Concat("http://", hostInput.AsSpan("https://".Length))
                    End If
                    Dim root = New Uri(If(hostInput.EndsWith("/"c), hostInput, hostInput & "/"c))
                    baseUri = New Uri(root, "cgi-bin/template.cgi")
                Else
                    baseUri = New Uri($"http://{hostInput}/cgi-bin/template.cgi")
                End If

                Dim templateWithBrackets = $"[{templateStr}]"
                Dim encodedTemplate = Uri.EscapeDataString(templateWithBrackets)
                Dim queryBuilder As New StringBuilder()
                queryBuilder.Append("template=").Append(encodedTemplate)
                queryBuilder.Append("&contenttype=").Append(Uri.EscapeDataString(ContentType))
                Return $"{baseUri}?{queryBuilder}"
            Catch ex As Exception
                Log.WriteException(ex, "Error building Weatherbridge URL")
                Return String.Empty
            End Try
        End Function

        Private Function IsValidIPOrHostname(address As String) As Boolean
            If String.IsNullOrWhiteSpace(address) Then Return False
            If IPAddress.TryParse(address, Nothing) Then Return True
            Dim hostType = Uri.CheckHostName(address)
            If hostType = UriHostNameType.Dns Then
                Try
                    If address.Length > 253 OrElse address.Contains(" "c) Then Return False
                    If address.StartsWith("-"c) OrElse address.EndsWith("-"c) OrElse address.StartsWith("."c) OrElse address.EndsWith("."c) Then Return False
                    For Each label In address.Split("."c)
                        If label.Length = 0 OrElse label.Length > 63 Then Return False
                        If Not System.Text.RegularExpressions.Regex.IsMatch(label, "^[A-Za-z0-9](?:[A-Za-z0-9\-]{0,61}[A-Za-z0-9])?$") Then Return False
                    Next
                    Return True
                Catch : Return False
                End Try
            End If
            Return False
        End Function

        Private Function ProcessResponseContentResult(content As String, templateStr As String, statusCode As HttpStatusCode?) As GwdResult
            If content Is Nothing Then
                Log.Write($"[Gwd3] Received null response content for template: {templateStr}")
                Return GwdResult.Fail("Null response content", templateStr, statusCode)
            End If
            Dim processedContent = content.Trim()
            If String.IsNullOrEmpty(processedContent) Then
                Log.Write($"[Gwd3] Received empty response content for template: {templateStr}")
                Return GwdResult.Fail("Empty response content", templateStr, statusCode)
            End If
            If ContainsErrorIndicators(processedContent) Then
                Log.Write($"[Gwd3] Response contains error indicators for template: {templateStr} | Content: {processedContent}")
                Globals.MeteoBridgeFetchFailureCount += 1
                Return GwdResult.Fail("Content contains error indicators", templateStr, statusCode)
            End If
            _lastSuccessUtc = Date.UtcNow

            ' Track successful MeteoBridge fetch
            Globals.LastSuccessfulMeteoBridgeFetch = DateTime.UtcNow
            Globals.MeteoBridgeFetchFailureCount = 0

            Return GwdResult.Ok(processedContent, templateStr, statusCode)
        End Function

        Private Function ContainsErrorIndicators(content As String) As Boolean
            If String.IsNullOrEmpty(content) Then Return True
            For Each indicator In ErrorIndicators
                If content.Contains(indicator, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
            Return False
        End Function

        Friend Async Function Gwd3BatchAsyncResult(templates As IEnumerable(Of String), Optional maxConcurrency As Integer = 5, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Dictionary(Of String, GwdResult))
            ArgumentNullException.ThrowIfNull(templates)
            Dim templateList = templates.ToList()
            If templateList.Count = 0 Then Return New Dictionary(Of String, GwdResult)()
            Log.Write($"[Gwd3Batch] Starting batch (structured) processing of {templateList.Count} templates")
            Dim effectiveToken As CancellationToken = If(cancellationToken = Nothing, CancellationToken.None, cancellationToken)
            Using semaphore As New SemaphoreSlim(maxConcurrency, maxConcurrency)
                Dim tasks = templateList.Select(Async Function(template) As Task(Of KeyValuePair(Of String, GwdResult))
                                                    Await semaphore.WaitAsync(effectiveToken).ConfigureAwait(False)
                                                    Try
                                                        Dim result = Await Gwd3AsyncResult(template, cancellationToken:=effectiveToken).ConfigureAwait(False)
                                                        Return New KeyValuePair(Of String, GwdResult)(template, result)
                                                    Finally
                                                        semaphore.Release()
                                                    End Try
                                                End Function)
                Dim results = Await Task.WhenAll(tasks).ConfigureAwait(False)
                Log.Write($"[Gwd3Batch] Completed batch (structured) processing of {templateList.Count} templates")
                Return results.ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value)
            End Using
        End Function

        Friend Function GetLastSuccessfulResponseTime() As Date?
            If _lastSuccessUtc.HasValue Then Return _lastSuccessUtc.Value.ToLocalTime()
            Return Nothing
        End Function

        Friend Async Function TestWeatherbridgeConnectivityAsync(Optional timeoutSeconds As Integer = 10) As Task(Of Boolean)
            Try
                Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds))
                    Dim result = Await Gwd3AsyncResult("test", cancellationToken:=cts.Token).ConfigureAwait(False)
                    Return result.Success
                End Using
            Catch
                Return False
            End Try
        End Function

    End Module
End Namespace