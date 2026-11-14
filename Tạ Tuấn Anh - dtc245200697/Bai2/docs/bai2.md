# Bài 2 – Xây dựng RESTful API cho tương tác máy khách–máy chủ (Catalog API & EduSubmit)

- **Họ và tên:** Tạ Tuấn Anh
- **Mã sinh viên:** DTC245200697
- **Lớp:** CNTT K23D
- **Môn học:** Phát triển ứng dụng phân tán

---

## 1. Cấu trúc thư mục Bài 2

```text
Bai2/
├── EduSubmit.slnx                         # Solution chính của hệ thống nộp bài EduSubmit (Bài 2)
├── .gitignore                             # Bỏ qua bin/, obj/ khi commit Git
├── .gitattributes
├── requests.http                          # 10 request kiểm thử đầy đủ các mã 200, 201, 400, 404
├── sv001.json                             # Dữ liệu JSON hợp lệ dùng để test POST bằng curl.exe
├── rong.json                              # Dữ liệu JSON rỗng dùng để test lỗi 400 bằng curl.exe
├── src/
│   ├── Submission.Api/                    # Máy chủ RESTful API (lắng nghe cổng 5000)
│   │   ├── Program.cs                     # 4 endpoint nghiệp vụ + 1 endpoint health check + DTO/ApiError
│   │   └── Submission.Api.csproj
│   └── Submission.Client/                 # Máy khách Console (chuỗi 4 nấc gọi API qua HttpClient)
│       ├── Program.cs
│       └── Submission.Client.csproj
├── CatalogApi/                            # Dự án thực hành Phần 2 (Catalog API - cổng 5100)
│   ├── CatalogApi.slnx
│   ├── requests.http
│   ├── sp-hop-le.json
│   ├── sp-sai.json
│   └── src/
│       ├── Catalog.Api/
│       └── Catalog.Client/
├── docs/
│   └── bai2.md                            # Báo cáo chi tiết Bài 2
└── README.md                              # Nhật ký thực hành, bảng phá thử và trả lời 3 câu hỏi
```

---

## 2. Nhật ký lệnh đã chạy và Kết quả thực hiện

### 2.1. Hợp đồng API của EduSubmit (Phần 3)

| # | Phương thức | URI | Mã thành công | Mã lỗi & Mã `ApiError` có thể gặp |
|:-:|:-----------:|-----|:-------------:|-----------------------------------|
| 0 | `GET` | `/` | `200 OK` | – |
| 1 | `GET` | `/api/assignments` | `200 OK` | – |
| 2 | `GET` | `/api/assignments/{assignmentId:int}` | `200 OK` | `404 Not Found` (`ASSIGNMENT_NOT_FOUND`) / `404` rỗng nếu `id` không phải số nguyên |
| 3 | `POST` | `/api/assignments/{assignmentId:int}/submissions` | `201 Created` + `Location` | `404` (`ASSIGNMENT_NOT_FOUND`), `400` (`ASSIGNMENT_CLOSED`), `400` (`INVALID_SUBMISSION`), `415` nếu thiếu `Content-Type` |
| 4 | `GET` | `/api/submissions/{submissionId:guid}` | `200 OK` | `404 Not Found` (`SUBMISSION_NOT_FOUND`) |

### 2.2. Các lệnh đã chạy

```powershell
# 1. Biên dịch toàn bộ solution EduSubmit
cd "Bai2"
dotnet build EduSubmit.slnx

# 2. Chạy máy chủ Submission.Api cố định ở cổng 5000 (Terminal 1)
dotnet run --project src/Submission.Api --urls http://localhost:5000

# 3. Kiểm tra nhanh các endpoint bằng curl.exe (Terminal 2)
curl.exe -i http://localhost:5000/
curl.exe -i http://localhost:5000/api/assignments
curl.exe -i http://localhost:5000/api/assignments/1
curl.exe -i http://localhost:5000/api/assignments/999
curl.exe -i http://localhost:5000/api/assignments/abc
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -H "Content-Type: application/json" -d "@sv001.json"
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -H "Content-Type: application/json" -d "@rong.json"
curl.exe -i -X POST http://localhost:5000/api/assignments/3/submissions -H "Content-Type: application/json" -d "@sv001.json"
curl.exe -i -X POST http://localhost:5000/api/assignments/999/submissions -H "Content-Type: application/json" -d "@sv001.json"
curl.exe -i -X POST http://localhost:5000/api/assignments/1/submissions -d "@sv001.json"
curl.exe -i -X PUT http://localhost:5000/api/assignments/1

# 4. Chạy máy khách Submission.Client qua 4 nấc (Terminal 2)
dotnet run --project src/Submission.Client -- http://localhost:5000
```

### 2.3. Kết quả chạy thực tế

#### Kết quả build (`dotnet build`)
```text
  Submission.Client -> Bai2\src\Submission.Client\bin\Debug\net10.0\Submission.Client.dll
  Submission.Api -> Bai2\src\Submission.Api\bin\Debug\net10.0\Submission.Api.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

#### Kết quả chạy `Submission.Client` (đủ 4 nấc: Liệt kê → Nộp bài → Đọc lại theo Location → Gửi sai nhận 400)
```text
Danh sach bai tap dang mo:
 - 1: Bai tap REST API
 - 2: Bai tap Client

Da tao luot nop: 48429b9f-0313-4b46-8d52-b8454e03cf15
Trang thai ban dau: received
Doc lai luot nop: DTC245200697 - https://github.com/PTUDPTK23/CNTTK23D

Thu gui yeu cau loi: 400 BadRequest
INVALID_SUBMISSION: Thieu ma sinh vien hoac kho ma nguon.
```

#### Kết quả gọi `POST` hợp lệ (`201 Created` kèm `Location`)
```http
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Server: Kestrel
Location: /api/submissions/b25c7a76-8932-4427-8fe8-056bafea6a51

{"id":"b25c7a76-8932-4427-8fe8-056bafea6a51","assignmentId":1,"studentCode":"DTC245200697","repositoryUrl":"https://github.com/PTUDPTK23/CNTTK23D","status":"received","receivedAt":"2026-11-13T03:57:19.9246436+00:00"}
```

---

## 3. Bảng phá thử (Kiểm thử các ca thành công và lỗi)

### 3.1. Bảng 8 ca phá thử ở Bước 5 (Endpoint `POST /api/assignments/{assignmentId:int}/submissions`)

| Ca | Yêu cầu thử nghiệm | Mã mong đợi | Kết quả thực tế nhận được | Ai trả lời | Ý nghĩa / Giải thích |
|:--:|--------------------|-------------|---------------------------|:----------:|----------------------|
| **5a** | `POST` bài tập 1, body `sv001.json` | `201 Created` + `Location` | `201 Created` + `Location: /api/submissions/{id}` + JSON `SubmissionDto` | **Handler** | Tạo lượt nộp mới thành công, máy chủ tự sinh `Id` (`Guid`), `Status = "received"`, `ReceivedAt` và chỉ đường đọc lại qua header `Location`. |
| **5b** | `POST` bài tập 1, body `rong.json` | `400 INVALID_SUBMISSION` | `400 Bad Request` + `{"code":"INVALID_SUBMISSION","message":"Thieu ma sinh vien hoac kho ma nguon."}` | **Handler** | Cổng kiểm tra thứ 3 từ chối do `StudentCode` hoặc `RepositoryUrl` rỗng/khoảng trắng. |
| **5c** | `POST` bài tập 999, body `sv001.json` | `404 ASSIGNMENT_NOT_FOUND` | `404 Not Found` + `{"code":"ASSIGNMENT_NOT_FOUND","message":"Khong tim thay bai tap."}` | **Handler** | Cổng kiểm tra thứ 1 từ chối do tài nguyên cha (bài tập 999) không tồn tại. |
| **5d** | Như 5a nhưng bỏ `-H "Content-Type: application/json"` | `415`, body rỗng | `415 Unsupported Media Type`, `Content-Length: 0` (body rỗng) | **Framework** | ASP.NET Core chặn trước khi vào handler vì không biết body gửi lên có định dạng JSON. |
| **5e** | Body JSON hỏng cú pháp: `{"studentCode":"SV001",` | `400`, body văn bản/rỗng | `400 Bad Request` (do bộ đọc JSON của framework từ chối) | **Framework** | Trạm đọc body (JSON deserializer) thất bại do JSON sai cú pháp, handler không được gọi. |
| **5f** | Body thiếu trường `repositoryUrl`: `{"studentCode":"SV001"}` | `400 INVALID_SUBMISSION` | `400 Bad Request` + `{"code":"INVALID_SUBMISSION","message":"Thieu ma sinh vien hoac kho ma nguon."}` | **Handler** | Trường thiếu được gán `null` khi deserialize; khối `if (string.IsNullOrWhiteSpace(...))` trong handler bắt được và trả `ApiError`. |
| **5g** | `PUT /api/assignments/1` | `405`, `Allow: GET` | `405 Method Not Allowed`, Header `Allow: GET` | **Framework** | Bộ định tuyến thấy URI `/api/assignments/1` có tồn tại nhưng chỉ đăng ký `MapGet`, không hỗ trợ động từ `PUT`. |
| **5h** | `POST` vào bài tập đã đóng (`IsOpen = false`, bài tập 3) | `400 ASSIGNMENT_CLOSED` | `400 Bad Request` + `{"code":"ASSIGNMENT_CLOSED","message":"Bai tap da dong nop."}` | **Handler** | Cổng kiểm tra thứ 2 từ chối: cú pháp đúng, bài tập tồn tại nhưng vi phạm quy tắc nghiệp vụ (`!assignment.IsOpen`). |

### 3.2. Bảng kiểm thử bổ sung (Bước 4, Bước 6 và Thí nghiệm phân tán Bước 7)

| Ca | Thao tác / Yêu cầu | Kết quả quan sát thực tế | Bài học rút ra |
|:--:|--------------------|--------------------------|----------------|
| **4a** | `GET /api/assignments/1` | `200 OK` + JSON bài tập 1 | Handler tìm thấy phần tử trong `List<AssignmentDto>`. |
| **4b** | `GET /api/assignments/999` | `404 Not Found` + `ApiError("ASSIGNMENT_NOT_FOUND", ...)` | `999` là số nguyên nên khớp `{assignmentId:int}`, handler chạy và trả `404` kèm `ApiError`. |
| **4c** | `GET /api/assignments/abc` | `404 Not Found`, `Content-Length: 0` (body rỗng) | `abc` không thỏa ràng buộc `:int`, bộ định tuyến của Framework từ chối ngay, handler không chạy. |
| **6a** | `GET` theo địa chỉ `Location` vừa nhận ở 5a | `200 OK` + JSON đúng lượt nộp vừa tạo | Header `Location` giúp client đọc lại tài nguyên vừa tạo mà không cần tự ghép URL. |
| **6b** | `GET /api/submissions/00000000-0000-0000-0000-000000000000` | `404 Not Found` + `ApiError("SUBMISSION_NOT_FOUND", ...)` | Handler kiểm tra không có `Guid` khớp và trả lỗi theo đúng hợp đồng `ApiError`. |
| **6c** | Tắt API, bật lại rồi gọi lại 6a với `id` cũ | `404 Not Found` + `SUBMISSION_NOT_FOUND` | Dữ liệu hiện lưu trong bộ nhớ RAM (`List<SubmissionDto>`) của tiến trình máy chủ nên mất khi tiến trình dừng. |
| **7a** | Tắt API rồi chạy `Submission.Client` | `HttpRequestException: No connection could be made... (localhost:5000)` | Lỗi mạng (không có phản hồi HTTP) khác với lỗi HTTP `4xx` (máy chủ có trả phản hồi). |
| **7b** | Chạy `Submission.Client` với cổng sai `http://localhost:5999` | `HttpRequestException: Connection refused (localhost:5999)` | Hai tiến trình phân tán phải thống nhất địa chỉ dịch vụ (URL). |
| **7c** | Thêm bài tập mở mới vào API rồi chạy lại Client | Client tự in thêm bài tập mới mà không cần sửa hay biên dịch lại code Client | Tính ghép lỏng (*loose coupling*): hai bên chỉ phụ thuộc vào hợp đồng JSON. |

---

## 4. Trả lời 3 câu hỏi giải thích

### Câu 1: Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?

**Trả lời:**
- Trong kiến trúc REST (*Representational State Transfer*), mọi thực thể nghiệp vụ mà hệ thống quản lý được mô hình hóa thành **tài nguyên (Resource)**. Vì URI (*Uniform Resource Identifier*) đóng vai trò là **định danh (tên/địa chỉ)** của tài nguyên nên URI phải là **danh từ** (thường viết ở dạng số nhiều, chữ thường và phân cấp cha–con), chứ không phải tên hàm hay động từ.
- Ngược lại, **hành động** muốn thực hiện lên tài nguyên đó được giao cho **phương thức HTTP (HTTP Method)** đảm nhận theo chuẩn giao diện thống nhất (*Uniform Interface*): `GET` để đọc, `POST` để tạo mới, `PUT` để thay thế toàn bộ, `PATCH` để cập nhật một phần, `DELETE` để xóa.
- **Ví dụ trực tiếp từ code đã viết trong `Submission.Api/Program.cs`:**
  - Thay vì đặt URI kiểu gọi hàm như `/getAssignments`, `/getAssignmentById` hay `/doSubmit`, code sử dụng các danh từ chỉ tài nguyên:
    - `app.MapGet("/api/assignments", ...)` → đọc danh sách tài nguyên bài tập.
    - `app.MapGet("/api/assignments/{assignmentId:int}", ...)` → đọc chi tiết một bài tập.
    - `app.MapPost("/api/assignments/{assignmentId:int}/submissions", ...)` → tạo một lượt nộp mới (`submissions`) là tài nguyên con thuộc về bài tập `{assignmentId}`.
    - `app.MapGet("/api/submissions/{submissionId:guid}", ...)` → đọc lại một lượt nộp cụ thể.
- Cách thiết kế này giúp hợp đồng API rõ ràng, nhất quán, không bị nhân bản vô số đường dẫn theo từng hành động và giúp máy khách đoán đúng ngữ nghĩa (an toàn, lũy đẳng) ngay từ phương thức HTTP.

---

### Câu 2: “Stateless” thể hiện ở đâu trong code đã viết?

**Trả lời:**
- **Stateless (Phi trạng thái phiên)** trong REST có nghĩa là **máy chủ không lưu giữ trạng thái cuộc hội thoại (session/client context) của máy khách giữa các yêu cầu liên tiếp**. Mỗi yêu cầu HTTP từ máy khách gửi lên phải **tự mang đầy đủ mọi thông tin cần thiết** để máy chủ hiểu và xử lý độc lập.
  *(Lưu ý: Stateless không có nghĩa là máy chủ không được lưu dữ liệu nghiệp vụ — danh sách `assignments` và `submissions` vẫn được lưu trên máy chủ).*
- **Thể hiện cụ thể trong code đã viết (`Submission.Api` và `Submission.Client`):**
  1. Ở `Submission.Client/Program.cs`, ở **Nấc 1**, máy khách gọi `GET /api/assignments` để lấy danh sách bài tập và chọn ra `selectedAssignmentId`. Máy chủ hoàn toàn **không ghi nhớ** máy khách này vừa xem hay chọn bài tập nào.
  2. Sang **Nấc 2**, khi nộp bài bằng `POST /api/assignments/{selectedAssignmentId}/submissions`, máy khách phải **tự truyền lại đầy đủ ngữ cảnh** trong một yêu cầu duy nhất:
     - Mã bài tập `{assignmentId}` nằm ngay trên đường dẫn URI (`/api/assignments/1/submissions`).
     - Thông tin người nộp và bài nộp nằm trọn trong body JSON thông qua `new CreateSubmissionRequest("DTC245200697", "https://github.com/PTUDPTK23/CNTTK23D")`.
  3. Sang **Nấc 3**, khi đọc lại lượt nộp qua `GET /api/submissions/{submissionId}`, máy khách cũng phải chỉ rõ `submissionId` trên URI lấy từ header `Location`, chứ không thể gửi yêu cầu kiểu *"hãy trả lại bài tôi vừa nộp lúc nãy"*.
  4. Phía `Submission.Api/Program.cs`, handler `MapPost` chỉ nhận dữ liệu từ tham số tuyến `(int assignmentId)` và body `(CreateSubmissionRequest request)` của chính request hiện tại để kiểm tra 3 cổng và xử lý, không phụ thuộc bất kỳ biến trạng thái phiên nào của request trước.

---

### Câu 3: Vì sao client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?

**Trả lời:**
- **Lý do mỗi bên khai báo DTO riêng:**
  - `Submission.Api` và `Submission.Client` là **hai chương trình độc lập, chạy trên hai tiến trình riêng biệt** (có thể đặt trên hai máy tính khác nhau, thậm chí viết bằng hai ngôn ngữ lập trình khác nhau) và **không tham chiếu project C# của nhau** (trong `Submission.Client.csproj` không hề có `<ProjectReference>` tới `Submission.Api.csproj`).
  - Đối tượng C# trong bộ nhớ RAM của tiến trình này không thể truyền trực tiếp sang tiến trình kia; qua mạng chỉ truyền được chuỗi byte theo định dạng văn bản **JSON**.
- **Lý do hai bên vẫn khớp nhau hoàn hảo:**
  - Hai bên giao tiếp thông qua **Hợp đồng dữ liệu (DTO – Data Transfer Object)** ở biên hệ thống, thống nhất về **tên trường (key)** và **kiểu dữ liệu (value type)** trong chuỗi JSON:
    - Khi `Submission.Api` trả `AssignmentDto(int Id, string Title, string Description, bool IsOpen)`, bộ tuần tự hóa JSON (`System.Text.Json`) của ASP.NET Core tự động chuyển tên thuộc tính sang dạng `camelCase`: `{"id": 1, "title": "...", "description": "...", "isOpen": true}`.
    - Khi `Submission.Client` nhận chuỗi JSON đó và gọi `GetFromJsonAsync<List<AssignmentDto>>()` hoặc `ReadFromJsonAsync<SubmissionDto>()`, bộ giải tuần tự hóa phía Client ánh xạ không phân biệt hoa thường từ các trường JSON (`id`, `title`, `description`, `isOpen`) vào các thuộc tính tương ứng của `record AssignmentDto` khai báo ở cuối `Submission.Client/Program.cs`.
  - Tương tự với `CreateSubmissionRequest(string StudentCode, string RepositoryUrl)` và `ApiError(string Code, string Message)`: chỉ cần hai bên giữ đúng "lời hứa" về cấu trúc JSON (tên thuộc tính và kiểu dữ liệu tương thích), dữ liệu sẽ luôn khớp nhau mà không cần dùng chung mã nguồn.
