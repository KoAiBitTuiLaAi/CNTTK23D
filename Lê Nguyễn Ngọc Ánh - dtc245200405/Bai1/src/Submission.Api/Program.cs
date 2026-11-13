var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var submissions = new System.Collections.Concurrent.ConcurrentDictionary<Guid, SubmissionResult>();

// Danh sách bài tập
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Viet Client goi API")
};

// Kiểm tra API đang hoạt động
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// Lấy danh sách bài tập
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// Tiếp nhận bài nộp
app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new
        {
            message = "Du lieu nop bai khong hop le."
        });
    }

    var result = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow
    );

    submissions[result.Id] = result;

    return Results.Created(
        $"/api/submissions/{result.Id}",
        result
    );
});

app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    if (!submissions.TryGetValue(id, out var submission))
    {
        return Results.NotFound();
    }
    return Results.Ok(submission);
});

app.Run();

// Các kiểu dữ liệu
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl
);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt
);
