using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var assignment in assignments ?? new List<Assignment>())
{
    Console.WriteLine($"- {assignment.Id}: {assignment.Title}");
}

var request = new CreateSubmissionRequest(
    1,
    "DTC245200117",
    "https://github.com/tahoanganh222222-hash/CNTTK23D"
);

using var response = await http.PostAsJsonAsync("/api/submissions", request);
response.EnsureSuccessStatusCode();
var submission = await response.Content.ReadFromJsonAsync<SubmissionResult>();
Console.WriteLine($"Da nop bai. Ma luot nop: {submission?.Id}");
Console.WriteLine($"Trang thai: {submission?.Status}");

if (submission is not null)
{
    var savedSubmission = await http.GetFromJsonAsync<SubmissionResult>(
        $"/api/submissions/{submission.Id}"
    );
    Console.WriteLine($"Doc lai luot nop: {savedSubmission?.Id}");
    Console.WriteLine($"Trang thai doc lai: {savedSubmission?.Status}");
}

record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt
);
