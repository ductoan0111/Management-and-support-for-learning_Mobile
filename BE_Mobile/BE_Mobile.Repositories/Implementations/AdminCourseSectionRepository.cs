using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminCourseSectionRepository(IDbConnectionFactory factory) : IAdminCourseSectionRepository
{
    private readonly AdminSql db = new(factory);
    private const string SelectSql = "SELECT SectionId, CourseId, SemesterId, SectionCode, SectionName, MaxStudents, Status FROM dbo.CourseSections";
    private const string FilterSql = "WHERE (@Search IS NULL OR SectionCode LIKE @Search OR SectionName LIKE @Search) AND (@CourseId IS NULL OR CourseId = @CourseId) AND (@SemesterId IS NULL OR SemesterId = @SemesterId) AND (@Status IS NULL OR Status = @Status)";

    public async Task<PagedResult<AdminCourseSectionDto>> ListAsync(AdminCourseSectionQuery query, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@Search", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(query.Search) ? null : "%" + query.Search.Trim() + "%", 402);
            AdminSql.Add(p, "@CourseId", SqlDbType.Int, query.CourseId);
            AdminSql.Add(p, "@SemesterId", SqlDbType.Int, query.SemesterId);
            AdminSql.Add(p, "@Status", SqlDbType.TinyInt, query.Status);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync("SELECT COUNT(*) FROM dbo.CourseSections " + FilterSql, Parameters,
            r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(SelectSql + " " + FilterSql + " ORDER BY SectionId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;",
            Parameters, Read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminCourseSectionDto?> GetAsync(long id, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SelectSql + " WHERE SectionId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.BigInt, id), Read, cancellationToken)).SingleOrDefault();

    public async Task<AdminCourseSectionDto?> SaveAsync(long? id, SaveAdminCourseSectionRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            IF @Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CourseSections WITH (UPDLOCK, HOLDLOCK) WHERE SectionId = @Id)
                THROW 50004, 'Record not found.', 1;
            IF @Id IS NOT NULL AND @MaxStudents IS NOT NULL AND @MaxStudents < (SELECT COUNT(*) FROM dbo.Enrollments WITH (UPDLOCK, HOLDLOCK) WHERE SectionId = @Id AND Status <> 0) THROW 50001, 'Capacity cannot be less than the number of enrolled students.', 1;
            IF @Id IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.CourseSections WHERE SectionId = @Id AND (CourseId <> @CourseId OR SemesterId <> @SemesterId))
                AND (EXISTS (SELECT 1 FROM dbo.Enrollments WHERE SectionId = @Id) OR EXISTS (SELECT 1 FROM dbo.SectionTeachers WHERE SectionId = @Id))
                THROW 50001, 'Cannot change course or semester after assigning teachers or students.', 1;
            IF @Id IS NULL
            BEGIN
                INSERT INTO dbo.CourseSections (CourseId, SemesterId, SectionCode, SectionName, MaxStudents, Status)
                VALUES (@CourseId, @SemesterId, @SectionCode, @SectionName, @MaxStudents, @Status);
                SET @Id = SCOPE_IDENTITY();
            END
            ELSE
                UPDATE dbo.CourseSections SET CourseId = @CourseId, SemesterId = @SemesterId, SectionCode = @SectionCode, SectionName = @SectionName, MaxStudents = @MaxStudents, Status = @Status WHERE SectionId = @Id;
            """;
        return (await db.QueryAsync(sql + SelectSql + " WHERE SectionId = @Id;", p =>
        {
            AdminSql.Add(p, "@Id", SqlDbType.BigInt, id);
        AdminSql.Add(p, "@CourseId", SqlDbType.Int, request.CourseId);
        AdminSql.Add(p, "@SemesterId", SqlDbType.Int, request.SemesterId);
        AdminSql.Add(p, "@SectionCode", SqlDbType.VarChar, request.SectionCode.Trim(), 40);
        AdminSql.Add(p, "@SectionName", SqlDbType.NVarChar, request.SectionName, 200);
        AdminSql.Add(p, "@MaxStudents", SqlDbType.Int, request.MaxStudents);
        AdminSql.Add(p, "@Status", SqlDbType.TinyInt, request.Status);
        }, Read, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken) =>
        db.ExecuteAsync("DELETE FROM dbo.CourseSections WHERE SectionId = @Id;",
            p => AdminSql.Add(p, "@Id", SqlDbType.BigInt, id), cancellationToken);

    private static AdminCourseSectionDto Read(SqlDataReader r) => new(
        r.GetFieldValue<long>(r.GetOrdinal("SectionId")),
        r.GetFieldValue<int>(r.GetOrdinal("CourseId")),
        r.GetFieldValue<int>(r.GetOrdinal("SemesterId")),
        r.GetFieldValue<string>(r.GetOrdinal("SectionCode")),
        AdminSql.Text(r, "SectionName"),
        AdminSql.Nullable<int>(r, "MaxStudents"),
        r.GetFieldValue<byte>(r.GetOrdinal("Status")));
}
