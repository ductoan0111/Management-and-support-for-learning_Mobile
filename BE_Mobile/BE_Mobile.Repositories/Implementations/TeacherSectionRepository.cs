using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
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

        await using var cmdUserId = connection.CreateCommand();
        cmdUserId.CommandText = "SELECT u.UserId FROM dbo.Teachers t INNER JOIN dbo.Users u ON u.UserId = t.UserId WHERE t.TeacherId = @TeacherId";
        SqlRepositoryHelper.AddParameter(cmdUserId, "@TeacherId", SqlDbType.BigInt, teacherId);
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
        SqlRepositoryHelper.AddParameter(cmd, "@UserId", SqlDbType.BigInt, userId);
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        SqlRepositoryHelper.AddParameter(cmd, "@FullName", SqlDbType.NVarChar, request.FullName.Trim(), 150);
        SqlRepositoryHelper.AddParameter(cmd, "@Phone", SqlDbType.VarChar, SqlRepositoryHelper.NormalizeText(request.Phone), 20);
        SqlRepositoryHelper.AddParameter(cmd, "@DateOfBirth", SqlDbType.Date, request.DateOfBirth);
        SqlRepositoryHelper.AddParameter(cmd, "@Gender", SqlDbType.TinyInt, request.Gender);
        SqlRepositoryHelper.AddParameter(cmd, "@AvatarUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.AvatarUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@AcademicTitle", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.AcademicTitle), 100);
        SqlRepositoryHelper.AddParameter(cmd, "@Specialization", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Specialization), 255);
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
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        SqlRepositoryHelper.AddParameter(cmd, "@SemesterId", SqlDbType.Int, semesterId);
        SqlRepositoryHelper.AddParameter(cmd, "@Status", SqlDbType.TinyInt, status);

        var sections = new List<TeacherSectionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sections.Add(new TeacherSectionDto(
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("SectionCode")),
                SqlRepositoryHelper.GetNullableString(reader, "SectionName"),
                reader.GetInt32(reader.GetOrdinal("CourseId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetByte(reader.GetOrdinal("Credits")),
                reader.GetInt32(reader.GetOrdinal("SemesterId")),
                reader.GetString(reader.GetOrdinal("SemesterCode")),
                reader.GetString(reader.GetOrdinal("SemesterName")),
                reader.GetString(reader.GetOrdinal("AcademicYear")),
                reader.GetByte(reader.GetOrdinal("Status")),
                SqlRepositoryHelper.GetNullableInt(reader, "MaxStudents"),
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
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound();

        var detail = new
        {
            SectionId     = reader.GetInt64(reader.GetOrdinal("SectionId")),
            SectionCode   = reader.GetString(reader.GetOrdinal("SectionCode")),
            SectionName   = SqlRepositoryHelper.GetNullableString(reader, "SectionName"),
            CourseId      = reader.GetInt32(reader.GetOrdinal("CourseId")),
            CourseCode    = reader.GetString(reader.GetOrdinal("CourseCode")),
            CourseName    = reader.GetString(reader.GetOrdinal("CourseName")),
            Credits       = reader.GetByte(reader.GetOrdinal("Credits")),
            SemesterId    = reader.GetInt32(reader.GetOrdinal("SemesterId")),
            SemesterCode  = reader.GetString(reader.GetOrdinal("SemesterCode")),
            SemesterName  = reader.GetString(reader.GetOrdinal("SemesterName")),
            AcademicYear  = reader.GetString(reader.GetOrdinal("AcademicYear")),
            Status        = reader.GetByte(reader.GetOrdinal("Status")),
            MaxStudents   = SqlRepositoryHelper.GetNullableInt(reader, "MaxStudents"),
            EnrolledCount = reader.GetInt32(reader.GetOrdinal("EnrolledCount")),
            IsPrimary     = reader.GetBoolean(reader.GetOrdinal("IsPrimary")),
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
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@From", SqlDbType.Date, from);
        SqlRepositoryHelper.AddParameter(cmd, "@To", SqlDbType.Date, to);

        var list = new List<TeacherScheduleDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(ReadSchedule(reader));

        return Ok(list);
    }

    // ── Helper methods ────────────────────────────────────────────────────────

    private static async Task<TeacherProfileDto?> FindTeacherProfileAsync(
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
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var dob = SqlRepositoryHelper.GetNullableDate(reader, "DateOfBirth");
        return new TeacherProfileDto(
            reader.GetInt64(reader.GetOrdinal("TeacherId")),
            reader.GetInt64(reader.GetOrdinal("UserId")),
            reader.GetString(reader.GetOrdinal("TeacherCode")),
            reader.GetString(reader.GetOrdinal("Username")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("Email")),
            SqlRepositoryHelper.GetNullableString(reader, "Phone"),
            dob,
            SqlRepositoryHelper.GetNullableByte(reader, "Gender"),
            SqlRepositoryHelper.GetNullableString(reader, "AvatarUrl"),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            reader.GetInt32(reader.GetOrdinal("DepartmentId")),
            reader.GetString(reader.GetOrdinal("DepartmentCode")),
            reader.GetString(reader.GetOrdinal("DepartmentName")),
            SqlRepositoryHelper.GetNullableString(reader, "AcademicTitle"),
            SqlRepositoryHelper.GetNullableString(reader, "Specialization"),
            reader.GetByte(reader.GetOrdinal("Status")));
    }

    private static TeacherScheduleDto ReadSchedule(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("SectionCode")),
        reader.GetString(reader.GetOrdinal("CourseCode")),
        reader.GetString(reader.GetOrdinal("CourseName")),
        reader.GetInt64(reader.GetOrdinal("ScheduleId")),
        reader.GetByte(reader.GetOrdinal("DayOfWeek")),
        reader.GetString(reader.GetOrdinal("StartTime")),
        reader.GetString(reader.GetOrdinal("EndTime")),
        SqlRepositoryHelper.GetNullableString(reader, "Room"),
        SqlRepositoryHelper.GetNullableString(reader, "Building"),
        DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EffectiveFrom"))),
        DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EffectiveTo"))),
        SqlRepositoryHelper.GetNullableString(reader, "Note"));
}
