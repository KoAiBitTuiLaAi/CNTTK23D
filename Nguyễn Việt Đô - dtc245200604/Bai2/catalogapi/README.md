# Catalog API – Bước 1–6

Họ tên: Nguyễn Việt Đô

MSSV: DTC245200604

Lớp: CNTT K23D

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
└── src/
    ├── Catalog.Api/
    └── Catalog.Client/
```

Yêu cầu .NET SDK 10. Tại thư mục `catalogapi`, chạy:

```powershell
dotnet --version
dotnet build catalogapi.slnx
```

Kết quả: SDK `10.0.403`; cả hai project build thành công, **0 Warning(s), 0 Error(s)**. `Catalog.Client` hiện giữ nguyên chương trình console mẫu, chưa thực hiện phần gọi API.

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
| `PUT /api/products/1` | 405, body rỗng, `Allow: GET` | Framework |

Ràng buộc `{id:int}` chỉ khớp ID dạng số nguyên. Với `/999`, route khớp nên handler chạy, tìm không thấy và trả `ApiError`. Với `/abc`, route không khớp nên handler không chạy, framework trả 404 rỗng. Sau bước 5, `/api/products` đã hỗ trợ POST; ca kiểm tra 405 dùng PUT vào URI chi tiết vì URI này chỉ có GET.

## 5. POST tạo sản phẩm

DTO đầu vào là `CreateProductRequest(string Name, string? Description, decimal Price)`, không có `Id` vì ID do máy chủ sinh. Tên không được rỗng hoặc chỉ có khoảng trắng, giá phải lớn hơn 0; sai điều kiện trả `400` cùng mã `INVALID_PRODUCT`.

Dữ liệu hợp lệ được `Trim()` tên và mô tả. Mô tả null hoặc bỏ qua được lưu thành chuỗi rỗng. Máy chủ lấy ID lớn nhất cộng 1, lưu sản phẩm và trả `201 Created` cùng header `Location`. Các thao tác trên danh sách dùng chung một khóa để tránh trùng ID hoặc lỗi đọc/ghi khi có yêu cầu đồng thời.

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

`requests.http` có đúng tám ca, dùng biến `@api = http://localhost:5100`. Mở bằng REST Client rồi bấm **Send Request** phía trên từng khối. Khởi động lại API trước khi chạy từ đầu nếu muốn kiểm tra danh sách ban đầu có ba sản phẩm.

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

Kết quả: **8/8 ca đúng mã trạng thái** khi chạy theo nội dung tệp bằng `curl.exe` trên cổng 5100. Đã kiểm tra thêm GET theo Location, tên rỗng/null/khoảng trắng, giá 0, mô tả null/bỏ qua và 24 lần tạo đồng thời với ID riêng biệt. Chưa xác minh thao tác Send Request qua giao diện REST Client.

## Phạm vi hiện tại

Đã hoàn thành mã nguồn và kiểm thử HTTP Catalog đến bước 6. `Catalog.Client` vẫn là khung console; chưa làm bước 7–10 hoặc EduSubmit. Ảnh minh chứng chưa bổ sung.
