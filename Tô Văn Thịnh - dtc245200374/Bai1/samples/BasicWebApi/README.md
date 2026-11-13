# BasicWebApi – luyện kỹ thuật Minimal API (Bài 1, PI 1.2)

Ví dụ của giáo trình mục 1.2: một API nhỏ để học cấu trúc solution, hosting model trong `Program.cs`,
endpoint GET/POST, nhận dữ liệu từ route / query string / body / DI và trả kết quả bằng `Results`.

## Tự tạo lại từ đầu

```bash
dotnet new sln -n BasicWebApi --format slnx
dotnet new webapi -n BasicWebApi -o src/BasicWebApi --no-openapi
dotnet sln BasicWebApi.slnx add src/BasicWebApi/BasicWebApi.csproj
```

Sau đó xóa code mẫu trong `src/BasicWebApi/Program.cs` và viết lại theo file trong thư mục này.

## Chạy

```bash
dotnet build
dotnet run --project src/BasicWebApi --urls http://localhost:5081
```

Kiểm thử bằng `requests.http` (VS Code REST Client) hoặc:

```powershell
curl.exe http://localhost:5081/
curl.exe "http://localhost:5081/hello/An?language=en"
```

| Request | Kết quả mong đợi |
|---|---|
| `GET /` | 200, `{"application":"BasicWebApi",...}` |
| `GET /hello/An?language=en` | 200, `{"message":"Hello, An!"}` |
| `POST /greetings {"name":"Binh"}` | 201, header `Location: /greetings/1`, JSON có `id, name, createdAt` |
| `POST /greetings {"name":""}` | 400, `{"message":"Name must not be empty."}` |
