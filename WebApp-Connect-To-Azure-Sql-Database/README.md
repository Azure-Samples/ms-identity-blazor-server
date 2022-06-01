---
page_type: sample
name: How to use an authenticated user's credentials for log-in to an Azure SQL Database from Blazor Web Server App
services: active-directory
platforms: dotnet
urlFragment: ms-identity-dotnet-blazor-azure-sql
description: This sample demonstrates how to use access token obtained from AAD for connecting to Azure SQL Server database as a user that is logged in into the application.
languages:
 - csharp
 - t-sql
products:
 - aspnet-core
 - blazor
 - azure-active-directory
 - azure-sql
---

# How to use an authenticated user's credentials for log-in to an Azure SQL Database from Blazor Web Server App

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Table Of Contents

* [Scenario](#Scenario)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Troubleshooting](#Troubleshooting)
* [Using the sample](#Using-the-sample)
* [About the code](#About-the-code)
* [How the code was created](#How-the-code-was-created)
* [How to deploy this sample to Azure](#How-to-deploy-this-sample-to-Azure)
* [Next Steps](#Next-Steps)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)

## Scenario

This sample demonstrates a Blazor Server App querying an Azure SQL Database with the same authenticated user logged-in into the database. In other words, SQL Database will act exactly for user logged-in instead of active with administrator access rights.

![Scenario Image](ReadmeFiles/topology.png)


## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* Azure subscription and Tenant with at least one user created in it
* Azure [SQL Database](https://docs.microsoft.com/azure/azure-sql/database/single-database-create-quickstart)

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/ms-identity-blazor-server.git
```

or download and extract the repository .zip file.

>:warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Setup Azure SQL Database and grant user permissions for managed identity

1. Create an [Azure SQL Database](https://docs.microsoft.com/azure/azure-sql/database/single-database-create-quickstart).
   * The Sql database Server should have either `Use only Azure Active Directory (Azure AD) authentication` or `Use both SQL and Azure AD authentication` set up as Authentication method.
2. Add one or more of this Azure AD tenant's user as or "Azure Active Directory admin". You would use this user to execute the next set of Sql statements.
3. Install [SQL Server Management Studio](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) and connect to your newly created Azure SQL database using the account you set as "Azure Active Directory admin".
4. In your newly created Database, run the following SQL statements to create and populate a database table to be used in this sample.

   ```sql
   CREATE TABLE [dbo].[Summary](
   [Summary] [nvarchar](50) NOT NULL) 
   ```

   ```sql
   Insert into [dbo].Summary values ('Freezing'),('Bracing'),('Chilly'),('Cool'),('Mild'),('Warm'),('Balmy'),('Hot'),('Sweltering'),('Scorching')
   ```

   ```sql
   CREATE FUNCTION [dbo].[UsernamePrintFn]()
   RETURNS nvarchar(500)
   AS
   BEGIN
       declare @host nvarchar(100), @user nvarchar(100);
       SELECT @host = HOST_NAME() , @user = SUSER_NAME()
       declare @result nvarchar(500) = cast(@user + ' at ' + @host as nvarchar(500))
       -- Return the result of the function
       return @result
   END   
   ```

   ```sql
   /**
   You can use the following command to ensure that the table and function were correctly created and work as expected
   **/
   SELECT * FROM [dbo].Summary
   GO

   SELECT [dbo].[UsernamePrintFn] ()
   GO
   ```

   ```Sql
      /**
   Create a user in database from users in your Tenant and grant them EXECUTE permission by running next set of commands.
   You can add more directory users to this database by running these statements repeatedly.
   **/
   DECLARE @AADDBUser nvarchar(128)
   SET @AADDBUser = '<myusername>@<mytenant>.onmicrosoft.com'

   DECLARE @sql as varchar(max)
   SET @SQL = 'CREATE USER [' + @AADDBUser + '] FROM EXTERNAL PROVIDER;
   EXECUTE sp_addrolemember db_datareader, ''' + @AADDBUser + ''';
   grant execute to ''' + @AADDBUser +''''

   EXEC @SQL
   ```

5. Update connection string inside appsettings.json with server and database names
6. You might need to [update the database Firewall](https://docs.microsoft.com/azure/azure-sql/database/firewall-configure?view=azuresql#from-the-database-overview-page) with your IP address.

### Step 3: Application Registration

There is one project in this sample. To register it, you can:

Follow the [manual steps](#Manual-steps)

**OR**

#### Run automation scripts

* use PowerShell scripts that:
  * **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  * modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > **WARNING**: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
    1. For interactive process - in PowerShell run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -Environment "[Optional] - Azure environment, defaults to 'Global'"
       ```

    1. In case the previous script fails with error about duplicate App Registration, you might want to run the next cleanup script prior to re-running Configure.ps1

       ```powershell
       cd .\AppCreationScripts\
       .\Cleanup.ps1
       ```

       > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md)
       > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

#### Manual Steps

 > Note: skip this part if you've just used Automation steps

Follow the steps below for manually register and configure your apps

<details>
   <summary>Expand this section if you want to use the steps:</summary>

   1. Sign in to the [Azure portal](https://portal.azure.com).
   2. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

##### Register the client app (ClientApp-blazor-azuresql)

   1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD** service.
   1. Select the **App Registrations** blade on the left, then select **New registration**.
   1. In the **Register an application page** that appears, enter your application's registration information:
      * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `ClientApp-blazor-azuresql`.
   1. Under **Supported account types**, select **Accounts in this organizational directory only**
   1. Click **Register** to create the application.
   1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
   1. In the app's registration screen, select **Authentication** in the menu.
      * If you don't have a platform added, select **Add a platform** and select the **Web** option.
   1. In the **Redirect URI** section enter the following redirect URIs: 
      * `https://localhost:44348/`
      * `https://localhost:44348/signin-oidc`
   1. In the **Front-channel logout URL** section, set it to `https://localhost:44348/signout-oidc`.
   1. Select **ID tokens (used for implicit and hybrid flows)** checkbox.
   1. Click **Save** to save your changes.
   1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
   1. In the **Client secrets** section, select **New client secret**:
      * Optionally you can type a key description (for instance `app secret`),
      * Select recommended Expire duration.
      * The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
      * You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
   1. Open **API Permissions** blade and add **'user_impersonation'** scope for **'Azure SQL Database'** API:
      * Open **Add a permission**
      * Switch to **APIs my organization uses**
      * Search for **Azure SQL Database**
      * Click on **Delegated permissions**
      * Check **user_impersonation**
      * Click **Add permissions**

##### Configure the client app (ClientApp-blazor-azuresql) to use your app registration

   Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

   > In the steps below, "ClientID" is the same as "Application ID" or "AppId".

   1. Open the `Client\appsettings.json` file.
      1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
      2. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
      3. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `ClientApp-blazor-azuresql` app copied from the Azure portal.
      4. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `ClientApp-blazor-azuresql` copied from the Azure portal.

  **For more information, visit** [Register Application AAD](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)

  </details>

### Step 4: Running the sample

 To run the sample, run the following commands in the console:

```console
    cd ./WebApp-Connect-To-Azure-Sql-Database/Client
    dotnet run
```

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).
</details>

## Using the sample

<details>
 <summary>Expand to see how to use the sample</summary>

 Running from **VS Code**:

 ```powershell
  dotnet run
 ```

 If you're running from Visual Studio, press **F5** or **Ctrl+F5** (for no debug run)

 On the main page you will be offered to Log In or to go to a "Fetch data" page
 If you choose to go to "Fetch data" page without logging-in, you will be asked to login with a standard UI.
 When the application will be logged in, it will try to connect to Azure SQL Database with an [Access Token](https://aka.ms/access-tokens) it acquired for the currently logged-in user.
 Successful connection will be indicated when the page will state that the user is logged-in into the database and a table with mock forecast data is displayed.

 ![fetch_data_page](ReadmeFiles/fetch-data-page.png)

 The page displays a message with user and host names that are values of @user and @host on SQL Database.

Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR73pcsbpbxNJuZCMKN0lURpUN0FTWEJKSlBBWEZPV1JQMVBMMzBLNjFHRyQlQCN0PWcu)

</details>

## About the code

<details>
 <summary>Expand the section</summary>

 The main purpose of this sample is to show how to propagate AAD user to SQL server. The scenario is as follows:

 1. Get Access Token through interactive log-in process and cache it. To enable caching we have to add the 2 last lines to AAD configuration inside Program.cs:

  ```csharp
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi() 
                .AddInMemoryTokenCaches();
  ```

 2. Every time, the new SQL connection is created, acquire the cached token and add it to the connection object. If the cached token is unavailable, the MsalUiRequiredException will be thrown and interactive Authorization process will be kicked-off. Here is relevant code snippet from UserAADServices.cs:

  ```csharp
    public async Task<string> GetAccessToken(AuthenticationState authState)
        {
            string accessToken = string.Empty;

            //https://database.windows.net/.default
            var scopes = new string[] { _azureSettings["Scopes"] };

            try
            {
                var accountIdentifier = GetAccountIdentifier(authState);

                IAccount account = await _app.GetAccountAsync(accountIdentifier);

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
  ```

  > Notice that the code is using a special default scope to be able to work with SQL Server - **https://database.windows.net/.default**

</details>

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


## How to deploy this sample to Azure

<details>
 <summary>Expand the section</summary>

  To deploy the sample to Azure you will need Azure SQL Database setup
  The deployment is straightforward out of [Visual Studio](https://visualstudio.microsoft.com/downloads/) - right click on the project, select Publish.
  Follow the [steps](https://docs.microsoft.com/aspnet/core/tutorials/publish-to-azure-webapp-using-vs?view=aspnetcore-6.0)

  Once succeeded the site will open on default browser.

</details>


## Next Steps

Learn how to:

* [Change your app to sign-in users from any organization or any Microsoft accounts](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
* [Enable users from National clouds to sign-in to your application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-4-Sovereign)
* [Enable your Web App to call a Web API on behalf of the signed-in user](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app)

## Contributing


Additional information about AAD authentication can be found [here](https://docs.microsoft.com/azure/azure-sql/database/authentication-aad-overview)

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* [Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Building Zero Trust ready apps](https://aka.ms/ztdevsession)

For more information, visit the following links:

 To learn more about the application registration, visit:

* [Configure and manage Azure AD authentication with Azure SQL](https://docs.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-configure?view=azuresql&tabs=azure-powershell)

* [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
* [Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

* To learn more about the code, visit:
  * [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  * [Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  * [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

* To learn more about security in aspnetcore,
  * [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
  * [AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder)
  * [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory)



