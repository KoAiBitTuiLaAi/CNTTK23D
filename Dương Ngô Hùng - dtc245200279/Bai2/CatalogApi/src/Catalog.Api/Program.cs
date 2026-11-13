// ① Khởi tạo
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ② Dữ liệu mẫu trong bộ nhớ (Bước 3)
var products = new List<ProductDto>
{
    new(1, "Giao trinh Lap trinh phan tan", "Giao trinh chinh thuc", 150000),
    new(2, "Tai lieu RESTful API", "Huong dan thiet ke API", 90000),
    new(3, "Bai tap thuc hanh .NET", "Bo bai tap kem loi giai", 120000)
};

// ③ Các endpoint

// GET / : kiểm tra máy chủ đang chạy (Bước 2)
app.MapGet("/", () => Results.Ok(new {
    service = "Catalog.Api",
    status = "running"
}));

// GET /api/products : danh sách sản phẩm (Bước 3)
app.MapGet("/api/products", () => Results.Ok(products));

// GET /api/products/{id:int} : chi tiết một sản phẩm (Bước 4)
app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null
        ? Results.NotFound(new ApiError("PRODUCT_NOT_FOUND", "Khong tim thay san pham."))
        : Results.Ok(product);
});

// ④ Chạy
app.Run();

// ⑤ Khai báo kiểu: DTO là hình dạng dữ liệu đi qua mạng
record ProductDto(int Id, string Name, string Description, decimal Price);
record ApiError(string Code, string Message);
