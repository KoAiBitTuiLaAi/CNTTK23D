# Bài 1 – Tổng quan hệ thống và MEP
Họ tên: Hoàng Sơn<br>
MSSV: DTC245200599<br>
Lớp: CNTT K23D
 
## 1. Cài đặt
<img width="1483" height="762" alt="image" src="https://github.com/user-attachments/assets/3ab1de67-0a52-4332-9e5b-f15db99c14e0" />
 
## 2. MepDemo (Bước 7)
1. MEP 3: Mỗi bài nộp sub-x được một worker xử lý; hai worker cạnh tranh lấy việc từ cùng một hàng đợi.
2. MEP 4: Mỗi sự kiện được gửi tới 3 dịch vụ đã đăng ký: ScoreService, NotificationService và StatsService.
Mô tả mỗi MEP trong demo:
- MEP 1 – Request–response: Bên gửi yêu cầu và chờ bên nhận trả lời.
- MEP 2 – One-way: Bên gửi gửi thông điệp rồi tiếp tục công việc mà không chờ kết quả nghiệp vụ.
- MEP 3 – Queue-based: Công việc vào hàng đợi và chỉ một worker trong nhóm lấy mỗi công việc để xử lý.
- MEP 4 – Publish–subscribe: Một sự kiện được phát tới mọi dịch vụ đã đăng ký nhận sự kiện đó.
- MEP 5 – Streaming/push: Máy chủ đẩy các cập nhật liên tục tới máy khách.
 
## 3. BasicWebApi (Bước 8)
- Ảnh chụp REST Client: 200, 201, 400
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/9c3b684f-cc5b-4ea2-9725-edd6b322c1c5" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/a48dca0b-0579-46d8-a6a8-7414094dfc51" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/afc85a8d-814b-42aa-99ff-3ce7d5d69feb" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/401ed818-46c9-4760-af31-9907ecb70c66" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/23d26b32-b81e-40f9-a665-fb1c00dc15ed" />
- Trả lời 3 câu hỏi mục 8.4
- 
builder.Services.AddSingleton<GreetingService>() phải đứng trước builder.Build(). Các dịch vụ cần được đăng ký khi cấu hình ứng dụng; Build() tạo ứng dụng từ cấu hình đó.
Với /hello/{name}?language=en, name được lấy từ đoạn route {name} (ví dụ /hello/An → An), còn language được lấy từ query string sau dấu ?.
POST hợp lệ tạo một lời chào mới, nên Results.Created(...) trả 201 Created và header Location trỏ tới tài nguyên vừa tạo (/greetings/{id}). Results.Ok chỉ báo yêu cầu thành công với 200, không thể hiện rõ việc tạo tài nguyên mới.
 
## 4. MiniDistributedSystem (Bước 9)
- Ảnh chụp 2 terminal API + Client
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/4b12b134-92c8-40a6-8e06-4a67122d7a5b" />
- Trả lời 3 câu hỏi

Khi chạy bình thường, client gọi ba endpoint; mỗi lần gọi tạo một cặp request–response:
- GET http://localhost:5000/api/assignments → 200 OK.
- POST http://localhost:5000/api/submissions → 201 Created, kèm thông tin lượt nộp mới.
- GET http://localhost:5000/api/submissions/{id} → 200 OK, trả lượt nộp vừa tạo.

Client không dùng chung code với API; nó tự khai báo các record dữ liệu của mình. Hai bên thống nhất qua hợp đồng HTTP: URL, method, cấu trúc JSON và ý nghĩa mã trạng thái.

Truyền địa chỉ API qua tham số giúp dùng cùng một client với các địa chỉ hoặc môi trường khác nhau mà không sửa code hay build lại. Với lệnh dotnet run, dấu -- ngăn cách tùy chọn của dotnet với tham số ứng dụng; địa chỉ sau dấu đó được client đọc từ args[0].
 
## 5. Thí nghiệm lỗi (Bước 10)
| # | Dự đoán | Thực tế | Đúng/Sai | Giải thích |
|---|---|---|---|---|
| a | Client sẽ in thêm bài tập thứ 3 mà không cần sửa code. | Client hiển thị đủ 3 bài tập và vẫn nộp bài thành công. | Đúng | Client lấy danh sách từ API rồi lặp qua các phần tử; số bài tập không bị cố định trong code client. |
| b | Client sẽ báo lỗi kết nối vì API đã tắt. | Client gặp `HttpRequestException`; kết nối tới `localhost:5000` bị từ chối. | Đúng | Gọi qua mạng cần API đang chạy và có thể thất bại do máy chủ không khả dụng. Gọi hàm trong cùng chương trình không phụ thuộc vào kết nối mạng hay tiến trình máy chủ riêng. |
| c | API sẽ trả `400 Bad Request`; `EnsureSuccessStatusCode()` sẽ ném lỗi. | API trả `400`; client ném `HttpRequestException` tại dòng `EnsureSuccessStatusCode()` rồi dừng. | Đúng | Máy chủ kiểm tra dữ liệu vì client có thể gửi dữ liệu thiếu hoặc không hợp lệ; server phải tự bảo vệ quy tắc nghiệp vụ. |
| d | Client gọi cổng mặc định `5000` sẽ lỗi; truyền địa chỉ `5050` lúc chạy sẽ kết nối được mà không sửa code. | Client mặc định gặp lỗi kết nối ở `5000`. Chạy `dotnet run --project src/Submission.Client -- http://localhost:5050` thì client gọi API thành công. | Đúng | Client đọc địa chỉ từ `args[0]`, nên có thể đổi địa chỉ API bằng tham số dòng lệnh. |
## 6. Dự án EduSubmit tự làm (Bước 11-12)
- Ảnh chụp kết quả; phần nào phải xem code mẫu?
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/725d6b8c-6b03-46e3-ac31-43dabe5c13e2" />
Code mẫu: Không cần xem code mẫu để viết API và client. Source MiniDistributedSystem được mở sau đó để thực hiện các thí nghiệm riêng theo yêu cầu.

- Luồng thông điệp:

Client --(HTTP GET /api/assignments)--> API
Client <--(HTTP 200 + danh sách bài tập)-- API

Client --(HTTP POST /api/submissions + JSON)--> API
Client <--(HTTP 201 + Location + thông tin lượt nộp)-- API

Client --(HTTP GET /api/submissions/{id})--> API
Client <--(HTTP 200 + thông tin lượt nộp)-- API
 
## 7. Khó khăn gặp phải và cách giải quyết
Không có.
