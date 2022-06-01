using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using ms_identity_dotnet_blazor_azure_sql.Database;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ms_identity_dotnet_blazor_azure_sql.AAD
{
    public class UserAADService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfidentialClientApplication _app;
        private readonly IConfigurationSection _azureADSettings;
        private readonly SqlDatabaseService _databaseService;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public UserAADService(IConfiguration configuration, SqlDatabaseService databaseService, MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _consentHandler = consentHandler;
            _databaseService = databaseService;
            _configuration = configuration;
            _azureADSettings = _configuration.GetSection("AzureAd");

            //create confidential client because this Blazor tempate is Blazor Server
            _app =
                ConfidentialClientApplicationBuilder.Create(_azureADSettings["ClientId"])
                    .WithClientSecret(_azureADSettings["ClientSecret"])
                    .WithAuthority(AzureCloudInstance.AzurePublic, _azureADSettings["TenantId"])
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();
        }

        //Acquire Access Token to be used for conneting to Azure SQL Database
        public async Task<string> GetAccessTokenForSqlDatabase(AuthenticationState authState)
        {
            string accessToken = string.Empty;

            //https://database.windows.net/.default
            var scopes = new string[] { _azureADSettings["Scopes"] };

            try
            {
                //construct account identifier in form of "userId.tenantId",
                //return null if no logged-in user information available from authentication context
                var accountIdentifier = GetAccountIdentifier(authState);

                //get user account from token cache
                //thow MSalUiRequiredException if account cannot be obtainer or if account identifier is null
                IAccount account = await _app.GetAccountAsync(accountIdentifier);

                //try to get a cached token based on logged-in user account
                //throw MsalUiRequiredException otherwise
                AuthenticationResult authResult = await _app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                accessToken = authResult.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                _consentHandler.ChallengeUser(scopes);
                return accessToken;
            }

            return accessToken;
        }

        //use access token to connect to underlying database to obtain @User and @Host information
        public async Task<string> GetDatabaseLoggedInUser(AuthenticationState authState)
        {
            //default user name that will indicate to web page that user could not obtained and suggestion to re-login
            var loggedInUser = "N/A";

            //get access token from cache or fron interactive session
            var token = await GetAccessTokenForSqlDatabase(authState);
            if (string.IsNullOrEmpty(token))
            {
                return loggedInUser;
            }

            //connect to database with connection string from configuration and access token
            using (SqlConnection conn = _databaseService.GetSqlConnection(token))
            {
                try
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        await conn.OpenAsync();
                    }

                    //obtain user and host from a function that was created on the sample setup
                    SqlCommand cmd = new(@"SELECT [dbo].[UsernamePrintFn]()", conn);

                    loggedInUser = (await cmd.ExecuteScalarAsync()).ToString();
                }
                catch (SqlException ex)
                {
                    //throw any sql exception to the page
                    throw new Exception(ex.Message);
                }
                finally
                {
                    //close SQL connection anyways
                    if (conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            return loggedInUser;
        }

        //construct account identifier as it stored in application cache: "userId.tenantId"
        //authentication state is obtainer from HttpContext.User which include corresponding claims
        private string GetAccountIdentifier(AuthenticationState authState)
        {
            if (authState.User.Identities.First().Claims.Where(c => c.Type == "uid").Count() == 0 ||
                authState.User.Identities.First().Claims.Where(c => c.Type == "utid").Count() == 0)
            {
                return null;
            }
            //return "<user object id>.<tenant id>" which is account identifier;
            return authState.User.Identities.First().Claims.Where(c => c.Type == "uid").First().Value + "." +
                authState.User.Identities.First().Claims.Where(c => c.Type == "utid").First().Value;
        }
    }
}

