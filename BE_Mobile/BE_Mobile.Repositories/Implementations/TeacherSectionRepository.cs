using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class TeacherSectionRepository(IDbConnectionFactory connectionFactory)
    : ControllerBase, ITeacherSectionRepository
{
    // ══════════════════════════════════════════════════════════════════════════
    // HỒ SƠ GIẢNG VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<TeacherProfileDto>> GetProfile(
        long teacherId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var profile = await FindTeacherProfileAsync(connection, teacherId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    public async Task<ActionResult<TeacherProfileDto>> UpdateProfile(
        long teacherId,
        UpdateTeacherProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "Họ tên không được để trống." });

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Lấy UserId từ TeacherId
        await using var cmdUserId = connection.CreateCommand();
        cmdUserId.CommandText = "SELECT u.UserId FROM dbo.Teachers t INNER JOIN dbo.Users u ON u.UserId = t.UserId WHERE t.TeacherId = @TeacherId";
        AddParameter(cmdUserId, "@TeacherId", SqlDbType.BigInt, teacherId);
        var userIdRaw = await cmdUserId.ExecuteScalarAsync(cancellationToken);
        if (userIdRaw is null or DBNull)
            return NotFound();

        var userId = (long)userIdRaw;

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.Users
            SET FullName   = @FullName,
                Phone      = @Phone,
                DateOfBirth = @DateOfBirth,
                Gender     = @Gender,
                AvatarUrl  = @AvatarUrl,
                UpdatedAt  = SYSDATETIME()
            WHERE UserId = @UserId;

            UPDATE dbo.Teachers
            SET AcademicTitle  = @AcademicTitle,
                Specialization = @Specialization
            WHERE TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@UserId", SqlDbType.BigInt, userId);
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        AddParameter(cmd, "@FullName", SqlDbType.NVarChar, request.FullName.Trim(), 150);
        AddParameter(cmd, "@Phone", SqlDbType.VarChar, NormalizeText(request.Phone), 20);
        AddParameter(cmd, "@DateOfBirth", SqlDbType.Date, request.DateOfBirth);
        AddParameter(cmd, "@Gender", SqlDbType.TinyInt, request.Gender);
        AddParameter(cmd, "@AvatarUrl", SqlDbType.NVarChar, NormalizeText(request.AvatarUrl), 1000);
        AddParameter(cmd, "@AcademicTitle", SqlDbType.NVarChar, NormalizeText(request.AcademicTitle), 100);
        AddParameter(cmd, "@Specialization", SqlDbType.NVarChar, NormalizeText(request.Specialization), 255);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        var profile = await FindTeacherProfileAsync(connection, teacherId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // LỚP HỌC PHẦN
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherSectionDto>>> GetSections(
        long teacherId,
        int? semesterId,
        byte? status,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT cs.SectionId,
                   cs.SectionCode,
                   cs.SectionName,
                   c.CourseId,
                   c.CourseCode,
                   c.CourseName,
                   c.Credits,
                   s.SemesterId,
                   s.SemesterCode,
                   s.SemesterName,
                   s.AcademicYear,
                   cs.Status,
                   cs.MaxStudents,
                   COUNT(e.EnrollmentId) AS EnrolledCount,
                   st.IsPrimary
            FROM   dbo.CourseSections cs
            INNER JOIN dbo.SectionTeachers st  ON st.SectionId  = cs.SectionId
            INNER JOIN dbo.Courses         c   ON c.CourseId    = cs.CourseId
            INNER JOIN dbo.Semesters       s   ON s.SemesterId  = cs.SemesterId
            LEFT  JOIN dbo.Enrollments     e   ON e.SectionId   = cs.SectionId AND e.Status IN (1,2)
            WHERE st.TeacherId = @TeacherId
              AND (@SemesterId IS NULL OR cs.SemesterId = @SemesterId)
              AND (@Status     IS NULL OR cs.Status     = @Status)
            GROUP BY cs.SectionId, cs.SectionCode, cs.SectionName,
                     c.CourseId, c.CourseCode, c.CourseName, c.Credits,
                     s.SemesterId, s.SemesterCode, s.SemesterName, s.AcademicYear,
                     cs.Status, cs.MaxStudents, st.IsPrimary
            ORDER BY s.AcademicYear DESC, s.SemesterId DESC, c.CourseCode;
            """;
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        AddParameter(cmd, "@SemesterId", SqlDbType.Int, semesterId);
        AddParameter(cmd, "@Status", SqlDbType.TinyInt, status);

        var sections = new List<TeacherSectionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sections.Add(new TeacherSectionDto(
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("SectionCode")),
                GetNullableString(reader, "SectionName"),
                reader.GetInt32(reader.GetOrdinal("CourseId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetByte(reader.GetOrdinal("Credits")),
                reader.GetInt32(reader.GetOrdinal("SemesterId")),
                reader.GetString(reader.GetOrdinal("SemesterCode")),
                reader.GetString(reader.GetOrdinal("SemesterName")),
                reader.GetString(reader.GetOrdinal("AcademicYear")),
                reader.GetByte(reader.GetOrdinal("Status")),
                GetNullableInt(reader, "MaxStudents"),
                reader.GetInt32(reader.GetOrdinal("EnrolledCount")),
                reader.GetBoolean(reader.GetOrdinal("IsPrimary"))));
        }

        return Ok(sections);
    }

    public async Task<ActionResult<TeacherSectionDetailDto>> GetSection(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT cs.SectionId,
                   cs.SectionCode,
                   cs.SectionName,
                   c.CourseId,
                   c.CourseCode,
                   c.CourseName,
                   c.Credits,
                   s.SemesterId,
                   s.SemesterCode,
                   s.SemesterName,
                   s.AcademicYear,
                   cs.Status,
                   cs.MaxStudents,
                   COUNT(e.EnrollmentId) AS EnrolledCount,
                   st.IsPrimary
            FROM   dbo.CourseSections cs
            INNER JOIN dbo.SectionTeachers st  ON st.SectionId  = cs.SectionId
            INNER JOIN dbo.Courses         c   ON c.CourseId    = cs.CourseId
            INNER JOIN dbo.Semesters       s   ON s.SemesterId  = cs.SemesterId
            LEFT  JOIN dbo.Enrollments     e   ON e.SectionId   = cs.SectionId AND e.Status IN (1,2)
            WHERE st.TeacherId = @TeacherId AND cs.SectionId = @SectionId
            GROUP BY cs.SectionId, cs.SectionCode, cs.SectionName,
                     c.CourseId, c.CourseCode, c.CourseName, c.Credits,
                     s.SemesterId, s.SemesterCode, s.SemesterName, s.AcademicYear,
                     cs.Status, cs.MaxStudents, st.IsPrimary;

            SELECT sch.ScheduleId,
                   cs.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   sch.DayOfWeek,
                   CONVERT(VARCHAR(8), sch.StartTime, 108) AS StartTime,
                   CONVERT(VARCHAR(8), sch.EndTime,   108) AS EndTime,
                   sch.Room,
                   sch.Building,
                   sch.EffectiveFrom,
                   sch.EffectiveTo,
                   sch.Note
            FROM   dbo.ClassSchedules sch
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = sch.SectionId
            INNER JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            WHERE  sch.SectionId = @SectionId
            ORDER BY sch.DayOfWeek, sch.StartTime;
            """;
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound();

        var detail = new
        {
            SectionId    = reader.GetInt64(reader.GetOrdinal("SectionId")),
            SectionCode  = reader.GetString(reader.GetOrdinal("SectionCode")),
            SectionName  = GetNullableString(reader, "SectionName"),
            CourseId     = reader.GetInt32(reader.GetOrdinal("CourseId")),
            CourseCode   = reader.GetString(reader.GetOrdinal("CourseCode")),
            CourseName   = reader.GetString(reader.GetOrdinal("CourseName")),
            Credits      = reader.GetByte(reader.GetOrdinal("Credits")),
            SemesterId   = reader.GetInt32(reader.GetOrdinal("SemesterId")),
            SemesterCode = reader.GetString(reader.GetOrdinal("SemesterCode")),
            SemesterName = reader.GetString(reader.GetOrdinal("SemesterName")),
            AcademicYear = reader.GetString(reader.GetOrdinal("AcademicYear")),
            Status       = reader.GetByte(reader.GetOrdinal("Status")),
            MaxStudents  = GetNullableInt(reader, "MaxStudents"),
            EnrolledCount = reader.GetInt32(reader.GetOrdinal("EnrolledCount")),
            IsPrimary    = reader.GetBoolean(reader.GetOrdinal("IsPrimary")),
        };

        var schedules = new List<TeacherScheduleDto>();
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                schedules.Add(ReadSchedule(reader));
            }
        }

        return Ok(new TeacherSectionDetailDto(
            detail.SectionId, detail.SectionCode, detail.SectionName,
            detail.CourseId, detail.CourseCode, detail.CourseName, detail.Credits,
            detail.SemesterId, detail.SemesterCode, detail.SemesterName, detail.AcademicYear,
            detail.Status, detail.MaxStudents, detail.EnrolledCount, detail.IsPrimary,
            schedules));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // THỜI KHÓA BIỂU
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherScheduleDto>>> GetSchedule(
        long teacherId,
        DateOnly? from,
        DateOnly? to,
        long? sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT sch.ScheduleId,
                   cs.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   sch.DayOfWeek,
                   CONVERT(VARCHAR(8), sch.StartTime, 108) AS StartTime,
                   CONVERT(VARCHAR(8), sch.EndTime,   108) AS EndTime,
                   sch.Room,
                   sch.Building,
                   sch.EffectiveFrom,
                   sch.EffectiveTo,
                   sch.Note
            FROM   dbo.ClassSchedules sch
            INNER JOIN dbo.CourseSections  cs  ON cs.SectionId  = sch.SectionId
            INNER JOIN dbo.SectionTeachers st  ON st.SectionId  = cs.SectionId
            INNER JOIN dbo.Courses         c   ON c.CourseId    = cs.CourseId
            WHERE  st.TeacherId = @TeacherId
              AND (@SectionId IS NULL OR cs.SectionId  = @SectionId)
              AND (@From      IS NULL OR sch.EffectiveTo   >= @From)
              AND (@To        IS NULL OR sch.EffectiveFrom <= @To)
            ORDER BY sch.DayOfWeek, sch.StartTime;
            """;
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@From", SqlDbType.Date, from);
        AddParameter(cmd, "@To", SqlDbType.Date, to);

        var list = new List<TeacherScheduleDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(ReadSchedule(reader));

        return Ok(list);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BÀI TẬP
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT a.AssignmentId,
                   cs.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   a.Title,
                   a.Description,
                   a.AttachmentUrl,
                   a.OpenAt,
                   a.DueAt,
                   a.MaxScore,
                   a.AllowLateSubmission,
                   a.IsPublished,
                   a.CreatedAt,
                   a.UpdatedAt,
                   COUNT(sub.SubmissionId)                                AS TotalSubmissions,
                   COUNT(CASE WHEN sub.Status = 2 THEN 1 END)            AS GradedSubmissions
            FROM   dbo.Assignments a
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = a.SectionId
            INNER JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            LEFT  JOIN dbo.AssignmentSubmissions sub ON sub.AssignmentId = a.AssignmentId
            WHERE  a.SectionId = @SectionId
            GROUP BY a.AssignmentId, cs.SectionId, cs.SectionCode, c.CourseCode, c.CourseName,
                     a.Title, a.Description, a.AttachmentUrl, a.OpenAt, a.DueAt,
                     a.MaxScore, a.AllowLateSubmission, a.IsPublished, a.CreatedAt, a.UpdatedAt
            ORDER BY a.DueAt DESC;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        var list = new List<TeacherAssignmentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(ReadAssignment(reader));

        return Ok(list);
    }

    public async Task<ActionResult<TeacherAssignmentDto>> GetAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT a.AssignmentId,
                   cs.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   a.Title,
                   a.Description,
                   a.AttachmentUrl,
                   a.OpenAt,
                   a.DueAt,
                   a.MaxScore,
                   a.AllowLateSubmission,
                   a.IsPublished,
                   a.CreatedAt,
                   a.UpdatedAt,
                   COUNT(sub.SubmissionId)                                AS TotalSubmissions,
                   COUNT(CASE WHEN sub.Status = 2 THEN 1 END)            AS GradedSubmissions
            FROM   dbo.Assignments a
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = a.SectionId
            INNER JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            LEFT  JOIN dbo.AssignmentSubmissions sub ON sub.AssignmentId = a.AssignmentId
            WHERE  a.AssignmentId = @AssignmentId AND a.SectionId = @SectionId
            GROUP BY a.AssignmentId, cs.SectionId, cs.SectionCode, c.CourseCode, c.CourseName,
                     a.Title, a.Description, a.AttachmentUrl, a.OpenAt, a.DueAt,
                     a.MaxScore, a.AllowLateSubmission, a.IsPublished, a.CreatedAt, a.UpdatedAt;
            """;
        AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound();

        return Ok(ReadAssignment(reader));
    }

    public async Task<ActionResult<TeacherAssignmentDto>> CreateAssignment(
        long teacherId,
        long sectionId,
        CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Tiêu đề bài tập không được để trống." });
        if (request.MaxScore <= 0)
            return BadRequest(new { message = "Điểm tối đa phải lớn hơn 0." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await GetUserIdByTeacherAsync(teacherId, cancellationToken);
        if (userId is null) return NotFound();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO dbo.Assignments
                (SectionId, CreatedByUserId, Title, Description, AttachmentUrl,
                 OpenAt, DueAt, MaxScore, AllowLateSubmission, IsPublished, CreatedAt)
            OUTPUT INSERTED.AssignmentId
            VALUES
                (@SectionId, @CreatedByUserId, @Title, @Description, @AttachmentUrl,
                 @OpenAt, @DueAt, @MaxScore, @AllowLateSubmission, @IsPublished, SYSDATETIME());
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@CreatedByUserId", SqlDbType.BigInt, userId.Value);
        AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        AddParameter(cmd, "@Description", SqlDbType.NVarChar, NormalizeText(request.Description));
        AddParameter(cmd, "@AttachmentUrl", SqlDbType.NVarChar, NormalizeText(request.AttachmentUrl), 1000);
        AddParameter(cmd, "@OpenAt", SqlDbType.DateTime2, request.OpenAt);
        AddParameter(cmd, "@DueAt", SqlDbType.DateTime2, request.DueAt);
        AddParameter(cmd, "@MaxScore", SqlDbType.Decimal, request.MaxScore);
        AddParameter(cmd, "@AllowLateSubmission", SqlDbType.Bit, request.AllowLateSubmission);
        AddParameter(cmd, "@IsPublished", SqlDbType.Bit, request.IsPublished);

        var newId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        return await GetAssignment(teacherId, sectionId, newId, cancellationToken);
    }

    public async Task<ActionResult<TeacherAssignmentDto>> UpdateAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Tiêu đề bài tập không được để trống." });
        if (request.MaxScore <= 0)
            return BadRequest(new { message = "Điểm tối đa phải lớn hơn 0." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.Assignments
            SET Title               = @Title,
                Description         = @Description,
                AttachmentUrl       = @AttachmentUrl,
                OpenAt              = @OpenAt,
                DueAt               = @DueAt,
                MaxScore            = @MaxScore,
                AllowLateSubmission = @AllowLateSubmission,
                IsPublished         = @IsPublished,
                UpdatedAt           = SYSDATETIME()
            WHERE AssignmentId = @AssignmentId AND SectionId = @SectionId;
            """;
        AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        AddParameter(cmd, "@Description", SqlDbType.NVarChar, NormalizeText(request.Description));
        AddParameter(cmd, "@AttachmentUrl", SqlDbType.NVarChar, NormalizeText(request.AttachmentUrl), 1000);
        AddParameter(cmd, "@OpenAt", SqlDbType.DateTime2, request.OpenAt);
        AddParameter(cmd, "@DueAt", SqlDbType.DateTime2, request.DueAt);
        AddParameter(cmd, "@MaxScore", SqlDbType.Decimal, request.MaxScore);
        AddParameter(cmd, "@AllowLateSubmission", SqlDbType.Bit, request.AllowLateSubmission);
        AddParameter(cmd, "@IsPublished", SqlDbType.Bit, request.IsPublished);

        if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
            return NotFound();

        return await GetAssignment(teacherId, sectionId, assignmentId, cancellationToken);
    }

    public async Task<IActionResult> DeleteAssignment(
        long teacherId,
        long sectionId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Assignments WHERE AssignmentId = @AssignmentId AND SectionId = @SectionId;";
        AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BÀI NỘP
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(
        long teacherId,
        long sectionId,
        long assignmentId,
        byte? status,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT sub.SubmissionId,
                   sub.AssignmentId,
                   a.Title          AS AssignmentTitle,
                   st.StudentId,
                   st.StudentCode,
                   u.FullName,
                   sub.TextContent,
                   sub.FileUrl,
                   sub.SubmittedAt,
                   sub.IsLate,
                   sub.Status,
                   sub.Score,
                   sub.Feedback,
                   sub.GradedAt
            FROM   dbo.AssignmentSubmissions sub
            INNER JOIN dbo.Assignments a   ON a.AssignmentId = sub.AssignmentId
            INNER JOIN dbo.Students   st  ON st.StudentId   = sub.StudentId
            INNER JOIN dbo.Users      u   ON u.UserId        = st.UserId
            WHERE  sub.AssignmentId = @AssignmentId
              AND  a.SectionId      = @SectionId
              AND  (@Status IS NULL OR sub.Status = @Status)
            ORDER BY sub.SubmittedAt DESC;
            """;
        AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@Status", SqlDbType.TinyInt, status);

        var list = new List<TeacherSubmissionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(ReadSubmission(reader));

        return Ok(list);
    }

    public async Task<ActionResult<TeacherSubmissionDto>> GradeSubmission(
        long teacherId,
        long sectionId,
        long assignmentId,
        long submissionId,
        GradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Score < 0)
            return BadRequest(new { message = "Điểm không được âm." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await GetUserIdByTeacherAsync(teacherId, cancellationToken);
        if (userId is null) return NotFound();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.AssignmentSubmissions
            SET Score          = @Score,
                Feedback       = @Feedback,
                GradedByUserId = @GradedByUserId,
                GradedAt       = SYSDATETIME(),
                Status         = 2
            WHERE SubmissionId  = @SubmissionId
              AND AssignmentId  = @AssignmentId;
            """;
        AddParameter(cmd, "@SubmissionId", SqlDbType.BigInt, submissionId);
        AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(cmd, "@Score", SqlDbType.Decimal, request.Score);
        AddParameter(cmd, "@Feedback", SqlDbType.NVarChar, NormalizeText(request.Feedback));
        AddParameter(cmd, "@GradedByUserId", SqlDbType.BigInt, userId.Value);

        if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
            return NotFound();

        // Trả về bài nộp đã cập nhật
        await using var cmdGet = connection.CreateCommand();
        cmdGet.CommandText = """
            SELECT sub.SubmissionId,
                   sub.AssignmentId,
                   a.Title     AS AssignmentTitle,
                   st.StudentId,
                   st.StudentCode,
                   u.FullName,
                   sub.TextContent,
                   sub.FileUrl,
                   sub.SubmittedAt,
                   sub.IsLate,
                   sub.Status,
                   sub.Score,
                   sub.Feedback,
                   sub.GradedAt
            FROM   dbo.AssignmentSubmissions sub
            INNER JOIN dbo.Assignments a  ON a.AssignmentId = sub.AssignmentId
            INNER JOIN dbo.Students   st ON st.StudentId   = sub.StudentId
            INNER JOIN dbo.Users      u  ON u.UserId        = st.UserId
            WHERE  sub.SubmissionId = @SubmissionId;
            """;
        AddParameter(cmdGet, "@SubmissionId", SqlDbType.BigInt, submissionId);

        await using var reader = await cmdGet.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound();

        return Ok(ReadSubmission(reader));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TÀI LIỆU
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(
        long teacherId,
        long sectionId,
        string? search,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT m.MaterialId,
                   cs.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   m.Title,
                   m.Description,
                   m.MaterialType,
                   m.FileUrl,
                   m.ExternalUrl,
                   m.IsVisible,
                   m.CreatedAt,
                   m.UpdatedAt
            FROM   dbo.Materials      m
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = m.SectionId
            INNER JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            WHERE  m.SectionId = @SectionId
              AND  (@Search IS NULL OR m.Title LIKE N'%' + @Search + N'%')
            ORDER BY m.CreatedAt DESC;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@Search", SqlDbType.NVarChar, NormalizeText(search), 250);

        var list = new List<TeacherMaterialDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(ReadMaterial(reader));

        return Ok(list);
    }

    public async Task<ActionResult<TeacherMaterialDto>> CreateMaterial(
        long teacherId,
        long sectionId,
        CreateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Tiêu đề tài liệu không được để trống." });
        if (string.IsNullOrWhiteSpace(request.FileUrl) && string.IsNullOrWhiteSpace(request.ExternalUrl))
            return BadRequest(new { message = "Phải cung cấp FileUrl hoặc ExternalUrl." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await GetUserIdByTeacherAsync(teacherId, cancellationToken);
        if (userId is null) return NotFound();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO dbo.Materials
                (SectionId, UploadedByUserId, Title, Description, MaterialType,
                 FileUrl, ExternalUrl, IsVisible, CreatedAt)
            OUTPUT INSERTED.MaterialId
            VALUES
                (@SectionId, @UploadedByUserId, @Title, @Description, @MaterialType,
                 @FileUrl, @ExternalUrl, @IsVisible, SYSDATETIME());
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@UploadedByUserId", SqlDbType.BigInt, userId.Value);
        AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        AddParameter(cmd, "@Description", SqlDbType.NVarChar, NormalizeText(request.Description));
        AddParameter(cmd, "@MaterialType", SqlDbType.VarChar, NormalizeText(request.MaterialType), 30);
        AddParameter(cmd, "@FileUrl", SqlDbType.NVarChar, NormalizeText(request.FileUrl), 1000);
        AddParameter(cmd, "@ExternalUrl", SqlDbType.NVarChar, NormalizeText(request.ExternalUrl), 1000);
        AddParameter(cmd, "@IsVisible", SqlDbType.Bit, request.IsVisible);

        var newId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;

        await using var cmdGet = connection.CreateCommand();
        cmdGet.CommandText = """
            SELECT m.MaterialId, cs.SectionId, cs.SectionCode, c.CourseCode, c.CourseName,
                   m.Title, m.Description, m.MaterialType, m.FileUrl, m.ExternalUrl,
                   m.IsVisible, m.CreatedAt, m.UpdatedAt
            FROM   dbo.Materials m
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = m.SectionId
            INNER JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            WHERE  m.MaterialId = @MaterialId;
            """;
        AddParameter(cmdGet, "@MaterialId", SqlDbType.BigInt, newId);

        await using var reader = await cmdGet.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return Created(string.Empty, ReadMaterial(reader));
    }

    public async Task<ActionResult<TeacherMaterialDto>> UpdateMaterial(
        long teacherId,
        long sectionId,
        long materialId,
        UpdateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Tiêu đề tài liệu không được để trống." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.Materials
            SET Title        = @Title,
                Description  = @Description,
                MaterialType = @MaterialType,
                FileUrl      = @FileUrl,
                ExternalUrl  = @ExternalUrl,
                IsVisible    = @IsVisible,
                UpdatedAt    = SYSDATETIME()
            WHERE MaterialId = @MaterialId AND SectionId = @SectionId;
            """;
        AddParameter(cmd, "@MaterialId", SqlDbType.BigInt, materialId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        AddParameter(cmd, "@Description", SqlDbType.NVarChar, NormalizeText(request.Description));
        AddParameter(cmd, "@MaterialType", SqlDbType.VarChar, NormalizeText(request.MaterialType), 30);
        AddParameter(cmd, "@FileUrl", SqlDbType.NVarChar, NormalizeText(request.FileUrl), 1000);
        AddParameter(cmd, "@ExternalUrl", SqlDbType.NVarChar, NormalizeText(request.ExternalUrl), 1000);
        AddParameter(cmd, "@IsVisible", SqlDbType.Bit, request.IsVisible);

        if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
            return NotFound();

        await using var cmdGet = connection.CreateCommand();
        cmdGet.CommandText = """
            SELECT m.MaterialId, cs.SectionId, cs.SectionCode, c.CourseCode, c.CourseName,
                   m.Title, m.Description, m.MaterialType, m.FileUrl, m.ExternalUrl,
                   m.IsVisible, m.CreatedAt, m.UpdatedAt
            FROM   dbo.Materials m
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = m.SectionId
            INNER JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            WHERE  m.MaterialId = @MaterialId;
            """;
        AddParameter(cmdGet, "@MaterialId", SqlDbType.BigInt, materialId);

        await using var reader = await cmdGet.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();
        return Ok(ReadMaterial(reader));
    }

    public async Task<IActionResult> DeleteMaterial(
        long teacherId,
        long sectionId,
        long materialId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Materials WHERE MaterialId = @MaterialId AND SectionId = @SectionId;";
        AddParameter(cmd, "@MaterialId", SqlDbType.BigInt, materialId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // THÀNH PHẦN ĐIỂM & ĐIỂM SINH VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT gc.GradeComponentId,
                   gc.SectionId,
                   gc.ComponentName,
                   gc.WeightPercent,
                   gc.MaxScore,
                   gc.DisplayOrder
            FROM   dbo.GradeComponents gc
            WHERE  gc.SectionId = @SectionId
            ORDER BY gc.DisplayOrder;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        var list = new List<TeacherGradeComponentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new TeacherGradeComponentDto(
                reader.GetInt64(reader.GetOrdinal("GradeComponentId")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("ComponentName")),
                reader.GetDecimal(reader.GetOrdinal("WeightPercent")),
                reader.GetDecimal(reader.GetOrdinal("MaxScore")),
                reader.GetInt32(reader.GetOrdinal("DisplayOrder"))));
        }

        return Ok(list);
    }

    public async Task<ActionResult<IReadOnlyList<TeacherStudentGradeDto>>> GetGrades(
        long teacherId,
        long sectionId,
        long? componentId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT st.StudentId,
                   st.StudentCode,
                   u.FullName,
                   gc.GradeComponentId,
                   gc.ComponentName,
                   sg.Score,
                   sg.Note,
                   sg.GradedByUserId,
                   sg.GradedAt,
                   sg.UpdatedAt
            FROM   dbo.Enrollments     e
            INNER JOIN dbo.Students        st  ON st.StudentId       = e.StudentId
            INNER JOIN dbo.Users           u   ON u.UserId            = st.UserId
            CROSS  JOIN dbo.GradeComponents gc
            LEFT   JOIN dbo.StudentGrades   sg  ON sg.GradeComponentId = gc.GradeComponentId
                                               AND sg.StudentId        = e.StudentId
            WHERE  e.SectionId        = @SectionId
              AND  gc.SectionId       = @SectionId
              AND  e.Status          IN (1,2)
              AND  (@ComponentId IS NULL OR gc.GradeComponentId = @ComponentId)
            ORDER BY gc.DisplayOrder, u.FullName;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@ComponentId", SqlDbType.BigInt, componentId);

        var list = new List<TeacherStudentGradeDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new TeacherStudentGradeDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetString(reader.GetOrdinal("StudentCode")),
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetInt64(reader.GetOrdinal("GradeComponentId")),
                reader.GetString(reader.GetOrdinal("ComponentName")),
                GetNullableDecimal(reader, "Score"),
                GetNullableString(reader, "Note"),
                GetNullableLong(reader, "GradedByUserId"),
                GetNullableDateTime(reader, "GradedAt"),
                GetNullableDateTime(reader, "UpdatedAt")));
        }

        return Ok(list);
    }

    public async Task<ActionResult<TeacherStudentGradeDto>> UpsertStudentGrade(
        long teacherId,
        long sectionId,
        long studentId,
        UpsertStudentGradeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Score < 0)
            return BadRequest(new { message = "Điểm không được âm." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await GetUserIdByTeacherAsync(teacherId, cancellationToken);
        if (userId is null) return NotFound();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            MERGE dbo.StudentGrades AS target
            USING (SELECT @GradeComponentId AS GradeComponentId, @StudentId AS StudentId) AS src
                ON target.GradeComponentId = src.GradeComponentId AND target.StudentId = src.StudentId
            WHEN MATCHED THEN
                UPDATE SET Score          = @Score,
                           Note           = @Note,
                           GradedByUserId = @GradedByUserId,
                           UpdatedAt      = SYSDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (GradeComponentId, StudentId, Score, Note, GradedByUserId, GradedAt)
                VALUES (@GradeComponentId, @StudentId, @Score, @Note, @GradedByUserId, SYSDATETIME());
            """;
        AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, request.GradeComponentId);
        AddParameter(cmd, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(cmd, "@Score", SqlDbType.Decimal, request.Score);
        AddParameter(cmd, "@Note", SqlDbType.NVarChar, NormalizeText(request.Note), 500);
        AddParameter(cmd, "@GradedByUserId", SqlDbType.BigInt, userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        // Trả về bản ghi vừa upsert
        await using var cmdGet = connection.CreateCommand();
        cmdGet.CommandText = """
            SELECT st.StudentId, st.StudentCode, u.FullName,
                   gc.GradeComponentId, gc.ComponentName,
                   sg.Score, sg.Note, sg.GradedByUserId, sg.GradedAt, sg.UpdatedAt
            FROM   dbo.StudentGrades    sg
            INNER JOIN dbo.GradeComponents gc ON gc.GradeComponentId = sg.GradeComponentId
            INNER JOIN dbo.Students        st ON st.StudentId         = sg.StudentId
            INNER JOIN dbo.Users           u  ON u.UserId             = st.UserId
            WHERE  sg.GradeComponentId = @GradeComponentId AND sg.StudentId = @StudentId;
            """;
        AddParameter(cmdGet, "@GradeComponentId", SqlDbType.BigInt, request.GradeComponentId);
        AddParameter(cmdGet, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await cmdGet.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();

        return Ok(new TeacherStudentGradeDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetString(reader.GetOrdinal("StudentCode")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetInt64(reader.GetOrdinal("GradeComponentId")),
            reader.GetString(reader.GetOrdinal("ComponentName")),
            GetNullableDecimal(reader, "Score"),
            GetNullableString(reader, "Note"),
            GetNullableLong(reader, "GradedByUserId"),
            GetNullableDateTime(reader, "GradedAt"),
            GetNullableDateTime(reader, "UpdatedAt")));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // THÔNG BÁO
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<ActionResult<IReadOnlyList<TeacherAnnouncementDto>>> GetAnnouncements(
        long teacherId,
        long? sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT a.AnnouncementId,
                   a.CreatedByUserId,
                   u.FullName      AS CreatedByFullName,
                   a.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   a.Title,
                   a.Content,
                   a.AnnouncementType,
                   a.PublishedAt,
                   a.ExpiresAt,
                   a.IsActive,
                   a.CreatedAt,    -- Announcements không có CreatedAt theo schema, dùng PublishedAt
                   NULL AS UpdatedAt
            FROM   dbo.Announcements a
            INNER JOIN dbo.Users         u   ON u.UserId        = a.CreatedByUserId
            INNER JOIN dbo.Teachers      t   ON t.UserId        = a.CreatedByUserId
            LEFT  JOIN dbo.CourseSections cs ON cs.SectionId   = a.SectionId
            LEFT  JOIN dbo.Courses        c  ON c.CourseId     = cs.CourseId
            WHERE  t.TeacherId = @TeacherId
              AND  (@SectionId IS NULL OR a.SectionId = @SectionId)
            ORDER BY a.PublishedAt DESC;
            """;
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        var list = new List<TeacherAnnouncementDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(ReadAnnouncement(reader));

        return Ok(list);
    }

    public async Task<ActionResult<TeacherAnnouncementDto>> GetAnnouncement(
        long teacherId,
        long announcementId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT a.AnnouncementId,
                   a.CreatedByUserId,
                   u.FullName      AS CreatedByFullName,
                   a.SectionId,
                   cs.SectionCode,
                   c.CourseCode,
                   c.CourseName,
                   a.Title,
                   a.Content,
                   a.AnnouncementType,
                   a.PublishedAt,
                   a.ExpiresAt,
                   a.IsActive,
                   a.PublishedAt  AS CreatedAt,
                   NULL           AS UpdatedAt
            FROM   dbo.Announcements a
            INNER JOIN dbo.Users         u   ON u.UserId      = a.CreatedByUserId
            INNER JOIN dbo.Teachers      t   ON t.UserId      = a.CreatedByUserId
            LEFT  JOIN dbo.CourseSections cs ON cs.SectionId = a.SectionId
            LEFT  JOIN dbo.Courses        c  ON c.CourseId   = cs.CourseId
            WHERE  a.AnnouncementId = @AnnouncementId AND t.TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();
        return Ok(ReadAnnouncement(reader));
    }

    public async Task<ActionResult<TeacherAnnouncementDto>> CreateAnnouncement(
        long teacherId,
        long sectionId,
        CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Tiêu đề thông báo không được để trống." });
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { message = "Nội dung thông báo không được để trống." });

        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await GetUserIdByTeacherAsync(teacherId, cancellationToken);
        if (userId is null) return NotFound();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO dbo.Announcements
                (CreatedByUserId, SectionId, Title, Content, AnnouncementType,
                 PublishedAt, ExpiresAt, IsActive)
            OUTPUT INSERTED.AnnouncementId
            VALUES
                (@CreatedByUserId, @SectionId, @Title, @Content, @AnnouncementType,
                 SYSDATETIME(), @ExpiresAt, @IsActive);
            """;
        AddParameter(cmd, "@CreatedByUserId", SqlDbType.BigInt, userId.Value);
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        AddParameter(cmd, "@Content", SqlDbType.NVarChar, request.Content.Trim());
        AddParameter(cmd, "@AnnouncementType", SqlDbType.TinyInt, request.AnnouncementType);
        AddParameter(cmd, "@ExpiresAt", SqlDbType.DateTime2, request.ExpiresAt);
        AddParameter(cmd, "@IsActive", SqlDbType.Bit, request.IsActive);

        var newId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        return await GetAnnouncement(teacherId, newId, cancellationToken);
    }

    public async Task<ActionResult<TeacherAnnouncementDto>> UpdateAnnouncement(
        long teacherId,
        long announcementId,
        UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Tiêu đề thông báo không được để trống." });

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE a
            SET    a.Title            = @Title,
                   a.Content          = @Content,
                   a.AnnouncementType = @AnnouncementType,
                   a.ExpiresAt        = @ExpiresAt,
                   a.IsActive         = @IsActive
            FROM   dbo.Announcements a
            INNER JOIN dbo.Teachers t ON t.UserId = a.CreatedByUserId
            WHERE  a.AnnouncementId = @AnnouncementId AND t.TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        AddParameter(cmd, "@Content", SqlDbType.NVarChar, request.Content.Trim());
        AddParameter(cmd, "@AnnouncementType", SqlDbType.TinyInt, request.AnnouncementType);
        AddParameter(cmd, "@ExpiresAt", SqlDbType.DateTime2, request.ExpiresAt);
        AddParameter(cmd, "@IsActive", SqlDbType.Bit, request.IsActive);

        if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
            return NotFound();

        return await GetAnnouncement(teacherId, announcementId, cancellationToken);
    }

    public async Task<IActionResult> DeleteAnnouncement(
        long teacherId,
        long announcementId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            DELETE a
            FROM   dbo.Announcements a
            INNER JOIN dbo.Teachers t ON t.UserId = a.CreatedByUserId
            WHERE  a.AnnouncementId = @AnnouncementId AND t.TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SINH VIÊN
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_GetStudentsBySection";
        command.CommandType = CommandType.StoredProcedure;
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);

        var students = new List<TeacherSectionStudentDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            students.Add(new TeacherSectionStudentDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetString(reader.GetOrdinal("StudentCode")),
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetString(reader.GetOrdinal("Email")),
                GetNullableString(reader, "Phone"),
                reader.GetDateTime(reader.GetOrdinal("EnrolledAt")),
                reader.GetByte(reader.GetOrdinal("Status")),
                GetNullableDecimal(reader, "FinalScore10"),
                GetNullableString(reader, "LetterGrade")));
        }

        return students;
    }

    public async Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(
        long teacherId,
        long sectionId,
        long studentId,
        CancellationToken cancellationToken)
    {
        if (!await IsTeacherOfSectionAsync(teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT st.StudentId,
                   st.StudentCode,
                   u.FullName,
                   u.Email,
                   u.Phone,
                   CAST(u.DateOfBirth AS DATE)  AS DateOfBirth,
                   u.Gender,
                   u.AvatarUrl,
                   ac.ClassCode,
                   ac.ClassName,
                   m.MajorCode,
                   m.MajorName,
                   e.EnrolledAt,
                   e.Status    AS EnrollmentStatus,
                   e.FinalScore10,
                   e.LetterGrade
            FROM   dbo.Enrollments e
            INNER JOIN dbo.Students       st  ON st.StudentId      = e.StudentId
            INNER JOIN dbo.Users          u   ON u.UserId          = st.UserId
            INNER JOIN dbo.Majors         m   ON m.MajorId         = st.MajorId
            LEFT  JOIN dbo.AcademicClasses ac ON ac.AcademicClassId = st.AcademicClassId
            WHERE  e.SectionId = @SectionId AND e.StudentId = @StudentId;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();

        var dob = GetNullableDate(reader, "DateOfBirth");
        return Ok(new TeacherSectionStudentDetailDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetString(reader.GetOrdinal("StudentCode")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("Email")),
            GetNullableString(reader, "Phone"),
            dob,
            GetNullableByte(reader, "Gender"),
            GetNullableString(reader, "AvatarUrl"),
            GetNullableString(reader, "ClassCode"),
            GetNullableString(reader, "ClassName"),
            reader.GetString(reader.GetOrdinal("MajorCode")),
            reader.GetString(reader.GetOrdinal("MajorName")),
            reader.GetDateTime(reader.GetOrdinal("EnrolledAt")),
            reader.GetByte(reader.GetOrdinal("EnrollmentStatus")),
            GetNullableDecimal(reader, "FinalScore10"),
            GetNullableString(reader, "LetterGrade")));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HÀM HELPER PRIVATE
    // ══════════════════════════════════════════════════════════════════════════

    private async Task<TeacherProfileDto?> FindTeacherProfileAsync(
        SqlConnection connection,
        long teacherId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT t.TeacherId,
                   u.UserId,
                   t.TeacherCode,
                   u.Username,
                   u.FullName,
                   u.Email,
                   u.Phone,
                   CAST(u.DateOfBirth AS DATE) AS DateOfBirth,
                   u.Gender,
                   u.AvatarUrl,
                   u.IsActive,
                   d.DepartmentId,
                   d.DepartmentCode,
                   d.DepartmentName,
                   t.AcademicTitle,
                   t.Specialization,
                   t.Status
            FROM   dbo.Teachers    t
            INNER JOIN dbo.Users       u  ON u.UserId       = t.UserId
            INNER JOIN dbo.Departments d  ON d.DepartmentId = t.DepartmentId
            WHERE  t.TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var dob = GetNullableDate(reader, "DateOfBirth");
        return new TeacherProfileDto(
            reader.GetInt64(reader.GetOrdinal("TeacherId")),
            reader.GetInt64(reader.GetOrdinal("UserId")),
            reader.GetString(reader.GetOrdinal("TeacherCode")),
            reader.GetString(reader.GetOrdinal("Username")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("Email")),
            GetNullableString(reader, "Phone"),
            dob,
            GetNullableByte(reader, "Gender"),
            GetNullableString(reader, "AvatarUrl"),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            reader.GetInt32(reader.GetOrdinal("DepartmentId")),
            reader.GetString(reader.GetOrdinal("DepartmentCode")),
            reader.GetString(reader.GetOrdinal("DepartmentName")),
            GetNullableString(reader, "AcademicTitle"),
            GetNullableString(reader, "Specialization"),
            reader.GetByte(reader.GetOrdinal("Status")));
    }

    private async Task<bool> IsTeacherOfSectionAsync(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1) FROM dbo.SectionTeachers
            WHERE SectionId = @SectionId AND TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        return (int)(await cmd.ExecuteScalarAsync(cancellationToken))! > 0;
    }

    private async Task<long?> GetUserIdByTeacherAsync(long teacherId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT UserId FROM dbo.Teachers WHERE TeacherId = @TeacherId;";
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        var raw = await cmd.ExecuteScalarAsync(cancellationToken);
        return raw is null or DBNull ? null : (long)raw;
    }

    // ── Reader helpers ────────────────────────────────────────────────────────

    private static TeacherScheduleDto ReadSchedule(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("SectionCode")),
        reader.GetString(reader.GetOrdinal("CourseCode")),
        reader.GetString(reader.GetOrdinal("CourseName")),
        reader.GetInt64(reader.GetOrdinal("ScheduleId")),
        reader.GetByte(reader.GetOrdinal("DayOfWeek")),
        reader.GetString(reader.GetOrdinal("StartTime")),
        reader.GetString(reader.GetOrdinal("EndTime")),
        GetNullableString(reader, "Room"),
        GetNullableString(reader, "Building"),
        DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EffectiveFrom"))),
        DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EffectiveTo"))),
        GetNullableString(reader, "Note"));

    private static TeacherAssignmentDto ReadAssignment(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("AssignmentId")),
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("SectionCode")),
        reader.GetString(reader.GetOrdinal("CourseCode")),
        reader.GetString(reader.GetOrdinal("CourseName")),
        reader.GetString(reader.GetOrdinal("Title")),
        GetNullableString(reader, "Description"),
        GetNullableString(reader, "AttachmentUrl"),
        GetNullableDateTime(reader, "OpenAt"),
        reader.GetDateTime(reader.GetOrdinal("DueAt")),
        reader.GetDecimal(reader.GetOrdinal("MaxScore")),
        reader.GetBoolean(reader.GetOrdinal("AllowLateSubmission")),
        reader.GetBoolean(reader.GetOrdinal("IsPublished")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        GetNullableDateTime(reader, "UpdatedAt"),
        reader.GetInt32(reader.GetOrdinal("TotalSubmissions")),
        reader.GetInt32(reader.GetOrdinal("GradedSubmissions")));

    private static TeacherSubmissionDto ReadSubmission(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("SubmissionId")),
        reader.GetInt64(reader.GetOrdinal("AssignmentId")),
        reader.GetString(reader.GetOrdinal("AssignmentTitle")),
        reader.GetInt64(reader.GetOrdinal("StudentId")),
        reader.GetString(reader.GetOrdinal("StudentCode")),
        reader.GetString(reader.GetOrdinal("FullName")),
        GetNullableString(reader, "TextContent"),
        GetNullableString(reader, "FileUrl"),
        reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
        reader.GetBoolean(reader.GetOrdinal("IsLate")),
        reader.GetByte(reader.GetOrdinal("Status")),
        GetNullableDecimal(reader, "Score"),
        GetNullableString(reader, "Feedback"),
        GetNullableDateTime(reader, "GradedAt"));

    private static TeacherMaterialDto ReadMaterial(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("MaterialId")),
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("SectionCode")),
        reader.GetString(reader.GetOrdinal("CourseCode")),
        reader.GetString(reader.GetOrdinal("CourseName")),
        reader.GetString(reader.GetOrdinal("Title")),
        GetNullableString(reader, "Description"),
        GetNullableString(reader, "MaterialType"),
        GetNullableString(reader, "FileUrl"),
        GetNullableString(reader, "ExternalUrl"),
        reader.GetBoolean(reader.GetOrdinal("IsVisible")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        GetNullableDateTime(reader, "UpdatedAt"));

    private static TeacherAnnouncementDto ReadAnnouncement(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("AnnouncementId")),
        reader.GetInt64(reader.GetOrdinal("CreatedByUserId")),
        reader.GetString(reader.GetOrdinal("CreatedByFullName")),
        GetNullableLong(reader, "SectionId"),
        GetNullableString(reader, "SectionCode"),
        GetNullableString(reader, "CourseCode"),
        GetNullableString(reader, "CourseName"),
        reader.GetString(reader.GetOrdinal("Title")),
        reader.GetString(reader.GetOrdinal("Content")),
        reader.GetByte(reader.GetOrdinal("AnnouncementType")),
        reader.GetDateTime(reader.GetOrdinal("PublishedAt")),
        GetNullableDateTime(reader, "ExpiresAt"),
        reader.GetBoolean(reader.GetOrdinal("IsActive")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        GetNullableDateTime(reader, "UpdatedAt"));

    // ── Parameter helpers ─────────────────────────────────────────────────────

    private static SqlParameter AddParameter(
        SqlCommand command,
        string name,
        SqlDbType type,
        object? value,
        int? size = null)
    {
        var parameter = size is null
            ? command.Parameters.Add(name, type)
            : command.Parameters.Add(name, type, size.Value);
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static long? GetNullableLong(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static byte? GetNullableByte(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetByte(ordinal);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static DateOnly? GetNullableDate(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(ordinal));
    }
}
