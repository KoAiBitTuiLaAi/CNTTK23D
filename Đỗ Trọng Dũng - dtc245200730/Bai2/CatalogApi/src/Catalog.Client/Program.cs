using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5100";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

// Bước 1: lấy danh sách sản phẩm
var products = await http.GetFromJsonAsync<List<ProductDto>>("/api/products");
Console.WriteLine("Danh sach san pham:");
foreach (var p in products ?? [])
{
    Console.WriteLine($"  [{p.Id}] {p.Name} - {p.Price:N0} VND");
}

// cuối tệp
record ProductDto(int Id, string Name, string Description, decimal Price);
