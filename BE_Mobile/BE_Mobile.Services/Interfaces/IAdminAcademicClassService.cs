using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminAcademicClassService
{
    Task<OperationResult<PagedResult<AdminAcademicClassDto>>> ListAsync(AdminAcademicClassQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminAcademicClassDto>> GetAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<AdminAcademicClassDto>> SaveAsync(int? id, SaveAdminAcademicClassRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
