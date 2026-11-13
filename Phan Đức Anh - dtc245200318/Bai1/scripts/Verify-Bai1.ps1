param([switch]$SkipBuild)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$evidence = Join-Path $projectRoot 'docs/evidence'
$scratch = Join-Path $projectRoot '.experiments'
New-Item -ItemType Directory -Path $evidence, $scratch -Force | Out-Null
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.runtime/dotnet'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.runtime/packages'
$env:NO_PROXY = 'localhost,127.0.0.1,::1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'
$env:MSBuildEnableWorkloadResolver = 'false'
$log = Join-Path $evidence 'verification.txt'
Set-Content -LiteralPath $log -Value ('Bai 1 - ' + [DateTimeOffset]::Now.ToString('o')) -Encoding UTF8
$script:checks = 0
$script:startedApis = @()
Add-Type -AssemblyName System.Net.Http
$httpHandler = [System.Net.Http.HttpClientHandler]::new()
$httpHandler.UseProxy = $false
$script:http = [System.Net.Http.HttpClient]::new($httpHandler)
$script:http.Timeout = [TimeSpan]::FromSeconds(3)

function Write-Evidence([string]$Line) {
    Write-Host $Line
    Add-Content -LiteralPath $log -Value $Line -Encoding UTF8
}

function Assert-Lab([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "FAIL: $Label" }
    $script:checks++
    Write-Evidence "PASS: $Label"
}

function Invoke-Dotnet([string[]]$Arguments, [string]$OutputFile, [int]$ExpectedExit = 0) {
    if ($Arguments[0] -eq 'build') { $Arguments += @('-m:1', '-p:UseSharedCompilation=false') }
    $savedPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = @(& dotnet @Arguments 2>&1)
    $code = $LASTEXITCODE
    $ErrorActionPreference = $savedPreference
    $lines = @($output | ForEach-Object { $_.ToString() })
    $lines | Set-Content -LiteralPath (Join-Path $evidence $OutputFile) -Encoding UTF8
    Write-Evidence ('dotnet ' + ($Arguments -join ' ') + " -> exit $code")
    foreach ($line in $lines) { Write-Evidence $line }
    Assert-Lab ($code -eq $ExpectedExit) "Exit code = $ExpectedExit ($OutputFile)"
    return ($lines -join "`n")
}

function Invoke-LabHttp([string]$Method, [string]$Url, [string]$Json) {
    $message = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method), $Url)
    if ($Json) { $message.Content = [System.Net.Http.StringContent]::new($Json, [Text.Encoding]::UTF8, 'application/json') }
    try {
        $response = $script:http.SendAsync($message).GetAwaiter().GetResult()
        try {
            $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $location = if ($response.Headers.Location) { $response.Headers.Location.ToString() } else { '' }
            Write-Evidence ("$Method $Url -> " + [int]$response.StatusCode + ' ' + $response.ReasonPhrase)
            if ($location) { Write-Evidence "Location: $location" }
            if ($body) { Write-Evidence $body }
            $data = if ($body) { $body | ConvertFrom-Json } else { $null }
            return [pscustomobject]@{ Status = [int]$response.StatusCode; Data = $data; Body = $body; Location = $location }
        } finally { $response.Dispose() }
    } finally { $message.Dispose() }
}

function Start-LabApi([string]$Dll, [int]$Port, [string]$Label) {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
    try { $listener.Start() } finally { $listener.Stop() }
    $process = Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @(('"' + $Dll + '"'), '--urls', "http://localhost:$Port") -WorkingDirectory (Split-Path -Parent $Dll) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $evidence "$Label-api.txt") -RedirectStandardError (Join-Path $evidence "$Label-api-errors.txt")
    $script:startedApis += $process
    $ready = $false
    for ($attempt = 0; $attempt -lt 80; $attempt++) {
        if ($process.HasExited) { throw "API $Label exited: $(Get-Content -LiteralPath (Join-Path $evidence "$Label-api-errors.txt") -Raw)" }
        try {
            $probe = $script:http.GetAsync("http://localhost:$Port/").GetAwaiter().GetResult()
            $ready = $probe.IsSuccessStatusCode
            $probe.Dispose()
            if ($ready) { break }
        } catch { }
        Start-Sleep -Milliseconds 100
    }
    if (-not $ready) { throw "API $Label did not become ready" }
    Write-Evidence "API $Label listening on http://localhost:$Port (PID $($process.Id))"
    return $process
}

function Stop-LabApi($Process) {
    if ($Process -and -not $Process.HasExited) { Stop-Process -Id $Process.Id; $Process.WaitForExit() }
}

Push-Location $projectRoot
try {
    Write-Evidence ('SDK: ' + (& dotnet --version))
    Write-Evidence ('Git: ' + (& git --version))
    if (-not $SkipBuild) {
        foreach ($solution in @('EduSubmit.slnx', 'samples/MepDemo/MepDemo.csproj', 'samples/BasicWebApi/BasicWebApi.slnx', 'samples/MiniDistributedSystem/MiniDistributedSystem.slnx')) {
            $buildName = ($solution -replace '[^a-zA-Z0-9]', '-') + '-build.txt'
            $null = Invoke-Dotnet @('build', $solution, '--nologo', '-p:NuGetAudit=false', '-p:RestoreIgnoreFailedSources=true') $buildName
        }
    }

    # Bước 4: kiểm tra một console .NET độc lập.
    $helloDir = Join-Path $scratch 'HelloPtudpt'
    if (-not (Test-Path -LiteralPath (Join-Path $helloDir 'HelloPtudpt.csproj'))) {
        $null = Invoke-Dotnet @('new', 'console', '-n', 'HelloPtudpt', '-o', $helloDir, '--no-restore') 'hello-create.txt'
    }
    $helloOutput = Invoke-Dotnet @('run', '--project', $helloDir, '-p:NuGetAudit=false', '-p:RestoreIgnoreFailedSources=true') 'environment.txt'
    Assert-Lab ($helloOutput.Contains('Hello, World!')) 'Console Hello, World!'

    $mep = Invoke-Dotnet @('run', '--no-build', '--project', 'samples/MepDemo') 'mep-all.txt'
    foreach ($number in @(1, 3, 4)) {
        $null = Invoke-Dotnet @('run', '--no-build', '--project', 'samples/MepDemo', '--', "$number") "mep-$number.txt"
    }
    foreach ($number in 1..4) {
        Assert-Lab ([regex]::Matches($mep, "chấm xong sub-$number").Count -eq 1) "Queue: sub-$number completed by exactly one worker"
    }
    foreach ($subscriber in @('ScoreService', 'NotificationService', 'StatsService')) {
        Assert-Lab ($mep.Contains("[$subscriber]")) "Pub/sub delivered to $subscriber"
    }

    $basic = Start-LabApi (Join-Path $projectRoot 'samples/BasicWebApi/src/BasicWebApi/bin/Debug/net10.0/BasicWebApi.dll') 5081 'basic'
    try {
        $r = Invoke-LabHttp GET 'http://localhost:5081/'
        Assert-Lab ($r.Status -eq 200 -and $r.Data.application -eq 'BasicWebApi') 'Basic root 200'
        foreach ($case in @(@('', 'Xin chao, An!'), @('?language=en', 'Hello, An!'), @('?language=ja', 'Konnichiwa, An!'))) {
            $r = Invoke-LabHttp GET ('http://localhost:5081/hello/An' + $case[0])
            Assert-Lab ($r.Status -eq 200 -and $r.Data.message -eq $case[1]) ('Greeting ' + $case[1])
        }
        $r = Invoke-LabHttp POST 'http://localhost:5081/greetings' '{"name":"Binh","language":"vi"}'
        Assert-Lab ($r.Status -eq 201 -and $r.Location -eq '/greetings/1') 'Basic POST 201 + Location'
        $r = Invoke-LabHttp POST 'http://localhost:5081/greetings' '{"name":""}'
        Assert-Lab ($r.Status -eq 400 -and $r.Data.message -eq 'Name must not be empty.') 'Basic empty name 400'
    } finally { Stop-LabApi $basic }

    $miniDll = Join-Path $projectRoot 'samples/MiniDistributedSystem/src/Submission.Api/bin/Debug/net10.0/Submission.Api.dll'
    $miniClient = Join-Path $projectRoot 'samples/MiniDistributedSystem/src/Submission.Client/bin/Debug/net10.0/Submission.Client.dll'
    $mini = Start-LabApi $miniDll 5000 'mini'
    try {
        $r = Invoke-LabHttp GET 'http://localhost:5000/'
        Assert-Lab ($r.Status -eq 200 -and $r.Data.service -eq 'Submission.Api') 'Mini root 200'
        $r = Invoke-LabHttp GET 'http://localhost:5000/api/assignments'
        Assert-Lab ($r.Status -eq 200 -and $r.Data.Count -eq 2) 'Mini two assignments'
        $r = Invoke-LabHttp POST 'http://localhost:5000/api/submissions' '{"assignmentId":1,"studentCode":"SV001","repositoryUrl":"https://github.com/example/EduSubmit"}'
        Assert-Lab ($r.Status -eq 201) 'Mini valid POST 201'
        $r = Invoke-LabHttp POST 'http://localhost:5000/api/submissions' '{"assignmentId":1,"studentCode":"","repositoryUrl":"https://github.com/example/EduSubmit"}'
        Assert-Lab ($r.Status -eq 400) 'Mini invalid POST 400'
        $miniFirst = Invoke-Dotnet @($miniClient, 'http://localhost:5000') 'mini-client-1.txt'
        $miniSecond = Invoke-Dotnet @($miniClient, 'http://localhost:5000') 'mini-client-2.txt'
        $idPattern = 'Ma luot nop: ([0-9a-f-]{36})'
        Assert-Lab ([regex]::Match($miniFirst, $idPattern).Groups[1].Value -ne [regex]::Match($miniSecond, $idPattern).Groups[1].Value) 'Repeated submissions receive different GUIDs'

        # Các thay đổi a/c chỉ áp dụng trên bản sao, bản gốc giữ nguyên.
        $experimentDir = Join-Path $scratch 'MiniDistributedSystem'
        New-Item -ItemType Directory -Path $experimentDir -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $projectRoot 'samples/MiniDistributedSystem/src') -Destination $experimentDir -Recurse -Force
        $experimentApi = Join-Path $experimentDir 'src/Submission.Api/Program.cs'
        $originalApi = Get-Content -LiteralPath $experimentApi -Raw -Encoding UTF8
        $experimentClient = Join-Path $experimentDir 'src/Submission.Client/Program.cs'
        $originalClient = Get-Content -LiteralPath $experimentClient -Raw -Encoding UTF8

        Write-Evidence 'Experiment c prediction: API returns 400; EnsureSuccessStatusCode throws HttpRequestException.'
        Set-Content -LiteralPath $experimentClient -Value ($originalClient.Replace('StudentCode: "SV001"', 'StudentCode: ""')) -Encoding UTF8
        $null = Invoke-Dotnet @('build', (Join-Path $experimentDir 'src/Submission.Client'), '-p:NuGetAudit=false') 'experiment-c-build.txt'
        $invalid = Invoke-Dotnet @((Join-Path $experimentDir 'src/Submission.Client/bin/Debug/net10.0/Submission.Client.dll'), 'http://localhost:5000') 'experiment-c.txt' 1
        Assert-Lab ($invalid.Contains('400') -and $invalid.Contains('HttpRequestException')) 'Experiment c: 400 triggers EnsureSuccessStatusCode'
        Set-Content -LiteralPath $experimentClient -Value $originalClient -Encoding UTF8
    } finally { Stop-LabApi $mini }

    Write-Evidence 'Experiment b prediction: API stopped; client fails with connection refused.'
    $offline = Invoke-Dotnet @($miniClient, 'http://localhost:5000') 'experiment-b.txt' 1
    Assert-Lab ($offline.Contains('HttpRequestException')) 'Experiment b: offline API fails'

    Write-Evidence 'Experiment a prediction: unchanged client displays three assignments after server data changes.'
    $third = 'new Assignment(2, "Bai tap 2", "Viet client giao tiep qua HTTP"), new Assignment(3, "Bai tap 3", "Thu them du lieu")'
    Set-Content -LiteralPath $experimentApi -Value ($originalApi.Replace('new Assignment(2, "Bai tap 2", "Viet client giao tiep qua HTTP")', $third)) -Encoding UTF8
    $null = Invoke-Dotnet @('build', (Join-Path $experimentDir 'src/Submission.Api'), '-p:NuGetAudit=false') 'experiment-a-build.txt'
    $changedApi = Start-LabApi (Join-Path $experimentDir 'src/Submission.Api/bin/Debug/net10.0/Submission.Api.dll') 5000 'experiment-a'
    try {
        $three = Invoke-Dotnet @($miniClient, 'http://localhost:5000') 'experiment-a.txt'
        Assert-Lab ($three.Contains('3: Bai tap 3')) 'Experiment a: client unchanged displays assignment 3'
    } finally { Stop-LabApi $changedApi }
    Set-Content -LiteralPath $experimentApi -Value $originalApi -Encoding UTF8

    Write-Evidence 'Experiment d prediction: port 5000 fails; passing port 5050 succeeds without source edits.'
    $moved = Start-LabApi $miniDll 5050 'experiment-d'
    try {
        $null = Invoke-Dotnet @($miniClient, 'http://localhost:5000') 'experiment-d-wrong-port.txt' 1
        $correctPort = Invoke-Dotnet @($miniClient, 'http://localhost:5050') 'experiment-d-correct-port.txt'
        Assert-Lab ($correctPort.Contains('Trang thai: received')) 'Experiment d: URL argument fixes port mismatch'
    } finally { Stop-LabApi $moved }

    $main = Start-LabApi (Join-Path $projectRoot 'src/Submission.Api/bin/Debug/net10.0/Submission.Api.dll') 5000 'edusubmit'
    try {
        $r = Invoke-LabHttp GET 'http://localhost:5000/'
        Assert-Lab ($r.Status -eq 200 -and $r.Data.status -eq 'running') 'EduSubmit health 200'
        $r = Invoke-LabHttp GET 'http://localhost:5000/api/assignments'
        Assert-Lab ($r.Status -eq 200 -and $r.Data.Count -eq 2) 'EduSubmit assignments 200'
        $created = Invoke-LabHttp POST 'http://localhost:5000/api/submissions' '{"assignmentId":1,"studentCode":"DTC245200318","repositoryUrl":"https://github.com/anhphann1412-max/CNTTK23D"}'
        $parsedId = [guid]::Empty
        Assert-Lab ($created.Status -eq 201 -and [guid]::TryParse($created.Data.id, [ref]$parsedId) -and $parsedId -ne [guid]::Empty -and $created.Data.status -eq 'received' -and $created.Location -eq "/api/submissions/$parsedId" -and $created.Data.receivedAt) 'EduSubmit 201, GUID, status, timestamp and Location'
        $r = Invoke-LabHttp GET ('http://localhost:5000' + $created.Location)
        Assert-Lab ($r.Status -eq 200 -and $r.Data.id -eq $created.Data.id -and $r.Data.studentCode -eq 'DTC245200318') 'Read created submission 200'
        $r = Invoke-LabHttp GET 'http://localhost:5000/api/submissions/00000000-0000-0000-0000-000000000000'
        Assert-Lab ($r.Status -eq 404) 'Unknown GUID 404'
        foreach ($json in @('{}', '{"assignmentId":0,"studentCode":"DTC245200318","repositoryUrl":"x"}', '{"assignmentId":-1,"studentCode":"DTC245200318","repositoryUrl":"x"}', '{"assignmentId":1,"studentCode":" ","repositoryUrl":"x"}', '{"assignmentId":1,"studentCode":"DTC245200318"}', '{"assignmentId":1,"studentCode":"DTC245200318","repositoryUrl":" "}')) {
            $r = Invoke-LabHttp POST 'http://localhost:5000/api/submissions' $json
            Assert-Lab ($r.Status -eq 400) ('Reject invalid data 400: ' + $json)
        }
        $clientOutput = Invoke-Dotnet @((Join-Path $projectRoot 'src/Submission.Client/bin/Debug/net10.0/Submission.Client.dll'), 'http://localhost:5000') 'edusubmit-client.txt'
        Assert-Lab ($clientOutput.Contains('Doc lai:') -and $clientOutput.Contains('DTC245200318') -and $clientOutput.Contains('Trang thai: received')) 'EduSubmit client submits and reads back'
    } finally { Stop-LabApi $main }

    Write-Evidence "ALL $script:checks CHECKS PASSED"
} finally {
    foreach ($process in $script:startedApis) { Stop-LabApi $process }
    $script:http.Dispose()
    Pop-Location
}
