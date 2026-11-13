var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Viet client giao tiep qua HTTP")
};

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));
app.MapGet("/api/assignments", () => Results.Ok(assignments));
app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 || string.IsNullOrWhiteSpace(request.StudentCode)
        || string.IsNullOrWhiteSpace(request.RepositoryUrl))
        return Results.BadRequest(new { message = "Invalid submission data." });

    var submission = new SubmissionResult(Guid.NewGuid(), request.AssignmentId,
        request.StudentCode, "received", DateTimeOffset.UtcNow);
    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

app.Run();

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string? StudentCode, string? RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode,
    string Status, DateTimeOffset ReceivedAt);
