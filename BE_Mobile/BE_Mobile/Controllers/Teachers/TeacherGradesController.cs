using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Security;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Authorize(AuthenticationSchemes = AppAuthenticationDefaults.Scheme, Roles = "TEACHER")]
[TeacherRouteGuard]
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

    /// <summary>Tạo thành phần điểm mới cho lớp học phần.</summary>
    [HttpPost("grade-components")]
    public Task<ActionResult<TeacherGradeComponentDto>> CreateGradeComponent(
        long teacherId,
        long sectionId,
        [FromBody] SaveGradeComponentRequest request,
        CancellationToken cancellationToken)
        => teacherGradeService.CreateGradeComponent(teacherId, sectionId, request, cancellationToken);

    /// <summary>Cập nhật thành phần điểm.</summary>
    [HttpPut("grade-components/{componentId:long}")]
    public Task<ActionResult<TeacherGradeComponentDto>> UpdateGradeComponent(
        long teacherId,
        long sectionId,
        long componentId,
        [FromBody] SaveGradeComponentRequest request,
        CancellationToken cancellationToken)
        => teacherGradeService.UpdateGradeComponent(teacherId, sectionId, componentId, request, cancellationToken);

    /// <summary>Xóa thành phần điểm chưa có điểm sinh viên.</summary>
    [HttpDelete("grade-components/{componentId:long}")]
    public Task<IActionResult> DeleteGradeComponent(
        long teacherId,
        long sectionId,
        long componentId,
        CancellationToken cancellationToken)
        => teacherGradeService.DeleteGradeComponent(teacherId, sectionId, componentId, cancellationToken);

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

    /// <summary>Tổng quan điểm tổng kết của lớp.</summary>
    [HttpGet("grade-summary")]
    public Task<ActionResult<TeacherGradeOverviewDto>> GetGradeOverview(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherGradeService.GetGradeOverview(teacherId, sectionId, cancellationToken);

    /// <summary>Tính và lưu điểm tổng kết vào bảng đăng ký học.</summary>
    [HttpPost("grades/finalize")]
    public Task<ActionResult<TeacherGradeOverviewDto>> FinalizeGrades(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherGradeService.FinalizeGrades(teacherId, sectionId, cancellationToken);
}
