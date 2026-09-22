using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class TeacherAnnouncementRepository(IDbConnectionFactory connectionFactory)
    : ControllerBase, ITeacherAnnouncementRepository
{
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
                   a.PublishedAt   AS CreatedAt,
                   NULL            AS UpdatedAt
            FROM   dbo.Announcements a
            INNER JOIN dbo.Users         u   ON u.UserId        = a.CreatedByUserId
            INNER JOIN dbo.Teachers      t   ON t.UserId        = a.CreatedByUserId
            LEFT  JOIN dbo.CourseSections cs ON cs.SectionId   = a.SectionId
            LEFT  JOIN dbo.Courses        c  ON c.CourseId     = cs.CourseId
            WHERE  t.TeacherId = @TeacherId
              AND  (@SectionId IS NULL OR a.SectionId = @SectionId)
            ORDER BY a.PublishedAt DESC;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

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
        SqlRepositoryHelper.AddParameter(cmd, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);

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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await SqlRepositoryHelper.GetUserIdByTeacherAsync(connection, teacherId, cancellationToken);
        if (userId is null) return NotFound();

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
        SqlRepositoryHelper.AddParameter(cmd, "@CreatedByUserId", SqlDbType.BigInt, userId.Value);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        SqlRepositoryHelper.AddParameter(cmd, "@Content", SqlDbType.NVarChar, request.Content.Trim());
        SqlRepositoryHelper.AddParameter(cmd, "@AnnouncementType", SqlDbType.TinyInt, request.AnnouncementType);
        SqlRepositoryHelper.AddParameter(cmd, "@ExpiresAt", SqlDbType.DateTime2, request.ExpiresAt);
        SqlRepositoryHelper.AddParameter(cmd, "@IsActive", SqlDbType.Bit, request.IsActive);

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
        SqlRepositoryHelper.AddParameter(cmd, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        SqlRepositoryHelper.AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        SqlRepositoryHelper.AddParameter(cmd, "@Content", SqlDbType.NVarChar, request.Content.Trim());
        SqlRepositoryHelper.AddParameter(cmd, "@AnnouncementType", SqlDbType.TinyInt, request.AnnouncementType);
        SqlRepositoryHelper.AddParameter(cmd, "@ExpiresAt", SqlDbType.DateTime2, request.ExpiresAt);
        SqlRepositoryHelper.AddParameter(cmd, "@IsActive", SqlDbType.Bit, request.IsActive);

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
        SqlRepositoryHelper.AddParameter(cmd, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        SqlRepositoryHelper.AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    private static TeacherAnnouncementDto ReadAnnouncement(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("AnnouncementId")),
        reader.GetInt64(reader.GetOrdinal("CreatedByUserId")),
        reader.GetString(reader.GetOrdinal("CreatedByFullName")),
        SqlRepositoryHelper.GetNullableLong(reader, "SectionId"),
        SqlRepositoryHelper.GetNullableString(reader, "SectionCode"),
        SqlRepositoryHelper.GetNullableString(reader, "CourseCode"),
        SqlRepositoryHelper.GetNullableString(reader, "CourseName"),
        reader.GetString(reader.GetOrdinal("Title")),
        reader.GetString(reader.GetOrdinal("Content")),
        reader.GetByte(reader.GetOrdinal("AnnouncementType")),
        reader.GetDateTime(reader.GetOrdinal("PublishedAt")),
        SqlRepositoryHelper.GetNullableDateTime(reader, "ExpiresAt"),
        reader.GetBoolean(reader.GetOrdinal("IsActive")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        SqlRepositoryHelper.GetNullableDateTime(reader, "UpdatedAt"));
}
