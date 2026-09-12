using Microsoft.Data.SqlClient;

namespace BE_Mobile.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
