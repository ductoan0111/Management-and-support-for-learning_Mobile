using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/departments")]
public sealed class AdminDepartmentsController(IAdminDepartmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminDepartmentDto>>> List([FromQuery] AdminDepartmentQuery query, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.ListAsync(query, cancellationToken));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminDepartmentDto>> Get(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.GetAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<AdminDepartmentDto>> Create(SaveAdminDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveAsync(null, request, cancellationToken);
        return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.DepartmentId }, result.Value) : this.ToActionResult(result);
    }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminDepartmentDto>> Update(int id, SaveAdminDepartmentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.SaveAsync(id, request, cancellationToken));
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.DeleteAsync(id, cancellationToken));
}
