using System.Net.Http.Json;

var apiAddress = args.Length > 0 ? args[0] : "http://localhost:5000";
var studentCode = args.Length > 1 ? args[1] : "dtc245200427";
var repositoryUrl = args.Length > 2 ? args[2] : "https://github.com/dongtuan-creator/CNTTK23D";

try
{
    using var http = new HttpClient { BaseAddress = new Uri(apiAddress) };

    var assignments = await http.GetFromJsonAsync<List<Assignment>>("/api/assignments") ?? [];
    Console.WriteLine("Danh sach bai tap:");
    foreach (var assignment in assignments)
    {
        Console.WriteLine($"  - {assignment.Id}: {assignment.Title} - {assignment.Description}");
    }

    var request = new CreateSubmissionRequest(1, studentCode, repositoryUrl);
    using var response = await http.PostAsJsonAsync("/api/submissions", request);
    response.EnsureSuccessStatusCode();
    var submission = await response.Content.ReadFromJsonAsync<SubmissionResult>()
        ?? throw new InvalidOperationException("API returned an empty submission response.");
    Console.WriteLine($"Da nop bai. Ma luot nop: {submission.Id}");
    Console.WriteLine($"Trang thai: {submission.Status}");

    var savedSubmission = await http.GetFromJsonAsync<SubmissionResult>($"/api/submissions/{submission.Id}");
    Console.WriteLine(savedSubmission is null
        ? "Khong doc lai duoc luot nop."
        : $"Da doc lai luot nop: {savedSubmission.Id} - {savedSubmission.Status}");
}
catch (HttpRequestException ex) when (ex.StatusCode is not null)
{
    Console.Error.WriteLine($"API trả về lỗi HTTP {(int)ex.StatusCode}: {ex.StatusCode}.");
    Console.Error.WriteLine("Kiểm tra dữ liệu StudentCode, AssignmentId và request gửi lên.");
    Environment.ExitCode = 1;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Không kết nối được API tại {apiAddress}.");
    Console.Error.WriteLine("Hãy chạy Submission.Api trước và truyền đúng địa chỉ/cổng API cho Client.");
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine($"Gọi API tại {apiAddress} bị quá thời gian chờ.");
    Console.Error.WriteLine("Kiểm tra API còn chạy và địa chỉ/cổng đã đúng chưa.");
    Environment.ExitCode = 1;
}
catch (UriFormatException ex)
{
    Console.Error.WriteLine($"Địa chỉ API không hợp lệ: {apiAddress}");
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
