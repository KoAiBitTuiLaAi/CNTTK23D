using System.Net.Http.Json;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseAddress)
    || (baseAddress.Scheme != Uri.UriSchemeHttp && baseAddress.Scheme != Uri.UriSchemeHttps))
{
    Console.Error.WriteLine("Dia chi API phai la URL HTTP hoac HTTPS hop le.");
    return 1;
}

using var client = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(10) };
try
{
    var assignments = await client.GetFromJsonAsync<List<Assignment>>("/api/assignments")
        ?? throw new InvalidOperationException("API khong tra ve danh sach bai tap.");
    Console.WriteLine("Danh sach bai tap:");
    foreach (var assignment in assignments)
        Console.WriteLine($"  - {assignment.Id}: {assignment.Title}");

    var request = new CreateSubmissionRequest(1, "DTC245200318",
        "https://github.com/anhphann1412-max/CNTTK23D");
    using var response = await client.PostAsJsonAsync("/api/submissions", request);
    Console.WriteLine($"POST /api/submissions -> {(int)response.StatusCode} {response.ReasonPhrase}");
    response.EnsureSuccessStatusCode();
    var submission = await response.Content.ReadFromJsonAsync<SubmissionResult>()
        ?? throw new InvalidOperationException("API khong tra ve luot nop.");
    Console.WriteLine($"Da nop bai. Ma luot nop: {submission.Id}");
    Console.WriteLine($"Trang thai: {submission.Status}");

    var saved = await client.GetFromJsonAsync<SubmissionResult>($"/api/submissions/{submission.Id}")
        ?? throw new InvalidOperationException("Khong doc duoc luot nop vua tao.");
    Console.WriteLine($"GET /api/submissions/{submission.Id} -> 200 OK");
    Console.WriteLine($"Doc lai: {saved.Id} | {saved.StudentCode} | {saved.Status}");
    return 0;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"Loi HTTP: {exception.Message}");
    Console.Error.WriteLine("Kiem tra API da chay va URL/cong da dung chua.");
    return 1;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine("Qua thoi gian cho API (10 giay).");
    return 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

// Client tự khai báo hợp đồng JSON, không tham chiếu project API.
record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
