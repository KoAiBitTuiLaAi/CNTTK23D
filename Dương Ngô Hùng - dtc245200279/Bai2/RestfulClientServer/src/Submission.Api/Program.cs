// ① Khởi tạo
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ③ Các endpoint

// GET / : kiểm tra máy chủ đang chạy (Bước 2)
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// ④ Chạy
app.Run();
