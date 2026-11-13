// EduSubmit - Submission.Client (máy khách)
// Chạy: dotnet run --project src/Submission.Client -- http://localhost:5000

using System.Net.Http.Json;

// 1. Địa chỉ API lấy từ tham số, không viết cứng
var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

// 2. HttpClient trỏ tới API
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// 3. GET danh sách bài tập
var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var a in assignments ?? new List<Assignment>())
{
    Console.WriteLine($"  - {a.Id}: {a.Title}");
}

// 4. POST một lượt nộp
var request = new CreateSubmissionRequest(
    AssignmentId: 1,
    StudentCode: "dtc245200279",
    RepositoryUrl: "https://github.com/PTUDPTK23/CNTTK23D");

var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode(); // mã 4xx/5xx sẽ ném HttpRequestException

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da nop bai. Ma luot nop: {result?.Id}");
Console.WriteLine($"Trang thai: {result?.Status}");

// Mở rộng: đọc lại lượt nộp vừa tạo theo Id
var saved = await http.GetFromJsonAsync<SubmissionResult>($"/api/submissions/{result?.Id}");
Console.WriteLine($"Doc lai luot nop: {saved?.Id} | bai {saved?.AssignmentId} | {saved?.StudentCode} | {saved?.Status} | {saved?.ReceivedAt:u}");

// 5. Client tự khai báo hợp đồng dữ liệu, không tham chiếu project API
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
