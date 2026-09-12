using BE_Mobile.Contracts.Teachers;

namespace BE_Mobile.Repositories.Interfaces;

public interface ITeacherSectionRepository
{
    Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(
        long sectionId,
        CancellationToken cancellationToken);
}
