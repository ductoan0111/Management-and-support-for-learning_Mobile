using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Repositories.Interfaces;
public interface IAdminMajorRepository
{
    Task<PagedResult<AdminMajorDto>> ListAsync(AdminMajorQuery query, CancellationToken cancellationToken);
    Task<AdminMajorDto?> GetAsync(int id, CancellationToken cancellationToken);
    Task<AdminMajorDto?> SaveAsync(int? id, SaveAdminMajorRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
