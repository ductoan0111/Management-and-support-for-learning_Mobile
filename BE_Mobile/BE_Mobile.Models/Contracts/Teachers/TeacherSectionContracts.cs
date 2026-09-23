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

// ─── Thời khóa biểu / Lịch dạy ───────────────────────────────────────────────

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
