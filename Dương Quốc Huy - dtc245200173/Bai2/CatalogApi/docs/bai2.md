# Bài 2 - RESTful API: thực hành CatalogApi

Họ tên: Dương Quốc Huy          MSSV: dtc245200173          Lớp: CNTT K23D

## 1. Tạo solution và môi trường - Bước 1

Solution `CatalogApi.slnx` có hai project .NET 10: `Catalog.Api` và
`Catalog.Client`. Hai project không tham chiếu nhau, chỉ trao đổi HTTP + JSON.
`.gitignore` loại các thư mục `bin/` và `obj/`.

Kết quả build thực tế ngày 13/11/2026: **0 warning, 0 error**.

## 2. Chạy khung API - Bước 2

Chạy các lệnh trong thư mục `CatalogApi` bằng terminal VS Code:

```powershell
dotnet build
dotnet run --project src/Catalog.Api --urls http://localhost:5100
```

Ở terminal khác:

```powershell
curl.exe -i http://localhost:5100/
```

Kết quả thực tế: 200 OK, body `{"service":"Catalog.Api","status":"running"}`.

## 3. Danh sách và chi tiết sản phẩm - Bước 3 và 4

| Request | Kết quả thực tế | Giải thích |
|---|---|---|
| GET `/api/products` | 200, mảng ba sản phẩm lúc khởi động | Danh sách nằm trong bộ nhớ API. |
| GET `/api/products/1` | 200, sản phẩm số 1 | Handler tìm thấy sản phẩm. |
| GET `/api/products/999` | 404 + `PRODUCT_NOT_FOUND` | Handler chạy nhưng không tìm thấy dữ liệu. |
| GET `/api/products/abc` | 404, body rỗng | Không khớp route có ràng buộc `:int`; handler không chạy. |
| PUT `/api/products/1` | 405 | Có URI chi tiết nhưng không hỗ trợ PUT. |

`ProductDto` gồm `Id`, `Name`, `Description`, `Price`. Khi chuyển sang JSON,
tên trường dùng camelCase: `id`, `name`, `description`, `price`.

## 4. Tạo sản phẩm - Bước 5

```powershell
curl.exe -i -X POST http://localhost:5100/api/products -H "Content-Type: application/json" -d "@sp-hop-le.json"
curl.exe -i -X POST http://localhost:5100/api/products -H "Content-Type: application/json" -d "@sp-sai.json"
curl.exe -i -X POST http://localhost:5100/api/products -d "@sp-hop-le.json"
```

| Ca | Dự đoán | Thực tế | Đúng/Sai | Giải thích |
|---|---|---|---|---|
| Body hợp lệ | 201 + Location | Client lần 1 nhận sản phẩm 4, Location `/api/products/4` | Đúng | Server tự sinh Id và tạo sản phẩm. |
| Giá âm | 400 | 400 + `INVALID_PRODUCT` | Đúng | Handler kiểm tra giá phải lớn hơn 0. |
| Tên rỗng | 400 | 400 + `INVALID_PRODUCT` | Đúng | Handler không chấp nhận tên rỗng. |
| Thiếu Content-Type JSON | 415 | 415, body rỗng trong lần kiểm thử | Đúng | Framework từ chối trước handler. |

DTO đầu vào `CreateProductRequest` không có `Id`. `Description` có thể null;
API dùng chuỗi rỗng nếu không nhận mô tả. API dùng `Trim()` trước khi lưu.

## 5. Bộ request - Bước 6

`requests.http` có 10 request: 8 request ban đầu và 2 ca bổ sung `/abc` cùng
PUT sai phương thức. Bao phủ 200, 201, 400, 404, 405, 415. Mở bằng REST Client
trong VS Code rồi bấm **Send Request**. API phải chạy tại cổng 5100.

## 6. Catalog.Client - Bước 7 và 8

```powershell
dotnet run --project src/Catalog.Client -- http://localhost:5100
```

Kết quả thực tế ở lần chạy đầu:

```text
Danh sach san pham:
  [1] Giao trinh Lap trinh phan tan -150.000 VND
  [2] Tai lieu RESTful API -90.000 VND
  [3] Bai tap thuc hanh .NET -120.000 VND

Da tao san pham moi: [4] Nhap mon REST API
Doc lai tai: /api/products/4

Chi tiet san pham 1: Giao trinh Lap trinh phan tan

Thu gui du lieu sai: 400 BadRequest
INVALID_PRODUCT: Ten san pham khong duoc de trong va gia phai lon hon 0.
```

Client kiểm tra mã 201 trước khi đọc `ProductDto`. Khi cố ý gửi sai, Client đọc
`ApiError` và kết thúc bình thường. Theo mẫu Catalog, nấc 3 đọc sản phẩm số 1;
Client có in Location của sản phẩm vừa tạo để quan sát hợp đồng HTTP.

## 7. Bảng thí nghiệm phân tán - Bước 9

Các thí nghiệm sau đã được chạy trên API cổng 5100 và Client thực tế.

| # | Dự đoán | Kết quả thực tế | Đúng/Sai | Bài học |
|---|---|---|---|---|
| 1. Chạy Client lần hai, API vẫn chạy | Thấy 4 sản phẩm, tạo thêm sản phẩm 5 | Danh sách có sản phẩm 4; POST mới tạo Id 5, Location `/api/products/5` | Đúng | Trạng thái ở tiến trình API, không nằm trong Client. |
| 2. Tắt API, chạy Client | Lỗi kết nối cổng 5100 | `HttpRequestException`, bên trong là `SocketException (10061)`, thông báo kết nối bị từ chối tại localhost:5100 | Đúng | Tiến trình bên kia có thể vắng mặt; không có HTTP response. |
| 3. Khởi động lại API | Còn 3 sản phẩm mẫu | GET danh sách trả đúng 3 sản phẩm; GET sản phẩm 4 trả 404 `PRODUCT_NOT_FOUND` | Đúng | List trong RAM mất khi API dừng. |
| 4. Gọi cổng 5999 | Lỗi kết nối | Client báo `HttpRequestException` do kết nối bị từ chối tại localhost:5999 | Đúng | URL dịch vụ là cấu hình, cần thống nhất địa chỉ và cổng. |

Client Catalog giữ hành vi lỗi mạng theo mẫu bài học: ngoại lệ chưa được bắt khi
API tắt hoặc sai cổng. Đây là kết quả thí nghiệm có chủ ý, khác trường hợp HTTP
400 đã được Client đọc bằng `ApiError`.

## 8. Trả lời câu hỏi thực hành

1. **Vì sao gọi hàm không bị Connection refused?** Hàm nội bộ nằm trong cùng
   tiến trình, còn HTTP phụ thuộc một tiến trình máy chủ và kết nối mạng.
2. **Dữ liệu sống ở đâu?** Biến `products` trong bộ nhớ `Catalog.Api`; Client
   chỉ nhận bản biểu diễn JSON của dữ liệu khi gửi request.
3. **Đổi DTO Client từ Name thành Ten thì sao?** Code vẫn có thể biên dịch,
   nhưng JSON có `name` không khớp `Ten`, nên trường này nhận null. Hợp đồng
   qua mạng không được trình biên dịch C# kiểm tra giữa hai project.
4. **400 khác 415 thế nào?** 400 `INVALID_PRODUCT` là handler từ chối dữ liệu
   nghiệp vụ; 415 là framework từ chối kiểu nội dung trước khi gọi handler.
5. **Vì sao hai kiểu 404 có body khác nhau?** `/999` đi vào handler và trả
   `ApiError`; `/abc` không khớp ràng buộc route nên framework trả 404 rỗng.

## 9. Ảnh minh chứng

Chưa chụp ảnh riêng cho CatalogApi. Có thể dùng ba ảnh gọn:

- Ảnh build và cây thư mục hai project.
- Ảnh Client đủ bốn nấc, API cổng 5100.
- Ảnh phản hồi lỗi 404 có body, 404 rỗng và 415.

Chỉ chèn ảnh chụp CatalogApi; ảnh Submission ở cổng 5000 thuộc phần EduSubmit.

## 10. Git và tự kiểm tra - Bước 10

Lịch sử commit được tạo hiện tại từ mã bài đã hoàn thành, chia theo các bước
thực hành để dễ xem thay đổi; không thay đổi ngày giờ commit.

| Mục kiểm tra | Kết quả |
|---|---|
| GET gốc | 200 |
| GET danh sách khi mới khởi động | 200, 3 phần tử |
| GET sản phẩm 1 | 200 |
| GET sản phẩm 999 | 404 + ApiError |
| GET sản phẩm abc | 404 rỗng |
| POST hợp lệ | 201 + Location |
| POST giá âm | 400 + ApiError |
| POST thiếu Content-Type JSON | 415 |
| Client đủ 4 nấc | Đạt |
| Tắt API rồi chạy Client | Lỗi kết nối đúng dự đoán |

Các commit sử dụng tên Dương Quốc Huy, email `dtc245200173@ictu.edu.vn`.
`bin/` và `obj/` không được Git theo dõi. Chưa push GitHub.
