# Catalog API – Client, thí nghiệm và mở rộng

Họ tên: Nguyễn Việt Đô

MSSV: DTC245200604

Lớp: CNTTK23D

## 1. Tạo khung và build

Solution `catalogapi.slnx` gồm hai project độc lập:

```text
catalogapi/
├── catalogapi.slnx
├── .gitignore
├── README.md
├── requests.http
├── sp-hop-le.json
├── sp-sai.json
├── tests/
│   └── test_catalog.py
└── src/
    ├── Catalog.Api/
    └── Catalog.Client/
```

Yêu cầu .NET SDK 10. Tại thư mục `catalogapi`, chạy:

```powershell
dotnet --version
dotnet build catalogapi.slnx
```

Kết quả: SDK `10.0.403`; cả hai project build thành công, **0 Warning(s), 0 Error(s)**. API và Client chạy ở hai tiến trình riêng, không có ProjectReference giữa hai project.

## 2. Chạy API

Terminal 1:

```powershell
dotnet run --project src/Catalog.Api --urls http://localhost:5100
```

Terminal 2:

```powershell
curl.exe -i http://localhost:5100/
```

`GET /` trả `200 OK` với JSON:

```json
{"service":"Catalog.Api","status":"running"}
```

## 3. Danh sách sản phẩm

```powershell
curl.exe -i http://localhost:5100/api/products
```

Kết quả: `200 OK`, mảng JSON có ba sản phẩm:

| ID | Tên | Giá |
|---|---|---:|
| 1 | Giao trinh Lap trinh phan tan | 150000 |
| 2 | Tai lieu RESTful API | 90000 |
| 3 | Bai tap thuc hanh .NET | 120000 |

`ProductDto` gồm `Id`, `Name`, `Description`, `Price`. Giá dùng `decimal`. ASP.NET Core chuyển tên thuộc tính thành camelCase trong JSON. Danh sách nằm trong bộ nhớ của máy chủ, không phải bộ nhớ dùng chung với Client.

## 4. Chi tiết và kiểm thử lỗi

```powershell
curl.exe -i http://localhost:5100/api/products/1
curl.exe -i http://localhost:5100/api/products/999
curl.exe -i http://localhost:5100/api/products/abc
curl.exe -i -X PUT http://localhost:5100/api/products/1
```

| Request | Kết quả đã kiểm tra | Nơi xử lý |
|---|---|---|
| `GET /` | 200, JSON dịch vụ | Handler |
| `GET /api/products` | 200, đủ ba sản phẩm | Handler |
| `GET /api/products/1` | 200, sản phẩm ID 1 | Handler |
| `GET /api/products/2` | 200, sản phẩm ID 2 | Handler |
| `GET /api/products/999` | 404, `{"code":"PRODUCT_NOT_FOUND","message":"Khong tim thay san pham."}` | Handler |
| `GET /api/products/abc` | 404, body rỗng | Framework |
| `PUT /api/products/1` | 405, body rỗng, `Allow: GET, DELETE` (không phụ thuộc thứ tự) | Framework |

Ràng buộc `{id:int}` chỉ khớp ID dạng số nguyên. Với `/999`, route khớp nên handler chạy, tìm không thấy và trả `ApiError`. Với `/abc`, route không khớp nên handler không chạy, framework trả 404 rỗng. Ca kiểm tra 405 dùng PUT vào URI chi tiết; URI này hỗ trợ GET và DELETE, không hỗ trợ PUT.

## 5. POST tạo sản phẩm

DTO đầu vào là `CreateProductRequest(string Name, string? Description, decimal Price)`, không có `Id` vì ID do máy chủ sinh. Tên không được rỗng hoặc chỉ có khoảng trắng, giá phải lớn hơn 0; sai điều kiện trả `400` cùng mã `INVALID_PRODUCT`.

Dữ liệu hợp lệ được `Trim()` tên và mô tả. Mô tả null hoặc bỏ qua được lưu thành chuỗi rỗng. Máy chủ dùng bộ đếm `nextId` bắt đầu từ 4, lưu sản phẩm và trả `201 Created` cùng header `Location`. Bộ đếm không lùi khi xóa nên vẫn tạo được khi danh sách rỗng và không tái dùng ID vừa xóa trong cùng tiến trình. Các thao tác đọc/lọc/thêm/xóa và sinh ID dùng chung một khóa để tránh trùng ID hoặc lỗi đọc/ghi đồng thời.

Tại thư mục `catalogapi`, với API đang chạy ở cổng 5100:

```powershell
curl.exe -i -X POST http://localhost:5100/api/products -H "Content-Type: application/json" -d "@sp-hop-le.json"
curl.exe -i -X POST http://localhost:5100/api/products -H "Content-Type: application/json" -d "@sp-sai.json"
curl.exe -i -X POST http://localhost:5100/api/products -d "@sp-hop-le.json"
```

| Ca | Kết quả đã kiểm tra | Nơi xử lý |
|---|---|---|
| Hợp lệ | 201, `Location: /api/products/4`, body sản phẩm ID 4 | Handler |
| Giá âm | 400, `INVALID_PRODUCT` | Handler |
| Không khai báo kiểu nội dung JSON | 415 | Framework, trước handler |

ID 4 là kết quả của POST hợp lệ đầu tiên sau khi khởi động API. Gửi hợp lệ tiếp sẽ sinh ID mới. GET theo `Location` trả 200 và đúng sản phẩm vừa tạo; danh sách tăng từ 3 lên 4. Các POST sai không thêm sản phẩm.

400 do handler có body `ApiError`; 415 do framework từ chối kiểu nội dung nên không cam kết cùng dạng lỗi. Với `curl.exe -d`, nếu không truyền `-H`, curl tự dùng `application/x-www-form-urlencoded`, không phải JSON nên vẫn nhận 415.

## 6. Kiểm thử requests.http

`requests.http` giữ nguyên tám ca đầu, bổ sung bảy ca mở rộng, dùng biến `@api = http://localhost:5100`. Mở bằng REST Client rồi bấm **Send Request** phía trên từng khối. Khởi động lại API trước khi chạy từ đầu nếu muốn kiểm tra danh sách ban đầu có ba sản phẩm. Ca 10 có tên `createForDelete`; ca 11–13 lấy ID từ response ca 10, không đoán ID cố định.

| # | Request | Kết quả đã chạy bằng curl.exe |
|---|---|---|
| 1 | `GET /` | 200, JSON dịch vụ |
| 2 | `GET /api/products` | 200, ba sản phẩm ban đầu |
| 3 | `GET /api/products/1` | 200, sản phẩm ID 1 |
| 4 | `GET /api/products/999` | 404 + `PRODUCT_NOT_FOUND` |
| 5 | `GET /api/products/abc` | 404, body rỗng |
| 6 | `POST /api/products`, hợp lệ | 201 + `Location` + sản phẩm mới |
| 7 | `POST /api/products`, giá âm | 400 + `INVALID_PRODUCT` |
| 8 | `POST /api/products`, không có Content-Type JSON | 415 |
| 9 | `GET /api/products?keyword=REST` | 200, tên chứa REST không phân biệt hoa thường |
| 10 | POST sản phẩm riêng để xóa | 201, lưu ID từ response |
| 11 | DELETE ID của ca 10 | 204, body rỗng |
| 12 | GET ID đã xóa | 404 + `PRODUCT_NOT_FOUND` |
| 13 | DELETE lại ID đã xóa | 404 + `PRODUCT_NOT_FOUND` |
| 14 | POST tên 101 ký tự, giá hợp lệ | 400 + `NAME_TOO_LONG` |
| 15 | POST tên 101 ký tự, giá âm | 400 + `INVALID_PRODUCT` |

Kết quả: **15/15 ca đúng mã trạng thái** khi chạy theo nội dung tệp bằng `curl.exe` trên cổng 5100; đã kiểm tra body, mã lỗi, Location và GET đọc lại sản phẩm vừa tạo. Hai tệp payload JSON có sẵn cũng đã chạy bằng curl.exe: hợp lệ 201, sai 400. Chưa xác minh thao tác Send Request qua giao diện REST Client.

## 7. Client HttpClient – bốn nấc

Giữ API chạy trong Terminal 1. Tại Terminal 2:

```powershell
dotnet run --project src/Catalog.Client
# Hoặc chỉ định địa chỉ bằng args[0]:
dotnet run --project src/Catalog.Client -- http://localhost:5100
```

Địa chỉ mặc định là `http://localhost:5100`; timeout 10 giây. Client tự định nghĩa `ProductDto`, `CreateProductRequest`, `ApiError`, dùng HttpClient và JSON thay vì dùng chung đối tượng với API.

| Nấc | Thao tác | Kết quả thực chạy sau khi khởi động API mới |
|---|---|---|
| 1 | GET danh sách | 200, in ba sản phẩm |
| 2 | POST tên `Nhap mon REST API`, mô tả `Sach tham khao`, giá `120000` | 201, in Location và ID 4 |
| 3 | GET chi tiết theo ID vừa nhận | 200, đúng sản phẩm ID 4 |
| 4 | Cố ý POST tên rỗng, giá -1 | 400, đọc `ApiError` có mã `INVALID_PRODUCT` |

Client in status trước khi đọc/giải mã body. Không gọi `EnsureSuccessStatusCode()` cho ca 400 có chủ ý. HTTP bất ngờ: đọc ApiError nếu có; body rỗng, text hoặc JSON lỗi được xử lý an toàn, không để lỗi giải mã che status. Không kết nối/timeout có thông báo ngắn, không crash.

Mã thoát: **0** khi đủ bốn nấc (400 cuối là kết quả mong đợi); **1** khi HTTP/JSON/cấu hình không đúng; **2** khi không kết nối được hoặc timeout. Đã chạy cả lệnh mặc định và lệnh có đối số: lần lượt tạo ID 4, 5; đều exit 0.

## 8. Bốn thí nghiệm

Dự đoán được ghi trước khi thực hiện từng thí nghiệm; khởi đầu bằng API mới. Dừng API bằng Ctrl+C, không dừng tiến trình khác.

| Thí nghiệm | Dự đoán | Thực tế | Đúng/Sai và giải thích |
|---|---|---|---|
| Chạy Client hai lần, giữ nguyên API | Tạo ID 4 rồi 5; danh sách có 5 sản phẩm | ID 4, 5; cả hai exit 0; danh sách 5 | Đúng: Client không reset dữ liệu của server; POST không idempotent |
| Dừng API rồi chạy Client | Lỗi kết nối, không có HTTP response | Thông báo không kết nối được, exit 2 | Đúng: không phải HTTP 400/404 vì chưa nhận response |
| Khởi động lại API cùng địa chỉ, GET danh sách và ID 4 | Trở lại ba seed; sản phẩm tạo trước bị mất | Danh sách ID 1–3; GET 4 trả 404 `PRODUCT_NOT_FOUND` | Đúng: List chỉ nằm trong RAM của tiến trình |
| API đang chạy nhưng Client dùng sai cổng chưa có dịch vụ | Lỗi kết nối ở Client, API đúng vẫn hoạt động | Client exit 2; GET root ở cổng đúng vẫn 200 | Đúng: địa chỉ/cổng phải khớp; không có response từ cổng sai |

Có thể tái lập tự động bằng `test_four_experiments` trong bộ kiểm thử bên dưới. Với thử sai cổng thủ công, chọn cổng chưa có dịch vụ, ví dụ:

```powershell
dotnet run --project src/Catalog.Client -- http://localhost:5199
```

## 9. Mở rộng Catalog

- `GET /api/products?keyword=REST`: lọc **Name** bằng `OrdinalIgnoreCase`; không khớp trả mảng rỗng, keyword trống/không có trả toàn bộ danh sách.
- `DELETE /api/products/{id:int}`: tìm thấy trả **204 No Content**, không có JSON body; thiếu ID trả **404** + `ApiError(PRODUCT_NOT_FOUND, ...)`. Đọc lại hoặc xóa lại ID đã xóa đều 404. ID không nguyên không khớp route.
- Kiểm tra POST theo thứ tự: tên null/rỗng/khoảng trắng hoặc giá <= 0 → **400 INVALID_PRODUCT**; sau đó `Name.Length > 100` → **400 NAME_TOO_LONG**; sau cùng mới cấp ID và thêm dữ liệu. Độ dài kiểm tra trên chuỗi đầu vào trước Trim; đúng 100 được chấp nhận, 101 bị từ chối. Request sai không tiêu thụ ID.

```powershell
curl.exe -i "http://localhost:5100/api/products?keyword=REST"
# Lấy ID từ response POST trước khi thay <id>:
curl.exe -i -X DELETE "http://localhost:5100/api/products/<id>"
curl.exe -i "http://localhost:5100/api/products/<id>"
```

## 10. Kiểm thử tự động

Yêu cầu Python 3, chỉ dùng thư viện chuẩn; không cần pip/NuGet test framework:

```powershell
python -m unittest discover -s tests -v
```

Kết quả thực chạy: **12/12 test đạt**. Bộ test build solution, chạy API và Client thật trên cổng tạm, kiểm tra tám ca gốc, bốn nấc Client, lỗi kết nối, body rỗng/text/JSON hỏng, lọc hoa/thường, xóa/404, xóa hết rồi tạo lại, biên 100/101, thứ tự validation, Trim/mô tả null và 24 POST đồng thời với ID riêng biệt. Bốn thí nghiệm có dự đoán và kết quả lưu riêng. Log mặc định nằm ngoài repo trong thư mục tạm; có thể đổi bằng biến môi trường `CATALOG_EVIDENCE_DIR`.

Test lỗi response tạm thay đúng một response của API, build và chạy máy chủ thật; `finally` khôi phục nguyên bytes nguồn và build lại. Không chạy song song bộ test này hoặc chỉnh Program.cs trong lúc test chạy. Mọi server do test tạo đều được dừng trong `finally`.

## 11. Câu hỏi

**1. Vì sao URI dùng danh từ, không dùng động từ?** `/api/products` định danh tập tài nguyên sản phẩm, `/api/products/4` định danh một sản phẩm. GET/POST/DELETE đã thể hiện hành động nên không cần `/getProducts` hay `/deleteProduct`.

**2. Stateless có nghĩa là server không được lưu dữ liệu không?** Không. Mỗi request phải mang đủ thông tin để server xử lý, không phụ thuộc phiên hội thoại của Client trước đó. Server vẫn có thể giữ tài nguyên trong RAM hoặc database. Thí nghiệm cho thấy RAM mất khi tiến trình khởi động lại; đó là tính bền vững của nơi lưu, không phải định nghĩa stateless.

**3. Vì sao Client và API có DTO riêng nhưng vẫn trao đổi được?** Chúng thống nhất hợp đồng JSON: tên thuộc tính `id`, `name`, `description`, `price` và kiểu dữ liệu tương thích. JSON đi qua HTTP được deserialize thành đối tượng riêng trong từng tiến trình; không cần chung class/assembly hay ProjectReference. DTO đầu vào không chứa ID vì server chịu trách nhiệm cấp ID.

## 12. Minh chứng

- Ảnh API và Client: chờ bổ sung.
- Ảnh kiểm thử HTTP: chờ bổ sung.

Phần Catalog đã có mã nguồn, build, kiểm thử và thí nghiệm thực chạy. Ảnh và thao tác REST Client GUI chưa xác minh; không thuộc kết quả curl.exe ở trên.
