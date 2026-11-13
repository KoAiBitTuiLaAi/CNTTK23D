# Bài 2 – RESTful API cho tương tác client–server

Họ tên: Nguyễn Văn Phương     MSSV: DTC245200722     Lớp: CNTT K23D

Cấu trúc thư mục:

```text
Bai2/
├── README.md                   # hồ sơ nộp: lệnh chạy, bảng phá thử, 3 câu hỏi
├── CatalogApi/                 # Phần 2: Catalog API (10 bước)
│   ├── CatalogApi.slnx
│   ├── requests.http
│   ├── sp-hop-le.json, sp-sai.json
│   └── src/
│       ├── Catalog.Api/        # máy chủ, cổng 5100
│       └── Catalog.Client/     # máy khách
├── RestfulClientServer/        # Phần 3: EduSubmit (8 bước + bước 9 mở rộng)
│   ├── RestfulClientServer.slnx
│   ├── requests.http
│   ├── sv001.json, rong.json, hong.json, thieu-repo.json
│   └── src/
│       ├── Submission.Api/     # máy chủ, cổng 5000
│       └── Submission.Client/  # máy khách
└── docs/
    ├── bai2.md                 # nhật ký này
    └── img/
```

Ảnh trong nhật ký được tạo từ kết quả chạy thật của từng lệnh (Terminal 1: máy chủ, Terminal 2: lệnh gọi / máy khách).

## 1. Tạo solution và 2 project (Bước 1)

- .NET SDK: `10.0.201`
- Solution `CatalogApi.slnx` gồm `Catalog.Api` (web) và `Catalog.Client` (console), có `.gitignore` để bỏ qua `bin/`, `obj/`.

![Bước 1 – solution, 2 project, build thành công](img/buoc1.png)

## 2. Chạy khung API (Bước 2)

Sửa endpoint `/` để trả JSON, chạy máy chủ ở cổng 5100:

```bash
dotnet run --project src/Catalog.Api --urls http://localhost:5100
```

![Bước 2 – GET / trả 200 + JSON](img/buoc2.png)

- `Results.Ok(new { service, status })` trả **200 OK** kèm JSON; tên thuộc tính tự đổi sang camelCase.
- Phải ép cổng bằng `--urls`, nếu không mẫu dùng cổng trong `launchSettings.json` (5197) và máy khách không biết gọi đâu.

## 3. DTO và GET danh sách (Bước 3)

- Khai báo `record ProductDto(int Id, string Name, string Description, decimal Price)` và `record ApiError(string Code, string Message)` ở **cuối** `Program.cs` (sau `app.Run()`), vì câu lệnh cấp cao nhất phải đứng trước khai báo kiểu.
- Dữ liệu mẫu là `List<ProductDto>` 3 sản phẩm, nằm trong bộ nhớ tiến trình máy chủ.

![Bước 3 – GET /api/products trả 200 và 3 sản phẩm](img/buoc3.png)

## 4. GET chi tiết (Bước 4)

`GET /api/products/{id:int}`: tìm thấy thì 200, không thấy thì 404 + `ApiError`.

![Bước 4 – 200, 404, 404 rỗng, 405](img/buoc4.png)

| Lệnh | Kết quả thật | Ai trả lời |
|------|--------------|------------|
| `GET /api/products/1` | 200 OK + sản phẩm 1 | Handler: tìm thấy |
| `GET /api/products/999` | 404 + `{"code":"PRODUCT_NOT_FOUND",...}` | Handler: chạy nhưng không tìm thấy |
| `GET /api/products/abc` | 404, `Content-Length: 0` | Framework: `abc` không khớp `{id:int}`, handler không chạy |
| `POST /api/products` | 405 Method Not Allowed, `Allow: GET` | Framework: URI có nhưng chưa có POST |

Cả `/999` và `/abc` đều 404; máy khách phân biệt bằng **body**: có `ApiError` là handler trả, body rỗng là sai đường dẫn.

## 5. POST tạo mới (Bước 5)

- DTO đầu vào `CreateProductRequest(string Name, string? Description, decimal Price)` **không có Id**: Id do máy chủ sinh (`Max + 1`).
- Kiểm tra ở biên hệ thống: tên rỗng hoặc giá ≤ 0 → 400 `INVALID_PRODUCT`.
- Thành công → `Results.Created($"/api/products/{id}", product)`: 201 + header `Location`.

![Bước 5 – 201, 400, 415](img/buoc5.png)

| Ca | Kết quả thật | Ai từ chối |
|----|--------------|------------|
| ① Hợp lệ (`sp-hop-le.json`) | 201 Created, `Location: /api/products/4` | – (handler tạo) |
| ② Giá âm (`sp-sai.json`) | 400 + `INVALID_PRODUCT` | Handler (khối `if`) |
| ③ Thiếu `Content-Type` | 415 Unsupported Media Type | Framework, chặn trước handler |

Sau ca ①, `GET /api/products` có 4 sản phẩm.

Ghi chú: với `curl.exe -d` không có `-H`, curl tự gắn `Content-Type: application/x-www-form-urlencoded` nên 415 trả về body rỗng; khi gửi không có Content-Type (ca 8 trong `requests.http`) thì chế độ Development trả trang lỗi dài.

## 6. requests.http (Bước 6)

Tệp `CatalogApi/requests.http` có 8 yêu cầu, tiêu đề ghi mã mong đợi. Kết quả gửi lần lượt 8 yêu cầu (máy chủ vừa khởi động lại):

![Bước 6 – 8 request đúng mã mong đợi](img/buoc6.png)

| # | Request | Mong đợi | Thực tế |
|---|---------|----------|---------|
| 1 | `GET /` | 200 | 200 |
| 2 | `GET /api/products` | 200 | 200, 3 phần tử |
| 3 | `GET /api/products/1` | 200 | 200 |
| 4 | `GET /api/products/999` | 404 + ApiError | 404 + `PRODUCT_NOT_FOUND` |
| 5 | `GET /api/products/abc` | 404 rỗng | 404 rỗng |
| 6 | `POST` hợp lệ | 201 + Location | 201, `Location: /api/products/4` |
| 7 | `POST` giá âm | 400 + ApiError | 400 + `INVALID_PRODUCT` |
| 8 | `POST` thiếu Content-Type | 415 | 415 |

## 7. Máy khách Catalog.Client (Bước 7–8)

Chạy (máy chủ chạy ở Terminal 1):

```bash
dotnet run --project src/Catalog.Client -- http://localhost:5100
```

![Bước 7–8 – máy khách chạy đủ 4 nấc](img/buoc8.png)

Kết quả máy khách:

```text
Danh sach san pham:
  [1] Giao trinh Lap trinh phan tan - 150,000 VND
  [2] Tai lieu RESTful API - 90,000 VND
  [3] Bai tap thuc hanh .NET - 120,000 VND

Da tao san pham moi: [4] Nhap mon REST API
Doc lai tai: /api/products/4

Chi tiet san pham 1: Giao trinh Lap trinh phan tan

Thu gui du lieu sai: 400 BadRequest
INVALID_PRODUCT: Ten san pham khong duoc de trong va gia phai lon hon 0.
```

1. Nấc 1: `GetFromJsonAsync<List<ProductDto>>` gửi GET và đổi JSON thành danh sách.
2. Nấc 2: `PostAsJsonAsync` trả về cả phản hồi; máy khách **kiểm tra `StatusCode == Created` trước**, rồi mới chọn đọc body là `ProductDto` (201) hay `ApiError` (mã khác). Header `Location` cho biết đọc lại ở đâu.
3. Nấc 3: `GetFromJsonAsync<ProductDto>("/api/products/1")` – chỉ dùng khi chắc chắn tồn tại, vì hàm này ném exception nếu mã không phải 2xx.
4. Nấc 4: cố ý gửi tên rỗng, giá âm; máy khách đọc được 400 và in `Code: Message` thay vì sập.
5. Máy khách khai báo **lại** `ProductDto`, `CreateProductRequest`, `ApiError`: hai chương trình không chung code, chỉ chung hợp đồng JSON (tên trường, kiểu dữ liệu).

## 8. Thí nghiệm phân tán (Bước 9)

| # | Thí nghiệm | Kết quả thật | Bài học |
|---|------------|--------------|---------|
| 1 | Chạy máy khách lần 2 (máy chủ vẫn chạy) | Danh sách có 4 sản phẩm, tạo mới ra `[5]`, `Doc lai tai: /api/products/5` | Trạng thái nằm ở máy chủ, không ở máy khách |
| 2 | Tắt máy chủ, chạy máy khách | `Unhandled exception. HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:5100)` | Lỗi chỉ có ở hệ phân tán: bên kia có thể vắng mặt |
| 3 | Chạy lại máy chủ rồi `GET /api/products` | Lại chỉ còn 3 sản phẩm | Dữ liệu trong bộ nhớ mất khi tiến trình dừng |
| 4 | Máy khách gọi sai cổng `http://localhost:5999` | `actively refused it. (localhost:5999)` | Hai bên phải thống nhất địa chỉ (URL) |

Thí nghiệm 1 và 2:

![Thí nghiệm 1–2](img/thi-nghiem-1-2.png)

Thí nghiệm 3 và 4:

![Thí nghiệm 3–4](img/thi-nghiem-3-4.png)

## 9. Phiếu tự kiểm tra (Bước 10)

| # | Yêu cầu | Mong đợi | Tự đánh giá |
|---|---------|----------|-------------|
| 1 | `GET /` | 200 | ✓ |
| 2 | `GET /api/products` | 200, 3 phần tử | ✓ |
| 3 | `GET /api/products/1` | 200 | ✓ |
| 4 | `GET /api/products/999` | 404 + ApiError | ✓ |
| 5 | `GET /api/products/abc` | 404 rỗng | ✓ |
| 6 | POST hợp lệ | 201 + Location | ✓ |
| 7 | POST giá âm | 400 + ApiError | ✓ |
| 8 | POST thiếu Content-Type | 415 | ✓ |
| 9 | Máy khách chạy đủ 4 nấc | 4 nấc | ✓ |
| 10 | Tắt máy chủ, chạy máy khách | lỗi kết nối | ✓ |

Commit theo từng bước:

```text
Buoc 1: tao solution CatalogApi
Buoc 2: GET / tra JSON
Buoc 3: DTO va GET /api/products
Buoc 4: GET /api/products/{id}
Buoc 5: POST /api/products
Buoc 6: requests.http
Buoc 7-8: Catalog.Client
```

Luồng thông điệp:

```text
Client --(HTTP GET  /api/products)-------> Api : request-response (200)
Client --(HTTP POST /api/products)-------> Api : request-response (201 + Location / 400)
Client --(HTTP GET  /api/products/{id})--> Api : request-response (200 / 404)
```

## 10. EduSubmit (Phần 3 – áp dụng)

Giữ nguyên khuôn của Catalog, đổi tài nguyên sang bài tập / lượt nộp, thêm tài nguyên con và quy tắc nghiệp vụ. Thư mục `RestfulClientServer`, máy chủ cổng **5000**.

Bảng hợp đồng API:

| Phương thức | URI | Thành công | Lỗi |
|-------------|-----|------------|-----|
| GET | `/api/assignments` | 200 danh sách | – |
| GET | `/api/assignments/{assignmentId}` | 200 một bài tập | 404 `ASSIGNMENT_NOT_FOUND` |
| POST | `/api/assignments/{assignmentId}/submissions` | 201 + Location | 404 `ASSIGNMENT_NOT_FOUND` · 400 `ASSIGNMENT_CLOSED` · 400 `INVALID_SUBMISSION` · 409 `DUPLICATE_SUBMISSION`* |
| GET | `/api/submissions/{submissionId}` | 200 lượt nộp | 404 `SUBMISSION_NOT_FOUND` |
| DELETE* | `/api/submissions/{submissionId}` | 204 không body | 404 `SUBMISSION_NOT_FOUND` |

\* Bước 9 mở rộng. Mọi lỗi do handler trả cùng dạng `{ "code": "...", "message": "..." }`.

### 10.1. Bước 1 – Dựng solution, 2 project, .gitignore

```bash
dotnet new sln -n RestfulClientServer
dotnet new web -n Submission.Api -o src/Submission.Api
dotnet new console -n Submission.Client -o src/Submission.Client
dotnet sln add src/Submission.Api/Submission.Api.csproj
dotnet sln add src/Submission.Client/Submission.Client.csproj
dotnet new gitignore
dotnet build
```

![EduSubmit Bước 1 – Build succeeded](img/edu-buoc1.png)

### 10.2. Bước 2 – Chạy khung API ở cổng 5000

```bash
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

![EduSubmit Bước 2 – GET / trả 200](img/edu-buoc2.png)

### 10.3. Bước 3 – DTO và danh sách bài tập

`record AssignmentDto(int Id, string Title, string Description, bool IsOpen)`, dữ liệu mẫu 2 bài tập, `GET /api/assignments` → 200. C# viết `IsOpen` nhưng JSON là `isOpen` vì ASP.NET mặc định đổi tên sang camelCase.

![EduSubmit Bước 3 – GET /api/assignments](img/edu-buoc3.png)

### 10.4. Bước 4 – Chi tiết một bài tập

![EduSubmit Bước 4 – 4a, 4b, 4c](img/edu-buoc4.png)

| Ca | Yêu cầu | Kết quả thật | Ai trả |
|----|---------|--------------|--------|
| 4a | `GET /api/assignments/1` | 200 + bài tập 1 | Handler |
| 4b | `GET /api/assignments/999` | 404 + `ASSIGNMENT_NOT_FOUND` | Handler |
| 4c | `GET /api/assignments/abc` | 404, body rỗng | Framework (không khớp `{assignmentId:int}`) |

4b và 4c đều 404, nhưng chỉ 4b có `ApiError` nên máy khách xử lý được bằng code (đọc `code`); 4c là lỗi gõ sai đường dẫn.

### 10.5. Bước 5 – Nộp bài: 2 DTO và 3 cổng kiểm tra

- Đầu vào `CreateSubmissionRequest(StudentCode, RepositoryUrl)` – không có `Id`, `Status`.
- Đầu ra `SubmissionDto(Id, AssignmentId, StudentCode, RepositoryUrl, Status, ReceivedAt)` – máy chủ tự sinh `Id = Guid.NewGuid()`, `Status = "received"`, `ReceivedAt = UtcNow`, `AssignmentId` lấy từ URI.
- Kiểm tra theo thứ tự: bài tập **tồn tại** (404) → **còn mở** (400 CLOSED) → body **hợp lệ** (400 INVALID) → tạo, trả 201 + Location.

![EduSubmit Bước 5 – 5a đến 5g](img/edu-buoc5.png)

Ca 5h: tạm đặt bài tập 2 `IsOpen = false` rồi POST vào bài 2 (sau đó trả lại `true`):

![EduSubmit Bước 5 – 5h bài tập đã đóng](img/edu-buoc5h.png)

| Ca | Yêu cầu | Kết quả thật | Ai trả |
|----|---------|--------------|--------|
| 5a | POST bài 1, `sv001.json` | 201 + `Location: /api/submissions/{guid}` | Handler |
| 5b | POST bài 1, `rong.json` | 400 `INVALID_SUBMISSION` | Handler |
| 5c | POST bài 999 | 404 `ASSIGNMENT_NOT_FOUND` | Handler |
| 5d | Bỏ `-H "Content-Type..."` | 415, body rỗng | Framework |
| 5e | JSON hỏng (`hong.json`) | 400, body là văn bản `BadHttpRequestException` | Framework |
| 5f | Thiếu `repositoryUrl` (`thieu-repo.json`) | 400 `INVALID_SUBMISSION` | Handler (trường thiếu thành `null`) |
| 5g | `PUT /api/assignments/1` | 405, `Allow: GET` | Framework |
| 5h | POST vào bài tập đã đóng | 400 `ASSIGNMENT_CLOSED` | Handler |

### 10.6. Bước 6 – Đọc lại lượt nộp theo Location

![EduSubmit Bước 6 – 6a, 6b](img/edu-buoc6.png)

Ca 6c: tắt API, chạy lại, gọi lại id cũ → 404 vì danh sách lượt nộp nằm trong bộ nhớ tiến trình đã dừng:

![EduSubmit Bước 6 – 6c](img/edu-buoc6c.png)

### 10.7. Bước 7 – Máy khách Submission.Client

```bash
dotnet run --project src/Submission.Client -- http://localhost:5000
```

![EduSubmit Bước 7 – máy khách 4 nấc](img/edu-buoc7.png)

```text
Danh sach bai tap dang mo:
- 1: Bai tap REST API
- 2: Bai tap Client

Da tao luot nop: a326e1d0-5cdc-4b47-b745-c6fbf71fe1fc
Trang thai ban dau: received

Doc lai luot nop tai /api/submissions/a326e1d0-5cdc-4b47-b745-c6fbf71fe1fc:
  SV001 - https://example.com/sv001/rest-api

Thu gui yeu cau loi: 400 BadRequest
INVALID_SUBMISSION: Thieu ma sinh vien hoac kho ma nguon.
```

1. Nấc 1 – liệt kê: `GetFromJsonAsync` (danh sách luôn 200), lọc `IsOpen`.
2. Nấc 2 – nộp: `PostAsJsonAsync`, **kiểm tra `StatusCode == Created` trước** rồi mới đọc `SubmissionDto`; mã khác thì đọc `ApiError` bằng hàm chung `PrintApiErrorAsync`.
3. Nấc 3 – đọc lại: gọi đúng `createResponse.Headers.Location` bằng `GetAsync` (không ném khi 404).
4. Nấc 4 – gửi sai: nhận 400, in `code: message`, không sập.

Cái bẫy `GetFromJsonAsync` (thử tạm gọi `/api/assignments/999`, sau đó bỏ): ném exception khi 404 và mất body `ApiError`:

![Bẫy GetFromJsonAsync với 404](img/edu-bay-getfromjson.png)

Thí nghiệm 7a, 7b (lỗi mạng – không có phản hồi) và 7c (thêm bài tập 3 vào API, client không sửa vẫn in thêm):

![7a và 7b – lỗi kết nối](img/edu-7a-7b.png)

![7c – client tự in thêm bài tập 3](img/edu-7c.png)

### 10.8. Bước 8 – requests.http

Tệp `RestfulClientServer/requests.http` có 15 yêu cầu (1–9 cho bước 1–8, 10–15 cho bước 9). Request 6 dùng `# @name createSubmission` và `{{createSubmission.response.body.$.id}}` để đọc lại lượt nộp vừa tạo. Kết quả 9 request đầu (chạy trước khi làm bước 9):

![EduSubmit Bước 8 – requests.http](img/edu-buoc8.png)

| # | Request | Mong đợi | Thực tế |
|---|---------|----------|---------|
| 1 | `GET /` | 200 | 200 |
| 2 | `GET /api/assignments` | 200 | 200 |
| 3 | `GET /api/assignments/1` | 200 | 200 |
| 4 | `GET /api/assignments/999` | 404 | 404 `ASSIGNMENT_NOT_FOUND` |
| 5 | `POST .../1/submissions` | 201 | 201 + Location |
| 6 | `GET /api/submissions/{id vừa tạo}` | 200 | 200 |
| 7 | `POST` body rỗng | 400 | 400 `INVALID_SUBMISSION` |
| 8 | `POST .../999/submissions` | 404 | 404 `ASSIGNMENT_NOT_FOUND` |
| 9 | `GET /api/submissions/0000...` | 404 | 404 `SUBMISSION_NOT_FOUND` |

### 10.9. Bước 9 (mở rộng) – 409 Conflict và DELETE 204

- Thêm bài tập 3 đã đóng (`IsOpen = false`).
- Trong POST, sau kiểm tra `INVALID_SUBMISSION`: nếu sinh viên đã nộp bài tập này → 409 `DUPLICATE_SUBMISSION`.
- `DELETE /api/submissions/{submissionId:guid}` → 204 không body, không có → 404.

![EduSubmit Bước 9 – request 10–15](img/edu-buoc9.png)

Ca 9g – chạy client 2 lần, lần 2 nhận 409 và client (không sửa dòng nào) in đúng lỗi:

![EduSubmit Bước 9 – 9g](img/edu-buoc9g.png)

| Ca | Yêu cầu | Kết quả thật |
|----|---------|--------------|
| 9a | POST bài tập 3 (đã đóng) | 400 `ASSIGNMENT_CLOSED` |
| 9b | POST bài 1, SV002 | 201 Created |
| 9c | Gửi lại 9b | 409 `DUPLICATE_SUBMISSION` |
| 9d | DELETE lượt nộp 9b | 204 No Content |
| 9e | Gửi lại 9d | 404 `SUBMISSION_NOT_FOUND` |
| 9f | Gửi lại 9b sau khi rút | 201, id mới |
| 9g | Chạy client 2 lần | Lần 2: `409 Conflict - DUPLICATE_SUBMISSION` |

Bảng phá thử và trả lời 3 câu hỏi: xem [README.md](../README.md).

Commit EduSubmit:

```text
EduSubmit Buoc 1: tao solution RestfulClientServer
EduSubmit Buoc 2: chay may chu cong 5000, GET / tra JSON
EduSubmit Buoc 3: DTO va GET /api/assignments
EduSubmit Buoc 4: GET /api/assignments/{assignmentId}
EduSubmit Buoc 5: POST /api/assignments/{assignmentId}/submissions
EduSubmit Buoc 6: GET /api/submissions/{submissionId}
EduSubmit Buoc 7: Submission.Client liet ke, nop, doc lai, gui sai
EduSubmit Buoc 8: requests.http
EduSubmit Buoc 9 (mo rong): 409 Conflict va DELETE 204
```

Luồng thông điệp:

```text
Client --(GET  /api/assignments)------------------> Api : 200
Client --(POST /api/assignments/{id}/submissions)-> Api : 201 + Location / 400 / 404 / 409
Client --(GET  Location = /api/submissions/{id})--> Api : 200 / 404
```

## 11. Khó khăn gặp phải và cách giải quyết

- Làm Bước 1–3 trước khi tạo repo git. Cách giải quyết: clone fork về máy, đồng bộ với repo lớp (`git pull upstream main`), chép bài vào `Bai2/` và commit lại đúng theo từng bước.
- Sửa code nhưng gọi API vẫn ra kết quả cũ (404 ở `/api/products`) vì máy chủ cũ vẫn đang chạy. Cách giải quyết: Ctrl+C ở Terminal 1 rồi chạy lại máy chủ sau mỗi lần sửa.
- `dotnet build` báo lỗi khóa tệp `Catalog.Api.exe` khi máy chủ còn chạy. Cách giải quyết: tắt máy chủ trước khi build lại.
- Có dùng trợ lý AI để hỗ trợ viết code, chạy kiểm thử và ghi nhật ký; đã đọc lại code và đối chiếu kết quả với slide.
