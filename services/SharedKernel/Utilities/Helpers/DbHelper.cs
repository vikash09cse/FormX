using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SharedKernel.Settings;

namespace SharedKernel.Utilities.Helpers;

public class DbHelper(IOptions<DatabaseSettings> databaseSettings)
{
    public SqlConnection GetConnection()
    {
        var cs = databaseSettings.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Database connection string is not configured.");
        return new SqlConnection(cs);
    }
}
