using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}/sections/{sectionId:long}")]
public sealed class TeacherGradesController(ITeacherGradeService teacherGradeService) : ControllerBase
{
    /// <summary>Danh sách thành phần điểm của lớp học phần.</summary>
    [HttpGet("grade-components")]
    public Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherGradeService.GetGradeComponents(teacherId, sectionId, cancellationToken);

    /// <summary>Bảng điểm toàn lớp (có thể lọc theo thành phần điểm).</summary>
    [HttpGet("grades")]
    public Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(
        long teacherId,
        long sectionId,
        [FromQuery] long? componentId,
        CancellationToken cancellationToken)
        => teacherGradeService.GetGrades(teacherId, sectionId, componentId, cancellationToken);

    /// <summary>Nhập hoặc cập nhật điểm một thành phần cho một sinh viên.</summary>
    [HttpPut("grades/{studentId:long}")]
    public Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(
        long teacherId,
        long sectionId,
        long studentId,
        [FromBody] UpsertStudentGradeRequest request,
        CancellationToken cancellationToken)
        => teacherGradeService.UpsertStudentGrade(teacherId, sectionId, studentId, request, cancellationToken);
}
