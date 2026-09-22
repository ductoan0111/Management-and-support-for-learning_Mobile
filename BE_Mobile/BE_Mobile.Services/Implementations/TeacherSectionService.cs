using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherSectionService(ITeacherSectionRepository teacherSectionRepository) : ITeacherSectionService
{
    public Task<ActionResult<TeacherProfileDto>> GetProfile(long teacherId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetProfile(teacherId, cancellationToken);

    public Task<ActionResult<TeacherProfileDto>> UpdateProfile(long teacherId, UpdateTeacherProfileRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.UpdateProfile(teacherId, request, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(long teacherId, int? semesterId, byte? status, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSections(teacherId, semesterId, status, cancellationToken);

    public Task<ActionResult<TeacherSectionDetailDto>> GetSection(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSection(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(long teacherId, DateOnly? from, DateOnly? to, long? sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSchedule(teacherId, from, to, sectionId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetAssignments(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<TeacherAssignmentDto>> GetAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    public Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(long teacherId, long sectionId, CreateAssignmentRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.CreateAssignment(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(long teacherId, long sectionId, long assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.UpdateAssignment(teacherId, sectionId, assignmentId, request, cancellationToken);

    public Task<IActionResult> DeleteAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken)
        => teacherSectionRepository.DeleteAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(long teacherId, long sectionId, long assignmentId, byte? status, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSubmissions(teacherId, sectionId, assignmentId, status, cancellationToken);

    public Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(long teacherId, long sectionId, long assignmentId, long submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.GradeSubmission(teacherId, sectionId, assignmentId, submissionId, request, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(long teacherId, long sectionId, string? search, CancellationToken cancellationToken)
        => teacherSectionRepository.GetMaterials(teacherId, sectionId, search, cancellationToken);

    public Task<ActionResult<TeacherMaterialDto>> CreateMaterial(long teacherId, long sectionId, CreateMaterialRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.CreateMaterial(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(long teacherId, long sectionId, long materialId, UpdateMaterialRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.UpdateMaterial(teacherId, sectionId, materialId, request, cancellationToken);

    public Task<IActionResult> DeleteMaterial(long teacherId, long sectionId, long materialId, CancellationToken cancellationToken)
        => teacherSectionRepository.DeleteMaterial(teacherId, sectionId, materialId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetGradeComponents(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(long teacherId, long sectionId, long? componentId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetGrades(teacherId, sectionId, componentId, cancellationToken);

    public Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(long teacherId, long sectionId, long studentId, UpsertStudentGradeRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.UpsertStudentGrade(teacherId, sectionId, studentId, request, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(long teacherId, long? sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetAnnouncements(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetAnnouncement(teacherId, announcementId, cancellationToken);

    public Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(long teacherId, long sectionId, CreateAnnouncementRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.CreateAnnouncement(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(long teacherId, long announcementId, UpdateAnnouncementRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.UpdateAnnouncement(teacherId, announcementId, request, cancellationToken);

    public Task<IActionResult> DeleteAnnouncement(long teacherId, long announcementId, CancellationToken cancellationToken)
        => teacherSectionRepository.DeleteAnnouncement(teacherId, announcementId, cancellationToken);

    public Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(long sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetStudentsBySectionAsync(sectionId, cancellationToken);

    public Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(long teacherId, long sectionId, long studentId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetStudentInSection(teacherId, sectionId, studentId, cancellationToken);
}
