namespace BE_Mobile.Contracts.Auth;

public sealed record LoginRequest(
    string Identifier,
    string Password,
    string? Role = null);

public sealed record AuthUserDto(
    long UserId,
    byte RoleId,
    string RoleCode,
    string RoleName,
    long? StudentId,
    long? TeacherId,
    string Identifier,
    string FullName,
    string Email,
    string? Phone,
    string? AvatarUrl,
    string Token);

public sealed record RegisterStudentRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string StudentCode,
    string? Phone = null,
    int? AcademicClassId = null,
    int MajorId = 1,
    short EnrollmentYear = 2026);
