using System.Net;
using System.Net.Http.Json;

// Địa chỉ API lấy từ dòng lệnh, không gán cứng (Bước 7)
var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// Nấc 1: liệt kê bài tập đang mở
var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");
var openAssignments = (assignments ?? []).Where(a => a.IsOpen).ToList();
Console.WriteLine("Danh sach bai tap dang mo:");
foreach (var a in openAssignments)
    Console.WriteLine($"- {a.Id}: {a.Title}");

// Nấc 2: nộp bài, xem mã trạng thái TRƯỚC khi đọc body
var selectedAssignmentId = openAssignments.FirstOrDefault()?.Id ?? 1;
var createResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("SV001", "https://example.com/sv001/rest-api"));

if (createResponse.StatusCode != HttpStatusCode.Created)
{
    await PrintApiErrorAsync(createResponse);
    return;
}

var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
Console.WriteLine($"\nDa tao luot nop: {created?.Id}");
Console.WriteLine($"Trang thai ban dau: {created?.Status}");

// Nấc 3: đọc lại lượt nộp theo đúng địa chỉ máy chủ trả trong header Location
var location = createResponse.Headers.Location;
var readResponse = await http.GetAsync(location);
if (readResponse.IsSuccessStatusCode)
{
    var readBack = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"Doc lai luot nop {readBack?.Id}: {readBack?.StudentCode} - {readBack?.RepositoryUrl}");
}
else
{
    await PrintApiErrorAsync(readResponse);
}

// Nấc 4: cố ý gửi dữ liệu sai, máy khách không sập mà in lỗi dễ hiểu
var invalidResponse = await http.PostAsJsonAsync(
    $"/api/assignments/{selectedAssignmentId}/submissions",
    new CreateSubmissionRequest("", ""));
Console.WriteLine($"\nThu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
await PrintApiErrorAsync(invalidResponse);

// Một hàm cho mọi lỗi, nhờ máy chủ trả cùng một dạng ApiError
static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode) return;
    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"{error?.Code}: {error?.Message}");
}

// Bản sao DTO: hai bên không chung code, chỉ chung hợp đồng JSON
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl,
    string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
