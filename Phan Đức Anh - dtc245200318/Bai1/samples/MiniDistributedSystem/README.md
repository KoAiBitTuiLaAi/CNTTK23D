# MiniDistributedSystem

Chương trình được dựng theo đặc tả Bước 9–11 vì không có bản mẫu này trong đầu vào.
API và client là hai project độc lập, chỉ giao tiếp bằng HTTP/JSON.

Terminal 1:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Terminal 2:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Các thí nghiệm thay đổi mã nguồn chạy trên bản sao tạm bằng script kiểm thử,
không để lại thay đổi trong bản mẫu này.
