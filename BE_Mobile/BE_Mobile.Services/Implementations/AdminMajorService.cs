using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
namespace BE_Mobile.Services.Implementations;
public sealed class AdminMajorService(IAdminMajorRepository repository) : IAdminMajorService
{
    public Task<OperationResult<PagedResult<AdminMajorDto>>> ListAsync(AdminMajorQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminMajorDto>>(async () => await repository.ListAsync(query, cancellationToken), query);
    public Task<OperationResult<AdminMajorDto>> GetAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.GetAsync(id, cancellationToken));
    public Task<OperationResult<AdminMajorDto>> SaveAsync(int? id, SaveAdminMajorRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.SaveAsync(id, request, cancellationToken), request);
    public Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.DeleteAsync(id, cancellationToken));
}
