using System.Net;
using System.Net.Http.Json;

// EduSubmit – Submission.Client (Bài 2)
// Chạy: dotnet run --project src/Submission.Client -- http://localhost:5000

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// Nấc 1: lấy danh sách bài tập đang mở
var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");
var openAssignments = (assignments ?? []).Where(a => a.IsOpen).ToList();
Console.WriteLine("Danh sach bai tap dang mo:");
foreach (var a in openAssignments)
{
    Console.WriteLine($"- {a.Id}: {a.Title}");
}

// Nấc 2: nộp bài, xem mã 201 trước rồi mới đọc body
var selectedAssignmentId = openAssignments.FirstOrDefault()?.Id ?? 1;

var createResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("DTC245200730", "https://github.com/KoAiBitTuiLaAi/CNTTK23D"));

if (createResponse.StatusCode != HttpStatusCode.Created)
{
    await PrintApiErrorAsync(createResponse);
    return;
}

var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
Console.WriteLine($"\nDa tao luot nop: {created?.Id}");
Console.WriteLine($"Trang thai ban dau: {created?.Status}");

// Nấc 3: đọc lại lượt nộp theo header Location
var location = createResponse.Headers.Location?.ToString() ?? $"/api/submissions/{created?.Id}";
var detailResponse = await http.GetAsync(location);
if (detailResponse.StatusCode == HttpStatusCode.OK)
{
    var saved = await detailResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"Doc lai luot nop {saved?.Id}: {saved?.StudentCode} - {saved?.RepositoryUrl}");
}
else
{
    await PrintApiErrorAsync(detailResponse);
}

// Nấc 4: cố ý gửi dữ liệu sai để quan sát lỗi 400
var invalidResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("", ""));
Console.WriteLine($"\nThu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
await PrintApiErrorAsync(invalidResponse);

// Một hàm đọc mọi lỗi, nhờ API trả cùng một dạng ApiError
static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode) return;
    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"{error?.Code}: {error?.Message}");
}

// Client tự khai báo hợp đồng dữ liệu, không dùng chung code với API
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
