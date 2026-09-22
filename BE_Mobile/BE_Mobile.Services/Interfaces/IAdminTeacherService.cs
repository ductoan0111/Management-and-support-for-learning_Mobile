using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminTeacherService
{
    Task<OperationResult<PagedResult<AdminTeacherDto>>> ListAsync(AdminTeacherQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminTeacherDto>> GetAsync(long id, CancellationToken cancellationToken);
    Task<OperationResult<AdminTeacherDto>> SaveAsync(long? id, SaveAdminTeacherRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(long id, CancellationToken cancellationToken);
}
