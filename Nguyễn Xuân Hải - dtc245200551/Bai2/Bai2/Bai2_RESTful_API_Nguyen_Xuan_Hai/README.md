# Bài 2 – RESTful API cho tương tác Client–Server

**Sinh viên:** Nguyễn Xuân Hải  
**MSSV:** dtc245200551  
**Tên máy (hostname):** `DESKTOP-H78NKA3`  
**Tài khoản Windows (whoami):** `desktop-h78nka3\\pc`  
**Môn học:** Phát triển ứng dụng phân tán

> Lưu ý: Mã sinh viên đã được điền là `dtc245200551`. URL GitHub trong dữ liệu mẫu vẫn là giá trị minh họa; hãy thay bằng repository bài làm của bạn. Không dùng URL ví dụ làm bằng chứng đã nộp GitHub.

## 1. Cấu trúc dự án

```text
RestfulClientServer/
├── RestfulClientServer.slnx
├── .gitignore
├── requests.http
├── README.md
├── Screenshots/
└── src/
    ├── Submission.Api/
    │   ├── Submission.Api.csproj
    │   └── Program.cs
    └── Submission.Client/
        ├── Submission.Client.csproj
        └── Program.cs
```

## 2. Hợp đồng API

| Phương thức | URI | Thành công | Lỗi chính |
|---|---|---|---|
| GET | `/api/assignments` | 200 – danh sách bài tập | — |
| GET | `/api/assignments/{assignmentId}` | 200 – chi tiết bài tập | 404 `ASSIGNMENT_NOT_FOUND` |
| POST | `/api/assignments/{assignmentId}/submissions` | 201 – tạo lượt nộp và trả `Location` | 400 `ASSIGNMENT_CLOSED`, 400 `INVALID_SUBMISSION`, 404 `ASSIGNMENT_NOT_FOUND` |
| GET | `/api/submissions/{submissionId}` | 200 – lượt nộp | 404 `SUBMISSION_NOT_FOUND` |

Mọi lỗi nghiệp vụ do API tự trả có dạng thống nhất:

```json
{
  "code": "INVALID_SUBMISSION",
  "message": "Thieu ma sinh vien hoac kho ma nguon."
}
```

## 3. Cách chạy

Mở PowerShell tại thư mục gốc của dự án.

Kiểm tra SDK:

```powershell
dotnet --version
```

Cần SDK .NET 10 (`10.0.xxx`) theo tài liệu bài học.

Build solution:

```powershell
dotnet build
```

Chạy API ở Terminal 1:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Mở Terminal 2, kiểm tra API:

```powershell
curl.exe -i http://localhost:5000/
curl.exe -i http://localhost:5000/api/assignments
```

Chạy Client ở Terminal 2:

```powershell
dotnet run --project src/Submission.Client
```

## 4. Kiểm thử

Cài extension **REST Client** trong VS Code, mở `requests.http`, sau đó bấm **Send Request** trên từng request. Gửi request số 4 trước request số 7 vì request số 7 đọc GUID do request số 4 tạo ra.

| Ca | Kết quả mong đợi |
|---|---|
| 1. GET danh sách | 200 OK |
| 2. GET bài tập 1 | 200 OK |
| 3. GET bài tập 999 | 404 `ASSIGNMENT_NOT_FOUND` |
| 4. POST hợp lệ | 201 Created và có header `Location` |
| 5. POST dữ liệu rỗng | 400 `INVALID_SUBMISSION` |
| 6. POST bài tập đã đóng | 400 `ASSIGNMENT_CLOSED` |
| 7. GET lượt nộp theo GUID | 200 OK |

Dữ liệu API hiện được lưu trong `List` ở bộ nhớ tiến trình. Khi tắt API, dữ liệu lượt nộp tạo trong lần chạy đó sẽ mất; đây là giới hạn của bài thực hành.

## 5. Chuỗi hoạt động của Client

1. Gọi GET để liệt kê bài tập.
2. Gọi POST để tạo lượt nộp hợp lệ.
3. Lấy GUID và gọi GET để đọc lại lượt nộp.
4. Cố ý gửi dữ liệu không hợp lệ, kiểm tra mã trạng thái trước rồi đọc `ApiError`.

## 6. Trả lời 3 câu hỏi trong bài

**Câu 1. Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?**

URI biểu diễn tài nguyên mà hệ thống quản lý, ví dụ `/api/assignments`. Phương thức HTTP thể hiện hành động trên tài nguyên: GET để đọc, POST để tạo. Cách này làm địa chỉ ngắn gọn, nhất quán và dễ mở rộng; tránh đặt động từ như `/getAssignments` hoặc `/doSubmit` trong URI.

**Câu 2. Stateless thể hiện ở đâu trong code đã viết?**

Mỗi request gửi lên API tự chứa thông tin cần thiết như assignmentId trên route và StudentCode, RepositoryUrl trong JSON body. Server không dựa vào trạng thái phiên đăng nhập của Client để hiểu request. Dữ liệu bài tập và lượt nộp trong ví dụ được lưu ở bộ nhớ của server, điều đó không đồng nghĩa với việc vi phạm Stateless; Stateless nói về ngữ cảnh giao tiếp của từng request.

**Câu 3. Vì sao Client và Server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?**

Hai bên không cần dùng chung một class C#. Chúng chỉ cần thống nhất hợp đồng JSON: tên trường, kiểu dữ liệu và ý nghĩa. ASP.NET Core chuyển DTO phía Server thành JSON; HttpClient chuyển JSON thành DTO phía Client. Nhờ vậy hai chương trình độc lập vẫn trao đổi được dữ liệu.

## 7. Ảnh minh chứng cần tự chụp

Lưu ảnh thật vào thư mục `Screenshots/`. Không dùng ảnh minh họa thay cho kết quả chạy trên máy.

1. `01-cau-truc-du-an.png` – Explorer hiển thị solution và hai project.
2. `02-dotnet-build.png` – `dotnet build` thành công.
3. `03-api-running.png` – API đang chạy ở `http://localhost:5000`.
4. `04-get-assignments.png` – GET danh sách trả 200.
5. `05-get-assignment-detail.png` – GET chi tiết trả 200 và GET ID 999 trả 404.
6. `06-post-created.png` – POST hợp lệ trả 201 và có Location.
7. `07-post-invalid.png` – POST dữ liệu sai trả 400 và ApiError.
8. `08-get-submission.png` – GET đọc lại lượt nộp trả 200.
9. `09-client-output.png` – Client hiển thị đủ 4 nấc.
10. `10-requests-http.png` – requests.http đã chạy và có phản hồi.
11. `11-github-repository.png` – repository GitHub có mã nguồn và README.
12. `12-pull-request.png` – Pull Request đã được tạo.

## 8. Git và Pull Request

Thực hiện trong fork repository lớp theo đúng đường dẫn giảng viên cung cấp. Không tự đoán URL repository lớp.

```powershell
git status
git add .
git commit -m "Complete Bai 2 RESTful API - dtc245200551"
git push
```

Tiêu đề Pull Request theo yêu cầu bài:

```text
Bai2 - dtc245200551 - Nguyen Xuan Hai
```

Trước khi nộp, kiểm tra README, `requests.http`, mã nguồn hai project và ảnh minh chứng đã có trong repository.
