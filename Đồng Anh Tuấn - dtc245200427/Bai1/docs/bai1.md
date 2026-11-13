# Bài 1 – Tổng quan hệ thống và MEP
Họ tên: **Đồng Anh Tuấn**  MSSV: **dtc245200427**  Lớp: **CNTT K23D**

## 1. Cài đặt
- `dotnet --version`: **10.0.201**.
- Build `EduSubmit-dtc245200427.slnx`: thành công, **0 cảnh báo, 0 lỗi**.
- Chưa chèn ảnh chụp màn hình kết quả.

## 2. MepDemo (Bước 7)
- MEP 1 – request–response: client gửi yêu cầu và chờ phản hồi tương ứng.
- MEP 2 – one-way: bên gửi chuyển thông điệp đi mà không chờ phản hồi nghiệp vụ.
- MEP 3 – queue: thông điệp vào hàng đợi và mỗi thông điệp được một worker xử lý; các worker chia nhau công việc.
- MEP 4 – publish–subscribe: một sự kiện được phát tới mọi dịch vụ đã đăng ký nhận sự kiện đó.
- MEP 5 – streaming: dữ liệu được chuyển thành nhiều phần liên tiếp thay vì chờ một kết quả gộp.
- MEP 3: đã chạy `MepDemo` với lựa chọn 3; các job `sub-1` đến `sub-4` được Worker A/B chia nhau, mỗi job chỉ xuất hiện ở một worker.
- MEP 4: đã chạy `MepDemo` với lựa chọn 4; sự kiện `SubmissionGraded` được `ScoreService`, `NotificationService` và `StatsService` nhận.

## 3. BasicWebApi (Bước 8)
- Thư mục mẫu không có sẵn nên đã tự dựng lại Minimal API theo đặc tả trong tài liệu, không lấy code từ PR của sinh viên khác. Build thành công, 0 cảnh báo/0 lỗi.
- Kết quả kiểm tra: `GET /` → 200 với `application=BasicWebApi`; `GET /hello/An?language=en` → 200, `Hello, An!`; `POST /greetings` tên `Binh` → 201, `Location: /greetings/1`; POST tên rỗng → 400 với `Name must not be empty.`.
- Thử mở rộng `language=ja`: `GET /hello/An?language=ja` trả 200, `Konnichiwa, An!`.
- `builder.Services.AddSingleton(...)` phải đứng trước `builder.Build()` để đăng ký dịch vụ trước khi ứng dụng được dựng.
- `name` lấy từ route `/hello/{name}`; `language` lấy từ query string `?language=en`.
- `Results.Created` trả 201 vì POST tạo tài nguyên mới; header `Location` cho biết đường dẫn tới tài nguyên vừa tạo.
- Chưa chụp ảnh REST Client/debugger; các mã HTTP trên đã được kiểm tra trực tiếp với API.

## 4. MiniDistributedSystem (Bước 9)
- Không có thư mục mẫu riêng; đã tự chạy hệ EduSubmit theo cùng đặc tả ở cổng 5000. API và Client là hai tiến trình độc lập, giao tiếp bằng HTTP/JSON và không tham chiếu code của nhau.
- Các cặp request–response: `GET /api/assignments` → 200; `POST /api/submissions` → 201; `GET /api/submissions/{id}` với ID vừa tạo → 200; ID không tồn tại → 404.
- Client và API thống nhất qua tên endpoint, HTTP method, trường JSON và record tương ứng; Client khai báo các record riêng.
- Địa chỉ API là đối số dòng lệnh để đổi máy chủ/cổng mà không sửa code.
- Chạy đủ luồng: Client in 2 assignment, tạo lượt nộp trạng thái `received`, rồi đọc lại đúng ID.

## 5. Thí nghiệm lỗi (Bước 10)
| # | Dự đoán | Thực tế | Đúng/Sai | Giải thích |
|---|---|---|---|---|
| a | Thêm assignment thứ ba thì Client vẫn nhận được danh sách mới mà không cần sửa code. | Trong bản API tạm, thêm assignment số 3; Client in cả ba bài mà không đổi code. Sau đó giữ nguyên API nộp bài (hai assignment). | Đúng | Client giải mã JSON thành `List<Assignment>` nên không cố định số phần tử. |
| b | Tắt API thì Client không kết nối được và phát sinh lỗi HTTP. | Đã tái hiện với API không chạy ở cổng 5999; Client in thông báo không kết nối được và kết thúc mã lỗi 1. | Đúng | Client bắt `HttpRequestException` và hướng dẫn chạy API/kiểm tra cổng. |
| c | StudentCode rỗng thì API trả 400; `EnsureSuccessStatusCode()` ném ngoại lệ cho phản hồi lỗi. | API trả 400; Client hiện tại in `API trả về lỗi HTTP 400: BadRequest` và kết thúc mã lỗi 1, không hiện hộp thoại ngoại lệ. | Đúng | `EnsureSuccessStatusCode()` tạo `HttpRequestException`; Client bắt lỗi này và hiển thị mã trạng thái. |
| d | Nếu API chạy ở cổng 5050 mà Client còn gọi 5000 thì lỗi kết nối; truyền URL 5050 qua đối số Client để sửa mà không đổi code. | Đã xác nhận Client lỗi khi gọi sai cổng và chạy thành công khi API/Client cùng dùng cổng 5050. | Đúng | URL API nhận qua đối số dòng lệnh. |

## 6. Dự án EduSubmit tự làm (Bước 11–12)
- Build đã thành công với 0 cảnh báo/0 lỗi. Khi chạy API ở cổng 5000, Client nhận danh sách 2 assignment, POST lượt nộp nhận trạng thái `received`, rồi GET lại lượt nộp thành công.
- Các lần chạy thủ công trước đó xác nhận GET lượt nộp vừa tạo trả 200; GET ID không tồn tại trả 404.
- Chưa chèn ảnh chụp màn hình; kết quả lệnh và mã trạng thái đã được ghi lại ở trên.
- Dự án EduSubmit được viết theo yêu cầu Bài 1; không sao chép dự án mẫu BasicWebApi/MiniDistributedSystem.
- Luồng thông điệp:
  - Client --(HTTP GET /api/assignments)--> API: request–response.
  - Client --(HTTP POST /api/submissions)--> API: request–response; API kiểm tra dữ liệu và tạo lượt nộp.
  - Client --(HTTP GET /api/submissions/{id})--> API: request–response để đọc lại lượt nộp.

## 7. Khó khăn gặp phải và cách giải quyết
- Ban đầu chạy Client khi API chưa bật làm ứng dụng kết thúc bằng ngoại lệ .NET không được xử lý (hộp thoại Windows báo mã `0xe0434352`). Đã sửa Client để bắt lỗi kết nối, lỗi HTTP, timeout và địa chỉ URI sai; giờ Client in thông báo dễ hiểu.
- Một lần build bị khóa file do tiến trình Client cũ còn chạy; đóng tiến trình rồi build lại thành công.
- Thư mục `bin/` và `obj/` đã được loại trừ bằng `.gitignore`, không đưa vào bài nộp.

