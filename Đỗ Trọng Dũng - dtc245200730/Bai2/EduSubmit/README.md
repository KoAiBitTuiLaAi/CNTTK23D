# EduSubmit – Bài 2 RESTful API

## Cấu trúc

- `src/Submission.Api`: Minimal API (máy chủ), cổng `5000`, 4 endpoint về bài tập và lượt nộp. Mọi lỗi do handler trả đều cùng dạng `ApiError { code, message }`.
- `src/Submission.Client`: máy khách console dùng `HttpClient`, chạy chuỗi 4 nấc: liệt kê bài tập, nộp bài, đọc lại theo `Location`, cố ý gửi sai.
- `requests.http`: 11 request cho REST Client (đủ 7 request bắt buộc, bao phủ 200, 201, 400, 404; thêm 415 và 405).
- `sv001.json`, `rong.json`: body mẫu cho các lệnh POST bằng `curl.exe`.

Dữ liệu mẫu có 3 bài tập: bài 1 và bài 2 đang mở, **bài 3 đã đóng** (`IsOpen = false`), dùng cho ca 5h.

## Hợp đồng API

| Method | URI | Thành công | Lỗi |
|---|---|---|---|
| GET | `/` | 200 + trạng thái dịch vụ | |
| GET | `/api/assignments` | 200 + mảng bài tập | |
| GET | `/api/assignments/{assignmentId:int}` | 200 + bài tập | 404 `ASSIGNMENT_NOT_FOUND` |
| POST | `/api/assignments/{assignmentId:int}/submissions` (body `CreateSubmissionRequest`) | 201 + header `Location` | 404 `ASSIGNMENT_NOT_FOUND`; 400 `ASSIGNMENT_CLOSED`; 400 `INVALID_SUBMISSION` |
| GET | `/api/submissions/{submissionId:guid}` | 200 + lượt nộp | 404 `SUBMISSION_NOT_FOUND` |

POST kiểm tra theo thứ tự: bài tập **tồn tại** → **còn mở** → body **hợp lệ**. `Id`, `AssignmentId`, `Status` (`received`) và `ReceivedAt` do máy chủ điền; DTO đầu vào chỉ có `StudentCode` và `RepositoryUrl`.

## Chạy ứng dụng

Từ thư mục `EduSubmit`:

```powershell
dotnet build
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Mở terminal thứ hai để gọi thử API hoặc chạy client:

```powershell
curl.exe -i http://localhost:5000/
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Dừng máy chủ bằng `Ctrl+C`. Dữ liệu lưu trong bộ nhớ nên sẽ mất khi khởi động lại API.

## Nhật ký kiểm tra (lệnh đã chạy và kết quả)

### Bước 1–2: build và chạy máy chủ

```text
> dotnet build
Build succeeded.
    0 Warning(s)
    0 Error(s)

> curl.exe -i http://localhost:5000/
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"service":"Submission.Api","status":"running"}
```

### Bước 3: danh sách bài tập

```text
> curl.exe -i http://localhost:5000/api/assignments
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[{"id":1,"title":"Bai tap REST API","description":"Thiet ke endpoint theo quy tac REST","isOpen":true},
 {"id":2,"title":"Bai tap Client","description":"Viet console client goi API","isOpen":true},
 {"id":3,"title":"Bai tap cu","description":"Bai tap da het han nop","isOpen":false}]
```

### Bước 4: chi tiết bài tập

| Ca | Lệnh | Kết quả thật | Ai trả lời |
|---|---|---|---|
| 4a | `curl.exe -i http://localhost:5000/api/assignments/1` | `200 OK` + `{"id":1,"title":"Bai tap REST API",...}` | Handler: tìm thấy |
| 4b | `curl.exe -i http://localhost:5000/api/assignments/999` | `404` + `{"code":"ASSIGNMENT_NOT_FOUND","message":"Khong tim thay bai tap."}` | Handler: không tìm thấy, tự trả `ApiError` |
| 4c | `curl.exe -i http://localhost:5000/api/assignments/abc` | `404`, `Content-Length: 0` | Framework: `abc` không khớp ràng buộc `:int`, handler không chạy |

4b và 4c đều là 404 nhưng khác nhau ở body: 4b có `ApiError` (URI đúng, tài nguyên không tồn tại), 4c body rỗng (đường dẫn không khớp tuyến nào).

### Bước 5: bảng phá thử (8 ca POST)

Body lấy từ tệp để tránh lỗi dấu nháy:

```powershell
# 5a
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -H "Content-Type: application/json" -d "@sv001.json"
# 5b
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -H "Content-Type: application/json" -d "@rong.json"
# 5c
curl.exe -i -X POST http://localhost:5000/api/assignments/999/submissions -H "Content-Type: application/json" -d "@sv001.json"
# 5d (bỏ Content-Type)
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -d "@sv001.json"
# 5g
curl.exe -i -X PUT http://localhost:5000/api/assignments/1
# 5h (bài 3 đã đóng)
curl.exe -i -X POST http://localhost:5000/api/assignments/3/submissions -H "Content-Type: application/json" -d "@sv001.json"
```

Ca 5e và 5f gửi body trực tiếp (chạy trong Git Bash):

```bash
# 5e: JSON hỏng
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -H "Content-Type: application/json" -d '{"studentCode":"SV001",'
# 5f: thiếu repositoryUrl
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -H "Content-Type: application/json" -d '{"studentCode":"SV001"}'
```

| Ca | Yêu cầu | Dự đoán | Kết quả thật | Ai trả lời | Giải thích |
|---|---|---|---|---|---|
| 5a | POST bài 1, `sv001.json` | 201 | `201 Created`, `Location: /api/submissions/568659ad-...`, body có `"status":"received"` | Handler | Tạo thành công, có địa chỉ đọc lại |
| 5b | POST bài 1, `rong.json` | 400 | `400` + `INVALID_SUBMISSION` | Handler | Chuỗi rỗng, `IsNullOrWhiteSpace` chặn lại |
| 5c | POST bài 999, `sv001.json` | 404 | `404` + `ASSIGNMENT_NOT_FOUND` | Handler | Tài nguyên cha không tồn tại, kiểm tra đầu tiên |
| 5d | Như 5a nhưng bỏ `Content-Type` | 415 | `415 Unsupported Media Type`, `Content-Length: 0` | Framework | Không biết body là JSON, handler không chạy |
| 5e | Body JSON hỏng `{"studentCode":"SV001",` | 400 | `400 Bad Request`, body dạng text (`BadHttpRequestException ... JsonException`) | Framework | Không đọc được body thành `CreateSubmissionRequest` |
| 5f | Body thiếu `repositoryUrl` | 400 | `400` + `INVALID_SUBMISSION` | Handler | Trường thiếu thành `null`, handler bắt được |
| 5g | `PUT /api/assignments/1` | 405 | `405 Method Not Allowed`, `Allow: GET` | Framework | URI có nhưng không hỗ trợ PUT |
| 5h | POST bài 3 (đã đóng), `sv001.json` | 400 | `400` + `ASSIGNMENT_CLOSED` | Handler | Đúng cú pháp nhưng sai quy tắc nghiệp vụ |

Đoán đúng 8/8. Ba ca của framework (5d, 5e, 5g) không có `ApiError`; mọi lỗi do handler trả đều cùng dạng `{ code, message }`.

### Bước 6: đọc lại lượt nộp

| Ca | Lệnh / thao tác | Kết quả thật |
|---|---|---|
| 6a | `curl.exe -i http://localhost:5000/api/submissions/568659ad-9819-41e1-a671-6469a0127649` (URI lấy từ `Location` của 5a) | `200 OK` + đúng lượt nộp `SV001` |
| 6b | `curl.exe -i http://localhost:5000/api/submissions/00000000-0000-0000-0000-000000000000` | `404` + `SUBMISSION_NOT_FOUND` |
| 6c | Tắt API, chạy lại, gọi lại 6a với id cũ | `404` + `SUBMISSION_NOT_FOUND` |

6c: lượt nộp "biến mất" vì danh sách `submissions` chỉ nằm trong RAM của tiến trình API; tắt tiến trình là mất dữ liệu. Nếu chạy 2 bản API cùng lúc, mỗi bản có danh sách riêng nên lượt nộp tạo ở bản này không đọc được ở bản kia.

### Bước 7: máy khách `Submission.Client`

```text
> dotnet run --project src/Submission.Client -- http://localhost:5000
Danh sach bai tap dang mo:
- 1: Bai tap REST API
- 2: Bai tap Client

Da tao luot nop: 7776db06-a8db-40bc-9a2b-58843f4d9586
Trang thai ban dau: received
Doc lai luot nop 7776db06-a8db-40bc-9a2b-58843f4d9586: DTC245200730 - https://github.com/KoAiBitTuiLaAi/CNTTK23D

Thu gui yeu cau loi: 400 BadRequest
INVALID_SUBMISSION: Thieu ma sinh vien hoac kho ma nguon.
```

| Nấc | Việc client làm | Kết quả |
|---|---|---|
| 1 | `GetFromJsonAsync` danh sách, lọc `IsOpen` | In 2 bài tập đang mở (bài 3 đã đóng không hiện) |
| 2 | `PostAsJsonAsync`, kiểm tra `StatusCode == Created` trước rồi mới đọc body | Tạo lượt nộp, trạng thái `received` |
| 3 | `GetAsync` đúng địa chỉ trong header `Location` | Đọc lại được lượt nộp vừa tạo |
| 4 | Cố ý gửi body rỗng | Nhận `400`, in `code` từ `ApiError`, không sập |

| Ca | Thao tác | Kết quả thật |
|---|---|---|
| 7a | Tắt API rồi chạy client | `HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:5000)` – lỗi mạng, không có phản hồi HTTP nên không có mã trạng thái |
| 7b | Chạy client với cổng sai `http://localhost:5999` | Giống 7a: địa chỉ dịch vụ là cấu hình truyền qua `args[0]`, không gán cứng |

Lỗi mạng (không có phản hồi) khác lỗi HTTP (có phản hồi 4xx kèm `ApiError`).

### Bước 8: `requests.http`

Tệp có 11 request, mở bằng REST Client và bấm **Send Request**. Biến `@api = http://localhost:5000` đặt địa chỉ một chỗ; request 4 đặt `# @name createSubmission` để request 5 đọc lại theo `id` vừa tạo.

| # | Request | Mã mong đợi |
|---|---|---|
| 1 | `GET /api/assignments` | 200 |
| 2 | `GET /api/assignments/1` | 200 |
| 3 | `GET /api/assignments/999` | 404 `ASSIGNMENT_NOT_FOUND` |
| 4 | `POST /api/assignments/1/submissions` | 201 + `Location` |
| 5 | `GET /api/submissions/{{createSubmission.response.body.$.id}}` | 200 |
| 6 | POST body rỗng | 400 `INVALID_SUBMISSION` |
| 7 | `GET /api/submissions/00000000-...` | 404 `SUBMISSION_NOT_FOUND` |
| 8 | POST vào bài 999 | 404 `ASSIGNMENT_NOT_FOUND` |
| 9 | POST vào bài 3 đã đóng | 400 `ASSIGNMENT_CLOSED` |
| 10 | POST thiếu `Content-Type` | 415 |
| 11 | `PUT /api/assignments/1` | 405, `Allow: GET` |

## Ba câu hỏi

1. **Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?**
   URI chỉ định danh tài nguyên: `assignments` (bài tập), `submissions` (lượt nộp). Hành động do phương thức HTTP nói: `GET` để đọc, `POST` để tạo. Ví dụ `POST /api/assignments/1/submissions` nghĩa là "tạo một lượt nộp thuộc bài tập 1", thay cho kiểu URI có động từ như `/submitAssignment`. Nhờ vậy cùng một URI `/api/assignments/1` dùng được với nhiều phương thức, và framework tự trả `405` (`Allow: GET`) khi gọi phương thức chưa hỗ trợ (ca 5g).

2. **"Stateless" thể hiện ở đâu trong code đã viết?**
   Mỗi request tự mang đủ thông tin để xử lý: `assignmentId` nằm trên URI (`{assignmentId:int}`), `studentCode` và `repositoryUrl` nằm trong body `CreateSubmissionRequest`; để đọc lại, client gửi `submissionId` ngay trên URI lấy từ header `Location`. Máy chủ không có session, cookie hay biến nào ghi nhớ "client này vừa làm gì"; mỗi handler chỉ dựa vào tham số của chính request đó. Danh sách `submissions` trong RAM là dữ liệu tài nguyên mẫu, không phải trạng thái phiên của client.

3. **Vì sao client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?**
   `Submission.Api` và `Submission.Client` là hai tiến trình riêng; `Submission.Client.csproj` không tham chiếu project API. Mỗi bên tự khai báo `AssignmentDto`, `CreateSubmissionRequest`, `SubmissionDto`, `ApiError` ở cuối `Program.cs` của mình. Chúng khớp nhau nhờ **hợp đồng JSON**: máy chủ chuyển tên thuộc tính thành camelCase (`StudentCode` → `studentCode`), máy khách đọc JSON theo cùng tên trường và cùng kiểu dữ liệu (`Guid`, `int`, `string`, `DateTimeOffset`). Chỉ cần tên trường và kiểu trong JSON giống nhau là hai bên hiểu nhau, không cần dùng chung lớp C#.
