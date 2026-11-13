// EduSubmit - Submission.Api (máy chủ)
// Chạy: dotnet run --project src/Submission.Api --urls http://localhost:5000

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu bài tập lưu trong bộ nhớ
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Viet client goi API")
};

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
    // 1. Kiểm tra dữ liệu: thiếu trường hoặc assignmentId <= 0 thì trả 400
    if (request.AssignmentId <= 0
        || string.IsNullOrWhiteSpace(request.StudentCode)
        || string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new { message = "Du lieu nop bai khong hop le" });
    }

    // 2. Tạo lượt nộp mới với mã Guid và trạng thái "received"
    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow);

    // 3. Trả 201 Created kèm header Location trỏ tới lượt nộp vừa tạo
    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

app.Run();

record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
