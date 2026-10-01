using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BE_Mobile.Contracts.Auth;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BE_Mobile.Security;

public static class AppAuthenticationDefaults
{
    public const string Scheme = "AppBearer";
    public const string StudentIdClaim = "student_id";
    public const string TeacherIdClaim = "teacher_id";
}

public sealed class AppAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthRepository users)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = header[7..].Trim();
        if (!AuthTokenCodec.TryReadPayload(token, out var payload))
            return AuthenticateResult.Fail("Invalid access token.");

        var user = await users.GetTokenUserAsync(payload.UserId, Context.RequestAborted);
        if (user is null || !user.IsActive)
            return AuthenticateResult.Fail("Account is inactive or no longer exists.");

        if (!AuthTokenCodec.Validate(token, user.PasswordHash, DateTimeOffset.UtcNow, out _))
            return AuthenticateResult.Fail("Invalid or expired access token.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Identifier),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleCode),
            new("role_id", user.RoleId.ToString(CultureInfo.InvariantCulture)),
            new("role_name", user.RoleName)
        };

        if (user.StudentId is { } studentId)
            claims.Add(new Claim(AppAuthenticationDefaults.StudentIdClaim, studentId.ToString(CultureInfo.InvariantCulture)));

        if (user.TeacherId is { } teacherId)
            claims.Add(new Claim(AppAuthenticationDefaults.TeacherIdClaim, teacherId.ToString(CultureInfo.InvariantCulture)));

        var identity = new ClaimsIdentity(claims, AppAuthenticationDefaults.Scheme);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AppAuthenticationDefaults.Scheme));
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TeacherRouteGuardAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.RouteData.Values.TryGetValue("teacherId", out var routeValue)
            || !long.TryParse(Convert.ToString(routeValue, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out var routeTeacherId))
        {
            context.Result = new BadRequestObjectResult(new { message = "Teacher route is invalid." });
            return;
        }

        var claimValue = context.HttpContext.User.FindFirst(AppAuthenticationDefaults.TeacherIdClaim)?.Value;
        if (!long.TryParse(claimValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tokenTeacherId)
            || tokenTeacherId != routeTeacherId)
        {
            context.Result = new ForbidResult(AppAuthenticationDefaults.Scheme);
            return;
        }

        await next();
    }
}
