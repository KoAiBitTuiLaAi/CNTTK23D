var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));
var assignments = new List<Assignment>
{
    new(1, "Web API cơ bản", "Xây dựng API nhận bài nộp bằng ASP.NET Core."),
    new(2, "HTTP Client", "Gọi API, gửi bài và đọc lại kết quả bằng HttpClient.")
};
var submissions = new List<SubmissionResult>();
var submissionsLock = new object();
app.MapGet("/api/assignments", () => Results.Ok(assignments));
app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
{
    if (!assignments.Any(assignment => assignment.Id == request.AssignmentId))
        return Results.BadRequest(new { error = "Bài tập không tồn tại." });
    if (string.IsNullOrWhiteSpace(request.StudentCode))
        return Results.BadRequest(new { error = "Mã sinh viên không được để trống." });
    if (!Uri.TryCreate(request.RepositoryUrl, UriKind.Absolute, out var repository)
        || (repository.Scheme != Uri.UriSchemeHttp && repository.Scheme != Uri.UriSchemeHttps)
        || string.IsNullOrWhiteSpace(repository.Host))
        return Results.BadRequest(new { error = "URL kho mã phải là địa chỉ HTTP/HTTPS tuyệt đối." });
    var result = new SubmissionResult(Guid.NewGuid(), request.AssignmentId,
        request.StudentCode, request.RepositoryUrl, "received", DateTimeOffset.UtcNow);
    // Dùng cùng một khóa cho mọi thao tác đọc, ghi danh sách dùng chung.
    lock (submissionsLock)
        submissions.Add(result);
    return Results.Created($"/api/submissions/{result.Id}", result);
});
app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    SubmissionResult? result;
    lock (submissionsLock)
        result = submissions.FirstOrDefault(submission => submission.Id == id);
    return result is null ? Results.NotFound() : Results.Ok(result);
});
app.Run();

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string? StudentCode, string? RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string? StudentCode,
    string? RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
