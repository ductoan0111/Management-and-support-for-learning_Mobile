using System.Data;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

internal static class SqlRepositoryHelper
{
    public static SqlParameter AddParameter(
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

    public static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }

    public static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static long? GetNullableLong(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    public static byte? GetNullableByte(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetByte(ordinal);
    }

    public static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    public static DateOnly? GetNullableDate(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(ordinal));
    }

    public static async Task<bool> IsTeacherOfSectionAsync(
        SqlConnection connection,
        long teacherId,
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1) FROM dbo.SectionTeachers
            WHERE SectionId = @SectionId AND TeacherId = @TeacherId;
            """;
        AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        var count = await cmd.ExecuteScalarAsync(cancellationToken);
        return count is not null and not DBNull && Convert.ToInt32(count) > 0;
    }

    public static async Task<long?> GetUserIdByTeacherAsync(
        SqlConnection connection,
        long teacherId,
        CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT UserId FROM dbo.Teachers WHERE TeacherId = @TeacherId;";
        AddParameter(cmd, "@TeacherId", SqlDbType.BigInt, teacherId);
        var raw = await cmd.ExecuteScalarAsync(cancellationToken);
        return raw is null or DBNull ? null : (long)raw;
    }
}
