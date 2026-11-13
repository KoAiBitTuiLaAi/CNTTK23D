# Bài 1 Tổng quan hệ thống và MEP

Họ tên: **Phan Đức Anh**

MSSV: **DTC245200318**

Lớp: **CNTT K23D**

Ngày thực hành: **06/11/2026**

Bài thực hành xây dựng hệ thống EduSubmit gồm API và Client độc lập, giao tiếp
bằng HTTP/JSON. Phần mở rộng lưu lượt nộp trong bộ nhớ và đọc lại bằng GUID đã
được thực hiện. Kết quả dưới đây lấy từ các lần chạy thật trên máy.

Việc tạo mã, chạy thí nghiệm và tổng hợp nhật ký có sự hỗ trợ của Codex. Không
khẳng định các bước đã được sinh viên tự gõ hoặc tự thao tác. MepDemo dùng mã
được cung cấp; BasicWebApi và MiniDistributedSystem được dựng lại từ đặc tả vì
đầu vào không có hai thư mục mẫu đó.

## 1 Cài đặt

- .NET SDK: **10.0.403**; các project đích `net10.0`.
- Git: **2.56.0.windows.4**.
- Đã tạo và chạy project console `HelloPtudpt`, kết quả `Hello, World!`.
- VS Code có C# Dev Kit, C# 2.160.4, .NET Install Tool và REST Client.
- Code được làm trong bản riêng, không sửa dữ liệu nguồn trong OneDrive.

Bằng chứng: [log kiểm thử](evidence/verification.txt),
[Hello World](evidence/environment.txt), [build solution](evidence/EduSubmit-slnx-build.txt).

Kết quả kiểm tra môi trường đã chạy:

```text
SDK: 10.0.403
Git: git version 2.56.0.windows.4
HelloPtudpt:
Hello, World!
Build succeeded.
0 Warning(s)
0 Error(s)
```

## 2 MepDemo Bước 7

Đã chạy cả 5 MEP và chạy riêng các tham số 1, 3, 4.

1. **MEP 3:** mỗi `sub-x` chỉ được **một** worker xử lý; Worker-A và Worker-B
   cạnh tranh lấy việc từ cùng hàng đợi. Kết quả có đúng một dòng hoàn thành cho
   mỗi sub-1, sub-2, sub-3, sub-4.
2. **MEP 4:** một sự kiện đến **ba** dịch vụ: ScoreService, NotificationService,
   StatsService. Mỗi dịch vụ nhận sự kiện và thực hiện phần việc riêng.
3. Mô tả từng MEP:
   - **Request–response:** bên gửi đưa yêu cầu và đợi phản hồi, có thể gặp timeout.
   - **One-way:** bên gửi phát thông điệp rồi tiếp tục, không đợi kết quả nghiệp vụ.
   - **Queue-based:** công việc xếp hàng, các worker chia nhau xử lý.
   - **Publish–subscribe:** một sự kiện được phân phối đến nhiều bên đăng ký.
   - **Streaming/push:** dữ liệu cập nhật liên tục qua một luồng, không cần yêu cầu
     mới cho từng bản cập nhật.

MepDemo chỉ mô phỏng trong một tiến trình, chưa phải broker hay mạng thật.
Bằng chứng: [toàn bộ MEP](evidence/mep-all.txt), [queue](evidence/mep-3.txt),
[publish subscribe](evidence/mep-4.txt).

## 3 BasicWebApi Bước 8

Đã build và gọi HTTP trên cổng 5081:

- `GET /`: **200**, JSON có `application: BasicWebApi`.
- `GET /hello/An`: **200**, `Xin chao, An!`.
- `GET /hello/An?language=en`: **200**, `Hello, An!`.
- `POST /greetings` với Binh: **201**, `Location: /greetings/1` trên API mới chạy.
- `POST /greetings` với tên rỗng: **400**, `Name must not be empty.`.
- Phần tự làm `language=ja`: **200**, `Konnichiwa, An!`.

**Câu 1:** `builder.Services.AddSingleton<GreetingService>()` phải đứng trước
`builder.Build()`. Dòng đầu đăng ký dịch vụ; Build tạo ứng dụng và service
provider từ các đăng ký đó. Sau Build không tiếp tục sửa collection dịch vụ.

**Câu 2:** trong `/hello/{name}?language=en`, `name` lấy từ route, còn `language`
lấy từ query string. Ở POST, record request nhận dữ liệu từ JSON body.

**Câu 3:** POST hợp lệ tạo một tài nguyên mới nên trả **201 Created** và Location
trỏ tới tài nguyên đó. **200 OK** chỉ mô tả yêu cầu thành công, không thể hiện
riêng việc tạo mới.

Bằng chứng: [log HTTP và kiểm tra](evidence/verification.txt),
[log API](evidence/basic-api.txt). File thử tay:
[requests.http](../samples/BasicWebApi/requests.http).

Đã thử trực tiếp bằng extension REST Client trong VS Code sau khi thư mục được
xác nhận Trust. Trích các nội dung phản hồi thực tế:

**GET trạng thái ứng dụng**

```text
GET http://localhost:5081/
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"application":"BasicWebApi"}
```

**POST tạo lời chào hợp lệ**

```text
POST http://localhost:5081/greetings
Content-Type: application/json

{"name":"Binh","language":"vi"}

Phản hồi:
HTTP/1.1 201 Created
Location: /greetings/1

{"id":1,"name":"Binh","message":"Xin chao, Binh!"}
```

**POST với tên rỗng**

```text
POST http://localhost:5081/greetings
Content-Type: application/json

{"name":"","language":"vi"}

Phản hồi:
HTTP/1.1 400 Bad Request

{"message":"Name must not be empty."}
```

**GET lời chào tiếng Nhật**

```text
GET http://localhost:5081/hello/An?language=ja
HTTP/1.1 200 OK

{"message":"Konnichiwa, An!"}
```

**Thực hành debug Bước 8.6:** đã chạy cấu hình Debug BasicWebApi và đặt điểm dừng
tại dòng 15 `var greeting = service.Create(...)`. Khi gửi POST từ REST Client,
API dừng tại breakpoint; biến request có `Name = Binh`, `Language = vi`. Bấm
Step Over chuyển sang dòng 16, biến greeting có `Id = 1`, `Name = Binh`,
`Message = Xin chao, Binh!`. Bấm Continue hoàn tất POST với **201 Created** và
`Location: /greetings/1`, sau đó dừng phiên debug bằng Stop.

| Thao tác debug | Vị trí dừng | Giá trị hoặc kết quả quan sát |
| --- | --- | --- |
| Gửi POST hợp lệ khi có breakpoint | Program.cs dòng 15 | request.Name = Binh; request.Language = vi; greeting chưa được tạo |
| Bấm Step Over | Program.cs dòng 16 | greeting.Id = 1; greeting.Name = Binh; greeting.Message = Xin chao, Binh! |
| Bấm Continue | API chạy tiếp | REST Client nhận 201 Created, Location: /greetings/1 và JSON của greeting |
| Bấm Stop | Kết thúc phiên debug | Tiến trình BasicWebApi được dừng |

Thời gian phản hồi POST trong phiên debug dài hơn bình thường vì chương trình
được giữ tại breakpoint để xem biến; sau Continue yêu cầu vẫn hoàn tất với 201.

## 4 MiniDistributedSystem Bước 9

Đã chạy API và Client ở hai tiến trình. Client in hai bài tập, GUID lượt nộp và
`Trang thai: received`. Chạy lại client sinh GUID khác.

**Câu 1:** client mẫu gọi hai endpoint:

- `GET http://localhost:5000/api/assignments` → **200**, nhận mảng bài tập JSON.
- `POST http://localhost:5000/api/submissions` → **201**, nhận kết quả tạo lượt nộp
  cùng Location. Mỗi lần là một cặp request–response riêng.

**Câu 2:** client không tham chiếu code API. Hai bên thống nhất bằng hợp đồng
HTTP: method, đường dẫn, schema JSON và mã trạng thái. Các record được khai báo
riêng trong từng project; việc trùng cấu trúc không làm chúng thành cùng một
kiểu dữ liệu trong bộ nhớ.

**Câu 3:** URL truyền qua đối số giúp đổi host/cổng giữa môi trường hoặc khi cổng
bị chiếm mà không sửa và biên dịch lại code.

Bằng chứng: [API](evidence/mini-api.txt), [client lần 1](evidence/mini-client-1.txt),
[client lần 2](evidence/mini-client-2.txt).

Trích log của hai tiến trình độc lập:

**Tiến trình API**

```text
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
Hosting environment: Production
```

**Tiến trình Client lần thứ nhất**

```text
Danh sach bai tap:
  - 1: Bai tap 1
  - 2: Bai tap 2
POST /api/submissions -> 201 Created
Da nop bai. Ma luot nop: 083a0577-6203-480d-9e9e-a21e12512dcd
Trang thai: received
```

## 5 Thí nghiệm lỗi Bước 10

[Dự đoán](du-doan-thi-nghiem.md) được ghi trước khi chạy. Các thay đổi a và c chỉ
áp dụng cho bản sao tạm; mã mẫu gốc giữ nguyên. Sau mỗi lần chạy API thử nghiệm
được dừng trước khi chuyển sang tình huống tiếp theo.

| Thí nghiệm | Dự đoán trước khi chạy | Kết quả thực tế | Đúng Sai | Giải thích |
| --- | --- | --- | --- | --- |
| a Thêm bài tập thứ ba | Client cũ in được 3 bài, không cần sửa | In thêm `3: Bai tap 3`; POST 201; trạng thái received | Đúng | Dữ liệu vẫn là mảng JSON cùng schema. Client duyệt toàn bộ danh sách nên tự nhận được phần tử mới. |
| b Tắt API rồi chạy client | Lỗi kết nối ở GET đầu tiên | `HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:5000)`; exit 1 | Đúng | Không có tiến trình lắng nghe. Gọi mạng còn phụ thuộc kết nối, địa chỉ và tiến trình bên kia. Gọi hàm nội bộ không có lỗi kết nối mạng này nhưng vẫn có thể lỗi logic hoặc ném exception. |
| c StudentCode rỗng | API trả 400, EnsureSuccessStatusCode ném lỗi | `POST /api/submissions -> 400 Bad Request`, sau đó `HttpRequestException: Response status code does not indicate success: 400 (Bad Request).`; exit 1 | Đúng | EnsureSuccessStatusCode ném lỗi khi mã không thuộc 200–299. Máy chủ phải tự kiểm tra vì client có thể bị sửa, lỗi hoặc gửi trực tiếp bằng công cụ khác. |
| d API ở 5050, client gọi 5000 | Sai cổng gây lỗi; đổi đối số URL sẽ thành công | Gọi 5000: lỗi kết nối, exit 1. Gọi 5050: POST 201, received, exit 0 | Đúng | Chạy `dotnet run --project src/Submission.Client -- http://localhost:5050`, không đổi mã nguồn. |

Log: [a](evidence/experiment-a.txt), [b](evidence/experiment-b.txt),
[c](evidence/experiment-c.txt), [d sai cổng](evidence/experiment-d-wrong-port.txt),
[d đúng cổng](evidence/experiment-d-correct-port.txt).

## 6 Dự án EduSubmit Bước 11 và 12

Hai project `src/Submission.Api` và `src/Submission.Client` không có
ProjectReference. API kiểm tra assignmentId dương, studentCode và repositoryUrl
không rỗng hoặc chỉ có khoảng trắng. Mỗi lần nộp hợp lệ tạo GUID mới, status
`received`, receivedAt theo UTC và trả 201 cùng Location.

Phần mở rộng dùng `List<SubmissionResult>` lưu trong bộ nhớ. Các thao tác trên
danh sách có khóa để tránh xung đột khi nhiều request truy cập cùng lúc.
`GET /api/submissions/{id:guid}` trả 200 cho ID có thật và 404 khi không tìm thấy.
Client gọi GET sau POST và in lại ID, mã sinh viên, trạng thái.

Kết quả chạy thật:

- Lượt nộp kiểm thử: `b413cacd-cd38-4868-8d77-4167cb9ee0c8`, POST **201**.
- GET đúng Location của lượt nộp này: **200**, cùng ID và MSSV DTC245200318.
- GET GUID toàn số 0 chưa được tạo: **404**.
- Client tạo lượt nộp `21a6223a-3c3c-4031-b2f7-915c6cdd06c2`, sau đó đọc lại **200**.
- JSON thiếu dữ liệu, mã sinh viên rỗng, repositoryUrl thiếu/rỗng và assignmentId
  bằng 0 hoặc âm đều được thử và trả **400**.

Luồng thông điệp:

```text
Client -- HTTP GET /api/assignments ----------> API
Client <---------- 200 JSON danh sách -------- API
Client -- HTTP POST /api/submissions --------> API
Client <---------- 201 JSON và Location ------ API
Client -- HTTP GET /api/submissions/{id} -----> API
Client <---------- 200 JSON lượt nộp ---------- API
```

Cả ba tương tác đều dùng request–response. Dữ liệu không tồn tại sau khi khởi
động lại API, phù hợp phạm vi lưu trong bộ nhớ của Bài 1.

Phần tham khảo: đặc tả Bước 11, 12 và mã MepDemo đã cung cấp. Không có bản mẫu
MiniDistributedSystem gốc để đối chiếu; bản trong samples được dựng lại từ đặc tả.

Bằng chứng: [log client](evidence/edusubmit-client.txt),
[log máy chủ](evidence/edusubmit-api.txt), [log kiểm thử](evidence/verification.txt).

Kết quả Client EduSubmit đã chạy:

```text
Danh sach bai tap:
  - 1: Bai tap 1
  - 2: Bai tap 2
POST /api/submissions -> 201 Created
Da nop bai. Ma luot nop: 21a6223a-3c3c-4031-b2f7-915c6cdd06c2
Trang thai: received
GET /api/submissions/21a6223a-3c3c-4031-b2f7-915c6cdd06c2 -> 200 OK
Doc lai: 21a6223a-3c3c-4031-b2f7-915c6cdd06c2 | DTC245200318 | received
```

## 7 Khó khăn và cách giải quyết

- Đầu vào thiếu BasicWebApi và MiniDistributedSystem: dựng hai ví dụ từ đặc tả
  rồi kiểm tra hành vi HTTP theo tài liệu.
- Build solution trong môi trường chạy hạn chế gặp lỗi tiến trình build song
  song: script dùng `-m:1` và tắt compiler server. Không cần thêm package ngoài.
- Windows PowerShell đọc script UTF-8 có dấu sai khi thiếu BOM: lưu script UTF-8
  có BOM để các chuỗi kiểm tra tiếng Việt khớp đầu ra.
- Kết nối localhost bị giới hạn trong phiên kiểm thử ban đầu: chạy lại bộ kiểm
  thử với quyền cho phép kết nối localhost; không đổi chính sách bảo mật máy.
- Đổi cổng hoặc tắt API gây lỗi thật như dự đoán; giải quyết bằng khởi động API
  và truyền đúng URL thay vì viết cứng địa chỉ.
- API đang chạy khóa DLL khi build lại: dừng các API kiểm thử rồi build lại
  solution; lần build cuối thành công với 0 warning và 0 error.

Kết quả kiểm tra chức năng lần hoàn thành: **ALL 50 CHECKS PASSED**. Cả bốn nhóm
project đã build thành công, không có warning hay error. Log build và kiểm thử
được giữ để đối chiếu; không thay log bằng kết quả tự dựng.

Đã chạy hàm `cham_chay` trong bộ chấm công khai của lớp trực tiếp trên dự án.
Kết quả kiểm tra trên máy: `chay: 4.0`, `mr: 1.0`, API khởi động được ở cổng
5198. Chỉ thay hàm dừng cây tiến trình để harness chạy trên Windows; các yêu cầu,
request và điều kiện chấm giữ nguyên. Đây là kiểm tra tương thích tại máy,
không phải điểm chấm cuối cùng của giảng viên.
Bằng chứng: [class-grader.json](evidence/class-grader.json).

Kết quả thực hành, phản hồi HTTP và diễn biến phiên debug được ghi trực tiếp
trong nhật ký bằng văn bản, khối kết quả và bảng. Thư mục đã được người dùng xác
nhận Trust, các extension và debugger đã hoạt động. Các phản hồi HTTP được kiểm
chứng cả bằng HttpClient trong script và trực tiếp qua REST Client.
Log build cuối sau khi nạp thêm mẫu BasicWebApi vào solution:
[final-solution-build.txt](evidence/final-solution-build.txt).

## 8 Câu hỏi tự kiểm tra

1. **Phân tán và nguyên khối:** hệ thống phân tán có các thành phần chạy độc lập,
   trao đổi qua mạng. Ở bài này ranh giới là giữa hai tiến trình API và Client.
   Ứng dụng nguyên khối thường đặt các phần nghiệp vụ trong một ứng dụng triển
   khai chung; gọi nội bộ không phải đi qua ranh giới HTTP như bài này.
2. **Năm MEP:** request–response, one-way, queue-based, publish–subscribe và
   streaming/push. EduSubmit Bài 1 dùng request–response.
3. **Queue và pub/sub:** queue chia việc giữa các consumer, một công việc của
   demo được một worker nhận; ví dụ hàng đợi chấm bài. Pub/sub gửi một sự kiện
   đến nhiều subscriber; ví dụ chấm xong vừa lưu điểm, gửi thông báo và cập nhật
   thống kê. Bảo đảm giao hàng trong broker thật còn tùy cấu hình và có thể cần
   chống xử lý lặp.
4. **Mã trạng thái:** 200 là xử lý thành công; 201 là tạo tài nguyên mới; 400 là
   yêu cầu/dữ liệu đầu vào không hợp lệ; 404 là không tìm thấy tài nguyên.
5. **Kiểm tra status trước khi đọc dữ liệu:** phản hồi lỗi có thể không cùng
   schema với phản hồi thành công. Bỏ qua status dễ giải mã sai JSON hoặc hiểu
   nhầm việc nộp đã thành công. EnsureSuccessStatusCode phát hiện lỗi HTTP sớm.
