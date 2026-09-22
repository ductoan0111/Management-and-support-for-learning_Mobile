using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Repositories.Interfaces;

public interface ITeacherGradeRepository
{
    Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(long teacherId, long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(long teacherId, long sectionId, long? componentId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(long teacherId, long sectionId, long studentId, UpsertStudentGradeRequest request, CancellationToken cancellationToken);
}
