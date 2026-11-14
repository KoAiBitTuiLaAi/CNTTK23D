# Bài 1 – Tổng quan hệ thống và MEP
Họ tên: Tạ Tuấn Anh
MSSV: DTC245200697
Lớp: CNTT K23D
 
## 1. Cài đặt
<img width="1680" height="940" alt="Screenshot 2026-11-06 090023" src="https://github.com/user-attachments/assets/92a48b72-6937-4d17-8541-05ea68274fa6" />

 
## 2. MepDemo (Bước 7)
Câu 1: Ở MEP 3, mỗi bài nộp sub-x được mấy worker xử lý?

Trả lời: Mỗi bài nộp sub-x chỉ được một worker xử lý và chấm điểm. Các worker sẽ chia nhau xử lý những bài nộp khác nhau.

Câu 2: Ở MEP 4, một sự kiện được gửi tới mấy dịch vụ?

Trả lời: Một sự kiện được gửi tới cả 3 dịch vụ. Mỗi dịch vụ đều nhận được sự kiện để xử lý độc lập.

Câu 3: Viết một câu mô tả cho mỗi MEP.
- MEP 1 – Request–Response: Máy khách gửi yêu cầu đến máy chủ và chờ nhận kết quả phản hồi.
- MEP 2 – One-way: Máy gửi thông điệp đến máy nhận mà không cần chờ phản hồi.
- MEP 3 – Queue-based: Thông điệp được đưa vào hàng đợi và các worker lần lượt lấy thông điệp để xử lý.
- MEP 4 – Publish–Subscribe: Một sự kiện được phát đi và nhiều dịch vụ cùng đăng ký sẽ nhận được sự kiện đó.
- MEP 5 – Streaming: Dữ liệu được truyền liên tục theo luồng thay vì gửi từng yêu cầu riêng lẻ.
 
## 3. BasicWebApi (Bước 8)
<img width="1370" height="757" alt="Screenshot 2026-11-06 103748" src="https://github.com/user-attachments/assets/807a5396-67e7-4cb1-b50b-b5915f11c2c1" />
<img width="992" height="737" alt="Screenshot 2026-11-06 103835" src="https://github.com/user-attachments/assets/2f6f5f7a-e416-4e4d-8963-cb4414410c86" />
<img width="975" height="776" alt="Screenshot 2026-11-06 103855" src="https://github.com/user-attachments/assets/622d16f2-cee0-425c-932c-7cb97b7eb096" />

Câu 1. Hai dòng builder.Services.AddSingleton(...) và var app = builder.Build(); – dòng nào phải đứng trước? Vì sao?
Trả lời:
Dòng builder.Services.AddSingleton(...) phải đứng trước dòng var app = builder.Build();.
Vì AddSingleton() dùng để đăng ký dịch vụ vào hệ thống Dependency Injection (DI). Sau khi gọi builder.Build(), ứng dụng được xây dựng từ các dịch vụ đã đăng ký. Vì vậy, cần đăng ký dịch vụ trước khi xây dựng ứng dụng để hệ thống có thể sử dụng dịch vụ đó.

Câu 2. Trong /hello/{name}?language=en, tham số name lấy từ đâu, language lấy từ đâu?
Trả lời:
- name được lấy từ Route Parameter (tham số trên đường dẫn URL). Ví dụ, với /hello/An, giá trị name là An.
- language được lấy từ Query Parameter (tham số truy vấn sau dấu ?). Ví dụ, với ?language=en, giá trị language là en.

Câu 3. Vì sao POST hợp lệ trả Results.Created (201) chứ không phải Results.Ok (200)?
Trả lời:
Vì request POST hợp lệ dùng để tạo một lời chào mới trong hệ thống. Mã trạng thái 201 Created cho biết tài nguyên mới đã được tạo thành công.
Ngoài ra, Results.Created() còn có thể trả về header Location chứa đường dẫn đến tài nguyên vừa tạo.
Trong khi đó, Results.Ok (200) chỉ thông báo yêu cầu đã được xử lý thành công, không thể hiện rõ ý nghĩa tạo mới tài nguyên.
 
## 4. MiniDistributedSystem (Bước 9)
<img width="1352" height="911" alt="Screenshot 2026-11-06 104403" src="https://github.com/user-attachments/assets/83836c37-1868-46d2-84a0-f0bf76259aea" />
<img width="1356" height="917" alt="Screenshot 2026-11-06 104423" src="https://github.com/user-attachments/assets/0ea6a0b3-584a-4fe4-bd1d-02bf2eee07fe" />

Câu 1. Client gọi những endpoint nào của API? Mỗi lần gọi là một cặp request–response gồm method, URL, mã trạng thái gì?
Trả lời:
Client giao tiếp với máy chủ Submission.Api thông qua các endpoint HTTP. Theo luồng thực hành, client thực hiện hai thao tác chính:
- GET /api/assignments: Gửi yêu cầu lấy danh sách bài tập từ máy chủ. Nếu thành công, máy chủ trả về 200 OK cùng dữ liệu danh sách bài tập.
- POST /api/submissions: Gửi yêu cầu nộp bài tập lên máy chủ. Nếu tạo lượt nộp thành công, máy chủ trả về 201 Created, kèm mã lượt nộp (GUID) và trạng thái received.
Mỗi lần client gửi request, máy chủ tiếp nhận, xử lý và gửi response tương ứng về cho client.

Câu 2. Client có tham chiếu (dùng chung) code của API không? Hai bên "thống nhất" với nhau bằng gì?
Trả lời:
Client không cần tham chiếu hoặc sử dụng trực tiếp code xử lý nội bộ của API. Hai chương trình hoạt động độc lập và giao tiếp với nhau thông qua giao thức HTTP.
Hai bên thống nhất với nhau bằng API Contract (hợp đồng API), bao gồm:
- Địa chỉ endpoint (URL).
- Phương thức HTTP như GET, POST.
- Cấu trúc dữ liệu gửi đi và nhận về (JSON).
- Quy tắc xử lý và mã trạng thái HTTP.
Nhờ có hợp đồng API, client có thể gửi yêu cầu và hiểu được phản hồi từ máy chủ mà không cần biết cách máy chủ triển khai bên trong.

Câu 3. Vì sao địa chỉ API được truyền qua tham số -- http://localhost:5000 mà không viết cứng trong code?
Trả lời:
Địa chỉ API được truyền qua tham số dòng lệnh để chương trình có thể linh hoạt thay đổi địa chỉ máy chủ mà không cần sửa mã nguồn.
Ví dụ:
- Khi chạy trên máy cá nhân: http://localhost:5000.
- Khi chạy trên máy chủ khác: có thể sử dụng địa chỉ IP hoặc tên miền tương ứng.
Cách làm này giúp chương trình dễ cấu hình, tái sử dụng và triển khai trong nhiều môi trường khác nhau như môi trường phát triển, kiểm thử và thực tế.


## 5. Thí nghiệm lỗi (Bước 10)
| # | Dự đoán | Thực tế | Đúng/Sai | Giải thích |
|---|---|---|:---:|---|
| **a** | Client không cần sửa code, vẫn lấy được và hiển thị đầy đủ cả 3 bài tập. | Client in ra màn hình đủ 3 bài tập (`- 1: Bai tap 1`, `- 2: Bai tap 2`, `- 3: Bai tap 3`) mà không cần chỉnh sửa hay build lại mã nguồn. | **Đúng** | **Client không cần sửa code**. Hai bên giao tiếp lỏng (*loose coupling*) qua giao thức HTTP và định dạng JSON; Client deserialize động theo kiểu `List<Assignment>` và duyệt danh sách theo vòng lặp `foreach`, nên khi máy chủ tăng/giảm số lượng dữ liệu thì client tự thích ứng mà không bị ảnh hưởng. |
| **b** | Client sẽ báo lỗi kết nối mạng (`HttpRequestException` / Connection Refused) do máy chủ không phản hồi. | Client ném ngoại lệ `System.Net.Http.HttpRequestException: No connection could be made because the target machine actively refused it (localhost:5000)`. | **Đúng** | **Lỗi kết nối (`Connection Refused`)**. Gọi qua mạng có thể thất bại vì phụ thuộc vào hạ tầng mạng, trạng thái cổng, tiến trình đích còn chạy hay không (đặc trưng của hệ thống phân tán: mạng không tin cậy, lỗi từng phần). Trong khi đó, gọi hàm trong cùng chương trình diễn ra trên cùng không gian bộ nhớ (*in-process memory*), không qua I/O mạng nên luôn đảm bảo gọi được. |
| **c** | API sẽ từ chối và trả về mã lỗi `400 Bad Request`. Client sẽ dừng lại và báo lỗi tại dòng `EnsureSuccessStatusCode`. | API trả về mã `400 Bad Request` kèm nội dung `{"message":"Du lieu nop bai khong hop le"}`. Client ném ra ngoại lệ `HttpRequestException: Response status code does not indicate success: 400 (Bad Request)`. | **Đúng** | - **Mã trạng thái**: `400 Bad Request`.<br>- **Tác dụng của `EnsureSuccessStatusCode()`**: Kiểm tra nếu HTTP response không nằm trong khoảng thành công (200–299) thì lập tức quăng ngoại lệ `HttpRequestException`, ngăn client tiếp tục xử lý với dữ liệu lỗi/không hợp lệ.<br>- **Vì sao máy chủ phải tự kiểm tra dữ liệu**: Nguyên tắc *"Never trust the client"*. Người dùng hoặc kẻ tấn công có thể bypass client (dùng curl, Postman, chỉnh sửa code client) để gửi dữ liệu rác hoặc độc hại. Server là chốt chặn cuối cùng bảo vệ tính toàn vẹn và an toàn của hệ thống. |
| **d** | Ban đầu Client gọi vào cổng 5000 bị lỗi kết nối. Có thể sửa bằng cách truyền URL cổng mới qua tham số dòng lệnh (CLI argument) khi chạy client. | Chạy lệnh `dotnet run --project src/Submission.Client -- http://localhost:5050`, Client kết nối thành công tới API cổng 5050 mà không cần sửa một dòng code nào. | **Đúng** | **Cách sửa**: Chạy lệnh truyền URL mới qua tham số: `dotnet run --project src/Submission.Client -- http://localhost:5050`.<br>Trong mã nguồn Client đã được thiết kế sẵn khả năng đọc cấu hình từ đối số dòng lệnh: `var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";`, giúp hệ thống linh hoạt thay đổi môi trường triển khai mà không cần sửa code. |
 
## 6. Dự án EduSubmit tự làm (Bước 11-12)
Bước 11:
11.1 – Khởi tạo Solution và hai Project.
 <img width="1577" height="1007" alt="image" src="https://github.com/user-attachments/assets/7fd82ab4-91f8-4670-8278-fd7961d654a1" />

11.2 – Xây dựng và kiểm tra 3 API.
<img width="1736" height="920" alt="image" src="https://github.com/user-attachments/assets/a4acdccb-ce4f-4059-a71c-8b8ba203ba93" />

Kết quả kiểm tra lần lượt cả 3 API:

<img width="550" height="192" alt="image" src="https://github.com/user-attachments/assets/cf2e1ec8-633e-4985-9c85-c174b47e0987" />

<img width="1322" height="142" alt="image" src="https://github.com/user-attachments/assets/87612370-62d0-4caa-866d-cb65ad24b15a" />

<img width="1515" height="921" alt="image" src="https://github.com/user-attachments/assets/e7872518-c626-4070-9da0-70bb0876a2fc" />

11.3 – Xây dựng Submission.Client.

<img width="1507" height="932" alt="image" src="https://github.com/user-attachments/assets/2aa4045d-f5f4-4a6b-afab-87466b118e77" />
<img width="1506" height="948" alt="image" src="https://github.com/user-attachments/assets/2a4baa60-aae1-4c3e-8494-81b725fcbdd4" />

Bước 12:

Code Submission.Api
<img width="1345" height="941" alt="image" src="https://github.com/user-attachments/assets/a6c46ce8-a71a-4766-bb52-7e8f4f55cfbb" />

Client: POST rồi GET đọc lại lượt nộp
<img width="1343" height="947" alt="image" src="https://github.com/user-attachments/assets/6e6a5e04-9bc1-48a9-965c-26dbc569a63d" />

Kết quả requests.http
<img width="1616" height="942" alt="image" src="https://github.com/user-attachments/assets/57e788a1-e52e-4a16-b455-ebd315d0ceea" />

Kết quả POST 201 Created
<img width="1622" height="925" alt="image" src="https://github.com/user-attachments/assets/9a78c799-f164-4b58-afc5-2c6e24d705c8" />

Kết quả GET 400 Not Found 
<img width="1613" height="927" alt="image" src="https://github.com/user-attachments/assets/e199fa6d-dfda-4378-a1b9-f0a981fa5e28" />

## 7. Khó khăn gặp phải và cách giải quyết
