# EduSubmit (Bài 2, Phần 3): RestfulClientServer

Sinh viên: Dương Ngô Hùng, MSSV dtc245200279.

Hệ phân tán thu nhỏ gồm hai chương trình, hai tiến trình, nói chuyện qua một hợp đồng HTTP + JSON:

- `src/Submission.Api`: máy chủ Minimal API, cổng 5000, giữ bài tập và lượt nộp trong bộ nhớ.
- `src/Submission.Client`: máy khách console, chuỗi 4 nấc: liệt kê → nộp → đọc lại → cố ý gửi sai.

**Nhật ký (lệnh đã chạy, ảnh kết quả từng bước, bảng phá thử, thí nghiệm lỗi): [docs/bai2.md](docs/bai2.md)**

## Cách chạy

```powershell
# Terminal 1: máy chủ
dotnet run --project src/Submission.Api --urls http://localhost:5000

# Terminal 2: máy khách (sau dấu -- là địa chỉ API, vào args[0])
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Ca kiểm thử: `requests.http` (REST Client, 12 yêu cầu) và các tệp body cho `curl.exe -d "@tep.json"`: `sv001.json`, `rong.json`, `thieu-truong.json`, `hong.json`.

## Hợp đồng API

| Phương thức | URI | Thành công | Lỗi |
|---|---|---|---|
| GET | `/` | 200 `{service, status}` | – |
| GET | `/api/assignments` | 200 danh sách | – |
| GET | `/api/assignments/{assignmentId:int}` | 200 một bài tập | 404 `ASSIGNMENT_NOT_FOUND` |
| POST | `/api/assignments/{assignmentId:int}/submissions` | 201 + `Location` + `SubmissionDto` | 404 `ASSIGNMENT_NOT_FOUND` · 400 `ASSIGNMENT_CLOSED` · 400 `INVALID_SUBMISSION` |
| GET | `/api/submissions/{submissionId:guid}` | 200 lượt nộp | 404 `SUBMISSION_NOT_FOUND` |

Mọi lỗi do handler trả có cùng dạng `ApiError { code, message }`. Dữ liệu mẫu có 3 bài tập: 1 và 2 đang mở, 3 đã đóng (để thử `ASSIGNMENT_CLOSED`).

## Ba câu hỏi giải thích

**1. Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?**

URI là tên của tài nguyên (bài tập, lượt nộp), còn việc làm gì với tài nguyên đã có sẵn trong phương thức: GET đọc, POST tạo. Tách như vậy thì cùng một URI dùng được cho nhiều thao tác và máy khách đoán được cách dùng chỉ từ URI + phương thức + mã trạng thái. Ví dụ trong code: `POST /api/assignments/1/submissions` nghĩa là "tạo một lượt nộp thuộc bài tập 1", thay cho kiểu gọi hàm `POST /submitAssignment`; và `GET /api/submissions/{id}` đọc lại đúng lượt nộp đó.

**2. "Stateless" thể hiện ở đâu trong code đã viết?**

Mỗi yêu cầu tự mang đủ thông tin để xử lý: `assignmentId` nằm trên URI, `studentCode` và `repositoryUrl` nằm trong body (`CreateSubmissionRequest`). Handler không đọc biến phiên nào kiểu "bài tập đang chọn" từ yêu cầu trước. Máy khách đọc lại lượt nộp bằng đúng URI trong `Location`, không dựa vào việc máy chủ nhớ mình vừa nộp gì. Máy chủ vẫn lưu dữ liệu nghiệp vụ (`List<SubmissionDto>`), stateless chỉ có nghĩa là không nhớ cuộc hội thoại giữa hai yêu cầu.

**3. Vì sao client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?**

Hai bên là hai chương trình riêng, không tham chiếu project của nhau; thứ đi qua mạng là JSON chứ không phải đối tượng C#. `SubmissionDto` ở máy khách là bản sao của hợp đồng: chỉ cần tên trường và kiểu dữ liệu khớp với JSON (`id`, `assignmentId`, `studentCode`, ...; `System.Text.Json` so tên không phân biệt hoa thường) là đọc được. Trình biên dịch không kiểm tra được điều này: đặt sai tên trường thì vẫn build được nhưng giá trị nhận về là null.
