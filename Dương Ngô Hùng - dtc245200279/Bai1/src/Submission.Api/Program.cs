// EduSubmit - Submission.Api (máy chủ)
// Chạy: dotnet run --project src/Submission.Api --urls http://localhost:5000

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu bài tập mẫu, lưu trong bộ nhớ
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Goi API tu client")
};

// Danh sách lượt nộp đã nhận (bài mở rộng)
var submissions = new List<SubmissionResult>();

// GET / : kiểm tra API đang chạy
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
    // Máy chủ tự kiểm tra dữ liệu, không tin máy khách
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new { message = "Du lieu nop bai khong hop le" });
    }

    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow);

    lock (submissions) submissions.Add(submission);

    // 201 Created + header Location trỏ tới tài nguyên vừa tạo
    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

// GET /api/submissions/{id} : đọc lại một lượt nộp (bài mở rộng)
app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    SubmissionResult? found;
    lock (submissions) found = submissions.FirstOrDefault(s => s.Id == id);

    return found is null ? Results.NotFound() : Results.Ok(found);
});

app.Run();

// Hợp đồng dữ liệu
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
