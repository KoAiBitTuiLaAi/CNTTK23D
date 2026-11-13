# Nguyễn Việt Đô - dtc245200604

Thư mục nộp bài của sinh viên Nguyễn Việt Đô - dtc245200604.

## Bài 1 – Tổng quan hệ thống và MEP

- Nhật ký và ảnh minh chứng: [docs/bai1.md](docs/bai1.md).
- Dự án EduSubmit: `EduSubmit.slnx`, gồm `src/Submission.Api` và `src/Submission.Client`.
- Kiểm thử bằng REST Client: `requests.http`.
- Các chương trình thực hành: `samples/MepDemo`, `samples/BasicWebApi`, `samples/MiniDistributedSystem`.
- `samples/BasicWebApi` đã bổ sung ngôn ngữ `ja`; hai chương trình mẫu còn lại giữ mã nguồn ban đầu.

### Chạy EduSubmit

Yêu cầu .NET SDK 10. Từ thư mục gốc repository, vào đúng thư mục bài trước khi build:

```powershell
cd "Nguyễn Việt Đô - dtc245200604/Bai1"
dotnet build
dotnet build EduSubmit.slnx

# Terminal 1
dotnet run --project src/Submission.Api --urls http://localhost:5000

# Terminal 2
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Hai project không tham chiếu mã nguồn trực tiếp vào nhau; dữ liệu trao đổi là JSON qua HTTP. API lưu lượt nộp trong bộ nhớ, vì vậy dữ liệu mất khi tắt/chạy lại API.

Mở `requests.http` với extension REST Client và gửi các request kiểm tra 200/201/400/404. Chạy request tạo lượt nộp trước request đọc lại để REST Client lấy đúng GUID từ phản hồi.

### Chạy mẫu

```powershell
dotnet run --project samples/MepDemo
dotnet run --project samples/MepDemo -- 3
dotnet run --project samples/MepDemo -- 4
dotnet run --project samples/BasicWebApi/src/BasicWebApi --urls http://localhost:5081
```

Dừng EduSubmit trước khi chạy MiniDistributedSystem vì cả hai mặc định dùng cổng 5000:

```powershell
dotnet run --project samples/MiniDistributedSystem/src/Submission.Api --urls http://localhost:5000
dotnet run --project samples/MiniDistributedSystem/src/Submission.Client -- http://localhost:5000
```
