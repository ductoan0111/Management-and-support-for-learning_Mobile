using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/teachers")]
public sealed class AdminTeachersController(IAdminTeacherService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminTeacherDto>>> List([FromQuery] AdminTeacherQuery query, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.ListAsync(query, cancellationToken));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminTeacherDto>> Get(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.GetAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<AdminTeacherDto>> Create(SaveAdminTeacherRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveAsync(null, request, cancellationToken);
        return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.TeacherId }, result.Value) : this.ToActionResult(result);
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<AdminTeacherDto>> Update(long id, SaveAdminTeacherRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.SaveAsync(id, request, cancellationToken));
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.DeleteAsync(id, cancellationToken));
}
