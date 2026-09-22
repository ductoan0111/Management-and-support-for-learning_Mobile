namespace BE_Mobile.Contracts.Teachers;

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
