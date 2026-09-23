using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Interfaces;

public interface ITeacherAssignmentService
{
    Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(long teacherId, long sectionId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAssignmentDto>> GetAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(long teacherId, long sectionId, CreateAssignmentRequest request, CancellationToken cancellationToken);
    Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(long teacherId, long sectionId, long assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken);
    Task<IActionResult> DeleteAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken);
    Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(long teacherId, long sectionId, long assignmentId, byte? status, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(long teacherId, long sectionId, long assignmentId, long submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken);
}
