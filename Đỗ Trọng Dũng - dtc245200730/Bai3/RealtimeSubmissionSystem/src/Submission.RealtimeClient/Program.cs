// EduSubmit realtime – Submission.RealtimeClient (Bài 3: REST + SignalR)
// Chạy:
//   dotnet run --project src/Submission.RealtimeClient -- http://localhost:5000 SV001                      (bước 6)
//   dotnet run --project src/Submission.RealtimeClient -- http://localhost:5000 SV003 http://example.com/sv003   (bài tập 2: failed)
//   dotnet run --project src/Submission.RealtimeClient -- http://localhost:5000 --hai-luot                 (bài tập 3: 2 lượt nộp)
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

var twoSubmissions = args.Contains("--hai-luot");
var positional = args.Where(a => !a.StartsWith("--")).ToArray();
var apiBaseUrl = positional.Length > 0 ? positional[0].TrimEnd('/') : "http://localhost:5000";
var studentCode = positional.Length > 1 ? positional[1] : "SV001";
var repositoryUrl = positional.Length > 2 ? positional[2] : $"https://example.com/{studentCode}/signalr";

// Kênh 1: REST – tạo và đọc tài nguyên
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// Kênh 2: SignalR – nhận thay đổi do máy chủ đẩy về
await using var connection = new HubConnectionBuilder()
    .WithUrl($"{apiBaseUrl}/hubs/submissions")
    .WithAutomaticReconnect()
    .Build();

// Mỗi lượt nộp đang theo dõi có một "cái chuông" riêng (bài tập 3),
// chuông reo khi gặp trạng thái cuối: graded hoặc failed (bài tập 2)
var bells = new ConcurrentDictionary<Guid, TaskCompletionSource<string>>();
var lastPushedStatus = new ConcurrentDictionary<Guid, string>();

// Đăng ký nghe TRƯỚC khi mở kết nối; tên phải khớp SendAsync ở máy chủ
connection.On<SubmissionStatusMessage>("SubmissionStatusChanged", m =>
{
    lastPushedStatus[m.SubmissionId] = m.Status;
    var id = twoSubmissions ? Short(m.SubmissionId) : m.SubmissionId.ToString();

    if (m.Status == "failed")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"!!! CANH BAO: luot nop {id} cham LOI (failed) luc {m.UpdatedAt.ToLocalTime():HH:mm:ss}");
        Console.ResetColor();
    }
    else
    {
        Console.WriteLine($"Thong bao SignalR: {id} -> {m.Status} luc {m.UpdatedAt.ToLocalTime():HH:mm:ss}");
    }

    if (m.Status is "graded" or "failed" && bells.TryGetValue(m.SubmissionId, out var bell))
        bell.TrySetResult(m.Status);
});

await connection.StartAsync();
Console.WriteLine("Da ket noi SignalR.");

// (1) REST: lấy danh sách bài tập, chọn bài đầu tiên còn mở
var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");
var selected = assignments?.FirstOrDefault(a => a.IsOpen);
if (selected is null)
{
    Console.WriteLine("Khong co bai tap nao dang mo.");
    Environment.ExitCode = 1;
    return;
}
Console.WriteLine($"Chon bai tap: {selected.Id} - {selected.Title}");

if (twoSubmissions)
{
    // Bài tập 3: tạo 2 lượt nộp, theo dõi cả hai trên CÙNG một kết nối
    // SV002 dùng http:// nên máy chủ chấm failed (bài tập 2)
    var created = new List<SubmissionDto>();
    foreach (var (code, repo) in new[] { ("SV001", "https://example.com/sv001/signalr"), ("SV002", "http://example.com/sv002/signalr") })
    {
        var submission = await CreateSubmissionAsync(selected.Id, code, repo);
        if (submission is null) { Environment.ExitCode = 1; return; }
        bells[submission.Id] = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        created.Add(submission);
        Console.WriteLine($"Da tao {code}: {Short(submission.Id)} ({submission.Status})");
    }

    foreach (var submission in created)
        await connection.InvokeAsync("WatchSubmission", submission.Id.ToString());
    Console.WriteLine("Dang cho thong bao trang thai tu may chu...");

    var allDone = Task.WhenAll(created.Select(s => bells[s.Id].Task));
    if (await Task.WhenAny(allDone, Task.Delay(TimeSpan.FromSeconds(10))) != allDone)
        Console.WriteLine("Qua thoi gian cho thong bao trang thai cuoi.");

    // Chứng minh không trộn lẫn: trạng thái SignalR của từng lượt nộp phải khớp REST
    foreach (var submission in created)
    {
        var latestOne = await http.GetFromJsonAsync<SubmissionDto>($"/api/submissions/{submission.Id}");
        var pushed = lastPushedStatus.TryGetValue(submission.Id, out var status) ? status : "(khong nhan)";
        Console.WriteLine($"{submission.StudentCode}: SignalR = {pushed}, REST = {latestOne?.Status}");
    }
    return;
}

// (2) REST: tạo lượt nộp – kiểm tra mã 201 TRƯỚC khi đọc body
var createdOne = await CreateSubmissionAsync(selected.Id, studentCode, repositoryUrl);
if (createdOne is null)
{
    Environment.ExitCode = 1;
    return;
}
var gradedNotification = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
bells[createdOne.Id] = gradedNotification;
Console.WriteLine($"Da tao luot nop: {createdOne.Id}");
Console.WriteLine($"Trang thai REST ban dau: {createdOne.Status}");

// (3) SignalR: vào nhóm submission:{id} – chỉ gọi được khi đã có id từ POST
await connection.InvokeAsync("WatchSubmission", createdOne.Id.ToString());
Console.WriteLine("Dang cho thong bao trang thai tu may chu...");

// (4) Chờ cái nào xong trước: chuông (graded/failed) hay đồng hồ 10 giây
var completed = await Task.WhenAny(gradedNotification.Task, Task.Delay(TimeSpan.FromSeconds(10)));
if (completed != gradedNotification.Task)
    Console.WriteLine("Qua thoi gian cho thong bao graded.");

// (5) REST: đọc lại – trạng thái cuối luôn lấy từ nguồn sự thật
var latest = await http.GetFromJsonAsync<SubmissionDto>($"/api/submissions/{createdOne.Id}");
Console.WriteLine($"Trang thai doc lai bang REST: {latest?.Status}");

// POST lượt nộp; mã khác 201 thì in ApiError và trả null
async Task<SubmissionDto?> CreateSubmissionAsync(int assignmentId, string code, string repo)
{
    var response = await http.PostAsJsonAsync(
        $"/api/assignments/{assignmentId}/submissions",
        new CreateSubmissionRequest(code, repo));

    if (response.StatusCode != HttpStatusCode.Created)
    {
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Console.WriteLine($"Loi {(int)response.StatusCode}: {error?.Code} - {error?.Message}");
        return null;
    }
    return await response.Content.ReadFromJsonAsync<SubmissionDto>();
}

static string Short(Guid id) => id.ToString()[..8];

// Hợp đồng dữ liệu – khai báo giống máy chủ
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);

record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);

record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt,
    DateTimeOffset UpdatedAt);

record SubmissionStatusMessage(Guid SubmissionId, string Status, DateTimeOffset UpdatedAt);

record ApiError(string Code, string Message);
