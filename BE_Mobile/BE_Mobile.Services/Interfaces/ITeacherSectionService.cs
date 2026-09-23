using BE_Mobile.Contracts.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Services.Interfaces;

public interface ITeacherSectionService
{
    // ── Hồ sơ ─────────────────────────────────────────────────────────────────
    Task<ActionResult<TeacherProfileDto>> GetProfile(long teacherId, CancellationToken cancellationToken);
    Task<ActionResult<TeacherProfileDto>> UpdateProfile(long teacherId, UpdateTeacherProfileRequest request, CancellationToken cancellationToken);

    // ── Lớp học phần ─────────────────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(long teacherId, int? semesterId, byte? status, CancellationToken cancellationToken);
    Task<ActionResult<TeacherSectionDetailDto>> GetSection(long teacherId, long sectionId, CancellationToken cancellationToken);

    // ── Thời khóa biểu / Lịch dạy ────────────────────────────────────────────
    Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(long teacherId, DateOnly? from, DateOnly? to, long? sectionId, CancellationToken cancellationToken);
}
