var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET / : kiểm tra máy chủ đang chạy
app.MapGet("/", () => Results.Ok(new {
    service = "Catalog.Api",
    status = "running"
}));

app.Run();
