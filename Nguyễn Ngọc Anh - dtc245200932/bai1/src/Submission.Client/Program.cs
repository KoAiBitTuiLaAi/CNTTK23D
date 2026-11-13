using System.Net.Http.Json;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

try
{
    // 1. Lay danh sach bai tap
    var assignments = await client.GetFromJsonAsync<List<Assignment>>("/api/assignments");
    Console.WriteLine("Danh sach bai tap:");
    if (assignments != null)
    {
        foreach (var item in assignments)
        {
            Console.WriteLine($"  - {item.Id}: {item.Title}");
        }
    }

    // 2. Nop bai voi MSSV dtc245200932
    var request = new CreateSubmissionRequest(1, "dtc245200932", "https://github.com/example/repo");
    var response = await client.PostAsJsonAsync("/api/submissions", request);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
    Console.WriteLine($"\nDa nop bai.");
    Console.WriteLine($"  Ma luot nop: {result?.Id}");
    Console.WriteLine($"  Trang thai : {result?.Status}");
}
catch (Exception ex)
{
    Console.WriteLine($"Loi: {ex.Message}");
}

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);