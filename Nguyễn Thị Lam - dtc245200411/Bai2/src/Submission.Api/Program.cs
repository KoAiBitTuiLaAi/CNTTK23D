var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Du lieu mau duoc luu trong bo nho cua tien trinh API.
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint RESTful va ma trang thai HTTP.", true),
    new(2, "Bai tap Client", "Viet console client goi API bang HttpClient.", true),
    new(3, "Bai tap da dong", "Dung de kiem tra loi ASSIGNMENT_CLOSED.", false)
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

    if (assignment is null)
    {
        return Results.NotFound(new ApiError(
            "ASSIGNMENT_NOT_FOUND",
            "Khong tim thay bai tap."));
    }

    return Results.Ok(assignment);
});

app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
    {
        var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);

        // Kiem tra dung thu tu: ton tai -> con mo -> du lieu hop le.
        if (assignment is null)
        {
            return Results.NotFound(new ApiError(
                "ASSIGNMENT_NOT_FOUND",
                "Khong tim thay bai tap."));
        }

        if (!assignment.IsOpen)
        {
            return Results.BadRequest(new ApiError(
                "ASSIGNMENT_CLOSED",
                "Bai tap da dong, khong the nop bai."));
        }

        if (string.IsNullOrWhiteSpace(request.StudentCode) ||
            string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            return Results.BadRequest(new ApiError(
                "INVALID_SUBMISSION",
                "Thieu ma sinh vien hoac dia chi kho ma nguon."));
        }

        var submission = new SubmissionDto(
            Id: Guid.NewGuid(),
            AssignmentId: assignmentId,
            StudentCode: request.StudentCode.Trim(),
            RepositoryUrl: request.RepositoryUrl.Trim(),
            Status: "received",
            ReceivedAt: DateTimeOffset.UtcNow);

        submissions.Add(submission);

        return Results.Created($"/api/submissions/{submission.Id}", submission);
    });

app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    var submission = submissions.FirstOrDefault(item => item.Id == submissionId);

    if (submission is null)
    {
        return Results.NotFound(new ApiError(
            "SUBMISSION_NOT_FOUND",
            "Khong tim thay luot nop."));
    }

    return Results.Ok(submission);
});

app.Run();

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);

record CreateSubmissionRequest(string? StudentCode, string? RepositoryUrl);

record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt);

record ApiError(string Code, string Message);
