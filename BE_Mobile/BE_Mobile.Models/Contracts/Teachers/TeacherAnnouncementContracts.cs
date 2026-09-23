namespace BE_Mobile.Contracts.Teachers;

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
