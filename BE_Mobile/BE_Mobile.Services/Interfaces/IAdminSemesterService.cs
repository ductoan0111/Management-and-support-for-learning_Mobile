using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminSemesterService
{
    Task<OperationResult<PagedResult<AdminSemesterDto>>> ListAsync(AdminSemesterQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminSemesterDto>> GetAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<AdminSemesterDto>> SaveAsync(int? id, SaveAdminSemesterRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
