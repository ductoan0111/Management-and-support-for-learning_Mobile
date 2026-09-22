using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminUserRepository(IDbConnectionFactory factory) : IAdminUserRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = """
        SELECT u.UserId, u.RoleId, r.RoleCode, u.Username, u.Email, u.FullName, u.Phone, u.IsActive
        FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId
        """;

    public async Task<PagedResult<AdminUserDto>> ListAsync(AdminUserQuery query, CancellationToken cancellationToken)
    {
        const string filter = """
            WHERE (@Search IS NULL OR u.Username LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search)
            AND (@RoleId IS NULL OR u.RoleId = @RoleId) AND (@IsActive IS NULL OR u.IsActive = @IsActive)
            """;
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@RoleId", SqlDbType.TinyInt, query.RoleId);
            AdminSql.Add(p, "@IsActive", SqlDbType.Bit, query.IsActive);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.Users u " + filter, Parameters, r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + filter + " ORDER BY u.UserId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;", Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminUserDto?> GetAsync(long id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE u.UserId = @Id;", p => AdminSql.Add(p, "@Id", SqlDbType.BigInt, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminUserCredentials?> CredentialsAsync(long? id, string? username, CancellationToken cancellationToken) =>
        (await db.QueryAsync("""
            SELECT u.UserId, u.RoleId, r.RoleCode, u.Username, u.Email, u.FullName, u.Phone, u.IsActive, u.PasswordHash
            FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId
            WHERE (@Id IS NOT NULL AND u.UserId = @Id) OR (@Id IS NULL AND u.Username = @Username);
            """, p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.BigInt, id);
            AdminSql.Add(p, "@Username", SqlDbType.VarChar, username, 100);
        }, r => new AdminUserCredentials(Read(r), r.GetString(r.GetOrdinal("PasswordHash"))), cancellationToken)).SingleOrDefault();

    public async Task<AdminUserDto?> CreateAsync(CreateAdminUserRequest request, string passwordHash, CancellationToken cancellationToken) =>
        (await db.QueryAsync("""
            INSERT INTO dbo.Users (RoleId, Username, Email, PasswordHash, FullName, Phone, IsActive)
            VALUES (@RoleId, @Username, @Email, @PasswordHash, @FullName, @Phone, @IsActive);
            DECLARE @Id bigint = SCOPE_IDENTITY();
            """ + SelectSql + " WHERE u.UserId = @Id;", p =>
        {
            ProfileParameters(p, request);
            AdminSql.Add(p, "@RoleId", SqlDbType.TinyInt, request.RoleId);
            AdminSql.Add(p, "@Username", SqlDbType.VarChar, request.Username.Trim(), 100);
            AdminSql.Add(p, "@PasswordHash", SqlDbType.NVarChar, passwordHash, 500);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();

    // Lock the administrator set before changes so concurrent requests cannot remove the last active admin.
    private const string GuardSql = """
        DECLARE @AdminCount int = (SELECT COUNT(*) FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
            WHERE IsActive = 1 AND RoleId = (SELECT RoleId FROM dbo.Roles WHERE RoleCode = 'ADMIN'));
        IF NOT EXISTS (SELECT 1 FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE UserId = @Id)
            THROW 50004, 'User not found.', 1;
        """;

    public async Task<AdminUserDto?> UpdateAsync(long id, UpdateAdminUserRequest request, CancellationToken cancellationToken) =>
        (await db.QueryAsync(GuardSql + """
            IF @IsActive = 0 AND @AdminCount <= 1 AND EXISTS
                (SELECT 1 FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId WHERE u.UserId = @Id AND u.IsActive = 1 AND r.RoleCode = 'ADMIN')
                THROW 50001, 'Cannot disable the last active administrator.', 1;
            UPDATE dbo.Users SET Email = @Email, FullName = @FullName, Phone = @Phone,
                IsActive = @IsActive, UpdatedAt = SYSDATETIME() WHERE UserId = @Id;
            """ + SelectSql + " WHERE u.UserId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.BigInt, id);
            ProfileParameters(p, request);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();

    public async Task<AdminUserDto?> SetRoleAsync(long id, byte roleId, CancellationToken cancellationToken) =>
        (await db.QueryAsync(GuardSql + """
            DECLARE @Code varchar(20) = (SELECT RoleCode FROM dbo.Roles WHERE RoleId = @RoleId);
            IF @Code IS NULL THROW 50001, 'Role not found.', 1;
            IF @Code <> 'ADMIN' AND @AdminCount <= 1 AND EXISTS
                (SELECT 1 FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId WHERE u.UserId = @Id AND u.IsActive = 1 AND r.RoleCode = 'ADMIN')
                THROW 50001, 'Cannot change the role of the last active administrator.', 1;
            IF @Code <> 'STUDENT' AND EXISTS (SELECT 1 FROM dbo.Students WHERE UserId = @Id)
                THROW 50001, 'Role conflicts with the student profile.', 1;
            IF @Code <> 'TEACHER' AND EXISTS (SELECT 1 FROM dbo.Teachers WHERE UserId = @Id)
                THROW 50001, 'Role conflicts with the teacher profile.', 1;
            UPDATE dbo.Users SET RoleId = @RoleId, UpdatedAt = SYSDATETIME() WHERE UserId = @Id;
            """ + SelectSql + " WHERE u.UserId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.BigInt, id);
            AdminSql.Add(p, "@RoleId", SqlDbType.TinyInt, roleId);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();

    public Task<bool> ResetPasswordAsync(long id, string passwordHash, CancellationToken cancellationToken) =>
        db.ExecuteAsync("UPDATE dbo.Users SET PasswordHash = @Hash, UpdatedAt = SYSDATETIME() WHERE UserId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.BigInt, id);
            AdminSql.Add(p, "@Hash", SqlDbType.NVarChar, passwordHash, 500);
        }, cancellationToken);

    public async Task<IReadOnlyList<AdminRoleDto>> RolesAsync(CancellationToken cancellationToken) =>
        await db.QueryAsync("SELECT RoleId, RoleCode, RoleName, Description FROM dbo.Roles ORDER BY RoleId;", _ => { },
            r => new AdminRoleDto(r.GetByte(0), r.GetString(1), r.GetString(2), r.IsDBNull(3) ? null : r.GetString(3)), cancellationToken);

    private static void ProfileParameters(SqlParameterCollection p, UpdateAdminUserRequest request)
    {
        AdminSql.Add(p, "@Email", SqlDbType.VarChar, request.Email.Trim(), 150);
        AdminSql.Add(p, "@FullName", SqlDbType.NVarChar, request.FullName.Trim(), 150);
        AdminSql.Add(p, "@Phone", SqlDbType.VarChar, request.Phone, 20);
        AdminSql.Add(p, "@IsActive", SqlDbType.Bit, request.IsActive);
    }

    private static AdminUserDto Read(SqlDataReader r) => new(r.GetInt64(0), r.GetByte(1), r.GetString(2),
        r.GetString(3), r.GetString(4), r.GetString(5), r.IsDBNull(6) ? null : r.GetString(6), r.GetBoolean(7));
}
