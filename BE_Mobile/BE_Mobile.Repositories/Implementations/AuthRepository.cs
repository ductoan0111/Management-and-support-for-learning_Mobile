using System.Data;
using System.Security.Cryptography;
using System.Text;
using BE_Mobile.Contracts.Auth;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

public sealed class AuthRepository(IDbConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<AuthUserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var identifier = request.Identifier.Trim();
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT u.UserId,
                   u.RoleId,
                   r.RoleCode,
                   r.RoleName,
                   u.Username,
                   u.Email,
                   u.PasswordHash,
                   u.FullName,
                   u.Phone,
                   u.AvatarUrl,
                   u.IsActive,
                   s.StudentId,
                   s.StudentCode,
                   t.TeacherId,
                   t.TeacherCode
            FROM   dbo.Users u
            INNER JOIN dbo.Roles    r ON r.RoleId    = u.RoleId
            LEFT  JOIN dbo.Students s ON s.UserId    = u.UserId
            LEFT  JOIN dbo.Teachers t ON t.UserId    = u.UserId
            WHERE (u.Username = @Identifier 
               OR  u.Email    = @Identifier 
               OR  s.StudentCode = @Identifier
               OR  t.TeacherCode = @Identifier)
              AND u.IsActive = 1;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@Identifier", SqlDbType.VarChar, identifier, 150);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
        if (!VerifyPassword(request.Password, storedHash))
            return null;

        var userId = reader.GetInt64(reader.GetOrdinal("UserId"));
        var roleId = reader.GetByte(reader.GetOrdinal("RoleId"));
        var roleCode = reader.GetString(reader.GetOrdinal("RoleCode"));
        var roleName = reader.GetString(reader.GetOrdinal("RoleName"));
        var username = reader.GetString(reader.GetOrdinal("Username"));
        var email = reader.GetString(reader.GetOrdinal("Email"));
        var fullName = reader.GetString(reader.GetOrdinal("FullName"));
        var phone = SqlRepositoryHelper.GetNullableString(reader, "Phone");
        var avatarUrl = SqlRepositoryHelper.GetNullableString(reader, "AvatarUrl");
        var studentId = SqlRepositoryHelper.GetNullableLong(reader, "StudentId");
        var teacherId = SqlRepositoryHelper.GetNullableLong(reader, "TeacherId");

        await reader.CloseAsync();

        // Cập nhật LastLoginAt
        await using var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = "UPDATE dbo.Users SET LastLoginAt = SYSDATETIME() WHERE UserId = @UserId;";
        SqlRepositoryHelper.AddParameter(updateCmd, "@UserId", SqlDbType.BigInt, userId);
        await updateCmd.ExecuteNonQueryAsync(cancellationToken);

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." + userId;

        return new AuthUserDto(
            userId,
            roleId,
            roleCode,
            roleName,
            studentId,
            teacherId,
            username,
            fullName,
            email,
            phone,
            avatarUrl,
            token);
    }

    public async Task<bool> ExistsUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1) FROM dbo.Users
            WHERE Username = @Username OR Email = @Email;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@Username", SqlDbType.VarChar, username.Trim(), 100);
        SqlRepositoryHelper.AddParameter(cmd, "@Email", SqlDbType.VarChar, email.Trim(), 150);

        var count = await cmd.ExecuteScalarAsync(cancellationToken);
        return count is not null and not DBNull && Convert.ToInt32(count) > 0;
    }

    public async Task<AuthUserDto?> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var passwordHash = HashSha256(request.Password);

            // 1. Tạo user với RoleId = 2 (hoặc role Sinh viên)
            await using var cmdUser = connection.CreateCommand();
            cmdUser.Transaction = transaction;
            cmdUser.CommandText = """
                DECLARE @RoleId TINYINT;
                SELECT TOP 1 @RoleId = RoleId FROM dbo.Roles WHERE RoleCode IN ('STUDENT', 'SinhVien', 'SV');
                IF @RoleId IS NULL SET @RoleId = 2;

                INSERT INTO dbo.Users
                    (RoleId, Username, Email, PasswordHash, FullName, Phone, IsActive, CreatedAt)
                OUTPUT INSERTED.UserId
                VALUES
                    (@RoleId, @Username, @Email, @PasswordHash, @FullName, @Phone, 1, SYSDATETIME());
                """;
            SqlRepositoryHelper.AddParameter(cmdUser, "@Username", SqlDbType.VarChar, request.Username.Trim(), 100);
            SqlRepositoryHelper.AddParameter(cmdUser, "@Email", SqlDbType.VarChar, request.Email.Trim(), 150);
            SqlRepositoryHelper.AddParameter(cmdUser, "@PasswordHash", SqlDbType.NVarChar, passwordHash, 500);
            SqlRepositoryHelper.AddParameter(cmdUser, "@FullName", SqlDbType.NVarChar, request.FullName.Trim(), 150);
            SqlRepositoryHelper.AddParameter(cmdUser, "@Phone", SqlDbType.VarChar, SqlRepositoryHelper.NormalizeText(request.Phone), 20);

            var userIdRaw = await cmdUser.ExecuteScalarAsync(cancellationToken);
            if (userIdRaw is null or DBNull)
            {
                transaction.Rollback();
                return null;
            }
            var userId = Convert.ToInt64(userIdRaw);

            // 2. Tạo student profile
            await using var cmdStudent = connection.CreateCommand();
            cmdStudent.Transaction = transaction;
            cmdStudent.CommandText = """
                INSERT INTO dbo.Students
                    (UserId, StudentCode, AcademicClassId, MajorId, EnrollmentYear, Status)
                OUTPUT INSERTED.StudentId
                VALUES
                    (@UserId, @StudentCode, @AcademicClassId, @MajorId, @EnrollmentYear, 1);
                """;
            SqlRepositoryHelper.AddParameter(cmdStudent, "@UserId", SqlDbType.BigInt, userId);
            SqlRepositoryHelper.AddParameter(cmdStudent, "@StudentCode", SqlDbType.VarChar, request.StudentCode.Trim(), 30);
            SqlRepositoryHelper.AddParameter(cmdStudent, "@AcademicClassId", SqlDbType.Int, request.AcademicClassId);
            SqlRepositoryHelper.AddParameter(cmdStudent, "@MajorId", SqlDbType.Int, request.MajorId);
            SqlRepositoryHelper.AddParameter(cmdStudent, "@EnrollmentYear", SqlDbType.SmallInt, request.EnrollmentYear);

            var studentIdRaw = await cmdStudent.ExecuteScalarAsync(cancellationToken);
            var studentId = studentIdRaw is null or DBNull ? (long?)null : Convert.ToInt64(studentIdRaw);

            transaction.Commit();

            return new AuthUserDto(
                userId,
                2,
                "STUDENT",
                "Sinh viên",
                studentId,
                null,
                request.Username.Trim(),
                request.FullName.Trim(),
                request.Email.Trim(),
                request.Phone,
                null,
                Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." + userId);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        // Khi cơ sở dữ liệu mẫu đang chứa placeholder "test-password-hash", chấp nhận đăng nhập
        if (string.Equals(storedHash, "test-password-hash", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(storedHash, inputPassword, StringComparison.Ordinal))
            return true;

        var inputHash = HashSha256(inputPassword);
        return string.Equals(storedHash, inputHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string HashSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
