## How the code was created

<details>
 <summary>Expand the section</summary>

 The application was generated out of standard Visual Studio template for **[Blazor Server App](https://docs.microsoft.com/aspnet/core/blazor/tooling)**
 After that SQL Server Database functionality and Authentication were configured.

1. Create initial sample, follow the [instructions](https://docs.microsoft.com/aspnet/core/blazor/tooling).  During the setup choose to use Microsoft Identity PLatform for Authentication.

1. Modify appsettings.json file
Replace contents of the configuration by the below lines:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "[Enter the domain of your tenant, e.g. contoso.onmicrosoft.com]",
    "TenantId": "[Enter 'common', or 'organizations' or the Tenant Id (Obtained from the Azure portal. Select 'Endpoints' from the 'App registrations' blade and use the GUID in any of the URLs), e.g. da41245a5-11b3-996c-00a8-4d99re19f292]",
    "ClientId": "[Enter the Client Id (Application ID obtained from the Azure portal), e.g. ba74781c2-53c2-442a-97c2-3d60re42f403]",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "Scopes": "https://database.windows.net/.default",
    "OnSignOutRedirectPage": "https://localhost:44348",
    "ClientSecret": "[Copy the client secret added to the app from the Azure portal]",
    //"ClientCertificates": [
    //  {
    //    "SourceType": "KeyVault",
    //    "KeyVaultUrl": "[Enter URL for you Key Vault]",
    //    "KeyVaultCertificateName": "[Enter name of the certificate]"
    //  }
    //]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SqlDbContext": "Server=<your server name>;database=<your database name>;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False"
  }
}
```

1. Open Program.cs.
   * Replace

    ```csharp
     builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
     .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    ```

    by

    ```csharp
     builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

    ```

   * Comment the below code. If you leave it uncommented, the application will try to login immediately after start and you won't have a chance to see main page while user is not logged-in

     ```csharp
      builder.Services.AddAuthorization(options =>
      {
       // By default, all incoming requests will be authorized according to the default policy
       options.FallbackPolicy = options.DefaultPolicy;
      });
     ```

   * Replace

     ```csharp
       builder.Services.AddSingleton<WeatherForecastService>();
      ```

      by

      ```csharp
       builder.Services
                .AddScoped<WeatherForecastService>()
                .AddScoped<UserAADService>()
                .AddSingleton<SqlDatabaseService>();
      ```

2. Open Data/WeatherForecastService.cs and replace the entire class by below code:

    ```csharp
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
    ```

3. Create SqlDatabaseService class

    ```csharp
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
    ```

4. Create UserAADService class

    ```csharp
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
    ```

5. Open Pages/FetchData.razor and replace the entire code with this:

    ```csharp
        @page "/fetchdata"

        @using ms_identity_dotnet_blazor_azure_sql.AAD
        @using ms_identity_dotnet_blazor_azure_sql.Data
        @inject WeatherForecastService ForecastService
        @inject UserAADService UserAADService
        @inject AuthenticationStateProvider GetAuthenticationStateAsync

        <h1>Weather forecast</h1>
        <h4><strong>@_greetingsMessage</strong></h4>

        <p>This component demonstrates fetching data from a service that is connected to SQL database.</p>

        @if (forecasts == null)
        {
            <p><em>Loading...</em></p>
        }
        else
        {
            <table class="table">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var forecast in forecasts)
                    {
                        <tr>
                            <td>@forecast.Date.ToShortDateString()</td>
                            <td>@forecast.TemperatureC</td>
                            <td>@forecast.TemperatureF</td>
                            <td>@forecast.Summary</td>
                        </tr>
                    }
                </tbody>
            </table>
        }

        @code {
            private WeatherForecast[] forecasts;
            private string _loggedUser;
            private string _greetingsMessage;

            protected override async Task OnInitializedAsync()
            {
                //obtain current authentication state of the application from HttpContext.User - https://docs.microsoft.com/aspnet/core/blazor/security/server/
                var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();

                //obtain user name from the underlying database based on logged-in user information
                _loggedUser = await UserAADService.GetDatabaseLoggedInUser(authstate);

                if (_loggedUser == "N/A")
                {
                    _greetingsMessage = "Could not obtain logged-in user. Please Log Out of the current user and re-login.";
                }
                else
                {
                    //set greetings message with user name obtained from database
                    _greetingsMessage = $"The user logged into SQL Database is {_loggedUser}";

                    //fetch data for the user
                    forecasts = await ForecastService.GetForecastAsync(DateTime.Now, authstate);
                }
            }
        }
    ```

6. Delete **counter** link from Shared/NavMenu.razor

</details>
