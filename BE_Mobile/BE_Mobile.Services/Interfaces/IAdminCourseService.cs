using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
namespace BE_Mobile.Services.Interfaces;
public interface IAdminCourseService
{
    Task<OperationResult<PagedResult<AdminCourseDto>>> ListAsync(AdminCourseQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminCourseDto>> GetAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<AdminCourseDto>> SaveAsync(int? id, SaveAdminCourseRequest request, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
