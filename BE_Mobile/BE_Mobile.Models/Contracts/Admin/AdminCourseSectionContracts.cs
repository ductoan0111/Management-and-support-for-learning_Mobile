using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminCourseSectionDto(long SectionId, int CourseId, int SemesterId, string SectionCode, string? SectionName, int? MaxStudents, byte Status);

public sealed class SaveAdminCourseSectionRequest
{
    [Range(1, int.MaxValue)] public int CourseId { get; set; }
    [Range(1, int.MaxValue)] public int SemesterId { get; set; }
    [Required, StringLength(40)] public string SectionCode { get; set; } = string.Empty;
    [StringLength(200)] public string? SectionName { get; set; }
    [Range(1, int.MaxValue)] public int? MaxStudents { get; set; }
    [Range(0, 2)] public byte Status { get; set; }
}

public sealed class AdminCourseSectionQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [Range(1, int.MaxValue)] public int? CourseId { get; set; }
    [Range(1, int.MaxValue)] public int? SemesterId { get; set; }
    [Range(0, 2)] public byte? Status { get; set; }
}
