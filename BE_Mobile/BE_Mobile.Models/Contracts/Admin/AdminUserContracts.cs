using System.ComponentModel.DataAnnotations;

namespace BE_Mobile.Contracts.Admin;

public sealed record AdminUserDto(long UserId, byte RoleId, string RoleCode, string Username,
    string Email, string FullName, string? Phone, bool IsActive);
public sealed record AdminRoleDto(byte RoleId, string RoleCode, string RoleName, string? Description);

public class UpdateAdminUserRequest
{
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
    [StringLength(20)] public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CreateAdminUserRequest : UpdateAdminUserRequest
{
    [Required, RegularExpression(@"[a-zA-Z0-9_.@-]{3,100}")] public string Username { get; set; } = string.Empty;
    [Range(1, 255)] public byte RoleId { get; set; }
    [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = string.Empty;
}

public sealed class SetAdminRoleRequest
{
    [Range(1, 255)] public byte RoleId { get; set; }
}

public sealed class ResetAdminPasswordRequest
{
    [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = string.Empty;
}

public sealed class AdminLoginRequest
{
    [Required, StringLength(100)] public string Username { get; set; } = string.Empty;
    [Required, StringLength(128)] public string Password { get; set; } = string.Empty;
}

public sealed class AdminUserQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 255)] public byte? RoleId { get; set; }
    public bool? IsActive { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
}
