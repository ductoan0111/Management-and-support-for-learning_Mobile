using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/course-sections")]
public sealed class AdminCourseSectionsController(IAdminCourseSectionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminCourseSectionDto>>> List([FromQuery] AdminCourseSectionQuery query, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.ListAsync(query, cancellationToken));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminCourseSectionDto>> Get(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.GetAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<AdminCourseSectionDto>> Create(SaveAdminCourseSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveAsync(null, request, cancellationToken);
        return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.SectionId }, result.Value) : this.ToActionResult(result);
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<AdminCourseSectionDto>> Update(long id, SaveAdminCourseSectionRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.SaveAsync(id, request, cancellationToken));
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.DeleteAsync(id, cancellationToken));
}
