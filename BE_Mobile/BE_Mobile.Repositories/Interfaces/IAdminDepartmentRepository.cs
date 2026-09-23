using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminDepartmentRepository
{
    Task<PagedResult<AdminDepartmentDto>> ListAsync(AdminDepartmentQuery query, CancellationToken cancellationToken);
    Task<AdminDepartmentDto?> GetAsync(int id, CancellationToken cancellationToken);
    Task<AdminDepartmentDto?> SaveAsync(int? id, SaveAdminDepartmentRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
