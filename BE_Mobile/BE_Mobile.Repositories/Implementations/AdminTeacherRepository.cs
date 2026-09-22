using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminTeacherRepository(IDbConnectionFactory factory) : IAdminTeacherRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT TeacherId, UserId, TeacherCode, DepartmentId, AcademicTitle, Specialization, Status FROM dbo.Teachers";
    private const string FilterSql = "WHERE (@Search IS NULL OR TeacherCode LIKE @Search) AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId) AND (@Status IS NULL OR Status = @Status)";

    public async Task<PagedResult<AdminTeacherDto>> ListAsync(AdminTeacherQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@DepartmentId", SqlDbType.Int, query.DepartmentId);
            AdminSql.Add(p, "@Status", SqlDbType.TinyInt, query.Status);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.Teachers " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY TeacherId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminTeacherDto?> GetAsync(long id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE TeacherId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.BigInt, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminTeacherDto?> SaveAsync(long? id, SaveAdminTeacherRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Teachers WITH (UPDLOCK, HOLDLOCK) WHERE TeacherId = @Id)
                THROW 50004, 'Record not found.', 1;
            IF NOT EXISTS (SELECT 1 FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId WHERE u.UserId = @UserId AND r.RoleCode = 'TEACHER') THROW 50001, 'User must have the TEACHER role.', 1;
            IF EXISTS (SELECT 1 FROM dbo.Students WHERE UserId = @UserId) THROW 50001, 'User already has a student profile.', 1;
            IF @Id IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Teachers WHERE TeacherId = @Id AND UserId <> @UserId)
                THROW 50001, 'Cannot transfer a teacher profile to another account.', 1;
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.Teachers (UserId, TeacherCode, DepartmentId, AcademicTitle, Specialization, Status)
                VALUES (@UserId, @TeacherCode, @DepartmentId, @AcademicTitle, @Specialization, @Status);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.Teachers SET UserId = @UserId, TeacherCode = @TeacherCode, DepartmentId = @DepartmentId, AcademicTitle = @AcademicTitle, Specialization = @Specialization, Status = @Status WHERE TeacherId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE TeacherId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.BigInt, id);
        AdminSql.Add(p, "@UserId", SqlDbType.BigInt, request.UserId);
        AdminSql.Add(p, "@TeacherCode", SqlDbType.VarChar, request.TeacherCode.Trim(), 30);
        AdminSql.Add(p, "@DepartmentId", SqlDbType.Int, request.DepartmentId);
        AdminSql.Add(p, "@AcademicTitle", SqlDbType.NVarChar, request.AcademicTitle, 100);
        AdminSql.Add(p, "@Specialization", SqlDbType.NVarChar, request.Specialization, 255);
        AdminSql.Add(p, "@Status", SqlDbType.TinyInt, request.Status);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.Teachers WHERE TeacherId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.BigInt, id), cancellationToken);

    private static AdminTeacherDto Read(SqlDataReader r) => new(
        r.GetFieldValue<long>(r.GetOrdinal("TeacherId")),
        r.GetFieldValue<long>(r.GetOrdinal("UserId")),
        r.GetFieldValue<string>(r.GetOrdinal("TeacherCode")),
        r.GetFieldValue<int>(r.GetOrdinal("DepartmentId")),
        AdminSql.Text(r, "AcademicTitle"),
        AdminSql.Text(r, "Specialization"),
        r.GetFieldValue<byte>(r.GetOrdinal("Status")));
}
