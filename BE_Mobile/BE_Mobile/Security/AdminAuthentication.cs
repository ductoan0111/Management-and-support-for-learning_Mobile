using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace BE_Mobile.Security;

public sealed record AdminTokenPayload(long UserId, string PasswordStamp);

public sealed class AdminTokenService(IDataProtectionProvider provider)
{
    public const string Scheme = "AdminBearer";
    private readonly ITimeLimitedDataProtector protector = provider.CreateProtector("BE_Mobile.Admin.AccessToken.v1").ToTimeLimitedDataProtector();
    public static string Stamp(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
    public string Issue(AdminUserCredentials credentials, DateTimeOffset expiresAt) =>
        protector.Protect(JsonSerializer.Serialize(new AdminTokenPayload(credentials.User.UserId, Stamp(credentials.PasswordHash))), expiresAt);
    public AdminTokenPayload? Read(string token) => JsonSerializer.Deserialize<AdminTokenPayload>(protector.Unprotect(token));
}

public sealed class AdminAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, AdminTokenService tokens, IAdminUserRepository users)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();
        AdminTokenPayload? payload;
        try { payload = tokens.Read(header[7..].Trim()); }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException or ArgumentException)
        {
            return AuthenticateResult.Fail("Invalid or expired access token.");
        }
        if (payload is null) return AuthenticateResult.Fail("Invalid access token.");
        var credentials = await users.CredentialsAsync(payload.UserId, null, Context.RequestAborted);
        if (credentials is null || !credentials.User.IsActive || payload.PasswordStamp != AdminTokenService.Stamp(credentials.PasswordHash))
            return AuthenticateResult.Fail("Account is inactive or credentials have changed.");
        var user = credentials.User;
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.RoleCode)
        ], AdminTokenService.Scheme);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AdminTokenService.Scheme));
    }
}
