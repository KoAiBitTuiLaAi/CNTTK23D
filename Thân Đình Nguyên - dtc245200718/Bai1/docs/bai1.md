# Bài 1 – Tổng quan hệ thống và MEP
Họ tên: Thân Đình Nguyên     MSSV: DTC245200718     Lớp: CNTT K23D

## 1. Cài đặt

Sử dụng .NET 10 SDK, Visual Studio Code, Git và các extension C# Dev Kit, REST Client.

Kiểm tra bằng dotnet --version, git --version và chạy chương trình HelloPtudpt.
![Kết quả kiểm tra môi trường](cai-dat.png)

## 2. MepDemo (Bước 7)

1. Ở MEP 3, mỗi bài nộp sub-x được một worker xử lý trong kịch bản chạy bình thường. Các worker chia nhau công việc trong hàng đợi.
2. Ở MEP 4, một sự kiện được gửi tới cả ba dịch vụ đăng ký nhận sự kiện theo kịch bản trong tài liệu.
3. Mô tả năm MEP:
   - Request–response: bên gửi đưa ra yêu cầu và nhận phản hồi tương ứng từ bên xử lý.
   - One-way: bên gửi gửi thông điệp mà không yêu cầu phản hồi ở mức nghiệp vụ.
   - Queue-based: thông điệp được đưa vào hàng đợi để các worker nhận và xử lý.
   - Publish–subscribe: một sự kiện được phân phối tới các subscriber đăng ký nhận sự kiện đó.
   - Streaming: dữ liệu được truyền dưới dạng một luồng gồm nhiều phần tử theo thời gian.
   ![Hàng đợi chia việc cho hai worker](mep-queue.png)
   ![Một sự kiện gửi tới ba dịch vụ](mep-pubsub.png)

## 3. BasicWebApi (Bước 8)

Đã chạy BasicWebApi, kiểm thử các trường hợp 200, 201, 400 và bổ sung ngôn ngữ tiếng Nhật..

1. builder.Services.AddSingleton(...) phải đứng trước builder.Build() để đăng ký dịch vụ trước khi xây dựng ứng dụng.
2. Trong /hello/An?language=en, name lấy từ route và có giá trị An; language lấy từ query string và có giá trị en.
3. POST hợp lệ tạo tài nguyên mới nên trả 201 Created. Header Location chỉ địa chỉ của tài nguyên vừa tạo; 200 OK chỉ thể hiện thành công nói chung.
![BasicWebApi trả 200](api-200-1.png)
![Tạo lời chào trả 201](api-201-1.png)
![Tên rỗng trả 400](api-400-1.png)
![Chào tiếng Nhật](basic-ja.png)
## 4. MiniDistributedSystem (Bước 9)

Đã chạy MiniDistributedSystem gồm hai tiến trình API và Client. Client lấy được danh sách bài tập và nộp bài thành công, nhận mã GUID cùng trạng thái received.. Ảnh API và Client của dự án EduSubmit tự tạo được đặt ở mục 6.

1. Theo đặc tả, Client gọi GET /api/assignments để nhận danh sách bài tập với mã 200 và POST /api/submissions để nộp bài. POST hợp lệ nhận 201, dữ liệu không hợp lệ nhận 400. Mỗi lần gọi gồm một HTTP request và một HTTP response.
2. Client không tham chiếu trực tiếp code của API. Hai bên thống nhất bằng HTTP method, URL, cấu trúc JSON và mã trạng thái phản hồi.
3. Địa chỉ API được truyền bằng tham số để có thể thay đổi host hoặc cổng khi chạy mà không cần sửa và biên dịch lại Client.
![API mẫu đang chạy](mini-api.png)
![Client mẫu gọi API thành công](mini-client.png)
## 5. Thí nghiệm lỗi (Bước 10)

| # | Dự đoán | Thực tế | Đúng/Sai | Giải thích |
|---|---|---|---|---|
| a |Client sẽ hiển thị ba bài tập mà không cần sửa code vì đọc danh sách động từ API|Client hiển thị đủ ba bài tập, không cần sửa code|Đúng| Client đọc danh sách động và duyệt từng phần tử; cấu trúc JSON không đổi|
| b |Tắt API rồi chạy Client sẽ gặp lỗi kết nối|Client lỗi kết nối khi API dừng|Đúng| API là tiến trình độc lập; khi dừng thì không tiếp nhận kết nối. Gọi hàm nội bộ không có phụ thuộc kết nối mạng này nhưng vẫn có thể gặp lỗi khác|
| c | StudentCode rỗng khiến API trả 400; EnsureSuccessStatusCode() phát sinh ngoại lệ|API trả 400; Client phát sinh HttpRequestException tại EnsureSuccessStatusCode()|Đúng| API phải tự kiểm tra dữ liệu vì Client có thể gửi yêu cầu sai. EnsureSuccessStatusCode() phát sinh ngoại lệ khi response không thuộc nhóm 200–299. |
| d |API chạy ở 5050 nhưng Client gọi 5000 sẽ lỗi kết nối|Gọi cổng 5000 bị từ chối kết nối; truyền cổng 5050 thì Client chạy thành công|Đúng|Truyền địa chỉ đúng qua tham số dòng lệnh, không cần sửa code để đổi cổng|
![Thí nghiệm a](thi-nghiem-a.png)
![Thí nghiệm b](thi-nghiem-b.png)
![Thí nghiệm c](thi-nghiem-c.png)
![Thí nghiệm d lỗi](thi-nghiem-d-loi.png)     
![Thí nghiệm d đúng](thi-nghiem-d-dung.png)
## 6. Dự án EduSubmit tự làm (Bước 11-12)

Đã tạo Submission.Api và Submission.Client chạy ở hai tiến trình độc lập. API cung cấp danh sách hai bài tập; Client lấy danh sách, gửi bài nộp và nhận GUID cùng trạng thái received.

Đã gửi các request kiểm thử GET, POST hợp lệ và POST không hợp lệ bằng REST Client. Đã hoàn thành bài mở rộng: Client đọc lại đúng lượt nộp vừa tạo; API trả 200 khi GUID tồn tại và 404 khi không tìm thấy.

Em tham khảo khung code trong tài liệu và sử dụng AI hỗ trợ viết, giải thích code API, Client, requests.http và xử lý lỗi khi chạy.
![GET trả 200](api-200.png)
![POST hợp lệ trả 201](api-201.png)
![POST không hợp lệ trả 400](api-400.png) 
![Kết quả API và Client](<API Client.png>)      

Luồng thông điệp:

Client --(HTTP GET /api/assignments)--> API: request–response.
API trả 200 và danh sách bài tập dạng JSON.

Client --(HTTP POST /api/submissions)--> API: request–response.
Client gửi assignmentId, studentCode và repositoryUrl.
API trả 201 cùng thông tin lượt nộp nếu hợp lệ, hoặc 400 nếu dữ liệu sai.

Luồng bổ sung ở Bước 12:
Client --(HTTP GET /api/submissions/{id})--> API: request–response.
API trả 200 nếu tìm thấy lượt nộp, hoặc 404 nếu không tìm thấy.
![Đọc lượt nộp thành công 200](<Ảnh 200 OK.png>)
![Lượt nộp không tồn tại 404](<Ảnh 404 Not Found.png>)
## 7. Khó khăn gặp phải và cách giải quyết

Không có file requests.http mẫu nên em tạo file trong thư mục gốc dự án và viết các request theo đặc tả.

Khi chạy lại API trong lúc bản cũ vẫn hoạt động, chương trình báo file Submission.Api.exe bị khóa. Em kiểm tra terminal, dừng bản cũ bằng Ctrl + C trước khi chạy lại.

Ban đầu em nhầm terminal chạy API và Client. Em sử dụng hai terminal riêng: một terminal giữ API hoạt động, terminal còn lại chạy Client.