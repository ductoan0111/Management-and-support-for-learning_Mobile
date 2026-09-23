using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminDepartmentService(IAdminDepartmentRepository repository) : IAdminDepartmentService
{
    public Task<OperationResult<PagedResult<AdminDepartmentDto>>> ListAsync(AdminDepartmentQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminDepartmentDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminDepartmentDto>> GetAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminDepartmentDto>> SaveAsync(int? id, SaveAdminDepartmentRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
