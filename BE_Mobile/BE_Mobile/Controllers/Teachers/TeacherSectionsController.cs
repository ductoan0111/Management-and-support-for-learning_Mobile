using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}")]
public sealed class TeacherSectionsController(ITeacherSectionService teacherSectionService) : ControllerBase
{
    // ══════════════════════════════════════════════════════════════════════════
    // HỒ SƠ GIẢNG VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Lấy hồ sơ giảng viên.</summary>
    [HttpGet("profile")]
    public Task<ActionResult<TeacherProfileDto>> GetProfile(
        long teacherId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetProfile(teacherId, cancellationToken);

    /// <summary>Cập nhật hồ sơ giảng viên.</summary>
    [HttpPut("profile")]
    public Task<ActionResult<TeacherProfileDto>> UpdateProfile(
        long teacherId,
        [FromBody] UpdateTeacherProfileRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.UpdateProfile(teacherId, request, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // LỚP HỌC PHẦN
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách lớp học phần giảng viên đang/đã dạy.</summary>
    [HttpGet("sections")]
    public Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(
        long teacherId,
        [FromQuery] int? semesterId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSections(teacherId, semesterId, status, cancellationToken);

    /// <summary>Chi tiết một lớp học phần (kèm thời khóa biểu của lớp).</summary>
    [HttpGet("sections/{sectionId:long}")]
    public Task<ActionResult<TeacherSectionDetailDto>> GetSection(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSection(teacherId, sectionId, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // THỜI KHÓA BIỂU / LỊCH DẠY
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Lịch dạy của giảng viên (có thể lọc theo ngày, lớp).</summary>
    [HttpGet("schedule")]
    public Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(
        long teacherId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSchedule(teacherId, from, to, sectionId, cancellationToken);
}
