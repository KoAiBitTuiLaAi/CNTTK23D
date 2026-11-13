// EduSubmit – lát cắt 1: Submission.Api (vai trò MÁY CHỦ)
// Chạy: dotnet run --project src/Submission.Api --urls http://localhost:5000

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu mẫu trong bộ nhớ (chưa dùng cơ sở dữ liệu)
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Goi API tu client")
};

// GET / : kiểm tra dịch vụ đang chạy
app.MapGet("/", () => Results.Ok(new {
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments : lấy danh sách bài tập
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// POST /api/submissions : gửi một lượt nộp bài
app.MapPost("/api/submissions", (CreateSubmissionRequest request) => {
    // 1. Kiểm tra dữ liệu đầu vào trước khi xử lý
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl)) {
        return Results.BadRequest(new { message = "Du lieu nop bai khong hop le" });
    }

    // 2. Ghi nhận lượt nộp với trạng thái ban đầu "received"
    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow);

    // 3. Trả 201 Created kèm header Location
    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

app.Run();

// Hợp đồng dữ liệu trao đổi qua ranh giới API
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
