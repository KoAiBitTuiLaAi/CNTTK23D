# Báo cáo Bài 1 - Ứng dụng phân tán

## 1. Thông tin sinh viên
- Họ và tên: Nguyễn Xuân Hải
- Mã sinh viên: dtc245200551
- Học phần: Phát triển ứng dụng phân tán

## 2. Mục tiêu
Xây dựng một ứng dụng phân tán đơn giản gồm hai thành phần:
- **Submission.Api:** cung cấp các API HTTP để kiểm tra trạng thái, lấy danh sách bài tập, nhận bài nộp và tra cứu bài nộp.
- **Submission.Client:** gửi HTTP request đến API, đọc dữ liệu JSON và hiển thị kết quả.

## 3. Công nghệ
- C# / .NET
- ASP.NET Core Minimal API
- HttpClient và System.Net.Http.Json
- JSON
- Git / GitHub

## 4. Các chức năng đã xây dựng
| STT | Chức năng | Kết quả kiểm thử |
|---|---|---|
| 1 | GET `/` kiểm tra API | HTTP 200 |
| 2 | GET `/api/assignments` lấy danh sách | HTTP 200 |
| 3 | POST `/api/submissions` với dữ liệu hợp lệ | HTTP 201 |
| 4 | POST dữ liệu không hợp lệ | HTTP 400 |
| 5 | GET `/api/submissions/{id:guid}` với GUID tồn tại | HTTP 200 |
| 6 | GET với GUID không tồn tại | HTTP 404 |
| 7 | Client đọc lại bài nộp vừa tạo | Thành công |

## 5. Bốn tình huống thử nghiệm

### Tình huống 1: API bổ sung bài tập thứ ba
Bổ sung phần tử thứ ba vào danh sách `assignments` trong API, sau đó chạy Client mà không sửa mã nguồn Client. Mục tiêu là kiểm tra Client có thể nhận danh sách mới từ API hay không.

**Kết quả cần ghi nhận:** Client hiển thị thêm bài tập thứ ba.

### Tình huống 2: Dừng API khi Client đang cần kết nối
Dừng tiến trình API, sau đó chạy Client.

**Kết quả cần ghi nhận:** Client không kết nối được tới API và xuất hiện lỗi kết nối.

### Tình huống 3: Gửi mã sinh viên rỗng
Gửi yêu cầu POST `/api/submissions` với `studentCode` là chuỗi rỗng.

**Kết quả đã kiểm tra:** API trả về HTTP 400 Bad Request.

### Tình huống 4: Thay đổi cổng API
Chạy API tại cổng `5050`, trong khi Client mặc định vẫn sử dụng cổng `5000`. Sau đó chạy Client với URL API chính xác:

```powershell
dotnet run -- http://localhost:5050
```

**Kết quả cần ghi nhận:** Khi URL được cung cấp đúng, Client kết nối được với API tại cổng 5050.

## 6. Kết luận
Bài thực hành đã xây dựng được mô hình Client - API giao tiếp qua HTTP/JSON. Các chức năng gửi bài, kiểm tra dữ liệu và tra cứu theo GUID được kiểm tra bằng các mã trạng thái HTTP tương ứng.

## 7. Minh chứng
Đặt ảnh chụp màn hình vào thư mục `screenshots/`. Gợi ý tên ảnh:
- `01-api-running.png`
- `02-assignments.png`
- `03-post-201.png`
- `04-post-400.png`
- `05-get-guid-200.png`
- `06-get-guid-404.png`
- `07-scenario-1.png`
- `08-scenario-2.png`
- `09-scenario-3.png`
- `10-scenario-4.png`

Chỉ giữ lại các ảnh thực tế đã chụp trong quá trình làm bài. Không tạo ảnh minh chứng giả.
