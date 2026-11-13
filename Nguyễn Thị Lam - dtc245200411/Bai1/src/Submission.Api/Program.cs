var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Tao client goi API")
};

var submissions = new List<SubmissionResult>();
var submissionsLock = new object();

app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new
        {
            message = "AssignmentId, StudentCode and RepositoryUrl are required."
        });
    }

    if (!assignments.Any(assignment => assignment.Id == request.AssignmentId))
    {
        return Results.BadRequest(new
        {
            message = $"Assignment {request.AssignmentId} does not exist."
        });
    }

    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode.Trim(),
        "received",
        DateTimeOffset.UtcNow);

    lock (submissionsLock)
    {
        submissions.Add(submission);
    }

    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    SubmissionResult? submission;

    lock (submissionsLock)
    {
        submission = submissions.FirstOrDefault(item => item.Id == id);
    }

    return submission is null
        ? Results.NotFound(new { message = $"Submission {id} was not found." })
        : Results.Ok(submission);
});

app.Run();

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
