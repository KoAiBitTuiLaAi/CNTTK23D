var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new List<AssignmentDto>
{
    new(1, "Bai tap REST API", "Thiet ke endpoint RESTful va ma trang thai HTTP.", true),
    new(2, "Bai tap Client", "Viet console client goi API bang HttpClient.", true),
    new(3, "Bai tap da dong", "Dung de kiem tra loi ASSIGNMENT_CLOSED.", false)
};

app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.Run();

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record ApiError(string Code, string Message);
