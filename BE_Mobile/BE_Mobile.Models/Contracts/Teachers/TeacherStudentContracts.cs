namespace BE_Mobile.Contracts.Teachers;

// ─── Sinh viên trong lớp học phần ────────────────────────────────────────────

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
