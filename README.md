# CNTT K23D

**Trường Đại học Công nghệ Thông tin & Truyền thông - Đại học Thái Nguyên**

Thư mục nộp bài tập và đồ án của sinh viên lớp **Phát triển ứng dụng phân tán** (K23.K1.D2.2627.CNTT K23D).

## 📢 Thông báo – Hạn sửa Bài 1, Bài 2 (cập nhật 17/11/2026)

1. Chỉ chấm những bạn **đã có Pull Request (PR)**. Bạn nào chưa nộp PR thì không chấm.
2. PR đang bị "Changes requested" hoặc có nhận xét lỗi: đọc lý do cô ghi ở tab **Conversation**, sửa trong fork của em rồi **commit + push thêm vào đúng nhánh của PR đó**. **Không mở PR mới, không đóng PR cũ** (PR mới nộp sau hạn sẽ bị hệ thống tự đóng).
3. Dòng "Quá hạn" do hệ thống tự gắn chỉ để ghi nhận. Bài sửa đến hạn bên dưới vẫn được chấm, không trừ điểm nộp muộn.
4. **Hạn cuối để push bản sửa: 17h00 ngày 18/11/2026.** Sau giờ này cô không xem thêm.
5. Trước khi push, tự kiểm: bài nằm trong `<Họ tên> - <MSSV>/Bai2/`; không có `bin/`, `obj/` (có `.gitignore`); `requests.http` đủ 7 request có 200, 201, 400, 404; README có lệnh chạy, ảnh/kết quả chạy, bảng phá thử, trả lời 3 câu hỏi.
6. Sau khi push, bình luận 1 dòng trong PR để cô biết đã sửa.

Tối 18/11 (sau 17h00) cô duyệt và chấm lần cuối cho Bài 1, Bài 2, sau đó chuyển sang Bài 3. Bài 3 sẽ có hạn nộp cố định, rõ ràng, không nhắc và không chấm lại.


## Hướng dẫn nộp bài (Fork + Pull Request)
> Sinh viên chỉ có quyền **xem** repo này, **không push trực tiếp** được. Mọi bài nộp đều gửi qua **Pull Request (PR)**; giảng viên kiểm tra rồi mới gộp vào.

1. **Fork repo:** bấm nút **Fork** (góc trên bên phải) → **Create fork**. Bạn sẽ có bản sao riêng: `<tài-khoản-github>/CNTTK23D`.
2. **Thêm bài vào fork của mình:** trong thư mục `[Họ và tên] - [Mã sinh viên]` của mình, tạo thư mục bài (`Bai1/`, `BaiTap1/`, `BTL/`, ...) rồi đưa bài vào.
   - Trên web: vào thư mục của mình → **Add file → Upload files** → **Commit changes**.
   - Hoặc dùng git:
     ```bash
     git clone https://github.com/<tài-khoản-github>/CNTTK23D.git
     cd CNTTK23D
     # chép bài vào thư mục "<Họ và tên> - <Mã sinh viên>/Bai1/"
     git add .
     git commit -m "Bai1 - <Mã sinh viên> - <Họ và tên>"
     git push
     ```
3. **Tạo Pull Request:** ở trang fork, bấm **Contribute → Open pull request**. Tiêu đề PR **bắt buộc** theo mẫu: `Bai1 - dtc2452xxxxx - Nguyễn Văn A` (bài 2 thì `Bai2 - ...`). Bấm **Create pull request** là xong.
4. **Sửa / bổ sung bài:** không tạo PR mới, chỉ cần commit thêm vào fork, PR cũ tự cập nhật. Trước khi làm bài mới, bấm **Sync fork** để fork có nội dung mới nhất.

### Ví dụ: tạo thư mục bài và dự án

Giả sử sinh viên **Nguyễn Văn A – dtc245200xxx**. Trong thư mục của mình, mỗi bài/dự án là một thư mục con:

```
Nguyễn Văn A - dtc245200xxx/
├── Bai1/
│   ├── README.md
│   └── (mã nguồn, báo cáo ...)
├── Bai2/
│   └── ...
└── DuAn/
    └── ...
```

Tạo thư mục và nộp bằng git:

```bash
cd "Nguyễn Văn A - dtc245200xxx"
mkdir Bai2          # dự án thì: mkdir DuAn
# chép bài vào Bai2/
git add .
git commit -m "Bai2 - dtc245200xxx - Nguyễn Văn A"
git push
```

Sau đó mở Pull Request với tiêu đề `Bai2 - dtc245200xxx - Nguyễn Văn A`.

### Cập nhật fork bằng git pull (trước khi làm bài mới)

Chỉ làm một lần, để máy biết repo gốc của lớp:

```bash
git remote add upstream https://github.com/PTUDPTK23/CNTTK23D.git
```

Mỗi lần trước khi làm bài mới:

```bash
git checkout main
git pull upstream main    # lấy nội dung mới nhất từ repo của lớp
git push origin main      # đẩy bản đã cập nhật lên fork của bạn
```

Nếu làm trên web thì bấm **Sync fork → Update branch** ở trang fork.

### Quy định
- Chỉ thêm/sửa file **trong thư mục của chính mình**. PR sửa thư mục của bạn khác hoặc file chung (README, .github, ...) sẽ bị từ chối.
- Hệ thống tự gắn nhãn theo thời điểm mở/cập nhật PR: **Đúng hạn**, **Muộn**, **Quá hạn**.
- **Bài 1:** đúng hạn trước **12:00 ngày 06/11/2026**; nộp muộn đến **23:59 ngày 06/11/2026**; sau thời điểm này **đóng link nộp**: PR mở mới sẽ bị đóng tự động.

## Danh sách sinh viên

| STT | Mã sinh viên | Họ và tên | Thư mục nộp bài |
|:---:|--------------|-----------|-----------------|
| 1 | dtc245200393 | Nguyễn Quang An | `Nguyễn Quang An - dtc245200393` |
| 2 | dtc245200117 | Tạ Hoàng Anh | `Tạ Hoàng Anh - dtc245200117` |
| 3 | dtc245200697 | Tạ Tuấn Anh | `Tạ Tuấn Anh - dtc245200697` |
| 4 | dtc245200318 | Phan Đức Anh | `Phan Đức Anh - dtc245200318` |
| 5 | dtc245200932 | Nguyễn Ngọc Anh | `Nguyễn Ngọc Anh - dtc245200932` |
| 6 | dtc245200502 | Nguyễn Tuấn Anh | `Nguyễn Tuấn Anh - dtc245200502` |
| 7 | dtc245200842 | Nguyễn Hoàng Anh | `Nguyễn Hoàng Anh - dtc245200842` |
| 8 | dtc245200405 | Lê Nguyễn Ngọc Ánh | `Lê Nguyễn Ngọc Ánh - dtc245200405` |
| 9 | dtc245200627 | Tạ Hải Đăng | `Tạ Hải Đăng - dtc245200627` |
| 10 | dtc245200724 | Nguyễn Thành Đạt | `Nguyễn Thành Đạt - dtc245200724` |
| 11 | dtc245200604 | Nguyễn Việt Đô | `Nguyễn Việt Đô - dtc245200604` |
| 12 | dtc245200730 | Đỗ Trọng Dũng | `Đỗ Trọng Dũng - dtc245200730` |
| 13 | dtc245200478 | Trần Trung Dũng | `Trần Trung Dũng - dtc245200478` |
| 14 | dtc245200873 | Đồng Minh Duy | `Đồng Minh Duy - dtc245200873` |
| 15 | dtc245200643 | Hà Yến Giang | `Hà Yến Giang - dtc245200643` |
| 16 | dtc245200551 | Nguyễn Xuân Hải | `Nguyễn Xuân Hải - dtc245200551` |
| 17 | dtc245200528 | Hoàng Đức Hiếu | `Hoàng Đức Hiếu - dtc245200528` |
| 18 | dtc245200432 | Ngô Việt Hoàng | `Ngô Việt Hoàng - dtc245200432` |
| 19 | dtc245200103 | Lãnh Thị Huệ | `Lãnh Thị Huệ - dtc245200103` |
| 20 | dtc245200279 | Dương Ngô Hùng | `Dương Ngô Hùng - dtc245200279` |
| 21 | dtc225201515 | Nguyễn Danh Hùng | `Nguyễn Danh Hùng - dtc225201515` |
| 22 | dtc245200387 | Bùi Quang Huy | `Bùi Quang Huy - dtc245200387` |
| 23 | dtc245200822 | Nguyễn Giang Huy | `Nguyễn Giang Huy - dtc245200822` |
| 24 | dtc245200686 | Triệu Văn Huy | `Triệu Văn Huy - dtc245200686` |
| 25 | dtc245200173 | Dương Quốc Huy | `Dương Quốc Huy - dtc245200173` |
| 26 | dtc245200714 | Dương Thu Huyền | `Dương Thu Huyền - dtc245200714` |
| 27 | dtc245200578 | Trần Duy Khánh | `Trần Duy Khánh - dtc245200578` |
| 28 | dtc245200841 | Đinh Văn Khu | `Đinh Văn Khu - dtc245200841` |
| 29 | dtc245200411 | Nguyễn Thị Lam | `Nguyễn Thị Lam - dtc245200411` |
| 30 | dtc245200402 | Vũ Duy Linh | `Vũ Duy Linh - dtc245200402` |
| 31 | dtc245200458 | Nguyễn Kim Thành Long | `Nguyễn Kim Thành Long - dtc245200458` |
| 32 | dtc245200634 | Nguyễn Thành Long | `Nguyễn Thành Long - dtc245200634` |
| 33 | dtc245200678 | Dương Văn Mạnh | `Dương Văn Mạnh - dtc245200678` |
| 34 | dtc245200152 | Nguyễn Đức Mạnh | `Nguyễn Đức Mạnh - dtc245200152` |
| 35 | dtc245200843 | Đinh Hà Ngọc Minh | `Đinh Hà Ngọc Minh - dtc245200843` |
| 36 | dtc245200718 | Thân Đình Nguyên | `Thân Đình Nguyên - dtc245200718` |
| 37 | dtc245200659 | Giáp Phương Nhung | `Giáp Phương Nhung - dtc245200659` |
| 38 | dtc245200722 | Nguyễn Văn Phương | `Nguyễn Văn Phương - dtc245200722` |
| 39 | dtc245200332 | Trần Thị Thu Phương | `Trần Thị Thu Phương - dtc245200332` |
| 40 | dtc245200583 | Nguyễn Thị Quế | `Nguyễn Thị Quế - dtc245200583` |
| 41 | dtc245200620 | Ngô Thị Như Quỳnh | `Ngô Thị Như Quỳnh - dtc245200620` |
| 42 | dtc245200599 | Hoàng Sơn | `Hoàng Sơn - dtc245200599` |
| 43 | dtc245200351 | Lưu Văn Thành | `Lưu Văn Thành - dtc245200351` |
| 44 | dtc245200374 | Tô Văn Thịnh | `Tô Văn Thịnh - dtc245200374` |
| 45 | dtc245200499 | Trần Văn Thưởng | `Trần Văn Thưởng - dtc245200499` |
| 46 | dtc245200314 | Trương Văn Đức Trung | `Trương Văn Đức Trung - dtc245200314` |
| 47 | dtc245200427 | Đồng Anh Tuấn | `Đồng Anh Tuấn - dtc245200427` |
| 48 | dtc245200285 | Đoàn Văn Tuyên | `Đoàn Văn Tuyên - dtc245200285` |

## Hướng dẫn nộp dự án – Phát triển ứng dụng phân tán (CNTT K23D)

### 1. Quy định chung
- Dự án kéo dài **8 tuần**. Mỗi tuần nộp vào một thư mục riêng: `Tuan1`, `Tuan2`, … `Tuan8`.
- Mỗi sinh viên làm **đúng đề tài đã đăng ký**. Tên thư mục dự án ghi trong bảng ở mục 4, **không tự đặt hoặc đổi tên**.
- **Hạn nộp Tuần 1 của lớp K23D: 23h59 thứ Năm 19/11/2026.** Sau hạn này Tuần 1 không nhận bài. Hạn các tuần sau sẽ được thông báo.

### 2. Cấu trúc thư mục
Trong thư mục cá nhân của mình, dự án nằm trong `DuAn/`, bên trong là thư mục tên dự án (theo bảng mục 4), rồi đến từng tuần:

```
Nguyễn Văn A - dtc245200xxx/
└── DuAn/
    └── DT03_HeThongDatVeSuKienPhanTan/     <- tên dự án đúng theo bảng mục 4
        ├── Tuan1/
        │   ├── README.md
        │   └── (mã nguồn, báo cáo ...)
        └── Tuan2/ ...
```

### 3. Cách nộp
1. Tạo thư mục theo đúng đường dẫn ở bảng mục 4, đặt toàn bộ bài làm Tuần 1 vào `Tuan1/`.
2. Commit, push và mở **Pull Request** vào nhánh `main` của repo này (giống cách nộp Bài 1, Bài 2).
3. Đặt tên PR: `DuAn-Tuan1 - MSSV - Họ tên`
4. Mỗi tuần chỉ **một PR**. Giảng viên yêu cầu sửa thì **push thêm commit vào chính PR đó**, không mở PR mới. Việc sửa cũng phải xong trước hạn.
5. Chỉ thêm file trong thư mục của mình. PR sửa thư mục của bạn khác hoặc sai đường dẫn sẽ bị trả lại.

### 4. Tên dự án và thư mục nộp Tuần 1 của từng sinh viên
Sinh viên không có tên trong bảng (chưa đăng ký đề tài) liên hệ giảng viên trước hạn nộp.

| STT | MSSV | Họ và tên | Đề tài | Thư mục nộp Tuần 1 |
|---|---|---|---|---|
| 1 | dtc245200393 | Nguyễn Quang An | 20 – Hệ thống thư viện đặt mượn sách phân tán | `Nguyễn Quang An - dtc245200393/DuAn/DT20_HeThongThuVienDatMuonSachPhanTan/Tuan1/` |
| 2 | dtc245200117 | Tạ Hoàng Anh | 13 – Hệ thống quản lý tồn kho đa dịch vụ | `Tạ Hoàng Anh - dtc245200117/DuAn/DT13_HeThongQuanLyTonKhoDaDichVu/Tuan1/` |
| 3 | dtc245200697 | Tạ Tuấn Anh | 07 – Hệ thống hỗ trợ yêu cầu khách hàng | `Tạ Tuấn Anh - dtc245200697/DuAn/DT07_HeThongHoTroYeuCauKhachHang/Tuan1/` |
| 4 | dtc245200318 | Phan Đức Anh | 08 – Hệ thống theo dõi bưu gửi | `Phan Đức Anh - dtc245200318/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 5 | dtc245200932 | Nguyễn Ngọc Anh | 07 – Hệ thống hỗ trợ yêu cầu khách hàng | `Nguyễn Ngọc Anh - dtc245200932/DuAn/DT07_HeThongHoTroYeuCauKhachHang/Tuan1/` |
| 6 | dtc245200502 | Nguyễn Tuấn Anh | 20 – Hệ thống thư viện đặt mượn sách phân tán | `Nguyễn Tuấn Anh - dtc245200502/DuAn/DT20_HeThongThuVienDatMuonSachPhanTan/Tuan1/` |
| 7 | dtc245200842 | Nguyễn Hoàng Anh | 20 – Hệ thống thư viện đặt mượn sách phân tán | `Nguyễn Hoàng Anh - dtc245200842/DuAn/DT20_HeThongThuVienDatMuonSachPhanTan/Tuan1/` |
| 8 | dtc245200405 | Lê Nguyễn Ngọc Ánh | 12 – Hệ thống phê duyệt tài liệu phân tán | `Lê Nguyễn Ngọc Ánh - dtc245200405/DuAn/DT12_HeThongPheDuyetTaiLieuPhanTan/Tuan1/` |
| 9 | dtc245200627 | Tạ Hải Đăng | 03 – Hệ thống đặt vé sự kiện phân tán | `Tạ Hải Đăng - dtc245200627/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
| 10 | dtc245200724 | Nguyễn Thành Đạt | 03 – Hệ thống đặt vé sự kiện phân tán | `Nguyễn Thành Đạt - dtc245200724/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
| 11 | dtc245200604 | Nguyễn Việt Đô | 13 – Hệ thống quản lý tồn kho đa dịch vụ | `Nguyễn Việt Đô - dtc245200604/DuAn/DT13_HeThongQuanLyTonKhoDaDichVu/Tuan1/` |
| 12 | dtc245200730 | Đỗ Trọng Dũng | 11 – Hệ thống gửi thông báo hàng loạt | `Đỗ Trọng Dũng - dtc245200730/DuAn/DT11_HeThongGuiThongBaoHangLoat/Tuan1/` |
| 13 | dtc245200478 | Trần Trung Dũng | 04 – Hệ thống đặt món và xử lý tại bếp | `Trần Trung Dũng - dtc245200478/DuAn/DT04_HeThongDatMonVaXuLyTaiBep/Tuan1/` |
| 14 | dtc245200873 | Đồng Minh Duy | 12 – Hệ thống phê duyệt tài liệu phân tán | `Đồng Minh Duy - dtc245200873/DuAn/DT12_HeThongPheDuyetTaiLieuPhanTan/Tuan1/` |
| 15 | dtc245200643 | Hà Yến Giang | 12 – Hệ thống phê duyệt tài liệu phân tán | `Hà Yến Giang - dtc245200643/DuAn/DT12_HeThongPheDuyetTaiLieuPhanTan/Tuan1/` |
| 16 | dtc245200551 | Nguyễn Xuân Hải | 07 – Hệ thống hỗ trợ yêu cầu khách hàng | `Nguyễn Xuân Hải - dtc245200551/DuAn/DT07_HeThongHoTroYeuCauKhachHang/Tuan1/` |
| 17 | dtc245200528 | Hoàng Đức Hiếu | 06 – Hệ thống giám sát cảm biến IoT | `Hoàng Đức Hiếu - dtc245200528/DuAn/DT06_HeThongGiamSatCamBienIot/Tuan1/` |
| 18 | dtc245200103 | Lãnh Thị Huệ | 07 – Hệ thống hỗ trợ yêu cầu khách hàng | `Lãnh Thị Huệ - dtc245200103/DuAn/DT07_HeThongHoTroYeuCauKhachHang/Tuan1/` |
| 19 | dtc245200279 | Dương Ngô Hùng | 08 – Hệ thống theo dõi bưu gửi | `Dương Ngô Hùng - dtc245200279/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 20 | dtc225201515 | Nguyễn Danh Hùng | 08 – Hệ thống theo dõi bưu gửi | `Nguyễn Danh Hùng - dtc225201515/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 21 | dtc245200387 | Bùi Quang Huy | 03 – Hệ thống đặt vé sự kiện phân tán | `Bùi Quang Huy - dtc245200387/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
| 22 | dtc245200822 | Nguyễn Giang Huy | 08 – Hệ thống theo dõi bưu gửi | `Nguyễn Giang Huy - dtc245200822/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 23 | dtc245200686 | Triệu Văn Huy | 11 – Hệ thống gửi thông báo hàng loạt | `Triệu Văn Huy - dtc245200686/DuAn/DT11_HeThongGuiThongBaoHangLoat/Tuan1/` |
| 24 | dtc245200173 | Dương Quốc Huy | 03 – Hệ thống đặt vé sự kiện phân tán | `Dương Quốc Huy - dtc245200173/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
| 25 | dtc245200714 | Dương Thu Huyền | 08 – Hệ thống theo dõi bưu gửi | `Dương Thu Huyền - dtc245200714/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 26 | dtc245200578 | Trần Duy Khánh | 08 – Hệ thống theo dõi bưu gửi | `Trần Duy Khánh - dtc245200578/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 27 | dtc245200411 | Nguyễn Thị Lam | 20 – Hệ thống thư viện đặt mượn sách phân tán | `Nguyễn Thị Lam - dtc245200411/DuAn/DT20_HeThongThuVienDatMuonSachPhanTan/Tuan1/` |
| 28 | dtc245200402 | Vũ Duy Linh | 20 – Hệ thống thư viện đặt mượn sách phân tán | `Vũ Duy Linh - dtc245200402/DuAn/DT20_HeThongThuVienDatMuonSachPhanTan/Tuan1/` |
| 29 | dtc245200458 | Nguyễn Kim Thành Long | 12 – Hệ thống phê duyệt tài liệu phân tán | `Nguyễn Kim Thành Long - dtc245200458/DuAn/DT12_HeThongPheDuyetTaiLieuPhanTan/Tuan1/` |
| 30 | dtc245200678 | Dương Văn Mạnh | 03 – Hệ thống đặt vé sự kiện phân tán | `Dương Văn Mạnh - dtc245200678/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
| 31 | dtc245200152 | Nguyễn Đức Mạnh | 04 – Hệ thống đặt món và xử lý tại bếp | `Nguyễn Đức Mạnh - dtc245200152/DuAn/DT04_HeThongDatMonVaXuLyTaiBep/Tuan1/` |
| 32 | dtc245200843 | Đinh Hà Ngọc Minh | 07 – Hệ thống hỗ trợ yêu cầu khách hàng | `Đinh Hà Ngọc Minh - dtc245200843/DuAn/DT07_HeThongHoTroYeuCauKhachHang/Tuan1/` |
| 33 | dtc245200718 | Thân Đình Nguyên | 03 – Hệ thống đặt vé sự kiện phân tán | `Thân Đình Nguyên - dtc245200718/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
| 34 | dtc245200659 | Giáp Phương Nhung | 08 – Hệ thống theo dõi bưu gửi | `Giáp Phương Nhung - dtc245200659/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 35 | dtc245200722 | Nguyễn Văn Phương | 11 – Hệ thống gửi thông báo hàng loạt | `Nguyễn Văn Phương - dtc245200722/DuAn/DT11_HeThongGuiThongBaoHangLoat/Tuan1/` |
| 36 | dtc245200332 | Trần Thị Thu Phương | 08 – Hệ thống theo dõi bưu gửi | `Trần Thị Thu Phương - dtc245200332/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 37 | dtc245200583 | Nguyễn Thị Quế | 07 – Hệ thống hỗ trợ yêu cầu khách hàng | `Nguyễn Thị Quế - dtc245200583/DuAn/DT07_HeThongHoTroYeuCauKhachHang/Tuan1/` |
| 38 | dtc245200620 | Ngô Thị Như Quỳnh | 13 – Hệ thống quản lý tồn kho đa dịch vụ | `Ngô Thị Như Quỳnh - dtc245200620/DuAn/DT13_HeThongQuanLyTonKhoDaDichVu/Tuan1/` |
| 39 | dtc245200599 | Hoàng Sơn | 04 – Hệ thống đặt món và xử lý tại bếp | `Hoàng Sơn - dtc245200599/DuAn/DT04_HeThongDatMonVaXuLyTaiBep/Tuan1/` |
| 40 | dtc245200351 | Lưu Văn Thành | 13 – Hệ thống quản lý tồn kho đa dịch vụ | `Lưu Văn Thành - dtc245200351/DuAn/DT13_HeThongQuanLyTonKhoDaDichVu/Tuan1/` |
| 41 | dtc245200374 | Tô Văn Thịnh | 12 – Hệ thống phê duyệt tài liệu phân tán | `Tô Văn Thịnh - dtc245200374/DuAn/DT12_HeThongPheDuyetTaiLieuPhanTan/Tuan1/` |
| 42 | dtc245200314 | Trương Văn Đức Trung | 08 – Hệ thống theo dõi bưu gửi | `Trương Văn Đức Trung - dtc245200314/DuAn/DT08_HeThongTheoDoiBuuGui/Tuan1/` |
| 43 | dtc245200427 | Đồng Anh Tuấn | 20 – Hệ thống thư viện đặt mượn sách phân tán | `Đồng Anh Tuấn - dtc245200427/DuAn/DT20_HeThongThuVienDatMuonSachPhanTan/Tuan1/` |
| 44 | dtc245200285 | Đoàn Văn Tuyên | 03 – Hệ thống đặt vé sự kiện phân tán | `Đoàn Văn Tuyên - dtc245200285/DuAn/DT03_HeThongDatVeSuKienPhanTan/Tuan1/` |
