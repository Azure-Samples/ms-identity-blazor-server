using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ms_identity_dotnet_blazor_azure_sql.AAD;
using ms_identity_dotnet_blazor_azure_sql.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ms_identity_dotnet_blazor_azure_sql.Data
{
    public class WeatherForecastService
    {
        private readonly UserAADService _userAAD;
        private readonly SqlDatabaseService _databaseService;

        public WeatherForecastService(UserAADService userAAD, SqlDatabaseService databaseService)
        {
            _userAAD = userAAD;
            _databaseService = databaseService;
        }

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, AuthenticationState authState)
        {
            //database call
            var dbSummaries = await GetSummaries(authState);

            var rnd = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rnd.Next(-20, 55),
                Summary = dbSummaries[rnd.Next(dbSummaries.Count)]
            }).ToArray();
        }

        //get summaries strings from underlying Azure SQL Database
        private async Task<IList<string>> GetSummaries(AuthenticationState authState)
        {
            var summaryList = new List<string>();

            //Lets acquire a access token from Azure Ad for your database connection string
            var accessToken = await _userAAD.GetAccessTokenForSqlDatabase(authState);
            if (accessToken.IsNullOrEmpty())
            {
                return summaryList;
            }
            
            //connect to database with connection string from configuration and access token
            using (SqlConnection conn = _databaseService.GetSqlConnection(accessToken))
            {
                try
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        await conn.OpenAsync();
                    }

                    SqlCommand cmd = new(@"select * from Summary", conn);

                    var myReader = await cmd.ExecuteReaderAsync();

                    while (myReader.Read())
                    {
                        summaryList.Add(myReader["Summary"].ToString());
                    }
                }
                catch (SqlException ex)
                {
                    //throw any sql exception to the page
                    throw new Exception(ex.Message);
                }
                finally
                {
                    //close connection anyways
                    if (conn.State == ConnectionState.Open)
                    {
                        await conn.CloseAsync();
                    }
                }
            }

            return summaryList;
        }
    }
}
