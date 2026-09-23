using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminDepartmentDto(int DepartmentId, string DepartmentCode, string DepartmentName, string? Description, bool IsActive);

public sealed class SaveAdminDepartmentRequest
{
    [Required, StringLength(20)] public string DepartmentCode { get; set; } = string.Empty;
    [Required, StringLength(150)] public string DepartmentName { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminDepartmentQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    public bool? IsActive { get; set; }
}
