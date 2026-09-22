namespace BE_Mobile.Contracts.Teachers;

// ─── Tài liệu học tập ────────────────────────────────────────────────────────

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
