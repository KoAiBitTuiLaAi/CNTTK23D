# Bài 2 – RESTful API cho tương tác client–server

Họ tên: Nguyễn Việt Đô

MSSV: DTC245200604

Lớp: CNTT K23D

## Các phần thực hành

| Phần | Solution và thư mục | Cổng API | Nhật ký và cách chạy |
|---|---|---:|---|
| Catalog | `catalogapi/catalogapi.slnx` | 5100 | [Catalog](catalogapi/README.md) |
| EduSubmit | `RestfulClientServer/RestfulClientServer.slnx` | 5000 | [EduSubmit](RestfulClientServer/README.md) |
| Đổi miền quản lý sách | `LibraryApi/LibraryApi.slnx` | 5200 | [LibraryApi và bảng hợp đồng](LibraryApi/README.md) |

Mỗi solution gồm API và Client độc lập, giao tiếp qua HTTP/JSON, không tham chiếu project của nhau. Chạy API trước, giữ terminal đó mở rồi chạy Client ở terminal thứ hai. Các lệnh dưới đây thực hiện tại thư mục `Bai2`:

```powershell
dotnet build catalogapi/catalogapi.slnx
dotnet build RestfulClientServer/RestfulClientServer.slnx
dotnet build LibraryApi/LibraryApi.slnx
```

Ví dụ chạy EduSubmit:

```powershell
# Terminal 1
dotnet run --project RestfulClientServer/src/Submission.Api --urls http://localhost:5000
# Terminal 2
dotnet run --project RestfulClientServer/src/Submission.Client -- http://localhost:5000
```

## Kết quả và bảng phá thử

- Catalog: Client đủ bốn nấc, bốn thí nghiệm; lọc từ khóa, DELETE 204/404, tên quá 100 ký tự trả 400. [Bảng kết quả](catalogapi/README.md#8-bốn-thí-nghiệm).
- EduSubmit: API nhận nộp bài tại `/api/assignments/{assignmentId}/submissions`, kiểm tra tồn tại → còn mở → hợp lệ → trùng; Client đọc chính `Location`; mở rộng 409 và DELETE 204. [Bảng kiểm thử](RestfulClientServer/README.md#5-kiểm-thử-và-bảng-thí-nghiệm).
- LibraryApi: năm câu hỏi thiết kế, bảng hợp đồng, Client bốn nấc và sáu request bao phủ 200/201/400/404/204/415. [Sáu ca thực chạy](LibraryApi/README.md#4-sáu-request-và-kết-quả-thực-chạy).

Các tệp `.http` được phát lại bằng HTTP thật trong kiểm thử tự động. Ảnh toàn màn hình và thao tác Send Request qua GUI REST Client chưa bổ sung; không coi log kiểm thử là ảnh minh chứng.

## Ba câu hỏi theo đề

### 1. Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?

URI định danh tài nguyên. `/api/assignments/1/submissions` là tập lượt nộp của bài tập 1; POST tạo lượt nộp, GET đọc và DELETE xóa. Cách này không cần động từ như `/doSubmit` trong URI, dùng đúng ý nghĩa HTTP.

### 2. Stateless thể hiện ở đâu trong code đã viết?

Một yêu cầu nộp bài có đủ `assignmentId` trên URI và `studentCode`, `repositoryUrl` trong body; API không phải nhớ request trước đã chọn bài nào. RAM lưu tài nguyên không phải phiên hội thoại. Dữ liệu mất khi khởi động lại do chưa có lưu trữ bền vững, không phải vì stateless cấm server lưu dữ liệu.

### 3. Vì sao Client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?

Hai bên thống nhất tên trường, kiểu dữ liệu và JSON camelCase. Mỗi tiến trình deserialize thông điệp thành record riêng của mình, không dùng chung biến hoặc assembly. Ví dụ `AssignmentDto` có `id`, `title`, `description`, `isOpen`; response nộp bài có GUID, trạng thái và thời gian do server sinh.
