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
    private const string ScheduleSelect = """
        SELECT ScheduleId, SectionId, DayOfWeek,
               CONVERT(VARCHAR(8), StartTime, 108) AS StartTime,
               CONVERT(VARCHAR(8), EndTime, 108) AS EndTime,
               Room, Building, EffectiveFrom, EffectiveTo, Note
        FROM dbo.ClassSchedules
        """;
    private const string ExamSelect = """
        SELECT ExamId, SectionId, CreatedByUserId, ExamName, ExamType, ExamDate,
               CONVERT(VARCHAR(8), StartTime, 108) AS StartTime,
               DurationMinutes, Room, Note, CreatedAt
        FROM dbo.Exams
        """;
    private const string ScheduleOverlapGuard = """
        IF EXISTS (
            SELECT 1 FROM dbo.ClassSchedules WITH (UPDLOCK, HOLDLOCK)
            WHERE SectionId = @SectionId
              AND DayOfWeek = @DayOfWeek
              AND EffectiveFrom <= @EffectiveTo
              AND EffectiveTo >= @EffectiveFrom
              AND StartTime < @EndTime
              AND EndTime > @StartTime
              AND (@ScheduleId IS NULL OR ScheduleId <> @ScheduleId)
        ) THROW 50001, N'Lịch học này trùng thời gian với một buổi khác của cùng lớp.', 1;
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

    public async Task<IReadOnlyList<AdminClassScheduleDto>> SchedulesAsync(long sectionId, CancellationToken cancellationToken)
    {
        var rows = await db.QueryAsync(SectionGuard + ScheduleSelect + " WHERE SectionId = @SectionId ORDER BY DayOfWeek, StartTime;",
            p => AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId), ReadSchedule, cancellationToken);
        return rows;
    }

    public async Task<AdminClassScheduleDto?> SaveScheduleAsync(long sectionId, long? scheduleId,
        SaveAdminClassScheduleRequest request, CancellationToken cancellationToken)
    {
        var sql = SectionGuard + """
            IF @ScheduleId IS NOT NULL AND NOT EXISTS (
                SELECT 1 FROM dbo.ClassSchedules WITH (UPDLOCK, HOLDLOCK)
                WHERE SectionId = @SectionId AND ScheduleId = @ScheduleId)
                THROW 50004, 'Schedule not found.', 1;
            """ + ScheduleOverlapGuard + """
            IF @ScheduleId IS NULL
            BEGIN
                INSERT INTO dbo.ClassSchedules
                    (SectionId, DayOfWeek, StartTime, EndTime, Room, Building, EffectiveFrom, EffectiveTo, Note)
                VALUES
                    (@SectionId, @DayOfWeek, @StartTime, @EndTime, @Room, @Building, @EffectiveFrom, @EffectiveTo, @Note);
                SET @ScheduleId = CONVERT(BIGINT, SCOPE_IDENTITY());
            END
            ELSE
                UPDATE dbo.ClassSchedules
                SET DayOfWeek = @DayOfWeek, StartTime = @StartTime, EndTime = @EndTime,
                    Room = @Room, Building = @Building, EffectiveFrom = @EffectiveFrom,
                    EffectiveTo = @EffectiveTo, Note = @Note
                WHERE SectionId = @SectionId AND ScheduleId = @ScheduleId;
            """ + ScheduleSelect + " WHERE SectionId = @SectionId AND ScheduleId = @ScheduleId;";

        return (await db.QueryAsync(sql, p => AddScheduleParameters(p, sectionId, scheduleId, request),
            ReadSchedule, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteScheduleAsync(long sectionId, long scheduleId, CancellationToken cancellationToken) =>
        db.ExecuteAsync(SectionGuard + "DELETE FROM dbo.ClassSchedules WHERE SectionId = @SectionId AND ScheduleId = @ScheduleId;",
            p =>
            {
                AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId);
                AdminSql.Add(p, "@ScheduleId", SqlDbType.BigInt, scheduleId);
            }, cancellationToken);

    public async Task<IReadOnlyList<AdminExamDto>> ExamsAsync(long sectionId, CancellationToken cancellationToken)
    {
        var rows = await db.QueryAsync(SectionGuard + ExamSelect + " WHERE SectionId = @SectionId ORDER BY ExamDate, StartTime;",
            p => AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId), ReadExam, cancellationToken);
        return rows;
    }

    public async Task<AdminExamDto?> SaveExamAsync(long sectionId, long? examId, long createdByUserId,
        SaveAdminExamRequest request, CancellationToken cancellationToken)
    {
        var sql = SectionGuard + """
            IF @ExamId IS NOT NULL AND NOT EXISTS (
                SELECT 1 FROM dbo.Exams WITH (UPDLOCK, HOLDLOCK)
                WHERE SectionId = @SectionId AND ExamId = @ExamId)
                THROW 50004, 'Exam not found.', 1;
            IF NOT EXISTS (
                SELECT 1 FROM dbo.Users u
                INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
                WHERE u.UserId = @CreatedByUserId AND u.IsActive = 1 AND r.RoleCode = 'ADMIN')
                THROW 50001, 'CreatedByUserId must be an active ADMIN user.', 1;
            IF @ExamId IS NULL
            BEGIN
                INSERT INTO dbo.Exams
                    (SectionId, CreatedByUserId, ExamName, ExamType, ExamDate, StartTime, DurationMinutes, Room, Note)
                VALUES
                    (@SectionId, @CreatedByUserId, @ExamName, @ExamType, @ExamDate, @StartTime, @DurationMinutes, @Room, @Note);
                SET @ExamId = CONVERT(BIGINT, SCOPE_IDENTITY());
            END
            ELSE
                UPDATE dbo.Exams
                SET ExamName = @ExamName, ExamType = @ExamType, ExamDate = @ExamDate,
                    StartTime = @StartTime, DurationMinutes = @DurationMinutes,
                    Room = @Room, Note = @Note
                WHERE SectionId = @SectionId AND ExamId = @ExamId;
            """ + ExamSelect + " WHERE SectionId = @SectionId AND ExamId = @ExamId;";

        return (await db.QueryAsync(sql, p => AddExamParameters(p, sectionId, examId, createdByUserId, request),
            ReadExam, cancellationToken, transaction: true)).SingleOrDefault();
    }

    public Task<bool> DeleteExamAsync(long sectionId, long examId, CancellationToken cancellationToken) =>
        db.ExecuteAsync(SectionGuard + "DELETE FROM dbo.Exams WHERE SectionId = @SectionId AND ExamId = @ExamId;",
            p =>
            {
                AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId);
                AdminSql.Add(p, "@ExamId", SqlDbType.BigInt, examId);
            }, cancellationToken);

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

    private static void AddScheduleParameters(SqlParameterCollection p, long sectionId, long? scheduleId,
        SaveAdminClassScheduleRequest request)
    {
        AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId);
        AdminSql.Add(p, "@ScheduleId", SqlDbType.BigInt, scheduleId);
        AdminSql.Add(p, "@DayOfWeek", SqlDbType.TinyInt, request.DayOfWeek);
        AdminSql.Add(p, "@StartTime", SqlDbType.Time, request.StartTime.ToTimeSpan());
        AdminSql.Add(p, "@EndTime", SqlDbType.Time, request.EndTime.ToTimeSpan());
        AdminSql.Add(p, "@Room", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(request.Room) ? null : request.Room.Trim());
        AdminSql.Add(p, "@Building", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(request.Building) ? null : request.Building.Trim());
        AdminSql.Add(p, "@EffectiveFrom", SqlDbType.Date, request.EffectiveFrom);
        AdminSql.Add(p, "@EffectiveTo", SqlDbType.Date, request.EffectiveTo);
        AdminSql.Add(p, "@Note", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim());
    }

    private static void AddExamParameters(SqlParameterCollection p, long sectionId, long? examId,
        long createdByUserId, SaveAdminExamRequest request)
    {
        AdminSql.Add(p, "@SectionId", SqlDbType.BigInt, sectionId);
        AdminSql.Add(p, "@ExamId", SqlDbType.BigInt, examId);
        AdminSql.Add(p, "@CreatedByUserId", SqlDbType.BigInt, createdByUserId);
        AdminSql.Add(p, "@ExamName", SqlDbType.NVarChar, request.ExamName.Trim(), 200);
        AdminSql.Add(p, "@ExamType", SqlDbType.TinyInt, request.ExamType);
        AdminSql.Add(p, "@ExamDate", SqlDbType.Date, request.ExamDate);
        AdminSql.Add(p, "@StartTime", SqlDbType.Time, request.StartTime.ToTimeSpan());
        AdminSql.Add(p, "@DurationMinutes", SqlDbType.SmallInt, request.DurationMinutes);
        AdminSql.Add(p, "@Room", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(request.Room) ? null : request.Room.Trim(), 50);
        AdminSql.Add(p, "@Note", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(), 500);
    }

    private static AdminClassScheduleDto ReadSchedule(SqlDataReader r) => new(
        r.GetInt64(r.GetOrdinal("ScheduleId")),
        r.GetInt64(r.GetOrdinal("SectionId")),
        r.GetByte(r.GetOrdinal("DayOfWeek")),
        r.GetString(r.GetOrdinal("StartTime")),
        r.GetString(r.GetOrdinal("EndTime")),
        AdminSql.Text(r, "Room"),
        AdminSql.Text(r, "Building"),
        DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("EffectiveFrom"))),
        DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("EffectiveTo"))),
        AdminSql.Text(r, "Note"));

    private static AdminExamDto ReadExam(SqlDataReader r) => new(
        r.GetInt64(r.GetOrdinal("ExamId")),
        r.GetInt64(r.GetOrdinal("SectionId")),
        r.GetInt64(r.GetOrdinal("CreatedByUserId")),
        r.GetString(r.GetOrdinal("ExamName")),
        r.GetByte(r.GetOrdinal("ExamType")),
        DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("ExamDate"))),
        r.GetString(r.GetOrdinal("StartTime")),
        r.GetInt16(r.GetOrdinal("DurationMinutes")),
        AdminSql.Text(r, "Room"),
        AdminSql.Text(r, "Note"),
        r.GetDateTime(r.GetOrdinal("CreatedAt")));
}
