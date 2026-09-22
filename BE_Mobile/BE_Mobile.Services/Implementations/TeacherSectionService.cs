using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Implementations;

public sealed class TeacherSectionService(ITeacherSectionRepository teacherSectionRepository) : ITeacherSectionService
{
    public Task<ActionResult<TeacherProfileDto>> GetProfile(long teacherId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetProfile(teacherId, cancellationToken);

    public Task<ActionResult<TeacherProfileDto>> UpdateProfile(long teacherId, UpdateTeacherProfileRequest request, CancellationToken cancellationToken)
        => teacherSectionRepository.UpdateProfile(teacherId, request, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(long teacherId, int? semesterId, byte? status, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSections(teacherId, semesterId, status, cancellationToken);

    public Task<ActionResult<TeacherSectionDetailDto>> GetSection(long teacherId, long sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSection(teacherId, sectionId, cancellationToken);

    public Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(long teacherId, DateOnly? from, DateOnly? to, long? sectionId, CancellationToken cancellationToken)
        => teacherSectionRepository.GetSchedule(teacherId, from, to, sectionId, cancellationToken);
}
