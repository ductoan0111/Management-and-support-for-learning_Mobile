using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Repositories.Interfaces;

public sealed record AdminUserCredentials(AdminUserDto User, string PasswordHash);

public interface IAdminUserRepository
{
    Task<PagedResult<AdminUserDto>> ListAsync(AdminUserQuery query, CancellationToken cancellationToken);
    Task<AdminUserDto?> GetAsync(long id, CancellationToken cancellationToken);
    Task<AdminUserCredentials?> CredentialsAsync(long? id, string? username, CancellationToken cancellationToken);
    Task<AdminUserDto?> CreateAsync(CreateAdminUserRequest request, string passwordHash, CancellationToken cancellationToken);
    Task<AdminUserDto?> UpdateAsync(long id, UpdateAdminUserRequest request, CancellationToken cancellationToken);
    Task<AdminUserDto?> SetRoleAsync(long id, byte roleId, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(long id, string passwordHash, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminRoleDto>> RolesAsync(CancellationToken cancellationToken);
}
