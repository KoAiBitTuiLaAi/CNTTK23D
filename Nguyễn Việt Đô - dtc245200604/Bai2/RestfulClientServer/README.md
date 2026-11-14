# EduSubmit – RESTful Client/Server

**Nguyễn Việt Đô · DTC245200604 · CNTTK23D**
Kho mã dùng trong bài nộp: https://github.com/vietdotest/EduSubmit-DTC245200604

## 1. Cấu trúc và chạy chương trình

Yêu cầu .NET SDK 10 và Python 3.10+ nếu chạy kiểm thử. Thực hiện các lệnh dưới đây tại thư mục `RestfulClientServer`.

```text
RestfulClientServer.slnx
src/Submission.Api/          # ASP.NET Core Minimal API
src/Submission.Client/       # Console + HttpClient
requests.http                # 7 ca cơ bản
requests-errors.http         # 8 ca 5a–5h
requests-extension.http      # 7 ca mở rộng, gồm 409 và DELETE
tests/                       # unittest, chỉ dùng thư viện chuẩn Python
```

Hai project `net10.0` có record DTO riêng, **không có ProjectReference**, không truy cập chung bộ nhớ. Không dùng database hoặc xác thực trong phạm vi bài.

```powershell
dotnet build RestfulClientServer.slnx
# Terminal 1: mặc định launch profile HTTP ở cổng 5000
dotnet run --project src/Submission.Api
# Hoặc chỉ định cổng rõ ràng, không dùng launch profile:
# dotnet run --project src/Submission.Api --no-launch-profile --urls http://localhost:5000
```

Terminal 2:

```powershell
curl.exe -i http://localhost:5000/
dotnet run --project src/Submission.Client
# Cùng API ở cổng khác:
# dotnet run --project src/Submission.Client -- http://localhost:5001/
```

`GET /` trả `200` và `{"service":"Submission.Api","status":"running"}`. API và Client là hai tiến trình riêng; dừng API bằng Ctrl+C tại terminal của API.

Client nhận URL gốc HTTP/HTTPS ở đối số đầu; mặc định `http://localhost:5000/`. URL không được kèm đường dẫn con, query, fragment hoặc thông tin đăng nhập. Khi đổi cổng API, phải đổi URL Client và biến `@api` trong các tệp `.http`.

## 2. Hợp đồng API

| Method và đường dẫn | Thành công | Lỗi |
|---|---|---|
| `GET /api/assignments` | 200, mảng `AssignmentDto` | — |
| `GET /api/assignments/{assignmentId:int}` | 200, `AssignmentDto` | 404 `ASSIGNMENT_NOT_FOUND` |
| `POST /api/assignments/{assignmentId:int}/submissions` | 201, `SubmissionDto`, header `Location` | 404 bài không có; 400 bài đóng/thiếu trường; 409 nộp trùng |
| `GET /api/submissions/{submissionId:guid}` | 200, `SubmissionDto` | 404 `SUBMISSION_NOT_FOUND` |
| `DELETE /api/submissions/{submissionId:guid}` | 204, **body rỗng** | 404 `SUBMISSION_NOT_FOUND` |

Không có route POST phẳng `/api/submissions` (gửi vào trả 404). Không yêu cầu query lọc: GET danh sách trả mọi bài, Client lọc `IsOpen`.

| Id | Title | IsOpen |
|---|---|---|
| 1 | Bai tap REST API | true |
| 2 | Bai tap Client | true |
| 3 | Bai tap da dong | false |

Các record (JSON dùng camelCase):

```csharp
record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string? StudentCode, string? RepositoryUrl);
record SubmissionDto(Guid Id, int AssignmentId, string StudentCode,
    string RepositoryUrl, string Status, DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
```

POST chỉ nhận hai trường nghiệp vụ `studentCode`, `repositoryUrl`:

```json
{"studentCode":"DTC245200604","repositoryUrl":"https://github.com/vietdotest/EduSubmit-DTC245200604"}
```

Máy chủ `Trim()` hai trường, tự sinh GUID, lấy AssignmentId từ route, đặt `Status = "received"` và `ReceivedAt = DateTimeOffset.UtcNow`. Client không được quyết định ID, trạng thái hay thời gian. Các trường JSON không thuộc DTO bị bỏ qua bởi bộ đọc JSON mặc định; test xác nhận không thể dùng chúng ghi đè dữ liệu máy chủ.

Với JSON đọc được thành DTO, thứ tự kiểm tra trong handler:

| Ưu tiên | Điều kiện | HTTP / ApiError | Message |
|---|---|---|---|
| 1 | Không tìm thấy bài | 404 `ASSIGNMENT_NOT_FOUND` | `Khong tim thay bai tap.` |
| 2 | Bài đã đóng | 400 `ASSIGNMENT_CLOSED` | `Bai tap da dong.` |
| 3 | Một trường null, thiếu, rỗng hoặc chỉ khoảng trắng | 400 `INVALID_SUBMISSION` | `Thieu ma sinh vien hoac kho ma nguon.` |
| 4 | Cùng bài và MSSV sau trim, không phân biệt hoa/thường | 409 `DUPLICATE_SUBMISSION` | `Sinh vien da nop bai tap nay.` |

Không tìm thấy bài nộp trả 404 `SUBMISSION_NOT_FOUND`, message `Khong tim thay bai nop.`. Một MSSV vẫn được nộp cho bài khác. Kiểm tra trùng và thêm bài nằm trong cùng `lock`; mọi đọc/ghi danh sách bài và dictionary bài nộp đều khóa cùng đối tượng. DELETE bỏ bản ghi nên cho phép nộp lại với GUID mới. Khởi động lại server xóa toàn bộ bài nộp trong bộ nhớ; ba bài tập mẫu được tạo lại.

**Lỗi framework khác lỗi nghiệp vụ:** JSON hỏng, sai kiểu trường hoặc body JSON `null` trả 400 trước handler; sai Content-Type trả 415. `/api/assignments/abc` không khớp `{assignmentId:int}`, trả 404 body rỗng. PUT vào `/api/assignments/1` trả 405, `Allow: GET`, body rỗng. Không ép các phản hồi framework có dạng `ApiError`.

## 3. Client bốn bước

1. GET assignments, kiểm tra 200, đọc mảng rồi chỉ in hai bài đang mở.
2. POST tới bài mở đầu tiên với MSSV của Đô; **kiểm tra 201 trước khi deserialize SubmissionDto**; in GUID và Location.
3. GET **chính URI trong header Location**, kiểm tra 200 và dữ liệu bằng bài vừa tạo.
4. Cố ý POST hai trường rỗng; nhận 400 và in `INVALID_SUBMISSION` cùng message.

Kết quả thực chạy lần đầu trên cổng 5000: `2. POST: 201` → `3. GET Location: 200` → `4. POST rong: 400`; exit **0**. Lần thứ hai trên cùng server: POST **409**, in `DUPLICATE_SUBMISSION: Sinh vien da nop bai tap nay.`, exit **1**, không crash. Lần hai dừng ở bước POST, không đọc ApiError như SubmissionDto. Khởi động lại API hoặc DELETE bài theo Location đã in để chạy lại đủ bốn bước.

Mã thoát: **0** kịch bản thành công (bao gồm 400 cố ý); **1** trạng thái HTTP/kịch bản không mong đợi, gồm 409; **2** BaseAddress/đối số sai; **3** kết nối hoặc timeout 10 giây; **4** JSON phản hồi sai. Lỗi HTTP có body rỗng, HTML hoặc JSON không đúng ApiError được in an toàn.

## 4. Chạy các tệp request

Mở `.http` bằng extension REST Client trong VS Code, gửi tuần tự từ trên xuống. `# @name createSubmission` cho phép request GET dùng `{{createSubmission.response.body.$.id}}`; extension tương tự với `createExtension` và `resubmitExtension`. Phải gửi POST được đặt tên trước khi dùng biến phản hồi.

| Tệp | Số ca | Chuỗi HTTP theo thứ tự |
|---|---:|---|
| `requests.http` | 7 | 200, 200, 404, 201, 200, 400, 404 |
| `requests-errors.http` | 8 | 400, 400, 400, 400, 415, 405, 404, 404 |
| `requests-extension.http` | 7 | 400, 201, 409, 204, 404, 201, 200 |

Ca mở rộng dùng SV002: đóng bài → nộp → nộp trùng khác hoa/thường → xóa → xóa lại → nộp lại → đọc bài mới. Ca core dùng CORE-SV001, Client dùng DTC245200604, không gây trùng chéo. Khởi động lại API trước khi chạy lại **toàn bộ** tệp core hoặc extension; nộp lại ca tạo sau lần chạy trước sẽ đúng quy tắc trả 409, không còn 201.

Đã phát lại nội dung thực của cả ba tệp qua HTTP bằng Python `urllib` trên `localhost:5000`: **22/22 đúng trạng thái**, kiểm tra GET theo Location, named ID thật, DELETE body rỗng và GUID nộp lại khác GUID cũ. Đây là kiểm thử HTTP tự động, **chưa phải xác minh thao tác Send Request qua GUI REST Client**.

## 5. Kiểm thử và bảng thí nghiệm

```powershell
dotnet build RestfulClientServer.slnx
python -m unittest discover -s tests -v
```

Test tự khởi động API thật trên cổng trống và dừng các tiến trình do nó tạo trong cleanup; không cần mở API thủ công. Không chạy đồng thời nhiều bộ test cùng thư mục log. Để phát lại `.http` ở đúng cổng 5000 bằng PowerShell, bảo đảm không có API khác đang chiếm cổng:

```powershell
$env:EDU_HTTP_PORT = "5000"
python -m unittest discover -s tests -v
Remove-Item Env:EDU_HTTP_PORT
```

`EDU_EVIDENCE_DIR` tùy chọn chỉ thư mục log ngoài mã nguồn. Mặc định log nằm trong thư mục tạm của người chạy. Không đưa log/cache/bin/obj vào bài nộp; `.gitignore` theo mẫu chuẩn .NET.

Kết quả thực chạy: SDK **10.0.403**, build solution **0 warnings, 0 errors**; **16/16 test methods đạt**. Test có các subcase về validation, JSON lỗi và Client; số method không phải tổng HTTP request. Bao phủ timestamp UTC, DTO hợp lệ/không hợp lệ, thứ tự kiểm tra, GET Location, trim, trùng theo bài, 24 POST đồng thời, DELETE/nộp lại, mất dữ liệu sau restart, Client chạy hai lần, URL sai, mất kết nối, timeout và body lỗi rỗng/không JSON. Các lỗi transport đặc biệt của Client dùng HTTP server Python cục bộ làm fixture riêng, không giả nhận đó là response của Submission.Api.

| Thí nghiệm | Dự đoán | Kết quả thực tế | Đối chiếu / giải thích |
|---|---|---|---|
| Bài 999 + trường rỗng | 404 bài không có | 404 `ASSIGNMENT_NOT_FOUND` | Đúng; kiểm tra tồn tại trước trường |
| Bài 3 + trường rỗng | 400 bài đã đóng | 400 `ASSIGNMENT_CLOSED` | Đúng; trạng thái bài trước trường |
| Trường rỗng ở bài 1 | 400 | 400 `INVALID_SUBMISSION` | Đúng; không lưu bài |
| JSON hỏng / text/plain | 400 / 415 | 400 / 415 | Đúng; binding/content negotiation trước handler |
| Cùng MSSV khác hoa/thường | 409 | 409 `DUPLICATE_SUBMISSION` | Đúng; trim + OrdinalIgnoreCase |
| 24 POST đồng thời cùng cặp | 1 lần 201, 23 lần 409 | 1 lần 201, 23 lần 409 | Đúng; kiểm tra và thêm trong cùng lock |
| DELETE rồi DELETE lại | 204 rỗng, 404 | 204 rỗng, 404 | Đúng; ID cũ không còn |
| Nộp lại sau xóa | 201, ID mới | 201, GUID mới | Đúng; cặp trùng đã được giải phóng |
| Khởi động lại API | Bài cũ mất | GET ID cũ 404, còn 3 bài tập | Đúng; dữ liệu chỉ trong RAM |
| Client lần 1 / lần 2 | exit 0 / exit 1 | 201→200→400 / 409 | Đúng; lần hai báo trùng an toàn |
| API không chạy / trả quá chậm | Lỗi kết nối / timeout | exit 3, thông báo rõ | Đúng; không có HTTP response khi mất kết nối |

## 6. Ba câu hỏi theo đề

**1. Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?** URI `/api/assignments/1/submissions` xác định tập lượt nộp thuộc bài tập 1. POST tạo một lượt nộp, GET đọc tài nguyên, DELETE xóa; không cần đặt động từ như `/submitAssignment` trong URI.

**2. Stateless thể hiện ở đâu trong code?** Mỗi POST tự mang `assignmentId` trên route, `studentCode` và `repositoryUrl` trong body. API không cần nhớ Client đã chọn bài ở một request trước đó. Dictionary lưu lượt nộp là trạng thái tài nguyên, không phải phiên hội thoại; stateless không có nghĩa server không lưu dữ liệu.

**3. Vì sao Client và server khai báo DTO riêng mà vẫn khớp nhau?** Hai tiến trình tuân theo cùng hợp đồng HTTP/JSON: tên trường camelCase và kiểu dữ liệu tương thích. Mỗi bên deserialize JSON thành record của mình, không cần ProjectReference hay dùng chung biến. `AssignmentDto` có `id`, `title`, `description`, `isOpen`; request nộp bài chỉ có hai trường do người nộp nhập, còn GUID, trạng thái và thời gian do server sinh.

## Minh chứng

Ảnh API/Client và REST Client chưa bổ sung. Kết quả HTTP tự động ở trên không thay thế kiểm chứng thao tác GUI.
