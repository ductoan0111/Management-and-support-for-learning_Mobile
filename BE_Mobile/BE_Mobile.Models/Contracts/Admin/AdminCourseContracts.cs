using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminCourseDto(int CourseId, int DepartmentId, string CourseCode, string CourseName, byte Credits, string? Description, bool IsActive);

public sealed class SaveAdminCourseRequest
{
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Required, StringLength(30)] public string CourseCode { get; set; } = string.Empty;
    [Required, StringLength(200)] public string CourseName { get; set; } = string.Empty;
    [Range(1, 15)] public byte Credits { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminCourseQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [Range(1, int.MaxValue)] public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
}
