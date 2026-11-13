using System.Net.Http.Json;

// EduSubmit – Submission.Client (vai trò MÁY KHÁCH)
// Chạy: dotnet run --project src/Submission.Client -- http://localhost:5000

// 1. Địa chỉ API lấy từ args[0], mặc định http://localhost:5000
var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

// 2. HttpClient trỏ tới API
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// 3. GET /api/assignments
var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var assignment in assignments ?? [])
{
    Console.WriteLine($"- {assignment.Id}: {assignment.Title} ({assignment.Description})");
}

// 4. POST /api/submissions
var request = new CreateSubmissionRequest(
    AssignmentId: 1,
    StudentCode: "DTC245200730",
    RepositoryUrl: "https://example.com/dtc245200730/bai-tap-1");

var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode(); // kiểm tra mã trạng thái trước khi đọc body

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>()
    ?? throw new InvalidOperationException("API khong tra ve luot nop");
Console.WriteLine($"Da nop bai. Ma luot nop: {result.Id}");
Console.WriteLine($"Trang thai: {result.Status}");

// Bước 12: đọc lại lượt nộp vừa tạo qua GET /api/submissions/{id}
// (dùng header Location mà API trả về; nếu thiếu thì tự ghép URL từ Id)
var location = response.Headers.Location?.ToString() ?? $"/api/submissions/{result.Id}";
var saved = await http.GetFromJsonAsync<SubmissionResult>(location);
Console.WriteLine("Doc lai luot nop tu API:");
Console.WriteLine($"- Id: {saved?.Id}");
Console.WriteLine($"- Bai tap: {saved?.AssignmentId}");
Console.WriteLine($"- Sinh vien: {saved?.StudentCode}");
Console.WriteLine($"- Trang thai: {saved?.Status}");
Console.WriteLine($"- Nhan luc: {saved?.ReceivedAt:yyyy-MM-dd HH:mm:ss} UTC");

// Client tự khai báo hợp đồng dữ liệu, không dùng chung code với API
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
