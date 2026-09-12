using System.Data;
using System.Globalization;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminStudentRepository(IDbConnectionFactory connectionFactory) : IAdminStudentRepository
{
    public async Task<PagedResult<AdminStudentDto>> GetStudentsAsync(
        string? search,
        byte? status,
        int? majorId,
        int? academicClassId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var filters = BuildStudentFilters(search, status, majorId, academicClassId);
        var whereSql = filters.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", filters)}";

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = $"""
            SELECT COUNT(1)
            FROM dbo.Students s
            INNER JOIN dbo.Users u ON u.UserId = s.UserId
            INNER JOIN dbo.Majors m ON m.MajorId = s.MajorId
            LEFT JOIN dbo.AcademicClasses ac ON ac.AcademicClassId = s.AcademicClassId
            {whereSql};
            """;
        AddFilterParameters(countCommand, search, status, majorId, academicClassId);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentSelectSql}
            {whereSql}
            ORDER BY s.StudentCode
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;
        AddFilterParameters(command, search, status, majorId, academicClassId);
        AddParameter(command, "@Offset", SqlDbType.Int, (page - 1) * pageSize);
        AddParameter(command, "@PageSize", SqlDbType.Int, pageSize);

        var students = new List<AdminStudentDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            students.Add(ReadStudent(reader));
        }

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<AdminStudentDto>(students, page, pageSize, totalCount, totalPages);
    }

    public Task<AdminStudentDto?> GetStudentAsync(long studentId, CancellationToken cancellationToken)
    {
        return FindStudentAsync(studentId, cancellationToken);
    }

    public async Task<AdminStudentDto?> GetStudentByUserAsync(long userId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentSelectSql}
            WHERE s.UserId = @UserId;
            """;
        AddParameter(command, "@UserId", SqlDbType.BigInt, userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStudent(reader) : null;
    }

    public async Task<AdminStudentDto?> CreateStudentAsync(
        CreateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Students
                (UserId, StudentCode, AcademicClassId, MajorId, EnrollmentYear, Status)
            OUTPUT INSERTED.StudentId
            VALUES
                (@UserId, @StudentCode, @AcademicClassId, @MajorId, @EnrollmentYear, @Status);
            """;
        AddStudentWriteParameters(
            command,
            request.UserId,
            request.StudentCode,
            request.AcademicClassId,
            request.MajorId,
            request.EnrollmentYear,
            request.Status);

        var studentId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        return await FindStudentAsync(studentId, cancellationToken);
    }

    public async Task<AdminStudentDto?> UpdateStudentAsync(
        long studentId,
        UpdateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var userId = await GetStudentUserIdAsync(connection, transaction, studentId, cancellationToken);
        if (userId is null)
        {
            transaction.Rollback();
            return null;
        }

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE dbo.Students
                SET StudentCode = @StudentCode,
                    AcademicClassId = @AcademicClassId,
                    MajorId = @MajorId,
                    EnrollmentYear = @EnrollmentYear,
                    Status = @Status
                WHERE StudentId = @StudentId;
                """;
            AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
            AddStudentWriteParameters(
                command,
                userId.Value,
                request.StudentCode,
                request.AcademicClassId,
                request.MajorId,
                request.EnrollmentYear,
                request.Status);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await UpdateUserProfileIfNeededAsync(connection, transaction, userId.Value, request, cancellationToken);
        transaction.Commit();

        return await FindStudentAsync(studentId, cancellationToken);
    }

    public async Task<bool> DeleteStudentAsync(long studentId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Students WHERE StudentId = @StudentId;";
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    private async Task<AdminStudentDto?> FindStudentAsync(long studentId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentSelectSql}
            WHERE s.StudentId = @StudentId;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStudent(reader) : null;
    }

    private static async Task<long?> GetStudentUserIdAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT UserId FROM dbo.Students WHERE StudentId = @StudentId;";
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static async Task UpdateUserProfileIfNeededAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long userId,
        UpdateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        var setParts = new List<string>();

        AddUserSetPart(setParts, request.FullName, "FullName");
        AddUserSetPart(setParts, request.Email, "Email");
        AddUserSetPart(setParts, request.Phone, "Phone");
        AddUserSetPart(setParts, request.DateOfBirth, "DateOfBirth");
        AddUserSetPart(setParts, request.Gender, "Gender");
        AddUserSetPart(setParts, request.AvatarUrl, "AvatarUrl");
        AddUserSetPart(setParts, request.IsActive, "IsActive");

        if (setParts.Count == 0)
        {
            return;
        }

        setParts.Add("UpdatedAt = SYSDATETIME()");

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"UPDATE dbo.Users SET {string.Join(", ", setParts)} WHERE UserId = @UserId;";
        AddParameter(command, "@UserId", SqlDbType.BigInt, userId);

        AddOptionalParameter(command, request.FullName, "@FullName", SqlDbType.NVarChar, 150);
        AddOptionalParameter(command, request.Email, "@Email", SqlDbType.VarChar, 150);
        AddOptionalParameter(command, request.Phone, "@Phone", SqlDbType.VarChar, 20);
        AddOptionalParameter(command, request.DateOfBirth, "@DateOfBirth", SqlDbType.Date);
        AddOptionalParameter(command, request.Gender, "@Gender", SqlDbType.TinyInt);
        AddOptionalParameter(command, request.AvatarUrl, "@AvatarUrl", SqlDbType.NVarChar, 1000);
        AddOptionalParameter(command, request.IsActive, "@IsActive", SqlDbType.Bit);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddUserSetPart<T>(List<string> setParts, T? value, string columnName)
    {
        if (value is not null)
        {
            setParts.Add($"{columnName} = @{columnName}");
        }
    }

    private static void AddOptionalParameter<T>(
        SqlCommand command,
        T? value,
        string name,
        SqlDbType type,
        int? size = null)
    {
        if (value is not null)
        {
            AddParameter(command, name, type, value, size);
        }
    }

    private static IReadOnlyList<string> BuildStudentFilters(
        string? search,
        byte? status,
        int? majorId,
        int? academicClassId)
    {
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add("(s.StudentCode LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search)");
        }

        if (status is not null)
        {
            filters.Add("s.Status = @Status");
        }

        if (majorId is not null)
        {
            filters.Add("s.MajorId = @MajorId");
        }

        if (academicClassId is not null)
        {
            filters.Add("s.AcademicClassId = @AcademicClassId");
        }

        return filters;
    }

    private static void AddFilterParameters(
        SqlCommand command,
        string? search,
        byte? status,
        int? majorId,
        int? academicClassId)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            AddParameter(command, "@Search", SqlDbType.NVarChar, $"%{search.Trim()}%", 256);
        }

        if (status is not null)
        {
            AddParameter(command, "@Status", SqlDbType.TinyInt, status.Value);
        }

        if (majorId is not null)
        {
            AddParameter(command, "@MajorId", SqlDbType.Int, majorId.Value);
        }

        if (academicClassId is not null)
        {
            AddParameter(command, "@AcademicClassId", SqlDbType.Int, academicClassId.Value);
        }
    }

    private static void AddStudentWriteParameters(
        SqlCommand command,
        long userId,
        string studentCode,
        int? academicClassId,
        int majorId,
        short enrollmentYear,
        byte status)
    {
        AddParameter(command, "@UserId", SqlDbType.BigInt, userId);
        AddParameter(command, "@StudentCode", SqlDbType.VarChar, studentCode.Trim(), 30);
        AddParameter(command, "@AcademicClassId", SqlDbType.Int, academicClassId);
        AddParameter(command, "@MajorId", SqlDbType.Int, majorId);
        AddParameter(command, "@EnrollmentYear", SqlDbType.SmallInt, enrollmentYear);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);
    }

    private static AdminStudentDto ReadStudent(SqlDataReader reader)
    {
        return new AdminStudentDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetInt64(reader.GetOrdinal("UserId")),
            reader.GetString(reader.GetOrdinal("StudentCode")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("Email")),
            GetNullableString(reader, "Phone"),
            GetNullableDateOnly(reader, "DateOfBirth"),
            GetNullableByte(reader, "Gender"),
            GetNullableString(reader, "AvatarUrl"),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            GetNullableInt(reader, "AcademicClassId"),
            GetNullableString(reader, "ClassCode"),
            GetNullableString(reader, "ClassName"),
            reader.GetInt32(reader.GetOrdinal("MajorId")),
            reader.GetString(reader.GetOrdinal("MajorCode")),
            reader.GetString(reader.GetOrdinal("MajorName")),
            reader.GetInt32(reader.GetOrdinal("DepartmentId")),
            reader.GetInt16(reader.GetOrdinal("EnrollmentYear")),
            reader.GetByte(reader.GetOrdinal("Status")));
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
        parameter.Value = ToDbValue(value);
        return parameter;
    }

    private static object ToDbValue(object? value)
    {
        return value switch
        {
            null => DBNull.Value,
            DateOnly date => date.ToDateTime(TimeOnly.MinValue),
            _ => value
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static byte? GetNullableByte(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetByte(ordinal);
    }

    private static DateOnly? GetNullableDateOnly(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(ordinal));
    }

    private const string StudentSelectSql = """
        SELECT
            s.StudentId,
            s.UserId,
            s.StudentCode,
            u.FullName,
            u.Email,
            u.Phone,
            u.DateOfBirth,
            u.Gender,
            u.AvatarUrl,
            u.IsActive,
            s.AcademicClassId,
            ac.ClassCode,
            ac.ClassName,
            s.MajorId,
            m.MajorCode,
            m.MajorName,
            m.DepartmentId,
            s.EnrollmentYear,
            s.Status
        FROM dbo.Students s
        INNER JOIN dbo.Users u ON u.UserId = s.UserId
        INNER JOIN dbo.Majors m ON m.MajorId = s.MajorId
        LEFT JOIN dbo.AcademicClasses ac ON ac.AcademicClassId = s.AcademicClassId
        """;
}
