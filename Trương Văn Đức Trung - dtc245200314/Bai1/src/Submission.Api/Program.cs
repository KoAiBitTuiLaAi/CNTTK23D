var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Viet client goi API qua HTTP")
};

// Bài 1 lưu trong bộ nhớ: khởi động lại API sẽ mất các lượt nộp.
var submissions = new List<SubmissionResult>();
var submissionsLock = new object();

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));
app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", IResult (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new
        {
            message = "AssignmentId must be positive; StudentCode and RepositoryUrl must not be empty."
        });
    }

    if (!assignments.Any(a => a.Id == request.AssignmentId))
        return Results.BadRequest(new { message = "Assignment does not exist." });

    var submission = new SubmissionResult(
        Guid.NewGuid(), request.AssignmentId, request.StudentCode.Trim(),
        "received", DateTimeOffset.UtcNow);

    lock (submissionsLock)
    {
        submissions.Add(submission);
    }

    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

// Phần mở rộng: đọc lại lượt nộp bằng id và trả đúng 200 hoặc 404.
app.MapGet("/api/submissions/{id:guid}", IResult (Guid id) =>
{
    SubmissionResult? submission;
    lock (submissionsLock)
    {
        submission = submissions.FirstOrDefault(s => s.Id == id);
    }

    return submission is null ? Results.NotFound() : Results.Ok(submission);
});

app.Run();

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string? StudentCode, string? RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
