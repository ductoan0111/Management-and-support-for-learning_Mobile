using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}")]
public sealed class TeacherAnnouncementsController(ITeacherAnnouncementService teacherAnnouncementService) : ControllerBase
{
    /// <summary>Danh sách thông báo giảng viên đã gửi.</summary>
    [HttpGet("announcements")]
    public Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(
        long teacherId,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
        => teacherAnnouncementService.GetAnnouncements(teacherId, sectionId, cancellationToken);

    /// <summary>Chi tiết một thông báo.</summary>
    [HttpGet("announcements/{announcementId:long}")]
    public Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(
        long teacherId,
        long announcementId,
        CancellationToken cancellationToken)
        => teacherAnnouncementService.GetAnnouncement(teacherId, announcementId, cancellationToken);

    /// <summary>Gửi thông báo đến sinh viên một lớp học phần.</summary>
    [HttpPost("sections/{sectionId:long}/announcements")]
    public Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(
        long teacherId,
        long sectionId,
        [FromBody] CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
        => teacherAnnouncementService.CreateAnnouncement(teacherId, sectionId, request, cancellationToken);

    /// <summary>Sửa thông báo đã gửi.</summary>
    [HttpPut("announcements/{announcementId:long}")]
    public Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(
        long teacherId,
        long announcementId,
        [FromBody] UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
        => teacherAnnouncementService.UpdateAnnouncement(teacherId, announcementId, request, cancellationToken);

    /// <summary>Xóa thông báo.</summary>
    [HttpDelete("announcements/{announcementId:long}")]
    public Task<IActionResult> DeleteAnnouncement(
        long teacherId,
        long announcementId,
        CancellationToken cancellationToken)
        => teacherAnnouncementService.DeleteAnnouncement(teacherId, announcementId, cancellationToken);
}
