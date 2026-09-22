using BE_Mobile.Contracts.Auth;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;

namespace BE_Mobile.Services.Implementations;

public sealed class AuthService(IAuthRepository authRepository) : IAuthService
{
    public async Task<OperationResult<AuthUserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
            return OperationResult<AuthUserDto>.BadRequest("Vui lòng nhập tài khoản hoặc email.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return OperationResult<AuthUserDto>.BadRequest("Vui lòng nhập mật khẩu.");

        var user = await authRepository.LoginAsync(request, cancellationToken);
        if (user is null)
            return OperationResult<AuthUserDto>.BadRequest("Tài khoản hoặc mật khẩu không chính xác.");

        // Nếu request có chỉ định role ("student", "teacher", "admin"), kiểm tra hợp lệ
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var requestedRole = request.Role.Trim().ToLowerInvariant();
            if (requestedRole == "student" && user.StudentId is null && !user.RoleCode.Equals("STUDENT", StringComparison.OrdinalIgnoreCase))
                return OperationResult<AuthUserDto>.BadRequest("Tài khoản này không phải là tài khoản Sinh viên.");

            if (requestedRole == "teacher" && user.TeacherId is null && !user.RoleCode.Equals("TEACHER", StringComparison.OrdinalIgnoreCase))
                return OperationResult<AuthUserDto>.BadRequest("Tài khoản này không phải là tài khoản Giảng viên.");
        }

        return OperationResult<AuthUserDto>.Success(user);
    }

    public async Task<OperationResult<AuthUserDto>> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return OperationResult<AuthUserDto>.BadRequest("Vui lòng nhập tên đăng nhập.");

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return OperationResult<AuthUserDto>.BadRequest("Email không hợp lệ.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return OperationResult<AuthUserDto>.BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return OperationResult<AuthUserDto>.BadRequest("Vui lòng nhập họ và tên.");

        if (string.IsNullOrWhiteSpace(request.StudentCode))
            return OperationResult<AuthUserDto>.BadRequest("Vui lòng nhập mã sinh viên.");

        var exists = await authRepository.ExistsUsernameOrEmailAsync(request.Username, request.Email, cancellationToken);
        if (exists)
            return OperationResult<AuthUserDto>.Conflict("Tên đăng nhập hoặc email đã tồn tại trên hệ thống.");

        var user = await authRepository.RegisterStudentAsync(request, cancellationToken);
        if (user is null)
            return OperationResult<AuthUserDto>.Problem("Không thể tạo tài khoản sinh viên. Vui lòng thử lại sau.");

        return OperationResult<AuthUserDto>.Success(user);
    }
}
