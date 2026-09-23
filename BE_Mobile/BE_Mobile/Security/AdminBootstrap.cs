using System.ComponentModel.DataAnnotations;
using System.Data;
using BE_Mobile.Contracts.Admin;
using BE_Mobile.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Security;

internal static class AdminBootstrap
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        var request = new CreateAdminUserRequest
        {
            Username = configuration["BootstrapAdmin:Username"] ?? "",
            Password = configuration["BootstrapAdmin:Password"] ?? "",
            Email = configuration["BootstrapAdmin:Email"] ?? "",
            FullName = configuration["BootstrapAdmin:FullName"] ?? "Administrator",
            RoleId = 1
        };
        Validator.ValidateObject(request, new ValidationContext(request), true);
        using var scope = services.CreateScope();
        await using var connection = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            DECLARE @RoleId tinyint = (SELECT RoleId FROM dbo.Roles WHERE RoleCode = 'ADMIN');
            IF @RoleId IS NULL THROW 50001, 'Initialize the database Roles table first.', 1;
            IF EXISTS (SELECT 1 FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE Username = @Username)
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username AND RoleId = @RoleId AND PasswordHash = N'test-password-hash')
                    THROW 50001, 'Bootstrap only accepts a new account or the untouched admin seed account.', 1;
                UPDATE dbo.Users SET PasswordHash = @Hash, IsActive = 1, UpdatedAt = SYSDATETIME() WHERE Username = @Username;
            END
            ELSE
                INSERT INTO dbo.Users (RoleId, Username, Email, FullName, PasswordHash)
                VALUES (@RoleId, @Username, @Email, @FullName, @Hash);
            """;
        command.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = request.Username;
        command.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = request.Email;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = request.FullName;
        command.Parameters.Add("@Hash", SqlDbType.NVarChar, 500).Value = new PasswordHasher<object>().HashPassword(request, request.Password);
        await command.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
        Console.WriteLine("Administrator initialized. The password was not logged.");
    }
}
