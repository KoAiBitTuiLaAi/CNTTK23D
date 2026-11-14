using System.Net.Http.Json;

// Địa chỉ API
var apiUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

// Tạo HttpClient
using var client = new HttpClient
{
    BaseAddress = new Uri(apiUrl)
};

Console.WriteLine("=== SUBMISSION CLIENT ===");

// 1. Lấy danh sách bài tập từ API
Console.WriteLine("\nDanh sach bai tap:");

var assignments = await client.GetFromJsonAsync<List<Assignment>>(
    "/api/assignments"
);

if (assignments != null)
{
    foreach (var assignment in assignments)
    {
        Console.WriteLine(
            $"{assignment.Id}. {assignment.Title} - {assignment.Description}"
        );
    }
}

// 2. Tạo thông tin bài nộp
var request = new CreateSubmissionRequest(
    1,
    "DTC245200697",
    "https://github.com/"
);

// 3. Gửi bài nộp đến API
Console.WriteLine("\nDang gui bai nop...");

var response = await client.PostAsJsonAsync(
    "/api/submissions",
    request
);

response.EnsureSuccessStatusCode();

// 4. Nhận kết quả từ API
var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();

if (result != null)
{
    Console.WriteLine("\n=== KET QUA NOP BAI ===");
    Console.WriteLine($"Submission ID: {result.Id}");
    Console.WriteLine($"Assignment ID: {result.AssignmentId}");
    Console.WriteLine($"Student Code: {result.StudentCode}");
    Console.WriteLine($"Status: {result.Status}");
    Console.WriteLine($"Received At: {result.ReceivedAt}");
}

// Các kiểu dữ liệu phía Client
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl
);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt
);