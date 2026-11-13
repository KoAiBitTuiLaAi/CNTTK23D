Bài 2 – RESTful API
Họ tên: Đỗ Trọng Dũng
MSSV: DTC245200730
Lớp: CNTT K23D

## 1. Dựng solution và chạy máy chủ (Bước 1–2)
- `dotnet --version`: 10.0.401
- Solution `CatalogApi` gồm 2 project: `src/Catalog.Api` (máy chủ) và `src/Catalog.Client` (máy khách console).
- Chạy máy chủ ở cổng cố định 5100:
```
dotnet run --project src/Catalog.Api --urls http://localhost:5100
```
- `curl.exe -i http://localhost:5100/` trả `200 OK` và `{"service":"Catalog.Api","status":"running"}`.

<img width="1920" height="1140" alt="{F82A7439-48C1-47C9-8524-2DE12CA984EA}" src="https://github.com/user-attachments/assets/832510ab-3a8a-4197-bfd5-f772a7adae6b" />

## 2. Máy chủ Catalog.Api (Bước 3–5)
Program.cs của máy chủ chia 5 vùng theo thứ tự: khởi tạo → dữ liệu mẫu → các endpoint → `app.Run()` → khai báo kiểu (record). Record phải nằm cuối tệp vì Program.cs dùng câu lệnh cấp cao nhất (top-level statements), C# bắt câu lệnh viết trước, khai báo kiểu viết sau.

Hợp đồng API:

| Method | URI | Thành công | Lỗi |
|--------|-----|------------|-----|
| GET | `/` | 200 + trạng thái dịch vụ | |
| GET | `/api/products` | 200 + mảng sản phẩm | |
| GET | `/api/products/{id:int}` | 200 + sản phẩm | 404 + `PRODUCT_NOT_FOUND` |
| POST | `/api/products` (body `CreateProductRequest`) | 201 + header `Location` | 400 + `INVALID_PRODUCT`; 415 nếu thiếu Content-Type |

### Bước 3: GET /api/products
`curl.exe -i http://localhost:5100/api/products` trả `200 OK`, `Content-Type: application/json` và mảng 3 sản phẩm. Tên thuộc tính C# (`Id`, `Name`) thành camelCase trong JSON (`id`, `name`); `List` thành mảng JSON; `decimal` thành số (không có dấu nháy).

### Bước 4: GET /api/products/{id:int}

| Lệnh | Kết quả thật | Ai trả lời |
|------|--------------|------------|
| `GET /api/products/1` | 200 OK + `{"id":1,"name":"Giao trinh Lap trinh phan tan",...}` | Handler: tìm thấy |
| `GET /api/products/999` | 404 + `{"code":"PRODUCT_NOT_FOUND","message":"Khong tim thay san pham."}` | Handler: chạy, không tìm thấy, tự trả 404 có body |
| `GET /api/products/abc` | 404, `Content-Length: 0` | Framework: `abc` không phải số nên không khớp tuyến, handler không chạy |
| `POST /api/products` (lúc chưa có MapPost) | 405 Method Not Allowed, `Allow: GET` | Framework: URI có nhưng chưa có động từ POST |

1. Vai trò của `:int` trong `{id:int}`?
   `:int` là ràng buộc tuyến: chỉ khớp khi đoạn đó là số nguyên. `/api/products/2` khớp, ASP.NET đổi chữ "2" thành số 2 rồi đưa vào tham số `int id`. `/api/products/abc` không khớp tuyến nào nên framework trả 404 luôn, handler không bị gọi.
2. Cả `/999` và `/abc` đều 404. Khi viết máy khách, phân biệt hai trường hợp bằng gì?
   Bằng body. `/999` do handler trả nên có body `ApiError` với `code = PRODUCT_NOT_FOUND`, nghĩa là URI đúng nhưng sản phẩm không tồn tại. `/abc` do framework trả nên body rỗng (`Content-Length: 0`), nghĩa là gọi sai đường dẫn. Máy khách đọc mã trước, nếu 404 thì xem body có `code` hay không.

### Bước 5: POST /api/products
Dùng 2 DTO: `ProductDto(Id, Name, Description, Price)` cho dữ liệu đi ra và `CreateProductRequest(Name, Description?, Price)` cho dữ liệu đi vào. DTO đầu vào không có `Id` vì Id do máy chủ sinh (`products.Max(p => p.Id) + 1`), máy khách không được tự chọn để tránh hai người gửi trùng số.

Body lấy từ tệp `sp-hop-le.json` và `sp-sai.json` để tránh lỗi dấu nháy trong PowerShell:
```
curl.exe -i -X POST http://localhost:5100/api/products -H "Content-Type: application/json" -d "@sp-hop-le.json"
curl.exe -i -X POST http://localhost:5100/api/products -H "Content-Type: application/json" -d "@sp-sai.json"
curl.exe -i -X POST http://localhost:5100/api/products -d "@sp-hop-le.json"
```

| Ca | Kết quả thật | Ai trả lời |
|----|--------------|------------|
| ① Hợp lệ | 201 Created, `Location: /api/products/4`, `{"id":4,"name":"Lap trinh gRPC",...}` | Handler: Id = 3 + 1 = 4 |
| ② Giá âm | 400 Bad Request + `{"code":"INVALID_PRODUCT",...}` | Handler: khối `if` từ chối |
| ③ Thiếu Content-Type | 415 Unsupported Media Type, body rỗng | Framework: không biết đọc body là gì nên chặn trước handler |

3. 400 khác 415 ở chỗ ai từ chối?
   415 do framework từ chối ở trạm "đọc body": thiếu header `Content-Type: application/json` nên ASP.NET không biết đổi body thành `CreateProductRequest`, handler không chạy, không có `ApiError`. 400 `INVALID_PRODUCT` do code tự viết trong handler từ chối: body đã đọc được nhưng sai luật nghiệp vụ (tên rỗng hoặc giá ≤ 0), body có `ApiError` nói rõ lỗi gì. Nhìn mã là biết lỗi ở đâu: 404/405 sai URI hoặc phương thức, 415 quên header, 400 rỗng là JSON hỏng, 400 có `ApiError` là dữ liệu sai luật.

<img width="1920" height="1140" alt="{5F25302A-ABFB-4B6D-A1B4-B4284574E21C}" src="https://github.com/user-attachments/assets/8b49d891-12d4-47e3-ad36-778599d70f2f" />

## 3. requests.http (Bước 6)
Tệp `CatalogApi/requests.http` dùng biến `@api = http://localhost:5100`, mỗi ca ngăn bằng `###`, tiêu đề ghi luôn mã mong đợi. Chạy lại máy chủ rồi bấm Send Request lần lượt:

| # | Yêu cầu | Mong đợi | Thực tế |
|---|---------|----------|---------|
| 1 | GET `/` | 200 | 200 |
| 2 | GET `/api/products` | 200, 3 phần tử | 200, 3 phần tử |
| 3 | GET `/api/products/1` | 200 | 200 |
| 4 | GET `/api/products/999` | 404 + ApiError | 404 + `PRODUCT_NOT_FOUND` |
| 5 | GET `/api/products/abc` | 404 rỗng | 404 rỗng |
| 6 | POST hợp lệ | 201 + Location | 201, `Location: /api/products/4` |
| 7 | POST giá âm | 400 + ApiError | 400 + `INVALID_PRODUCT` |
| 8 | POST thiếu Content-Type | 415 | 415 |

200
<img width="1920" height="1140" alt="{0DA18B85-4A2E-49FC-8AF0-AB61503F0BE0}" src="https://github.com/user-attachments/assets/db5f27a9-5ef4-4828-b129-17f7682eebef" />

404
<img width="1920" height="1140" alt="{A2BFC05D-C8BC-4E35-973D-63711E037DB4}" src="https://github.com/user-attachments/assets/07f27fb5-d7ee-423f-9c4f-de355528fc95" />

201
<img width="1920" height="1140" alt="{A65574B8-3522-4F01-B5EC-25F28F0A288F}" src="https://github.com/user-attachments/assets/80ed470a-c371-4206-bd6c-6cbdb2f99cd4" />

400
<img width="1920" height="1140" alt="{3D68F25C-0682-4FA3-A99E-12EBB6F0CCFB}" src="https://github.com/user-attachments/assets/714fdff2-aedf-47d8-9e95-1cc0485b1eee" />

415
<img width="1920" height="1140" alt="{B5F29249-B2C6-4613-BC95-D4C1F59E034A}" src="https://github.com/user-attachments/assets/883d9088-67a9-4634-9282-29ba1a05ddae" />


## 4. Máy khách Catalog.Client (Bước 7–8)
Chạy hệ thống 2 tiến trình: Terminal 1 chạy máy chủ, Terminal 2 chạy
```
dotnet run --project src/Catalog.Client -- http://localhost:5100
```
Kết quả 4 nấc:
```
Danh sach san pham:
  [1] Giao trinh Lap trinh phan tan - 150,000 VND
  [2] Tai lieu RESTful API - 90,000 VND
  [3] Bai tap thuc hanh .NET - 120,000 VND

Da tao san pham moi: [4] Nhap mon REST API
Doc lai tai: /api/products/4

Chi tiet san pham 1: Giao trinh Lap trinh phan tan

Thu gui du lieu sai: 400 BadRequest
INVALID_PRODUCT: Ten san pham khong duoc de trong va gia phai lon hon 0.
```

<img width="1920" height="1140" alt="{E9C4A1D2-0D81-4197-A91C-41E3E220D9E2}" src="https://github.com/user-attachments/assets/af55c7be-ef00-40e8-a563-b09bf120e9f9" />

1. Vì sao máy khách phải khai báo lại `ProductDto`?
   Vì máy chủ và máy khách là 2 chương trình riêng, `Catalog.Client.csproj` không tham chiếu tới `Catalog.Api`. Hai bên không dùng chung code mà chỉ chung hợp đồng là JSON đi qua mạng. Máy khách cần một kiểu có tên thuộc tính khớp JSON (`id`, `name`, `description`, `price`) để `GetFromJsonAsync<List<ProductDto>>` đổi JSON thành đối tượng C#. Tương tự máy khách cũng khai báo lại `CreateProductRequest` và `ApiError`.
2. Vì sao địa chỉ máy chủ truyền qua `-- http://localhost:5100`?
   Gọi qua mạng thì máy khách phải biết máy chủ ở đâu. Mọi thứ sau `--` được chuyển vào `args`, nên `args[0]` là địa chỉ máy chủ; không truyền thì dùng mặc định `http://localhost:5100`. Đổi cổng hoặc đổi máy chỉ cần đổi tham số, không phải sửa code.
3. Vì sao phải đọc mã trạng thái trước khi đọc body ở nấc 2?
   Vì body có 2 hình dạng khác nhau: 201 thì body là `ProductDto`, mã khác thì body là `ApiError`. Máy khách kiểm tra `createResponse.StatusCode == HttpStatusCode.Created` rồi mới chọn kiểu để `ReadFromJsonAsync`. Ở nấc 4 cố ý gửi `("", "", -1)` để chứng minh máy khách đọc được lỗi 400 thành câu dễ hiểu, không bị sập.

## 5. Thí nghiệm phân tán (Bước 9)

| # | Dự đoán | Thực tế | Đúng/Sai | Bài học |
|---|---------|---------|----------|---------|
| 1<br>Chạy máy khách lần 2 (máy chủ vẫn chạy) | Danh sách có thêm sản phẩm vừa tạo ở lần 1 | Danh sách có 4 sản phẩm (`[4] Nhap mon REST API`), tạo mới ra `[5]`, `Doc lai tai: /api/products/5` | Đúng | Trạng thái nằm ở máy chủ, không ở máy khách. Code máy khách không đổi nhưng chạy 2 lần thấy dữ liệu khác, vì danh sách `products` sống trong bộ nhớ của tiến trình `Catalog.Api`. |
| 2<br>Tắt máy chủ (Ctrl+C), chạy máy khách | Máy khách báo lỗi không kết nối được | Dừng ngay ở nấc 1: `Unhandled exception. System.Net.Http.HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:5100)` | Đúng | Lỗi chỉ có ở hệ phân tán: bên kia có thể vắng mặt. Không có tiến trình nào nghe ở cổng 5100 nên máy từ chối kết nối. |
| 3<br>Chạy lại máy chủ rồi GET `/api/products` | Sản phẩm 4, 5 mất, còn 3 sản phẩm mẫu | Chỉ còn 3 sản phẩm `id` 1, 2, 3 | Đúng | Dữ liệu trong bộ nhớ mất khi tiến trình dừng. Muốn giữ lại phải lưu vào CSDL hoặc tệp. |
| 4<br>Máy khách gọi sai cổng `-- http://localhost:5999` | Lỗi kết nối giống thí nghiệm 2 | `HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:5999)` | Đúng | Hai bên phải thống nhất địa chỉ (URL). Máy chủ vẫn chạy ở 5100 nhưng máy khách gọi 5999 thì không tới được. |

### Thử thách: gọi qua mạng khác gọi hàm ở những điểm nào?
- **A. Vì sao `LaySanPham(2)` không bao giờ "Connection refused" mà `GET /api/products/2` thì có thể?**
  Hàm nằm cùng tiến trình nên lúc nào cũng có, trình biên dịch đã kiểm tra là hàm tồn tại. Máy chủ là tiến trình khác, có thể chưa chạy, đã tắt, chạy sai cổng hoặc mạng bị đứt (thí nghiệm 2 và 4). Bên kia có thể vắng mặt.
- **B. Máy khách chạy 2 lần thấy dữ liệu khác nhau dù code không đổi. Dữ liệu "sống" ở đâu?**
  Trong bộ nhớ của tiến trình `Catalog.Api` (biến `products`). Máy khách chỉ hỏi, ai hỏi cũng thấy cùng một dữ liệu (thí nghiệm 1). Trạng thái nằm ở máy chủ.
- **C. Máy khách khai báo `record ProductDto(int Id, string Ten, ...)` thì có lỗi biên dịch không? Chạy ra gì?**
  Không lỗi biên dịch, vì 2 project không tham chiếu nhau nên trình biên dịch không biết JSON có trường gì. Khi chạy, `Ten` luôn `null` vì JSON có `"name"` chứ không có `"ten"`, máy khách in ra tên rỗng. Hợp đồng không được trình biên dịch kiểm tra, hai bên chỉ "hứa" với nhau.

## 6. Ba câu hỏi giải thích
1. Vì sao URI dùng danh từ?
   URI chỉ tài nguyên, còn hành động do phương thức HTTP đảm nhận. Trong code chỉ có một URI `/api/products` (danh từ số nhiều, "tập hợp sản phẩm"): `GET /api/products` là đọc danh sách, `POST /api/products` là thêm một sản phẩm vào tập hợp, `GET /api/products/1` là đọc một phần tử. Không cần URI kiểu `/getProducts` hay `/createProduct`, vì động từ đã nằm ở GET/POST.
2. "Stateless" thể hiện ở đâu?
   Mỗi yêu cầu tự mang đủ thông tin để máy chủ xử lý: `GET /api/products/1` có id ngay trên URI, `POST /api/products` có đủ `name`, `description`, `price` trong body. Máy chủ không nhớ phiên hay "cuộc hội thoại" của máy khách giữa 2 lần gọi; máy khách gọi lần 2 không cần gửi gì từ lần 1. Stateless không có nghĩa là máy chủ không lưu dữ liệu: danh sách `products` vẫn được lưu, chỉ là không lưu trạng thái phiên của từng máy khách.
3. Vì sao mỗi bên khai báo DTO riêng?
   DTO là hợp đồng JSON, không phải lớp dùng chung. `Catalog.Client` không tham chiếu project `Catalog.Api`, hai bên khớp nhau nhờ tên trường và kiểu dữ liệu trong JSON (`id`, `name`, `description`, `price`). Nhờ vậy máy khách viết bằng ngôn ngữ khác cũng gọi được API. Ngoài ra dữ liệu vào và ra là 2 hợp đồng khác nhau nên máy chủ dùng 2 DTO: `CreateProductRequest` không có `Id`, `ProductDto` có `Id` do máy chủ sinh.

## 7. Phiếu tự kiểm tra (Bước 10)

| # | Yêu cầu | Mong đợi | Tự đánh giá |
|---|---------|----------|-------------|
| 1 | GET `/` | 200 | ✓ |
| 2 | GET `/api/products` | 200, 3 phần tử | ✓ |
| 3 | GET `/api/products/1` | 200 | ✓ |
| 4 | GET `/api/products/999` | 404 + ApiError | ✓ |
| 5 | GET `/api/products/abc` | 404 rỗng | ✓ |
| 6 | POST hợp lệ | 201 + Location | ✓ |
| 7 | POST giá âm | 400 + ApiError | ✓ |
| 8 | POST thiếu Content-Type | 415 | ✓ |
| 9 | Máy khách chạy | đủ 4 nấc | ✓ |
| 10 | Tắt máy chủ, chạy máy khách | lỗi kết nối | ✓ |

## 8. Khó khăn gặp phải và cách giải quyết
Không có.
