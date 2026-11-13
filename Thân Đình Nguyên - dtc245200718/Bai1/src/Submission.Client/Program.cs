using System.Net.Http.Json;

var apiUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var client = new HttpClient
{
    BaseAddress = new Uri(apiUrl)
};

var assignments = await client
    .GetFromJsonAsync<List<Assignment>>("/api/assignments");

Console.WriteLine("Danh sach bai tap:");

foreach (var assignment in assignments ?? [])
{
    Console.WriteLine($"  - {assignment.Id}: {assignment.Title}");
}

var request = new CreateSubmissionRequest(
    1,
    "DTC245200718",
    "https://github.com/example/edusubmit"
);

var response = await client.PostAsJsonAsync(
    "/api/submissions",
    request
);

response.EnsureSuccessStatusCode();

var submission = await response.Content
    .ReadFromJsonAsync<SubmissionResult>();

if (submission is not null)
{
    Console.WriteLine($"Da nop bai. Ma luot nop: {submission.Id}");
    Console.WriteLine($"Trang thai: {submission.Status}");
    var savedSubmission = await client
    .GetFromJsonAsync<SubmissionResult>(
        $"/api/submissions/{submission.Id}"
    );

Console.WriteLine($"Doc lai luot nop: {savedSubmission?.Id}");
Console.WriteLine($"Ma sinh vien: {savedSubmission?.StudentCode}");
Console.WriteLine($"Trang thai doc lai: {savedSubmission?.Status}");
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