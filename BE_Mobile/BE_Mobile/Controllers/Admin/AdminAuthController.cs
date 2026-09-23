using BE_Mobile.Contracts.Admin;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController(IAdminUserRepository users, AdminTokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("admin-login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var credentials = await users.CredentialsAsync(null, request.Username.Trim(), cancellationToken);
        var valid = false;
        if (credentials is not null)
        {
            try
            {
                valid = new PasswordHasher<object>().VerifyHashedPassword(this, credentials.PasswordHash, request.Password)
                    != PasswordVerificationResult.Failed;
            }
            catch (FormatException) { }
        }
        if (!valid || credentials is null || !credentials.User.IsActive || credentials.User.RoleCode != "ADMIN")
            return Unauthorized(new { message = "Invalid administrator credentials." });
        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        Response.Headers.CacheControl = "no-store";
        return Ok(new { accessToken = tokens.Issue(credentials, expiresAt), tokenType = "Bearer", expiresAt, user = credentials.User });
    }
}
