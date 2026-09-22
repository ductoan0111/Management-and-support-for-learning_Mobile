using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Interfaces;

public interface ITeacherMaterialService
{
    Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(long teacherId, long sectionId, string? search, CancellationToken cancellationToken);
    Task<ActionResult<TeacherMaterialDto>> CreateMaterial(long teacherId, long sectionId, CreateMaterialRequest request, CancellationToken cancellationToken);
    Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(long teacherId, long sectionId, long materialId, UpdateMaterialRequest request, CancellationToken cancellationToken);
    Task<IActionResult> DeleteMaterial(long teacherId, long sectionId, long materialId, CancellationToken cancellationToken);
}
