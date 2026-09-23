using BE_Mobile.Contracts.Auth;

namespace BE_Mobile.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<AuthUserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto?> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken);
    Task<bool> ExistsUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken);
}
