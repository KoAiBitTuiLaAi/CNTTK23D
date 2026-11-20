# kiem-tra-requests.ps1 – chạy lần lượt các ca của requests.http bằng curl.exe, in mã trạng thái THẬT
# Cần: máy chủ EduSubmit chạy cổng 5000 (và Ping.Api cổng 5200 cho ca 14)
#   .\kiem-tra-requests.ps1
param([string]$Api = "http://localhost:5000", [string]$PingApi = "http://localhost:5200")

$tmp = [IO.Path]::GetTempFileName(); $bodyFile = [IO.Path]::GetTempFileName()
$script:dat = 0; $script:tong = 0

function Send-Request([string]$method, [string]$url, [string]$json) {
    $a = @("-s", "-o", $tmp, "-D", "-", "-w", "`n%{http_code}", "-X", $method, $url)
    if ($json) {
        [IO.File]::WriteAllText($bodyFile, $json)
        $a += @("-H", "Content-Type: application/json", "--data-binary", "@$bodyFile")
    }
    $out = & curl.exe @a
    $code = [int]($out | Select-Object -Last 1)
    $loc = ($out | Where-Object { $_ -match "^Location:" }) -replace "^Location:\s*", ""
    $body = Get-Content $tmp -Raw -Encoding UTF8
    $parsed = $null; try { $parsed = $body | ConvertFrom-Json } catch {}
    [pscustomobject]@{ Code = $code; Location = "$loc".Trim(); Body = $body; Json = $parsed }
}

function Show-Result([string]$name, [int]$expect, $r, [string]$extra = "", [bool]$extraOk = $true) {
    $script:tong++
    $ok = ($r.Code -eq $expect) -and $extraOk
    if ($ok) { $script:dat++ }
    $tag = if ($ok) { "[DAT] " } else { "[SAI] " }
    Write-Host $tag -ForegroundColor $(if ($ok) { "Green" } else { "Red" }) -NoNewline
    Write-Host ("{0,-62} mong doi {1}  thuc te {2}  {3}" -f $name, $expect, $r.Code, $extra)
}

$r = Send-Request GET "$Api/";                       Show-Result " 1. GET /" 200 $r
$r = Send-Request GET "$Api/api/assignments";        Show-Result " 2. GET /api/assignments" 200 $r "($(@($r.Json).Count) bai tap)"
$r = Send-Request GET "$Api/api/assignments/1";      Show-Result " 3. GET /api/assignments/1" 200 $r
$hopLe = '{ "studentCode": "SV001", "repositoryUrl": "https://example.com/sv001/signalr" }'
$created = Send-Request POST "$Api/api/assignments/1/submissions" $hopLe
$t0 = Get-Date
Show-Result " 4. POST /api/assignments/1/submissions (hop le)" 201 $created "status=$($created.Json.status) Location=$($created.Location)" ($created.Location -ne "" -and $created.Json.status -eq "received")
$r = Send-Request GET "$Api$($created.Location)";    Show-Result " 5. GET luot nop vua tao (ngay lap tuc)" 200 $r "status=$($r.Json.status)"
$r = Send-Request POST "$Api/api/assignments/1/submissions" '{ "studentCode": "", "repositoryUrl": "" }'
Show-Result " 6. POST thieu studentCode, repositoryUrl" 400 $r "code=$($r.Json.code)"
$r = Send-Request POST "$Api/api/assignments/999/submissions" $hopLe; Show-Result " 7. POST /api/assignments/999/submissions" 404 $r "code=$($r.Json.code)"
$r = Send-Request POST "$Api/api/assignments/3/submissions" $hopLe;   Show-Result " 8. POST bai tap da dong (bai 3)" 400 $r "code=$($r.Json.code)"
$r = Send-Request GET "$Api/api/submissions/00000000-0000-0000-0000-000000000000"; Show-Result " 9. GET /api/submissions/{id rong}" 404 $r "code=$($r.Json.code)"
$r = Send-Request POST "$Api/hubs/submissions/negotiate?negotiateVersion=1"
Show-Result "10. POST /hubs/submissions/negotiate" 200 $r "connectionId=$($r.Json.connectionId)" ([bool]$r.Json.connectionId)
$r = Send-Request POST "$Api/hub/submissions/negotiate?negotiateVersion=1";   Show-Result "11. POST /hub/submissions/negotiate (sai hub)" 404 $r
$failed = Send-Request POST "$Api/api/assignments/1/submissions" '{ "studentCode": "SV002", "repositoryUrl": "http://example.com/sv002/signalr" }'
Show-Result "12. POST kho http:// (bai tap 2)" 201 $failed "status=$($failed.Json.status)"
$wait = 5 - ((Get-Date) - $t0).TotalSeconds; if ($wait -gt 0) { Start-Sleep -Milliseconds ([int]($wait * 1000)) }
$r = Send-Request GET "$Api$($created.Location)";    Show-Result " 5b. GET theo Location sau 5 giay" 200 $r "status=$($r.Json.status)" ($r.Json.status -eq "graded")
$r = Send-Request GET "$Api$($failed.Location)";     Show-Result "13. GET luot nop http:// sau ~5 giay (bai tap 2)" 200 $r "status=$($r.Json.status)" ($r.Json.status -eq "failed")
$r = Send-Request POST "$PingApi/hubs/ping/negotiate?negotiateVersion=1"
Show-Result "14. POST :5200/hubs/ping/negotiate (PingSignalR)" 200 $r "connectionId=$($r.Json.connectionId)" ([bool]$r.Json.connectionId)

Write-Host ""
Write-Host "Ket qua: dat $($script:dat)/$($script:tong) ca" -ForegroundColor $(if ($script:dat -eq $script:tong) { "Green" } else { "Yellow" })
Remove-Item $tmp, $bodyFile -ErrorAction SilentlyContinue
