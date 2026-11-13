var builder = WebApplication.CreateBuilder(args);
// Đăng ký dịch vụ trước Build vì sau đó container đã được tạo.
builder.Services.AddSingleton<GreetingService>();
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { application = "BasicWebApi" }));
app.MapGet("/hello/{name}", (string name, string? language) =>
    Results.Ok(new { message = GreetingService.Message(name, language) }));

app.MapPost("/greetings", (CreateGreetingRequest request, GreetingService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Name must not be empty." });

    var greeting = service.Create(request.Name.Trim(), request.Language);
    return Results.Created($"/greetings/{greeting.Id}", greeting);
});

app.MapGet("/greetings/{id:int}", (int id, GreetingService service) =>
    service.Find(id) is { } greeting ? Results.Ok(greeting) : Results.NotFound());

app.Run();

record CreateGreetingRequest(string? Name, string? Language);
record Greeting(int Id, string Name, string Message);

sealed class GreetingService
{
    private readonly List<Greeting> _greetings = [];
    private readonly object _gate = new();

    public static string Message(string name, string? language) => language?.ToLowerInvariant() switch
    {
        "en" => $"Hello, {name}!",
        "ja" => $"Konnichiwa, {name}!",
        _ => $"Xin chao, {name}!"
    };

    public Greeting Create(string name, string? language)
    {
        lock (_gate)
        {
            var greeting = new Greeting(_greetings.Count + 1, name, Message(name, language));
            _greetings.Add(greeting);
            return greeting;
        }
    }

    public Greeting? Find(int id)
    {
        lock (_gate) return _greetings.FirstOrDefault(g => g.Id == id);
    }
}
