# CatalogApi - Bài 2

Dương Quốc Huy - dtc245200173 - CNTT K23D

Nhật ký thực hành, kết quả kiểm thử, bảng thí nghiệm và câu trả lời:
[docs/bai2.md](docs/bai2.md).

Trong terminal VS Code tại thư mục này:

```powershell
dotnet build
dotnet run --project src/Catalog.Api --urls http://localhost:5100
```

Terminal thứ hai:

```powershell
dotnet run --project src/Catalog.Client -- http://localhost:5100
```

`requests.http` có 10 request cho REST Client; API phải đang chạy khi gửi.
Các mã kiểm thử gồm 200, 201, 400, 404, 405 và 415.

Đã build thành công và chạy đủ bốn thí nghiệm phân tán. Ảnh minh chứng Catalog
chưa được chụp; các vị trí cần ảnh được ghi rõ trong nhật ký.

Xem lịch sử theo bước bằng `git log --oneline --reverse`. Commit sử dụng email
`dtc245200173@ictu.edu.vn` và được tạo tại thời điểm hoàn thiện hồ sơ. Không theo
dõi `bin/`, `obj/`; chưa push GitHub.
