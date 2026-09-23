using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminAcademicClassDto(int AcademicClassId, int MajorId, string ClassCode, string ClassName, short IntakeYear, short? GraduationYear, bool IsActive);

public sealed class SaveAdminAcademicClassRequest : IValidatableObject
{
    [Range(1, int.MaxValue)] public int MajorId { get; set; }
    [Required, StringLength(30)] public string ClassCode { get; set; } = string.Empty;
    [Required, StringLength(150)] public string ClassName { get; set; } = string.Empty;
    [Range(1900, 2100)] public short IntakeYear { get; set; }
    [Range(1900, 2100)] public short? GraduationYear { get; set; }
    public bool IsActive { get; set; } = true;
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GraduationYear < IntakeYear) yield return new ValidationResult("GraduationYear must be on or after IntakeYear.", [nameof(GraduationYear)]);
    }
}

public sealed class AdminAcademicClassQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [Range(1, int.MaxValue)] public int? MajorId { get; set; }
    public bool? IsActive { get; set; }
}
