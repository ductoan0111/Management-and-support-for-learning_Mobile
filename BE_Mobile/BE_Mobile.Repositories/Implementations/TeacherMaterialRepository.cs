using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class TeacherMaterialRepository(IDbConnectionFactory connectionFactory)
    : ControllerBase, ITeacherMaterialRepository
{
    public async Task<ActionResult<IReadOnlyList<TeacherMaterialDto>>> GetMaterials(
        long teacherId,
        long sectionId,
        string? search,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@Search", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(search), 250);

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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        var userId = await SqlRepositoryHelper.GetUserIdByTeacherAsync(connection, teacherId, cancellationToken);
        if (userId is null) return NotFound();

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
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@UploadedByUserId", SqlDbType.BigInt, userId.Value);
        SqlRepositoryHelper.AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        SqlRepositoryHelper.AddParameter(cmd, "@Description", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Description));
        SqlRepositoryHelper.AddParameter(cmd, "@MaterialType", SqlDbType.VarChar, SqlRepositoryHelper.NormalizeText(request.MaterialType), 30);
        SqlRepositoryHelper.AddParameter(cmd, "@FileUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.FileUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@ExternalUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.ExternalUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@IsVisible", SqlDbType.Bit, request.IsVisible);

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
        SqlRepositoryHelper.AddParameter(cmdGet, "@MaterialId", SqlDbType.BigInt, newId);

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

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

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
        SqlRepositoryHelper.AddParameter(cmd, "@MaterialId", SqlDbType.BigInt, materialId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@Title", SqlDbType.NVarChar, request.Title.Trim(), 250);
        SqlRepositoryHelper.AddParameter(cmd, "@Description", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.Description));
        SqlRepositoryHelper.AddParameter(cmd, "@MaterialType", SqlDbType.VarChar, SqlRepositoryHelper.NormalizeText(request.MaterialType), 30);
        SqlRepositoryHelper.AddParameter(cmd, "@FileUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.FileUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@ExternalUrl", SqlDbType.NVarChar, SqlRepositoryHelper.NormalizeText(request.ExternalUrl), 1000);
        SqlRepositoryHelper.AddParameter(cmd, "@IsVisible", SqlDbType.Bit, request.IsVisible);

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
        SqlRepositoryHelper.AddParameter(cmdGet, "@MaterialId", SqlDbType.BigInt, materialId);

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
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Materials WHERE MaterialId = @MaterialId AND SectionId = @SectionId;";
        SqlRepositoryHelper.AddParameter(cmd, "@MaterialId", SqlDbType.BigInt, materialId);
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    private static TeacherMaterialDto ReadMaterial(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("MaterialId")),
        reader.GetInt64(reader.GetOrdinal("SectionId")),
        reader.GetString(reader.GetOrdinal("SectionCode")),
        reader.GetString(reader.GetOrdinal("CourseCode")),
        reader.GetString(reader.GetOrdinal("CourseName")),
        reader.GetString(reader.GetOrdinal("Title")),
        SqlRepositoryHelper.GetNullableString(reader, "Description"),
        SqlRepositoryHelper.GetNullableString(reader, "MaterialType"),
        SqlRepositoryHelper.GetNullableString(reader, "FileUrl"),
        SqlRepositoryHelper.GetNullableString(reader, "ExternalUrl"),
        reader.GetBoolean(reader.GetOrdinal("IsVisible")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        SqlRepositoryHelper.GetNullableDateTime(reader, "UpdatedAt"));
}
