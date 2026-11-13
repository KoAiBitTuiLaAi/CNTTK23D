# Bài 2: RESTful API cho tương tác Client-Server (EduSubmit)

- **Sinh viên:** Nguyễn Kim Thành Long
- **Mã sinh viên:** dtc245200458
- **Lớp:** CNTTK23D

## 1. Hướng dẫn biên dịch và chạy

### Bước 1: Biên dịch toàn bộ Solution

    dotnet build RestfulClientServer.slnx

### Bước 2: Khởi chạy máy chủ Submission.Api (cổng 5000)

    dotnet run --project src/Submission.Api --urls http://localhost:5000

### Bước 3: Chạy máy khách Submission.Client (ở terminal thứ hai)

    dotnet run --project src/Submission.Client -- http://localhost:5000

Kết quả đầu ra mẫu từ Submission.Client:

    Danh sach bai tap dang mo:
    - 1: Bai tap REST API
    - 2: Bai tap Client

    Da tao luot nop: <guid>
    Trang thai ban dau: received
    Location: /api/submissions/<guid>
    Doc lai luot nop: DTC245200458 -> https://github.com/dtc245200458/CNTTK23D

    Thu gui yeu cau loi: 400 BadRequest
    INVALID_SUBMISSION: Thieu ma sinh vien hoac kho ma nguon.

## 2. Bảng kết quả kiểm thử và phá thử (HTTP & Lỗi mạng)

| Tình huống kiểm thử | Yêu cầu gửi đi | Mã trạng thái | Kết quả / Thông điệp nhận được |
| :--- | :--- | :---: | :--- |
| Lấy danh sách bài tập | GET /api/assignments | 200 OK | Mảng JSON gồm các bài tập mẫu |
| Xem bài tập không tồn tại | GET /api/assignments/999 | 404 Not Found | ApiError: ASSIGNMENT_NOT_FOUND |
| Sai kiểu tham số tuyến | GET /api/assignments/abc | 404 Not Found | Body rỗng (Framework từ chối do :int) |
| Nộp bài hợp lệ | POST /api/assignments/1/submissions | 201 Created | Header Location: /api/submissions/{id} + JSON |
| Nộp bài thiếu dữ liệu | POST /api/assignments/1/submissions (rỗng) | 400 Bad Request | ApiError: INVALID_SUBMISSION |
| Nộp vào bài tập đã đóng | POST /api/assignments/3/submissions | 400 Bad Request | ApiError: ASSIGNMENT_CLOSED |
| Nộp trùng lần 2 | POST /api/assignments/1/submissions (trùng SV) | 409 Conflict | ApiError: DUPLICATE_SUBMISSION |
| Thiếu header Content-Type | POST không kèm application/json | 415 Unsupported Media Type | Framework từ chối trước khi vào handler |
| Sai phương thức HTTP | PUT /api/assignments/1 | 405 Method Not Allowed | Header Allow: GET |
| Xóa lượt nộp | DELETE /api/submissions/{id} | 204 No Content | Thành công, không trả về body |
| Tắt API hoặc sai cổng (5999) | Chạy Submission.Client khi API tắt | Lỗi mạng | HttpRequestException: Connection refused |
| Khởi động lại Submission.Api | Gọi lại GET /api/submissions/{id} cũ | 404 Not Found | Dữ liệu lưu trong RAM (List) mất khi dừng tiến trình |

## 3. Trả lời 3 câu hỏi giải thích

### Câu 1: Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?
- Trong kiến trúc REST, URI định danh **tài nguyên** (danh từ số nhiều như /api/assignments, /api/submissions), còn **phương thức HTTP** biểu diễn hành động chuẩn hóa (GET để đọc, POST để tạo mới, DELETE để xóa).
- Cách tách bạch này giúp giao diện API nhất quán, dễ đoán và tận dụng đúng ngữ nghĩa an toàn/lũy đẳng của HTTP. Ví dụ: dùng POST /api/assignments/1/submissions thay vì nhét động từ vào đường dẫn như /submitAssignment.

### Câu 2: "Stateless" thể hiện ở đâu trong code đã viết?
- Tính phi trạng thái (**Stateless**) thể hiện ở việc Submission.Api **không lưu phiên làm việc (session)** hay ghi nhớ ngữ cảnh của máy khách giữa các lần gọi.
- Mỗi yêu cầu tự mang đầy đủ thông tin để máy chủ xử lý độc lập: ví dụ POST /api/assignments/{assignmentId}/submissions chứa sẵn assignmentId trên URI cùng StudentCode và RepositoryUrl trong body JSON; yêu cầu đọc lại GET /api/submissions/{submissionId} truyền trực tiếp submissionId trên URI.

### Câu 3: Vì sao client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?
- Trong hệ phân tán, hai tiến trình Submission.Api và Submission.Client giao tiếp qua mạng bằng **hợp đồng dữ liệu JSON** chứ không dùng chung bộ nhớ hay tham chiếu mã nguồn của nhau.
- Miễn là tên thuộc tính và kiểu dữ liệu trong record ở hai bên tương thích theo quy ước JSON (camelCase trong ASP.NET Core / System.Text.Json), bộ tuần tự hóa sẽ tự động ánh xạ chính xác dữ liệu qua lại giữa hai chương trình.
