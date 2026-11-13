using System.Net.Http.Json;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5050";
using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine($"Dang ket noi toi API tai: {baseUrl}");

// 1. Gọi GET /api/assignments lấy danh sách bài tập
var assignments = await client.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
if (assignments != null)
{
    foreach (var a in assignments)
    {
        Console.WriteLine($"  - {a.Id}: {a.Title} - {a.Description}");
    }
}

// 2. Gọi POST /api/submissions nộp bài
var request = new CreateSubmissionRequest(1, "SV123456", "https://github.com/user/repo");
var response = await client.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine("\nDa nop bai thanh cong!");
Console.WriteLine($"Ma luot nop: {result?.Id}");
Console.WriteLine($"Trang thai:  {result?.Status}");

// 3. Bài mở rộng: Gọi GET /api/submissions/{id} để đọc lại bài nộp vừa tạo
if (result != null)
{
    Console.WriteLine($"\n[Client-MoRong] Dang truy van lai luot nop ID: {result.Id}...");
    var fetchedSub = await client.GetFromJsonAsync<SubmissionResult>($"/api/submissions/{result.Id}");
    Console.WriteLine($"[Client-MoRong] Thoi gian nhan bai: {fetchedSub?.ReceivedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"[Client-MoRong] Trang thai xac nhan: {fetchedSub?.Status}");
}

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);