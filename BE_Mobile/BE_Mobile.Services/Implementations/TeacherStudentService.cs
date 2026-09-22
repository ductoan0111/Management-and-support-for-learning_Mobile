using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherStudentService(ITeacherStudentRepository teacherStudentRepository) : ITeacherStudentService
{
    public Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(long sectionId, CancellationToken cancellationToken)
        => teacherStudentRepository.GetStudentsBySectionAsync(sectionId, cancellationToken);

    public Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(long teacherId, long sectionId, long studentId, CancellationToken cancellationToken)
        => teacherStudentRepository.GetStudentInSection(teacherId, sectionId, studentId, cancellationToken);
}
