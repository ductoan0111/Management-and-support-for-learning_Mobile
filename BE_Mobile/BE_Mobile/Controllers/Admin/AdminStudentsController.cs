using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Admin;

[ApiController]
[Route("api/admin/students")]
public sealed class AdminStudentsController(IAdminStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminStudentDto>>> GetStudents(
        [FromQuery] string? search,
        [FromQuery] byte? status,
        [FromQuery] int? majorId,
        [FromQuery] int? academicClassId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await studentService.GetStudentsAsync(
            search,
            status,
            majorId,
            academicClassId,
            page,
            pageSize,
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{studentId:long}")]
    public async Task<ActionResult<AdminStudentDto>> GetStudent(
        long studentId,
        CancellationToken cancellationToken)
    {
        var result = await studentService.GetStudentAsync(studentId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("by-user/{userId:long}")]
    public async Task<ActionResult<AdminStudentDto>> GetStudentByUser(
        long userId,
        CancellationToken cancellationToken)
    {
        var result = await studentService.GetStudentByUserAsync(userId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminStudentDto>> CreateStudent(
        CreateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentService.CreateStudentAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(
            nameof(GetStudent),
            new { studentId = result.Value!.StudentId },
            result.Value);
    }

    [HttpPut("{studentId:long}")]
    public async Task<ActionResult<AdminStudentDto>> UpdateStudent(
        long studentId,
        UpdateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentService.UpdateStudentAsync(studentId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{studentId:long}")]
    public async Task<IActionResult> DeleteStudent(
        long studentId,
        CancellationToken cancellationToken)
    {
        var result = await studentService.DeleteStudentAsync(studentId, cancellationToken);
        return this.ToActionResult(result);
    }
}
