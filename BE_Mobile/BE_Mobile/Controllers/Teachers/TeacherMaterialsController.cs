using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}/sections/{sectionId:long}/materials")]
public sealed class TeacherMaterialsController(ITeacherMaterialService teacherMaterialService) : ControllerBase
{
    /// <summary>Danh sách tài liệu của một lớp học phần.</summary>
    [HttpGet]
    public Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(
        long teacherId,
        long sectionId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => teacherMaterialService.GetMaterials(teacherId, sectionId, search, cancellationToken);

    /// <summary>Thêm tài liệu mới cho lớp học phần.</summary>
    [HttpPost]
    public Task<ActionResult<TeacherMaterialDto>> CreateMaterial(
        long teacherId,
        long sectionId,
        [FromBody] CreateMaterialRequest request,
        CancellationToken cancellationToken)
        => teacherMaterialService.CreateMaterial(teacherId, sectionId, request, cancellationToken);

    /// <summary>Cập nhật tài liệu.</summary>
    [HttpPut("{materialId:long}")]
    public Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(
        long teacherId,
        long sectionId,
        long materialId,
        [FromBody] UpdateMaterialRequest request,
        CancellationToken cancellationToken)
        => teacherMaterialService.UpdateMaterial(teacherId, sectionId, materialId, request, cancellationToken);

    /// <summary>Xóa tài liệu.</summary>
    [HttpDelete("{materialId:long}")]
    public Task<IActionResult> DeleteMaterial(
        long teacherId,
        long sectionId,
        long materialId,
        CancellationToken cancellationToken)
        => teacherMaterialService.DeleteMaterial(teacherId, sectionId, materialId, cancellationToken);
}
