using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

try
{
    // Nac 1: liet ke cac bai tap dang mo.
    var assignmentsResponse = await http.GetAsync("/api/assignments");
    if (!assignmentsResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(assignmentsResponse);
        return;
    }

    var assignments = await assignmentsResponse.Content
        .ReadFromJsonAsync<List<AssignmentDto>>() ?? [];

    var openAssignments = assignments.Where(item => item.IsOpen).ToList();

    Console.WriteLine("Danh sach bai tap dang mo:");
    foreach (var assignment in openAssignments)
    {
        Console.WriteLine($"- {assignment.Id}: {assignment.Title}");
    }

    // Nac 2: nop bai va chi doc SubmissionDto sau khi nhan 201 Created.
    var selectedAssignmentId = openAssignments.FirstOrDefault()?.Id ?? 1;
    var createResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignmentId}/submissions",
        new CreateSubmissionRequest(
            "dtc245200173",
            "https://github.com/Huiiii26/CNTTK23D"));

    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        await PrintApiErrorAsync(createResponse);
        return;
    }

    var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    var location = createResponse.Headers.Location;

    Console.WriteLine($"\nDa tao luot nop: {created?.Id}");
    Console.WriteLine($"Trang thai ban dau: {created?.Status}");
    Console.WriteLine($"Location: {location}");

    // Nac 3: doc lai dung dia chi trong header Location.
    if (location is null)
    {
        Console.WriteLine("API khong tra header Location.");
        return;
    }

    var detailResponse = await http.GetAsync(location);
    if (!detailResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(detailResponse);
        return;
    }

    var detail = await detailResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine(
        $"Doc lai luot nop: {detail?.StudentCode} - {detail?.RepositoryUrl}");

    // Nac 4: gui sai de chung minh Client doc ApiError va khong bi sap.
    var invalidResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignmentId}/submissions",
        new CreateSubmissionRequest("", ""));

    Console.WriteLine(
        $"\nThu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    await PrintApiErrorAsync(invalidResponse);
}
catch (HttpRequestException exception)
{
    Console.WriteLine($"Khong ket noi duoc API tai {apiBaseUrl}.");
    Console.WriteLine($"Loi mang: {exception.Message}");
}

static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    try
    {
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Console.WriteLine($"{error?.Code}: {error?.Message}");
    }
    catch (JsonException)
    {
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(
            $"HTTP {(int)response.StatusCode} {response.StatusCode}: {content}");
    }
}

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);

record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);

record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt);

record ApiError(string Code, string Message);
