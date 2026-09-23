using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminDepartmentService
{
    Task<OperationResult<PagedResult<AdminDepartmentDto>>> ListAsync(AdminDepartmentQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminDepartmentDto>> GetAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<AdminDepartmentDto>> SaveAsync(int? id, SaveAdminDepartmentRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
