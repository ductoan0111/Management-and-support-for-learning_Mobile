namespace BE_Mobile.Contracts.Common;

public enum OperationErrorType
{
    Validation,
    NotFound,
    BadRequest,
    Conflict,
    Problem
}

public sealed record OperationError(
    OperationErrorType Type,
    string? Message = null,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);

public sealed record OperationResult(OperationError? Error = null)
{
    public bool Succeeded => Error is null;

    public static OperationResult Success()
    {
        return new OperationResult();
    }

    public static OperationResult NotFound()
    {
        return new OperationResult(new OperationError(OperationErrorType.NotFound));
    }

    public static OperationResult BadRequest(string message)
    {
        return new OperationResult(new OperationError(OperationErrorType.BadRequest, message));
    }
}

public sealed record OperationResult<T>(T? Value, OperationError? Error = null)
{
    public bool Succeeded => Error is null;

    public static OperationResult<T> Success(T value)
    {
        return new OperationResult<T>(value);
    }

    public static OperationResult<T> NotFound()
    {
        return new OperationResult<T>(default, new OperationError(OperationErrorType.NotFound));
    }

    public static OperationResult<T> Validation(IReadOnlyDictionary<string, string[]> errors)
    {
        return new OperationResult<T>(default, new OperationError(OperationErrorType.Validation, ValidationErrors: errors));
    }

    public static OperationResult<T> BadRequest(string message)
    {
        return new OperationResult<T>(default, new OperationError(OperationErrorType.BadRequest, message));
    }

    public static OperationResult<T> Conflict(string message)
    {
        return new OperationResult<T>(default, new OperationError(OperationErrorType.Conflict, message));
    }

    public static OperationResult<T> Problem(string message)
    {
        return new OperationResult<T>(default, new OperationError(OperationErrorType.Problem, message));
    }
}
