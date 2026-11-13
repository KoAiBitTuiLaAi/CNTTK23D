using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");
Console.WriteLine("Danh sach bai tap dang mo:");
foreach (var a in assignments ?? [])
    Console.WriteLine($"- {a.Id}: {a.Title}");

var selectedAssignmentId = assignments?.FirstOrDefault()?.Id ?? 1;
var createResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("SV001", "https://example.com/sv001/rest-api"));
if (createResponse.StatusCode != HttpStatusCode.Created)
{
    await PrintApiErrorAsync(createResponse);
    return;
}

var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
Console.WriteLine($"Da tao luot nop: {created?.Id}");
Console.WriteLine($"Trang thai ban dau: {created?.Status}");

var detailResponse = await http.GetAsync(createResponse.Headers.Location);
if (!detailResponse.IsSuccessStatusCode)
{
    await PrintApiErrorAsync(detailResponse);
    return;
}
var detail = await detailResponse.Content.ReadFromJsonAsync<SubmissionDto>();
Console.WriteLine($"Doc lai luot nop: {detail?.StudentCode} - {detail?.RepositoryUrl}");

var invalidResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("", ""));
Console.WriteLine($"Thu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
await PrintApiErrorAsync(invalidResponse);

static async Task PrintApiErrorAsync(HttpResponseMessage r)
{
    if (r.IsSuccessStatusCode) return;
    var error = await r.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"{error?.Code}: {error?.Message}");
}

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl,
    string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
