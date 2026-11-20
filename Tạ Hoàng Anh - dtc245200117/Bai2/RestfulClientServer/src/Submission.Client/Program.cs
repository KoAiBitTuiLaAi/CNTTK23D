using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

try
{
    using var assignmentsResponse = await http.GetAsync("/api/assignments");
    if (!assignmentsResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(assignmentsResponse);
        return;
    }

    var assignments = await assignmentsResponse.Content.ReadFromJsonAsync<List<AssignmentDto>>() ?? [];
    var openAssignments = assignments.Where(assignment => assignment.IsOpen).ToList();
    Console.WriteLine("Danh sach bai tap dang mo:");
    foreach (var assignment in openAssignments)
    {
        Console.WriteLine($"- {assignment.Id}: {assignment.Title}");
    }

    var selectedAssignment = openAssignments.FirstOrDefault();
    if (selectedAssignment is null)
    {
        Console.WriteLine("Khong co bai tap dang mo.");
        return;
    }

    var submissionsPath = $"/api/assignments/{selectedAssignment.Id}/submissions";
    using var createResponse = await http.PostAsJsonAsync(
        submissionsPath, new CreateSubmissionRequest("SV001", "https://example.com/sv001/rest-api"));
    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        await PrintApiErrorAsync(createResponse);
        return;
    }

    var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    var location = createResponse.Headers.Location;
    if (created is null || location is null)
    {
        Console.WriteLine("Phan hoi tao luot nop thieu du lieu hoac Location.");
        return;
    }

    Console.WriteLine($"Da tao luot nop: {created.Id}");
    Console.WriteLine($"Trang thai ban dau: {created.Status}");

    using var readResponse = await http.GetAsync(location);
    if (!readResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(readResponse);
        return;
    }

    var submission = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    if (submission is null)
    {
        Console.WriteLine("Khong doc duoc luot nop.");
        return;
    }
    Console.WriteLine($"Doc lai luot nop: {submission.StudentCode} - {submission.RepositoryUrl}");

    using var invalidResponse = await http.PostAsJsonAsync(
        submissionsPath, new CreateSubmissionRequest("", ""));
    Console.WriteLine($"Thu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    if (!invalidResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(invalidResponse);
    }
}
catch (HttpRequestException exception)
{
    Console.WriteLine($"Loi ket noi API: {exception.Message}");
}

static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine(error is null
        ? $"Loi HTTP: {(int)response.StatusCode} {response.StatusCode}"
        : $"{error.Code}: {error.Message}");
}

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
