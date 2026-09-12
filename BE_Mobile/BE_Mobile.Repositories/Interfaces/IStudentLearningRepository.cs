using BE_Mobile.Contracts.Common;
using BE_Mobile.Contracts.Students;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Repositories.Interfaces;

public interface IStudentLearningRepository
{
    Task<ActionResult<StudentProfileDto>> GetProfile(
        long studentId,
        CancellationToken cancellationToken);

    Task<ActionResult<StudentProfileDto>> UpdateProfile(
        long studentId,
        UpdateStudentProfileRequest request,
        CancellationToken cancellationToken);

    Task<ActionResult<StudentDashboardDto>> GetDashboard(long studentId, CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentSectionDto>>> GetSections(
        long studentId,
        int? semesterId,
        byte? status,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentScheduleDto>>> GetSchedule(
        long studentId,
        DateOnly? from,
        DateOnly? to,
        long? sectionId,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentExamScheduleDto>>> GetExams(
        long studentId,
        DateOnly? from,
        DateOnly? to,
        long? sectionId,
        byte? examType,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentAssignmentDto>>> GetAssignments(
        long studentId,
        long? sectionId,
        byte? submissionStatus,
        DateTime? fromDueAt,
        DateTime? toDueAt,
        CancellationToken cancellationToken);

    Task<ActionResult<StudentAssignmentDto>> GetAssignment(
        long studentId,
        long assignmentId,
        CancellationToken cancellationToken);

    Task<ActionResult<AssignmentSubmissionDto>> SubmitAssignment(
        long studentId,
        long assignmentId,
        SubmitAssignmentRequest request,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentSubmissionDto>>> GetSubmissions(
        long studentId,
        long? assignmentId,
        long? sectionId,
        byte? status,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentMaterialDto>>> GetMaterials(
        long studentId,
        long? sectionId,
        string? search,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentGradeDto>>> GetGrades(
        long studentId,
        long? sectionId,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudentSectionScoreDto>>> GetSectionScores(
        long studentId,
        long? sectionId,
        CancellationToken cancellationToken);

    Task<ActionResult<StudentGpaDto>> GetGpa(long studentId, CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudyGoalDto>>> GetStudyGoals(
        long studentId,
        byte? status,
        byte? goalType,
        CancellationToken cancellationToken);

    Task<ActionResult<StudyGoalDto>> GetStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken);

    Task<ActionResult<StudyGoalDto>> CreateStudyGoal(
        long studentId,
        CreateStudyGoalRequest request,
        CancellationToken cancellationToken);

    Task<ActionResult<StudyGoalDto>> UpdateStudyGoal(
        long studentId,
        long goalId,
        UpdateStudyGoalRequest request,
        CancellationToken cancellationToken);

    Task<IActionResult> DeleteStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken);

    Task<ActionResult<IReadOnlyList<StudyTaskDto>>> GetStudyTasks(
        long studentId,
        byte? status,
        int? courseId,
        DateTime? fromDueAt,
        DateTime? toDueAt,
        CancellationToken cancellationToken);

    Task<ActionResult<StudyTaskDto>> GetStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken);

    Task<ActionResult<StudyTaskDto>> CreateStudyTask(
        long studentId,
        CreateStudyTaskRequest request,
        CancellationToken cancellationToken);

    Task<ActionResult<StudyTaskDto>> UpdateStudyTask(
        long studentId,
        long studyTaskId,
        UpdateStudyTaskRequest request,
        CancellationToken cancellationToken);

    Task<IActionResult> DeleteStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken);

    Task<ActionResult<PagedResult<StudentAnnouncementDto>>> GetAnnouncements(
        long studentId,
        bool? isRead,
        byte? announcementType,
        long? sectionId,
        bool activeOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IActionResult> MarkAnnouncementAsRead(
        long studentId,
        long announcementId,
        CancellationToken cancellationToken);
}
