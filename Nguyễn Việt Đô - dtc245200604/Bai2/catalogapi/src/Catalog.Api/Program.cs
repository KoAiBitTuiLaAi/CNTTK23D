var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET /: thông tin dịch vụ để kiểm tra máy chủ đang chạy.
app.MapGet("/", () => Results.Ok(new { service = "Catalog.Api", status = "running" }));

// Dữ liệu mẫu trong bộ nhớ tiến trình máy chủ.
var products = new List<ProductDto>
{
    new(1, "Giao trinh Lap trinh phan tan", "Giao trinh chinh thuc", 150000m),
    new(2, "Tai lieu RESTful API", "Huong dan thiet ke API", 90000m),
    new(3, "Bai tap thuc hanh .NET", "Bo bai tap kem loi giai", 120000m)
};
var productsLock = new object();
var nextId = 4;

// GET /api/products: chụp danh sách dưới khóa, rồi mới trả JSON.
app.MapGet("/api/products", (string? keyword) =>
{
    lock (productsLock)
    {
        var matches = string.IsNullOrWhiteSpace(keyword)
            ? products
            : products.Where(item => item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        return Results.Ok(matches.ToArray());
    }
});

// :int chỉ cho ID số nguyên đi vào handler; /abc không khớp route.
app.MapGet("/api/products/{id:int}", (int id) =>
{
    lock (productsLock)
    {
        var product = products.FirstOrDefault(item => item.Id == id);
        return product is null
            ? Results.NotFound(new ApiError("PRODUCT_NOT_FOUND", "Khong tim thay san pham."))
            : Results.Ok(product);
    }
});

// POST /api/products: máy chủ sinh ID và trả địa chỉ đọc lại.
app.MapPost("/api/products", (CreateProductRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0)
    {
        return Results.BadRequest(new ApiError(
            "INVALID_PRODUCT",
            "Ten san pham khong duoc de trong va gia phai lon hon 0."));
    }

    if (request.Name.Length > 100)
    {
        return Results.BadRequest(new ApiError(
            "NAME_TOO_LONG", "Ten san pham khong duoc vuot qua 100 ky tu."));
    }

    // Sinh ID và thêm vào danh sách là một thao tác nguyên tử.
    lock (productsLock)
    {
        var product = new ProductDto(
            nextId++,
            request.Name.Trim(),
            request.Description?.Trim() ?? "",
            request.Price);
        products.Add(product);
        return Results.Created($"/api/products/{product.Id}", product);
    }
});

app.MapDelete("/api/products/{id:int}", (int id) =>
{
    lock (productsLock)
    {
        var product = products.FirstOrDefault(item => item.Id == id);
        if (product is null)
            return Results.NotFound(new ApiError("PRODUCT_NOT_FOUND", "Khong tim thay san pham."));
        products.Remove(product);
        return Results.NoContent();
    }
});

app.Run();

record ProductDto(int Id, string Name, string Description, decimal Price);
record CreateProductRequest(string Name, string? Description, decimal Price);
record ApiError(string Code, string Message);
