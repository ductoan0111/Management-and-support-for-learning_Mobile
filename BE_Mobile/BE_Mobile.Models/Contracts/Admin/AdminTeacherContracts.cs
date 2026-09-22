using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminTeacherDto(long TeacherId, long UserId, string TeacherCode, int DepartmentId, string? AcademicTitle, string? Specialization, byte Status);

public sealed class SaveAdminTeacherRequest
{
    [Range(1, long.MaxValue)] public long UserId { get; set; }
    [Required, StringLength(30)] public string TeacherCode { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [StringLength(100)] public string? AcademicTitle { get; set; }
    [StringLength(255)] public string? Specialization { get; set; }
    [Range(0, 1)] public byte Status { get; set; }
}

public sealed class AdminTeacherQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [Range(1, int.MaxValue)] public int? DepartmentId { get; set; }
    [Range(0, 1)] public byte? Status { get; set; }
}
