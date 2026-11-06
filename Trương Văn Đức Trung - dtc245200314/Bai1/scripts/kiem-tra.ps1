param([int]$Port = 5098)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$baseUrl = "http://localhost:$Port"
$apiLog = Join-Path ([IO.Path]::GetTempPath()) ('edusubmit-api-' + [Guid]::NewGuid() + '.log')
$apiErrorLog = $apiLog + '.err'

dotnet build (Join-Path $projectRoot 'EduSubmit.slnx') --nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

# Chỉ dừng tiến trình API do script này tạo, không đụng tới chương trình khác.
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $Port)
try { $listener.Start() } finally { $listener.Stop() }
$apiDll = Join-Path $projectRoot 'src/Submission.Api/bin/Debug/net10.0/Submission.Api.dll'
$server = Start-Process -FilePath 'dotnet' -ArgumentList @(('"' + $apiDll + '"'),'--urls',$baseUrl) -WorkingDirectory $projectRoot -PassThru -WindowStyle Hidden -RedirectStandardOutput $apiLog -RedirectStandardError $apiErrorLog
Add-Type -AssemblyName System.Net.Http
$http = [Net.Http.HttpClient]::new()
$http.Timeout = [TimeSpan]::FromSeconds(5)
function Send-Check([string]$Method, [string]$Path, [int]$Expected, [string]$Body = '') {
    $message = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($Method), $baseUrl + $Path)
    try {
        if ($Body) { $message.Content = [Net.Http.StringContent]::new($Body, [Text.Encoding]::UTF8, 'application/json') }
        $response = $http.SendAsync($message).GetAwaiter().GetResult()
        try {
            $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            if ([int]$response.StatusCode -ne $Expected) { throw "$Method $Path expected $Expected; got $([int]$response.StatusCode): $text" }
            Write-Host "PASS $Method $Path -> $Expected"
            return $text
        } finally { $response.Dispose() }
    } finally { $message.Dispose() }
}
try {
    $ready = $false
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        if ($server.HasExited) { throw "API stopped. See $apiLog and $apiErrorLog" }
        try {
            $r = $http.GetAsync($baseUrl + '/').GetAwaiter().GetResult()
            $ready = $r.IsSuccessStatusCode
            $r.Dispose()
            if ($ready) { break }
        } catch { }
        Start-Sleep -Milliseconds 200
    }
    if (-not $ready) { throw 'API did not start in time.' }
    Send-Check 'GET' '/' 200 | Out-Null
    Send-Check 'GET' '/api/assignments' 200 | Out-Null
    $created = Send-Check 'POST' '/api/submissions' 201 '{"assignmentId":1,"studentCode":"DTC245200314","repositoryUrl":"https://example.com/repo.git"}' | ConvertFrom-Json
    Send-Check 'GET' ("/api/submissions/" + $created.id) 200 | Out-Null
    Send-Check 'GET' '/api/submissions/00000000-0000-0000-0000-000000000000' 404 | Out-Null
    Send-Check 'POST' '/api/submissions' 400 '{"assignmentId":1,"studentCode":"","repositoryUrl":"https://example.com/repo.git"}' | Out-Null
    Send-Check 'POST' '/api/submissions' 400 '{"assignmentId":0,"studentCode":"DTC245200314","repositoryUrl":"https://example.com/repo.git"}' | Out-Null
    dotnet run --no-build --project (Join-Path $projectRoot 'src/Submission.Client') -- $baseUrl
    if ($LASTEXITCODE -ne 0) { throw 'Client failed.' }
    Write-Host 'PASS: API, Client and extension.'
} finally {
    $http.Dispose()
    if (-not $server.HasExited) { Stop-Process -Id $server.Id }
    Write-Host "API logs: $apiLog"
}
