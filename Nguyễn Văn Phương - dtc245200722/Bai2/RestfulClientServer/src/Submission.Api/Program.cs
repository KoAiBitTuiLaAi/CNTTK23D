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

app.Run();

// DTO: hình dạng dữ liệu đi qua mạng (khai báo sau app.Run())
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record ApiError(string Code, string Message);
