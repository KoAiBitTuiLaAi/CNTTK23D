# EduSubmit – Bài 2 RESTful API

## Cấu trúc

- `src/Submission.Api`: Minimal API dùng cổng `5000`.
- `src/Submission.Client`: console client dùng `HttpClient`.
- `requests.http`: 7 request bắt buộc cho REST Client.
- `requests-extension.http`: các ca mở rộng `409 Conflict` và `204 No Content`.
- `sv001.json`, `rong.json`: body mẫu cho các lệnh POST.

## Chạy ứng dụng

Từ thư mục `RestfulClientServer`:

```powershell
dotnet build
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Mở terminal thứ hai để gọi thử API hoặc chạy client:

```powershell
curl.exe -i http://localhost:5000/
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Dừng máy chủ bằng `Ctrl+C`. Dữ liệu được lưu trong bộ nhớ nên sẽ mất khi khởi động lại API.

## Nhật ký kiểm tra

| Bước / yêu cầu | Lệnh hoặc thao tác | Kết quả quan sát |
|---|---|---|
| 1 | `dotnet build` | Build succeeded, 0 warning, 0 error |
| 2 | `curl.exe -i http://localhost:5000/` | `200 OK`, `Submission.Api`, `running` |
| 3 | `curl.exe -i http://localhost:5000/api/assignments` | `200 OK`, mảng JSON gồm 2 bài tập |
| 4 | GET `/api/assignments/1`, `/999`, `/abc` | `200`; `404 ASSIGNMENT_NOT_FOUND`; `404` rỗng do route không khớp |
| 5a | POST hợp lệ vào bài 1 | `201 Created` và header `Location` |
| 5b, 5f | POST thiếu dữ liệu / thiếu `repositoryUrl` | `400 INVALID_SUBMISSION` |
| 5c | POST vào bài 999 | `404 ASSIGNMENT_NOT_FOUND` |
| 5d | POST không có `Content-Type` | `415 Unsupported Media Type` |
| 5e | POST JSON sai cú pháp | `400 Bad Request` do framework |
| 5g | `PUT /api/assignments/1` | `405 Method Not Allowed`, `Allow: GET` |
| 5h | POST vào bài 2 đã đóng | `400 ASSIGNMENT_CLOSED` |
| 6 | GET URI trong `Location` sau POST | `200 OK`; ID không tồn tại trả `404 SUBMISSION_NOT_FOUND` |
| 7 | Chạy `Submission.Client` | In danh sách, tạo lượt nộp, đọc lại qua `Location`, rồi in `400 INVALID_SUBMISSION`; lỗi kết nối cũng được bắt riêng |
| 8 | Đếm request trong `requests.http` | Có đúng 7 request: GET, POST hợp lệ, POST lỗi, đọc lại và các ca 404 |
| 9 mở rộng | Gửi trùng, DELETE, GET sau DELETE | `409 DUPLICATE_SUBMISSION`, `204 No Content`, `404 SUBMISSION_NOT_FOUND`; gửi lại sau xóa nhận `201` |

## Ba câu hỏi

1. **Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?** URI định danh tài nguyên như `assignments` và `submissions`; phương thức nói hành động, ví dụ `GET` để đọc và `POST` để tạo. Vì thế `POST /api/assignments/1/submissions` thể hiện rõ việc tạo lượt nộp thuộc bài tập 1.
2. **“Stateless” thể hiện ở đâu?** Mỗi request tự mang thông tin cần xử lý: `assignmentId` nằm trên URI, `studentCode` và `repositoryUrl` nằm trong body. Server không dựa vào phiên client được giữ giữa các request; danh sách trong RAM chỉ là dữ liệu tài nguyên mẫu.
3. **Vì sao client và server khai báo DTO riêng mà vẫn khớp nhau?** Hai DTO là các lớp riêng ở hai tiến trình. Chúng khớp qua hợp đồng JSON: tên trường và kiểu dữ liệu giống nhau, không cần project client tham chiếu project server.

## Minh chứng

Các kết quả trong nhật ký là phản hồi nhận được khi chạy lệnh thật trên máy. Ảnh chụp màn hình CMD/Terminal chưa được đính kèm trong thư mục này.
