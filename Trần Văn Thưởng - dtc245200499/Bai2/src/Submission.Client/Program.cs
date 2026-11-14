using System.Net;
using System.Net.Http.Json;

var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiUrl) };

try
{
    Console.WriteLine("1) Open books");
    using var listResponse = await http.GetAsync("/api/books");
    if (!listResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(listResponse);
        return;
    }

    var books = await listResponse.Content.ReadFromJsonAsync<List<BookDto>>() ?? [];
    foreach (var book in books)
        Console.WriteLine($"- {book.Id}: {book.Title} ({book.AvailableCopies} available)");

    var selectedBook = books.FirstOrDefault(book => book.AvailableCopies > 0);
    if (selectedBook is null)
    {
        Console.WriteLine("No book is currently available to reserve.");
        return;
    }

    Console.WriteLine("\n2) Reserve a book");
    using var createResponse = await http.PostAsJsonAsync(
        $"/api/books/{selectedBook.Id}/reservations",
        new CreateReservationRequest("DTC245200499", "Tran Van Thuong"));
    Console.WriteLine($"HTTP {(int)createResponse.StatusCode} {createResponse.StatusCode}");
    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        await PrintApiErrorAsync(createResponse);
        return;
    }

    var reservation = await createResponse.Content.ReadFromJsonAsync<ReservationDto>();
    var location = createResponse.Headers.Location;
    if (reservation is null || location is null)
    {
        Console.WriteLine("The API response is missing the reservation body or Location header.");
        return;
    }
    Console.WriteLine($"Created {reservation.Id}; Location: {location}");

    Console.WriteLine("\n3) Read the reservation from Location");
    using var readResponse = await http.GetAsync(location);
    if (!readResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(readResponse);
        return;
    }
    var readBack = await readResponse.Content.ReadFromJsonAsync<ReservationDto>();
    Console.WriteLine($"{readBack?.StudentCode}: {readBack?.BookTitle} ({readBack?.Status})");

    Console.WriteLine("\n4) Send an invalid request and read ApiError");
    using var invalidResponse = await http.PostAsJsonAsync(
        "/api/books/1/reservations", new CreateReservationRequest("", ""));
    Console.WriteLine($"HTTP {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    await PrintApiErrorAsync(invalidResponse);
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"Network error calling Library.Api at {apiUrl}: {exception.Message}");
    Environment.ExitCode = 1;
}

static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine(error is null
        ? "The response did not contain a JSON ApiError body."
        : $"{error.Code}: {error.Message}");
}

record BookDto(int Id, string Title, string Author, string Isbn, string Category, int AvailableCopies);
record CreateReservationRequest(string StudentCode, string StudentName);
record ReservationDto(Guid Id, int BookId, string BookTitle, string StudentCode,
    string StudentName, string Status, DateTimeOffset ReservedAt);
record ApiError(string Code, string Message);
