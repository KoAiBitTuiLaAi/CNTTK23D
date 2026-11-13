var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Viet Client va giao tiep HTTP")
};

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.StudentCode) || request.AssignmentId <= 0)
    {
        return Results.BadRequest(new { message = "Du lieu khong hop le." });
    }

    var result = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow
    );

    return Results.Created($"/api/submissions/{result.Id}", result);
});

app.Run();

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);
