using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherAssignmentService(ITeacherAssignmentRepository teacherAssignmentRepository) : ITeacherAssignmentService
{
    public Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherAssignmentRepository.GetAssignments(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<TeacherAssignmentDto>> GetAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken)
        => teacherAssignmentRepository.GetAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    public Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(long teacherId, long sectionId, CreateAssignmentRequest request, CancellationToken cancellationToken)
        => teacherAssignmentRepository.CreateAssignment(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(long teacherId, long sectionId, long assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken)
        => teacherAssignmentRepository.UpdateAssignment(teacherId, sectionId, assignmentId, request, cancellationToken);

    public Task<IActionResult> DeleteAssignment(long teacherId, long sectionId, long assignmentId, CancellationToken cancellationToken)
        => teacherAssignmentRepository.DeleteAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(long teacherId, long sectionId, long assignmentId, byte? status, CancellationToken cancellationToken)
        => teacherAssignmentRepository.GetSubmissions(teacherId, sectionId, assignmentId, status, cancellationToken);

    public Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(long teacherId, long sectionId, long assignmentId, long submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken)
        => teacherAssignmentRepository.GradeSubmission(teacherId, sectionId, assignmentId, submissionId, request, cancellationToken);
}
