using System.ComponentModel.DataAnnotations;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Contracts.Common;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Services.Implementations;

public sealed class AdminStudentService(IAdminStudentRepository studentRepository) : IAdminStudentService
{
    private const int MaxPageSize = 100;
    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<OperationResult<PagedResult<AdminStudentDto>>> GetStudentsAsync(
        string? search,
        byte? status,
        int? majorId,
        int? academicClassId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        if (status is not null && !IsValidStudentStatus(status.Value))
        {
            return OperationResult<PagedResult<AdminStudentDto>>.Validation(new Dictionary<string, string[]>
            {
                ["status"] = ["Status must be 0, 1, or 2."]
            });
        }

        var students = await studentRepository.GetStudentsAsync(
            search,
            status,
            majorId,
            academicClassId,
            page,
            pageSize,
            cancellationToken);

        return OperationResult<PagedResult<AdminStudentDto>>.Success(students);
    }

    public async Task<OperationResult<AdminStudentDto>> GetStudentAsync(
        long studentId,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetStudentAsync(studentId, cancellationToken);
        return student is null
            ? OperationResult<AdminStudentDto>.NotFound()
            : OperationResult<AdminStudentDto>.Success(student);
    }

    public async Task<OperationResult<AdminStudentDto>> GetStudentByUserAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetStudentByUserAsync(userId, cancellationToken);
        return student is null
            ? OperationResult<AdminStudentDto>.NotFound()
            : OperationResult<AdminStudentDto>.Success(student);
    }

    public async Task<OperationResult<AdminStudentDto>> CreateStudentAsync(
        CreateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateCreateRequest(request);
        if (errors.Count > 0)
        {
            return OperationResult<AdminStudentDto>.Validation(errors);
        }

        try
        {
            var student = await studentRepository.CreateStudentAsync(request, cancellationToken);
            return student is null
                ? OperationResult<AdminStudentDto>.Problem("Could not create student.")
                : OperationResult<AdminStudentDto>.Success(student);
        }
        catch (SqlException exception) when (IsDuplicateKey(exception))
        {
            return OperationResult<AdminStudentDto>.Conflict("StudentCode or UserId already belongs to another student.");
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return OperationResult<AdminStudentDto>.BadRequest("Related user, major, or academic class was not found, or a check constraint failed.");
        }
    }

    public async Task<OperationResult<AdminStudentDto>> UpdateStudentAsync(
        long studentId,
        UpdateAdminStudentRequest request,
        CancellationToken cancellationToken)
    {
        var errors = ValidateUpdateRequest(request);
        if (errors.Count > 0)
        {
            return OperationResult<AdminStudentDto>.Validation(errors);
        }

        try
        {
            var student = await studentRepository.UpdateStudentAsync(studentId, request, cancellationToken);
            return student is null
                ? OperationResult<AdminStudentDto>.NotFound()
                : OperationResult<AdminStudentDto>.Success(student);
        }
        catch (SqlException exception) when (IsDuplicateKey(exception))
        {
            return OperationResult<AdminStudentDto>.Conflict("StudentCode already belongs to another student.");
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return OperationResult<AdminStudentDto>.BadRequest("Related major or academic class was not found, or a check constraint failed.");
        }
    }

    public async Task<OperationResult> DeleteStudentAsync(
        long studentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await studentRepository.DeleteStudentAsync(studentId, cancellationToken);
            return deleted ? OperationResult.Success() : OperationResult.NotFound();
        }
        catch (SqlException exception) when (IsConstraintViolation(exception))
        {
            return new OperationResult(new OperationError(
                OperationErrorType.Conflict,
                "Student cannot be deleted because related records exist."));
        }
    }

    private static Dictionary<string, string[]> ValidateCreateRequest(CreateAdminStudentRequest request)
    {
        var errors = ValidateStudentFields(
            request.StudentCode,
            request.AcademicClassId,
            request.MajorId,
            request.EnrollmentYear,
            request.Status);

        if (request.UserId <= 0)
        {
            errors["userId"] = ["UserId must be greater than 0."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateUpdateRequest(UpdateAdminStudentRequest request)
    {
        var errors = ValidateStudentFields(
            request.StudentCode,
            request.AcademicClassId,
            request.MajorId,
            request.EnrollmentYear,
            request.Status);

        if (request.FullName is not null && string.IsNullOrWhiteSpace(request.FullName))
        {
            errors["fullName"] = ["FullName cannot be empty."];
        }

        if (request.Email is not null && !EmailValidator.IsValid(request.Email))
        {
            errors["email"] = ["Email is invalid."];
        }

        if (request.Gender is not null && request.Gender is not (0 or 1 or 2))
        {
            errors["gender"] = ["Gender must be 0, 1, or 2."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateStudentFields(
        string studentCode,
        int? academicClassId,
        int majorId,
        short enrollmentYear,
        byte status)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(studentCode))
        {
            errors["studentCode"] = ["StudentCode is required."];
        }
        else if (studentCode.Trim().Length > 30)
        {
            errors["studentCode"] = ["StudentCode cannot exceed 30 characters."];
        }

        if (academicClassId <= 0)
        {
            errors["academicClassId"] = ["AcademicClassId must be greater than 0 when provided."];
        }

        if (majorId <= 0)
        {
            errors["majorId"] = ["MajorId must be greater than 0."];
        }

        if (enrollmentYear is < 1900 or > 2100)
        {
            errors["enrollmentYear"] = ["EnrollmentYear must be between 1900 and 2100."];
        }

        if (!IsValidStudentStatus(status))
        {
            errors["status"] = ["Status must be 0, 1, or 2."];
        }

        return errors;
    }

    private static bool IsValidStudentStatus(byte status)
    {
        return status is 0 or 1 or 2;
    }

    private static bool IsDuplicateKey(SqlException exception)
    {
        return exception.Number is 2601 or 2627;
    }

    private static bool IsConstraintViolation(SqlException exception)
    {
        return exception.Number == 547;
    }
}
