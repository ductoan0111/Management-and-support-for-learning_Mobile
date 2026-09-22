using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Interfaces;

public interface ITeacherStudentService
{
    Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(long teacherId, long sectionId, long studentId, CancellationToken cancellationToken);
}
