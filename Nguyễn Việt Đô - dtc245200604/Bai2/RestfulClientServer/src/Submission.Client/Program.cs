using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var address = args.Length == 0 ? "http://localhost:5000/" : args[0];
if (args.Length > 1 || !Uri.TryCreate(address, UriKind.Absolute, out var baseUri) ||
    (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps) ||
    baseUri.AbsolutePath != "/" || baseUri.Query.Length != 0 ||
    baseUri.Fragment.Length != 0 || baseUri.UserInfo.Length != 0)
{
    Console.Error.WriteLine("BaseAddress khong hop le. Dung URL goc http(s)://host:port/.");
    return 2;
}

using var client = new HttpClient { BaseAddress = baseUri, Timeout = TimeSpan.FromSeconds(10) };
try
{
    using var listResponse = await client.GetAsync("api/assignments");
    if (listResponse.StatusCode != HttpStatusCode.OK)
        return await PrintError(listResponse, "GET assignments");
    var assignments = await listResponse.Content.ReadFromJsonAsync<AssignmentDto[]>();
    if (assignments is null)
        throw new JsonException("Danh sach bai tap rong.");
    var open = assignments.Where(item => item.IsOpen).ToArray();
    Console.WriteLine("1. Bai tap dang mo:");
    foreach (var item in open)
        Console.WriteLine($"{item.Id}: {item.Title}");
    var assignment = open.FirstOrDefault();
    if (assignment is null)
    {
        Console.Error.WriteLine("Khong co bai tap dang mo de nop.");
        return 1;
    }

    var path = $"api/assignments/{assignment.Id}/submissions";
    var input = new CreateSubmissionRequest("DTC245200604",
        "https://github.com/vietdotest/EduSubmit-DTC245200604");
    using var post = await client.PostAsJsonAsync(path, input);
    Console.WriteLine($"2. POST: {(int)post.StatusCode}");
    // Check status before reading the successful DTO: errors have a different contract.
    if (post.StatusCode != HttpStatusCode.Created)
        return await PrintError(post, "POST submission");
    var submission = await post.Content.ReadFromJsonAsync<SubmissionDto>()
        ?? throw new JsonException("Thieu SubmissionDto.");
    var location = post.Headers.Location
        ?? throw new JsonException("Thieu header Location.");
    Console.WriteLine($"Location: {location}");
    Console.WriteLine($"Da nhan {submission.Id} - {submission.StudentCode} - {submission.Status}");

    // Use the returned Location, not a reconstructed URL.
    using var get = await client.GetAsync(location);
    Console.WriteLine($"3. GET Location: {(int)get.StatusCode}");
    if (get.StatusCode != HttpStatusCode.OK)
        return await PrintError(get, "GET Location");
    var saved = await get.Content.ReadFromJsonAsync<SubmissionDto>()
        ?? throw new JsonException("Thieu bai nop khi GET Location.");
    if (saved != submission)
        throw new JsonException("GET Location khong khop bai nop vua tao.");
    Console.WriteLine($"{saved.StudentCode} | {saved.RepositoryUrl} | {saved.ReceivedAt:O}");

    using var invalid = await client.PostAsJsonAsync(path, new CreateSubmissionRequest("", ""));
    Console.WriteLine($"4. POST rong: {(int)invalid.StatusCode}");
    if (invalid.StatusCode != HttpStatusCode.BadRequest)
        return await PrintError(invalid, "POST rong (mong doi 400)");
    var error = await invalid.Content.ReadFromJsonAsync<ApiError>()
        ?? throw new JsonException("Thieu ApiError.");
    Console.WriteLine($"{error.Code}: {error.Message}");
    return error.Code == "INVALID_SUBMISSION" ? 0 : 1;
}
catch (HttpRequestException error)
{
    Console.Error.WriteLine($"Loi ket noi HTTP: {error.Message}");
    return 3;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Het thoi gian cho phan hoi HTTP (10 giay).");
    return 3;
}
catch (JsonException error)
{
    Console.Error.WriteLine($"Phan hoi JSON khong hop le: {error.Message}");
    return 4;
}

static async Task<int> PrintError(HttpResponseMessage response, string operation)
{
    Console.Error.WriteLine($"{operation}: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
    var text = await response.Content.ReadAsStringAsync();
    if (string.IsNullOrWhiteSpace(text))
    {
        Console.Error.WriteLine("Phan hoi loi khong co body (co the do framework).");
        return 1;
    }
    try
    {
        var error = JsonSerializer.Deserialize<ApiError>(text, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Console.Error.WriteLine(error is { Code: not null, Message: not null }
            ? $"{error.Code}: {error.Message}"
            : "Body loi khong theo dang ApiError.");
    }
    catch (JsonException)
    {
        Console.Error.WriteLine("Body loi khong phai JSON ApiError.");
    }
    return 1;
}

// Local wire contracts: no ProjectReference to the API.
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl,
    string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
