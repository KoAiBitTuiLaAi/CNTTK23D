using System.Net.Http.Json;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseAddress)
    || (baseAddress.Scheme != Uri.UriSchemeHttp && baseAddress.Scheme != Uri.UriSchemeHttps)
    || string.IsNullOrWhiteSpace(baseAddress.Host))
{
    Console.Error.WriteLine("Lỗi: URL API phải là địa chỉ HTTP/HTTPS tuyệt đối.");
    return 1;
}
try
{
    using var http = new HttpClient(new HttpStatusHandler())
    {
        BaseAddress = baseAddress,
        Timeout = TimeSpan.FromSeconds(10)
    };
    Console.WriteLine($"API: {http.BaseAddress}");
    var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments")
        ?? throw new InvalidOperationException("API không trả về danh sách bài tập.");
    Console.WriteLine($"Danh sách bài tập ({assignments.Count}):");
    foreach (var assignment in assignments)
        Console.WriteLine($"{assignment.Id}: {assignment.Title} — {assignment.Description}");
    var request = new CreateSubmissionRequest(assignments.First().Id, "DTC245200604",
        "https://github.com/vietdotest/EduSubmit-DTC245200604");
    using var response = await http.PostAsJsonAsync("/api/submissions", request);
    response.EnsureSuccessStatusCode();
    var created = await response.Content.ReadFromJsonAsync<SubmissionResult>()
        ?? throw new InvalidOperationException("API không trả về kết quả nộp bài.");
    Console.WriteLine($"MSSV: {created.StudentCode}");
    Console.WriteLine($"Kho mã: {created.RepositoryUrl}");
    Console.WriteLine($"ID: {created.Id}");
    Console.WriteLine($"Trạng thái: {created.Status}");
    Console.WriteLine($"UTC: {created.ReceivedAt.UtcDateTime:O}");
    var location = response.Headers.Location
        ?? throw new InvalidOperationException("API thiếu Location của bài nộp mới.");
    var retrieved = await http.GetFromJsonAsync<SubmissionResult>(location)
        ?? throw new InvalidOperationException("Không đọc được bài nộp vừa tạo.");
    if (retrieved != created)
        throw new InvalidOperationException("Dữ liệu đọc lại khác kết quả tạo bài nộp.");
    Console.WriteLine($"Đọc lại: {location}");
    Console.WriteLine("Đối chiếu dữ liệu: khớp");
    return 0;
}
catch (HttpRequestException error)
{
    Console.Error.WriteLine($"Lỗi HTTP: {error.Message}");
    return 1;
}

// Ghi mã trạng thái từ phản hồi thật, kể cả phản hồi lỗi.
sealed class HttpStatusHandler : DelegatingHandler
{
    public HttpStatusHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine($"HTTP {request.Method} {request.RequestUri!.AbsolutePath} -> {(int)response.StatusCode}");
        return response;
    }
}

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
