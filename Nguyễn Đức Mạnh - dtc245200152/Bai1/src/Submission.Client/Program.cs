// EduSubmit - Submission.Client (máy khách)
// Chạy: dotnet run --project src/Submission.Client -- http://localhost:5000

using System.Net.Http.Json;

// Địa chỉ API lấy từ tham số dòng lệnh, mặc định http://localhost:5000
var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient { BaseAddress = new Uri(apiUrl) };

// Request 1: GET /api/assignments -> in danh sách bài tập
var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments")
                  ?? new List<Assignment>();

Console.WriteLine("Danh sach bai tap:");
foreach (var a in assignments)
{
    Console.WriteLine($"- {a.Id}: {a.Title}");
}

// Request 2: POST /api/submissions -> nộp bài
var request = new CreateSubmissionRequest(
    AssignmentId: 1,
    StudentCode: "SV001",
    RepositoryUrl: "https://github.com/sv001/EduSubmit");

var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode(); // kiểm tra mã trạng thái trước khi đọc dữ liệu

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da nop bai. Ma luot nop: {result?.Id}");
Console.WriteLine($"Trang thai: {result?.Status}");

// Client tự khai báo kiểu dữ liệu, không dùng chung code với API
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
