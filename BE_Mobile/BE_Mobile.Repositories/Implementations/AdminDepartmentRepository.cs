using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminDepartmentRepository(IDbConnectionFactory factory) : IAdminDepartmentRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT DepartmentId, DepartmentCode, DepartmentName, Description, IsActive FROM dbo.Departments";
    private const string FilterSql = "WHERE (@Search IS NULL OR DepartmentCode LIKE @Search OR DepartmentName LIKE @Search) AND (@IsActive IS NULL OR IsActive = @IsActive)";

    public async Task<PagedResult<AdminDepartmentDto>> ListAsync(AdminDepartmentQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@IsActive", SqlDbType.Bit, query.IsActive);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.Departments " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY DepartmentId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminDepartmentDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE DepartmentId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminDepartmentDto?> SaveAsync(int? id, SaveAdminDepartmentRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Departments WITH (UPDLOCK, HOLDLOCK) WHERE DepartmentId = @Id)
                THROW 50004, 'Record not found.', 1;
            
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.Departments (DepartmentCode, DepartmentName, Description, IsActive)
                VALUES (@DepartmentCode, @DepartmentName, @Description, @IsActive);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.Departments SET DepartmentCode = @DepartmentCode, DepartmentName = @DepartmentName, Description = @Description, IsActive = @IsActive WHERE DepartmentId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE DepartmentId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.Int, id);
        AdminSql.Add(p, "@DepartmentCode", SqlDbType.VarChar, request.DepartmentCode.Trim(), 20);
        AdminSql.Add(p, "@DepartmentName", SqlDbType.NVarChar, request.DepartmentName.Trim(), 150);
        AdminSql.Add(p, "@Description", SqlDbType.NVarChar, request.Description, 500);
        AdminSql.Add(p, "@IsActive", SqlDbType.Bit, request.IsActive);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.Departments WHERE DepartmentId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), cancellationToken);

    private static AdminDepartmentDto Read(SqlDataReader r) => new(
        r.GetFieldValue<int>(r.GetOrdinal("DepartmentId")),
        r.GetFieldValue<string>(r.GetOrdinal("DepartmentCode")),
        r.GetFieldValue<string>(r.GetOrdinal("DepartmentName")),
        AdminSql.Text(r, "Description"),
        r.GetFieldValue<bool>(r.GetOrdinal("IsActive")));
}
