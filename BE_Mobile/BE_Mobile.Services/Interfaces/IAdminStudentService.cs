using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Services.Interfaces;

public interface IAdminStudentService
{
    Task<OperationResult<PagedResult<AdminStudentDto>>> GetStudentsAsync(
        string? search,
        byte? status,
        int? majorId,
        int? academicClassId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<OperationResult<AdminStudentDto>> GetStudentAsync(long studentId, CancellationToken cancellationToken);

    Task<OperationResult<AdminStudentDto>> GetStudentByUserAsync(long userId, CancellationToken cancellationToken);

    Task<OperationResult<AdminStudentDto>> CreateStudentAsync(
        CreateAdminStudentRequest request,
        CancellationToken cancellationToken);

    Task<OperationResult<AdminStudentDto>> UpdateStudentAsync(
        long studentId,
        UpdateAdminStudentRequest request,
        CancellationToken cancellationToken);

    Task<OperationResult> DeleteStudentAsync(long studentId, CancellationToken cancellationToken);
}
