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

// ④ Chạy
app.Run();

// ⑤ Khai báo kiểu: DTO là hợp đồng dữ liệu đi qua mạng
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record ApiError(string Code, string Message);
