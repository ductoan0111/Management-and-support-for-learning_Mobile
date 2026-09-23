using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminAcademicClassService(IAdminAcademicClassRepository repository) : IAdminAcademicClassService
{
    public Task<OperationResult<PagedResult<AdminAcademicClassDto>>> ListAsync(AdminAcademicClassQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminAcademicClassDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminAcademicClassDto>> GetAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminAcademicClassDto>> SaveAsync(int? id, SaveAdminAcademicClassRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
