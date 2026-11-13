using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5100";
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// Nac 1: liet ke san pham
using (var listResponse = await http.GetAsync("/api/products"))
{
    if (!listResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Danh sach loi: {(int)listResponse.StatusCode} {listResponse.StatusCode}");
        Console.WriteLine(await listResponse.Content.ReadAsStringAsync());
        return;
    }

    var products = await listResponse.Content.ReadFromJsonAsync<List<ProductDto>>()
        ?? new List<ProductDto>();

    Console.WriteLine("Danh sach san pham:");
    foreach (var product in products)
    {
        Console.WriteLine($"[{product.Id}] {product.Name} - {product.Price:N0} VND");
    }
}

// Nac 2: tao san pham, kiem tra 201 truoc khi doc body
var createRequest = new CreateProductRequest(
    Name: "Nhap mon REST API",
    Description: "Tai lieu buoc 8",
    Price: 80000);

using var createResponse = await http.PostAsJsonAsync("/api/products", createRequest);
if (createResponse.StatusCode != HttpStatusCode.Created)
{
    Console.WriteLine($"Tao san pham loi: {(int)createResponse.StatusCode} {createResponse.StatusCode}");
    Console.WriteLine(await createResponse.Content.ReadAsStringAsync());
    return;
}

var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
var location = createResponse.Headers.Location;
Console.WriteLine($"Da tao san pham moi: [{createdProduct?.Id}] {createdProduct?.Name}");
Console.WriteLine($"Doc lai tai: {location}");

// Nac 3: doc chi tiet mot san pham
using var detailResponse = await http.GetAsync("/api/products/1");
if (!detailResponse.IsSuccessStatusCode)
{
    Console.WriteLine($"Doc chi tiet loi: {(int)detailResponse.StatusCode} {detailResponse.StatusCode}");
    Console.WriteLine(await detailResponse.Content.ReadAsStringAsync());
    return;
}

var detail = await detailResponse.Content.ReadFromJsonAsync<ProductDto>();
Console.WriteLine($"Chi tiet san pham 1: {detail?.Name}");

// Nac 4: gui du lieu sai, kiem tra 400 truoc khi doc ApiError
var invalidRequest = new CreateProductRequest(" ", "", -1);
using var invalidResponse = await http.PostAsJsonAsync("/api/products", invalidRequest);
Console.WriteLine($"Thu gui du lieu sai: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");

if (invalidResponse.StatusCode != HttpStatusCode.BadRequest)
{
    Console.WriteLine(await invalidResponse.Content.ReadAsStringAsync());
    return;
}

var apiError = await invalidResponse.Content.ReadFromJsonAsync<ApiError>();
Console.WriteLine($"{apiError?.Code}: {apiError?.Message}");

record ProductDto(int Id, string Name, string Description, decimal Price);
record CreateProductRequest(string Name, string Description, decimal Price);
record ApiError(string Code, string Message);
