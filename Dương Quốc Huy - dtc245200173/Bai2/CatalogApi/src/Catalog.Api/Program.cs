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

app.Run();

record ProductDto(int Id, string Name, string Description, decimal Price);
record ApiError(string Code, string Message);
