var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET / : kiem tra may chu dang chay
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

app.Run();

