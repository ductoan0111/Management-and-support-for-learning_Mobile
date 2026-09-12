using BE_Mobile.Contracts.Common;
using BE_Mobile.Contracts.Students;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Students;

[ApiController]
[Route("api/students/{studentId:long}")]
public sealed class StudentLearningController(IStudentLearningService studentLearningService) : ControllerBase
{
    [HttpGet("profile")]
    public Task<ActionResult<StudentProfileDto>> GetProfile(
        long studentId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetProfile(studentId, cancellationToken);
    }

    [HttpPut("profile")]
    public Task<ActionResult<StudentProfileDto>> UpdateProfile(
        long studentId,
        UpdateStudentProfileRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningService.UpdateProfile(studentId, request, cancellationToken);
    }

    [HttpGet("dashboard")]
    public Task<ActionResult<StudentDashboardDto>> GetDashboard(
        long studentId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetDashboard(studentId, cancellationToken);
    }

    [HttpGet("sections")]
    [HttpGet("courses")]
    public Task<ActionResult<IReadOnlyList<StudentSectionDto>>> GetSections(
        long studentId,
        [FromQuery] int? semesterId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetSections(studentId, semesterId, status, cancellationToken);
    }

    [HttpGet("schedule")]
    public Task<ActionResult<IReadOnlyList<StudentScheduleDto>>> GetSchedule(
        long studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetSchedule(studentId, from, to, sectionId, cancellationToken);
    }

    [HttpGet("exams")]
    public Task<ActionResult<IReadOnlyList<StudentExamScheduleDto>>> GetExams(
        long studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? sectionId,
        [FromQuery] byte? examType,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetExams(studentId, from, to, sectionId, examType, cancellationToken);
    }

    [HttpGet("assignments")]
    [HttpGet("deadlines")]
    public Task<ActionResult<IReadOnlyList<StudentAssignmentDto>>> GetAssignments(
        long studentId,
        [FromQuery] long? sectionId,
        [FromQuery] byte? submissionStatus,
        [FromQuery] DateTime? fromDueAt,
        [FromQuery] DateTime? toDueAt,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetAssignments(
            studentId,
            sectionId,
            submissionStatus,
            fromDueAt,
            toDueAt,
            cancellationToken);
    }

    [HttpGet("assignments/{assignmentId:long}")]
    public Task<ActionResult<StudentAssignmentDto>> GetAssignment(
        long studentId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetAssignment(studentId, assignmentId, cancellationToken);
    }

    [HttpPost("assignments/{assignmentId:long}/submissions")]
    public Task<ActionResult<AssignmentSubmissionDto>> SubmitAssignment(
        long studentId,
        long assignmentId,
        SubmitAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningService.SubmitAssignment(studentId, assignmentId, request, cancellationToken);
    }

    [HttpGet("submissions")]
    public Task<ActionResult<IReadOnlyList<StudentSubmissionDto>>> GetSubmissions(
        long studentId,
        [FromQuery] long? assignmentId,
        [FromQuery] long? sectionId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetSubmissions(studentId, assignmentId, sectionId, status, cancellationToken);
    }

    [HttpGet("materials")]
    public Task<ActionResult<IReadOnlyList<StudentMaterialDto>>> GetMaterials(
        long studentId,
        [FromQuery] long? sectionId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetMaterials(studentId, sectionId, search, cancellationToken);
    }

    [HttpGet("grades")]
    public Task<ActionResult<IReadOnlyList<StudentGradeDto>>> GetGrades(
        long studentId,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetGrades(studentId, sectionId, cancellationToken);
    }

    [HttpGet("scores")]
    public Task<ActionResult<IReadOnlyList<StudentSectionScoreDto>>> GetSectionScores(
        long studentId,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetSectionScores(studentId, sectionId, cancellationToken);
    }

    [HttpGet("gpa")]
    public Task<ActionResult<StudentGpaDto>> GetGpa(
        long studentId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetGpa(studentId, cancellationToken);
    }

    [HttpGet("goals")]
    public Task<ActionResult<IReadOnlyList<StudyGoalDto>>> GetStudyGoals(
        long studentId,
        [FromQuery] byte? status,
        [FromQuery] byte? goalType,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetStudyGoals(studentId, status, goalType, cancellationToken);
    }

    [HttpGet("goals/{goalId:long}")]
    public Task<ActionResult<StudyGoalDto>> GetStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetStudyGoal(studentId, goalId, cancellationToken);
    }

    [HttpPost("goals")]
    public Task<ActionResult<StudyGoalDto>> CreateStudyGoal(
        long studentId,
        CreateStudyGoalRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningService.CreateStudyGoal(studentId, request, cancellationToken);
    }

    [HttpPut("goals/{goalId:long}")]
    public Task<ActionResult<StudyGoalDto>> UpdateStudyGoal(
        long studentId,
        long goalId,
        UpdateStudyGoalRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningService.UpdateStudyGoal(studentId, goalId, request, cancellationToken);
    }

    [HttpDelete("goals/{goalId:long}")]
    public Task<IActionResult> DeleteStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.DeleteStudyGoal(studentId, goalId, cancellationToken);
    }

    [HttpGet("tasks")]
    public Task<ActionResult<IReadOnlyList<StudyTaskDto>>> GetStudyTasks(
        long studentId,
        [FromQuery] byte? status,
        [FromQuery] int? courseId,
        [FromQuery] DateTime? fromDueAt,
        [FromQuery] DateTime? toDueAt,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetStudyTasks(
            studentId,
            status,
            courseId,
            fromDueAt,
            toDueAt,
            cancellationToken);
    }

    [HttpGet("tasks/{studyTaskId:long}")]
    public Task<ActionResult<StudyTaskDto>> GetStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.GetStudyTask(studentId, studyTaskId, cancellationToken);
    }

    [HttpPost("tasks")]
    public Task<ActionResult<StudyTaskDto>> CreateStudyTask(
        long studentId,
        CreateStudyTaskRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningService.CreateStudyTask(studentId, request, cancellationToken);
    }

    [HttpPut("tasks/{studyTaskId:long}")]
    public Task<ActionResult<StudyTaskDto>> UpdateStudyTask(
        long studentId,
        long studyTaskId,
        UpdateStudyTaskRequest request,
        CancellationToken cancellationToken)
    {
        return studentLearningService.UpdateStudyTask(studentId, studyTaskId, request, cancellationToken);
    }

    [HttpDelete("tasks/{studyTaskId:long}")]
    public Task<IActionResult> DeleteStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.DeleteStudyTask(studentId, studyTaskId, cancellationToken);
    }

    [HttpGet("announcements")]
    public Task<ActionResult<PagedResult<StudentAnnouncementDto>>> GetAnnouncements(
        long studentId,
        [FromQuery] bool? isRead,
        [FromQuery] byte? announcementType,
        [FromQuery] long? sectionId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return studentLearningService.GetAnnouncements(
            studentId,
            isRead,
            announcementType,
            sectionId,
            activeOnly,
            page,
            pageSize,
            cancellationToken);
    }

    [HttpPost("announcements/{announcementId:long}/read")]
    public Task<IActionResult> MarkAnnouncementAsRead(
        long studentId,
        long announcementId,
        CancellationToken cancellationToken)
    {
        return studentLearningService.MarkAnnouncementAsRead(studentId, announcementId, cancellationToken);
    }
}
