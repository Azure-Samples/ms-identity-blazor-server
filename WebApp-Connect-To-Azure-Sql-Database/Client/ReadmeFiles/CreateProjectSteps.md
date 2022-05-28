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
