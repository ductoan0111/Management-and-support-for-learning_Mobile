using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherMaterialService(ITeacherMaterialRepository teacherMaterialRepository) : ITeacherMaterialService
{
    public Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(long teacherId, long sectionId, string? search, CancellationToken cancellationToken)
        => teacherMaterialRepository.GetMaterials(teacherId, sectionId, search, cancellationToken);

    public Task<ActionResult<TeacherMaterialDto>> CreateMaterial(long teacherId, long sectionId, CreateMaterialRequest request, CancellationToken cancellationToken)
        => teacherMaterialRepository.CreateMaterial(teacherId, sectionId, request, cancellationToken);

    public Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(long teacherId, long sectionId, long materialId, UpdateMaterialRequest request, CancellationToken cancellationToken)
        => teacherMaterialRepository.UpdateMaterial(teacherId, sectionId, materialId, request, cancellationToken);

    public Task<IActionResult> DeleteMaterial(long teacherId, long sectionId, long materialId, CancellationToken cancellationToken)
        => teacherMaterialRepository.DeleteMaterial(teacherId, sectionId, materialId, cancellationToken);
}
