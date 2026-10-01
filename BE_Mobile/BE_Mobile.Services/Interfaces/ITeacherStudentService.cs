using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Interfaces;

public interface ITeacherStudentService
{
    Task<ActionResult<IReadOnlyList<TeacherSectionStudentDto>>> GetStudentsBySectionAsync(long teacherId, long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(long teacherId, long sectionId, long studentId, CancellationToken cancellationToken);
}
