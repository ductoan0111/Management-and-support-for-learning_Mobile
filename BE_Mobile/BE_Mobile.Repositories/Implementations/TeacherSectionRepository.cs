using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

public sealed class TeacherSectionRepository(IDbConnectionFactory connectionFactory) : ITeacherSectionRepository
{
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
}
