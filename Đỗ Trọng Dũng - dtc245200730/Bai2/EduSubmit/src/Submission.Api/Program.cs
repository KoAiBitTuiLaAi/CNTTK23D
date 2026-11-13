// EduSubmit – Submission.Api (Bài 2: RESTful API)
// Chạy: dotnet run --project src/Submission.Api --urls http://localhost:5000

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu mẫu trong bộ nhớ (mất khi tắt API)
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint theo quy tac REST", true),
    new(2, "Bai tap Client", "Viet console client goi API", true),
    new(3, "Bai tap cu", "Bai tap da het han nop", false)
};

var submissions = new List<SubmissionDto>();
var submissionsLock = new object(); // List<T> không an toàn khi nhiều request chạy song song

// GET / : kiểm tra dịch vụ đang chạy
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments : danh sách bài tập
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// GET /api/assignments/{assignmentId:int} : chi tiết một bài tập
app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
        : Results.Ok(assignment);
});

// POST /api/assignments/{assignmentId:int}/submissions : nộp bài (lượt nộp là tài nguyên con)
// Kiểm tra theo thứ tự: tồn tại → còn mở → hợp lệ
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    if (assignment is null)
        return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));

    if (!assignment.IsOpen)
        return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong, khong nhan bai nop."));

    if (string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
        return Results.BadRequest(new ApiError("INVALID_SUBMISSION", "Thieu ma sinh vien hoac kho ma nguon."));

    // Máy chủ tự sinh Id, gán AssignmentId từ URI, Status và ReceivedAt
    var submission = new SubmissionDto(
        Guid.NewGuid(),
        assignmentId,
        request.StudentCode.Trim(),
        request.RepositoryUrl.Trim(),
        "received",
        DateTimeOffset.UtcNow);

    lock (submissionsLock)
    {
        submissions.Add(submission);
    }

    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

// GET /api/submissions/{submissionId:guid} : đọc lại một lượt nộp
app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    SubmissionDto? submission;
    lock (submissionsLock)
    {
        submission = submissions.FirstOrDefault(item => item.Id == submissionId);
    }

    return submission is null
        ? Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", "Khong tim thay luot nop."))
        : Results.Ok(submission);
});

app.Run();

// Hợp đồng dữ liệu
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);

// Đầu vào: máy khách chỉ gửi những gì được phép quyết định
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);

// Đầu ra: Id, AssignmentId, Status, ReceivedAt do máy chủ điền
record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt);

record ApiError(string Code, string Message);
