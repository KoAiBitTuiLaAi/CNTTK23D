# BasicWebApi

Chương trình được dựng theo đặc tả Bước 8 vì thư mục đầu vào chỉ có MepDemo.

```powershell
dotnet build
dotnet run --project src/BasicWebApi --urls http://localhost:5081
```

Mở `requests.http`, bấm Send Request. Đã thêm `language=ja`.
Đặt breakpoint tại `var greeting = service.Create(...)` để xem request POST.
