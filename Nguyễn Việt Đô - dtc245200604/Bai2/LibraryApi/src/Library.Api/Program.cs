var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "Library.Api" }));

var books = new List<BookDto>();
var gate = new object();
var nextId = 1;

app.MapGet("/api/books", () =>
{
    lock (gate) return Results.Ok(books.ToArray());
});

app.MapPost("/api/books", (CreateBookRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) ||
        string.IsNullOrWhiteSpace(request.Author) || request.Price <= 0)
        return Results.BadRequest(new ApiError("INVALID_BOOK",
            "Title and author are required; price must be positive."));
    lock (gate)
    {
        var book = new BookDto(nextId++, request.Title, request.Author, request.Price);
        books.Add(book);
        return Results.Created($"/api/books/{book.Id}", book);
    }
});

app.MapGet("/api/books/{id:int}", (int id) =>
{
    lock (gate)
    {
        var book = books.Find(book => book.Id == id);
        return book is null
            ? Results.NotFound(new ApiError("BOOK_NOT_FOUND", "Book not found."))
            : Results.Ok(book);
    }
});

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    lock (gate)
    {
        var book = books.Find(book => book.Id == id);
        if (book is null)
            return Results.NotFound(new ApiError("BOOK_NOT_FOUND", "Book not found."));
        books.Remove(book);
        return Results.NoContent();
    }
});

app.Run();

public record ApiError(string Code, string Message);

public record BookDto(int Id, string Title, string Author, decimal Price);
public record CreateBookRequest(string Title, string Author, decimal Price);
