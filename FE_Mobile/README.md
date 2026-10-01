# Study Support Mobile

Ứng dụng mobile dùng cho sinh viên và giảng viên. Admin được tách riêng sang `../FE_Admin_Web`.

## Chạy app

```bash
npm install
npm run web
```

Hoặc chạy trên thiết bị/emulator:

```bash
npm run android
npm run ios
```

## Cấu trúc chính

- `app/(auth)`: route đăng nhập và đăng ký.
- `app/student`: route dành cho sinh viên.
- `app/teacher`: route dành cho giảng viên.
- `features/auth`: màn hình xác thực.
- `features/student`: màn hình nghiệp vụ sinh viên.
- `features/teacher`: màn hình nghiệp vụ giảng viên.
- `components`: component dùng chung.
- `constants/theme.ts`: màu sắc và style dùng chung.

## Luồng vai trò

- Sinh viên đăng nhập vào `/student`.
- Giảng viên đăng nhập vào `/teacher`.
- Admin dùng giao diện web riêng trong `FE_Admin_Web`.

## Giảng viên và API

- Chọn vai trò Giảng viên khi đăng nhập. Tài khoản cần có RoleCode TEACHER và hồ sơ Teachers được admin tạo.
- Trang chủ, lớp học phần, lịch dạy, sinh viên và hồ sơ lấy dữ liệu API thật.
- Trong lớp học phần: quản lý tài liệu bằng URL, bài tập, bài nộp/chấm điểm, bảng điểm theo thành phần và thông báo.
- `api/client.ts`: địa chỉ API, timeout, lỗi và token. `api/teacher.ts`: các yêu cầu giảng viên.
- `features/teacher/components`: hộp thoại, biểu mẫu, bảng điểm, bài nộp; `config/resourceForms.ts`: cấu hình biểu mẫu.
- API chưa có endpoint upload tệp: tài liệu và bài tập dùng đường dẫn HTTP/HTTPS.
- Phiên đăng nhập hiện lưu trong bộ nhớ; khởi động lại ứng dụng cần đăng nhập lại. Không tự đăng nhập demo khi mất mạng.

Chạy backend từ thư mục gốc bằng `dotnet run --project BE_Mobile/BE_Mobile --launch-profile http`.
Ứng dụng tự lấy IP máy chạy Expo và cổng 5113. Điện thoại và máy tính cần cùng mạng và cổng backend phải truy cập được.
Có thể đặt `EXPO_PUBLIC_API_URL` trong `.env.local` theo `.env.example`, rồi khởi động lại Expo.
IIS Express ở localhost:62225 chỉ phù hợp thử trên trình duyệt máy tính; trên điện thoại dùng địa chỉ LAN với backend profile http.
Để chỉ đổi API cho bản web, đặt `EXPO_PUBLIC_WEB_API_URL=http://localhost:62225` trong `.env.local`. Biến này không ảnh hưởng Android/iOS; `EXPO_PUBLIC_API_URL` vẫn dùng để cấu hình chung.

```bash
npx tsc --noEmit
npx expo export --platform web
```

Lưu ý backend hiện có: token của `/api/auth/login` chưa được xác thực bởi các controller giảng viên và dữ liệu seed còn hỗ trợ mật khẩu demo. Cần hoàn thiện xác thực/phân quyền server trước khi triển khai công khai.
