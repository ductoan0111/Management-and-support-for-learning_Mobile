using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminCourseSectionRepository
{
    Task<PagedResult<AdminCourseSectionDto>> ListAsync(AdminCourseSectionQuery query, CancellationToken cancellationToken);
    Task<AdminCourseSectionDto?> GetAsync(long id, CancellationToken cancellationToken);
    Task<AdminCourseSectionDto?> SaveAsync(long? id, SaveAdminCourseSectionRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}
