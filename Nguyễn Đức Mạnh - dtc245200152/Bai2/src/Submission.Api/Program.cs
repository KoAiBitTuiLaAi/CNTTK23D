var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Du lieu mau trong bo nho
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint cho he thong nop bai", true),
    new(2, "Bai tap Client", "Viet console client goi API bang HttpClient", true),
    new(3, "Bai tap da dong", "Bai tap da het han, khong nhan them luot nop", false)
};

// Cac luot nop da nhan (them o buoc 5)
var submissions = new List<SubmissionDto>();

// GET / : kiem tra may chu dang chay
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments : danh sach bai tap
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// GET /api/assignments/{assignmentId:int} : chi tiet mot bai tap
app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
        : Results.Ok(assignment);
});

// POST /api/assignments/{assignmentId:int}/submissions : nop bai (tai nguyen con)
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
{
    // Cong 1: bai tap co ton tai khong?
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    if (assignment is null)
        return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));

    // Cong 2: bai tap con mo khong?
    if (!assignment.IsOpen)
        return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong, khong nhan them luot nop."));

    // Cong 3: du lieu gui len co hop le khong?
    if (string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
        return Results.BadRequest(new ApiError("INVALID_SUBMISSION", "Thieu ma sinh vien hoac duong dan repository."));

    // May chu tu dien Id, AssignmentId (tu URI), Status, ReceivedAt
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

app.Run();

// DTO: hinh dang du lieu di qua mang (dat sau app.Run())
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl); // dau vao: khong co Id, Status
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode, string RepositoryUrl, string Status, DateTimeOffset ReceivedAt); // dau ra
record ApiError(string Code, string Message);
