# BasicWebApi Bước 8

Mẫu dựng theo đặc tả trong tài liệu Bài 1, đã bổ sung tiếng Nhật.

Từ thư mục này:

```powershell
dotnet build
dotnet run --project src/BasicWebApi --urls http://localhost:5081
```

Mở `requests.http` bằng REST Client. `GET /hello/An?language=ja` trả `Konnichiwa, An!`. POST tên hợp lệ trả 201 và Location; tên rỗng trả 400.

Để debug từ thư mục gốc bài nộp, chọn cấu hình `Bai 1 BasicWebApi breakpoint` trong Run and Debug. Đặt breakpoint tại `var greeting = service.Create(...)`, nhấn F5, rồi gửi POST hợp lệ. F10 chạy từng dòng, F5 tiếp tục và Shift+F5 dừng.
