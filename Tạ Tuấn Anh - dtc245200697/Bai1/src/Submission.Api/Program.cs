var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bài tập 1", "Tạo API đầu tiên"),
    new Assignment(2, "Bài tập 2", "Xây dựng hệ thống phân tán")
};

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "Submission.Api",
        status = "running"
    });
});

app.MapGet("/api/assignments", () =>
{
    return Results.Ok(assignments);
});

app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new
        {
            message = "Thông tin nộp bài không hợp lệ"
        });
    }

    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow
    );

    return Results.Created(
        $"/api/submissions/{submission.Id}",
        submission
    );
});

app.Run();

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