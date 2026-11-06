# EduSubmit Bài 1

**Trương Văn Đức Trung · DTC245200314 · CNTT K23D**

Bài thực hành Phát triển ứng dụng phân tán: hai tiến trình độc lập `Submission.Api` và `Submission.Client` giao tiếp bằng HTTP và JSON theo mô hình request–response. Đã có phần mở rộng lưu lượt nộp trong bộ nhớ, đọc lại bằng id và trả 404 cho id không tồn tại.

## Chạy bài chính

Yêu cầu .NET SDK 10. Mở terminal tại thư mục chứa `EduSubmit.slnx`.

```powershell
dotnet build EduSubmit.slnx
```

Terminal 1 chạy API:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Terminal 2 chạy Client:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Client in danh sách hai bài tập, mã lượt nộp GUID, trạng thái `received`, rồi đọc lại lượt nộp từ API. Mã sinh viên mặc định là `DTC245200314`. `repositoryUrl` mặc định dùng URL minh họa `https://example.com/edusubmit.git`; có thể truyền URL thật ở tham số thứ ba:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000 DTC245200314 https://github.com/dtc245200314-del/CNTTK23D
```

Đổi cổng bằng tham số, ví dụ API dùng `--urls http://localhost:5050` và Client nhận `http://localhost:5050`.

## Kiểm thử

Mở `requests.http` bằng VS Code và dùng REST Client. Chạy request tạo lượt nộp trước request đọc lại; id được lấy từ phản hồi có tên `createSubmission`.

| API | Kết quả |
|---|---|
| `GET /` | 200, tên dịch vụ và trạng thái |
| `GET /api/assignments` | 200, hai bài tập |
| `POST /api/submissions` hợp lệ | 201, GUID mới, `Location`, trạng thái `received` |
| `POST /api/submissions` thiếu dữ liệu hoặc id không hợp lệ | 400 |
| `GET /api/submissions/{id:guid}` có thật | 200, dữ liệu đã lưu |
| `GET /api/submissions/{id:guid}` không tồn tại | 404 |

Có thể chạy kiểm tra nhanh bằng PowerShell 7 có sẵn trên máy:

```powershell
pwsh -NoProfile -File scripts/kiem-tra.ps1
```

Bản ghi kiểm thử thực tế nằm trong `docs/evidence/`: **68/68 kiểm tra đạt**. Đã đối chiếu thêm với chương trình `.github/scripts/chay_thu.py` của kho lớp: các kiểm tra chạy được và phần mở rộng đều đạt. Đây là kết quả chạy thử, điểm chính thức do giảng viên xác nhận.

## Nội dung thư mục

```text
EduSubmit.slnx
src/Submission.Api/       API chính
src/Submission.Client/    Client chính, không tham chiếu project API
requests.http             Kiểm thử 200, 201, 400, 404 và dữ liệu thiếu
docs/bai1.md               Nhật ký và trả lời câu hỏi
docs/evidence/             Log chạy thật và ảnh trình bày bản ghi
.vscode/                   Cấu hình build và debug
samples/MepDemo/           Bản MepDemo đã được cung cấp
samples/BasicWebApi/       Dựng theo đặc tả Bước 8, có language=ja
samples/MiniDistributedSystem/  Dựng theo đặc tả Bước 9 và dùng thí nghiệm lỗi
samples/HelloPtudpt/       Kiểm tra môi trường
```

Hai mẫu BasicWebApi và MiniDistributedSystem được dựng lại từ đặc tả của tài liệu vì gói đầu vào chỉ có MepDemo. Mã và nhật ký được chuẩn bị với sự hỗ trợ của Codex; các kết quả kiểm thử ghi trong nhật ký đã được chạy trên máy.

## Đặc điểm của Bài 1

Dữ liệu lưu trong bộ nhớ, khởi động lại API sẽ mất các lượt nộp. Hai project tự khai báo hợp đồng JSON tương ứng và giao tiếp qua HTTP. MepDemo mô phỏng năm MEP trong một tiến trình; phần này giúp quan sát mô hình trước khi triển khai mạng hoặc message broker thật.

## Nộp bài theo quy định lớp

Bài nằm trong `Trương Văn Đức Trung - dtc245200314/Bai1/` ở fork của tài khoản `dtc245200314-del`. PR gửi về `PTUDPTK23/CNTTK23D`, tiêu đề:

```text
Bai1 - dtc245200314 - Trương Văn Đức Trung
```

PR bài nộp: https://github.com/PTUDPTK23/CNTTK23D/pull/2 (đã mở và được hệ thống lớp gắn nhãn Đúng hạn).

Chỉ thêm hoặc sửa tệp bên trong thư mục sinh viên này. Thư mục `bin`, `obj` được loại khỏi bài nộp bằng `.gitignore`. Khi bổ sung bài, commit vào cùng nhánh để cập nhật PR hiện tại.
