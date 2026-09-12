using System.Data;
using System.Globalization;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Contracts.Students;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class StudentLearningRepository(IDbConnectionFactory connectionFactory) : ControllerBase, IStudentLearningRepository
{
    private const int MaxPageSize = 100;

    public async Task<ActionResult<StudentProfileDto>> GetProfile(
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var profile = await FindStudentProfileAsync(connection, studentId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    public async Task<ActionResult<StudentProfileDto>> UpdateProfile(
        long studentId,
        UpdateStudentProfileRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateStudentProfileRequest(request);
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var userId = await GetStudentUserIdAsync(connection, studentId, cancellationToken);
        if (userId is null)
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Users
            SET FullName = @FullName,
                Phone = @Phone,
                DateOfBirth = @DateOfBirth,
                Gender = @Gender,
                AvatarUrl = @AvatarUrl,
                UpdatedAt = SYSDATETIME()
            WHERE UserId = @UserId;
            """;
        AddParameter(command, "@UserId", SqlDbType.BigInt, userId.Value);
        AddParameter(command, "@FullName", SqlDbType.NVarChar, request.FullName.Trim(), 150);
        AddParameter(command, "@Phone", SqlDbType.VarChar, NormalizeOptionalText(request.Phone), 20);
        AddParameter(command, "@DateOfBirth", SqlDbType.Date, request.DateOfBirth);
        AddParameter(command, "@Gender", SqlDbType.TinyInt, request.Gender);
        AddParameter(command, "@AvatarUrl", SqlDbType.NVarChar, NormalizeOptionalText(request.AvatarUrl), 1000);

        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            return NotFound();
        }

        var profile = await FindStudentProfileAsync(connection, studentId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    public async Task<ActionResult<StudentDashboardDto>> GetDashboard(
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_GetStudentDashboard";
        command.CommandType = CommandType.StoredProcedure;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        StudentDashboardProfileDto? profile = null;
        if (await reader.ReadAsync(cancellationToken))
        {
            profile = new StudentDashboardProfileDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetString(reader.GetOrdinal("StudentCode")),
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.GetString(reader.GetOrdinal("MajorName")),
                GetNullableString(reader, "ClassName"),
                GetNullableDecimal(reader, "GPA"));
        }

        if (profile is null)
        {
            return NotFound();
        }

        var deadlines = new List<StudentAssignmentDeadlineDto>();
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                deadlines.Add(new StudentAssignmentDeadlineDto(
                    reader.GetInt64(reader.GetOrdinal("StudentId")),
                    reader.GetInt64(reader.GetOrdinal("SectionId")),
                    reader.GetString(reader.GetOrdinal("CourseCode")),
                    reader.GetString(reader.GetOrdinal("CourseName")),
                    reader.GetInt64(reader.GetOrdinal("AssignmentId")),
                    reader.GetString(reader.GetOrdinal("Title")),
                    reader.GetDateTime(reader.GetOrdinal("DueAt")),
                    reader.GetDecimal(reader.GetOrdinal("MaxScore")),
                    reader.GetString(reader.GetOrdinal("SubmissionStatus")),
                    GetNullableDateTime(reader, "SubmittedAt"),
                    GetNullableDecimal(reader, "Score")));
            }
        }

        var exams = new List<StudentExamDto>();
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                exams.Add(new StudentExamDto(
                    reader.GetInt64(reader.GetOrdinal("StudentId")),
                    reader.GetString(reader.GetOrdinal("CourseCode")),
                    reader.GetString(reader.GetOrdinal("CourseName")),
                    reader.GetInt64(reader.GetOrdinal("ExamId")),
                    reader.GetString(reader.GetOrdinal("ExamName")),
                    reader.GetByte(reader.GetOrdinal("ExamType")),
                    GetDateOnly(reader, "ExamDate"),
                    GetTimeString(reader, "StartTime"),
                    reader.GetInt16(reader.GetOrdinal("DurationMinutes")),
                    GetNullableString(reader, "Room")));
            }
        }

        return Ok(new StudentDashboardDto(profile, deadlines, exams));
    }

    public async Task<ActionResult<IReadOnlyList<StudentSectionDto>>> GetSections(
        long studentId,
        [FromQuery] int? semesterId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
    {
        if (status is not null && !IsEnrollmentStatus(status.Value))
        {
            return BadRequest(ValidationError("status", "Status must be 0, 1, or 2."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                StudentId,
                EnrollmentId,
                SectionId,
                SectionCode,
                SectionName,
                CourseId,
                CourseCode,
                CourseName,
                Credits,
                SemesterId,
                SemesterCode,
                SemesterName,
                AcademicYear,
                EnrollmentStatus,
                FinalScore10,
                LetterGrade
            FROM dbo.vw_StudentSections
            WHERE StudentId = @StudentId
              AND (@SemesterId IS NULL OR SemesterId = @SemesterId)
              AND (@Status IS NULL OR EnrollmentStatus = @Status)
            ORDER BY AcademicYear DESC, SemesterCode DESC, CourseCode, SectionCode;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SemesterId", SqlDbType.Int, semesterId);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);

        var sections = new List<StudentSectionDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sections.Add(new StudentSectionDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetInt64(reader.GetOrdinal("EnrollmentId")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("SectionCode")),
                GetNullableString(reader, "SectionName"),
                reader.GetInt32(reader.GetOrdinal("CourseId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetByte(reader.GetOrdinal("Credits")),
                reader.GetInt32(reader.GetOrdinal("SemesterId")),
                reader.GetString(reader.GetOrdinal("SemesterCode")),
                reader.GetString(reader.GetOrdinal("SemesterName")),
                reader.GetString(reader.GetOrdinal("AcademicYear")),
                reader.GetByte(reader.GetOrdinal("EnrollmentStatus")),
                GetNullableDecimal(reader, "FinalScore10"),
                GetNullableString(reader, "LetterGrade")));
        }

        return Ok(sections);
    }

    public async Task<ActionResult<IReadOnlyList<StudentScheduleDto>>> GetSchedule(
        long studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
    {
        if (from is not null && to is not null && to < from)
        {
            return BadRequest(ValidationError("to", "To date must be greater than or equal to from date."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                e.StudentId,
                cs.SectionId,
                cs.SectionCode,
                c.CourseCode,
                c.CourseName,
                sch.ScheduleId,
                sch.DayOfWeek,
                sch.StartTime,
                sch.EndTime,
                sch.Room,
                sch.Building,
                sch.EffectiveFrom,
                sch.EffectiveTo,
                sch.Note
            FROM dbo.Enrollments e
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = e.SectionId
            INNER JOIN dbo.Courses c ON c.CourseId = cs.CourseId
            INNER JOIN dbo.ClassSchedules sch ON sch.SectionId = cs.SectionId
            WHERE e.StudentId = @StudentId
              AND e.Status = 1
              AND (@SectionId IS NULL OR cs.SectionId = @SectionId)
              AND (@FromDate IS NULL OR sch.EffectiveTo >= @FromDate)
              AND (@ToDate IS NULL OR sch.EffectiveFrom <= @ToDate)
            ORDER BY sch.DayOfWeek, sch.StartTime, c.CourseCode, cs.SectionCode;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(command, "@FromDate", SqlDbType.Date, from);
        AddParameter(command, "@ToDate", SqlDbType.Date, to);

        var schedules = new List<StudentScheduleDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            schedules.Add(new StudentScheduleDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("SectionCode")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetInt64(reader.GetOrdinal("ScheduleId")),
                reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                GetTimeString(reader, "StartTime"),
                GetTimeString(reader, "EndTime"),
                GetNullableString(reader, "Room"),
                GetNullableString(reader, "Building"),
                GetDateOnly(reader, "EffectiveFrom"),
                GetDateOnly(reader, "EffectiveTo"),
                GetNullableString(reader, "Note")));
        }

        return Ok(schedules);
    }

    public async Task<ActionResult<IReadOnlyList<StudentExamScheduleDto>>> GetExams(
        long studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? sectionId,
        [FromQuery] byte? examType,
        CancellationToken cancellationToken)
    {
        if (from is not null && to is not null && to < from)
        {
            return BadRequest(ValidationError("to", "To date must be greater than or equal to from date."));
        }

        if (examType is not null && !IsExamType(examType.Value))
        {
            return BadRequest(ValidationError("examType", "ExamType must be 1, 2, 3, or 4."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                e.StudentId,
                cs.SectionId,
                cs.SectionCode,
                c.CourseCode,
                c.CourseName,
                ex.ExamId,
                ex.ExamName,
                ex.ExamType,
                ex.ExamDate,
                ex.StartTime,
                ex.DurationMinutes,
                ex.Room,
                ex.Note,
                ex.CreatedAt
            FROM dbo.Enrollments e
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = e.SectionId
            INNER JOIN dbo.Courses c ON c.CourseId = cs.CourseId
            INNER JOIN dbo.Exams ex ON ex.SectionId = cs.SectionId
            WHERE e.StudentId = @StudentId
              AND e.Status = 1
              AND (@SectionId IS NULL OR cs.SectionId = @SectionId)
              AND (@ExamType IS NULL OR ex.ExamType = @ExamType)
              AND (@FromDate IS NULL OR ex.ExamDate >= @FromDate)
              AND (@ToDate IS NULL OR ex.ExamDate <= @ToDate)
            ORDER BY ex.ExamDate, ex.StartTime, c.CourseCode, cs.SectionCode;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(command, "@ExamType", SqlDbType.TinyInt, examType);
        AddParameter(command, "@FromDate", SqlDbType.Date, from);
        AddParameter(command, "@ToDate", SqlDbType.Date, to);

        var exams = new List<StudentExamScheduleDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            exams.Add(ReadStudentExamSchedule(reader));
        }

        return Ok(exams);
    }

    public async Task<ActionResult<IReadOnlyList<StudentAssignmentDto>>> GetAssignments(
        long studentId,
        [FromQuery] long? sectionId,
        [FromQuery] byte? submissionStatus,
        [FromQuery] DateTime? fromDueAt,
        [FromQuery] DateTime? toDueAt,
        CancellationToken cancellationToken)
    {
        if (submissionStatus is not null && submissionStatus is > 4)
        {
            return BadRequest(ValidationError("submissionStatus", "SubmissionStatus must be 0, 1, 2, 3, or 4."));
        }

        if (fromDueAt is not null && toDueAt is not null && toDueAt < fromDueAt)
        {
            return BadRequest(ValidationError("toDueAt", "To due date must be greater than or equal to from due date."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentAssignmentSelectSql}
            WHERE e.StudentId = @StudentId
              AND e.Status = 1
              AND a.IsPublished = 1
              AND (@SectionId IS NULL OR cs.SectionId = @SectionId)
              AND (@FromDueAt IS NULL OR a.DueAt >= @FromDueAt)
              AND (@ToDueAt IS NULL OR a.DueAt <= @ToDueAt)
              AND
              (
                  @SubmissionStatus IS NULL
                  OR (@SubmissionStatus = 0 AND sub.SubmissionId IS NULL AND SYSDATETIME() <= a.DueAt)
                  OR (@SubmissionStatus = 4 AND sub.SubmissionId IS NULL AND SYSDATETIME() > a.DueAt)
                  OR (@SubmissionStatus IN (1, 2, 3) AND sub.Status = @SubmissionStatus)
              )
            ORDER BY a.DueAt, c.CourseCode, a.Title;
            """;
        AddAssignmentFilterParameters(command, studentId, sectionId, submissionStatus, fromDueAt, toDueAt);

        var assignments = new List<StudentAssignmentDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            assignments.Add(ReadStudentAssignment(reader));
        }

        return Ok(assignments);
    }

    public async Task<ActionResult<StudentAssignmentDto>> GetAssignment(
        long studentId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentAssignmentSelectSql}
            WHERE e.StudentId = @StudentId
              AND e.Status = 1
              AND a.IsPublished = 1
              AND a.AssignmentId = @AssignmentId;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@AssignmentId", SqlDbType.BigInt, assignmentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Ok(ReadStudentAssignment(reader)) : NotFound();
    }

    public async Task<ActionResult<AssignmentSubmissionDto>> SubmitAssignment(
        long studentId,
        long assignmentId,
        SubmitAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateSubmitAssignmentRequest(request);
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var availability = await GetAssignmentAvailabilityAsync(connection, studentId, assignmentId, cancellationToken);
        if (availability is null)
        {
            return NotFound();
        }

        if (availability.OpenAt is not null && availability.OpenAt > availability.CurrentTime)
        {
            return BadRequest(new { message = "Assignment is not open yet." });
        }

        if (!availability.AllowLateSubmission && availability.DueAt < availability.CurrentTime)
        {
            return BadRequest(new { message = "Assignment is overdue and does not allow late submissions." });
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @Now DATETIME2(0) = SYSDATETIME();
            DECLARE @IsLate BIT =
                CASE WHEN @Now > @DueAt THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.AssignmentSubmissions
                WHERE AssignmentId = @AssignmentId
                  AND StudentId = @StudentId
            )
            BEGIN
                UPDATE dbo.AssignmentSubmissions
                SET TextContent = @TextContent,
                    FileUrl = @FileUrl,
                    SubmittedAt = @Now,
                    IsLate = @IsLate,
                    Status = 1,
                    Score = NULL,
                    Feedback = NULL,
                    GradedByUserId = NULL,
                    GradedAt = NULL
                WHERE AssignmentId = @AssignmentId
                  AND StudentId = @StudentId;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.AssignmentSubmissions
                    (AssignmentId, StudentId, TextContent, FileUrl, SubmittedAt, IsLate, Status)
                VALUES
                    (@AssignmentId, @StudentId, @TextContent, @FileUrl, @Now, @IsLate, 1);
            END;

            SELECT
                SubmissionId,
                AssignmentId,
                StudentId,
                TextContent,
                FileUrl,
                SubmittedAt,
                IsLate,
                Status,
                Score,
                Feedback,
                GradedByUserId,
                GradedAt
            FROM dbo.AssignmentSubmissions
            WHERE AssignmentId = @AssignmentId
              AND StudentId = @StudentId;
            """;
        AddParameter(command, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@TextContent", SqlDbType.NVarChar, NormalizeOptionalText(request.TextContent), -1);
        AddParameter(command, "@FileUrl", SqlDbType.NVarChar, NormalizeOptionalText(request.FileUrl), 1000);
        AddParameter(command, "@DueAt", SqlDbType.DateTime2, availability.DueAt);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new ObjectResult(new { message = "Could not save assignment submission." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        var submission = ReadAssignmentSubmission(reader);
        return Ok(submission);
    }

    public async Task<ActionResult<IReadOnlyList<StudentSubmissionDto>>> GetSubmissions(
        long studentId,
        [FromQuery] long? assignmentId,
        [FromQuery] long? sectionId,
        [FromQuery] byte? status,
        CancellationToken cancellationToken)
    {
        if (status is not null && !IsSubmissionStatus(status.Value))
        {
            return BadRequest(ValidationError("status", "Status must be 1, 2, or 3."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                sub.SubmissionId,
                sub.AssignmentId,
                a.Title AS AssignmentTitle,
                cs.SectionId,
                c.CourseCode,
                c.CourseName,
                a.DueAt,
                sub.TextContent,
                sub.FileUrl,
                sub.SubmittedAt,
                sub.IsLate,
                sub.Status,
                sub.Score,
                sub.Feedback,
                sub.GradedAt
            FROM dbo.AssignmentSubmissions sub
            INNER JOIN dbo.Assignments a ON a.AssignmentId = sub.AssignmentId
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = a.SectionId
            INNER JOIN dbo.Courses c ON c.CourseId = cs.CourseId
            INNER JOIN dbo.Enrollments e
                ON e.SectionId = cs.SectionId
               AND e.StudentId = sub.StudentId
            WHERE sub.StudentId = @StudentId
              AND e.Status = 1
              AND (@AssignmentId IS NULL OR sub.AssignmentId = @AssignmentId)
              AND (@SectionId IS NULL OR cs.SectionId = @SectionId)
              AND (@Status IS NULL OR sub.Status = @Status)
            ORDER BY sub.SubmittedAt DESC, c.CourseCode, a.Title;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@AssignmentId", SqlDbType.BigInt, assignmentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);

        var submissions = new List<StudentSubmissionDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            submissions.Add(new StudentSubmissionDto(
                reader.GetInt64(reader.GetOrdinal("SubmissionId")),
                reader.GetInt64(reader.GetOrdinal("AssignmentId")),
                reader.GetString(reader.GetOrdinal("AssignmentTitle")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetDateTime(reader.GetOrdinal("DueAt")),
                GetNullableString(reader, "TextContent"),
                GetNullableString(reader, "FileUrl"),
                reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                reader.GetBoolean(reader.GetOrdinal("IsLate")),
                reader.GetByte(reader.GetOrdinal("Status")),
                GetNullableDecimal(reader, "Score"),
                GetNullableString(reader, "Feedback"),
                GetNullableDateTime(reader, "GradedAt")));
        }

        return Ok(submissions);
    }

    public async Task<ActionResult<IReadOnlyList<StudentMaterialDto>>> GetMaterials(
        long studentId,
        [FromQuery] long? sectionId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                m.MaterialId,
                cs.SectionId,
                c.CourseCode,
                c.CourseName,
                m.UploadedByUserId,
                u.FullName AS UploadedByFullName,
                m.Title,
                m.Description,
                m.MaterialType,
                m.FileUrl,
                m.ExternalUrl,
                m.IsVisible,
                m.CreatedAt,
                m.UpdatedAt
            FROM dbo.Materials m
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = m.SectionId
            INNER JOIN dbo.Courses c ON c.CourseId = cs.CourseId
            INNER JOIN dbo.Users u ON u.UserId = m.UploadedByUserId
            INNER JOIN dbo.Enrollments e
                ON e.SectionId = cs.SectionId
               AND e.StudentId = @StudentId
               AND e.Status = 1
            WHERE m.IsVisible = 1
              AND (@SectionId IS NULL OR cs.SectionId = @SectionId)
              AND
              (
                  @Search IS NULL
                  OR m.Title LIKE @Search
                  OR m.Description LIKE @Search
                  OR c.CourseCode LIKE @Search
                  OR c.CourseName LIKE @Search
              )
            ORDER BY m.CreatedAt DESC, c.CourseCode, m.Title;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);
        AddSearchParameter(command, search);

        var materials = new List<StudentMaterialDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            materials.Add(new StudentMaterialDto(
                reader.GetInt64(reader.GetOrdinal("MaterialId")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetInt64(reader.GetOrdinal("UploadedByUserId")),
                reader.GetString(reader.GetOrdinal("UploadedByFullName")),
                reader.GetString(reader.GetOrdinal("Title")),
                GetNullableString(reader, "Description"),
                GetNullableString(reader, "MaterialType"),
                GetNullableString(reader, "FileUrl"),
                GetNullableString(reader, "ExternalUrl"),
                reader.GetBoolean(reader.GetOrdinal("IsVisible")),
                reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                GetNullableDateTime(reader, "UpdatedAt")));
        }

        return Ok(materials);
    }

    public async Task<ActionResult<IReadOnlyList<StudentGradeDto>>> GetGrades(
        long studentId,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                gc.GradeComponentId,
                cs.SectionId,
                c.CourseCode,
                c.CourseName,
                gc.ComponentName,
                gc.WeightPercent,
                gc.MaxScore,
                gc.DisplayOrder,
                sg.StudentGradeId,
                sg.Score,
                sg.Note,
                sg.GradedByUserId,
                sg.GradedAt,
                sg.UpdatedAt
            FROM dbo.Enrollments e
            INNER JOIN dbo.CourseSections cs ON cs.SectionId = e.SectionId
            INNER JOIN dbo.Courses c ON c.CourseId = cs.CourseId
            INNER JOIN dbo.GradeComponents gc ON gc.SectionId = cs.SectionId
            LEFT JOIN dbo.StudentGrades sg
                ON sg.GradeComponentId = gc.GradeComponentId
               AND sg.StudentId = e.StudentId
            WHERE e.StudentId = @StudentId
              AND e.Status IN (1, 2)
              AND (@SectionId IS NULL OR cs.SectionId = @SectionId)
            ORDER BY c.CourseCode, cs.SectionCode, gc.DisplayOrder, gc.ComponentName;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);

        var grades = new List<StudentGradeDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            grades.Add(new StudentGradeDto(
                reader.GetInt64(reader.GetOrdinal("GradeComponentId")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetString(reader.GetOrdinal("ComponentName")),
                reader.GetDecimal(reader.GetOrdinal("WeightPercent")),
                reader.GetDecimal(reader.GetOrdinal("MaxScore")),
                reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                GetNullableLong(reader, "StudentGradeId"),
                GetNullableDecimal(reader, "Score"),
                GetNullableString(reader, "Note"),
                GetNullableLong(reader, "GradedByUserId"),
                GetNullableDateTime(reader, "GradedAt"),
                GetNullableDateTime(reader, "UpdatedAt")));
        }

        return Ok(grades);
    }

    public async Task<ActionResult<IReadOnlyList<StudentSectionScoreDto>>> GetSectionScores(
        long studentId,
        [FromQuery] long? sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                StudentId,
                SectionId,
                CourseCode,
                CourseName,
                Credits,
                WeightedScore10,
                dbo.fn_Score10ToGPA4(WeightedScore10) AS Gpa4
            FROM dbo.vw_StudentSectionScores
            WHERE StudentId = @StudentId
              AND (@SectionId IS NULL OR SectionId = @SectionId)
            ORDER BY CourseCode;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);

        var scores = new List<StudentSectionScoreDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            scores.Add(new StudentSectionScoreDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetInt64(reader.GetOrdinal("SectionId")),
                reader.GetString(reader.GetOrdinal("CourseCode")),
                reader.GetString(reader.GetOrdinal("CourseName")),
                reader.GetByte(reader.GetOrdinal("Credits")),
                reader.GetDecimal(reader.GetOrdinal("WeightedScore10")),
                GetNullableDecimal(reader, "Gpa4")));
        }

        return Ok(scores);
    }

    public async Task<ActionResult<StudentGpaDto>> GetGpa(
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT GPA
            FROM dbo.vw_StudentGPA
            WHERE StudentId = @StudentId;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var gpa = result is null or DBNull
            ? (decimal?)null
            : Convert.ToDecimal(result, CultureInfo.InvariantCulture);

        return Ok(new StudentGpaDto(studentId, gpa));
    }

    public async Task<ActionResult<IReadOnlyList<StudyGoalDto>>> GetStudyGoals(
        long studentId,
        [FromQuery] byte? status,
        [FromQuery] byte? goalType,
        CancellationToken cancellationToken)
    {
        if (status is not null && !IsStudyGoalStatus(status.Value))
        {
            return BadRequest(ValidationError("status", "Status must be 1, 2, or 3."));
        }

        if (goalType is not null && !IsStudyGoalType(goalType.Value))
        {
            return BadRequest(ValidationError("goalType", "GoalType must be between 1 and 5."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                GoalId,
                StudentId,
                Title,
                Description,
                GoalType,
                TargetValue,
                CurrentValue,
                StartDate,
                EndDate,
                Status,
                CreatedAt,
                UpdatedAt
            FROM dbo.StudyGoals
            WHERE StudentId = @StudentId
              AND (@Status IS NULL OR Status = @Status)
              AND (@GoalType IS NULL OR GoalType = @GoalType)
            ORDER BY Status, EndDate, StartDate DESC, GoalId DESC;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);
        AddParameter(command, "@GoalType", SqlDbType.TinyInt, goalType);

        var goals = new List<StudyGoalDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            goals.Add(ReadStudyGoal(reader));
        }

        return Ok(goals);
    }

    public async Task<ActionResult<StudyGoalDto>> GetStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        var goal = await FindStudyGoalAsync(studentId, goalId, cancellationToken);
        return goal is null ? NotFound() : Ok(goal);
    }

    public async Task<ActionResult<StudyGoalDto>> CreateStudyGoal(
        long studentId,
        CreateStudyGoalRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateStudyGoalRequest(
            request.Title,
            request.GoalType,
            request.StartDate,
            request.EndDate,
            request.Status);

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            if (!await StudentExistsAsync(connection, studentId, cancellationToken))
            {
                return NotFound();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO dbo.StudyGoals
                    (StudentId, Title, Description, GoalType, TargetValue, CurrentValue, StartDate, EndDate, Status)
                OUTPUT INSERTED.GoalId
                VALUES
                    (@StudentId, @Title, @Description, @GoalType, @TargetValue, @CurrentValue, @StartDate, @EndDate, @Status);
                """;
            AddStudyGoalWriteParameters(
                command,
                studentId,
                request.Title,
                request.Description,
                request.GoalType,
                request.TargetValue,
                request.CurrentValue,
                request.StartDate,
                request.EndDate,
                request.Status);

            var goalId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            var goal = await FindStudyGoalAsync(studentId, goalId, cancellationToken);

            return CreatedAtAction(nameof(GetStudyGoal), new { studentId, goalId }, goal);
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return BadRequest(new { message = "Related student was not found, or a check constraint failed." });
        }
    }

    public async Task<ActionResult<StudyGoalDto>> UpdateStudyGoal(
        long studentId,
        long goalId,
        UpdateStudyGoalRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateStudyGoalRequest(
            request.Title,
            request.GoalType,
            request.StartDate,
            request.EndDate,
            request.Status);

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE dbo.StudyGoals
                SET Title = @Title,
                    Description = @Description,
                    GoalType = @GoalType,
                    TargetValue = @TargetValue,
                    CurrentValue = @CurrentValue,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Status = @Status,
                    UpdatedAt = SYSDATETIME()
                WHERE GoalId = @GoalId
                  AND StudentId = @StudentId;
                """;
            AddParameter(command, "@GoalId", SqlDbType.BigInt, goalId);
            AddStudyGoalWriteParameters(
                command,
                studentId,
                request.Title,
                request.Description,
                request.GoalType,
                request.TargetValue,
                request.CurrentValue,
                request.StartDate,
                request.EndDate,
                request.Status);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound();
            }
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return BadRequest(new { message = "Related student was not found, or a check constraint failed." });
        }

        var goal = await FindStudyGoalAsync(studentId, goalId, cancellationToken);
        return goal is null ? NotFound() : Ok(goal);
    }

    public async Task<IActionResult> DeleteStudyGoal(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM dbo.StudyGoals
            WHERE GoalId = @GoalId
              AND StudentId = @StudentId;
            """;
        AddParameter(command, "@GoalId", SqlDbType.BigInt, goalId);
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows == 0 ? NotFound() : NoContent();
    }

    public async Task<ActionResult<IReadOnlyList<StudyTaskDto>>> GetStudyTasks(
        long studentId,
        [FromQuery] byte? status,
        [FromQuery] int? courseId,
        [FromQuery] DateTime? fromDueAt,
        [FromQuery] DateTime? toDueAt,
        CancellationToken cancellationToken)
    {
        if (status is not null && !IsStudyTaskStatus(status.Value))
        {
            return BadRequest(ValidationError("status", "Status must be 1, 2, 3, or 4."));
        }

        if (fromDueAt is not null && toDueAt is not null && toDueAt < fromDueAt)
        {
            return BadRequest(ValidationError("toDueAt", "To due date must be greater than or equal to from due date."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await StudentExistsAsync(connection, studentId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudyTaskSelectSql}
            WHERE st.StudentId = @StudentId
              AND (@Status IS NULL OR st.Status = @Status)
              AND (@CourseId IS NULL OR st.CourseId = @CourseId)
              AND (@FromDueAt IS NULL OR st.DueAt >= @FromDueAt)
              AND (@ToDueAt IS NULL OR st.DueAt <= @ToDueAt)
            ORDER BY
                CASE WHEN st.DueAt IS NULL THEN 1 ELSE 0 END,
                st.DueAt,
                st.Priority DESC,
                st.CreatedAt DESC;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);
        AddParameter(command, "@CourseId", SqlDbType.Int, courseId);
        AddParameter(command, "@FromDueAt", SqlDbType.DateTime2, fromDueAt);
        AddParameter(command, "@ToDueAt", SqlDbType.DateTime2, toDueAt);

        var tasks = new List<StudyTaskDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tasks.Add(ReadStudyTask(reader));
        }

        return Ok(tasks);
    }

    public async Task<ActionResult<StudyTaskDto>> GetStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        var task = await FindStudyTaskAsync(studentId, studyTaskId, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    public async Task<ActionResult<StudyTaskDto>> CreateStudyTask(
        long studentId,
        CreateStudyTaskRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateStudyTaskRequest(
            request.CourseId,
            request.Title,
            request.StartAt,
            request.DueAt,
            request.Priority,
            request.Status);

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            if (!await StudentExistsAsync(connection, studentId, cancellationToken))
            {
                return NotFound();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO dbo.StudyTasks
                    (StudentId, CourseId, Title, Description, StartAt, DueAt, ReminderAt, Priority, Status, CompletedAt)
                OUTPUT INSERTED.StudyTaskId
                VALUES
                    (
                        @StudentId,
                        @CourseId,
                        @Title,
                        @Description,
                        @StartAt,
                        @DueAt,
                        @ReminderAt,
                        @Priority,
                        @Status,
                        CASE WHEN @Status = 3 THEN SYSDATETIME() ELSE NULL END
                    );
                """;
            AddStudyTaskWriteParameters(
                command,
                studentId,
                request.CourseId,
                request.Title,
                request.Description,
                request.StartAt,
                request.DueAt,
                request.ReminderAt,
                request.Priority,
                request.Status);

            var studyTaskId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            var task = await FindStudyTaskAsync(studentId, studyTaskId, cancellationToken);

            return CreatedAtAction(nameof(GetStudyTask), new { studentId, studyTaskId }, task);
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return BadRequest(new { message = "Related student or course was not found, or a check constraint failed." });
        }
    }

    public async Task<ActionResult<StudyTaskDto>> UpdateStudyTask(
        long studentId,
        long studyTaskId,
        UpdateStudyTaskRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateStudyTaskRequest(
            request.CourseId,
            request.Title,
            request.StartAt,
            request.DueAt,
            request.Priority,
            request.Status);

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE dbo.StudyTasks
                SET CourseId = @CourseId,
                    Title = @Title,
                    Description = @Description,
                    StartAt = @StartAt,
                    DueAt = @DueAt,
                    ReminderAt = @ReminderAt,
                    Priority = @Priority,
                    Status = @Status,
                    UpdatedAt = SYSDATETIME(),
                    CompletedAt =
                        CASE
                            WHEN @Status = 3 THEN COALESCE(CompletedAt, SYSDATETIME())
                            ELSE NULL
                        END
                WHERE StudyTaskId = @StudyTaskId
                  AND StudentId = @StudentId;
                """;
            AddParameter(command, "@StudyTaskId", SqlDbType.BigInt, studyTaskId);
            AddStudyTaskWriteParameters(
                command,
                studentId,
                request.CourseId,
                request.Title,
                request.Description,
                request.StartAt,
                request.DueAt,
                request.ReminderAt,
                request.Priority,
                request.Status);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound();
            }
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return BadRequest(new { message = "Related student or course was not found, or a check constraint failed." });
        }

        var task = await FindStudyTaskAsync(studentId, studyTaskId, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    public async Task<IActionResult> DeleteStudyTask(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM dbo.StudyTasks
            WHERE StudyTaskId = @StudyTaskId
              AND StudentId = @StudentId;
            """;
        AddParameter(command, "@StudyTaskId", SqlDbType.BigInt, studyTaskId);
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows == 0 ? NotFound() : NoContent();
    }

    public async Task<ActionResult<PagedResult<StudentAnnouncementDto>>> GetAnnouncements(
        long studentId,
        [FromQuery] bool? isRead,
        [FromQuery] byte? announcementType,
        [FromQuery] long? sectionId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        if (announcementType is not null && !IsAnnouncementType(announcementType.Value))
        {
            return BadRequest(ValidationError("announcementType", "AnnouncementType must be between 1 and 5."));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var userId = await GetStudentUserIdAsync(connection, studentId, cancellationToken);
        if (userId is null)
        {
            return NotFound();
        }

        var visibleWhereSql = BuildStudentAnnouncementWhereSql(activeOnly);

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = $"""
            SELECT COUNT(1)
            {StudentAnnouncementFromSql}
            {visibleWhereSql}
              AND (@IsRead IS NULL OR ISNULL(ar.IsRead, 0) = @IsRead)
              AND (@AnnouncementType IS NULL OR a.AnnouncementType = @AnnouncementType)
              AND (@SectionId IS NULL OR a.SectionId = @SectionId);
            """;
        AddAnnouncementFilterParameters(countCommand, studentId, userId.Value, isRead, announcementType, sectionId);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentAnnouncementSelectSql}
            {StudentAnnouncementFromSql}
            {visibleWhereSql}
              AND (@IsRead IS NULL OR ISNULL(ar.IsRead, 0) = @IsRead)
              AND (@AnnouncementType IS NULL OR a.AnnouncementType = @AnnouncementType)
              AND (@SectionId IS NULL OR a.SectionId = @SectionId)
            ORDER BY a.PublishedAt DESC, a.AnnouncementId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;
        AddAnnouncementFilterParameters(command, studentId, userId.Value, isRead, announcementType, sectionId);
        AddParameter(command, "@Offset", SqlDbType.Int, (page - 1) * pageSize);
        AddParameter(command, "@PageSize", SqlDbType.Int, pageSize);

        var announcements = new List<StudentAnnouncementDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            announcements.Add(ReadStudentAnnouncement(reader));
        }

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return Ok(new PagedResult<StudentAnnouncementDto>(announcements, page, pageSize, totalCount, totalPages));
    }

    public async Task<IActionResult> MarkAnnouncementAsRead(
        long studentId,
        long announcementId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var userId = await GetStudentUserIdAsync(connection, studentId, cancellationToken);
        if (userId is null)
        {
            return NotFound();
        }

        if (!await AnnouncementVisibleToStudentAsync(connection, studentId, announcementId, cancellationToken))
        {
            return NotFound();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_MarkAnnouncementAsRead";
        command.CommandType = CommandType.StoredProcedure;
        AddParameter(command, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        AddParameter(command, "@UserId", SqlDbType.BigInt, userId.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return NoContent();
    }

    private async Task<StudyGoalDto?> FindStudyGoalAsync(
        long studentId,
        long goalId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                GoalId,
                StudentId,
                Title,
                Description,
                GoalType,
                TargetValue,
                CurrentValue,
                StartDate,
                EndDate,
                Status,
                CreatedAt,
                UpdatedAt
            FROM dbo.StudyGoals
            WHERE GoalId = @GoalId
              AND StudentId = @StudentId;
            """;
        AddParameter(command, "@GoalId", SqlDbType.BigInt, goalId);
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStudyGoal(reader) : null;
    }

    private async Task<StudyTaskDto?> FindStudyTaskAsync(
        long studentId,
        long studyTaskId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudyTaskSelectSql}
            WHERE st.StudyTaskId = @StudyTaskId
              AND st.StudentId = @StudentId;
            """;
        AddParameter(command, "@StudyTaskId", SqlDbType.BigInt, studyTaskId);
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStudyTask(reader) : null;
    }

    private static async Task<bool> StudentExistsAsync(
        SqlConnection connection,
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM dbo.Students WHERE StudentId = @StudentId;";
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null and not DBNull;
    }

    private static async Task<long?> GetStudentUserIdAsync(
        SqlConnection connection,
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT UserId FROM dbo.Students WHERE StudentId = @StudentId;";
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static async Task<StudentProfileDto?> FindStudentProfileAsync(
        SqlConnection connection,
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            {StudentProfileSelectSql}
            WHERE StudentId = @StudentId;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStudentProfile(reader) : null;
    }

    private static async Task<AssignmentAvailability?> GetAssignmentAvailabilityAsync(
        SqlConnection connection,
        long studentId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                a.OpenAt,
                a.DueAt,
                a.AllowLateSubmission,
                SYSDATETIME() AS CurrentTime
            FROM dbo.Assignments a
            INNER JOIN dbo.Enrollments e
                ON e.SectionId = a.SectionId
               AND e.StudentId = @StudentId
               AND e.Status = 1
            WHERE a.AssignmentId = @AssignmentId
              AND a.IsPublished = 1;
            """;
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@AssignmentId", SqlDbType.BigInt, assignmentId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new AssignmentAvailability(
            GetNullableDateTime(reader, "OpenAt"),
            reader.GetDateTime(reader.GetOrdinal("DueAt")),
            reader.GetBoolean(reader.GetOrdinal("AllowLateSubmission")),
            reader.GetDateTime(reader.GetOrdinal("CurrentTime")));
    }

    private static async Task<bool> AnnouncementVisibleToStudentAsync(
        SqlConnection connection,
        long studentId,
        long announcementId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT 1
            FROM dbo.Announcements a
            WHERE a.AnnouncementId = @AnnouncementId
              AND a.IsActive = 1
              AND a.PublishedAt <= SYSDATETIME()
              AND (a.ExpiresAt IS NULL OR a.ExpiresAt >= SYSDATETIME())
              AND
              (
                  a.SectionId IS NULL
                  OR EXISTS
                  (
                      SELECT 1
                      FROM dbo.Enrollments e
                      WHERE e.StudentId = @StudentId
                        AND e.SectionId = a.SectionId
                        AND e.Status IN (1, 2)
                  )
              );
            """;
        AddParameter(command, "@AnnouncementId", SqlDbType.BigInt, announcementId);
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null and not DBNull;
    }

    private static Dictionary<string, string[]> ValidateSubmitAssignmentRequest(SubmitAssignmentRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var textContent = NormalizeOptionalText(request.TextContent);
        var fileUrl = NormalizeOptionalText(request.FileUrl);

        if (textContent is null && fileUrl is null)
        {
            errors["content"] = ["TextContent or FileUrl is required."];
        }

        if (fileUrl is not null && fileUrl.Length > 1000)
        {
            errors["fileUrl"] = ["FileUrl cannot exceed 1000 characters."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateStudentProfileRequest(UpdateStudentProfileRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors["fullName"] = ["FullName is required."];
        }
        else if (request.FullName.Trim().Length > 150)
        {
            errors["fullName"] = ["FullName cannot exceed 150 characters."];
        }

        if (request.Phone is not null && request.Phone.Trim().Length > 20)
        {
            errors["phone"] = ["Phone cannot exceed 20 characters."];
        }

        if (request.DateOfBirth is not null && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            errors["dateOfBirth"] = ["DateOfBirth cannot be in the future."];
        }

        if (request.Gender is not null && request.Gender > 2)
        {
            errors["gender"] = ["Gender must be 0, 1, or 2."];
        }

        if (request.AvatarUrl is not null && request.AvatarUrl.Trim().Length > 1000)
        {
            errors["avatarUrl"] = ["AvatarUrl cannot exceed 1000 characters."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateStudyGoalRequest(
        string title,
        byte goalType,
        DateOnly startDate,
        DateOnly? endDate,
        byte status)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Title is required."];
        }
        else if (title.Trim().Length > 250)
        {
            errors["title"] = ["Title cannot exceed 250 characters."];
        }

        if (!IsStudyGoalType(goalType))
        {
            errors["goalType"] = ["GoalType must be between 1 and 5."];
        }

        if (!IsStudyGoalStatus(status))
        {
            errors["status"] = ["Status must be 1, 2, or 3."];
        }

        if (endDate is not null && endDate < startDate)
        {
            errors["endDate"] = ["EndDate must be greater than or equal to StartDate."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateStudyTaskRequest(
        int? courseId,
        string title,
        DateTime? startAt,
        DateTime? dueAt,
        byte priority,
        byte status)
    {
        var errors = new Dictionary<string, string[]>();

        if (courseId <= 0)
        {
            errors["courseId"] = ["CourseId must be greater than 0 when provided."];
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Title is required."];
        }
        else if (title.Trim().Length > 250)
        {
            errors["title"] = ["Title cannot exceed 250 characters."];
        }

        if (!IsStudyTaskPriority(priority))
        {
            errors["priority"] = ["Priority must be 1, 2, or 3."];
        }

        if (!IsStudyTaskStatus(status))
        {
            errors["status"] = ["Status must be 1, 2, 3, or 4."];
        }

        if (startAt is not null && dueAt is not null && dueAt < startAt)
        {
            errors["dueAt"] = ["DueAt must be greater than or equal to StartAt."];
        }

        return errors;
    }

    private static void AddAssignmentFilterParameters(
        SqlCommand command,
        long studentId,
        long? sectionId,
        byte? submissionStatus,
        DateTime? fromDueAt,
        DateTime? toDueAt)
    {
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);
        AddParameter(command, "@SubmissionStatus", SqlDbType.TinyInt, submissionStatus);
        AddParameter(command, "@FromDueAt", SqlDbType.DateTime2, fromDueAt);
        AddParameter(command, "@ToDueAt", SqlDbType.DateTime2, toDueAt);
    }

    private static void AddStudyGoalWriteParameters(
        SqlCommand command,
        long studentId,
        string title,
        string? description,
        byte goalType,
        decimal? targetValue,
        decimal? currentValue,
        DateOnly startDate,
        DateOnly? endDate,
        byte status)
    {
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@Title", SqlDbType.NVarChar, title.Trim(), 250);
        AddParameter(command, "@Description", SqlDbType.NVarChar, NormalizeOptionalText(description), -1);
        AddParameter(command, "@GoalType", SqlDbType.TinyInt, goalType);
        AddDecimalParameter(command, "@TargetValue", targetValue, 10, 2);
        AddDecimalParameter(command, "@CurrentValue", currentValue, 10, 2);
        AddParameter(command, "@StartDate", SqlDbType.Date, startDate);
        AddParameter(command, "@EndDate", SqlDbType.Date, endDate);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);
    }

    private static void AddStudyTaskWriteParameters(
        SqlCommand command,
        long studentId,
        int? courseId,
        string title,
        string? description,
        DateTime? startAt,
        DateTime? dueAt,
        DateTime? reminderAt,
        byte priority,
        byte status)
    {
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@CourseId", SqlDbType.Int, courseId);
        AddParameter(command, "@Title", SqlDbType.NVarChar, title.Trim(), 250);
        AddParameter(command, "@Description", SqlDbType.NVarChar, NormalizeOptionalText(description), -1);
        AddParameter(command, "@StartAt", SqlDbType.DateTime2, startAt);
        AddParameter(command, "@DueAt", SqlDbType.DateTime2, dueAt);
        AddParameter(command, "@ReminderAt", SqlDbType.DateTime2, reminderAt);
        AddParameter(command, "@Priority", SqlDbType.TinyInt, priority);
        AddParameter(command, "@Status", SqlDbType.TinyInt, status);
    }

    private static void AddAnnouncementFilterParameters(
        SqlCommand command,
        long studentId,
        long userId,
        bool? isRead,
        byte? announcementType,
        long? sectionId)
    {
        AddParameter(command, "@StudentId", SqlDbType.BigInt, studentId);
        AddParameter(command, "@UserId", SqlDbType.BigInt, userId);
        AddParameter(command, "@IsRead", SqlDbType.Bit, isRead);
        AddParameter(command, "@AnnouncementType", SqlDbType.TinyInt, announcementType);
        AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);
    }

    private static void AddSearchParameter(SqlCommand command, string? search)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : $"%{search.Trim()}%";

        AddParameter(command, "@Search", SqlDbType.NVarChar, normalizedSearch, 256);
    }

    private static string BuildStudentAnnouncementWhereSql(bool activeOnly)
    {
        var activeFilter = activeOnly
            ? """
              AND a.IsActive = 1
              AND a.PublishedAt <= SYSDATETIME()
              AND (a.ExpiresAt IS NULL OR a.ExpiresAt >= SYSDATETIME())
            """
            : string.Empty;

        return $"""
            WHERE
            (
                a.SectionId IS NULL
                OR EXISTS
                (
                    SELECT 1
                    FROM dbo.Enrollments e
                    WHERE e.StudentId = @StudentId
                      AND e.SectionId = a.SectionId
                      AND e.Status IN (1, 2)
                )
            )
            {activeFilter}
            """;
    }

    private static StudyGoalDto ReadStudyGoal(SqlDataReader reader)
    {
        return new StudyGoalDto(
            reader.GetInt64(reader.GetOrdinal("GoalId")),
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetString(reader.GetOrdinal("Title")),
            GetNullableString(reader, "Description"),
            reader.GetByte(reader.GetOrdinal("GoalType")),
            GetNullableDecimal(reader, "TargetValue"),
            GetNullableDecimal(reader, "CurrentValue"),
            GetDateOnly(reader, "StartDate"),
            GetNullableDateOnly(reader, "EndDate"),
            reader.GetByte(reader.GetOrdinal("Status")),
            reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            GetNullableDateTime(reader, "UpdatedAt"));
    }

    private static StudyTaskDto ReadStudyTask(SqlDataReader reader)
    {
        return new StudyTaskDto(
            reader.GetInt64(reader.GetOrdinal("StudyTaskId")),
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            GetNullableInt(reader, "CourseId"),
            GetNullableString(reader, "CourseCode"),
            GetNullableString(reader, "CourseName"),
            reader.GetString(reader.GetOrdinal("Title")),
            GetNullableString(reader, "Description"),
            GetNullableDateTime(reader, "StartAt"),
            GetNullableDateTime(reader, "DueAt"),
            GetNullableDateTime(reader, "ReminderAt"),
            reader.GetByte(reader.GetOrdinal("Priority")),
            reader.GetByte(reader.GetOrdinal("Status")),
            reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            GetNullableDateTime(reader, "UpdatedAt"),
            GetNullableDateTime(reader, "CompletedAt"));
    }

    private static StudentExamScheduleDto ReadStudentExamSchedule(SqlDataReader reader)
    {
        return new StudentExamScheduleDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetInt64(reader.GetOrdinal("SectionId")),
            reader.GetString(reader.GetOrdinal("SectionCode")),
            reader.GetString(reader.GetOrdinal("CourseCode")),
            reader.GetString(reader.GetOrdinal("CourseName")),
            reader.GetInt64(reader.GetOrdinal("ExamId")),
            reader.GetString(reader.GetOrdinal("ExamName")),
            reader.GetByte(reader.GetOrdinal("ExamType")),
            GetDateOnly(reader, "ExamDate"),
            GetTimeString(reader, "StartTime"),
            reader.GetInt16(reader.GetOrdinal("DurationMinutes")),
            GetNullableString(reader, "Room"),
            GetNullableString(reader, "Note"),
            reader.GetDateTime(reader.GetOrdinal("CreatedAt")));
    }

    private static StudentAssignmentDto ReadStudentAssignment(SqlDataReader reader)
    {
        return new StudentAssignmentDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetInt64(reader.GetOrdinal("SectionId")),
            reader.GetString(reader.GetOrdinal("CourseCode")),
            reader.GetString(reader.GetOrdinal("CourseName")),
            reader.GetInt64(reader.GetOrdinal("AssignmentId")),
            reader.GetString(reader.GetOrdinal("Title")),
            GetNullableString(reader, "Description"),
            GetNullableString(reader, "AttachmentUrl"),
            GetNullableDateTime(reader, "OpenAt"),
            reader.GetDateTime(reader.GetOrdinal("DueAt")),
            reader.GetDecimal(reader.GetOrdinal("MaxScore")),
            reader.GetBoolean(reader.GetOrdinal("AllowLateSubmission")),
            reader.GetBoolean(reader.GetOrdinal("IsPublished")),
            reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            GetNullableDateTime(reader, "UpdatedAt"),
            GetNullableLong(reader, "SubmissionId"),
            GetNullableString(reader, "TextContent"),
            GetNullableString(reader, "FileUrl"),
            GetNullableDateTime(reader, "SubmittedAt"),
            GetNullableBool(reader, "IsLate"),
            GetNullableByte(reader, "SubmissionRawStatus"),
            reader.GetString(reader.GetOrdinal("SubmissionStatus")),
            GetNullableDecimal(reader, "Score"),
            GetNullableString(reader, "Feedback"),
            GetNullableDateTime(reader, "GradedAt"));
    }

    private static AssignmentSubmissionDto ReadAssignmentSubmission(SqlDataReader reader)
    {
        return new AssignmentSubmissionDto(
            reader.GetInt64(reader.GetOrdinal("SubmissionId")),
            reader.GetInt64(reader.GetOrdinal("AssignmentId")),
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            GetNullableString(reader, "TextContent"),
            GetNullableString(reader, "FileUrl"),
            reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
            reader.GetBoolean(reader.GetOrdinal("IsLate")),
            reader.GetByte(reader.GetOrdinal("Status")),
            GetNullableDecimal(reader, "Score"),
            GetNullableString(reader, "Feedback"),
            GetNullableLong(reader, "GradedByUserId"),
            GetNullableDateTime(reader, "GradedAt"));
    }

    private static StudentAnnouncementDto ReadStudentAnnouncement(SqlDataReader reader)
    {
        return new StudentAnnouncementDto(
            reader.GetInt64(reader.GetOrdinal("AnnouncementId")),
            reader.GetInt64(reader.GetOrdinal("CreatedByUserId")),
            reader.GetString(reader.GetOrdinal("CreatedByFullName")),
            GetNullableLong(reader, "SectionId"),
            GetNullableString(reader, "SectionCode"),
            GetNullableString(reader, "CourseCode"),
            GetNullableString(reader, "CourseName"),
            reader.GetString(reader.GetOrdinal("Title")),
            reader.GetString(reader.GetOrdinal("Content")),
            reader.GetByte(reader.GetOrdinal("AnnouncementType")),
            reader.GetDateTime(reader.GetOrdinal("PublishedAt")),
            GetNullableDateTime(reader, "ExpiresAt"),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            reader.GetBoolean(reader.GetOrdinal("IsRead")),
            GetNullableDateTime(reader, "ReadAt"));
    }

    private static StudentProfileDto ReadStudentProfile(SqlDataReader reader)
    {
        return new StudentProfileDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetInt64(reader.GetOrdinal("UserId")),
            reader.GetString(reader.GetOrdinal("StudentCode")),
            reader.GetString(reader.GetOrdinal("Username")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("Email")),
            GetNullableString(reader, "Phone"),
            GetNullableDateOnly(reader, "DateOfBirth"),
            GetNullableByte(reader, "Gender"),
            GetNullableString(reader, "AvatarUrl"),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            GetNullableInt(reader, "AcademicClassId"),
            GetNullableString(reader, "ClassCode"),
            GetNullableString(reader, "ClassName"),
            reader.GetInt32(reader.GetOrdinal("MajorId")),
            reader.GetString(reader.GetOrdinal("MajorCode")),
            reader.GetString(reader.GetOrdinal("MajorName")),
            reader.GetInt32(reader.GetOrdinal("DepartmentId")),
            reader.GetString(reader.GetOrdinal("DepartmentCode")),
            reader.GetString(reader.GetOrdinal("DepartmentName")),
            reader.GetInt16(reader.GetOrdinal("EnrollmentYear")),
            reader.GetByte(reader.GetOrdinal("Status")));
    }

    private static bool IsEnrollmentStatus(byte status)
    {
        return status is 0 or 1 or 2;
    }

    private static bool IsExamType(byte examType)
    {
        return examType is >= 1 and <= 4;
    }

    private static bool IsSubmissionStatus(byte status)
    {
        return status is >= 1 and <= 3;
    }

    private static bool IsStudyGoalType(byte goalType)
    {
        return goalType is >= 1 and <= 5;
    }

    private static bool IsStudyGoalStatus(byte status)
    {
        return status is >= 1 and <= 3;
    }

    private static bool IsStudyTaskPriority(byte priority)
    {
        return priority is >= 1 and <= 3;
    }

    private static bool IsStudyTaskStatus(byte status)
    {
        return status is >= 1 and <= 4;
    }

    private static bool IsAnnouncementType(byte announcementType)
    {
        return announcementType is >= 1 and <= 5;
    }

    private static bool IsConstraintViolation(SqlException exception)
    {
        return exception.Number == 547;
    }

    private static ValidationProblemDetails ValidationError(string field, string message)
    {
        return new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [field] = [message]
        });
    }

    private static SqlParameter AddParameter(
        SqlCommand command,
        string name,
        SqlDbType type,
        object? value,
        int? size = null)
    {
        var parameter = size is null
            ? command.Parameters.Add(name, type)
            : command.Parameters.Add(name, type, size.Value);
        parameter.Value = ToDbValue(value);
        return parameter;
    }

    private static SqlParameter AddDecimalParameter(
        SqlCommand command,
        string name,
        decimal? value,
        byte precision,
        byte scale)
    {
        var parameter = AddParameter(command, name, SqlDbType.Decimal, value);
        parameter.Precision = precision;
        parameter.Scale = scale;
        return parameter;
    }

    private static object ToDbValue(object? value)
    {
        return value switch
        {
            null => DBNull.Value,
            DateOnly date => date.ToDateTime(TimeOnly.MinValue),
            _ => value
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static long? GetNullableLong(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static byte? GetNullableByte(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetByte(ordinal);
    }

    private static bool? GetNullableBool(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static DateOnly? GetNullableDateOnly(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(ordinal));
    }

    private static DateOnly GetDateOnly(SqlDataReader reader, string columnName)
    {
        return DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal(columnName)));
    }

    private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }

    private static string GetTimeString(SqlDataReader reader, string columnName)
    {
        return reader.GetTimeSpan(reader.GetOrdinal(columnName)).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }

    private sealed record AssignmentAvailability(
        DateTime? OpenAt,
        DateTime DueAt,
        bool AllowLateSubmission,
        DateTime CurrentTime);

    private const string StudentAssignmentSelectSql = """
        SELECT
            e.StudentId,
            cs.SectionId,
            c.CourseCode,
            c.CourseName,
            a.AssignmentId,
            a.Title,
            a.Description,
            a.AttachmentUrl,
            a.OpenAt,
            a.DueAt,
            a.MaxScore,
            a.AllowLateSubmission,
            a.IsPublished,
            a.CreatedAt,
            a.UpdatedAt,
            sub.SubmissionId,
            sub.TextContent,
            sub.FileUrl,
            sub.SubmittedAt,
            sub.IsLate,
            sub.Status AS SubmissionRawStatus,
            CASE
                WHEN sub.SubmissionId IS NULL AND SYSDATETIME() > a.DueAt THEN N'Quá hạn'
                WHEN sub.SubmissionId IS NULL THEN N'Chưa nộp'
                WHEN sub.Status = 1 THEN N'Đã nộp'
                WHEN sub.Status = 2 THEN N'Đã chấm'
                WHEN sub.Status = 3 THEN N'Yêu cầu nộp lại'
                ELSE N'Không xác định'
            END AS SubmissionStatus,
            sub.Score,
            sub.Feedback,
            sub.GradedAt
        FROM dbo.Enrollments e
        INNER JOIN dbo.CourseSections cs ON cs.SectionId = e.SectionId
        INNER JOIN dbo.Courses c ON c.CourseId = cs.CourseId
        INNER JOIN dbo.Assignments a ON a.SectionId = cs.SectionId
        LEFT JOIN dbo.AssignmentSubmissions sub
            ON sub.AssignmentId = a.AssignmentId
           AND sub.StudentId = e.StudentId
        """;

    private const string StudentProfileSelectSql = """
        SELECT
            StudentId,
            UserId,
            StudentCode,
            Username,
            FullName,
            Email,
            Phone,
            DateOfBirth,
            Gender,
            AvatarUrl,
            IsActive,
            AcademicClassId,
            ClassCode,
            ClassName,
            MajorId,
            MajorCode,
            MajorName,
            DepartmentId,
            DepartmentCode,
            DepartmentName,
            EnrollmentYear,
            Status
        FROM dbo.vw_StudentProfile
        """;

    private const string StudyTaskSelectSql = """
        SELECT
            st.StudyTaskId,
            st.StudentId,
            st.CourseId,
            c.CourseCode,
            c.CourseName,
            st.Title,
            st.Description,
            st.StartAt,
            st.DueAt,
            st.ReminderAt,
            st.Priority,
            st.Status,
            st.CreatedAt,
            st.UpdatedAt,
            st.CompletedAt
        FROM dbo.StudyTasks st
        LEFT JOIN dbo.Courses c ON c.CourseId = st.CourseId
        """;

    private const string StudentAnnouncementSelectSql = """
        SELECT
            a.AnnouncementId,
            a.CreatedByUserId,
            u.FullName AS CreatedByFullName,
            a.SectionId,
            cs.SectionCode,
            c.CourseCode,
            c.CourseName,
            a.Title,
            a.Content,
            a.AnnouncementType,
            a.PublishedAt,
            a.ExpiresAt,
            a.IsActive,
            CONVERT(BIT, ISNULL(ar.IsRead, 0)) AS IsRead,
            ar.ReadAt
        """;

    private const string StudentAnnouncementFromSql = """
        FROM dbo.Announcements a
        INNER JOIN dbo.Users u ON u.UserId = a.CreatedByUserId
        LEFT JOIN dbo.CourseSections cs ON cs.SectionId = a.SectionId
        LEFT JOIN dbo.Courses c ON c.CourseId = cs.CourseId
        LEFT JOIN dbo.AnnouncementReads ar
            ON ar.AnnouncementId = a.AnnouncementId
           AND ar.UserId = @UserId
        """;
}
