using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

// Nấc 1: Liệt kê danh sách bài tập đang mở
var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");
Console.WriteLine("Danh sach bai tap dang mo:");
foreach (var a in (assignments ?? []).Where(a => a.IsOpen))
{
    Console.WriteLine($" - {a.Id}: {a.Title}");
}

// Nấc 2: Nộp bài và kiểm tra mã trạng thái 201 Created trước khi đọc body
var selectedAssignmentId = assignments?.FirstOrDefault(a => a.IsOpen)?.Id ?? 1;

var createResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("DTC245200697", "https://github.com/PTUDPTK23/CNTTK23D"));

if (createResponse.StatusCode != HttpStatusCode.Created)
{
    await PrintApiErrorAsync(createResponse);
    return;
}

var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
Console.WriteLine($"\nDa tao luot nop: {created?.Id}");
Console.WriteLine($"Trang thai ban dau: {created?.Status}");

// Nấc 3: Đọc lại lượt nộp theo đúng địa chỉ trong header Location
var location = createResponse.Headers.Location;
if (location is not null)
{
    var detailResponse = await http.GetAsync(location);
    if (!detailResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(detailResponse);
        return;
    }

    var fetched = await detailResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"Doc lai luot nop: {fetched?.StudentCode} - {fetched?.RepositoryUrl}");
}

// Nấc 4: Cố ý gửi yêu cầu sai để quan sát lỗi 400 BadRequest và đọc ApiError
var invalidResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("", ""));

Console.WriteLine($"\nThu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
await PrintApiErrorAsync(invalidResponse);

static async Task PrintApiErrorAsync(HttpResponseMessage r)
{
    if (r.IsSuccessStatusCode) return;
    var error = await r.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"{error?.Code}: {error?.Message}");
}

// Các record bản sao DTO phía Client
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
