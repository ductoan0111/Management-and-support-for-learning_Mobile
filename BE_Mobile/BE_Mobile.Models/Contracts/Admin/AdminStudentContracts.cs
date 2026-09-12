namespace BE_Mobile.Contracts.Admin;

public sealed record AdminStudentDto(
    long StudentId,
    long UserId,
    string StudentCode,
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
    short EnrollmentYear,
    byte Status);

public sealed record CreateAdminStudentRequest(
    long UserId,
    string StudentCode,
    int? AcademicClassId,
    int MajorId,
    short EnrollmentYear,
    byte Status = 1);

public sealed record UpdateAdminStudentRequest(
    string StudentCode,
    int? AcademicClassId,
    int MajorId,
    short EnrollmentYear,
    byte Status,
    string? FullName,
    string? Email,
    string? Phone,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? AvatarUrl,
    bool? IsActive);
