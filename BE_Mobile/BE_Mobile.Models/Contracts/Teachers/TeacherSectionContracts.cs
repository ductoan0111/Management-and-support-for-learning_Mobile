namespace BE_Mobile.Contracts.Teachers;

public sealed record TeacherSectionStudentDto(
    long StudentId,
    string StudentCode,
    string FullName,
    string Email,
    string? Phone,
    DateTime EnrolledAt,
    byte Status,
    decimal? FinalScore10,
    string? LetterGrade);
