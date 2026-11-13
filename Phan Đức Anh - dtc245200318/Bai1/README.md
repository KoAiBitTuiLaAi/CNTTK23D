# EduSubmit Bài 1

Họ tên: **Phan Đức Anh**

MSSV: **DTC245200318**

Lớp: **CNTT K23D**

Học phần: **Phát triển ứng dụng phân tán**

EduSubmit gồm hai tiến trình độc lập: `Submission.Api` và `Submission.Client`,
giao tiếp qua HTTP/JSON theo mô hình request–response. Bài có kiểm tra dữ liệu,
lưu lượt nộp trong bộ nhớ và đọc lại lượt nộp theo GUID.

## Chạy nhanh

Cần .NET 10 SDK. Mở terminal trong thư mục chứa `EduSubmit.slnx`.

```powershell
dotnet build EduSubmit.slnx
```

Terminal thứ nhất:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Terminal thứ hai:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Có thể đổi cổng ở cả hai lệnh, ví dụ `5050`. Client không tham chiếu project API.
Khi tắt và chạy lại API, dữ liệu lượt nộp sẽ mất vì bài này chưa dùng cơ sở dữ liệu.

## Cấu trúc

```text
Bai1/
├── EduSubmit.slnx
├── src/
│   ├── Submission.Api/
│   └── Submission.Client/
├── requests.http
├── docs/
│   ├── bai1.md
│   ├── du-doan-thi-nghiem.md
│   └── evidence/
├── scripts/Verify-Bai1.ps1
└── samples/
    ├── MepDemo/
    ├── BasicWebApi/
    └── MiniDistributedSystem/
```

`MepDemo` giữ nguyên mã nguồn đã được cung cấp. Hai ví dụ còn thiếu được dựng
theo đặc tả trong tài liệu hướng dẫn, không phải bản mẫu nguyên gốc của giảng viên.

## Kiểm thử

Trong VS Code, cài REST Client, mở `requests.http`, bấm **Send Request** trên mỗi
request. Chạy request có tên `createSubmission` trước khi đọc lại ID trong request
số 5. Khi khởi động lại API phải tạo lượt nộp mới.

| Endpoint | Kết quả |
| --- | --- |
| GET / | 200, trạng thái dịch vụ |
| GET /api/assignments | 200, danh sách bài tập |
| POST /api/submissions hợp lệ | 201, GUID, received, receivedAt và Location |
| POST thiếu trường hoặc assignmentId ≤ 0 | 400 |
| GET /api/submissions/{id:guid} có thật | 200 |
| GET /api/submissions/{id:guid} không có | 404 |

Kiểm thử tự động và chạy lại các thí nghiệm trên Windows PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Verify-Bai1.ps1
```

Script chạy các API localhost, kiểm tra phản hồi thật và ghi log trong `docs/evidence`.
Các cổng 5000, 5050 và 5081 cần đang trống. Script chỉ dừng tiến trình do nó tạo.
Thí nghiệm thay đổi code chạy trong `.experiments`, không sửa mã nguồn chính.
Thư mục tạm, `bin` và `obj` không thuộc bài nộp.

## Debug

BasicWebApi được nạp trong nhóm samples của solution để xem và debug mã mẫu.

Build trước, dừng API đang chạy, đặt breakpoint ở dòng
`var greeting = service.Create(...)` trong ví dụ BasicWebApi. Mở Run and Debug,
chọn **Debug BasicWebApi**, nhấn F5 rồi gửi POST trong file HTTP của ví dụ.
F10 chạy từng dòng, F5 chạy tiếp, Shift+F5 dừng debug.

Đã kiểm chứng điểm dừng, xem biến request, Step Over và Continue. Kết quả REST
Client và debugger được trình bày bằng văn bản, JSON và bảng trong nhật ký.

## Nộp bài

Bài đặt tại `Phan Đức Anh - dtc245200318/Bai1` trong fork
`anhphann1412-max/CNTTK23D`. PR gửi về `PTUDPTK23/CNTTK23D:main`, tiêu đề:

```text
Bai1 - dtc245200318 - Phan Đức Anh
```

Nhật ký và bằng chứng chạy thử: [docs/bai1.md](docs/bai1.md).

Đã mở [Pull Request số 8](https://github.com/PTUDPTK23/CNTTK23D/pull/8) với
tiêu đề đúng mẫu. GitHub gắn nhãn **Đúng hạn** lúc 09:34 ngày 06/11/2026.
