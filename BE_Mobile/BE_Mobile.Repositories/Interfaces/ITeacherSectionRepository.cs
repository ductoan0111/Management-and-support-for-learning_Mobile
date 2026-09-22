using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Repositories.Interfaces;

public interface ITeacherSectionRepository
{
    // ── Hồ sơ ─────────────────────────────────────────────────────────────────
    Task<ActionResult<TeacherProfileDto>> GetProfile(long teacherId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherProfileDto>> UpdateProfile(long teacherId, UpdateTeacherProfileRequest request, CancellationToken cancellationToken);

    // ── Lớp học phần ─────────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(long teacherId, int? semesterId, byte? status, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSectionDetailDto>> GetSection(long teacherId, long sectionId, CancellationToken cancellationToken);

    // ── Thời khóa biểu ───────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(long teacherId, DateOnly? from, DateOnly? to, long? sectionId, CancellationToken cancellationToken);

    // ── Bài tập ───────────────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(long teacherId, long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAssignmentDto>> GetAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(long teacherId, long sectionId, CreateAssignmentRequest request, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(long teacherId, long sectionId, long assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken);
    Task<IActionResult> DeleteAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken);

    // ── Bài nộp ───────────────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(long teacherId, long sectionId, long assignmentId, byte? status, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(long teacherId, long sectionId, long assignmentId, long submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken);

    // ── Tài liệu ─────────────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(long teacherId, long sectionId, string? search, CancellationToken cancellationToken);
    Task<ActionResult<TeacherMaterialDto>> CreateMaterial(long teacherId, long sectionId, CreateMaterialRequest request, CancellationToken cancellationToken);
    Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(long teacherId, long sectionId, long materialId, UpdateMaterialRequest request, CancellationToken cancellationToken);
    Task<IActionResult> DeleteMaterial(long teacherId, long sectionId, long materialId, CancellationToken cancellationToken);

    // ── Thành phần điểm & Điểm sinh viên ─────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(long teacherId, long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(long teacherId, long sectionId, long? componentId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(long teacherId, long sectionId, long studentId, UpsertStudentGradeRequest request, CancellationToken cancellationToken);

    // ── Thông báo ─────────────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(long teacherId, long? sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(long teacherId, long sectionId, CreateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(long teacherId, long announcementId, UpdateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<IActionResult> DeleteAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken);

    // ── Sinh viên ─────────────────────────────────────────────────────────────
    Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(long teacherId, long sectionId, long studentId, CancellationToken cancellationToken);
}
