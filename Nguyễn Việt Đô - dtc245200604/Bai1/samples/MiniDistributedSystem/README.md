# MiniDistributedSystem – EduSubmit, lát cắt 1

Hai tiến trình độc lập giao tiếp qua HTTP (MEP yêu cầu–phản hồi):

- `src/Submission.Api` – máy chủ, Minimal API trên ASP.NET Core (.NET 10)
- `src/Submission.Client` – máy khách dòng lệnh dùng `HttpClient`

## Tự tạo lại từ đầu bằng .NET CLI

```bash
mkdir MiniDistributedSystem && cd MiniDistributedSystem
dotnet new sln -n MiniDistributedSystem
dotnet new web -n Submission.Api -o src/Submission.Api
dotnet new console -n Submission.Client -o src/Submission.Client
dotnet sln add src/Submission.Api/Submission.Api.csproj
dotnet sln add src/Submission.Client/Submission.Client.csproj
```

## Chạy

Terminal 1 (API):

```bash
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Terminal 2 (Client):

```bash
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Kết quả mong đợi:

```
Danh sach bai tap:
- 1: Bai tap 1
- 2: Bai tap 2
Da nop bai. Ma luot nop: <guid>
Trang thai: received
```

Có thể thử từng endpoint bằng `requests.http` (VS Code REST Client / Visual Studio) hoặc `curl`.

## Bài tập

1. Thêm bài tập mẫu thứ 3 và kiểm tra máy khách in đúng.
2. Tắt API rồi chạy máy khách: ghi lại lỗi và giải thích.
3. Gửi thiếu `StudentCode`/`RepositoryUrl`: quan sát 400 Bad Request.
4. (Mở rộng) Thêm `GET /api/submissions/{id}` và cho máy khách tra cứu lại lượt nộp.
