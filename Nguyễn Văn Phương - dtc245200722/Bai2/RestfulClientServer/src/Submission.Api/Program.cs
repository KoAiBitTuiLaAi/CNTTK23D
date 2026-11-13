var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu mẫu trong bộ nhớ
var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint cho he thong nop bai", true),
    new(2, "Bai tap Client", "Viet console client goi API", true)
};

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

app.Run();

// DTO: hình dạng dữ liệu đi qua mạng (khai báo sau app.Run())
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record ApiError(string Code, string Message);
