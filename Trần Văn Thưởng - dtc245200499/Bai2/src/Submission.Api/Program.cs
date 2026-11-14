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

app.MapGet("/api/books/{bookId:int}", (int bookId) =>
{
    var book = books.FirstOrDefault(item => item.Id == bookId);
    return book is null
        ? Results.NotFound(new ApiError("BOOK_NOT_FOUND", $"Book {bookId} was not found."))
        : Results.Ok(book);
});

app.MapPost("/api/books/{bookId:int}/reservations", (int bookId, CreateReservationRequest? request) =>
{
    var book = books.FirstOrDefault(item => item.Id == bookId);
    if (book is null)
        return Results.NotFound(new ApiError("BOOK_NOT_FOUND", $"Book {bookId} was not found."));

    if (request is null || string.IsNullOrWhiteSpace(request.StudentCode)
        || string.IsNullOrWhiteSpace(request.StudentName))
        return Results.BadRequest(new ApiError("INVALID_RESERVATION", "StudentCode and StudentName are required."));

    if (book.AvailableCopies <= 0)
        return Results.Conflict(new ApiError("NO_COPIES_AVAILABLE", "No available copies remain for this book."));

    book.AvailableCopies--;
    var reservation = new ReservationDto(Guid.NewGuid(), book.Id, book.Title,
        request.StudentCode.Trim(), request.StudentName.Trim(), "reserved", DateTimeOffset.UtcNow);
    reservations.Add(reservation);
    return Results.Created($"/api/reservations/{reservation.Id}", reservation);
});

app.MapGet("/api/reservations/{reservationId:guid}", (Guid reservationId) =>
{
    var reservation = reservations.FirstOrDefault(item => item.Id == reservationId);
    return reservation is null
        ? Results.NotFound(new ApiError("RESERVATION_NOT_FOUND", $"Reservation {reservationId} was not found."))
        : Results.Ok(reservation);
});

app.Run();

record BookDto(int Id, string Title, string Author, string Isbn, string Category, int AvailableCopies)
{
    public int AvailableCopies { get; set; } = AvailableCopies;
}
record CreateReservationRequest(string StudentCode, string StudentName);
record ReservationDto(Guid Id, int BookId, string BookTitle, string StudentCode,
    string StudentName, string Status, DateTimeOffset ReservedAt);
record ApiError(string Code, string Message);
