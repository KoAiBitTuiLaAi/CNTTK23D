# Bài 1 – Tổng quan hệ thống và MEP

Họ tên: Trần Trung Dũng · MSSV: dtc245200478 · Lớp: (chưa có thông tin)

> Bản nộp đầy đủ nằm trong thư mục `Bai1` của repository này. Bài thực hành dùng .NET SDK `10.0.403` và Git `2.56.0.windows.4`.

## 1. Cài đặt và kiểm tra môi trường

- `dotnet --version`: `10.0.403`.
- `git --version`: `git version 2.56.0.windows.4`.
- Đã tạo và chạy ứng dụng Hello World theo bước 4; kết quả `Hello, World!`.
- Windows cài sẵn .NET 10 và Git. Chưa xác nhận danh sách extension trong VS Code.

Ảnh chụp kết quả bước 4: chưa được lưu.

## 2. MepDemo (Bước 7)

- **MEP 3 – queue-based:** mỗi bài nộp `sub-x` do đúng **một worker** xử lý; hai worker lấy việc trong cùng hàng đợi và chia nhau các bài.
- **MEP 4 – publish–subscribe:** một sự kiện được gửi cho **3 dịch vụ** đã đăng ký: ScoreService, NotificationService và StatsService.
- **Mô tả 5 MEP:**
  1. Request–response: bên gọi gửi yêu cầu và chờ kết quả trả về.
  2. One-way: bên gửi chuyển thông điệp rồi tiếp tục, không chờ kết quả nghiệp vụ.
  3. Queue-based: công việc được xếp hàng và một worker trong nhóm lấy mỗi công việc ra xử lý.
  4. Publish–subscribe: bên phát công bố sự kiện, mọi subscriber phù hợp đều nhận được.
  5. Streaming/push: bên gửi đẩy liên tiếp các cập nhật đến bên nhận.

Ảnh chụp MepDemo: chưa được lưu.

## 3. BasicWebApi (Bước 8)

REST Client thực hiện `GET /` (200), `POST /greetings` với tên Binh (201 Created, Location `/greetings/1`) và POST tên rỗng (400 Bad Request). Đã bổ sung `language=ja`; `/hello/An?language=ja` trả `Konnichiwa, An!`.

Ảnh chụp REST Client với 200, 201 và 400: chưa được lưu.

Trả lời mục 8.4:

1. `builder.Services.AddSingleton<GreetingService>()` phải chạy trước `builder.Build()`, vì đăng ký dịch vụ thuộc giai đoạn cấu hình builder; sau khi build xong container dịch vụ đã được tạo.
2. `name` lấy từ route `/hello/{name}`; `language` lấy từ query string `?language=en` (hoặc `ja`).
3. `Results.Created` trả `201 Created` cùng địa chỉ `Location` để chỉ tài nguyên vừa tạo; `200 OK` chỉ báo yêu cầu thành công và không diễn đạt việc tạo tài nguyên mới rõ bằng.

## 4. MiniDistributedSystem (Bước 9)

Hai tiến trình chạy thành công: API lắng nghe cổng 5000; client lấy hai bài tập, gửi lượt nộp và nhận mã lượt nộp mới cùng trạng thái `received`.

Ảnh chụp hai terminal API và Client: chưa được lưu.

1. Client gọi `GET http://localhost:5000/api/assignments` (API trả 200), rồi `POST http://localhost:5000/api/submissions` với JSON lượt nộp (API trả 201 Created).
2. Client không tham chiếu mã nguồn API. Hai bên thống nhất qua hợp đồng HTTP: method, URL, JSON request/response và mã trạng thái.
3. URL API được truyền qua tham số dòng lệnh để chọn máy/cổng theo môi trường chạy mà không sửa và biên dịch lại mã client.

## 5. Thí nghiệm lỗi (Bước 10)

| # | Dự đoán | Thực tế | Đúng/Sai | Giải thích |
|---|---|---|---|---|
| a | Thêm bài tập thứ ba trong API thì client sẽ in thêm bài đó mà không cần sửa client. | Client in đủ bài 1, 2 và 3 sau khi API khởi động lại. | Đúng | Client đọc danh sách JSON động từ endpoint; API bổ sung dữ liệu đúng hợp đồng nên client không phụ thuộc số lượng bản ghi cố định. |
| b | Tắt API rồi chạy client sẽ báo không kết nối được. | `HttpRequestException`: kết nối tới `localhost:5000` bị từ chối (`actively refused`). | Đúng | Giao tiếp HTTP cần tiến trình server và cổng đích đang hoạt động. Gọi hàm trong cùng tiến trình không có bước kết nối qua mạng/cổng này. |
| c | Gửi `StudentCode` rỗng sẽ bị API từ chối với 400; client ném lỗi ở `EnsureSuccessStatusCode()`. | API trả 400 Bad Request; client phát `HttpRequestException` với trạng thái 400 tại dòng `EnsureSuccessStatusCode()`. | Đúng | Client chỉ kiểm tra phản hồi thành công trước khi đọc body. Server phải xác thực vì mọi client đều có thể gửi dữ liệu thiếu/sai. |
| d | API chuyển sang cổng 5050 trong khi client gọi 5000 sẽ không kết nối được; truyền đúng URL sẽ chạy lại không cần sửa code. | Gọi client với 5000 bị từ chối. Chạy client với tham số `http://localhost:5050` thành công, không đổi mã nguồn. | Đúng | Địa chỉ gốc được cấu hình từ đối số dòng lệnh; client cần trỏ tới đúng cổng mà API đang lắng nghe. |

Sau thí nghiệm a và c, code mẫu MiniDistributedSystem đã được đưa về trạng thái ban đầu.

## 6. Dự án EduSubmit tự làm (Bước 11–12)

Solution `Bai1/EduSubmit.slnx` gồm `Submission.Api` và `Submission.Client`, không project-reference lẫn nhau. API có GET `/`, GET `/api/assignments`, POST `/api/submissions` (kiểm tra dữ liệu, 400 nếu không hợp lệ; 201 và Location nếu hợp lệ) cùng phần mở rộng GET `/api/submissions/{id:guid}` (200 khi tìm thấy, 404 nếu không). Client đọc danh sách, gửi lượt nộp, kiểm tra mã phản hồi và đọc lại lượt vừa tạo. `Bai1/requests.http` có các ca 200, 201, 400 và GET lượt nộp 200/404.

**Đối chiếu code mẫu:** đã xem `Program.cs` của BasicWebApi và MiniDistributedSystem để hiểu pattern Minimal API, hợp đồng record và cách gọi `HttpClient`; vì thế phần này không phải bài tự gõ hoàn toàn không nhìn mẫu. Mã EduSubmit được triển khai riêng và có thêm kiểm tra assignment tồn tại cùng endpoint tra cứu lịch sử.

Ảnh chụp kết quả EduSubmit: chưa được lưu.

Luồng thông điệp:

```text
Client --(HTTP GET /api/assignments)--> Api : request-response, 200 + danh sách bài tập
Client <--(HTTP 200 + JSON danh sách)------ Api
Client --(HTTP POST /api/submissions)-----> Api : request-response, 201 + Location + JSON lượt nộp
Client <--(HTTP 201 + JSON lượt nộp)------- Api
Client --(HTTP GET /api/submissions/{id})--> Api : request-response, 200 + lượt nộp (404 nếu không có)
Client <--(HTTP 200 + JSON lượt nộp)-------- Api
```

## 7. Khó khăn gặp phải và cách giải quyết

- Khi khởi chạy solution mới trong VS Code, terminal cảnh báo publisher PowerShell chưa được tin cậy; các bước build/chạy đã thực hiện từ terminal ngoài VS Code.
- Các API dùng cổng 5000 luân phiên để tránh xung đột tiến trình; trước khi đổi dự án đã dừng API đang chạy.
- Chưa có thông tin lớp trong tài liệu đầu vào nên trường lớp được để trống.
- Câu hỏi tự kiểm tra: ứng dụng nguyên khối chạy các thành phần chính trong cùng ứng dụng/tiến trình; ứng dụng phân tán chia thành tiến trình/dịch vụ riêng và giao tiếp qua ranh giới mạng. Trong bài này, ranh giới nằm giữa API và Client, qua HTTP.
- Năm MEP là request–response, one-way, queue-based, publish–subscribe và streaming/push. EduSubmit dùng request–response.
- Queue phân phối mỗi thông điệp cho một worker trong nhóm cạnh tranh, ví dụ hàng đợi chấm bài. Publish–subscribe gửi một sự kiện đến mọi bên đăng ký, ví dụ một lượt chấm xong vừa lưu điểm, vừa thông báo sinh viên, vừa cập nhật thống kê.
- 200 là yêu cầu thành công; 201 là tạo tài nguyên thành công; 400 là request không hợp lệ; 404 là không tìm thấy tài nguyên. Client kiểm tra mã trạng thái để không diễn giải phản hồi lỗi như dữ liệu hợp lệ và tránh lỗi parse/logic sau đó.

## Kết quả kiểm tra

- `dotnet build` thành công cho BasicWebApi, MiniDistributedSystem, EduSubmit trong `Code_demo` và solution nộp `Bai1/EduSubmit.slnx` (không có cảnh báo hay lỗi build).
- Kiểm tra HTTP đã quan sát được BasicWebApi 200/201/400 và EduSubmit 200/201/400/404; client EduSubmit đọc lại lượt nộp vừa tạo thành công.
- Fork đã được đồng bộ với `upstream/main`; solution và hai project đã được thêm vào `Bai1` để build ngay tại thư mục nộp.
- Ảnh chụp màn hình chưa được lưu vào nhật ký trong lượt thao tác này; các liên kết ảnh ở trên cần được bổ sung trước khi nộp.
