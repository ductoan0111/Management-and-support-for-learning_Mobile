using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminSemesterRepository(IDbConnectionFactory factory) : IAdminSemesterRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT SemesterId, SemesterCode, SemesterName, AcademicYear, StartDate, EndDate, IsCurrent FROM dbo.Semesters";
    private const string FilterSql = "WHERE (@Search IS NULL OR SemesterCode LIKE @Search OR SemesterName LIKE @Search OR AcademicYear LIKE @Search) AND (@IsCurrent IS NULL OR IsCurrent = @IsCurrent)";

    public async Task<PagedResult<AdminSemesterDto>> ListAsync(AdminSemesterQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@IsCurrent", SqlDbType.Bit, query.IsCurrent);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.Semesters " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY SemesterId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminSemesterDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE SemesterId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminSemesterDto?> SaveAsync(int? id, SaveAdminSemesterRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Semesters WITH (UPDLOCK, HOLDLOCK) WHERE SemesterId = @Id)
                THROW 50004, 'Record not found.', 1;
            IF @IsCurrent = 1 UPDATE dbo.Semesters WITH (UPDLOCK, HOLDLOCK) SET IsCurrent = 0 WHERE IsCurrent = 1;
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.Semesters (SemesterCode, SemesterName, AcademicYear, StartDate, EndDate, IsCurrent)
                VALUES (@SemesterCode, @SemesterName, @AcademicYear, @StartDate, @EndDate, @IsCurrent);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.Semesters SET SemesterCode = @SemesterCode, SemesterName = @SemesterName, AcademicYear = @AcademicYear, StartDate = @StartDate, EndDate = @EndDate, IsCurrent = @IsCurrent WHERE SemesterId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE SemesterId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.Int, id);
        AdminSql.Add(p, "@SemesterCode", SqlDbType.VarChar, request.SemesterCode.Trim(), 30);
        AdminSql.Add(p, "@SemesterName", SqlDbType.NVarChar, request.SemesterName.Trim(), 100);
        AdminSql.Add(p, "@AcademicYear", SqlDbType.VarChar, request.AcademicYear.Trim(), 20);
        AdminSql.Add(p, "@StartDate", SqlDbType.Date, request.StartDate);
        AdminSql.Add(p, "@EndDate", SqlDbType.Date, request.EndDate);
        AdminSql.Add(p, "@IsCurrent", SqlDbType.Bit, request.IsCurrent);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.Semesters WHERE SemesterId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), cancellationToken);

    private static AdminSemesterDto Read(SqlDataReader r) => new(
        r.GetFieldValue<int>(r.GetOrdinal("SemesterId")),
        r.GetFieldValue<string>(r.GetOrdinal("SemesterCode")),
        r.GetFieldValue<string>(r.GetOrdinal("SemesterName")),
        r.GetFieldValue<string>(r.GetOrdinal("AcademicYear")),
        DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("StartDate"))),
        DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("EndDate"))),
        r.GetFieldValue<bool>(r.GetOrdinal("IsCurrent")));
}
