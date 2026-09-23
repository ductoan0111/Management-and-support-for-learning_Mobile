using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminMajorDto(int MajorId, int DepartmentId, string MajorCode, string MajorName, string? Description, bool IsActive);

public sealed class SaveAdminMajorRequest
{
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Required, StringLength(20)] public string MajorCode { get; set; } = string.Empty;
    [Required, StringLength(150)] public string MajorName { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminMajorQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [Range(1, int.MaxValue)] public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
}
