var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint va xu ly ma trang thai HTTP.", true),
    new(2, "Bai tap Client", "Viet console client goi Submission.Api.", false)
};
var submissions = new List<SubmissionDto>();

app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
        : Results.Ok(assignment);
});

app.MapPost("/api/assignments/{assignmentId:int}/submissions", (int assignmentId, CreateSubmissionRequest request) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    if (assignment is null)
    {
        return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));
    }

    if (!assignment.IsOpen)
    {
        return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong, khong nhan bai nop."));
    }

    if (string.IsNullOrWhiteSpace(request.StudentCode) || string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new ApiError("INVALID_SUBMISSION", "Thieu ma sinh vien hoac kho ma nguon."));
    }

    var studentCode = request.StudentCode.Trim();
    var duplicated = submissions.Any(item =>
        item.AssignmentId == assignmentId &&
        item.StudentCode.Equals(studentCode, StringComparison.OrdinalIgnoreCase));
    if (duplicated)
    {
        return Results.Conflict(new ApiError("DUPLICATE_SUBMISSION", "Sinh vien da nop bai tap nay."));
    }

    var submission = new SubmissionDto(
        Guid.NewGuid(),
        assignmentId,
        studentCode,
        request.RepositoryUrl.Trim(),
        "received",
        DateTimeOffset.UtcNow);
    submissions.Add(submission);

    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    var submission = submissions.FirstOrDefault(item => item.Id == submissionId);
    return submission is null
        ? Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", "Khong tim thay luot nop."))
        : Results.Ok(submission);
});

app.MapDelete("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
    DeleteSubmission(submissions, submissionId));

app.Run();

static IResult DeleteSubmission(List<SubmissionDto> submissions, Guid submissionId)
{
    var submission = submissions.FirstOrDefault(item => item.Id == submissionId);
    if (submission is null)
    {
        return Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", "Khong tim thay luot nop."));
    }

    submissions.Remove(submission);
    return Results.NoContent();
}

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record ApiError(string Code, string Message);
record CreateSubmissionRequest(string? StudentCode, string? RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
