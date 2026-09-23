using System.Data;
using BE_Mobile.Data;
using Microsoft.Data.SqlClient;

namespace BE_Mobile.Repositories.Implementations;

internal sealed class AdminSql(IDbConnectionFactory factory)
{
    public async Task<List<T>> QueryAsync<T>(string sql, Action<SqlParameterCollection> parameters,
        Func<SqlDataReader, T> read, CancellationToken cancellationToken, bool transaction = false)
    {
        await using var connection = factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var scope = transaction ? connection.BeginTransaction(IsolationLevel.Serializable) : null;
        await using var command = connection.CreateCommand();
        command.Transaction = scope;
        command.CommandText = sql;
        parameters(command.Parameters);
        var rows = new List<T>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken)) rows.Add(read(reader));
        }
        if (scope is not null) await scope.CommitAsync(cancellationToken);
        return rows;
    }

    public async Task<bool> ExecuteAsync(string sql, Action<SqlParameterCollection> parameters,
        CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(sql + "\nSELECT CAST(@@ROWCOUNT AS int);", parameters,
            r => r.GetInt32(0), cancellationToken, transaction: true);
        return rows.Single() > 0;
    }

    internal static void Add(SqlParameterCollection parameters, string name, SqlDbType type,
        object? value, int size = 0)
    {
        var parameter = size == 0 ? parameters.Add(name, type) : parameters.Add(name, type, size);
        parameter.Value = value switch
        {
            null => DBNull.Value,
            DateOnly date => date.ToDateTime(TimeOnly.MinValue),
            _ => value
        };
    }

    internal static T? Nullable<T>(SqlDataReader reader, string name) where T : struct =>
        reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetFieldValue<T>(reader.GetOrdinal(name));

    internal static string? Text(SqlDataReader reader, string name) =>
        reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetString(reader.GetOrdinal(name));
}
