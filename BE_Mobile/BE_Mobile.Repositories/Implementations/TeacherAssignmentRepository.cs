using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class TeacherAssignmentRepository(IDbConnectionFactory connectionFactory)
    : ControllerBase, ITeacherAssignmentRepository
{
    public async Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetAssignments(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

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
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await SqlRepositoryHelper.GetUserIdByTeacherAsync(connection, teacherId, cancellationToken);
        if (userId is null) return NotFound();

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
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@CreatedByUserId", SqlDbType.BigInt, userId.Value);
        SqlRepositoryHelper.AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        SqlRepositoryHelper.AddParameter(cmd, "@Description", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Description));
        SqlRepositoryHelper.AddParameter(cmd, "@AttachmentUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.AttachmentUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@OpenAt", SqlDbType.DateTime2, request.OpenAt);
        SqlRepositoryHelper.AddParameter(cmd, "@DueAt", SqlDbType.DateTime2, request.DueAt);
        SqlRepositoryHelper.AddParameter(cmd, "@MaxScore", SqlDbType.Decimal, request.MaxScore);
        SqlRepositoryHelper.AddParameter(cmd, "@AllowLateSubmission", SqlDbType.Bit, request.AllowLateSubmission);
        SqlRepositoryHelper.AddParameter(cmd, "@IsPublished", SqlDbType.Bit, request.IsPublished);

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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        SqlRepositoryHelper.AddParameter(cmd, "@Description", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Description));
        SqlRepositoryHelper.AddParameter(cmd, "@AttachmentUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.AttachmentUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@OpenAt", SqlDbType.DateTime2, request.OpenAt);
        SqlRepositoryHelper.AddParameter(cmd, "@DueAt", SqlDbType.DateTime2, request.DueAt);
        SqlRepositoryHelper.AddParameter(cmd, "@MaxScore", SqlDbType.Decimal, request.MaxScore);
        SqlRepositoryHelper.AddParameter(cmd, "@AllowLateSubmission", SqlDbType.Bit, request.AllowLateSubmission);
        SqlRepositoryHelper.AddParameter(cmd, "@IsPublished", SqlDbType.Bit, request.IsPublished);

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
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Assignments WHERE AssignmentId = @AssignmentId AND SectionId = @SectionId;";
        SqlRepositoryHelper.AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    public async Task<ActionResult<IReadOnlyList<TeacherSubmissionDto>>> GetSubmissions(
        long teacherId,
        long sectionId,
        long assignmentId,
        byte? status,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@Status", SqlDbType.TinyInt, status);

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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await SqlRepositoryHelper.GetUserIdByTeacherAsync(connection, teacherId, cancellationToken);
        if (userId is null) return NotFound();

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
        SqlRepositoryHelper.AddParameter(cmd, "@SubmissionId", SqlDbType.BigInt, submissionId);
        SqlRepositoryHelper.AddParameter(cmd, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        SqlRepositoryHelper.AddParameter(cmd, "@Score", SqlDbType.Decimal, request.Score);
        SqlRepositoryHelper.AddParameter(cmd, "@Feedback", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Feedback));
        SqlRepositoryHelper.AddParameter(cmd, "@GradedByUserId", SqlDbType.BigInt, userId.Value);

        if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
            return NotFound();

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
        SqlRepositoryHelper.AddParameter(cmdGet, "@SubmissionId", SqlDbType.BigInt, submissionId);

        await using var reader = await cmdGet.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound();

        return Ok(ReadSubmission(reader));
    }

    private static TeacherAssignmentDto ReadAssignment(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("AssignmentId")),
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("SectionCode")),
        reader.GetString(reader.GetOrdinal("CourseCode")),
        reader.GetString(reader.GetOrdinal("CourseName")),
        reader.GetString(reader.GetOrdinal("Title")),
        SqlRepositoryHelper.GetNullableString(reader, "Description"),
        SqlRepositoryHelper.GetNullableString(reader, "AttachmentUrl"),
        SqlRepositoryHelper.GetNullableDateTime(reader, "OpenAt"),
        reader.GetDateTime(reader.GetOrdinal("DueAt")),
        reader.GetDecimal(reader.GetOrdinal("MaxScore")),
        reader.GetBoolean(reader.GetOrdinal("AllowLateSubmission")),
        reader.GetBoolean(reader.GetOrdinal("IsPublished")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        SqlRepositoryHelper.GetNullableDateTime(reader, "UpdatedAt"),
        reader.GetInt32(reader.GetOrdinal("TotalSubmissions")),
        reader.GetInt32(reader.GetOrdinal("GradedSubmissions")));

    private static TeacherSubmissionDto ReadSubmission(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("SubmissionId")),
        reader.GetInt64(reader.GetOrdinal("AssignmentId")),
        reader.GetString(reader.GetOrdinal("AssignmentTitle")),
        reader.GetInt64(reader.GetOrdinal("StudentId")),
        reader.GetString(reader.GetOrdinal("StudentCode")),
        reader.GetString(reader.GetOrdinal("FullName")),
        SqlRepositoryHelper.GetNullableString(reader, "TextContent"),
        SqlRepositoryHelper.GetNullableString(reader, "FileUrl"),
        reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
        reader.GetBoolean(reader.GetOrdinal("IsLate")),
        reader.GetByte(reader.GetOrdinal("Status")),
        SqlRepositoryHelper.GetNullableDecimal(reader, "Score"),
        SqlRepositoryHelper.GetNullableString(reader, "Feedback"),
        SqlRepositoryHelper.GetNullableDateTime(reader, "GradedAt"));
}
