using System.Net.Http.Json;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
var studentCode = args.Length > 1 ? args[1] : "DTC245200314";
// URL mẫu dùng dữ liệu minh họa; thay bằng repository thật khi nộp bài.
var repositoryUrl = args.Length > 2 ? args[2] : "https://example.com/edusubmit.git";

if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseAddress) ||
    (baseAddress.Scheme != "http" && baseAddress.Scheme != "https"))
{
    Console.Error.WriteLine("Dia chi API phai la URL HTTP hoac HTTPS hop le.");
    return 1;
}

using var http = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(10) };
try
{
    var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments")
        ?? throw new InvalidOperationException("API khong tra ve danh sach bai tap.");
    Console.WriteLine("Danh sach bai tap:");
    foreach (var assignment in assignments)
        Console.WriteLine($"  - {assignment.Id}: {assignment.Title}");

    var request = new CreateSubmissionRequest(1, studentCode, repositoryUrl);
    using var response = await http.PostAsJsonAsync("/api/submissions", request);
    Console.WriteLine($"POST /api/submissions -> {(int)response.StatusCode} {response.StatusCode}");
    if (!response.IsSuccessStatusCode)
        Console.Error.WriteLine(await response.Content.ReadAsStringAsync());
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<SubmissionResult>()
        ?? throw new InvalidOperationException("API khong tra ve luot nop.");
    Console.WriteLine($"Da nop bai. Ma luot nop: {result.Id}");
    Console.WriteLine($"Trang thai: {result.Status}");
    Console.WriteLine($"Location: {response.Headers.Location}");

    // Bước 12: gửi một request HTTP mới để đọc lại dữ liệu đã lưu trên API.
    var saved = await http.GetFromJsonAsync<SubmissionResult>($"/api/submissions/{result.Id}")
        ?? throw new InvalidOperationException("Khong doc lai duoc luot nop.");
    Console.WriteLine($"Doc lai: {saved.Id} | MSSV: {saved.StudentCode} | Trang thai: {saved.Status}");
    return 0;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Loi HTTP: {ex.Message}");
    Console.Error.WriteLine("Kiem tra API da chay, dung cong va du lieu nop bai hop le.");
    return 1;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine("Qua thoi gian cho 10 giay. Kiem tra ket noi den API.");
    return 1;
}

// Client tự khai báo hợp đồng JSON, không tham chiếu project của API.
record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
