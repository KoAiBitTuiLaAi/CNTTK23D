// EduSubmit realtime – Submission.RealtimeClient (Bài 3: REST + SignalR)
// Chạy: dotnet run --project src/Submission.RealtimeClient -- http://localhost:5000 SV001
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

var apiBaseUrl = args.Length > 0 ? args[0].TrimEnd('/') : "http://localhost:5000";
var studentCode = args.Length > 1 ? args[1] : "SV001";

// Kênh 1: REST – tạo và đọc tài nguyên
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// Kênh 2: SignalR – nhận thay đổi do máy chủ đẩy về
await using var connection = new HubConnectionBuilder()
    .WithUrl($"{apiBaseUrl}/hubs/submissions")
    .WithAutomaticReconnect()
    .Build();

var gradedNotification = new TaskCompletionSource(); // "cái chuông" reo khi nhận graded

// Đăng ký nghe TRƯỚC khi mở kết nối; tên phải khớp SendAsync ở máy chủ
connection.On<SubmissionStatusMessage>("SubmissionStatusChanged", m =>
{
    Console.WriteLine($"Thong bao SignalR: {m.SubmissionId} -> {m.Status} luc {m.UpdatedAt.ToLocalTime():HH:mm:ss}");
    if (m.Status == "graded") gradedNotification.TrySetResult();
});

await connection.StartAsync();
Console.WriteLine("Da ket noi SignalR.");

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
