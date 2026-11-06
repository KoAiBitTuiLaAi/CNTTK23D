using System.Net.Http.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiUrl), Timeout = TimeSpan.FromSeconds(10) };

var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var assignment in assignments ?? [])
    Console.WriteLine($"  - {assignment.Id}: {assignment.Title}");

var request = new CreateSubmissionRequest(1, StudentCode: "SV001",
    RepositoryUrl: "https://example.com/edusubmit.git");
using var response = await http.PostAsJsonAsync("/api/submissions", request);
Console.WriteLine($"HTTP POST: {(int)response.StatusCode} {response.StatusCode}");
response.EnsureSuccessStatusCode();
var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da nop bai. Ma luot nop: {result!.Id}");
Console.WriteLine($"Trang thai: {result.Status}");

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
