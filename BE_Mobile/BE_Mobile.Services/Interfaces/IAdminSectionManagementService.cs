using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Services.Interfaces;

public interface IAdminSectionManagementService
{
    Task<OperationResult<PagedResult<AdminSectionTeacherDto>>> TeachersAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminSectionTeacherDto>> AssignTeacherAsync(long sectionId, long teacherId, AssignAdminTeacherRequest request, CancellationToken cancellationToken);
    Task<OperationResult> RemoveTeacherAsync(long sectionId, long teacherId, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<AdminEnrollmentDto>>> StudentsAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken);
    Task<OperationResult<AdminEnrollmentDto>> EnrollAsync(long sectionId, long studentId, SaveAdminEnrollmentRequest request, CancellationToken cancellationToken);
    Task<OperationResult> CancelEnrollmentAsync(long sectionId, long studentId, CancellationToken cancellationToken);
    Task<OperationResult<AdminStatisticsDto>> StatisticsAsync(CancellationToken cancellationToken);
}
