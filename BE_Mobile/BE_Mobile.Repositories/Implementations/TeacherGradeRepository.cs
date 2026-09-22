using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class TeacherGradeRepository(IDbConnectionFactory connectionFactory)
    : ControllerBase, ITeacherGradeRepository
{
    public async Task<ActionResult<IReadOnlyList<TeacherGradeComponentDto>>> GetGradeComponents(
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
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

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
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@ComponentId", SqlDbType.BigInt, componentId);

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
                SqlRepositoryHelper.GetNullableDecimal(reader, "Score"),
                SqlRepositoryHelper.GetNullableString(reader, "Note"),
                SqlRepositoryHelper.GetNullableLong(reader, "GradedByUserId"),
                SqlRepositoryHelper.GetNullableDateTime(reader, "GradedAt"),
                SqlRepositoryHelper.GetNullableDateTime(reader, "UpdatedAt")));
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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await SqlRepositoryHelper.GetUserIdByTeacherAsync(connection, teacherId, cancellationToken);
        if (userId is null) return NotFound();

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
        SqlRepositoryHelper.AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, request.GradeComponentId);
        SqlRepositoryHelper.AddParameter(cmd, "@StudentId", SqlDbType.BigInt, studentId);
        SqlRepositoryHelper.AddParameter(cmd, "@Score", SqlDbType.Decimal, request.Score);
        SqlRepositoryHelper.AddParameter(cmd, "@Note", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Note), 500);
        SqlRepositoryHelper.AddParameter(cmd, "@GradedByUserId", SqlDbType.BigInt, userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

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
        SqlRepositoryHelper.AddParameter(cmdGet, "@GradeComponentId", SqlDbType.BigInt, request.GradeComponentId);
        SqlRepositoryHelper.AddParameter(cmdGet, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await cmdGet.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();

        return Ok(new TeacherStudentGradeDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetString(reader.GetOrdinal("StudentCode")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetInt64(reader.GetOrdinal("GradeComponentId")),
            reader.GetString(reader.GetOrdinal("ComponentName")),
            SqlRepositoryHelper.GetNullableDecimal(reader, "Score"),
            SqlRepositoryHelper.GetNullableString(reader, "Note"),
            SqlRepositoryHelper.GetNullableLong(reader, "GradedByUserId"),
            SqlRepositoryHelper.GetNullableDateTime(reader, "GradedAt"),
            SqlRepositoryHelper.GetNullableDateTime(reader, "UpdatedAt")));
    }
}
