using System.Net;
using System.Net.Http.Json;

// Dia chi may chu lay tu tham so dong lenh, mac dinh cong 5000
var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

try
{
    // Nac 1: liet ke bai tap dang mo
    var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");
    Console.WriteLine("Danh sach bai tap dang mo:");
    foreach (var a in (assignments ?? []).Where(a => a.IsOpen))
    {
        Console.WriteLine($"- {a.Id}: {a.Title}");
    }

    var selectedAssignmentId = assignments?.FirstOrDefault(a => a.IsOpen)?.Id ?? 1;

    // Nac 2: nop bai. Xem ma trang thai TRUOC, doc body SAU
    var createResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignmentId}/submissions",
        new CreateSubmissionRequest("SV001", "https://example.com/sv001/rest-api"));

    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        await PrintApiErrorAsync(createResponse);
        return 1;
    }

    var created = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"\nDa tao luot nop: {created?.Id}");
    Console.WriteLine($"Trang thai ban dau: {created?.Status}");

    // Nac 3: doc lai luot nop theo dia chi trong header Location
    var readResponse = await http.GetAsync(createResponse.Headers.Location);
    if (!readResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(readResponse);
        return 1;
    }

    var readBack = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>();
    Console.WriteLine($"Doc lai luot nop: {readBack?.StudentCode} - {readBack?.RepositoryUrl}");

    // Nac 4: co y gui du lieu sai de quan sat loi 400, khong de chuong trinh sap
    var invalidResponse = await http.PostAsJsonAsync(
        $"/api/assignments/{selectedAssignmentId}/submissions",
        new CreateSubmissionRequest("", "https://example.com/sv001/rest-api"));

    Console.WriteLine($"\nThu gui yeu cau loi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    await PrintApiErrorAsync(invalidResponse);
    return 0;
}
catch (HttpRequestException ex)
{
    // Loi mang (khong co phan hoi) khac voi loi HTTP (co phan hoi 4xx/5xx)
    Console.WriteLine($"Loi mang: khong ket noi duoc toi {apiBaseUrl}. {ex.Message}");
    return 1;
}

// Mot ham doc moi loi, nho API tra moi loi cung dang ApiError
static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode) return;

    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"{error?.Code}: {error?.Message}");
}

// DTO: ban sao hop dong JSON cua may chu (khong dung chung lop voi Submission.Api)
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
