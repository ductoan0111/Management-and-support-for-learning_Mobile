# FE Admin Web

Giao diện React + TypeScript dành cho quản trị viên. Ứng dụng có màn đăng nhập admin, gửi Bearer token cho API và hỗ trợ quản lý các danh mục chính: tài khoản, sinh viên, giảng viên, khoa, ngành, lớp hành chính, môn học, học kỳ và lớp học phần.

## Chạy giao diện

```bash
npm install
npm run dev
```

## Build

```bash
npm run build
```

Mặc định web admin gọi backend ở `http://localhost:5113`. Nếu backend chạy ở địa chỉ khác, tạo file `.env` và đặt:

```bash
VITE_API_BASE_URL=http://localhost:5113
```

Backend admin cần chạy trước khi đăng nhập:

```bash
dotnet run --project ../BE_Mobile/BE_Mobile --launch-profile http
```
