using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

public sealed class AdminSectionManagementRepository(IDbConnectionFactory factory) : IAdminSectionManagementRepository
{
    private readonly AdminSql db = new(factory);
    private const string SectionGuard = """
        IF NOT EXISTS (SELECT 1 FROM dbo.CourseSections WITH (UPDLOCK, HOLDLOCK) WHERE SectionId = @SectionId)
            THROW 50004, 'Section not found.', 1;
        """;
    private const string TeacherSelect = """
        SELECT st.SectionId, st.TeacherId, t.TeacherCode, u.FullName, st.IsPrimary, st.AssignedAt
        FROM dbo.SectionTeachers st JOIN dbo.Teachers t ON t.TeacherId = st.TeacherId
        JOIN dbo.Users u ON u.UserId = t.UserId WHERE st.SectionId = @SectionId
        """;
    private const string StudentSelect = """
        SELECT e.EnrollmentId, e.SectionId, e.StudentId, s.StudentCode, u.FullName, e.Status, e.EnrolledAt
        FROM dbo.Enrollments e JOIN dbo.Students s ON s.StudentId = e.StudentId
        JOIN dbo.Users u ON u.UserId = s.UserId WHERE e.SectionId = @SectionId
        """;

    public Task<PagedResult<AdminSectionTeacherDto>> TeachersAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken) =>
        PageAsync(sectionId, query, "SELECT COUNT(*) FROM dbo.SectionTeachers WHERE SectionId = @SectionId;",
            TeacherSelect + " ORDER BY st.TeacherId", ReadTeacher, cancellationToken);

    public Task<PagedResult<AdminEnrollmentDto>> StudentsAsync(long sectionId, AdminPageQuery query, CancellationToken cancellationToken) =>
        PageAsync(sectionId, query, "SELECT COUNT(*) FROM dbo.Enrollments WHERE SectionId = @SectionId;",
            StudentSelect + " ORDER BY e.EnrollmentId", ReadStudent, cancellationToken);

    private async Task<PagedResult<T>> PageAsync<T>(long sectionId, AdminPageQuery query, string countSql,
        string selectSql, Func<SqlDataReader, T> read, CancellationToken cancellationToken)
    {
        void Parameters(SqlParameterCollection p)
        {
            AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId);
            AdminSql.Add(p, "@Offset", SqlDbType.Int, (query.Page - 1) * query.PageSize);
            AdminSql.Add(p, "@PageSize", SqlDbType.Int, query.PageSize);
        }
        var count = (await db.QueryAsync(SectionGuard + countSql, Parameters, r => r.GetInt32(0), cancellationToken)).Single();
        var rows = await db.QueryAsync(selectSql + " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;", Parameters, read, cancellationToken);
        return new(rows, query.Page, query.PageSize, count, (int)Math.Ceiling(count / (double)query.PageSize));
    }

    public async Task<AdminSectionTeacherDto?> AssignTeacherAsync(long sectionId, long teacherId, bool primary, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SectionGuard + """
            IF NOT EXISTS (SELECT 1 FROM dbo.Teachers t JOIN dbo.Users u ON u.UserId = t.UserId
                JOIN dbo.Roles r ON r.RoleId = u.RoleId
                WHERE t.TeacherId = @TeacherId AND t.Status = 1 AND u.IsActive = 1 AND r.RoleCode = 'TEACHER')
                THROW 50001, 'Teacher must be active and have the TEACHER role.', 1;
            IF @IsPrimary = 1 UPDATE dbo.SectionTeachers SET IsPrimary = 0 WHERE SectionId = @SectionId;
            IF EXISTS (SELECT 1 FROM dbo.SectionTeachers WHERE SectionId = @SectionId AND TeacherId = @TeacherId)
                UPDATE dbo.SectionTeachers SET IsPrimary = @IsPrimary WHERE SectionId = @SectionId AND TeacherId = @TeacherId;
            ELSE
                INSERT INTO dbo.SectionTeachers (SectionId, TeacherId, IsPrimary) VALUES (@SectionId, @TeacherId, @IsPrimary);
            """ + TeacherSelect + " AND st.TeacherId = @TeacherId;", p =>
        {
            Pair(p, sectionId, "@TeacherId", teacherId);
            AdminSql.Add(p, "@IsPrimary", SqlDbType.Bit, primary);
        }, ReadTeacher, cancellationToken, transaction: true)).SingleOrDefault();

    public Task<bool> RemoveTeacherAsync(long sectionId, long teacherId, CancellationToken cancellationToken) =>
        db.ExecuteAsync(SectionGuard + "DELETE FROM dbo.SectionTeachers WHERE SectionId = @SectionId AND TeacherId = @TeacherId;",
            p => Pair(p, sectionId, "@TeacherId", teacherId), cancellationToken);

    public async Task<AdminEnrollmentDto?> EnrollAsync(long sectionId, long studentId, byte status, CancellationToken cancellationToken) =>
        (await db.QueryAsync(SectionGuard + """
            IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentId = @StudentId)
                THROW 50004, 'Student not found.', 1;
            IF @Status = 1
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM dbo.CourseSections WHERE SectionId = @SectionId AND Status = 1)
                    THROW 50001, 'Section must be open for enrollment.', 1;
                IF NOT EXISTS (SELECT 1 FROM dbo.Students s JOIN dbo.Users u ON u.UserId = s.UserId
                    JOIN dbo.Roles r ON r.RoleId = u.RoleId
                    WHERE s.StudentId = @StudentId AND s.Status = 1 AND u.IsActive = 1 AND r.RoleCode = 'STUDENT')
                    THROW 50001, 'Student must be active and have the STUDENT role.', 1;
            END;
            IF @Status <> 0 AND NOT EXISTS
                (SELECT 1 FROM dbo.Enrollments WHERE SectionId = @SectionId AND StudentId = @StudentId AND Status <> 0)
                AND (SELECT MaxStudents FROM dbo.CourseSections WHERE SectionId = @SectionId) <=
                    (SELECT COUNT(*) FROM dbo.Enrollments WHERE SectionId = @SectionId AND Status <> 0)
                THROW 50001, 'Section capacity has been reached.', 1;
            IF EXISTS (SELECT 1 FROM dbo.Enrollments WHERE SectionId = @SectionId AND StudentId = @StudentId)
                UPDATE dbo.Enrollments SET Status = @Status WHERE SectionId = @SectionId AND StudentId = @StudentId;
            ELSE
            BEGIN
                IF @Status <> 1 THROW 50001, 'New enrollment must have active status.', 1;
                INSERT INTO dbo.Enrollments (SectionId, StudentId, Status) VALUES (@SectionId, @StudentId, @Status);
            END;
            """ + StudentSelect + " AND e.StudentId = @StudentId;", p =>
        {
            Pair(p, sectionId, "@StudentId", studentId);
            AdminSql.Add(p, "@Status", SqlDbType.TinyInt, status);
        }, ReadStudent, cancellationToken, transaction: true)).SingleOrDefault();

    public Task<bool> CancelEnrollmentAsync(long sectionId, long studentId, CancellationToken cancellationToken) =>
        db.ExecuteAsync(SectionGuard + "UPDATE dbo.Enrollments SET Status = 0 WHERE SectionId = @SectionId AND StudentId = @StudentId;",
            p => Pair(p, sectionId, "@StudentId", studentId), cancellationToken);

    public async Task<AdminStatisticsDto> StatisticsAsync(CancellationToken cancellationToken) =>
        (await db.QueryAsync("""
            SELECT (SELECT COUNT(*) FROM dbo.Users), (SELECT COUNT(*) FROM dbo.Users WHERE IsActive = 1),
                (SELECT COUNT(*) FROM dbo.Students), (SELECT COUNT(*) FROM dbo.Students WHERE Status = 1),
                (SELECT COUNT(*) FROM dbo.Teachers), (SELECT COUNT(*) FROM dbo.Teachers WHERE Status = 1),
                (SELECT COUNT(*) FROM dbo.Courses), (SELECT COUNT(*) FROM dbo.CourseSections),
                (SELECT COUNT(*) FROM dbo.CourseSections WHERE Status = 1), (SELECT COUNT(*) FROM dbo.Semesters),
                (SELECT COUNT(*) FROM dbo.Enrollments WHERE Status = 1);
            """, _ => { }, r => new AdminStatisticsDto(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3),
                r.GetInt32(4), r.GetInt32(5), r.GetInt32(6), r.GetInt32(7), r.GetInt32(8), r.GetInt32(9), r.GetInt32(10)), cancellationToken)).Single();

    public async Task<AdminReportDto> ReportAsync(CancellationToken cancellationToken)
    {
        static AdminChartPointDto Point(SqlDataReader r) => new(r.GetString(0), r.GetInt32(1));
        var byDepartment = await db.QueryAsync("""
            SELECT d.DepartmentName, COUNT(s.StudentId)
            FROM dbo.Departments d
            LEFT JOIN dbo.Majors m ON m.DepartmentId = d.DepartmentId
            LEFT JOIN dbo.Students s ON s.MajorId = m.MajorId AND s.Status = 1
            GROUP BY d.DepartmentId, d.DepartmentName
            ORDER BY COUNT(s.StudentId) DESC, d.DepartmentName;
            """, _ => { }, Point, cancellationToken);
        var byMajor = await db.QueryAsync("""
            SELECT m.MajorName, COUNT(s.StudentId)
            FROM dbo.Majors m
            LEFT JOIN dbo.Students s ON s.MajorId = m.MajorId AND s.Status = 1
            GROUP BY m.MajorId, m.MajorName
            ORDER BY COUNT(s.StudentId) DESC, m.MajorName;
            """, _ => { }, Point, cancellationToken);
        var bySemester = await db.QueryAsync("""
            SELECT sm.SemesterName + N' ' + sm.AcademicYear, COUNT(DISTINCT cs.SectionId), COUNT(e.EnrollmentId)
            FROM dbo.Semesters sm
            LEFT JOIN dbo.CourseSections cs ON cs.SemesterId = sm.SemesterId
            LEFT JOIN dbo.Enrollments e ON e.SectionId = cs.SectionId AND e.Status <> 0
            GROUP BY sm.SemesterId, sm.SemesterName, sm.AcademicYear, sm.StartDate
            ORDER BY sm.StartDate;
            """, _ => { }, r => new AdminSemesterReportDto(r.GetString(0), r.GetInt32(1), r.GetInt32(2)), cancellationToken);
        var grades = await db.QueryAsync("""
            SELECT b.Label, COUNT(e.EnrollmentId)
            FROM (VALUES (1, N'0 – 3.9', 0.0, 4.0), (2, N'4 – 4.9', 4.0, 5.0), (3, N'5 – 6.4', 5.0, 6.5),
                         (4, N'6.5 – 7.9', 6.5, 8.0), (5, N'8 – 8.9', 8.0, 9.0), (6, N'9 – 10', 9.0, 10.01)) b(Ord, Label, Lo, Hi)
            LEFT JOIN dbo.Enrollments e ON e.Status <> 0 AND e.FinalScore10 >= b.Lo AND e.FinalScore10 < b.Hi
            GROUP BY b.Ord, b.Label
            ORDER BY b.Ord;
            """, _ => { }, Point, cancellationToken);
        var average = (await db.QueryAsync("""
            SELECT ISNULL(AVG(CAST(FinalScore10 AS FLOAT)), 0) FROM dbo.Enrollments
            WHERE Status <> 0 AND FinalScore10 IS NOT NULL;
            """, _ => { }, r => r.GetDouble(0), cancellationToken)).Single();
        var gradeList = grades.ToList();
        return new AdminReportDto(byDepartment.ToList(), byMajor.ToList(), bySemester.ToList(), gradeList,
            Math.Round(average, 2), gradeList.Sum(x => x.Value));
    }

    private static void Pair(SqlParameterCollection p, long sectionId, string name, long id)
    {
        AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId);
        AdminSql.Add(p, name, SqlDbType.BigInt, id);
    }
    private static AdminSectionTeacherDto ReadTeacher(SqlDataReader r) =>
        new(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3), r.GetBoolean(4), r.GetDateTime(5));
    private static AdminEnrollmentDto ReadStudent(SqlDataReader r) =>
        new(r.GetInt64(0), r.GetInt64(1), r.GetInt64(2), r.GetString(3), r.GetString(4), r.GetByte(5), r.GetDateTime(6));
}
