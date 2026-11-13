// Bài 1.2 – BasicWebApi: luyện kỹ thuật Minimal API (giáo trình mục 1.2)
// Chạy: dotnet run --project src/BasicWebApi --urls http://localhost:5081

// Pha 1: chuẩn bị ứng dụng – tạo builder, đọc cấu hình, đăng ký dịch vụ
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GreetingService>();

// Pha 2: tạo ứng dụng, ánh xạ endpoint, rồi chạy
var app = builder.Build();

// GET / : kiểm tra API đang chạy
app.MapGet("/", () => Results.Ok(new {
    application = "BasicWebApi",
    message = "ASP.NET Core Web API is running."
}));

// GET /hello/{name}?language=en : name lấy từ route, language lấy từ query string
app.MapGet("/hello/{name}", (string name, string? language) => {
    var message = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase)
        ? $"Hello, {name}!"
        : string.Equals(language, "ja", StringComparison.OrdinalIgnoreCase)
            ? $"Konnichiwa, {name}!"
            : $"Xin chào, {name}!";

    return Results.Ok(new GreetingResponse(message));
});

// POST /greetings : request lấy từ body JSON, service lấy từ dependency injection
app.MapPost("/greetings", (CreateGreetingRequest request, GreetingService service) => {
    if (string.IsNullOrWhiteSpace(request.Name)) {
        return Results.BadRequest(new ApiError("Name must not be empty."));
    }

    var greeting = service.Create(request.Name.Trim());
    return Results.Created($"/greetings/{greeting.Id}", greeting);
});

app.Run();

public sealed record CreateGreetingRequest(string Name);
public sealed record GreetingResponse(string Message);
public sealed record ApiError(string Message);
public sealed record StoredGreeting(int Id, string Name, DateTimeOffset CreatedAt);

// Dịch vụ tách xử lý ra khỏi route handler; sinh mã tăng dần an toàn khi có nhiều request đồng thời
public sealed class GreetingService {
    private int _lastId;

    public StoredGreeting Create(string name) =>
        new(Interlocked.Increment(ref _lastId), name, DateTimeOffset.UtcNow);
}

