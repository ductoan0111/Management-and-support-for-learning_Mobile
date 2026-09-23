using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}/sections/{sectionId:long}/assignments")]
public sealed class TeacherAssignmentsController(ITeacherAssignmentService teacherAssignmentService) : ControllerBase
{
    /// <summary>Danh sách bài tập của một lớp học phần.</summary>
    [HttpGet]
    public Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherAssignmentService.GetAssignments(teacherId, sectionId, cancellationToken);

    /// <summary>Chi tiết một bài tập.</summary>
    [HttpGet("{assignmentId:long}")]
    public Task<ActionResult<TeacherAssignmentDto>> GetAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        CancellationToken cancellationToken)
        => teacherAssignmentService.GetAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    /// <summary>Tạo bài tập mới cho lớp học phần.</summary>
    [HttpPost]
    public Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(
        long teacherId,
        long sectionId,
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
        => teacherAssignmentService.CreateAssignment(teacherId, sectionId, request, cancellationToken);

    /// <summary>Cập nhật bài tập.</summary>
    [HttpPut("{assignmentId:long}")]
    public Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        [FromBody] UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
        => teacherAssignmentService.UpdateAssignment(teacherId, sectionId, assignmentId, request, cancellationToken);

    /// <summary>Xóa bài tập.</summary>
    [HttpDelete("{assignmentId:long}")]
    public Task<IActionResult> DeleteAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        CancellationToken cancellationToken)
        => teacherAssignmentService.DeleteAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    /// <summary>Danh sách bài nộp của sinh viên cho một bài tập.</summary>
    [HttpGet("{assignmentId:long}/submissions")]
    public Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(
        long teacherId,
        long sectionId,
        long assignmentId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
        => teacherAssignmentService.GetSubmissions(teacherId, sectionId, assignmentId, status, cancellationToken);

    /// <summary>Chấm điểm một bài nộp của sinh viên.</summary>
    [HttpPut("{assignmentId:long}/submissions/{submissionId:long}/grade")]
    public Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(
        long teacherId,
        long sectionId,
        long assignmentId,
        long submissionId,
        [FromBody] GradeSubmissionRequest request,
        CancellationToken cancellationToken)
        => teacherAssignmentService.GradeSubmission(teacherId, sectionId, assignmentId, submissionId, request, cancellationToken);
}
