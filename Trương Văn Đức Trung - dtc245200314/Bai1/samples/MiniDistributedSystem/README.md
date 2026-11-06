# MiniDistributedSystem Bước 9

Mẫu dựng theo đặc tả tài liệu để quan sát hai tiến trình giao tiếp qua HTTP và thực hiện bốn thí nghiệm lỗi. Từ thư mục này:

```powershell
dotnet build
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Trong terminal khác tại cùng thư mục:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Đọc `../../docs/bai1.md` để xem dự đoán, kết quả và giải thích thí nghiệm. Mã nguồn đã được khôi phục về hai bài tập và `StudentCode: "SV001"` sau thí nghiệm. Phần mở rộng nằm ở dự án chính, cách thư mục này hai cấp.
