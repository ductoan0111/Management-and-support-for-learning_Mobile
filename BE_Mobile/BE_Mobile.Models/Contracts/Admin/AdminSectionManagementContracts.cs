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
public sealed class SaveAdminClassScheduleRequest : IValidatableObject
{
    [Range(2, 8)] public byte DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    [StringLength(50)] public string? Room { get; set; }
    [StringLength(100)] public string? Building { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    [StringLength(500)] public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
            yield return new ValidationResult("Giờ kết thúc phải sau giờ bắt đầu.", [nameof(EndTime)]);
        if (EffectiveTo < EffectiveFrom)
            yield return new ValidationResult("Ngày kết thúc phải từ ngày bắt đầu trở đi.", [nameof(EffectiveTo)]);
    }
}
public sealed record AdminSectionTeacherDto(long SectionId, long TeacherId, string TeacherCode,
    string FullName, bool IsPrimary, DateTime AssignedAt);
public sealed record AdminClassScheduleDto(long ScheduleId, long SectionId, byte DayOfWeek,
    string StartTime, string EndTime, string? Room, string? Building,
    DateOnly EffectiveFrom, DateOnly EffectiveTo, string? Note);
public sealed record AdminEnrollmentDto(long EnrollmentId, long SectionId, long StudentId,
    string StudentCode, string FullName, byte Status, DateTime EnrolledAt);
public sealed record AdminStatisticsDto(int TotalUsers, int ActiveUsers, int TotalStudents, int ActiveStudents,
    int TotalTeachers, int ActiveTeachers, int TotalCourses, int TotalSections, int OpenSections,
    int TotalSemesters, int ActiveEnrollments);
public sealed record AdminChartPointDto(string Label, int Value);
public sealed record AdminSemesterReportDto(string Label, int Sections, int Enrollments);
public sealed record AdminReportDto(IReadOnlyList<AdminChartPointDto> StudentsByDepartment,
    IReadOnlyList<AdminChartPointDto> StudentsByMajor, IReadOnlyList<AdminSemesterReportDto> SectionsBySemester,
    IReadOnlyList<AdminChartPointDto> GradeDistribution, double AverageScore, int GradedEnrollments);
