using BE_Mobile.Contracts.Common;
using BE_Mobile.Contracts.Students;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class StudentLearningService(IStudentLearningRepository studentLearningRepository) : IStudentLearningService
{
    public Task<ActionResult<StudentProfileDto>> GetProfile(
        long studentId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetProfile(studentId, cancellationToken);
    }

    public Task<ActionResult<StudentProfileDto>> UpdateProfile(
        long studentId,
        UpdateStudentProfileRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.UpdateProfile(studentId, request, cancellationToken);
    }

    public Task<ActionResult<StudentDashboardDto>> GetDashboard(
        long studentId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetDashboard(studentId, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentSectionDto>>> GetSections(
        long studentId,
        int? semesterId,
        byte? status,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetSections(studentId, semesterId, status, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentScheduleDto>>> GetSchedule(
        long studentId,
        DateOnly? from,
        DateOnly? to,
        long? sectionId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetSchedule(studentId, from, to, sectionId, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentExamScheduleDto>>> GetExams(
        long studentId,
        DateOnly? from,
        DateOnly? to,
        long? sectionId,
        byte? examType,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetExams(studentId, from, to, sectionId, examType, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentAssignmentDto>>> GetAssignments(
        long studentId,
        long? sectionId,
        byte? submissionStatus,
        DateTime? fromDueAt,
        DateTime? toDueAt,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetAssignments(
            studentId,
            sectionId,
            submissionStatus,
            fromDueAt,
            toDueAt,
            cancellationToken);
    }

    public Task<ActionResult<StudentAssignmentDto>> GetAssignment(
        long studentId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetAssignment(studentId, assignmentId, cancellationToken);
    }

    public Task<ActionResult<AssignmentSubmissionDto>> SubmitAssignment(
        long studentId,
        long assignmentId,
        SubmitAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.SubmitAssignment(studentId, assignmentId, request, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentSubmissionDto>>> GetSubmissions(
        long studentId,
        long? assignmentId,
        long? sectionId,
        byte? status,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetSubmissions(
            studentId,
            assignmentId,
            sectionId,
            status,
            cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentMaterialDto>>> GetMaterials(
        long studentId,
        long? sectionId,
        string? search,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetMaterials(studentId, sectionId, search, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentGradeDto>>> GetGrades(
        long studentId,
        long? sectionId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetGrades(studentId, sectionId, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudentSectionScoreDto>>> GetSectionScores(
        long studentId,
        long? sectionId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetSectionScores(studentId, sectionId, cancellationToken);
    }

    public Task<ActionResult<StudentGpaDto>> GetGpa(
        long studentId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetGpa(studentId, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudyGoalDto>>> GetStudyGoals(
        long studentId,
        byte? status,
        byte? goalType,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetStudyGoals(studentId, status, goalType, cancellationToken);
    }

    public Task<ActionResult<StudyGoalDto>> GetStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetStudyGoal(studentId, goalId, cancellationToken);
    }

    public Task<ActionResult<StudyGoalDto>> CreateStudyGoal(
        long studentId,
        CreateStudyGoalRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.CreateStudyGoal(studentId, request, cancellationToken);
    }

    public Task<ActionResult<StudyGoalDto>> UpdateStudyGoal(
        long studentId,
        long goalId,
        UpdateStudyGoalRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.UpdateStudyGoal(studentId, goalId, request, cancellationToken);
    }

    public Task<IActionResult> DeleteStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.DeleteStudyGoal(studentId, goalId, cancellationToken);
    }

    public Task<ActionResult<IReadOnlyList<StudyTaskDto>>> GetStudyTasks(
        long studentId,
        byte? status,
        int? courseId,
        DateTime? fromDueAt,
        DateTime? toDueAt,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetStudyTasks(
            studentId,
            status,
            courseId,
            fromDueAt,
            toDueAt,
            cancellationToken);
    }

    public Task<ActionResult<StudyTaskDto>> GetStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetStudyTask(studentId, studyTaskId, cancellationToken);
    }

    public Task<ActionResult<StudyTaskDto>> CreateStudyTask(
        long studentId,
        CreateStudyTaskRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.CreateStudyTask(studentId, request, cancellationToken);
    }

    public Task<ActionResult<StudyTaskDto>> UpdateStudyTask(
        long studentId,
        long studyTaskId,
        UpdateStudyTaskRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.UpdateStudyTask(studentId, studyTaskId, request, cancellationToken);
    }

    public Task<IActionResult> DeleteStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.DeleteStudyTask(studentId, studyTaskId, cancellationToken);
    }

    public Task<ActionResult<PagedResult<StudentAnnouncementDto>>> GetAnnouncements(
        long studentId,
        bool? isRead,
        byte? announcementType,
        long? sectionId,
        bool activeOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.GetAnnouncements(
            studentId,
            isRead,
            announcementType,
            sectionId,
            activeOnly,
            page,
            pageSize,
            cancellationToken);
    }

    public Task<IActionResult> MarkAnnouncementAsRead(
        long studentId,
        long announcementId,
        CancellationToken cancellationToken)
    {
        return studentLearningRepository.MarkAnnouncementAsRead(studentId, announcementId, cancellationToken);
    }
}
