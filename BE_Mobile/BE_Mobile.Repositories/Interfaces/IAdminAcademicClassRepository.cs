using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminAcademicClassRepository
{
    Task<PagedResult<AdminAcademicClassDto>> ListAsync(AdminAcademicClassQuery query, CancellationToken cancellationToken);
    Task<AdminAcademicClassDto?> GetAsync(int id, CancellationToken cancellationToken);
    Task<AdminAcademicClassDto?> SaveAsync(int? id, SaveAdminAcademicClassRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
