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
