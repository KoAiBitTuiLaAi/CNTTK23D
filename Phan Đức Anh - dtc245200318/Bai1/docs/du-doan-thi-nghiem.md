# Dự đoán trước khi chạy thí nghiệm

Các dự đoán sau được ghi trước lần chạy script kiểm thử đầu tiên.

| Thí nghiệm | Dự đoán |
| --- | --- |
| a | Thêm bài tập thứ ba trên API thì client cũ vẫn đọc và in được cả ba vì nhận mảng JSON và duyệt danh sách. |
| b | Tắt API thì GET đầu tiên thất bại với HttpRequestException do kết nối bị từ chối. Chưa tới bước POST. |
| c | Mã sinh viên rỗng làm POST trả 400. EnsureSuccessStatusCode ném HttpRequestException. |
| d | API ở 5050 nhưng client gọi 5000 thì lỗi kết nối. Truyền http://localhost:5050 qua đối số sẽ chạy lại được mà không sửa code. |
