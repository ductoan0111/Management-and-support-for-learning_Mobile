using BE_Mobile.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace BE_Mobile.Controllers;

internal static class ControllerResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, OperationResult<T> result)
    {
        if (result.Succeeded)
        {
            return controller.Ok(result.Value);
        }

        return result.Error!.Type switch
        {
            OperationErrorType.Validation => controller.BadRequest(new ValidationProblemDetails(
                result.Error.ValidationErrors?.ToDictionary() ?? new Dictionary<string, string[]>())),
            OperationErrorType.NotFound => controller.NotFound(),
            OperationErrorType.Conflict => controller.Conflict(new { message = result.Error.Message }),
            OperationErrorType.BadRequest => controller.BadRequest(new { message = result.Error.Message }),
            _ => controller.Problem(result.Error.Message)
        };
    }

    public static IActionResult ToActionResult(this ControllerBase controller, OperationResult result)
    {
        if (result.Succeeded)
        {
            return controller.NoContent();
        }

        return result.Error!.Type switch
        {
            OperationErrorType.Validation => controller.BadRequest(new ValidationProblemDetails(
                result.Error.ValidationErrors?.ToDictionary() ?? new Dictionary<string, string[]>())),
            OperationErrorType.NotFound => controller.NotFound(),
            OperationErrorType.Conflict => controller.Conflict(new { message = result.Error.Message }),
            OperationErrorType.BadRequest => controller.BadRequest(new { message = result.Error.Message }),
            _ => controller.Problem(result.Error.Message)
        };
    }

    private static Dictionary<string, string[]> ToDictionary(
        this IReadOnlyDictionary<string, string[]> validationErrors)
    {
        return validationErrors.ToDictionary(
            static error => error.Key,
            static error => error.Value);
    }
}
