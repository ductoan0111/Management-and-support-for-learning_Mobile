using System.ComponentModel.DataAnnotations;
namespace BE_Mobile.Contracts.Admin;

public sealed record AdminSemesterDto(int SemesterId, string SemesterCode, string SemesterName, string AcademicYear, DateOnly StartDate, DateOnly EndDate, bool IsCurrent);

public sealed class SaveAdminSemesterRequest : IValidatableObject
{
    [Required, StringLength(30)] public string SemesterCode { get; set; } = string.Empty;
    [Required, StringLength(100)] public string SemesterName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string AcademicYear { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate < StartDate) yield return new ValidationResult("EndDate must be on or after StartDate.", [nameof(EndDate)]);
    }
}

public sealed class AdminSemesterQuery
{
    [StringLength(200)] public string? Search { get; set; }
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    public bool? IsCurrent { get; set; }
}
