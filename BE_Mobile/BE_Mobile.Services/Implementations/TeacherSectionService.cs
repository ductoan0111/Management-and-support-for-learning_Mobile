using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherSectionService(ITeacherSectionRepository teacherSectionRepository) : ITeacherSectionService
{
    public Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        return teacherSectionRepository.GetStudentsBySectionAsync(sectionId, cancellationToken);
    }
}
