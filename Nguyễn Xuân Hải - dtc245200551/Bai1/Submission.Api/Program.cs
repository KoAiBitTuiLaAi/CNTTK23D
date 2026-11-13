var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new List<Assignment>
{
    new(1, "Bai tap 1", "Tim hieu ve ung dung phan tan"),
    new(2, "Bai tap 2", "Thuc hanh giao tiep giua Client va API"),
    new(3, "Bai tap 3", "Thuc hanh mo rong API")
};

var submissions = new List<SubmissionResult>();

app.MapGet("/", () => new
{
    service = "Submission.Api",
    status = "running"
});

app.MapGet("/api/assignments", () => assignments);

app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new
        {
            message = "Thong tin bai nop khong hop le"
        });
    }

    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow
    );

    submissions.Add(submission);

    return Results.Created(
        $"/api/submissions/{submission.Id}",
        submission
    );
});

app.MapGet("/api/submissions/{id:guid}", IResult (Guid id) =>
{
    var submission = submissions.FirstOrDefault(item => item.Id == id);

    return submission is null
        ? Results.NotFound()
        : Results.Ok(submission);
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
