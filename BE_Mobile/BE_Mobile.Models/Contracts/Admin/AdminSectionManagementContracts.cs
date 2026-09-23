using System.ComponentModel.DataAnnotations;

namespace BE_Mobile.Contracts.Admin;

public sealed class AdminPageQuery
{
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
}
public sealed class AssignAdminTeacherRequest
{
    public bool IsPrimary { get; set; }
}
public sealed class SaveAdminEnrollmentRequest
{
    [Range(0, 2)] public byte Status { get; set; } = 1;
}
public sealed record AdminSectionTeacherDto(long SectionId, long TeacherId, string TeacherCode,
    string FullName, bool IsPrimary, DateTime AssignedAt);
public sealed record AdminEnrollmentDto(long EnrollmentId, long SectionId, long StudentId,
    string StudentCode, string FullName, byte Status, DateTime EnrolledAt);
public sealed record AdminStatisticsDto(int TotalUsers, int ActiveUsers, int TotalStudents, int ActiveStudents,
    int TotalTeachers, int ActiveTeachers, int TotalCourses, int TotalSections, int OpenSections,
    int TotalSemesters, int ActiveEnrollments);
