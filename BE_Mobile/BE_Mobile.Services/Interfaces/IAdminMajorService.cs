using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminMajorService
{
    Task<OperationResult<PagedResult<AdminMajorDto>>> ListAsync(AdminMajorQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminMajorDto>> GetAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<AdminMajorDto>> SaveAsync(int? id, SaveAdminMajorRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
