var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var products = new List<ProductDto>
{
new(1, "Giao trinh Lap trinh phan tan", "Giao trinh chinh thuc", 150000),
new(2, "Tai lieu RESTful API", "Huong dan thiet ke API", 90000),
new(3, "Bai tap thuc hanh .NET", "Bo bai tap kem loi giai", 120000)
};

app.MapGet("/", () => Results.Ok(new
{
    service = "Catalog.Api",
    status = "running"
}));

app.MapGet("/api/products", () => Results.Ok(products));

app.MapGet("/api/products/{id:int}", (int id) => {
var product = products.FirstOrDefault(p => p.Id == id);
return product is null
? Results.NotFound(new ApiError("PRODUCT_NOT_FOUND", "Khong tim thay san pham."))
: Results.Ok(product);
});

// POST /api/products : tạo mới từ body JSON
app.MapPost("/api/products", (CreateProductRequest request) => {
// Kiểm tra tại biên hệ thống
if (string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0) {
return Results.BadRequest(new ApiError(
"INVALID_PRODUCT",
"Ten san pham khong duoc de trong va gia phai lon hon 0."));
}
// ... phần tạo sản phẩm: slide sau ...
// (tiếp sau khối if, trước dấu }); )
var product = new ProductDto(
Id: products.Max(p => p.Id) + 1,
Name: request.Name.Trim(),
Description: request.Description?.Trim() ?? "",
Price: request.Price);
products.Add(product);
return Results.Created($"/api/products/{product.Id}", product);
});

app.Run();
record ProductDto(int Id, string Name, string Description, decimal Price);
record CreateProductRequest(string Name, string? Description, decimal Price);  // đầu vào
record ApiError(string Code, string Message);