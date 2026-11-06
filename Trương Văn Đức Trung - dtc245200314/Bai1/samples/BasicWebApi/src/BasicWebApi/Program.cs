var builder = WebApplication.CreateBuilder(args);
// Đăng ký dịch vụ trước Build, sau đó DI cung cấp service cho các endpoint.
builder.Services.AddSingleton<GreetingService>();
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { application = "BasicWebApi" }));
app.MapGet("/hello/{name}", (string name, string? language, GreetingService service) =>
    Results.Ok(new { message = service.Hello(name, language) }));

app.MapPost("/greetings", IResult (GreetingRequest request, GreetingService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Name must not be empty." });

    // Đặt breakpoint ở dòng bên dưới để quan sát request khi POST.
    var greeting = service.Create(request.Name.Trim(), request.Language);
    return Results.Created($"/greetings/{greeting.Id}", greeting);
});

app.MapGet("/greetings/{id:int}", IResult (int id, GreetingService service) =>
{
    var greeting = service.Find(id);
    return greeting is null ? Results.NotFound() : Results.Ok(greeting);
});

app.Run();

record GreetingRequest(string? Name, string? Language);
record Greeting(int Id, string Name, string Message);

sealed class GreetingService
{
    private readonly List<Greeting> _greetings = new();
    private readonly object _gate = new();

    public string Hello(string name, string? language) => language?.ToLowerInvariant() switch
    {
        "en" => $"Hello, {name}!",
        "ja" => $"Konnichiwa, {name}!",
        _ => $"Xin chao, {name}!"
    };

    public Greeting Create(string name, string? language)
    {
        lock (_gate)
        {
            var greeting = new Greeting(_greetings.Count + 1, name, Hello(name, language));
            _greetings.Add(greeting);
            return greeting;
        }
    }

    public Greeting? Find(int id)
    {
        lock (_gate) return _greetings.FirstOrDefault(g => g.Id == id);
    }
}
