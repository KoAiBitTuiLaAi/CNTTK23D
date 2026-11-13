var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Xay dung client giao tiep qua HTTP")
};

// Khởi tạo danh sách lưu trữ các lượt nộp bài trong bộ nhớ
var submissions = new List<SubmissionResult>();

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 || 
        string.IsNullOrWhiteSpace(request.StudentCode) || 
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new { message = "Du lieu nop bai khong hop le." });
    }

    var result = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow
    );

    // Lưu lượt nộp bài mới vào danh sách
    submissions.Add(result);

    return Results.Created($"/api/submissions/{result.Id}", result);
});

// Endpoint bài mở rộng: GET /api/submissions/{id:guid}
app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    var sub = submissions.FirstOrDefault(s => s.Id == id);
    return sub is not null 
        ? Results.Ok(sub) 
        : Results.NotFound(new { message = "Khong tim thay luot nop bai." });
});

app.Run();

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);