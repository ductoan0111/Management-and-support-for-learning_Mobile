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

    public async Task<ActionResult<TeacherGradeComponentDto>> CreateGradeComponent(
        long teacherId,
        long sectionId,
        SaveGradeComponentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateComponentRequest(request);
        if (validation is not null) return BadRequest(new { message = validation });

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var totalWeight = await GetTotalWeightAsync(connection, sectionId, null, cancellationToken);
        if (totalWeight + request.WeightPercent > 100m)
            return BadRequest(new { message = "Total grade weight cannot exceed 100%." });

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO dbo.GradeComponents
                    (SectionId, ComponentName, WeightPercent, MaxScore, DisplayOrder)
                OUTPUT INSERTED.GradeComponentId
                VALUES
                    (@SectionId, @ComponentName, @WeightPercent, @MaxScore, @DisplayOrder);
                """;
            AddComponentParameters(cmd, sectionId, request);

            var rawId = await cmd.ExecuteScalarAsync(cancellationToken);
            if (rawId is null or DBNull) return NotFound();

            await ClearSectionFinalGradesAsync(connection, sectionId, cancellationToken);
            return await GetGradeComponent(connection, sectionId, Convert.ToInt64(rawId), cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return Conflict(new { message = "A grade component with this name already exists in the section." });
        }
    }

    public async Task<ActionResult<TeacherGradeComponentDto>> UpdateGradeComponent(
        long teacherId,
        long sectionId,
        long componentId,
        SaveGradeComponentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateComponentRequest(request);
        if (validation is not null) return BadRequest(new { message = validation });

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        if (!await GradeComponentExistsAsync(connection, sectionId, componentId, cancellationToken))
            return NotFound();

        var totalWeight = await GetTotalWeightAsync(connection, sectionId, componentId, cancellationToken);
        if (totalWeight + request.WeightPercent > 100m)
            return BadRequest(new { message = "Total grade weight cannot exceed 100%." });

        var highestScore = await GetHighestStudentScoreAsync(connection, componentId, cancellationToken);
        if (highestScore.HasValue && request.MaxScore < highestScore.Value)
            return BadRequest(new { message = "Max score cannot be lower than an existing student score." });

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE dbo.GradeComponents
                SET ComponentName = @ComponentName,
                    WeightPercent = @WeightPercent,
                    MaxScore      = @MaxScore,
                    DisplayOrder  = @DisplayOrder
                WHERE GradeComponentId = @GradeComponentId
                  AND SectionId = @SectionId;
                """;
            SqlRepositoryHelper.AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, componentId);
            AddComponentParameters(cmd, sectionId, request);

            if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
                return NotFound();

            await ClearSectionFinalGradesAsync(connection, sectionId, cancellationToken);
            return await GetGradeComponent(connection, sectionId, componentId, cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return Conflict(new { message = "A grade component with this name already exists in the section." });
        }
    }

    public async Task<IActionResult> DeleteGradeComponent(
        long teacherId,
        long sectionId,
        long componentId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        if (!await GradeComponentExistsAsync(connection, sectionId, componentId, cancellationToken))
            return NotFound();

        await using var countCmd = connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(1) FROM dbo.StudentGrades WHERE GradeComponentId = @GradeComponentId;";
        SqlRepositoryHelper.AddParameter(countCmd, "@GradeComponentId", SqlDbType.BigInt, componentId);
        var gradesCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
        if (gradesCount > 0)
            return Conflict(new { message = "Cannot delete a grade component that already has student grades." });

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.GradeComponents WHERE GradeComponentId = @GradeComponentId AND SectionId = @SectionId;";
        SqlRepositoryHelper.AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, componentId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
            return NotFound();

        await ClearSectionFinalGradesAsync(connection, sectionId, cancellationToken);
        return NoContent();
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
        if (request.Score.HasValue && request.Score.Value < 0)
            return BadRequest(new { message = "Điểm không được âm." });

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var validationCmd = connection.CreateCommand();
        validationCmd.CommandText = """
            SELECT gc.MaxScore,
                   CAST(CASE WHEN EXISTS (
                       SELECT 1
                       FROM   dbo.Enrollments e
                       WHERE  e.SectionId = @SectionId
                         AND  e.StudentId = @StudentId
                         AND  e.Status IN (1,2)
                   ) THEN 1 ELSE 0 END AS bit) AS IsStudentEnrolled
            FROM   dbo.GradeComponents gc
            WHERE  gc.GradeComponentId = @GradeComponentId
              AND  gc.SectionId = @SectionId;
            """;
        SqlRepositoryHelper.AddParameter(validationCmd, "@GradeComponentId", SqlDbType.BigInt, request.GradeComponentId);
        SqlRepositoryHelper.AddParameter(validationCmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(validationCmd, "@StudentId", SqlDbType.BigInt, studentId);

        await using (var validationReader = await validationCmd.ExecuteReaderAsync(cancellationToken))
        {
            if (!await validationReader.ReadAsync(cancellationToken))
                return NotFound();

            var maxScore = validationReader.GetDecimal(validationReader.GetOrdinal("MaxScore"));
            var isStudentEnrolled = validationReader.GetBoolean(validationReader.GetOrdinal("IsStudentEnrolled"));
            if (!isStudentEnrolled)
                return NotFound();

            if (request.Score.HasValue && request.Score.Value > maxScore)
                return BadRequest(new { message = "Score cannot exceed the grade component max score." });
        }

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
        await ClearStudentFinalGradeAsync(connection, sectionId, studentId, cancellationToken);

        await using var cmdGet = connection.CreateCommand();
        cmdGet.CommandText = """
            SELECT st.StudentId, st.StudentCode, u.FullName,
                   gc.GradeComponentId, gc.ComponentName,
                   sg.Score, sg.Note, sg.GradedByUserId, sg.GradedAt, sg.UpdatedAt
            FROM   dbo.StudentGrades    sg
            INNER JOIN dbo.GradeComponents gc ON gc.GradeComponentId = sg.GradeComponentId
            INNER JOIN dbo.Students        st ON st.StudentId         = sg.StudentId
            INNER JOIN dbo.Users           u  ON u.UserId             = st.UserId
            WHERE  sg.GradeComponentId = @GradeComponentId
              AND  sg.StudentId = @StudentId
              AND  gc.SectionId = @SectionId
              AND  EXISTS (
                   SELECT 1
                   FROM   dbo.Enrollments e
                   WHERE  e.SectionId = @SectionId
                     AND  e.StudentId = sg.StudentId
                     AND  e.Status IN (1,2)
              );
            """;
        SqlRepositoryHelper.AddParameter(cmdGet, "@GradeComponentId", SqlDbType.BigInt, request.GradeComponentId);
        SqlRepositoryHelper.AddParameter(cmdGet, "@StudentId", SqlDbType.BigInt, studentId);
        SqlRepositoryHelper.AddParameter(cmdGet, "@SectionId", SqlDbType.BigInt, sectionId);

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

    public async Task<ActionResult<TeacherGradeOverviewDto>> GetGradeOverview(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        return Ok(await ReadGradeOverviewAsync(connection, sectionId, cancellationToken));
    }

    public async Task<ActionResult<TeacherGradeOverviewDto>> FinalizeGrades(
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var totalWeight = await GetTotalWeightAsync(connection, sectionId, null, cancellationToken);
        if (totalWeight != 100m)
            return BadRequest(new { message = "Total grade weight must be exactly 100% before finalizing grades." });

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE e
            SET FinalScore10 = x.WeightedScore10,
                LetterGrade = CASE
                    WHEN x.WeightedScore10 >= 8.5 THEN 'A'
                    WHEN x.WeightedScore10 >= 8.0 THEN 'B+'
                    WHEN x.WeightedScore10 >= 7.0 THEN 'B'
                    WHEN x.WeightedScore10 >= 6.5 THEN 'C+'
                    WHEN x.WeightedScore10 >= 5.5 THEN 'C'
                    WHEN x.WeightedScore10 >= 5.0 THEN 'D+'
                    WHEN x.WeightedScore10 >= 4.0 THEN 'D'
                    ELSE 'F'
                END
            FROM dbo.Enrollments e
            INNER JOIN dbo.vw_StudentSectionScores x
                ON x.SectionId = e.SectionId
               AND x.StudentId = e.StudentId
            WHERE e.SectionId = @SectionId
              AND e.Status IN (1,2);
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        return Ok(await ReadGradeOverviewAsync(connection, sectionId, cancellationToken));
    }

    private static string? ValidateComponentRequest(SaveGradeComponentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ComponentName))
            return "Grade component name is required.";
        if (request.WeightPercent <= 0 || request.WeightPercent > 100)
            return "Grade component weight must be from 0.01 to 100.";
        if (request.MaxScore <= 0)
            return "Grade component max score must be greater than 0.";
        if (request.DisplayOrder <= 0)
            return "Grade component display order must be greater than 0.";
        return null;
    }

    private static void AddComponentParameters(SqlCommand cmd, long sectionId, SaveGradeComponentRequest request)
    {
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@ComponentName", SqlDbType.NVarChar, request.ComponentName.Trim(), 100);
        SqlRepositoryHelper.AddParameter(cmd, "@WeightPercent", SqlDbType.Decimal, request.WeightPercent);
        SqlRepositoryHelper.AddParameter(cmd, "@MaxScore", SqlDbType.Decimal, request.MaxScore);
        SqlRepositoryHelper.AddParameter(cmd, "@DisplayOrder", SqlDbType.Int, request.DisplayOrder);
    }

    private async Task<ActionResult<TeacherGradeComponentDto>> GetGradeComponent(
        SqlConnection connection,
        long sectionId,
        long componentId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT GradeComponentId, SectionId, ComponentName, WeightPercent, MaxScore, DisplayOrder
            FROM dbo.GradeComponents
            WHERE SectionId = @SectionId
              AND GradeComponentId = @GradeComponentId;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, componentId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();
        return Ok(ReadGradeComponent(reader));
    }

    private static TeacherGradeComponentDto ReadGradeComponent(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("GradeComponentId")),
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("ComponentName")),
        reader.GetDecimal(reader.GetOrdinal("WeightPercent")),
        reader.GetDecimal(reader.GetOrdinal("MaxScore")),
        reader.GetInt32(reader.GetOrdinal("DisplayOrder")));

    private static async Task<bool> GradeComponentExistsAsync(
        SqlConnection connection,
        long sectionId,
        long componentId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1)
            FROM dbo.GradeComponents
            WHERE SectionId = @SectionId
              AND GradeComponentId = @GradeComponentId;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, componentId);
        var count = await cmd.ExecuteScalarAsync(cancellationToken);
        return count is not null and not DBNull && Convert.ToInt32(count) > 0;
    }

    private static async Task<decimal> GetTotalWeightAsync(
        SqlConnection connection,
        long sectionId,
        long? exceptComponentId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COALESCE(SUM(WeightPercent), 0)
            FROM dbo.GradeComponents
            WHERE SectionId = @SectionId
              AND (@ExceptComponentId IS NULL OR GradeComponentId <> @ExceptComponentId);
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@ExceptComponentId", SqlDbType.BigInt, exceptComponentId);
        var raw = await cmd.ExecuteScalarAsync(cancellationToken);
        return raw is null or DBNull ? 0m : Convert.ToDecimal(raw);
    }

    private static async Task<decimal?> GetHighestStudentScoreAsync(
        SqlConnection connection,
        long componentId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT MAX(Score) FROM dbo.StudentGrades WHERE GradeComponentId = @GradeComponentId;";
        SqlRepositoryHelper.AddParameter(cmd, "@GradeComponentId", SqlDbType.BigInt, componentId);
        var raw = await cmd.ExecuteScalarAsync(cancellationToken);
        return raw is null or DBNull ? null : Convert.ToDecimal(raw);
    }

    private static async Task ClearSectionFinalGradesAsync(
        SqlConnection connection,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.Enrollments
            SET FinalScore10 = NULL,
                LetterGrade = NULL
            WHERE SectionId = @SectionId;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ClearStudentFinalGradeAsync(
        SqlConnection connection,
        long sectionId,
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.Enrollments
            SET FinalScore10 = NULL,
                LetterGrade = NULL
            WHERE SectionId = @SectionId
              AND StudentId = @StudentId;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@StudentId", SqlDbType.BigInt, studentId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<TeacherGradeOverviewDto> ReadGradeOverviewAsync(
        SqlConnection connection,
        long sectionId,
        CancellationToken cancellationToken)
    {
        var componentCount = 0;
        var totalWeight = 0m;

        await using (var summaryCmd = connection.CreateCommand())
        {
            summaryCmd.CommandText = """
                SELECT COUNT(1) AS ComponentCount,
                       COALESCE(SUM(WeightPercent), 0) AS TotalWeightPercent
                FROM dbo.GradeComponents
                WHERE SectionId = @SectionId;
                """;
            SqlRepositoryHelper.AddParameter(summaryCmd, "@SectionId", SqlDbType.BigInt, sectionId);

            await using var summaryReader = await summaryCmd.ExecuteReaderAsync(cancellationToken);
            if (await summaryReader.ReadAsync(cancellationToken))
            {
                componentCount = summaryReader.GetInt32(summaryReader.GetOrdinal("ComponentCount"));
                totalWeight = summaryReader.GetDecimal(summaryReader.GetOrdinal("TotalWeightPercent"));
            }
        }

        var students = new List<TeacherFinalGradeDto>();
        await using (var studentsCmd = connection.CreateCommand())
        {
            studentsCmd.CommandText = """
                SELECT e.StudentId,
                       st.StudentCode,
                       u.FullName,
                       CAST(COALESCE(x.WeightedScore10, 0) AS DECIMAL(5,2)) AS CalculatedScore10,
                       e.FinalScore10,
                       e.LetterGrade,
                       @ComponentCount AS TotalComponents,
                       (
                           SELECT COUNT(1)
                           FROM dbo.GradeComponents gc
                           INNER JOIN dbo.StudentGrades sg
                               ON sg.GradeComponentId = gc.GradeComponentId
                              AND sg.StudentId = e.StudentId
                              AND sg.Score IS NOT NULL
                           WHERE gc.SectionId = @SectionId
                       ) AS GradedComponents
                FROM dbo.Enrollments e
                INNER JOIN dbo.Students st ON st.StudentId = e.StudentId
                INNER JOIN dbo.Users u ON u.UserId = st.UserId
                LEFT JOIN dbo.vw_StudentSectionScores x
                    ON x.SectionId = e.SectionId
                   AND x.StudentId = e.StudentId
                WHERE e.SectionId = @SectionId
                  AND e.Status IN (1,2)
                ORDER BY u.FullName;
                """;
            SqlRepositoryHelper.AddParameter(studentsCmd, "@SectionId", SqlDbType.BigInt, sectionId);
            SqlRepositoryHelper.AddParameter(studentsCmd, "@ComponentCount", SqlDbType.Int, componentCount);

            await using var reader = await studentsCmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                students.Add(new TeacherFinalGradeDto(
                    reader.GetInt64(reader.GetOrdinal("StudentId")),
                    reader.GetString(reader.GetOrdinal("StudentCode")),
                    reader.GetString(reader.GetOrdinal("FullName")),
                    reader.GetDecimal(reader.GetOrdinal("CalculatedScore10")),
                    SqlRepositoryHelper.GetNullableDecimal(reader, "FinalScore10"),
                    SqlRepositoryHelper.GetNullableString(reader, "LetterGrade"),
                    reader.GetInt32(reader.GetOrdinal("GradedComponents")),
                    reader.GetInt32(reader.GetOrdinal("TotalComponents"))));
            }
        }

        return new TeacherGradeOverviewDto(
            sectionId,
            componentCount,
            totalWeight,
            totalWeight == 100m,
            students.Count,
            students.Count(student => student.FinalScore10.HasValue),
            students);
    }
}
