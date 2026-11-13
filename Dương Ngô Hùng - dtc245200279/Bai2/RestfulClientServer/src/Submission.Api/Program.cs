// ① Khởi tạo
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ② Dữ liệu mẫu trong bộ nhớ (Bước 3): tắt API là mất
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint cho he thong nop bai", true),
    new(2, "Bai tap Client", "Viet console client goi API bang HttpClient", true),
    new(3, "Bai tap da dong", "Bai tap da het han nop", false)
};
var submissions = new List<SubmissionDto>();

// ③ Các endpoint

// GET / : kiểm tra máy chủ đang chạy (Bước 2)
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments : danh sách bài tập (Bước 3)
app.MapGet("/api/assignments", () => Results.Ok(assignments));

// GET /api/assignments/{assignmentId:int} : chi tiết một bài tập (Bước 4)
app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."))
        : Results.Ok(assignment);
});

// POST /api/assignments/{assignmentId:int}/submissions : nộp bài (Bước 5)
// Lượt nộp là tài nguyên con của bài tập: assignmentId nằm trên URI, không nằm trong body.
// Kiểm tra theo thứ tự: tồn tại → còn mở → body hợp lệ, rồi mới tạo.
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
{
    // Cổng 1: bài tập có tồn tại?
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
    if (assignment is null)
        return Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", "Khong tim thay bai tap."));

    // Cổng 2: bài tập còn mở?
    if (!assignment.IsOpen)
        return Results.BadRequest(new ApiError("ASSIGNMENT_CLOSED", "Bai tap da dong, khong nhan bai nop."));

    // Cổng 3: body hợp lệ?
    if (string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
        return Results.BadRequest(new ApiError("INVALID_SUBMISSION",
            "Thieu ma sinh vien hoac kho ma nguon."));

    // Máy chủ tự sinh Id, Status, ReceivedAt: máy khách không được quyết định
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

// ④ Chạy
app.Run();

// ⑤ Khai báo kiểu: DTO là hợp đồng dữ liệu đi qua mạng
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);

// Đầu vào: chỉ chứa những gì máy khách được phép quyết định
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);

// Đầu ra: thêm các trường máy chủ tự sinh
record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt);

record ApiError(string Code, string Message);
