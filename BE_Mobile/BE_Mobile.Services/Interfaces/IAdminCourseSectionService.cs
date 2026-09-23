using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminCourseSectionService
{
    Task<OperationResult<PagedResult<AdminCourseSectionDto>>> ListAsync(AdminCourseSectionQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminCourseSectionDto>> GetAsync(long id, CancellationToken cancellationToken);
    Task<OperationResult<AdminCourseSectionDto>> SaveAsync(long? id, SaveAdminCourseSectionRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(long id, CancellationToken cancellationToken);
}
