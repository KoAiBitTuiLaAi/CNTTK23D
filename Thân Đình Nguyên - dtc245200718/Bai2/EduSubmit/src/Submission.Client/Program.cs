using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

try
{
    // Nấc 1: đọc danh sách, chọn bài tập đang mở.
    using var listResponse = await http.GetAsync("/api/assignments");
    if (!listResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(listResponse);
        return;
    }
    var assignments = await listResponse.Content.ReadFromJsonAsync<List<AssignmentDto>>();
    Console.WriteLine("Danh sach bai tap dang mo:");
    foreach (var assignment in assignments?.Where(a => a.IsOpen) ?? [])
        Console.WriteLine($"- {assignment.Id}: {assignment.Title}");

    var selectedAssignmentId = assignments?.FirstOrDefault(a => a.IsOpen)?.Id;
    if (selectedAssignmentId is null)
    {
        Console.WriteLine("Khong co bai tap dang mo.");
        return;
    }

    // Nấc 2: tạo lượt nộp, xem mã 201 trước khi đọc JSON.
    using var createResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignmentId}/submissions",
        new CreateSubmissionRequest("DTC245200718", "https://github.com/ThanDinhNguyen/CNTTK23D"));
    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        await PrintApiErrorAsync(createResponse);
        return;
    }
    var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    if (created is null || createResponse.Headers.Location is null)
    {
        Console.WriteLine("Phan hoi tao luot nop thieu JSON hoac Location.");
        return;
    }
    Console.WriteLine($"Da tao luot nop: {created.Id}");
    Console.WriteLine($"Trang thai ban dau: {created.Status}");

    // Nấc 3: đọc đúng địa chỉ do Location trả về.
    using var readResponse = await http.GetAsync(createResponse.Headers.Location);
    if (!readResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(readResponse);
        return;
    }
    var saved = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"Doc lai luot nop: {saved?.StudentCode} - {saved?.RepositoryUrl}");

    // Nấc 4: cố tình gửi sai, đọc ApiError mà không làm chương trình sập.
    using var invalidResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignmentId}/submissions",
        new CreateSubmissionRequest("", ""));
    Console.WriteLine($"Thu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    await PrintApiErrorAsync(invalidResponse);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Loi ket noi API: {ex.Message}");
}

static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode) return;
    try
    {
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Console.WriteLine($"{error?.Code}: {error?.Message}");
    }
    catch (System.Text.Json.JsonException)
    {
        Console.WriteLine($"API tra {(int)response.StatusCode} va khong co ApiError JSON.");
    }
}

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode,
    string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
