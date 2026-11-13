// EduSubmit – Submission.Client (máy khách)
// Chạy: dotnet run --project src/Submission.Client -- http://localhost:5000

using System.Net.Http.Json;

// Địa chỉ API lấy từ tham số dòng lệnh, mặc định localhost:5000
var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient {
    BaseAddress = new Uri(apiBaseUrl)
};

// 1. GET /api/assignments
var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var assignment in assignments ?? new List<Assignment>()) {
    Console.WriteLine($"- {assignment.Id}: {assignment.Title} ({assignment.Description})");
}

// 2. POST /api/submissions
var request = new CreateSubmissionRequest(
    AssignmentId: 1,
    StudentCode: "DTC245200722",
    RepositoryUrl: "https://github.com/PTUDPTK23/CNTTK23D");

var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da nop bai. Ma luot nop: {result?.Id}");
Console.WriteLine($"Trang thai: {result?.Status}");

// 3. Bài mở rộng: GET /api/submissions/{id} để đọc lại lượt nộp vừa tạo
var saved = await http.GetFromJsonAsync<SubmissionResult>($"/api/submissions/{result?.Id}");
Console.WriteLine("Doc lai luot nop tu API:");
Console.WriteLine($"- Ma: {saved?.Id}");
Console.WriteLine($"- Bai tap: {saved?.AssignmentId}, SV: {saved?.StudentCode}");
Console.WriteLine($"- Trang thai: {saved?.Status}, nhan luc: {saved?.ReceivedAt:u}");

// Client tự khai báo hợp đồng dữ liệu, không tham chiếu code của API
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
