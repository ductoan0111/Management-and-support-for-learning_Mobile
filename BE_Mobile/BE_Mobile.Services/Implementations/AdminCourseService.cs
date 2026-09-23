using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminCourseService(IAdminCourseRepository repository) : IAdminCourseService
{
    public Task<OperationResult<PagedResult<AdminCourseDto>>> ListAsync(AdminCourseQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminCourseDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminCourseDto>> GetAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminCourseDto>> SaveAsync(int? id, SaveAdminCourseRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
