# Bài 2 — RESTful API

## Catalog

### Bước 2 — Chạy Catalog API và kiểm tra endpoint `/`

**Ảnh 1.** API trả về `200 OK` và thông tin trạng thái dưới dạng JSON.

![Bước 2 - Kiểm tra endpoint gốc của Catalog API](docs/buoc-2-api-200.png)

### Bước 3 — Kiểm tra danh sách sản phẩm

**Ảnh 1.** Gọi `GET /api/products` trả về `200 OK` và danh sách 3 sản phẩm.

![Bước 3 - Kiểm tra danh sách sản phẩm](docs/buoc-3-danh-sach-san-pham.png)

### Bước 4 — Xem chi tiết sản phẩm

**Ảnh 1.** Bốn ca kiểm tra lần lượt trả `200 OK`, `404 PRODUCT_NOT_FOUND`, `404` với body rỗng và `405 Method Not Allowed`.

![Catalog - Kiểm tra chi tiết sản phẩm](docs/catalog-buoc-4-kiem-thu.png)

### Bước 5 — Tạo sản phẩm

**Ảnh 1.** Tạo sản phẩm hợp lệ trả `201 Created` kèm `Location`, giá âm trả `400 INVALID_PRODUCT` và yêu cầu thiếu `Content-Type` JSON trả `415 Unsupported Media Type`.

![Catalog - Kiểm tra tạo sản phẩm](docs/catalog-buoc-5-kiem-thu.png)

### Bước 6 — Kiểm tra bằng requests.http

**Ảnh 1.** Kiểm tra máy chủ trả `200 OK`.

![Catalog bước 6 - Kiểm tra máy chủ](docs/catalog-buoc-6-anh-1.png)

**Ảnh 2.** Lấy danh sách sản phẩm trả `200 OK`.

![Catalog bước 6 - Danh sách sản phẩm](docs/catalog-buoc-6-anh-2.png)

**Ảnh 3.** Lấy sản phẩm ID 1 trả `200 OK`.

![Catalog bước 6 - Sản phẩm ID 1](docs/catalog-buoc-6-anh-3.png)

**Ảnh 4.** Sản phẩm ID 999 trả `404 PRODUCT_NOT_FOUND`.

![Catalog bước 6 - Sản phẩm không tồn tại](docs/catalog-buoc-6-anh-4.png)

**Ảnh 5.** ID `abc` trả `404` với body rỗng.

![Catalog bước 6 - ID không phải số](docs/catalog-buoc-6-anh-5.png)

**Ảnh 6.** Tạo sản phẩm hợp lệ trả `201 Created` và `Location`.

![Catalog bước 6 - Tạo sản phẩm](docs/catalog-buoc-6-anh-6.png)

**Ảnh 7.** Giá không hợp lệ trả `400 INVALID_PRODUCT`.

![Catalog bước 6 - Giá không hợp lệ](docs/catalog-buoc-6-anh-7.png)

**Ảnh 8.** Thiếu `Content-Type` JSON trả `415 Unsupported Media Type`.

![Catalog bước 6 - Thiếu Content-Type JSON](docs/catalog-buoc-6-anh-8.png)

### Bước 7–8 — Chạy Catalog.Client

**Ảnh 1.** Máy khách liệt kê sản phẩm, tạo sản phẩm mới, đọc chi tiết sản phẩm 1 và xử lý `400 INVALID_PRODUCT` khi gửi dữ liệu sai.

![Catalog - Kết quả máy khách](docs/catalog-buoc-8-client.png)

## EduSubmit

### Lệnh build và chạy

Mở PowerShell tại thư mục `Bai2/RestfulClientServer`, sau đó build cả solution:

```powershell
dotnet build
```

Chạy API trong terminal thứ nhất và giữ terminal đó mở:

```powershell
dotnet run --project src/Submission.Api -- --urls http://localhost:5000
```

Chạy client trong terminal thứ hai, cũng tại thư mục `RestfulClientServer`:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Bước 9 kiểm tra nộp trùng. Nếu SV001 đã nộp bài tập 1, chạy lại client sẽ nhận `409 DUPLICATE_SUBMISSION`. Khởi động lại API hoặc xóa lượt nộp cũ để chạy lại ca tạo mới. Dữ liệu chỉ lưu trong bộ nhớ và sẽ mất khi API dừng.

### Bước 1 — Kiểm tra solution

**Ảnh 1.** Chạy `dotnet build` trong thư mục `RestfulClientServer`. Cả `Submission.Api` và `Submission.Client` đều build thành công.

![EduSubmit - Build solution](docs/edusubmit-buoc-1-build.png)

### Bước 2 — Chạy và kiểm tra máy chủ

**Ảnh 1.** Máy chủ `Submission.Api` chạy trên cổng 5000.

![EduSubmit bước 2 - Chạy máy chủ](docs/edusubmit-buoc-2-may-chu.png)

**Ảnh 2.** Gọi `GET /` bằng `curl.exe` trả `200 OK` và JSON trạng thái dịch vụ.

![EduSubmit bước 2 - Gọi endpoint gốc](docs/edusubmit-buoc-2-curl.png)

### Bước 3 — Danh sách bài tập

**Ảnh 1.** Gọi `GET /api/assignments` trả `200 OK` và hai bài tập mẫu.

![EduSubmit bước 3 - Danh sách bài tập](docs/edusubmit-buoc-3-danh-sach.png)

### Bước 4 — Chạy API và kiểm tra chi tiết bài tập

**Ảnh 1.** Submission API đang chạy tại `http://localhost:5000`.

![EduSubmit - Chạy API trên cổng 5000](docs/edusubmit-api-cong-5000.png)

**Ảnh 2.** Kiểm tra bài tập có ID `1`, ID `999` không tồn tại và ID `abc` không hợp lệ. Các kết quả lần lượt là `200 OK`, `404` kèm `ASSIGNMENT_NOT_FOUND` và `404` với body rỗng.

![EduSubmit - Kiểm tra chi tiết bài tập](docs/edusubmit-buoc-4.png)

### Bước 5 — Nộp bài

**Ảnh 1.** Nộp bài hợp lệ trả `201 Created` và `Location`. Dữ liệu rỗng trả `400 INVALID_SUBMISSION`, còn bài tập không tồn tại trả `404 ASSIGNMENT_NOT_FOUND`.

![EduSubmit - Kiểm tra nộp bài](docs/edusubmit-buoc-5.png)

### Bước 6 — Đọc lại lượt nộp

**Ảnh 1.** Tạo lượt nộp trả `201 Created` kèm `Location`. Gọi GUID không tồn tại trả `404 SUBMISSION_NOT_FOUND`. Ảnh cũng cho thấy yêu cầu với chuỗi `ID-VUA-TAO` trả `404` vì không khớp định dạng GUID.

![EduSubmit - Tạo lượt nộp và kiểm tra 404](docs/edusubmit-buoc-6-tao-va-404.png)

**Ảnh 2.** Gọi đúng địa chỉ trong `Location` trả `200 OK` và thông tin lượt nộp.

![EduSubmit - Đọc lại lượt nộp](docs/edusubmit-buoc-6-doc-lai.png)

**Ảnh 3.** Ca 6c: tạo lượt nộp và ghi lại ID từ `Location`.

![EduSubmit bước 6c - Tạo lượt nộp](docs/edusubmit-buoc-6c-tao.png)

**Ảnh 4.** Dừng và khởi động lại API.

![EduSubmit bước 6c - Khởi động lại API](docs/edusubmit-buoc-6c-khoi-dong-lai.png)

**Ảnh 5.** Gọi lại ID cũ trả `404 SUBMISSION_NOT_FOUND` vì dữ liệu trong bộ nhớ đã mất khi API dừng.

![EduSubmit bước 6c - Đọc lại ID cũ](docs/edusubmit-buoc-6c-ket-qua.png)

### Bước 7 — Chạy máy khách

**Ảnh 1.** Máy khách liệt kê bài tập đang mở, tạo và đọc lại lượt nộp, rồi xử lý yêu cầu sai với `400 INVALID_SUBMISSION`.

![EduSubmit - Kết quả máy khách](docs/edusubmit-buoc-7-client.png)

### Bước 8 — Kiểm tra bằng requests.http

**Ảnh 1.** Lấy danh sách bài tập trả `200 OK`.

![EduSubmit bước 8 - Danh sách bài tập](docs/edusubmit-buoc-8-anh-1.png)

**Ảnh 2.** Lấy bài tập ID 1 trả `200 OK`.

![EduSubmit bước 8 - Bài tập ID 1](docs/edusubmit-buoc-8-anh-2.png)

**Ảnh 3.** Bài tập ID 999 trả `404 ASSIGNMENT_NOT_FOUND`.

![EduSubmit bước 8 - Bài tập không tồn tại](docs/edusubmit-buoc-8-anh-3.png)

**Ảnh 4.** Nộp bài hợp lệ trả `201 Created` và `Location`.

![EduSubmit bước 8 - Tạo lượt nộp](docs/edusubmit-buoc-8-anh-4.png)

**Ảnh 5.** Đọc lại lượt nộp vừa tạo trả `200 OK`.

![EduSubmit bước 8 - Đọc lại lượt nộp](docs/edusubmit-buoc-8-anh-5.png)

**Ảnh 6.** Nộp dữ liệu rỗng trả `400 INVALID_SUBMISSION`.

![EduSubmit bước 8 - Dữ liệu không hợp lệ](docs/edusubmit-buoc-8-anh-6.png)

**Ảnh 7.** Lượt nộp không tồn tại trả `404 SUBMISSION_NOT_FOUND`.

![EduSubmit bước 8 - Lượt nộp không tồn tại](docs/edusubmit-buoc-8-anh-7.png)

### Bước 9 — Kiểm tra nộp trùng và xóa lượt nộp

**Ảnh 1.** Nộp vào bài tập đã đóng trả `400 ASSIGNMENT_CLOSED`.

![EduSubmit bước 9a - Bài tập đã đóng](docs/edusubmit-buoc-9-anh-1.png)

**Ảnh 2.** Nộp bài SV002 trả `201 Created`.

![EduSubmit bước 9b - Tạo lượt nộp](docs/edusubmit-buoc-9-anh-2.png)

**Ảnh 3.** Nộp trùng trả `409 DUPLICATE_SUBMISSION`.

![EduSubmit bước 9c - Nộp trùng](docs/edusubmit-buoc-9-anh-3.png)

**Ảnh 4.** Xóa lượt nộp trả `204 No Content`.

![EduSubmit bước 9d - Xóa lượt nộp](docs/edusubmit-buoc-9-anh-4.png)

**Ảnh 5.** Xóa lại lượt nộp trả `404 SUBMISSION_NOT_FOUND`.

![EduSubmit bước 9e - Xóa lại lượt nộp](docs/edusubmit-buoc-9-anh-5.png)

**Ảnh 6.** Nộp lại SV002 sau khi xóa trả `201 Created` với ID mới.

![EduSubmit bước 9f - Nộp lại sau khi xóa](docs/edusubmit-buoc-9-anh-6.png)

### Bảng phá thử EduSubmit

| Ca | Yêu cầu | Kết quả mong đợi | Kết quả đã kiểm tra |
| --- | --- | --- | --- |
| 5a | POST bài tập 1 với `sv001.json` hợp lệ | `201 Created`, có `Location` | Đã kiểm tra, xem ảnh bước 5 |
| 5b | POST bài tập 1 với `rong.json` | `400 INVALID_SUBMISSION` | Đã kiểm tra, xem ảnh bước 5 |
| 5c | POST bài tập 999 | `404 ASSIGNMENT_NOT_FOUND` | Đã kiểm tra, xem ảnh bước 5 |
| 5d | POST với `Content-Type: text/plain` thay vì JSON | `415 Unsupported Media Type` | Đã kiểm tra bằng HTTP, trả `415` |
| 5e | POST body JSON sai cú pháp | `400 Bad Request` | Đã kiểm tra bằng HTTP, trả `400` |
| 5f | POST body thiếu `repositoryUrl` | `400 INVALID_SUBMISSION` | Đã kiểm tra bằng HTTP, trả `400 INVALID_SUBMISSION` |
| 5g | PUT `/api/assignments/1` | `405 Method Not Allowed` | Đã kiểm tra bằng HTTP, trả `405` |
| 5h | POST vào bài tập đã đóng | `400 ASSIGNMENT_CLOSED` | Đã kiểm tra với bài tập 3, xem ảnh bước 9a |
| 6a | GET địa chỉ `Location` vừa tạo | `200 OK`, đúng lượt nộp | Đã kiểm tra, xem ảnh bước 6 |
| 6b | GET GUID chưa tồn tại | `404 SUBMISSION_NOT_FOUND` | Đã kiểm tra, xem ảnh bước 6 |
| 6c | Khởi động lại API rồi GET lượt nộp cũ | `404 SUBMISSION_NOT_FOUND` | Đã kiểm tra, xem ảnh 3–5 của bước 6 |

### Ba câu hỏi giải thích

#### 1. Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?

URI định danh tài nguyên mà máy khách muốn làm việc, còn phương thức HTTP thể hiện thao tác. Trong code, `assignments` là tập bài tập và `submissions` là tập lượt nộp. Ví dụ `GET /api/assignments/1` đọc bài tập số 1, còn `POST /api/assignments/1/submissions` tạo lượt nộp thuộc bài tập đó.

#### 2. “Stateless” thể hiện ở đâu trong code đã viết?

Mỗi yêu cầu tự mang thông tin cần thiết để API xử lý. Với `POST /api/assignments/1/submissions`, mã bài tập nằm trong URI, còn `studentCode` và `repositoryUrl` nằm trong body. Máy chủ xử lý dựa trên yêu cầu hiện tại, không cần nhớ phiên làm việc của máy khách từ lần gọi trước. Danh sách bài tập và lượt nộp vẫn được lưu trong bộ nhớ máy chủ.

#### 3. Vì sao client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?

Hai bên trao đổi thông qua cấu trúc JSON đã thống nhất. Server và client mỗi bên khai báo `CreateSubmissionRequest` riêng với `StudentCode` và `RepositoryUrl`, tương ứng với các trường `studentCode` và `repositoryUrl` trong JSON. Hai project vì thế có thể phát triển độc lập, dựa trên cùng một hợp đồng dữ liệu.
