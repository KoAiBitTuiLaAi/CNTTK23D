// EduSubmit – Submission.Api (vai trò MÁY CHỦ)
// Chạy: dotnet run --project src/Submission.Api --urls http://localhost:5000

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu mẫu trong bộ nhớ
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Goi API tu client")
};

// Bước 12: lưu các lượt nộp trong bộ nhớ (mất khi tắt API)
var submissions = new List<SubmissionResult>();
var submissionsLock = new object(); // List<T> không an toàn khi nhiều request chạy song song

// GET / : kiểm tra dịch vụ đang chạy
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments : danh sách bài tập
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// POST /api/submissions : nộp bài
app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    // 1. Kiểm tra dữ liệu đầu vào
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new { message = "Du lieu nop bai khong hop le" });
    }

    // 2. Tạo lượt nộp với trạng thái ban đầu "received"
    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow);

    lock (submissionsLock)
    {
        submissions.Add(submission);
    }

    // 3. Trả 201 Created + header Location trỏ tới lượt nộp vừa tạo
    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

// Bước 12 – GET /api/submissions/{id} : đọc lại một lượt nộp
app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    SubmissionResult? submission;
    lock (submissionsLock)
    {
        submission = submissions.FirstOrDefault(s => s.Id == id);
    }

    return submission is null
        ? Results.NotFound(new { message = $"Khong tim thay luot nop {id}" })
        : Results.Ok(submission);
});

app.Run();

// Hợp đồng dữ liệu trao đổi qua API
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
