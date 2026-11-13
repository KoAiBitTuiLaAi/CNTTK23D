using System.Net.Http.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var client = new HttpClient { BaseAddress = new Uri(apiUrl), Timeout = TimeSpan.FromSeconds(5) };

try
{
    var assignments = await client.GetFromJsonAsync<List<Assignment>>("/api/assignments");
    Console.WriteLine("Danh sach bai tap:");
    foreach (var assignment in assignments ?? [])
        Console.WriteLine($"  - {assignment.Id}: {assignment.Title}");

    var request = new CreateSubmissionRequest(1, StudentCode: "SV001",
        RepositoryUrl: "https://github.com/example/EduSubmit");
    using var response = await client.PostAsJsonAsync("/api/submissions", request);
    Console.WriteLine($"POST /api/submissions -> {(int)response.StatusCode} {response.ReasonPhrase}");
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
    Console.WriteLine($"Da nop bai. Ma luot nop: {result!.Id}");
    Console.WriteLine($"Trang thai: {result.Status}");
    return 0;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"HttpRequestException: {exception.Message}");
    return 1;
}

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
