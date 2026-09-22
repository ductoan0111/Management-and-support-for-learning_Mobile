using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminTeacherRepository
{
    Task<PagedResult<AdminTeacherDto>> ListAsync(AdminTeacherQuery query, CancellationToken cancellationToken);
    Task<AdminTeacherDto?> GetAsync(long id, CancellationToken cancellationToken);
    Task<AdminTeacherDto?> SaveAsync(long? id, SaveAdminTeacherRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}
