using System.Net.Http.Json;

var apiAddress = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var client = new HttpClient
{
    BaseAddress = new Uri(apiAddress)
};

try
{
    var assignments = await client.GetFromJsonAsync<List<Assignment>>(
        "/api/assignments");

    Console.WriteLine("Danh sach bai tap:");
    foreach (var assignment in assignments ?? [])
    {
        Console.WriteLine(
            $"  - {assignment.Id}: {assignment.Title} - {assignment.Description}");
    }

    var request = new CreateSubmissionRequest(
        AssignmentId: 1,
        StudentCode: "dtc245200173",
        RepositoryUrl: "https://github.com/Huiiii26/CNTTK23D");

    using var response = await client.PostAsJsonAsync(
        "/api/submissions",
        request);
    response.EnsureSuccessStatusCode();

    var created = await response.Content.ReadFromJsonAsync<SubmissionResult>()
        ?? throw new InvalidOperationException("API did not return a submission.");

    Console.WriteLine();
    Console.WriteLine($"Da nop bai. Ma luot nop: {created.Id}");
    Console.WriteLine($"Trang thai: {created.Status}");

    var saved = await client.GetFromJsonAsync<SubmissionResult>(
        $"/api/submissions/{created.Id}")
        ?? throw new InvalidOperationException("Could not read the saved submission.");

    Console.WriteLine();
    Console.WriteLine("Doc lai luot nop vua tao:");
    Console.WriteLine($"  Ma luot nop: {saved.Id}");
    Console.WriteLine($"  MSSV: {saved.StudentCode}");
    Console.WriteLine($"  Trang thai: {saved.Status}");
    Console.WriteLine($"  Thoi gian nhan: {saved.ReceivedAt:O}");
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"Khong the goi API tai {apiAddress}.");
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
