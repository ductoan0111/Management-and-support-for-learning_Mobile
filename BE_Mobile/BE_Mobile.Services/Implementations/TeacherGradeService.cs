using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherGradeService(ITeacherGradeRepository teacherGradeRepository) : ITeacherGradeService
{
    public Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherGradeRepository.GetGradeComponents(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<TeacherGradeComponentDto>> CreateGradeComponent(long teacherId, long sectionId, SaveGradeComponentRequest request, CancellationToken cancellationToken)
        => teacherGradeRepository.CreateGradeComponent(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherGradeComponentDto>> UpdateGradeComponent(long teacherId, long sectionId, long componentId, SaveGradeComponentRequest request, CancellationToken cancellationToken)
        => teacherGradeRepository.UpdateGradeComponent(teacherId, sectionId, componentId, request, cancellationToken);

    public Task<IActionResult> DeleteGradeComponent(long teacherId, long sectionId, long componentId, CancellationToken cancellationToken)
        => teacherGradeRepository.DeleteGradeComponent(teacherId, sectionId, componentId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(long teacherId, long sectionId, long? componentId, CancellationToken cancellationToken)
        => teacherGradeRepository.GetGrades(teacherId, sectionId, componentId, cancellationToken);

    public Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(long teacherId, long sectionId, long studentId, UpsertStudentGradeRequest request, CancellationToken cancellationToken)
        => teacherGradeRepository.UpsertStudentGrade(teacherId, sectionId, studentId, request, cancellationToken);

    public Task<ActionResult<TeacherGradeOverviewDto>> GetGradeOverview(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherGradeRepository.GetGradeOverview(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<TeacherGradeOverviewDto>> FinalizeGrades(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherGradeRepository.FinalizeGrades(teacherId, sectionId, cancellationToken);
}
