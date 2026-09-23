using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;

namespace BE_Mobile.Services.Implementations;

public sealed class AdminSectionManagementService(IAdminSectionManagementRepository repository) : IAdminSectionManagementService
{
    public Task<OperationResult<PagedResult<AdminSectionTeacherDto>>> TeachersAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminSectionTeacherDto>>(async () => await repository.TeachersAsync(sectionId, query, cancellationToken), query);
    public Task<OperationResult<AdminSectionTeacherDto>> AssignTeacherAsync(long sectionId, long teacherId, AssignAdminTeacherRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.AssignTeacherAsync(sectionId, teacherId, request.IsPrimary, cancellationToken), request);
    public Task<OperationResult> RemoveTeacherAsync(long sectionId, long teacherId, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.RemoveTeacherAsync(sectionId, teacherId, cancellationToken));
    public Task<OperationResult<PagedResult<AdminEnrollmentDto>>> StudentsAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<PagedResult<AdminEnrollmentDto>>(async () => await repository.StudentsAsync(sectionId, query, cancellationToken), query);
    public Task<OperationResult<AdminEnrollmentDto>> EnrollAsync(long sectionId, long studentId, SaveAdminEnrollmentRequest request, CancellationToken cancellationToken) =>
        AdminOperation.RunAsync(() => repository.EnrollAsync(sectionId, studentId, request.Status, cancellationToken), request);
    public Task<OperationResult> CancelEnrollmentAsync(long sectionId, long studentId, CancellationToken cancellationToken) =>
        AdminOperation.DeleteAsync(() => repository.CancelEnrollmentAsync(sectionId, studentId, cancellationToken));
    public Task<OperationResult<AdminStatisticsDto>> StatisticsAsync(CancellationToken cancellationToken) =>
        AdminOperation.RunAsync<AdminStatisticsDto>(async () => await repository.StatisticsAsync(cancellationToken));
}
