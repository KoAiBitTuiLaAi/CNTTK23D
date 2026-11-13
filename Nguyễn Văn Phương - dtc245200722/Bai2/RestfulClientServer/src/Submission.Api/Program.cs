var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu mẫu trong bộ nhớ
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint cho he thong nop bai", true),
    new(2, "Bai tap Client", "Viet console client goi API", true)
};
var submissions = new List<SubmissionDto>();

// GET / : kiểm tra máy chủ đang chạy
app.MapGet("/", () => Results.Ok(new {
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments : danh sách bài tập
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// GET /api/assignments/{assignmentId:int} : chi tiết một bài tập
app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) => {
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
        : Results.Ok(assignment);
});

// POST /api/assignments/{assignmentId:int}/submissions : nộp bài (tài nguyên con)
// Kiểm tra theo thứ tự: tồn tại → còn mở → hợp lệ, rồi mới tạo
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) => {
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    if (assignment is null)
        return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));

    if (!assignment.IsOpen)
        return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong, khong nhan bai nop."));

    if (string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
        return Results.BadRequest(new ApiError("INVALID_SUBMISSION", "Thieu ma sinh vien hoac kho ma nguon."));

    // Máy chủ tự sinh Id, trạng thái, thời điểm nhận; assignmentId lấy từ URI
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

// DTO: hình dạng dữ liệu đi qua mạng (khai báo sau app.Run())
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);   // đầu vào: không có Id, Status
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode,
    string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);       // đầu ra
record ApiError(string Code, string Message);
