using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminTeacherService(IAdminTeacherRepository repository) : IAdminTeacherService
{
    public Task<OperationResult<PagedResult<AdminTeacherDto>>> ListAsync(AdminTeacherQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminTeacherDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminTeacherDto>> GetAsync(long id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminTeacherDto>> SaveAsync(long? id, SaveAdminTeacherRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(long id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
