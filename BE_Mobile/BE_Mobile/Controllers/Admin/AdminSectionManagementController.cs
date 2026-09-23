using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/course-sections/{sectionId:long}")]
public sealed class AdminSectionManagementController(IAdminSectionManagementService service) : ControllerBase
{
    [HttpGet("teachers")]
    public async Task<ActionResult<PagedResult<AdminSectionTeacherDto>>> Teachers(long sectionId, [FromQuery] AdminPageQuery query, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.TeachersAsync(sectionId, query, cancellationToken));
    [HttpPut("teachers/{teacherId:long}")]
    public async Task<ActionResult<AdminSectionTeacherDto>> AssignTeacher(long sectionId, long teacherId, AssignAdminTeacherRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.AssignTeacherAsync(sectionId, teacherId, request, cancellationToken));
    [HttpDelete("teachers/{teacherId:long}")]
    public async Task<IActionResult> RemoveTeacher(long sectionId, long teacherId, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.RemoveTeacherAsync(sectionId, teacherId, cancellationToken));
    [HttpGet("students")]
    public async Task<ActionResult<PagedResult<AdminEnrollmentDto>>> Students(long sectionId, [FromQuery] AdminPageQuery query, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.StudentsAsync(sectionId, query, cancellationToken));
    [HttpPut("students/{studentId:long}")]
    public async Task<ActionResult<AdminEnrollmentDto>> Enroll(long sectionId, long studentId, SaveAdminEnrollmentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.EnrollAsync(sectionId, studentId, request, cancellationToken));
    [HttpDelete("students/{studentId:long}")]
    public async Task<IActionResult> CancelEnrollment(long sectionId, long studentId, CancellationToken cancellationToken) =>
        this.ToActionResult(await service.CancelEnrollmentAsync(sectionId, studentId, cancellationToken));
}

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/statistics")]
public sealed class AdminStatisticsController(IAdminSectionManagementService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminStatisticsDto>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await service.StatisticsAsync(cancellationToken));
}
