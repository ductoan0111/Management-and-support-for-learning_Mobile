namespace BE_Mobile.Contracts.Teachers;

// ─── Hồ sơ giảng viên ────────────────────────────────────────────────────────

public sealed record TeacherProfileDto(
    long TeacherId,
    long UserId,
    string TeacherCode,
    string Username,
    string FullName,
    string Email,
    string? Phone,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? AvatarUrl,
    bool IsActive,
    int DepartmentId,
    string DepartmentCode,
    string DepartmentName,
    string? AcademicTitle,
    string? Specialization,
    byte Status);

public sealed record UpdateTeacherProfileRequest(
    string FullName,
    string? Phone,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? AvatarUrl,
    string? AcademicTitle,
    string? Specialization);

// ─── Lớp học phần ────────────────────────────────────────────────────────────

public sealed record TeacherSectionDto(
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
    byte Status,
    int? MaxStudents,
    int EnrolledCount,
    bool IsPrimary);

public sealed record TeacherSectionDetailDto(
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
    byte Status,
    int? MaxStudents,
    int EnrolledCount,
    bool IsPrimary,
    IReadOnlyList<TeacherScheduleDto> Schedules);

// ─── Thời khóa biểu ──────────────────────────────────────────────────────────

public sealed record TeacherScheduleDto(
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

// ─── Tài liệu ────────────────────────────────────────────────────────────────

public sealed record TeacherMaterialDto(
    long MaterialId,
    long SectionId,
    string SectionCode,
    string CourseCode,
    string CourseName,
    string Title,
    string? Description,
    string? MaterialType,
    string? FileUrl,
    string? ExternalUrl,
    bool IsVisible,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateMaterialRequest(
    string Title,
    string? Description,
    string? MaterialType,
    string? FileUrl,
    string? ExternalUrl,
    bool IsVisible = true);

public sealed record UpdateMaterialRequest(
    string Title,
    string? Description,
    string? MaterialType,
    string? FileUrl,
    string? ExternalUrl,
    bool IsVisible);

// ─── Thành phần điểm ─────────────────────────────────────────────────────────

public sealed record TeacherGradeComponentDto(
    long GradeComponentId,
    long SectionId,
    string ComponentName,
    decimal WeightPercent,
    decimal MaxScore,
    int DisplayOrder);

// ─── Điểm sinh viên ──────────────────────────────────────────────────────────

public sealed record TeacherStudentGradeDto(
    long StudentId,
    string StudentCode,
    string FullName,
    long GradeComponentId,
    string ComponentName,
    decimal? Score,
    string? Note,
    long? GradedByUserId,
    DateTime? GradedAt,
    DateTime? UpdatedAt);

public sealed record UpsertStudentGradeRequest(
    long GradeComponentId,
    decimal? Score,
    string? Note);

// ─── Thông báo ───────────────────────────────────────────────────────────────

public sealed record TeacherAnnouncementDto(
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
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateAnnouncementRequest(
    string Title,
    string Content,
    byte AnnouncementType,
    DateTime? ExpiresAt,
    bool IsActive = true);

public sealed record UpdateAnnouncementRequest(
    string Title,
    string Content,
    byte AnnouncementType,
    DateTime? ExpiresAt,
    bool IsActive);

// ─── Sinh viên trong lớp ─────────────────────────────────────────────────────

// Giữ nguyên để tương thích ngược với stored procedure sp_GetStudentsBySection
public sealed record TeacherSectionStudentDto(
    long StudentId,
    string StudentCode,
    string FullName,
    string Email,
    string? Phone,
    DateTime EnrolledAt,
    byte Status,
    decimal? FinalScore10,
    string? LetterGrade);
public sealed record TeacherSectionStudentDetailDto(
    long StudentId,
    string StudentCode,
    string FullName,
    string Email,
    string? Phone,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? AvatarUrl,
    string? ClassCode,
    string? ClassName,
    string MajorCode,
    string MajorName,
    DateTime EnrolledAt,
    byte EnrollmentStatus,
    decimal? FinalScore10,
    string? LetterGrade);
