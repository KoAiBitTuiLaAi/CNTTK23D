var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));

var gate = new object();
var assignments = new[]
{
    new AssignmentDto(1, "Bai tap REST API", "Thiet ke endpoint RESTful", true),
    new AssignmentDto(2, "Bai tap Client", "Viet console client HttpClient", true),
    new AssignmentDto(3, "Bai tap da dong", "Bai tap het han nop", false)
};

app.MapGet("/api/assignments", () =>
{
    lock (gate) return Results.Ok(assignments.ToArray());
});
app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    lock (gate)
    {
        var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
        return assignment is null
            ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
            : Results.Ok(assignment);
    }
});

var submissions = new Dictionary<Guid, SubmissionDto>();
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest input) =>
{
    lock (gate)
    {
        var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
        if (assignment is null)
            return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));
        if (!assignment.IsOpen)
            return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong."));
        if (string.IsNullOrWhiteSpace(input.StudentCode) || string.IsNullOrWhiteSpace(input.RepositoryUrl))
            return Results.BadRequest(new ApiError("INVALID_SUBMISSION", "Thieu ma sinh vien hoac kho ma nguon."));

        var studentCode = input.StudentCode.Trim();
        if (submissions.Values.Any(item => item.AssignmentId == assignmentId &&
            string.Equals(item.StudentCode, studentCode, StringComparison.OrdinalIgnoreCase)))
            return Results.Conflict(new ApiError("DUPLICATE_SUBMISSION", "Sinh vien da nop bai tap nay."));

        var submission = new SubmissionDto(Guid.NewGuid(), assignmentId,
            studentCode, input.RepositoryUrl.Trim(), "received", DateTimeOffset.UtcNow);
        submissions.Add(submission.Id, submission);
        return Results.Created($"/api/submissions/{submission.Id}", submission);
    }
});
app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    lock (gate)
    {
        return submissions.TryGetValue(submissionId, out var submission)
            ? Results.Ok(submission)
            : Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", "Khong tim thay bai nop."));
    }
});

app.MapDelete("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    lock (gate)
    {
        return submissions.Remove(submissionId)
            ? Results.NoContent()
            : Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", "Khong tim thay bai nop."));
    }
});

app.Run();

record CreateSubmissionRequest(string? StudentCode, string? RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl,
    string Status, DateTimeOffset ReceivedAt);

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record ApiError(string Code, string Message);
