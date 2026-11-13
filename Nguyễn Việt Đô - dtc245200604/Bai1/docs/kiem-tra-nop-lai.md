# Kiểm tra lại Bài 1 sau khi sửa cấu trúc

- Sinh viên: Nguyễn Việt Đô – DTC245200604 – CNTT K23D.
- Ngày kiểm tra: 11/11/2026.
- Thư mục chạy: `Nguyễn Việt Đô - dtc245200604/Bai1/`.

## Các nội dung sửa theo nhận xét

1. Đặt `EduSubmit.slnx`, `src/Submission.Api`, `src/Submission.Client`, `requests.http`, `docs`, `samples` và `tests` trong `Bai1/`.
2. Giữ bảng đủ bốn thí nghiệm a–d tại mục 5 của [nhật ký](bai1.md#5-thí-nghiệm-lỗi-bước-10).
3. Giữ sáu request trong `requests.http`: GET dịch vụ, GET danh sách, POST hợp lệ, POST mã sinh viên rỗng, GET lượt nộp vừa tạo, GET ID không tồn tại.
4. Sửa tên `READNE.md` thành `README.md`; README ngoài thư mục bài dẫn vào đúng tài liệu và mã nguồn.

## Kết quả thực chạy

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build` tại `Bai1/` | Thành công, 0 cảnh báo, 0 lỗi |
| `dotnet build EduSubmit.slnx --nologo` | Thành công, 0 cảnh báo, 0 lỗi |
| `python tests/test_edusubmit.py --label review-fix` | 14/14 kiểm thử tích hợp đạt |
| Sáu request chính | 200, 200, 201, 400, 200, 404 |
| Client mặc định và URL tùy chỉnh | Nộp bài thành công, in GUID và `received`, đọc lại dữ liệu khớp |
| Kiểm tra dữ liệu đầu vào | Từ chối mã sinh viên rỗng, bài tập không tồn tại và URL không hợp lệ |
| 36 lượt tạo/đọc đồng thời | ID khác nhau, dữ liệu đọc lại khớp |
| HelloPtudpt | Build/chạy thành công, in `Hello, World!` |
| MepDemo | Chạy mặc định và chế độ 1/3/4; mỗi công việc có một worker nhận, sự kiện tới đủ ba dịch vụ |
| BasicWebApi | Build và kiểm tra root/vi/en/ja, POST 201 và POST lỗi 400 đạt |
| MiniDistributedSystem | Build thành công trước và sau bốn thí nghiệm |

## Chạy lại thí nghiệm lỗi

Dự đoán được ghi trước khi chạy. Mỗi thí nghiệm thay một điều kiện và khôi phục mã nguồn sau khi kiểm tra:

| Mã | Quan sát lại | Kết luận |
|---|---|---|
| a | Thêm bài thứ ba, Client đọc đủ ba bài và nộp thành công mà không sửa Client | Đúng dự đoán |
| b | API dừng: Client báo lỗi kết nối bị từ chối, không có phản hồi HTTP | Đúng dự đoán |
| c | Mã sinh viên rỗng: POST trả 400, `EnsureSuccessStatusCode()` ném ngoại lệ | Đúng dự đoán |
| d | API ở 5050, Client gọi 5000 thất bại; truyền URL 5050 thì nộp thành công, POST trả 201 | Đúng dự đoán |

## Đối chiếu helper của kho lớp

Chạy riêng `cham_chay` trong `.github/scripts/chay_thu.py` trên bài này; chỉ điều chỉnh cleanup tiến trình cho Windows trong chương trình kiểm tra cục bộ, không sửa helper trong repository:

- Build, GET, POST và Client: `chay = 4.0`.
- Đọc lại 200/404 và Client đọc lại: `mr = 1.0`.

Đây là kết quả kiểm tra cục bộ, **không phải điểm chính thức hoặc xác nhận giảng viên đã duyệt**. Ảnh trong nhật ký được giữ từ lần thực hành trước; các mã GUID và thời điểm trong log lần chạy mới đương nhiên khác ảnh cũ.
