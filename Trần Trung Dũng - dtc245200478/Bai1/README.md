# EduSubmit

Solution gồm `Submission.Api` và `Submission.Client`, hai tiến trình riêng giao tiếp qua HTTP. API giữ dữ liệu mẫu trong bộ nhớ và cung cấp `GET /`, `GET /api/assignments`, `POST /api/submissions` cùng endpoint mở rộng `GET /api/submissions/{id:guid}`.

## Chạy

Terminal 1:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Terminal 2:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Thử các request trong `requests.http` bằng REST Client. API lưu lượt nộp trong bộ nhớ nên dữ liệu bị xóa khi khởi động lại.
