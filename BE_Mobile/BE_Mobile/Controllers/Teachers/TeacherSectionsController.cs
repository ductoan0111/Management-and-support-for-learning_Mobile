using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers.Teachers;

[ApiController]
[Route("api/teachers/{teacherId:long}")]
public sealed class TeacherSectionsController(ITeacherSectionService teacherSectionService) : ControllerBase
{
    // ══════════════════════════════════════════════════════════════════════════
    // HỒ SƠ GIẢNG VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Lấy hồ sơ giảng viên.</summary>
    [HttpGet("profile")]
    public Task<ActionResult<TeacherProfileDto>> GetProfile(
        long teacherId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetProfile(teacherId, cancellationToken);

    /// <summary>Cập nhật hồ sơ giảng viên.</summary>
    [HttpPut("profile")]
    public Task<ActionResult<TeacherProfileDto>> UpdateProfile(
        long teacherId,
        [FromBody] UpdateTeacherProfileRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.UpdateProfile(teacherId, request, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // LỚP HỌC PHẦN
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách lớp học phần giảng viên đang/đã dạy.</summary>
    [HttpGet("sections")]
    public Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(
        long teacherId,
        [FromQuery] int? semesterId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSections(teacherId, semesterId, status, cancellationToken);

    /// <summary>Chi tiết một lớp học phần (kèm thời khóa biểu của lớp).</summary>
    [HttpGet("sections/{sectionId:long}")]
    public Task<ActionResult<TeacherSectionDetailDto>> GetSection(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSection(teacherId, sectionId, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // THỜI KHÓA BIỂU
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Lịch dạy của giảng viên (có thể lọc theo ngày, lớp).</summary>
    [HttpGet("schedule")]
    public Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(
        long teacherId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSchedule(teacherId, from, to, sectionId, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // BÀI TẬP
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách bài tập của một lớp học phần.</summary>
    [HttpGet("sections/{sectionId:long}/assignments")]
    public Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetAssignments(teacherId, sectionId, cancellationToken);

    /// <summary>Chi tiết một bài tập.</summary>
    [HttpGet("sections/{sectionId:long}/assignments/{assignmentId:long}")]
    public Task<ActionResult<TeacherAssignmentDto>> GetAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    /// <summary>Tạo bài tập mới cho lớp học phần.</summary>
    [HttpPost("sections/{sectionId:long}/assignments")]
    public Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(
        long teacherId,
        long sectionId,
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.CreateAssignment(teacherId, sectionId, request, cancellationToken);

    /// <summary>Cập nhật bài tập.</summary>
    [HttpPut("sections/{sectionId:long}/assignments/{assignmentId:long}")]
    public Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        [FromBody] UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.UpdateAssignment(teacherId, sectionId, assignmentId, request, cancellationToken);

    /// <summary>Xóa bài tập.</summary>
    [HttpDelete("sections/{sectionId:long}/assignments/{assignmentId:long}")]
    public Task<IActionResult> DeleteAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        CancellationToken cancellationToken)
        => teacherSectionService.DeleteAssignment(teacherId, sectionId, assignmentId, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // BÀI NỘP
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách bài nộp của sinh viên cho một bài tập.</summary>
    [HttpGet("sections/{sectionId:long}/assignments/{assignmentId:long}/submissions")]
    public Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(
        long teacherId,
        long sectionId,
        long assignmentId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
        => teacherSectionService.GetSubmissions(teacherId, sectionId, assignmentId, status, cancellationToken);

    /// <summary>Chấm điểm một bài nộp của sinh viên.</summary>
    [HttpPut("sections/{sectionId:long}/assignments/{assignmentId:long}/submissions/{submissionId:long}/grade")]
    public Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(
        long teacherId,
        long sectionId,
        long assignmentId,
        long submissionId,
        [FromBody] GradeSubmissionRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.GradeSubmission(teacherId, sectionId, assignmentId, submissionId, request, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // TÀI LIỆU
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách tài liệu của một lớp học phần.</summary>
    [HttpGet("sections/{sectionId:long}/materials")]
    public Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(
        long teacherId,
        long sectionId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => teacherSectionService.GetMaterials(teacherId, sectionId, search, cancellationToken);

    /// <summary>Thêm tài liệu mới cho lớp học phần.</summary>
    [HttpPost("sections/{sectionId:long}/materials")]
    public Task<ActionResult<TeacherMaterialDto>> CreateMaterial(
        long teacherId,
        long sectionId,
        [FromBody] CreateMaterialRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.CreateMaterial(teacherId, sectionId, request, cancellationToken);

    /// <summary>Cập nhật tài liệu.</summary>
    [HttpPut("sections/{sectionId:long}/materials/{materialId:long}")]
    public Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(
        long teacherId,
        long sectionId,
        long materialId,
        [FromBody] UpdateMaterialRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.UpdateMaterial(teacherId, sectionId, materialId, request, cancellationToken);

    /// <summary>Xóa tài liệu.</summary>
    [HttpDelete("sections/{sectionId:long}/materials/{materialId:long}")]
    public Task<IActionResult> DeleteMaterial(
        long teacherId,
        long sectionId,
        long materialId,
        CancellationToken cancellationToken)
        => teacherSectionService.DeleteMaterial(teacherId, sectionId, materialId, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // THÀNH PHẦN ĐIỂM & ĐIỂM SINH VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách thành phần điểm của lớp học phần.</summary>
    [HttpGet("sections/{sectionId:long}/grade-components")]
    public Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetGradeComponents(teacherId, sectionId, cancellationToken);

    /// <summary>Bảng điểm toàn lớp (có thể lọc theo thành phần điểm).</summary>
    [HttpGet("sections/{sectionId:long}/grades")]
    public Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(
        long teacherId,
        long sectionId,
        [FromQuery] long? componentId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetGrades(teacherId, sectionId, componentId, cancellationToken);

    /// <summary>Nhập hoặc cập nhật điểm một thành phần cho một sinh viên.</summary>
    [HttpPut("sections/{sectionId:long}/grades/{studentId:long}")]
    public Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(
        long teacherId,
        long sectionId,
        long studentId,
        [FromBody] UpsertStudentGradeRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.UpsertStudentGrade(teacherId, sectionId, studentId, request, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // THÔNG BÁO
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách thông báo giảng viên đã gửi.</summary>
    [HttpGet("announcements")]
    public Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(
        long teacherId,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetAnnouncements(teacherId, sectionId, cancellationToken);

    /// <summary>Chi tiết một thông báo.</summary>
    [HttpGet("announcements/{announcementId:long}")]
    public Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(
        long teacherId,
        long announcementId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetAnnouncement(teacherId, announcementId, cancellationToken);

    /// <summary>Gửi thông báo đến sinh viên một lớp học phần.</summary>
    [HttpPost("sections/{sectionId:long}/announcements")]
    public Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(
        long teacherId,
        long sectionId,
        [FromBody] CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.CreateAnnouncement(teacherId, sectionId, request, cancellationToken);

    /// <summary>Sửa thông báo đã gửi.</summary>
    [HttpPut("announcements/{announcementId:long}")]
    public Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(
        long teacherId,
        long announcementId,
        [FromBody] UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
        => teacherSectionService.UpdateAnnouncement(teacherId, announcementId, request, cancellationToken);

    /// <summary>Xóa thông báo.</summary>
    [HttpDelete("announcements/{announcementId:long}")]
    public Task<IActionResult> DeleteAnnouncement(
        long teacherId,
        long announcementId,
        CancellationToken cancellationToken)
        => teacherSectionService.DeleteAnnouncement(teacherId, announcementId, cancellationToken);

    // ══════════════════════════════════════════════════════════════════════════
    // SINH VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Danh sách sinh viên đăng ký một lớp học phần.</summary>
    [HttpGet("sections/{sectionId:long}/students")]
    public async Task<ActionResult<IReadOnlyList<TeacherSectionStudentDto>>> GetStudents(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        var students = await teacherSectionService.GetStudentsBySectionAsync(sectionId, cancellationToken);
        return Ok(students);
    }

    /// <summary>Chi tiết hồ sơ và kết quả học tập của một sinh viên trong lớp.</summary>
    [HttpGet("sections/{sectionId:long}/students/{studentId:long}")]
    public Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(
        long teacherId,
        long sectionId,
        long studentId,
        CancellationToken cancellationToken)
        => teacherSectionService.GetStudentInSection(teacherId, sectionId, studentId, cancellationToken);
}
