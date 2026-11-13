using System.Net.Http.Json;

var apiAddress = args.Length > 0 ? args[0] : "http://localhost:5000";
using var httpClient = new HttpClient
{
    BaseAddress = new Uri(apiAddress)
};

var assignments = await httpClient.GetFromJsonAsync<List<Assignment>>("/api/assignments");
Console.WriteLine("Danh sach bai tap:");
foreach (var assignment in assignments ?? [])
{
    Console.WriteLine($"{assignment.Id}. {assignment.Title} - {assignment.Description}");
}

var request = new CreateSubmissionRequest(
    AssignmentId: 1,
    StudentCode: "SV001",
    RepositoryUrl: "https://example.com/sv001/bai-tap-1");

using var createResponse = await httpClient.PostAsJsonAsync("/api/submissions", request);
createResponse.EnsureSuccessStatusCode();

var createdSubmission = await createResponse.Content.ReadFromJsonAsync<SubmissionResult>()
    ?? throw new InvalidOperationException("API returned an empty submission response.");

Console.WriteLine($"Ma luot nop: {createdSubmission.Id}");
Console.WriteLine($"Trang thai: {createdSubmission.Status}");

var savedSubmission = await httpClient.GetFromJsonAsync<SubmissionResult>(
    $"/api/submissions/{createdSubmission.Id}")
    ?? throw new InvalidOperationException("API did not return the saved submission.");

Console.WriteLine($"Da doc lai luot nop: {savedSubmission.Id} - Trang thai: {savedSubmission.Status}");

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);
