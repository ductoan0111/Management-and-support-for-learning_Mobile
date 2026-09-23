using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminCourseRepository
{
    Task<PagedResult<AdminCourseDto>> ListAsync(AdminCourseQuery query, CancellationToken cancellationToken);
    Task<AdminCourseDto?> GetAsync(int id, CancellationToken cancellationToken);
    Task<AdminCourseDto?> SaveAsync(int? id, SaveAdminCourseRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
