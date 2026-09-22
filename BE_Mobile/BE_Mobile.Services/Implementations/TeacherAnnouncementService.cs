using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherAnnouncementService(ITeacherAnnouncementRepository teacherAnnouncementRepository) : ITeacherAnnouncementService
{
    public Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(long teacherId, long? sectionId, CancellationToken cancellationToken)
        => teacherAnnouncementRepository.GetAnnouncements(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken)
        => teacherAnnouncementRepository.GetAnnouncement(teacherId, announcementId, cancellationToken);

    public Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(long teacherId, long sectionId, CreateAnnouncementRequest request, CancellationToken cancellationToken)
        => teacherAnnouncementRepository.CreateAnnouncement(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(long teacherId, long announcementId, UpdateAnnouncementRequest request, CancellationToken cancellationToken)
        => teacherAnnouncementRepository.UpdateAnnouncement(teacherId, announcementId, request, cancellationToken);

    public Task<IActionResult> DeleteAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken)
        => teacherAnnouncementRepository.DeleteAnnouncement(teacherId, announcementId, cancellationToken);
}
