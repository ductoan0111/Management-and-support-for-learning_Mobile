using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Services.Interfaces;

public interface IAdminUserService
{
    Task<OperationResult<PagedResult<AdminUserDto>>> ListAsync(AdminUserQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminUserDto>> GetAsync(long id, CancellationToken cancellationToken);
    Task<OperationResult<AdminUserDto>> CreateAsync(CreateAdminUserRequest request, CancellationToken cancellationToken);
    Task<OperationResult<AdminUserDto>> UpdateAsync(long id, UpdateAdminUserRequest request, CancellationToken cancellationToken);
    Task<OperationResult<AdminUserDto>> SetRoleAsync(long id, SetAdminRoleRequest request, CancellationToken cancellationToken);
    Task<OperationResult> ResetPasswordAsync(long id, ResetAdminPasswordRequest request, CancellationToken cancellationToken);
    Task<OperationResult<IReadOnlyList<AdminRoleDto>>> RolesAsync(CancellationToken cancellationToken);
}
