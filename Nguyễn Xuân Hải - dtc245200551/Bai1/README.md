# Bài 1 - Ứng dụng phân tán

**Sinh viên:** Nguyễn Xuân Hải  
**Mã sinh viên:** dtc245200551  
**Môn học:** Phát triển ứng dụng phân tán

## 1. Giới thiệu
Bài thực hành xây dựng ứng dụng gồm API và Client bằng .NET. Client giao tiếp với API qua HTTP để lấy danh sách bài tập, gửi bài nộp và tra cứu bài nộp theo mã GUID.

## 2. Cấu trúc dự án
```text
Bai1/
├── EduSubmit.slnx
├── Submission.Api/
│   ├── Program.cs
│   └── Submission.Api.csproj
├── Submission.Client/
│   ├── Program.cs
│   └── Submission.Client.csproj
├── requests.http
├── README.md
├── docs/
│   └── bai1.md
└── screenshots/
```

> Không đưa các thư mục `bin/` và `obj/` lên GitHub.

## 3. Công nghệ sử dụng
- C# và .NET
- ASP.NET Core Minimal API
- HttpClient
- HTTP/JSON
- Git và GitHub

## 4. Chạy chương trình
Mở Terminal tại thư mục `Bai1`:

**Terminal 1 - chạy API**
```powershell
dotnet run --project Submission.Api -- --urls http://localhost:5000
```

**Terminal 2 - chạy Client**
```powershell
dotnet run --project Submission.Client
```

Chạy Client với API ở cổng khác:
```powershell
dotnet run --project Submission.Client -- http://localhost:5050
```

## 5. Các API
| Phương thức | Đường dẫn | Chức năng |
|---|---|---|
| GET | `/` | Kiểm tra trạng thái API |
| GET | `/api/assignments` | Lấy danh sách bài tập |
| POST | `/api/submissions` | Gửi bài nộp |
| GET | `/api/submissions/{id}` | Tra cứu bài nộp theo GUID |

## 6. Kết quả kiểm thử
Các mã trạng thái cần kiểm tra:
- `200 OK`: lấy dữ liệu hoặc tra cứu bài nộp thành công.
- `201 Created`: tạo bài nộp thành công.
- `400 Bad Request`: dữ liệu gửi lên không hợp lệ.
- `404 Not Found`: không tìm thấy bài nộp theo GUID.

Chi tiết thực hiện và bốn tình huống thử nghiệm được trình bày trong [`docs/bai1.md`](docs/bai1.md).
