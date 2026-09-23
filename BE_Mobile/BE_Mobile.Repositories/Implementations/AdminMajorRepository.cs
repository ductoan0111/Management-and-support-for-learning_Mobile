using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminMajorRepository(IDbConnectionFactory factory) : IAdminMajorRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT MajorId, DepartmentId, MajorCode, MajorName, Description, IsActive FROM dbo.Majors";
    private const string FilterSql = "WHERE (@Search IS NULL OR MajorCode LIKE @Search OR MajorName LIKE @Search) AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId) AND (@IsActive IS NULL OR IsActive = @IsActive)";

    public async Task<PagedResult<AdminMajorDto>> ListAsync(AdminMajorQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@DepartmentId", SqlDbType.Int, query.DepartmentId);
            AdminSql.Add(p, "@IsActive", SqlDbType.Bit, query.IsActive);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.Majors " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY MajorId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminMajorDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE MajorId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminMajorDto?> SaveAsync(int? id, SaveAdminMajorRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Majors WITH (UPDLOCK, HOLDLOCK) WHERE MajorId = @Id)
                THROW 50004, 'Record not found.', 1;
            IF @Id IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Majors WHERE MajorId = @Id AND DepartmentId <> @DepartmentId) AND EXISTS (SELECT 1 FROM dbo.Students WHERE MajorId = @Id) THROW 50001, 'Cannot move a major with student records to another department.', 1;
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.Majors (DepartmentId, MajorCode, MajorName, Description, IsActive)
                VALUES (@DepartmentId, @MajorCode, @MajorName, @Description, @IsActive);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.Majors SET DepartmentId = @DepartmentId, MajorCode = @MajorCode, MajorName = @MajorName, Description = @Description, IsActive = @IsActive WHERE MajorId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE MajorId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.Int, id);
        AdminSql.Add(p, "@DepartmentId", SqlDbType.Int, request.DepartmentId);
        AdminSql.Add(p, "@MajorCode", SqlDbType.VarChar, request.MajorCode.Trim(), 20);
        AdminSql.Add(p, "@MajorName", SqlDbType.NVarChar, request.MajorName.Trim(), 150);
        AdminSql.Add(p, "@Description", SqlDbType.NVarChar, request.Description, 500);
        AdminSql.Add(p, "@IsActive", SqlDbType.Bit, request.IsActive);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.Majors WHERE MajorId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), cancellationToken);

    private static AdminMajorDto Read(SqlDataReader r) => new(
        r.GetFieldValue<int>(r.GetOrdinal("MajorId")),
        r.GetFieldValue<int>(r.GetOrdinal("DepartmentId")),
        r.GetFieldValue<string>(r.GetOrdinal("MajorCode")),
        r.GetFieldValue<string>(r.GetOrdinal("MajorName")),
        AdminSql.Text(r, "Description"),
        r.GetFieldValue<bool>(r.GetOrdinal("IsActive")));
}
