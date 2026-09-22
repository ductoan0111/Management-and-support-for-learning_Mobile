using BE_Mobile.Contracts.Auth;
using BE_Mobile.Contracts.Common;

namespace BE_Mobile.Services.Interfaces;

public interface IAuthService
{
    Task<OperationResult<AuthUserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<OperationResult<AuthUserDto>> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken);
}
