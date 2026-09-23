using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminSemesterRepository
{
    Task<PagedResult<AdminSemesterDto>> ListAsync(AdminSemesterQuery query, CancellationToken cancellationToken);
    Task<AdminSemesterDto?> GetAsync(int id, CancellationToken cancellationToken);
    Task<AdminSemesterDto?> SaveAsync(int? id, SaveAdminSemesterRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
