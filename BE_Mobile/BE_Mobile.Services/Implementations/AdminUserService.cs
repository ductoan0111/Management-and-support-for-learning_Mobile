using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BE_Mobile.Services.Implementations;

public sealed class AdminUserService(IAdminUserRepository repository) : IAdminUserService
{
    private readonly PasswordHasher<object> hasher = new();
    public Task<OperationResult<PagedResult<AdminUserDto>>> ListAsync(AdminUserQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminUserDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminUserDto>> GetAsync(long id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminUserDto>> CreateAsync(CreateAdminUserRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.CreateAsync(request, hasher.HashPassword(this, request.Password), cancellationToken), request);
    public Task<OperationResult<AdminUserDto>> UpdateAsync(long id, UpdateAdminUserRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.UpdateAsync(id, request, cancellationToken), request);
    public Task<OperationResult<AdminUserDto>> SetRoleAsync(long id, SetAdminRoleRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SetRoleAsync(id, request.RoleId, cancellationToken), request);
    public async Task<OperationResult> ResetPasswordAsync(long id, ResetAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await AdminOperation.RunAsync<bool?>(async () =>
            await repository.ResetPasswordAsync(id, hasher.HashPassword(this, request.Password), cancellationToken) ? true : null, request);
        return new(result.Error);
    }
    public Task<OperationResult<IReadOnlyList<AdminRoleDto>>> RolesAsync(CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<IReadOnlyList<AdminRoleDto>>(async () => await repository.RolesAsync(cancellationToken));
}
