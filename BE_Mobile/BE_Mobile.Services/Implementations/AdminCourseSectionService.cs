using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminCourseSectionService(IAdminCourseSectionRepository repository) : IAdminCourseSectionService
{
    public Task<OperationResult<PagedResult<AdminCourseSectionDto>>> ListAsync(AdminCourseSectionQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminCourseSectionDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminCourseSectionDto>> GetAsync(long id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminCourseSectionDto>> SaveAsync(long? id, SaveAdminCourseSectionRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(long id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
