using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherGradeService(ITeacherGradeRepository teacherGradeRepository) : ITeacherGradeService
{
    public Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherGradeRepository.GetGradeComponents(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(long teacherId, long sectionId, long? componentId, CancellationToken cancellationToken)
        => teacherGradeRepository.GetGrades(teacherId, sectionId, componentId, cancellationToken);

    public Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(long teacherId, long sectionId, long studentId, UpsertStudentGradeRequest request, CancellationToken cancellationToken)
        => teacherGradeRepository.UpsertStudentGrade(teacherId, sectionId, studentId, request, cancellationToken);
}
