using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Repositories.Interfaces;

public interface ITeacherAnnouncementRepository
{
    Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(long teacherId, long? sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(long teacherId, long sectionId, CreateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(long teacherId, long announcementId, UpdateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<IActionResult> DeleteAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken);
}
