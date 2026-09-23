using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminAcademicClassRepository(IDbConnectionFactory factory) : IAdminAcademicClassRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT AcademicClassId, MajorId, ClassCode, ClassName, IntakeYear, GraduationYear, IsActive FROM dbo.AcademicClasses";
    private const string FilterSql = "WHERE (@Search IS NULL OR ClassCode LIKE @Search OR ClassName LIKE @Search) AND (@MajorId IS NULL OR MajorId = @MajorId) AND (@IsActive IS NULL OR IsActive = @IsActive)";

    public async Task<PagedResult<AdminAcademicClassDto>> ListAsync(AdminAcademicClassQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@MajorId", SqlDbType.Int, query.MajorId);
            AdminSql.Add(p, "@IsActive", SqlDbType.Bit, query.IsActive);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.AcademicClasses " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY AcademicClassId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminAcademicClassDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE AcademicClassId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminAcademicClassDto?> SaveAsync(int? id, SaveAdminAcademicClassRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.AcademicClasses WITH (UPDLOCK, HOLDLOCK) WHERE AcademicClassId = @Id)
                THROW 50004, 'Record not found.', 1;
            IF @Id IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Students WHERE AcademicClassId = @Id AND MajorId <> @MajorId) THROW 50001, 'Class major must match its students.', 1;
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.AcademicClasses (MajorId, ClassCode, ClassName, IntakeYear, GraduationYear, IsActive)
                VALUES (@MajorId, @ClassCode, @ClassName, @IntakeYear, @GraduationYear, @IsActive);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.AcademicClasses SET MajorId = @MajorId, ClassCode = @ClassCode, ClassName = @ClassName, IntakeYear = @IntakeYear, GraduationYear = @GraduationYear, IsActive = @IsActive WHERE AcademicClassId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE AcademicClassId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.Int, id);
        AdminSql.Add(p, "@MajorId", SqlDbType.Int, request.MajorId);
        AdminSql.Add(p, "@ClassCode", SqlDbType.VarChar, request.ClassCode.Trim(), 30);
        AdminSql.Add(p, "@ClassName", SqlDbType.NVarChar, request.ClassName.Trim(), 150);
        AdminSql.Add(p, "@IntakeYear", SqlDbType.SmallInt, request.IntakeYear);
        AdminSql.Add(p, "@GraduationYear", SqlDbType.SmallInt, request.GraduationYear);
        AdminSql.Add(p, "@IsActive", SqlDbType.Bit, request.IsActive);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.AcademicClasses WHERE AcademicClassId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), cancellationToken);

    private static AdminAcademicClassDto Read(SqlDataReader r) => new(
        r.GetFieldValue<int>(r.GetOrdinal("AcademicClassId")),
        r.GetFieldValue<int>(r.GetOrdinal("MajorId")),
        r.GetFieldValue<string>(r.GetOrdinal("ClassCode")),
        r.GetFieldValue<string>(r.GetOrdinal("ClassName")),
        r.GetFieldValue<short>(r.GetOrdinal("IntakeYear")),
        AdminSql.Nullable<short>(r, "GraduationYear"),
        r.GetFieldValue<bool>(r.GetOrdinal("IsActive")));
}
