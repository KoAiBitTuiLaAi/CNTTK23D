// EduSubmit – Submission.Api (máy chủ)
// Chạy: dotnet run --project src/Submission.Api --urls http://localhost:5000

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dữ liệu mẫu trong bộ nhớ
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Goi API tu client")
};

// Bài mở rộng: lưu các lượt nộp trong bộ nhớ
var submissions = new List<SubmissionResult>();

app.MapGet("/", () => Results.Ok(new {
    service = "Submission.Api",
    status = "running"
}));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", (CreateSubmissionRequest request) => {
    if (request.AssignmentId <= 0 ||
        string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl)) {
        return Results.BadRequest(new { message = "Du lieu nop bai khong hop le" });
    }

    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow);

    lock (submissions) {
        submissions.Add(submission);
    }

    return Results.Created($"/api/submissions/{submission.Id}", submission);
});

// Bài mở rộng: đọc lại một lượt nộp theo id
app.MapGet("/api/submissions/{id:guid}", (Guid id) => {
    SubmissionResult? submission;
    lock (submissions) {
        submission = submissions.FirstOrDefault(s => s.Id == id);
    }

    return submission is null ? Results.NotFound() : Results.Ok(submission);
});

app.Run();

record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt);
