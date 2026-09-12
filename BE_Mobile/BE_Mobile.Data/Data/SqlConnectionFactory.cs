using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BE_Mobile.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
