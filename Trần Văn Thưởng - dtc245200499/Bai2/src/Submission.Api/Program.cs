var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var books = new List<BookDto>
{
    new(1, "Clean Code", "Robert C. Martin", "9780132350884", "Programming", 3),
    new(2, "The Pragmatic Programmer", "Andrew Hunt and David Thomas", "9780201616224", "Programming", 2),
    new(3, "Designing Data-Intensive Applications", "Martin Kleppmann", "9781449373320", "Systems", 1),
    new(4, "Computer Networking: A Top-Down Approach", "James Kurose and Keith Ross", "9780136681557", "Networking", 0)
};
var reservations = new List<ReservationDto>();

app.MapGet("/", () => Results.Ok(new { service = "Library.Api", status = "running" }));

app.MapGet("/api/books", (string? keyword) =>
{
    var result = string.IsNullOrWhiteSpace(keyword)
        ? books
        : books.Where(book => book.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || book.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || book.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(result);
});

app.Run();

record BookDto(int Id, string Title, string Author, string Isbn, string Category, int AvailableCopies);
