using System.Data;
using BE_Mobile.Contracts.Teachers;
using BE_Mobile.Data;
using BE_Mobile.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

[NonController]
public sealed class TeacherStudentRepository(IDbConnectionFactory connectionFactory)
    : ControllerBase, ITeacherStudentRepository
{
    public async Task<IReadOnlyList<TeacherSectionStudentDto>> GetStudentsBySectionAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_GetStudentsBySection";
        command.CommandType = CommandType.StoredProcedure;
        SqlRepositoryHelper.AddParameter(command, "@SectionId", SqlDbType.BigInt, sectionId);

        var students = new List<TeacherSectionStudentDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            students.Add(new TeacherSectionStudentDto(
                reader.GetInt64(reader.GetOrdinal("StudentId")),
                reader.GetString(reader.GetOrdinal("StudentCode")),
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetString(reader.GetOrdinal("Email")),
                SqlRepositoryHelper.GetNullableString(reader, "Phone"),
                reader.GetDateTime(reader.GetOrdinal("EnrolledAt")),
                reader.GetByte(reader.GetOrdinal("Status")),
                SqlRepositoryHelper.GetNullableDecimal(reader, "FinalScore10"),
                SqlRepositoryHelper.GetNullableString(reader, "LetterGrade")));
        }

        return students;
    }

    public async Task<ActionResult<TeacherSectionStudentDetailDto>> GetStudentInSection(
        long teacherId,
        long sectionId,
        long studentId,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!await SqlRepositoryHelper.IsTeacherOfSectionAsync(connection, teacherId, sectionId, cancellationToken))
            return Forbid();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT st.StudentId,
                   st.StudentCode,
                   u.FullName,
                   u.Email,
                   u.Phone,
                   CAST(u.DateOfBirth AS DATE)  AS DateOfBirth,
                   u.Gender,
                   u.AvatarUrl,
                   ac.ClassCode,
                   ac.ClassName,
                   m.MajorCode,
                   m.MajorName,
                   e.EnrolledAt,
                   e.Status    AS EnrollmentStatus,
                   e.FinalScore10,
                   e.LetterGrade
            FROM   dbo.Enrollments e
            INNER JOIN dbo.Students       st  ON st.StudentId      = e.StudentId
            INNER JOIN dbo.Users          u   ON u.UserId          = st.UserId
            INNER JOIN dbo.Majors         m   ON m.MajorId         = st.MajorId
            LEFT  JOIN dbo.AcademicClasses ac ON ac.AcademicClassId = st.AcademicClassId
            WHERE  e.SectionId = @SectionId AND e.StudentId = @StudentId;
            """;
        SqlRepositoryHelper.AddParameter(cmd, "@SectionId", SqlDbType.BigInt, sectionId);
        SqlRepositoryHelper.AddParameter(cmd, "@StudentId", SqlDbType.BigInt, studentId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound();

        var dob = SqlRepositoryHelper.GetNullableDate(reader, "DateOfBirth");
        return Ok(new TeacherSectionStudentDetailDto(
            reader.GetInt64(reader.GetOrdinal("StudentId")),
            reader.GetString(reader.GetOrdinal("StudentCode")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("Email")),
            SqlRepositoryHelper.GetNullableString(reader, "Phone"),
            dob,
            SqlRepositoryHelper.GetNullableByte(reader, "Gender"),
            SqlRepositoryHelper.GetNullableString(reader, "AvatarUrl"),
            SqlRepositoryHelper.GetNullableString(reader, "ClassCode"),
            SqlRepositoryHelper.GetNullableString(reader, "ClassName"),
            reader.GetString(reader.GetOrdinal("MajorCode")),
            reader.GetString(reader.GetOrdinal("MajorName")),
            reader.GetDateTime(reader.GetOrdinal("EnrolledAt")),
            reader.GetByte(reader.GetOrdinal("EnrollmentStatus")),
            SqlRepositoryHelper.GetNullableDecimal(reader, "FinalScore10"),
            SqlRepositoryHelper.GetNullableString(reader, "LetterGrade")));
    }
}
