using System.Net;
using System.Net.Http.Json;

try
{
    using var client = new HttpClient
    {
        BaseAddress = new Uri(args.Length > 0 ? args[0].TrimEnd('/') + "/" : "http://localhost:5200/"),
        Timeout = TimeSpan.FromSeconds(10)
    };
    using var listResponse = await client.GetAsync("api/books");
    listResponse.EnsureSuccessStatusCode();
    var books = await listResponse.Content.ReadFromJsonAsync<List<BookDto>>()
        ?? throw new InvalidOperationException("Missing book list.");
    Console.WriteLine($"1. LIST {(int)listResponse.StatusCode}: {books.Count} book(s)");

    using var createResponse = await client.PostAsJsonAsync("api/books",
        new CreateBookRequest("The Pragmatic Programmer", "Andrew Hunt", 250.50m));
    // Check the status before treating a response body as BookDto.
    if (createResponse.StatusCode != HttpStatusCode.Created)
        throw new InvalidOperationException($"Expected 201, received {(int)createResponse.StatusCode}.");
    var created = await createResponse.Content.ReadFromJsonAsync<BookDto>()
        ?? throw new InvalidOperationException("Missing created book.");
    var location = createResponse.Headers.Location
        ?? throw new InvalidOperationException("Missing Location header.");
    Console.WriteLine($"2. CREATE 201: Id={created.Id}; Location={location}");

    using var readResponse = await client.GetAsync(location);
    readResponse.EnsureSuccessStatusCode();
    var found = await readResponse.Content.ReadFromJsonAsync<BookDto>()
        ?? throw new InvalidOperationException("Missing book at Location.");
    if (found != created)
        throw new InvalidOperationException("Book at Location differs from the created book.");
    Console.WriteLine($"3. READ {(int)readResponse.StatusCode}: Id={found.Id}; {found.Title}");

    using var invalidResponse = await client.PostAsJsonAsync("api/books",
        new CreateBookRequest("", "", 0));
    if (invalidResponse.StatusCode != HttpStatusCode.BadRequest)
        throw new InvalidOperationException($"Expected 400, received {(int)invalidResponse.StatusCode}.");
    var error = await invalidResponse.Content.ReadFromJsonAsync<ApiError>()
        ?? throw new InvalidOperationException("Missing ApiError.");
    if (error.Code != "INVALID_BOOK")
        throw new InvalidOperationException($"Unexpected error code: {error.Code}.");
    Console.WriteLine($"4. INVALID 400: {error.Code}; {error.Message}");

    return 0;
}
catch (Exception error) when (error is HttpRequestException or TaskCanceledException
    or System.Text.Json.JsonException or NotSupportedException
    or InvalidOperationException or UriFormatException or ArgumentException)
{
    Console.Error.WriteLine($"CLIENT_ERROR: {error.Message}");
    return 1;
}

public record ApiError(string Code, string Message);
public record BookDto(int Id, string Title, string Author, decimal Price);
public record CreateBookRequest(string Title, string Author, decimal Price);
