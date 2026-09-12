namespace BE_Mobile.Contracts.Students;

public sealed record StudentProfileDto(
    long StudentId,
    long UserId,
    string StudentCode,
    string Username,
    string FullName,
    string Email,
    string? Phone,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? AvatarUrl,
    bool IsActive,
    int? AcademicClassId,
    string? ClassCode,
    string? ClassName,
    int MajorId,
    string MajorCode,
    string MajorName,
    int DepartmentId,
    string DepartmentCode,
    string DepartmentName,
    short EnrollmentYear,
    byte Status);

public sealed record UpdateStudentProfileRequest(
    string FullName,
    string? Phone,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? AvatarUrl);

public sealed record StudentDashboardDto(
    StudentDashboardProfileDto Student,
    IReadOnlyList<StudentAssignmentDeadlineDto> Deadlines,
    IReadOnlyList<StudentExamDto> Exams);

public sealed record StudentDashboardProfileDto(
    long StudentId,
    string StudentCode,
    string FullName,
    string Email,
    string MajorName,
    string? ClassName,
    decimal? Gpa);

public sealed record StudentAssignmentDeadlineDto(
    long StudentId,
    long SectionId,
    string CourseCode,
    string CourseName,
    long AssignmentId,
    string Title,
    DateTime DueAt,
    decimal MaxScore,
    string SubmissionStatus,
    DateTime? SubmittedAt,
    decimal? Score);

public sealed record StudentExamDto(
    long StudentId,
    string CourseCode,
    string CourseName,
    long ExamId,
    string ExamName,
    byte ExamType,
    DateOnly ExamDate,
    string StartTime,
    short DurationMinutes,
    string? Room);

public sealed record StudentSectionDto(
    long StudentId,
    long EnrollmentId,
    long SectionId,
    string SectionCode,
    string? SectionName,
    int CourseId,
    string CourseCode,
    string CourseName,
    byte Credits,
    int SemesterId,
    string SemesterCode,
    string SemesterName,
    string AcademicYear,
    byte EnrollmentStatus,
    decimal? FinalScore10,
    string? LetterGrade);

public sealed record StudentScheduleDto(
    long StudentId,
    long SectionId,
    string SectionCode,
    string CourseCode,
    string CourseName,
    long ScheduleId,
    byte DayOfWeek,
    string StartTime,
    string EndTime,
    string? Room,
    string? Building,
    DateOnly EffectiveFrom,
    DateOnly EffectiveTo,
    string? Note);

public sealed record StudentExamScheduleDto(
    long StudentId,
    long SectionId,
    string SectionCode,
    string CourseCode,
    string CourseName,
    long ExamId,
    string ExamName,
    byte ExamType,
    DateOnly ExamDate,
    string StartTime,
    short DurationMinutes,
    string? Room,
    string? Note,
    DateTime CreatedAt);

public sealed record StudentAssignmentDto(
    long StudentId,
    long SectionId,
    string CourseCode,
    string CourseName,
    long AssignmentId,
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
    long? SubmissionId,
    string? TextContent,
    string? FileUrl,
    DateTime? SubmittedAt,
    bool? IsLate,
    byte? SubmissionRawStatus,
    string SubmissionStatus,
    decimal? Score,
    string? Feedback,
    DateTime? GradedAt);

public sealed record SubmitAssignmentRequest(
    string? TextContent,
    string? FileUrl);

public sealed record AssignmentSubmissionDto(
    long SubmissionId,
    long AssignmentId,
    long StudentId,
    string? TextContent,
    string? FileUrl,
    DateTime SubmittedAt,
    bool IsLate,
    byte Status,
    decimal? Score,
    string? Feedback,
    long? GradedByUserId,
    DateTime? GradedAt);

public sealed record StudentSubmissionDto(
    long SubmissionId,
    long AssignmentId,
    string AssignmentTitle,
    long SectionId,
    string CourseCode,
    string CourseName,
    DateTime DueAt,
    string? TextContent,
    string? FileUrl,
    DateTime SubmittedAt,
    bool IsLate,
    byte Status,
    decimal? Score,
    string? Feedback,
    DateTime? GradedAt);

public sealed record StudentMaterialDto(
    long MaterialId,
    long SectionId,
    string CourseCode,
    string CourseName,
    long UploadedByUserId,
    string UploadedByFullName,
    string Title,
    string? Description,
    string? MaterialType,
    string? FileUrl,
    string? ExternalUrl,
    bool IsVisible,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record StudentGradeDto(
    long GradeComponentId,
    long SectionId,
    string CourseCode,
    string CourseName,
    string ComponentName,
    decimal WeightPercent,
    decimal MaxScore,
    int DisplayOrder,
    long? StudentGradeId,
    decimal? Score,
    string? Note,
    long? GradedByUserId,
    DateTime? GradedAt,
    DateTime? UpdatedAt);

public sealed record StudentSectionScoreDto(
    long StudentId,
    long SectionId,
    string CourseCode,
    string CourseName,
    byte Credits,
    decimal WeightedScore10,
    decimal? Gpa4);

public sealed record StudentGpaDto(
    long StudentId,
    decimal? Gpa);

public sealed record StudyGoalDto(
    long GoalId,
    long StudentId,
    string Title,
    string? Description,
    byte GoalType,
    decimal? TargetValue,
    decimal? CurrentValue,
    DateOnly StartDate,
    DateOnly? EndDate,
    byte Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateStudyGoalRequest(
    string Title,
    string? Description,
    byte GoalType,
    decimal? TargetValue,
    decimal? CurrentValue,
    DateOnly StartDate,
    DateOnly? EndDate,
    byte Status = 1);

public sealed record UpdateStudyGoalRequest(
    string Title,
    string? Description,
    byte GoalType,
    decimal? TargetValue,
    decimal? CurrentValue,
    DateOnly StartDate,
    DateOnly? EndDate,
    byte Status);

public sealed record StudyTaskDto(
    long StudyTaskId,
    long StudentId,
    int? CourseId,
    string? CourseCode,
    string? CourseName,
    string Title,
    string? Description,
    DateTime? StartAt,
    DateTime? DueAt,
    DateTime? ReminderAt,
    byte Priority,
    byte Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CompletedAt);

public sealed record CreateStudyTaskRequest(
    int? CourseId,
    string Title,
    string? Description,
    DateTime? StartAt,
    DateTime? DueAt,
    DateTime? ReminderAt,
    byte Priority = 2,
    byte Status = 1);

public sealed record UpdateStudyTaskRequest(
    int? CourseId,
    string Title,
    string? Description,
    DateTime? StartAt,
    DateTime? DueAt,
    DateTime? ReminderAt,
    byte Priority,
    byte Status);

public sealed record StudentAnnouncementDto(
    long AnnouncementId,
    long CreatedByUserId,
    string CreatedByFullName,
    long? SectionId,
    string? SectionCode,
    string? CourseCode,
    string? CourseName,
    string Title,
    string Content,
    byte AnnouncementType,
    DateTime PublishedAt,
    DateTime? ExpiresAt,
    bool IsActive,
    bool IsRead,
    DateTime? ReadAt);
