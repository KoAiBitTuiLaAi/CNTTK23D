var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Mo rong he thong nop bai")
};
var submissions = new List<SubmissionResult>();

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));
app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", (CreateSubmissionRequest? request) =>
{
    if (request is null || request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new { message = "AssignmentId, StudentCode, and RepositoryUrl are required." });
    }

    if (!assignments.Any(a => a.Id == request.AssignmentId))
    {
        return Results.BadRequest(new { message = "AssignmentId does not identify an existing assignment." });
    }

    var result = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode.Trim(),
        "received",
        DateTimeOffset.UtcNow);
    submissions.Add(result);

    return Results.Created($"/api/submissions/{result.Id}", result);
});

app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    var result = submissions.FirstOrDefault(s => s.Id == id);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.Run();

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
