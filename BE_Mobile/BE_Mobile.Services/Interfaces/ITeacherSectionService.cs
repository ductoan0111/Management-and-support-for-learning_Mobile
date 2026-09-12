using BE_Mobile.Contracts.Teachers;

namespace BE_Mobile.Services.Interfaces;

public interface ITeacherSectionService
{
    Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(
        long sectionId,
        CancellationToken cancellationToken);
}
