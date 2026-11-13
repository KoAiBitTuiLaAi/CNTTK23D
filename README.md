# CNTT K23D

**Trường Đại học Công nghệ Thông tin & Truyền thông - Đại học Thái Nguyên**

Thư mục nộp bài tập và đồ án của sinh viên lớp **Phát triển ứng dụng phân tán** (K23.K1.D2.2627.CNTT K23D).

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
