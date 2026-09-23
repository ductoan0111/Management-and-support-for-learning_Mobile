# Admin API

API dùng schema hiện có trong `Management and academic support.sql`, không cần thêm bảng.
Các module đi theo cấu trúc Contracts -> Controller -> Service -> Repository.

## Khởi tạo và đăng nhập

Dữ liệu SQL mẫu dùng `test-password-hash`, chưa phải mật khẩu đăng nhập hợp lệ.
Chạy lệnh sau từ thư mục gốc để khởi tạo tài khoản admin. Lệnh chỉ chấp nhận
tài khoản mới hoặc tài khoản ADMIN mẫu chưa được đổi mật khẩu.

```powershell
$credential = Get-Credential -UserName admin01 -Message 'Admin password: at least 12 characters'
$env:BootstrapAdmin__Username = $credential.UserName
$env:BootstrapAdmin__Password = $credential.GetNetworkCredential().Password
$env:BootstrapAdmin__Email = 'admin01@school.edu.vn'
dotnet run --project BE_Mobile/BE_Mobile -- --bootstrap-admin
Remove-Item Env:BootstrapAdmin__Password
```

Chạy API bằng `dotnet run --project BE_Mobile/BE_Mobile --launch-profile http`.
Swagger: `http://localhost:5113/swagger`.

1. Gọi `POST /api/admin/auth/login` với `username` và `password`.
2. Lấy `accessToken` trong response và nhập vào nút **Authorize** trên Swagger.
3. Frontend gửi header `Authorization: Bearer <accessToken>` trong mọi request admin.

Token có hiệu lực 8 giờ, là token được bảo vệ bằng ASP.NET Core Data Protection,
không phải JWT. Khóa tài khoản hoặc đổi mật khẩu sẽ làm token cũ không sử dụng được;
quyền được đọc lại từ database mỗi request. Khi triển khai nhiều instance, cần dùng
chung kho Data Protection keys. Login giới hạn 10 request/phút/IP.

API trả `401` khi chưa đăng nhập/token hết hạn và `403` khi không có quyền ADMIN.
Frontend admin hiện tại cần bổ sung đăng nhập và gửi Bearer token, kể cả API sinh viên cũ.
CORS development cho phép `http://localhost:5173` và `http://localhost:5174`;
cấu hình môi trường khác qua `Cors:AllowedOrigins`.

## Nhóm endpoint

Các tài nguyên sau hỗ trợ `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}`:

| Prefix | Chức năng | Bộ lọc bổ sung |
| --- | --- | --- |
| `/api/admin/courses` | Môn học | `departmentId`, `isActive` |
| `/api/admin/semesters` | Học kỳ | `isCurrent` |
| `/api/admin/course-sections` | Lớp học phần | `courseId`, `semesterId`, `status` |
| `/api/admin/teachers` | Hồ sơ giảng viên | `departmentId`, `status` |
| `/api/admin/departments` | Khoa | `isActive` |
| `/api/admin/majors` | Ngành | `departmentId`, `isActive` |
| `/api/admin/academic-classes` | Lớp hành chính | `majorId`, `isActive` |
| `/api/admin/students` | Hồ sơ sinh viên (API có sẵn) | `majorId`, `academicClassId`, `status` |

Danh sách có `search`, `page`, `pageSize` (mặc định 1/20, tối đa 100 bản ghi/trang).
Response: `{ items, page, pageSize, totalCount, totalPages }`.
Các danh mục này dùng trực tiếp cho dropdown; frontend có thể tìm kiếm/phân trang.
PUT nhận đầy đủ dữ liệu của tài nguyên, trừ API cập nhật sinh viên giữ hợp đồng cũ.

| Method | Endpoint | Chức năng |
| --- | --- | --- |
| GET/POST | `/api/admin/users` | Danh sách/tạo tài khoản |
| GET/PUT | `/api/admin/users/{id}` | Chi tiết/cập nhật, khóa hoặc mở qua `isActive` |
| PUT | `/api/admin/users/{id}/role` | Gán vai trò, body `{ "roleId": 1 }` |
| PUT | `/api/admin/users/{id}/password` | Đặt mật khẩu mới, body `{ "password": "..." }` |
| GET | `/api/admin/roles` | Các vai trò có sẵn trong database |
| GET | `/api/admin/statistics` | Tổng tài khoản, sinh viên, giảng viên, môn, lớp, học kỳ, lượt đăng ký |
| GET | `/api/admin/course-sections/{sectionId}/teachers` | Giảng viên của lớp, có phân trang |
| PUT | `/api/admin/course-sections/{sectionId}/teachers/{teacherId}` | Phân công/cập nhật, body `{ "isPrimary": true }` |
| DELETE | `/api/admin/course-sections/{sectionId}/teachers/{teacherId}` | Bỏ phân công |
| GET | `/api/admin/course-sections/{sectionId}/students` | Sinh viên của lớp, có phân trang, gồm cả đăng ký đã hủy |
| PUT | `/api/admin/course-sections/{sectionId}/students/{studentId}` | Đăng ký/cập nhật, body `{ "status": 1 }` |
| DELETE | `/api/admin/course-sections/{sectionId}/students/{studentId}` | Hủy đăng ký, giữ lại bản ghi và điểm |

Danh sách users hỗ trợ `search`, `roleId`, `isActive`, `page`, `pageSize`.
Không xóa cứng tài khoản; dùng `isActive = false`. RoleId phải lấy từ API roles,
không giả định ID cố định. Các mã quyền hiện tại là ADMIN, TEACHER và STUDENT.

## Luồng tạo dữ liệu

Tạo tài khoản qua users trước, rồi dùng `userId` tạo hồ sơ teacher/student với vai trò
tương ứng. Hai bước là hai request riêng; nếu tạo hồ sơ thất bại, tài khoản vẫn tồn tại
để admin sửa và thử lại. Hồ sơ giảng viên không được chuyển sang tài khoản khác.

Tạo khoa -> ngành -> lớp hành chính cho sinh viên. Tạo môn học và học kỳ -> lớp học phần
-> phân công giảng viên -> thêm sinh viên.

Ví dụ tạo lớp học phần:

```json
{
  "courseId": 1,
  "semesterId": 1,
  "sectionCode": "KTPM-01",
  "sectionName": "Ky thuat phan mem - Nhom 01",
  "maxStudents": 40,
  "status": 1
}
```

Lớp học phần: `status` 0 đóng, 1 mở, 2 kết thúc. Đăng ký: 0 hủy, 1 đang học, 2 hoàn thành.
Giảng viên: 0 ngưng công tác, 1 đang công tác. Số tín chỉ từ 1 đến 15.

## Ràng buộc

- Đặt học kỳ hiện tại sẽ bỏ cờ hiện tại của các học kỳ khác trong cùng transaction.
- Một lớp có tối đa một giảng viên chính qua API phân công.
- Đăng ký mới chỉ vào lớp mở, với sinh viên đang học và tài khoản hoạt động.
- Không vượt sĩ số, kể cả đăng ký đồng thời; không giảm sĩ số xuống dưới số đăng ký chưa hủy.
- Không đổi môn/học kỳ của lớp đã có phân công hoặc đăng ký.
- Vai trò phải khớp hồ sơ; lớp hành chính phải thuộc ngành của sinh viên.
- Không khóa hoặc đổi vai trò của admin hoạt động cuối cùng.
- Mật khẩu mới dài 12–128 ký tự, được băm bằng ASP.NET Core PasswordHasher, không trả hash trong API.
- Trùng mã/email/tài khoản, khóa ngoại và xung đột nghiệp vụ trả `409`; dữ liệu đầu vào sai trả `400`; không tìm thấy trả `404`.
- Xóa danh mục/hồ sơ có dữ liệu phụ thuộc trả `409`, không tự xóa dữ liệu liên quan.

## Biên dịch

```powershell
dotnet build BE_Mobile/BE_Mobile.slnx
```
