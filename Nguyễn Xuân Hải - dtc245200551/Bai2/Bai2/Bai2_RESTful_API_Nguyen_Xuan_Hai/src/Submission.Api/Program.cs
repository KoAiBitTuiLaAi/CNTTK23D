var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Du lieu mau luu trong bo nho tien trinh API.
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thuc hanh xay dung RESTful API voi Minimal API.", true),
    new(2, "Bai tap Client", "Thuc hanh HttpClient va xu ly loi HTTP.", false)
};
var submissions = new List<SubmissionDto>();

app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// 1. GET danh sach bai tap.
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// 2. GET chi tiet mot bai tap.
app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(x => x.Id == assignmentId);
    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
        : Results.Ok(assignment);
});

// 3. POST tao luot nop bai.
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
{
    // Kiem tra theo thu tu: bai tap ton tai -> bai tap con mo -> du lieu hop le.
    var assignment = assignments.FirstOrDefault(x => x.Id == assignmentId);
    if (assignment is null)
        return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));

    if (!assignment.IsOpen)
        return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong, khong the nop."));

    if (string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
        return Results.BadRequest(new ApiError(
            "INVALID_SUBMISSION", "Thieu ma sinh vien hoac kho ma nguon."));

    if (!Uri.TryCreate(request.RepositoryUrl, UriKind.Absolute, out var repoUri) ||
        (repoUri.Scheme != Uri.UriSchemeHttp && repoUri.Scheme != Uri.UriSchemeHttps))
        return Results.BadRequest(new ApiError(
            "INVALID_SUBMISSION", "RepositoryUrl phai la dia chi HTTP hoac HTTPS hop le."));

    var submission = new SubmissionDto(
        Guid.NewGuid(),
        assignmentId,
        request.StudentCode.Trim(),
        request.RepositoryUrl.Trim(),
        "received",
        DateTimeOffset.UtcNow);

    submissions.Add(submission);

    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

// 4. GET doc lai mot luot nop theo GUID trong Location.
app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    var submission = submissions.FirstOrDefault(x => x.Id == submissionId);
    return submission is null
        ? Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", "Khong tim thay luot nop."))
        : Results.Ok(submission);
});

app.Run();

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
