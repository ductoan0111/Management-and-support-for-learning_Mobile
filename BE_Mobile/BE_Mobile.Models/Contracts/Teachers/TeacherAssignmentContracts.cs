namespace BE_Mobile.Contracts.Teachers;

// ─── Bài tập ─────────────────────────────────────────────────────────────────

public sealed record TeacherAssignmentDto(
    long AssignmentId,
    long SectionId,
    string SectionCode,
    string CourseCode,
    string CourseName,
    string Title,
    string? Description,
    string? AttachmentUrl,
    DateTime? OpenAt,
    DateTime DueAt,
    decimal MaxScore,
    bool AllowLateSubmission,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int TotalSubmissions,
    int GradedSubmissions);

public sealed record CreateAssignmentRequest(
    string Title,
    string? Description,
    string? AttachmentUrl,
    DateTime? OpenAt,
    DateTime DueAt,
    decimal MaxScore,
    bool AllowLateSubmission = false,
    bool IsPublished = true);

public sealed record UpdateAssignmentRequest(
    string Title,
    string? Description,
    string? AttachmentUrl,
    DateTime? OpenAt,
    DateTime DueAt,
    decimal MaxScore,
    bool AllowLateSubmission,
    bool IsPublished);

// ─── Bài nộp ─────────────────────────────────────────────────────────────────

public sealed record TeacherSubmissionDto(
    long SubmissionId,
    long AssignmentId,
    string AssignmentTitle,
    long StudentId,
    string StudentCode,
    string FullName,
    string? TextContent,
    string? FileUrl,
    DateTime SubmittedAt,
    bool IsLate,
    byte Status,
    decimal? Score,
    string? Feedback,
    DateTime? GradedAt);

public sealed record GradeSubmissionRequest(
    decimal Score,
    string? Feedback);
