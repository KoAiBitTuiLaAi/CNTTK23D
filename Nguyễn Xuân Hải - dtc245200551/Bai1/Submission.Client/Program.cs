using System.Net.Http.Json;

var apiUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiUrl)
};

var assignments = await http.GetFromJsonAsync<List<Assignment>>(
    "/api/assignments"
);

Console.WriteLine("DANH SACH BAI TAP:");

foreach (var assignment in assignments ?? new())
{
    Console.WriteLine($"{assignment.Id}. {assignment.Title}");
}

var request = new CreateSubmissionRequest(
    1,
    "dtc245200551",
    "https://github.com/dtc245200551"
);

var response = await http.PostAsJsonAsync(
    "/api/submissions",
    request
);

Console.WriteLine($"\nHTTP STATUS: {(int)response.StatusCode}");

if (!response.IsSuccessStatusCode)
{
    Console.WriteLine("Gui bai that bai: " + response.StatusCode);
}
else
{
    var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();

    Console.WriteLine("\nKET QUA NOP BAI:");
    Console.WriteLine($"Ma bai nop: {result?.Id}");
    Console.WriteLine($"Trang thai: {result?.Status}");

    if (result is not null)
    {
        var savedSubmission = await http.GetFromJsonAsync<SubmissionResult>(
            $"/api/submissions/{result.Id}"
        );

        Console.WriteLine("\nTRA CUU BAI NOP:");
        Console.WriteLine($"Ma bai nop: {savedSubmission?.Id}");
        Console.WriteLine($"Trang thai: {savedSubmission?.Status}");

        var missingId = Guid.NewGuid();
        var missingResponse = await http.GetAsync(
            $"/api/submissions/{missingId}"
        );

        Console.WriteLine(
            $"\nHTTP STATUS ID KHONG TON TAI: {(int)missingResponse.StatusCode}"
        );
    }
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
