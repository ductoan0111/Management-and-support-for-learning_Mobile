using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers")]
public sealed class TeacherSectionsController(ITeacherSectionService teacherSectionService) : ControllerBase
{
    [HttpGet("sections/{sectionId:long}/students")]
    public async Task<ActionResult<IReadOnlyList<TeacherSectionStudentDto>>> GetStudentsBySection(
        long sectionId,
        CancellationToken cancellationToken)
    {
        var students = await teacherSectionService.GetStudentsBySectionAsync(sectionId, cancellationToken);
        return Ok(students);
    }
}
