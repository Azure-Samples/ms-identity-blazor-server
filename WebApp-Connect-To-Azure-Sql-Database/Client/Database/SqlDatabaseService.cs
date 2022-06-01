using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ms_identity_dotnet_blazor_azure_sql.Database
{
    //simple SQL Database Service intended to encapsulate SQL related configuration and functionality
    public class SqlDatabaseService
    {
        readonly IConfiguration _configuration;

        public SqlDatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetSqlConnection(string accessToken, string connStringName = "SqlDbContext")
        {
            SqlConnection conn = new(_configuration.GetConnectionString(connStringName));

            //use the obtained token inside Azure SQL Database connection string
            conn.AccessToken = accessToken;

            return conn;
        }
    }
}
