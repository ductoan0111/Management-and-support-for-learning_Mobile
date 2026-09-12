using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Repositories.Interfaces;

public interface IAdminStudentRepository
{
    Task<PagedResult<AdminStudentDto>> GetStudentsAsync(
        string? search,
        byte? status,
        int? majorId,
        int? academicClassId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminStudentDto?> GetStudentAsync(long studentId, CancellationToken cancellationToken);

    Task<AdminStudentDto?> GetStudentByUserAsync(long userId, CancellationToken cancellationToken);

    Task<AdminStudentDto?> CreateStudentAsync(CreateAdminStudentRequest request, CancellationToken cancellationToken);

    Task<AdminStudentDto?> UpdateStudentAsync(
        long studentId,
        UpdateAdminStudentRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteStudentAsync(long studentId, CancellationToken cancellationToken);
}
