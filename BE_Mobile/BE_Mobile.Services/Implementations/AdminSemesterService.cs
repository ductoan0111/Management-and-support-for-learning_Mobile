using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminSemesterService(IAdminSemesterRepository repository) : IAdminSemesterService
{
    public Task<OperationResult<PagedResult<AdminSemesterDto>>> ListAsync(AdminSemesterQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminSemesterDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminSemesterDto>> GetAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminSemesterDto>> SaveAsync(int? id, SaveAdminSemesterRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
