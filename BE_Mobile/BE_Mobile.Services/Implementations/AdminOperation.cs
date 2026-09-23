using System.ComponentModel.DataAnnotations;
using BE_Mobile.Contracts.Common;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Services.Implementations;

internal static class AdminOperation
{
    public static async Task<OperationResult<T>> RunAsync<T>(Func<Task<T?>> action, object? request = null)
    {
        if (request is not null)
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, new ValidationContext(request), results, true))
                return OperationResult<T>.Validation(results
                    .SelectMany(r => r.MemberNames.DefaultIfEmpty("request"), (r, member) => new { member, r.ErrorMessage })
                    .GroupBy(r => r.member).ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage!).ToArray()));
        }
        try
        {
            var value = await action();
            return value is null ? OperationResult<T>.NotFound() : OperationResult<T>.Success(value);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return OperationResult<T>.Conflict("A record with this code, username, email, or assignment already exists.");
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return OperationResult<T>.Conflict("A referenced record does not exist, or related records prevent this operation.");
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            return OperationResult<T>.Conflict(ex.Message);
        }
        catch (SqlException ex) when (ex.Number == 50004)
        {
            return OperationResult<T>.NotFound();
        }
    }

    public static async Task<OperationResult> DeleteAsync(Func<Task<bool>> action)
    {
        var result = await RunAsync(async () => await action() ? (bool?)true : null);
        return new OperationResult(result.Error);
    }
}
