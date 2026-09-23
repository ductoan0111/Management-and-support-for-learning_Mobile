using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminCourseRepository(IDbConnectionFactory factory) : IAdminCourseRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT CourseId, DepartmentId, CourseCode, CourseName, Credits, Description, IsActive FROM dbo.Courses";
    private const string FilterSql = "WHERE (@Search IS NULL OR CourseCode LIKE @Search OR CourseName LIKE @Search) AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId) AND (@IsActive IS NULL OR IsActive = @IsActive)";

    public async Task<PagedResult<AdminCourseDto>> ListAsync(AdminCourseQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@DepartmentId", SqlDbType.Int, query.DepartmentId);
            AdminSql.Add(p, "@IsActive", SqlDbType.Bit, query.IsActive);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.Courses " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY CourseId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminCourseDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE CourseId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminCourseDto?> SaveAsync(int? id, SaveAdminCourseRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Courses WITH (UPDLOCK, HOLDLOCK) WHERE CourseId = @Id)
                THROW 50004, 'Record not found.', 1;
            
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.Courses (DepartmentId, CourseCode, CourseName, Credits, Description, IsActive)
                VALUES (@DepartmentId, @CourseCode, @CourseName, @Credits, @Description, @IsActive);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.Courses SET DepartmentId = @DepartmentId, CourseCode = @CourseCode, CourseName = @CourseName, Credits = @Credits, Description = @Description, IsActive = @IsActive WHERE CourseId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE CourseId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.Int, id);
        AdminSql.Add(p, "@DepartmentId", SqlDbType.Int, request.DepartmentId);
        AdminSql.Add(p, "@CourseCode", SqlDbType.VarChar, request.CourseCode.Trim(), 30);
        AdminSql.Add(p, "@CourseName", SqlDbType.NVarChar, request.CourseName.Trim(), 200);
        AdminSql.Add(p, "@Credits", SqlDbType.TinyInt, request.Credits);
        AdminSql.Add(p, "@Description", SqlDbType.NVarChar, request.Description, -1);
        AdminSql.Add(p, "@IsActive", SqlDbType.Bit, request.IsActive);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.Courses WHERE CourseId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.Int, id), cancellationToken);

    private static AdminCourseDto Read(SqlDataReader r) => new(
        r.GetFieldValue<int>(r.GetOrdinal("CourseId")),
        r.GetFieldValue<int>(r.GetOrdinal("DepartmentId")),
        r.GetFieldValue<string>(r.GetOrdinal("CourseCode")),
        r.GetFieldValue<string>(r.GetOrdinal("CourseName")),
        r.GetFieldValue<byte>(r.GetOrdinal("Credits")),
        AdminSql.Text(r, "Description"),
        r.GetFieldValue<bool>(r.GetOrdinal("IsActive")));
}
