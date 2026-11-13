var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GreetingService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { application = "BasicWebApi", status = "running" }));

app.MapGet("/hello/{name}", (string name, string? language, GreetingService greetings) =>
    Results.Ok(new { message = greetings.SayHello(name, language) }));

app.MapPost("/greetings", (GreetingRequest? request, GreetingService greetings) =>
{
    if (request is null || string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { message = "Name must not be empty." });
    }

    var greeting = greetings.Create(request.Name.Trim());
    return Results.Created($"/greetings/{greeting.Id}", greeting);
});

app.MapGet("/greetings/{id:int}", (int id, GreetingService greetings) =>
{
    var greeting = greetings.Find(id);
    return greeting is null ? Results.NotFound() : Results.Ok(greeting);
});

app.Run();

sealed class GreetingService
{
    private readonly List<Greeting> _greetings = [];

    public string SayHello(string name, string? language) => language?.ToLowerInvariant() switch
    {
        "ja" => $"Konnichiwa, {name}!",
        _ => $"Hello, {name}!"
    };

    public Greeting Create(string name)
    {
        var greeting = new Greeting(_greetings.Count + 1, name);
        _greetings.Add(greeting);
        return greeting;
    }

    public Greeting? Find(int id) => _greetings.FirstOrDefault(greeting => greeting.Id == id);
}

record GreetingRequest(string Name);
record Greeting(int Id, string Name);
