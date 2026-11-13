using System.Net.Http.Json;

var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient { BaseAddress = new Uri(apiUrl) };

// 1. Lấy danh sách bài tập
var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var a in assignments!)
{
    Console.WriteLine($"  - {a.Id}: {a.Title}");
}

// 2. Nộp bài
var request = new CreateSubmissionRequest(1, "SV001", "https://github.com/DTC245200643/EduSubmit-DTC245200643");
var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da nop bai. Ma luot nop: {result!.Id}");
Console.WriteLine($"  Trang thai: {result.Status}");

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);