using BE_Mobile.Contracts.Auth;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Đăng nhập hệ thống (Sinh viên, Giảng viên, Admin).</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthUserDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Đăng ký tài khoản sinh viên.</summary>
    [HttpPost("register-student")]
    public async Task<ActionResult<AuthUserDto>> RegisterStudent(
        [FromBody] RegisterStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterStudentAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}
