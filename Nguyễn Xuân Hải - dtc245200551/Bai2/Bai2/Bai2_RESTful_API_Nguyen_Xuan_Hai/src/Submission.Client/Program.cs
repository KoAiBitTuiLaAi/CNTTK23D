using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

Console.WriteLine("=== Submission.Client ===");
Console.WriteLine($"API: {apiBaseUrl}");

// NAC 1: Liet ke bai tap.
Console.WriteLine("\n[1] Danh sach bai tap dang co:");
try
{
    using var listResponse = await http.GetAsync("/api/assignments");
    Console.WriteLine($"HTTP {(int)listResponse.StatusCode} {listResponse.StatusCode}");
    if (listResponse.IsSuccessStatusCode)
    {
        var assignments = await listResponse.Content.ReadFromJsonAsync<List<AssignmentDto>>(jsonOptions);
        if (assignments is not null)
        {
            foreach (var item in assignments)
                Console.WriteLine($"- {item.Id}: {item.Title} | Mo: {item.IsOpen}");
        }
    }
    else
    {
        await PrintApiErrorAsync(listResponse, jsonOptions);
    }

    // NAC 2: Gui bai hop le cho assignment 1.
    Console.WriteLine("\n[2] Gui bai tap hop le:");
    var request = new CreateSubmissionRequest(
        "dtc245200551",
        "https://github.com/example/student-assignment");
    using var postResponse = await http.PostAsJsonAsync(
        "/api/assignments/1/submissions", request, jsonOptions);

    Console.WriteLine($"HTTP {(int)postResponse.StatusCode} {postResponse.StatusCode}");
    SubmissionDto? createdSubmission = null;

    if (postResponse.StatusCode == HttpStatusCode.Created)
    {
        createdSubmission = await postResponse.Content.ReadFromJsonAsync<SubmissionDto>(jsonOptions);
        Console.WriteLine($"Da tao luot nop: {createdSubmission?.Id}");
        Console.WriteLine($"Trang thai ban dau: {createdSubmission?.Status}");
        Console.WriteLine($"Location: {postResponse.Headers.Location}");
    }
    else
    {
        await PrintApiErrorAsync(postResponse, jsonOptions);
    }

    // NAC 3: Doc lai luot nop vua tao.
    if (createdSubmission is not null)
    {
        Console.WriteLine("\n[3] Doc lai luot nop:");
        using var readResponse = await http.GetAsync($"/api/submissions/{createdSubmission.Id}");
        Console.WriteLine($"HTTP {(int)readResponse.StatusCode} {readResponse.StatusCode}");
        if (readResponse.IsSuccessStatusCode)
        {
            var readBack = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>(jsonOptions);
            Console.WriteLine($"Doc lai: {readBack?.StudentCode} - {readBack?.RepositoryUrl}");
        }
        else
        {
            await PrintApiErrorAsync(readResponse, jsonOptions);
        }
    }

    // NAC 4: Co y gui du lieu sai va doc ApiError.
    Console.WriteLine("\n[4] Thu gui yeu cau loi:");
    using var badResponse = await http.PostAsJsonAsync(
        "/api/assignments/1/submissions",
        new CreateSubmissionRequest("", ""),
        jsonOptions);
    Console.WriteLine($"HTTP {(int)badResponse.StatusCode} {badResponse.StatusCode}");
    await PrintApiErrorAsync(badResponse, jsonOptions);
}
catch (HttpRequestException ex)
{
    Console.WriteLine("Loi ket noi: khong nhan duoc phan hoi HTTP.");
    Console.WriteLine(ex.Message);
    Console.WriteLine("Hay kiem tra Submission.Api da chay o cong 5000 chua.");
}

static async Task PrintApiErrorAsync(HttpResponseMessage response, JsonSerializerOptions options)
{
    var error = await response.Content.ReadFromJsonAsync<ApiError>(options);
    if (error is not null)
        Console.WriteLine($"{error.Code}: {error.Message}");
    else
        Console.WriteLine("Phan hoi loi khong co ApiError JSON.");
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
