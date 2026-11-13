var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var submissions = new List<SubmissionResult>();

var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Tim hieu he thong phan tan")
};

app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

app.MapGet("/api/assignments", () =>
    Results.Ok(assignments));

app.MapPost("/api/submissions",
    IResult (CreateSubmissionRequest request) =>
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
    var submission = submissions.FirstOrDefault(s => s.Id == id);

    if (submission is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(submission);
});

app.Run();

record Assignment(
    int Id,
    string Title,
    string Description
);

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