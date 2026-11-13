using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

try
{
    var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments") ?? [];
    Console.WriteLine("Danh sach bai tap dang mo:");
    foreach (var assignment in assignments.Where(item => item.IsOpen))
    {
        Console.WriteLine($"- {assignment.Id}: {assignment.Title}");
    }

    var selectedAssignment = assignments.FirstOrDefault(item => item.IsOpen);
    if (selectedAssignment is null)
    {
        Console.WriteLine("Hien khong co bai tap dang mo.");
        return;
    }

    using var createResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignment.Id}/submissions",
        new CreateSubmissionRequest("SV001", "https://github.com/SV001/rest-api"));
    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        Console.WriteLine($"Nop bai that bai: {(int)createResponse.StatusCode} {createResponse.StatusCode}");
        await PrintApiErrorAsync(createResponse);
        return;
    }

    var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    if (created is null)
    {
        Console.WriteLine("Khong doc duoc phan hoi tao luot nop.");
        return;
    }

    Console.WriteLine($"Da tao luot nop: {created.Id}");
    Console.WriteLine($"Trang thai ban dau: {created.Status}");

    using var readResponse = await http.GetAsync($"/api/submissions/{created.Id}");
    if (!readResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Doc lai that bai: {(int)readResponse.StatusCode} {readResponse.StatusCode}");
        await PrintApiErrorAsync(readResponse);
        return;
    }

    var readBack = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"Doc lai luot nop: {readBack?.StudentCode} - {readBack?.RepositoryUrl}");

    using var invalidResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignment.Id}/submissions",
        new CreateSubmissionRequest("", ""));
    Console.WriteLine($"Thu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    await PrintApiErrorAsync(invalidResponse);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Loi ket noi API ({apiBaseUrl}): {ex.Message}");
}

static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode)
    {
        return;
    }

    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    if (error is not null)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
}

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
