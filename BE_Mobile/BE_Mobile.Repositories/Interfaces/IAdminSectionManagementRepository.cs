using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Repositories.Interfaces;

public interface IAdminSectionManagementRepository
{
    Task<PagedResult<AdminSectionTeacherDto>> TeachersAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken);
    Task<AdminSectionTeacherDto?> AssignTeacherAsync(long sectionId, long teacherId, bool primary, CancellationToken cancellationToken);
    Task<bool> RemoveTeacherAsync(long sectionId, long teacherId, CancellationToken cancellationToken);
    Task<PagedResult<AdminEnrollmentDto>> StudentsAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken);
    Task<AdminEnrollmentDto?> EnrollAsync(long sectionId, long studentId, byte status, CancellationToken cancellationToken);
    Task<bool> CancelEnrollmentAsync(long sectionId, long studentId, CancellationToken cancellationToken);
    Task<AdminStatisticsDto> StatisticsAsync(CancellationToken cancellationToken);
}
