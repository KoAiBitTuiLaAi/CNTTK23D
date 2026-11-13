using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

try
{
    return await RunAsync(args);
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"Khong ket noi duoc API: {exception.Message}");
    return 2;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine("Khong ket noi duoc API: het thoi gian cho.");
    return 2;
}

catch (Exception exception) when (exception is InvalidOperationException or JsonException or ArgumentException or UriFormatException)
{
    Console.Error.WriteLine($"Loi Client: {exception.Message}");
    return 1;
}

static async Task<int> RunAsync(string[] args)
{
    using var http = new HttpClient
    {
        BaseAddress = new Uri(args.Length > 0 ? args[0] : "http://localhost:5100"),
        Timeout = TimeSpan.FromSeconds(10)
    };
    var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    using var listResponse = await http.GetAsync("/api/products");
    var products = await ReadAsync<ProductDto[]>(listResponse, "[1] GET /api/products", HttpStatusCode.OK);
    Console.WriteLine($"So san pham: {products.Length}");
    foreach (var product in products)
        Console.WriteLine($"{product.Id}: {product.Name} - {product.Price}");

    var input = new CreateProductRequest("Nhap mon REST API", "Sach tham khao", 120000m);
    using var createResponse = await http.PostAsJsonAsync("/api/products", input);
    var created = await ReadAsync<ProductDto>(createResponse, "[2] POST /api/products", HttpStatusCode.Created);
    Console.WriteLine($"Location: {createResponse.Headers.Location}");
    Console.WriteLine($"Da tao ID: {created.Id}");

    using var detailResponse = await http.GetAsync($"/api/products/{created.Id}");
    var detail = await ReadAsync<ProductDto>(detailResponse, $"[3] GET /api/products/{created.Id}", HttpStatusCode.OK);
    Console.WriteLine($"Chi tiet: {detail.Id}: {detail.Name} - {detail.Description} - {detail.Price}");

    using var invalidResponse = await http.PostAsJsonAsync("/api/products", new CreateProductRequest("", "", -1m));
    var error = await ReadAsync<ApiError>(invalidResponse, "[4] POST /api/products", HttpStatusCode.BadRequest);
    Console.WriteLine($"{error.Code}: {error.Message}");
    return error.Code == "INVALID_PRODUCT" ? 0 : 1;

    async Task<T> ReadAsync<T>(HttpResponseMessage response, string step, HttpStatusCode expected)
    {
        // In trạng thái trước khi đọc hoặc giải mã body.
        Console.WriteLine($"{step}: {(int)response.StatusCode} {response.ReasonPhrase}");
        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != expected)
        {
            var message = "Body rong.";
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var apiError = JsonSerializer.Deserialize<ApiError>(body, json);
                    message = apiError?.Code is not null && apiError.Message is not null
                        ? $"{apiError.Code}: {apiError.Message}"
                        : body;
                }
                catch (JsonException)
                {
                    message = body;
                }
            }
            throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {message}");
        }
        if (string.IsNullOrWhiteSpace(body))
            throw new JsonException("Body JSON rong.");
        return JsonSerializer.Deserialize<T>(body, json)
            ?? throw new JsonException("Body JSON rong.");
    }
}

// DTO độc lập: Client chỉ trao đổi JSON, không tham chiếu project API.
record ProductDto(int Id, string Name, string Description, decimal Price);
record CreateProductRequest(string Name, string? Description, decimal Price);
record ApiError(string Code, string Message);
