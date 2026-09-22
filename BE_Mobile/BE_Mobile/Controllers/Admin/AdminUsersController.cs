using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/users")]
public sealed class AdminUsersController(IAdminUserService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> List([FromQuery] AdminUserQuery query, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.ListAsync(query, cancellationToken));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminUserDto>> Get(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.GetAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.UserId }, result.Value) : this.ToActionResult(result);
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<AdminUserDto>> Update(long id, UpdateAdminUserRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.UpdateAsync(id, request, cancellationToken));
    [HttpPut("{id:long}/role")]
    public async Task<ActionResult<AdminUserDto>> SetRole(long id, SetAdminRoleRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.SetRoleAsync(id, request, cancellationToken));
    [HttpPut("{id:long}/password")]
    public async Task<IActionResult> ResetPassword(long id, ResetAdminPasswordRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.ResetPasswordAsync(id, request, cancellationToken));
    [HttpGet("/api/admin/roles")]
    public async Task<ActionResult<IReadOnlyList<AdminRoleDto>>> Roles(CancellationToken cancellationToken) =>
        this.ToActionResult(await service.RolesAsync(cancellationToken));
}
