using System.Net.Http.Json;

var apiUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var client = new HttpClient
{
    BaseAddress = new Uri(apiUrl)
};

// Lấy danh sách bài tập
var assignments =
    await client.GetFromJsonAsync<List<Assignment>>("/api/assignments");

Console.WriteLine("Danh sach bai tap:");

if (assignments is not null)
{
    foreach (var assignment in assignments)
    {
        Console.WriteLine($"- {assignment.Id}: {assignment.Title}");
    }
}

// Gửi bài nộp
var request = new CreateSubmissionRequest(
    1,
    "DTC245200405",
    "https://github.com/stickman006/CNTTK23D"
);

var response =
    await client.PostAsJsonAsync("/api/submissions", request);

response.EnsureSuccessStatusCode();

var result =
    await response.Content.ReadFromJsonAsync<SubmissionResult>();

if (result is not null)
{
    Console.WriteLine($"Da nop bai. Ma luot nop: {result.Id}");
    Console.WriteLine($"Trang thai: {result.Status}");
    var saved = await client.GetFromJsonAsync<SubmissionResult>(
        $"/api/submissions/{result.Id}");
    Console.WriteLine($"Doc lai luot nop: {saved?.Id}");
    Console.WriteLine($"Trang thai doc lai: {saved?.Status}");
}

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
