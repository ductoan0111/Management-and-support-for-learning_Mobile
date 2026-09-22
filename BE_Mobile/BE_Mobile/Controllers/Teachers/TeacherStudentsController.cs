using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}/sections/{sectionId:long}/students")]
public sealed class TeacherStudentsController(ITeacherStudentService teacherStudentService) : ControllerBase
{
    /// <summary>Danh sách sinh viên đăng ký một lớp học phần.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeacherSectionStudentDto>>> GetStudents(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        var students = await teacherStudentService.GetStudentsBySectionAsync(sectionId, cancellationToken);
        return Ok(students);
    }

    /// <summary>Chi tiết hồ sơ và kết quả học tập của một sinh viên trong lớp.</summary>
    [HttpGet("{studentId:long}")]
    public Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(
        long teacherId,
        long sectionId,
        long studentId,
        CancellationToken cancellationToken)
        => teacherStudentService.GetStudentInSection(teacherId, sectionId, studentId, cancellationToken);
}
