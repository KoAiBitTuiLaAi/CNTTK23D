# EduSubmit Bài 1
Đồng Minh Duy — DTC245200873 — CNTT K23D

## Chạy bài cá nhân
Yêu cầu .NET 10 SDK. Mở terminal tại thư mục này.

```powershell
dotnet build EduSubmit.slnx -m:1 -p:UseSharedCompilation=false
dotnet run --project src/Submission.Api --urls http://localhost:5000
```
Terminal thứ hai:
```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```
File requests.http chứa thử 200/201/400/404. Chạy createSubmission trước request đọc lại.
Dữ liệu trong bộ nhớ, mất sau restart. Client không tham chiếu API.

## Mẫu thực hành
samples chứa bản mẫu giảng viên để đối chiếu, BasicWebApi đã bổ sung tiếng Nhật. Các mẫu không thuộc solution EduSubmit, chạy theo README của từng mẫu. Bốn thí nghiệm đã thực hiện trên mẫu MiniDistributedSystem và hoàn tác; xem docs/logs và nhật ký.

## Nhật ký và ảnh
Đọc docs/bai1.md. Ảnh MEP 3 đã chèn; các ảnh còn lại cần sinh viên tự chụp. Chưa xác nhận thực hành debug hoặc hoàn tất nộp bài.

## Nộp bằng Fork và PR
Fork: https://github.com/stickman006/CNTTK23D
Kho lớp: https://github.com/PTUDPTK23/CNTTK23D
Đặt nội dung dự án trong Đồng Minh Duy - dtc245200873/Bai1, giữ nguyên tên thư mục cá nhân có sẵn.
Không upload bin, obj hoặc .git; .gitignore bỏ qua các thư mục build kể cả trong samples.
Tiêu đề: Bai1 - DTC245200873 - Đồng Minh Duy
Nếu đã có PR, cập nhật fork để PR tự cập nhật, không tạo PR mới.
Sau khi hoàn thiện ảnh, commit và tạo tag bai1 theo hướng dẫn Word. Khi làm qua web, có thể dùng giao diện tạo release/tag của fork hoặc Git.


## Nguồn ảnh và kết quả tham khảo
Ảnh và log được dùng lại từ bài của Tô Văn Thịnh (DTC245200374), không xác nhận người mang tên trong bản sao đã tự thực hiện. Cần tự chạy và thay ảnh cá nhân trước khi nộp. Link stickman006 là fork nguồn, cần đổi sang fork người nộp.

