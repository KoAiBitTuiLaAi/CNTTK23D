# Nhật ký Bài 3 – SignalR (Đỗ Trọng Dũng – dtc245200730)

Nhật ký đầy đủ (lệnh, kết quả, ảnh, giải thích) ở [../README.md](../README.md). File này tóm tắt tiến trình theo commit và các bảng thí nghiệm.

## Tiến trình theo bước

| Phần | Bước | Commit | Việc | Kết quả thấy được | Ảnh |
|---|---|---|---|---|---|
| PingSignalR | 1 | `init bai 3` | Solution + `Ping.Api`, `Ping.Client`, gói SignalR Client 10.0.15 | Build succeeded, 0 Error(s) | `anh/buoc1-build.png` |
| PingSignalR | 2 | `buoc 2` | `AddSignalR` + `MapHub<PingHub>("/hubs/ping")` | negotiate trả `connectionId` + 3 transport; `GET /hubs/ping` → 400 | `anh/buoc2-negotiate.png` |
| PingSignalR | 3 | `buoc 3` | Hub `Echo` → `Clients.Caller` | Build 0 lỗi, máy chủ chạy cổng 5200 | `anh/buoc3-chay.png` |
| PingSignalR | 4 | `buoc 4` | Máy khách Build → On → Start → Invoke | `[Echo] Echo: Xin chao SignalR tu An`; sai `Ech0` → HubException | `anh/buoc4-echo.png`, `anh/buoc4-sai-ten.png` |
| PingSignalR | 5 | `buoc 5` | `Broadcast` (All), `JoinGroup`/`SendToGroup` (Group) | An thấy tin của Bình trừ `[Echo]` | `anh/buoc5-nhom.png` |
| PingSignalR | 6 | `buoc 6` | `OnConnected/OnDisconnected`, `Reconnecting/Reconnected/Closed` | `Dang ket noi lai...` → `Da ket noi lai` với ConnectionId mới | `anh/buoc6-noilai.png` |
| PingSignalR | 7 | `buoc 7` | 3 lỗi cố ý | refused / 404 negotiate / im lặng | `anh/buoc7-loi1..3.png` |
| EduSubmit | 1 | `buoc 1` | Solution `RealtimeSubmissionSystem` | Build 0 lỗi | `anh/edusubmit-buoc1-build.png` |
| EduSubmit | 2 | `buoc 2` | REST Bài 2 + `UpdatedAt` + `lock` | POST 201 `received`; 400; 404 | `anh/edusubmit-buoc2-post.png` |
| EduSubmit | 3 | `buoc 3` | `SubmissionHub.WatchSubmission` → nhóm `submission:{id}` | negotiate 200; `/hub/submissions` 404 | `anh/edusubmit-buoc3-negotiate.png` |
| EduSubmit | 4 | `buoc 4` | `IHubContext` + chấm nền `Task.Delay` | GET: received → processing → graded; `[Push]` | `anh/edusubmit-buoc4-trangthai.png` |
| EduSubmit | 5 | `buoc 5` | Máy khách 2 kênh, `On<SubmissionStatusMessage>` | `Da ket noi SignalR.` | `anh/edusubmit-buoc5-ketnoi.png` |
| EduSubmit | 6 | `buoc 6` | POST → Watch → chờ → GET | 8 dòng, 2 thông báo cách 2 giây, REST `graded` | `anh/edusubmit-buoc6-client.png` |
| EduSubmit | 7 | `buoc 7` | 5 thí nghiệm phân tán | Xem bảng dưới | `anh/edusubmit-buoc7-tn1..5.png` |
| EduSubmit | 8 | `buoc 8` | `requests.http` 14 ca, bài tập 2 + 3, trả lời câu hỏi | 15/15 lần gọi đạt; `failed` + cảnh báo đỏ; 2 lượt nộp không trộn | `anh/edusubmit-buoc8-*.png`, `anh/buoc8-kiemtra-nop.png` |

## Thí nghiệm lỗi PingSignalR (bước 7)

| # | Thí nghiệm | Dự đoán | Kết quả thật | Giải thích |
|---|---|---|---|---|
| 1 | Máy chủ chưa chạy | Lỗi kết nối | `HttpRequestException` actively refused (localhost:5200) | `StartAsync` ném lỗi ở lần nối đầu; tự kết nối lại chỉ áp dụng cho kết nối đã mở |
| 2 | `WithUrl` sai `/hub/ping` | 404 | `HttpRequestException ... 404` tại `NegotiateAsync` | Đường dẫn phải khớp `MapHub` |
| 3 | `On("EchoRespone")` | Im lặng | Không lỗi, không có dòng `[Echo]` | Sai tên chiều xuống không báo lỗi |
| 4 | `InvokeAsync("Ech0")` | Lỗi | `HubException: Method does not exist` | Sai tên chiều lên báo lỗi ngay |

## Thí nghiệm phân tán EduSubmit (bước 7)

| # | Thí nghiệm | Dự đoán | Kết quả thật | Giải thích |
|---|---|---|---|---|
| 1 | SV001 và SV002 cùng lúc | Mỗi bên chỉ thấy lượt của mình | Đúng, không trộn tin | Nhóm `submission:{id}` tách người nhận |
| 2 | Máy chủ chưa chạy | Lỗi kết nối | `HttpRequestException` actively refused (localhost:5000) | Lần nối đầu không tự thử lại |
| 3 | Sai `/hub/submissions` | 404 | `HttpRequestException ... 404` tại negotiate | Khớp `MapHub("/hubs/submissions")` |
| 4 | Sai tên `On("SubmissionStatusChange")` | Không nhận tin, không lỗi | Hết 10 giây: `Qua thoi gian cho thong bao graded.`; REST `graded` | Sai tên chiều xuống = im lặng; REST cứu kết quả |
| 5 | Watch muộn 4 giây | Lỡ hết tin | Không nhận tin nào; REST `graded` | Vào nhóm muộn là lỡ tin; push để nhanh + REST để chắc |
