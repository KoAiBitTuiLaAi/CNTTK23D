var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new {
    service = "Submission.Api",
    status = "running"
}));

app.Run();
