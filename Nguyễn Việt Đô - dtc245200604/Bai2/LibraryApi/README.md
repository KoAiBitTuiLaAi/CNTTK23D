# Bài 2 – LibraryApi: đổi miền quản lý sách

| Thông tin | Giá trị |
|---|---|
| Họ và tên | Nguyễn Việt Đô |
| Mã sinh viên | DTC245200604 |
| Lớp | CNTTK23D |
| Nền tảng | .NET 10; ASP.NET Core Minimal API và Console Client |
| Địa chỉ chạy | `http://localhost:5200` |

## 1. Thiết kế miền bài toán

Đổi tài nguyên Catalog sang **Book**, giữ cách giao tiếp HTTP/JSON. Phạm vi là danh mục sách, không triển khai mượn/trả hoặc cơ sở dữ liệu. Mỗi lần API khởi động, danh sách rỗng và bộ đếm ID bắt đầu từ 1.

### Năm câu hỏi thiết kế

| Câu hỏi | Quyết định |
|---|---|
| 1. Tài nguyên là gì? | Sách (`Book`); tập tài nguyên `/api/books`, một sách `/api/books/{id:int}`. |
| 2. Có những hành động nào? | Xem danh sách, xem một sách, tạo sách và xóa sách. `GET /` chỉ giới thiệu dịch vụ. |
| 3. DTO trao đổi gồm những gì? | `BookDto(int Id, string Title, string Author, decimal Price)`; `CreateBookRequest(string Title, string Author, decimal Price)`; `ApiError(string Code, string Message)`. |
| 4. Kiểm tra dữ liệu ở đâu? | API kiểm tra `Title`, `Author` không null/rỗng/toàn khoảng trắng và `Price > 0`. Sai nghiệp vụ trả `400 INVALID_BOOK`. ID do server sinh; không lấy ID từ body. |
| 5. Client gọi API theo luồng nào? | Lấy danh sách → tạo, kiểm tra **201 trước khi đọc BookDto** → đọc sách qua `Location` → gửi dữ liệu sai, kiểm tra **400** và đọc `ApiError`. |

### Bảng hợp đồng API

| Method | URI | Request | Thành công | Lỗi chính |
|---|---|---|---|---|
| GET | `/` | Không body | 200, `{"service":"Library.Api"}` | — |
| GET | `/api/books` | Không body | 200, mảng `BookDto`; ban đầu `[]` | — |
| GET | `/api/books/{id:int}` | ID nguyên trong đường dẫn | 200, một `BookDto` | 404, `BOOK_NOT_FOUND` nếu không có ID |
| POST | `/api/books` | JSON `CreateBookRequest` | 201, `BookDto` và `Location: /api/books/{id}` | 400 `INVALID_BOOK`; 415 khi không phải JSON |
| DELETE | `/api/books/{id:int}` | Không body | 204, **body rỗng** | 404, `BOOK_NOT_FOUND` nếu không có ID |

JSON dùng camelCase. `Price` là `decimal` ở cả API và Client, được truyền bằng JSON number. `CreateBookRequest` không có `Id`; các trường JSON ngoài DTO không được dùng để cấp ID. Truy cập danh sách và bộ đếm ID được bảo vệ bằng cùng một `lock`, kể cả khi tạo đồng thời.

**Ví dụ tạo sách:**

```http
POST http://localhost:5200/api/books
Content-Type: application/json

{"title":"The Pragmatic Programmer","author":"Andrew Hunt","price":250.50}
```

```http
HTTP/1.1 201 Created
Location: /api/books/1
Content-Type: application/json; charset=utf-8

{"id":1,"title":"The Pragmatic Programmer","author":"Andrew Hunt","price":250.50}
```

**Lỗi nghiệp vụ có cấu trúc:**

```json
{"code":"INVALID_BOOK","message":"Title and author are required; price must be positive."}
```

```json
{"code":"BOOK_NOT_FOUND","message":"Book not found."}
```

**Phân biệt lỗi framework:** `POST` với `Content-Type: text/plain` trả **415, body rỗng**; `PUT /api/books` trả **405, body rỗng**, header `Allow` chứa `GET, POST` (không phụ thuộc thứ tự). `/api/books/not-an-integer` không khớp `{id:int}` nên trả **404, body rỗng**, không phải `ApiError`. JSON sai cú pháp hoặc giá trị không chuyển được sang kiểu DTO bị bộ bind JSON từ chối bằng 400, không thuộc lỗi kiểm tra nghiệp vụ `INVALID_BOOK`.

## 2. Mười bước thực hiện

1. Chọn miền sách và giới hạn danh mục, xác định các thao tác cần thiết.
2. Tạo solution `LibraryApi.slnx` gồm web project `Library.Api` và console project `Library.Client` trên `net10.0`.
3. Chốt DTO đầu vào, đầu ra và DTO lỗi; Client tự khai báo DTO, không tham chiếu project API.
4. Chốt URI, HTTP method, status code và header `Location` theo bảng hợp đồng.
5. Viết kiểm thử GET dịch vụ/danh sách rồi triển khai các lát dọc RED → GREEN.
6. Viết kiểm thử tạo sách, cấp ID đồng thời rồi bổ sung POST trả 201.
7. Viết kiểm thử đọc sách/404 và dữ liệu sai/400 rồi bổ sung handler tương ứng.
8. Viết kiểm thử xóa rồi bổ sung DELETE trả 204 hoặc 404; đối chiếu 415, 405 và route constraint của framework.
9. Dựng Client qua từng nấc kiểm thử: LIST → CREATE → READ Location → INVALID; xử lý lỗi mạng không có stack trace và trả exit code 1.
10. Build, chạy toàn bộ test Python, gửi đúng sáu request `.http` trên cổng 5200, chạy Client mặc định và có tham số, đối chiếu bảng kết quả.

## 3. Cấu trúc và lệnh chạy

```text
LibraryApi/
├── LibraryApi.slnx
├── src/
│   ├── Library.Api/       # ASP.NET Core Minimal API; DTO phía server
│   └── Library.Client/    # HttpClient; DTO độc lập
├── tests/test_library.py  # Python standard library, tiến trình .NET thật
├── requests.http         # Chính xác sáu request
├── .gitignore
└── README.md
```

Yêu cầu: .NET SDK 10 và Python 3. Không cần thư viện Python bên ngoài hoặc gói test NuGet. Mở terminal tại thư mục chứa `LibraryApi.slnx`:

```bash
dotnet build LibraryApi.slnx
dotnet run --project src/Library.Api --launch-profile http
```

API lắng nghe `http://localhost:5200`; giữ terminal này chạy. Tại terminal thứ hai:

```bash
# Mặc định gọi localhost:5200
dotnet run --project src/Library.Client

# Hoặc chỉ định base URL
dotnet run --project src/Library.Client -- http://localhost:5200
```

Không có `ProjectReference` giữa hai project; Client gọi qua mạng, không gọi trực tiếp phương thức C# của API. `HttpClient` có timeout 10 giây, được dispose cùng các response. Client thành công trả exit code 0; lỗi kết nối/timeout, HTTP bất ngờ hoặc JSON không hợp lệ được báo bằng `CLIENT_ERROR: ...`, exit code 1, không in stack trace.

Kiểm thử tự động:

```bash
python tests/test_library.py
```

Test tự build solution, mỗi ca khởi động API với dữ liệu sạch trên cổng rảnh riêng và dừng **đúng tiến trình do test tạo** trong cleanup, kể cả khi ca kiểm thử thất bại. Log mặc định nằm ở scratch hệ thống qua `TMPDIR`; có thể chỉ định thư mục ngoài bài bằng biến môi trường `LIBRARY_TEST_LOG_DIR`. Không chạy test dựa trên API đã mở ở 5200.

## 4. Sáu request và kết quả thực chạy

Mở `requests.http` trong VS Code có REST Client và gửi theo thứ tự 1–6. Request số 3 được đặt tên `createBook`; số 5 lấy ID từ response của request đó, không xóa một ID gõ cố định. Nếu chạy lại số 3 thì response mới là sách sẽ được xóa ở số 5. Gửi lại DELETE sau khi đã xóa sẽ nhận 404 đúng hợp đồng.

Bảng dưới là kết quả đã gửi **HTTP thật bằng Python urllib**, đọc method/URI/header/body trực tiếp từ `requests.http`, trên API riêng tại `http://localhost:5200`. Không ghi nhận thao tác bấm Send Request trong GUI REST Client.

| STT | Request | Dự đoán | Thực tế | Đối chiếu response |
|---|---|---|---|---|
| 1 | GET `/api/books` | 200 | **200** | `[]` khi API vừa khởi động |
| 2 | GET `/api/books/999999` | 404 | **404** | `ApiError.code = BOOK_NOT_FOUND` |
| 3 | POST hợp lệ, tên `createBook` | 201 | **201** | `id = 1`, `price = 250.50`, `Location: /api/books/1` |
| 4 | POST title/author rỗng, price = 0 | 400 | **400** | `ApiError.code = INVALID_BOOK` |
| 5 | DELETE ID từ request 3 | 204 | **204** | Body rỗng; đọc lại danh sách không còn sách vừa xóa |
| 6 | POST `Content-Type: text/plain` | 415 | **415** | Body rỗng; framework từ chối media type |

**Kết quả build/test:** SDK 10.0.403; build thành công, **0 warning, 0 error**. Bộ Python có **12 test đều PASS**, bao gồm 49 ID duy nhất (1 lần tạo trước và 48 lần tạo qua 12 worker), dữ liệu rỗng/null/khoảng trắng/giá không dương, header Location, 204 body rỗng, 405/Allow, route constraint, bốn nấc Client và lỗi kết nối sau khi dừng API.

Hai lần chạy Client trên cổng 5200 (mặc định và có tham số) đều exit 0. Sau sáu request, lần Client mặc định thực tế in:

```text
1. LIST 200: 0 book(s)
2. CREATE 201: Id=2; Location=/api/books/2
3. READ 200: Id=2; The Pragmatic Programmer
4. INVALID 400: INVALID_BOOK; Title and author are required; price must be positive.
```

ID bằng 2 vì sách số 1 đã được tạo rồi xóa; ID không tái sử dụng trong cùng phiên API. Lần Client tiếp theo tạo ID 3 và thấy một sách ở bước LIST. API kiểm chứng đã được dừng sau khi chạy xong.

## 5. Ba câu trả lời

### 1. Tại sao Client cần DTO độc lập với API?

Hợp đồng giữa hai ứng dụng là HTTP/JSON, không phải một assembly chung. DTO độc lập làm rõ ranh giới tiến trình và giúp Client vẫn dùng API mà không tham chiếu mã nguồn phía server; tên và kiểu các trường cần tương thích với JSON đã công bố.

### 2. Vì sao phải kiểm tra status trước khi đọc response body?

Một POST thành công trả `BookDto`, nhưng POST lỗi nghiệp vụ trả `ApiError`; lỗi media type còn có thể không có body. Nếu luôn đọc `BookDto` thì có thể đọc nhầm lỗi thành dữ liệu hoặc gặp lỗi JSON. Vì vậy Client yêu cầu đúng 201 rồi mới đọc sách và `Location`, yêu cầu đúng 400 rồi mới đọc `ApiError`.

### 3. Phân biệt 400, 404, 405, 415 và 204 thế nào?

400 là dữ liệu đầu vào không hợp lệ; 404 là sách không tồn tại hoặc URI không khớp (phân biệt bằng body); 405 là URI tồn tại nhưng HTTP method không được hỗ trợ; 415 là định dạng body không được hỗ trợ; 204 là thao tác xóa thành công và không gửi body. Đây là phản hồi HTTP từ API. Khi API đã tắt, Client gặp lỗi kết nối, không được coi đó là HTTP 400 hay 404.
