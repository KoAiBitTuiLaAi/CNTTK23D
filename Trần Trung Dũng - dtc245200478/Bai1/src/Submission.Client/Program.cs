using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var assignment in assignments ?? [])
    Console.WriteLine($"- {assignment.Id}: {assignment.Title}");

var request = new CreateSubmissionRequest(
    1, "SV001", "https://example.com/sv001/bai-tap-1");
using var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>()
    ?? throw new InvalidOperationException("API khong tra ve du lieu luot nop.");
Console.WriteLine($"Da nop bai. Ma luot nop: {result.Id}");
Console.WriteLine($"Trang thai: {result.Status}");

using var lookupResponse = await http.GetAsync($"/api/submissions/{result.Id}");
lookupResponse.EnsureSuccessStatusCode();
var saved = await lookupResponse.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da doc lai luot nop: {saved?.Id} | {saved?.Status}");

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);
